//    Copyright 2014 Productivity Apex Inc.
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
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Infrastructure;


namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// Provides a means of interacting with and between Routes
    /// </summary>
    public class RouteService : IRouteService
    {
        private readonly IRouteExitFunction _routeExitFunction;
        private readonly INodeService _nodeService;
        private readonly IRouteStatisticsComparer _routeStatisticsComparer;
        private readonly IRouteStatisticsService _routeStatisticsService;
        private readonly INodeConnectionFactory _nodeConnectionFactory;

        public RouteService(IRouteExitFunction routeExitFunction, 
            INodeService nodeService, 
            IRouteStatisticsComparer routeStatisticsComparer, 
            IRouteStatisticsService routeStatisticsService,
            INodeConnectionFactory nodeConnectionFactory)
        {
            _routeExitFunction = routeExitFunction;
            _nodeService = nodeService;
            _routeStatisticsComparer = routeStatisticsComparer;
            _routeStatisticsService = routeStatisticsService;
            _nodeConnectionFactory = nodeConnectionFactory;
        }
        
        
        /// <summary>
        /// Returns true if the given route solution is feasable within time windows and exit criteria
        /// </summary>
        /// <param name="nodeRouteSolution"></param>
        /// <returns></returns>
        public bool IsFeasableRouteSolution(NodeRouteSolution nodeRouteSolution)
        {
            var driverNode = nodeRouteSolution.DriverNode;
            var currentNodeEndTime = driverNode.Driver.EarliestStartTimeSpan;
            var cumulativeRouteStatistics = new RouteStatistics();
            var allNodes = nodeRouteSolution.AllNodes;

            for (int i = 0; i < allNodes.Count - 1; i++)
            {
                var nodeTiming = _routeStatisticsService.GetNodeTiming(allNodes[i], allNodes[i + 1], currentNodeEndTime, cumulativeRouteStatistics);
                var previousNode = i > 0 ? allNodes[i - 1] : null;

                if (nodeTiming.IsFeasableTimeWindow)
                {
                    // is it a feasable route

                    var lastConnection = _nodeService.GetNodeConnection(nodeTiming.Node, driverNode);
                    var lastLeg = _routeStatisticsService.GetRouteStatistics(lastConnection, nodeTiming.EndExecutionTime, previousNode);
                    var finalRouteStatistics = nodeTiming.CumulativeRouteStatistics + lastLeg;

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
        public NodeRouteSolution CreateRouteSolution(DriverNode driverNode, IEnumerable<INode> nodes)
        {
            var startTime = driverNode.Driver.EarliestStartTimeSpan;
            var firstJobNode = nodes.FirstOrDefault();

            // adjust the start time
            if (firstJobNode != null)
            {
                // there may have been waiting for the driver to leave, do not count that as idle time
                // update the currentTime to the JobNode WindowEnd, less travel time
                var connection = _nodeService.GetNodeConnection(driverNode, nodes.First());
                if (firstJobNode.WindowEnd.Subtract(connection.RouteStatistics.TotalTravelTime) > startTime)
                {
                    startTime = firstJobNode.WindowEnd.Subtract(connection.RouteStatistics.TotalTravelTime);
                }
            }


            var routeSolution = new NodeRouteSolution
                {
                    DriverNode = driverNode,
                    StartTime = startTime,
                    Nodes = nodes.ToList()
                };

            var allNodes = routeSolution.AllNodes;


            var currentTime = startTime;

            // calculate route statistics
            for (int i = 0; i < allNodes.Count; i++)
            {
                // create node plan
                var node = allNodes[i];
                var previousNode = i > 0 ? allNodes[i - 1] : null;

                // insert waiting time as per JobNode windows
                if (i >= 1 && node is JobNode && previousNode is JobNode)
                {
                    // there may have been waiting for the driver to leave, do not count that as idle time
                    // update the currentTime to the JobNode WindowEnd, less travel time
                    var connection = _nodeService.GetNodeConnection(previousNode, node);
                    if (node.WindowEnd.Subtract(connection.RouteStatistics.TotalTravelTime) > currentTime)
                    {
                        // waiting found
                        var waitingTime = node.WindowEnd.Subtract(currentTime);
                        var currentStatistics = routeSolution.RouteStatistics;
                        currentStatistics.TotalIdleTime = currentStatistics.TotalIdleTime.Add(waitingTime);
                        routeSolution.RouteStatistics = currentStatistics;
                        currentTime = currentTime.Add(waitingTime);
                    }
                }

                // add node trip length
                var stats = _routeStatisticsService.GetRouteStatistics(node, currentTime, previousNode);
                routeSolution.RouteStatistics += stats;

                // currentTime = startTime + routeSolution.RouteStatistics.TotalTime;

                if (previousNode != null)
                {
                    var statistics = _routeStatisticsService.GetRouteStatistics(previousNode, node, currentTime);
                    routeSolution.RouteStatistics += statistics;
                }

                currentTime = startTime + routeSolution.RouteStatistics.TotalTime;
            }

            return routeSolution;
        }

        public IList<RouteStop> GetRouteStopsForRouteSolution(NodeRouteSolution routeSolution)
        {
            var allNodes = routeSolution.AllNodes;

            var routeStops = new List<RouteStop>();

            foreach (var rs in allNodes[0].RouteStops)
            {
                routeStops.Add(rs);    
            }
            

            // calculate route statistics
            for (int i = 1; i < allNodes.Count; i++)
            {
                // create node plan
                var node = allNodes[i];
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
        public NodeRouteSolution GetBestFeasableSolution(IEnumerable<INode> nodes, DriverNode driverNode, NodeRouteSolution bestSolution)
        {
            // create solution
            var routeSolution = CreateRouteSolution(driverNode, nodes);

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

            // set driver count
            var leftRs = left.RouteStatistics;
            leftRs.DriversWithAssignments = left.Nodes.Count > 0 ? 1 : 0;
            left.RouteStatistics = leftRs;

            var rightRs = right.RouteStatistics;
            rightRs.DriversWithAssignments = right.Nodes.Count > 0 ? 1 : 0;
            right.RouteStatistics = rightRs;

            //if (leftRs.DriversWithAssignments > 0 || rightRs.DriversWithAssignments > 0)
            //{
            //    ;
            //}
            int priorityCompareResult = prioritySumLeft.CompareTo(prioritySumRight);
            if (priorityCompareResult == 0)
            {
                return _routeStatisticsComparer.Compare(left.RouteStatistics, right.RouteStatistics) > 0
                           ? right
                           : left;
            }

            return _routeStatisticsComparer.Compare(left.RouteStatistics, right.RouteStatistics) > 0
           ? right
           : left;

            // return the solution with the highest priority sum
            //return priorityCompareResult > 0 ? left : right;
            var result = _routeStatisticsComparer.Min(left, right);
            return result;
            //return _routeStatisticsComparer.Compare(left.RouteStatistics, right.RouteStatistics) > 0 ? right : left;
        }

        public Solution GetBestSolution(Solution left, Solution right)
        {
            var result = left;

            var leftScore = left.RouteStatistics.TotalTime.TotalSeconds + 1000000*left.UnassignedJobNodes.Count();
            var rightScore = right.RouteStatistics.TotalTime.TotalSeconds + 1000000 * right.UnassignedJobNodes.Count();

            if (rightScore < leftScore)
            {
                result = right;
            }

            //// set driver count
            //var leftRs = left.RouteStatistics;
            //leftRs.DriversWithAssignments = left.RouteSolutions.Count(p => p.JobCount > 0);
            //left.RouteStatistics = leftRs;

            //var rightRs = right.RouteStatistics;
            //rightRs.DriversWithAssignments = left.RouteSolutions.Count(p => p.JobCount > 0);
            //right.RouteStatistics = rightRs;

            //var unassignedJobComparison = left.UnassignedJobNodes.Count.CompareTo(right.UnassignedJobNodes.Count);
            //if (unassignedJobComparison > 0)
            //{
            //    result = right;
            //} 
            //else if (unassignedJobComparison < 0)
            //{
            //    result = left;
            //}  
            //else if (unassignedJobComparison == 0)
            //{
            //    result = _routeStatisticsComparer.Min(left, right);    
            //}
            
            return result;
            //return _routeStatisticsComparer.Compare(left.RouteStatistics, right.RouteStatistics) > 0 ? right : left;
        }

    }
}