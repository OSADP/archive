using System;
using System.Collections.Generic;
using System.Linq;
using Ninject.Extensions.Logging;
using PAI.CTIP.Domain;
using PAI.CTIP.Services.Optimization.Model;

namespace PAI.CTIP.Services.Optimization
{
    public class NodeRouteService : INodeRouteService
    {
        private readonly ILogger _logger;
        private readonly IRouteStopService _routeStopService;
        private readonly IRouteExitFunction _routeExitFunction;
        private readonly OptimizerConfiguration _configuration;
        private readonly IObjectiveFunction _objectiveFunction;
        private readonly IDictionary<Tuple<INode, INode>, NodeConnection> _nodeConnectionCache;

        public NodeRouteService(IObjectiveFunction objectiveFunction, 
            IRouteStopService routeStopService, IRouteExitFunction routeExitFunction, ILogger logger,
            OptimizerConfiguration configuration)
        {
            _objectiveFunction = objectiveFunction;
            _routeStopService = routeStopService;
            _routeExitFunction = routeExitFunction;
            _configuration = configuration;
            _logger = logger;

            _nodeConnectionCache = new Dictionary<Tuple<INode, INode>, NodeConnection>();
        }

        /// <summary>
        /// Calculates the RouteStatistics between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public RouteStatistics CalculateRouteStatistics(INode origin, INode destination)
        {
            var nodeConnection = GetNodeConnection(origin, destination);
            return nodeConnection.LocalRouteStatistics;
        }

        /// <summary>
        /// Gets the <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public NodeConnection GetNodeConnection(INode startNode, INode endNode)
        {
            NodeConnection nodeConnection = null;

            var key = new Tuple<INode, INode>(startNode, endNode);
            if (_nodeConnectionCache.TryGetValue(key, out nodeConnection))
            {
                return nodeConnection;
            }

            nodeConnection = new NodeConnection()
            {
                StartNode = startNode,
                EndNode = endNode
            };

            // TODO : Add dynamic stops
            nodeConnection.RouteStops = new List<RouteStop>();


            //these stops are just used to calcuate route statistics for things like trip length
            var stops = new List<RouteStop>();

            // start of connection is last stop of start node
            stops.Add(startNode.RouteStops.Last());

            // end of connection is first stop of end node
            stops.Add(endNode.RouteStops.First());

            nodeConnection.LocalRouteStatistics = _routeStopService.CalculateRouteStatistics(stops, true);

            return nodeConnection;
        }

        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        public NodeTiming GetNodeTiming(INode currentNode, INode nextNode, DateTime currentNodeEndTime, RouteStatistics currentRouteStatistics)
        {
            var connection = GetNodeConnection(currentNode, nextNode);

            DateTime nextNodeArrivalTime = currentNodeEndTime + connection.LocalRouteStatistics.TotalTime;
            
            bool isFirstStop = currentNode is DriverNode;

            // determine if time arrived within time window and calculate wait time
            bool early = nextNodeArrivalTime < nextNode.WindowStart;
            bool late = nextNodeArrivalTime > nextNode.WindowEnd;

            bool isFeasableTimeWindow = false;
            TimeSpan waitTime = TimeSpan.Zero;

            if (early)
            {
                waitTime = nextNode.WindowStart.Subtract(nextNodeArrivalTime);

                TimeSpan maxWaitTime = isFirstStop ? _configuration.MaximumWaitTimeBeforeStart : _configuration.MaximumWaitTimeAtStop;

                isFeasableTimeWindow = waitTime < maxWaitTime;
            }
            else if (late)
            {
                // we started past the time window
                isFeasableTimeWindow = false;
            }
            else
            {
                isFeasableTimeWindow = true;
            }

            DateTime nextNodeStartTime = nextNodeArrivalTime + waitTime;
            DateTime nextNodeEndTime = nextNodeStartTime + nextNode.LocalRouteStatistics.TotalTime;

            RouteStatistics cumulativeRouteStatistics = currentRouteStatistics + connection.LocalRouteStatistics + nextNode.LocalRouteStatistics;
            
            var result = new NodeTiming()
            {
                Node = nextNode,
                ArrivalTime = nextNodeArrivalTime,
                StartTime = nextNodeStartTime,
                EndTime = nextNodeEndTime,
                IsFeasableTimeWindow = isFeasableTimeWindow,
                CumulativeRouteStatistics = cumulativeRouteStatistics
            };

            //var st = (result.ArrivalTime - DateTime.Now.Date).TotalMinutes;
            //var et = (result.EndTime - DateTime.Now.Date).TotalMinutes;
            //Console.WriteLine("{0},{1},{2}", nextNode, st, et);
            
            return result;
        }


        /// <summary>
        /// Returns true if the given route solution is feasable within time windows and exit criteria
        /// </summary>
        /// <param name="routeSolution"></param>
        /// <returns></returns>
        public bool IsFeasableRouteSolution(RouteSolution routeSolution)
        {
            var driverNode = routeSolution.DriverNode;
            var currentNodeEndTime = driverNode.Driver.EarliestStartTime;
            var cumulativeRouteStatistics = new RouteStatistics();
            var allNodes = routeSolution.AllNodes;

            for (int i = 0; i < allNodes.Count - 1; i++)
            {
                var nodeTiming = GetNodeTiming(allNodes[i], allNodes[i + 1], currentNodeEndTime, cumulativeRouteStatistics);

                if (nodeTiming.IsFeasableTimeWindow)
                {
                    // is it a feasable route
                    var lastConnection = GetNodeConnection(nodeTiming.Node, driverNode);
                    var finalRouteStatistics = nodeTiming.CumulativeRouteStatistics + lastConnection.LocalRouteStatistics;

                    if (_routeExitFunction.ExeedsExitCriteria(finalRouteStatistics, driverNode.Driver))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                currentNodeEndTime = nodeTiming.EndTime;
                cumulativeRouteStatistics = nodeTiming.CumulativeRouteStatistics;
            }

            return true;
        }

        ///// <summary>
        ///// Creates a route solution from a list of nodes
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <returns></returns>
        public RouteSolution CreateRouteSolution(IEnumerable<INode> nodes, DriverNode driverNode)
        {
            var routeSolution = new RouteSolution
                {
                    DriverNode = driverNode,
                    Nodes = nodes.ToList()
                };

            var allNodes = routeSolution.AllNodes;

            // calculate route statistics
            for (int i = 0; i < allNodes.Count; i++)
            {
                // create node plan
                var node = allNodes[i];

                // add node trip length
                routeSolution.RouteStatistics += node.LocalRouteStatistics;

                var previousNode = i == 0 ? null : allNodes[i - 1];
                if (previousNode != null)
                {
                    var statistics = CalculateRouteStatistics(previousNode, node);
                    routeSolution.RouteStatistics += statistics;
                }
            }
            
            return routeSolution;
        }

        /// <summary>
        /// Gets the best solution between a new list of <see cref="INode"/> and a current best <see cref="RouteSolution"/>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="driverNode"> </param>
        /// <param name="bestSolution"></param>
        /// <returns>the best solution</returns>
        public RouteSolution GetBestFeasableSolution(IList<INode> nodes, DriverNode driverNode, RouteSolution bestSolution)
        {
            // create solution
            var routeSolution = CreateRouteSolution(nodes, driverNode);

            // check feasibility
            if (IsFeasableRouteSolution(routeSolution))
            {
                if (bestSolution != null)
                {
                    routeSolution = GetBestSolution(bestSolution, routeSolution);
                }
            } 
            else
            {
                routeSolution = bestSolution;
            }

            return routeSolution;
        }
        
        public RouteSolution GetBestSolution(RouteSolution left, RouteSolution right)
        {
            return CompareSolutions(left.RouteStatistics, right.RouteStatistics) > 0 ? right : left;
        }

        public Solution GetBestSolution(Solution left, Solution right)
        {
            return CompareSolutions(left.RouteStatistics, right.RouteStatistics) > 0 ? right : left;
        }

        /// <summary>
        /// Compares route solutions
        /// </summary>
        public int CompareSolutions(RouteStatistics left, RouteStatistics right)
        {
            var leftMeasure = _objectiveFunction.GetObjectiveMeasure(left);
            var rightMeasure = _objectiveFunction.GetObjectiveMeasure(right);
            return leftMeasure.CompareTo(rightMeasure);
        }

    }
}