//    Copyright 2013 Productivity Apex Inc.
//        http://www.productivityapex.com/
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.Core;

namespace PAI.CTIP.Optimization.Services
{
    public class RouteService : IRouteService
    {
        protected readonly ILogger _logger;
        private readonly IRouteExitFunction _routeExitFunction;
        private readonly INodeService _nodeService;
        private readonly IStatisticsService _statisticsService;

        public RouteService(IRouteExitFunction routeExitFunction, 
            INodeService nodeService, IStatisticsService statisticsService, ILogger logger)
        {
            _routeExitFunction = routeExitFunction;
            _nodeService = nodeService;
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the RouteStatistics between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public RouteStatistics CalculateRouteStatistics(INode origin, INode destination)
        {
            var nodeConnection = _nodeService.GetNodeConnection(origin, destination);
            return nodeConnection.RouteStatistics;
        }
        
        /// <summary>
        /// Returns true if the given route solution is feasable within time windows and exit criteria
        /// </summary>
        /// <param name="nodeRouteSolution"></param>
        /// <returns></returns>
        public bool IsFeasableRouteSolution(NodeRouteSolution nodeRouteSolution)
        {
            var driverNode = nodeRouteSolution.DriverNode;
            var currentNodeEndTime = driverNode.Driver.EarliestStartTime;
            var cumulativeRouteStatistics = new RouteStatistics();
            var allNodes = nodeRouteSolution.AllNodes;

            for (int i = 0; i < allNodes.Count - 1; i++)
            {
                var nodeTiming = _nodeService.GetNodeTiming(allNodes[i], allNodes[i + 1], currentNodeEndTime, cumulativeRouteStatistics);

                if (nodeTiming.IsFeasableTimeWindow)
                {
                    // is it a feasable route
                    var lastConnection = _nodeService.GetNodeConnection(nodeTiming.Node, driverNode);
                    var finalRouteStatistics = nodeTiming.CumulativeRouteStatistics + lastConnection.RouteStatistics;

                    if (_routeExitFunction.ExeedsExitCriteria(finalRouteStatistics, driverNode.Driver))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                currentNodeEndTime = nodeTiming.EndExecutionTime;
                cumulativeRouteStatistics = nodeTiming.CumulativeRouteStatistics;
            }

            return true;
        }

        ///// <summary>
        ///// Creates a route solution from a list of nodes
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <returns></returns>
        public NodeRouteSolution CreateRouteSolution(IEnumerable<INode> nodes, DriverNode driverNode)
        {
            var routeSolution = new NodeRouteSolution
                {
                    DriverNode = driverNode,
                    StartTime =  driverNode.Driver.EarliestStartTime,
                    Nodes = nodes.ToList()
                };

            var allNodes = routeSolution.AllNodes;

            // calculate route statistics
            for (int i = 0; i < allNodes.Count; i++)
            {
                // create node plan
                var node = allNodes[i];

                // add node trip length
                routeSolution.RouteStatistics += node.RouteStatistics;

                var previousNode = i == 0 ? null : allNodes[i - 1];
                if (previousNode != null)
                {
                    var statistics = CalculateRouteStatistics(previousNode, node);
                    routeSolution.RouteStatistics += statistics;
                }
            }
            
            return routeSolution;
        }

     
        public IList<RouteStop> GetRouteStopsForRouteSolution(NodeRouteSolution routeSolution)
        {
            var allNodes = routeSolution.AllNodes;

            var routeStops = new List<RouteStop>();

            routeStops.AddRange(allNodes[0].RouteStops);

            // calculate route statistics
            for (int i = 1; i < allNodes.Count; i++)
            {
                // create node plan
                var node = allNodes[i];
                
                var previousNode = allNodes[i - 1];
                if (previousNode != null)
                {
                    var nodeConnection = _nodeService.GetNodeConnection(previousNode, node);
                    if (nodeConnection.RouteStops != null)
                    {
                        routeStops.AddRange(nodeConnection.RouteStops);
                    }
                }

                routeStops.AddRange(node.RouteStops);
            }

            return routeStops;
        }

        /// <summary>
        /// Gets the best solution between a new list of <see cref="INode"/> and a current best <see cref="NodeRouteSolution"/>
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="driverNode"> </param>
        /// <param name="bestSolution"></param>
        /// <returns>the best solution</returns>
        public NodeRouteSolution GetBestFeasableSolution(IList<INode> nodes, DriverNode driverNode, NodeRouteSolution bestSolution)
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
        
        public NodeRouteSolution GetBestSolution(NodeRouteSolution left, NodeRouteSolution right)
        {
            double prioritySumLeft = left.AllNodes.Sum(f => f.Priority);
            double prioritySumRight = right.AllNodes.Sum(f => f.Priority);
            int priorityCompareResult = prioritySumLeft.CompareTo(prioritySumRight);
            
            if (priorityCompareResult == 0)
            {
                return _statisticsService.CompareRouteStatistics(left.RouteStatistics, right.RouteStatistics) > 0
                           ? right
                           : left;
            }

            // return the solution with the highest priority sum
            return priorityCompareResult > 0 ? left : right;
        }

        public Solution GetBestSolution(Solution left, Solution right)
        {
            double prioritySumLeft = left.RouteSolutions.SelectMany(f => f.AllNodes).Sum(g => g.Priority);
            double prioritySumRight = right.RouteSolutions.SelectMany(f => f.AllNodes).Sum(g => g.Priority);
            int priorityCompareResult = prioritySumLeft.CompareTo(prioritySumRight);

            if (priorityCompareResult == 0)
            {
                return _statisticsService.CompareRouteStatistics(left.RouteStatistics, right.RouteStatistics) > 0
                           ? right
                           : left;
            }

            // return the solution with the highest priority sum
            return priorityCompareResult > 0 ? left : right;
        }

        

    }
}