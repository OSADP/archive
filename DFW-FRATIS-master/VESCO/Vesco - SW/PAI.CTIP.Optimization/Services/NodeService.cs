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
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;


namespace PAI.CTIP.Optimization.Services
{
    public class NodeService : INodeService
    {
        protected readonly OptimizerConfiguration _configuration;
        protected readonly IRouteStopService _routeStopService;
        
        public NodeService(IRouteStopService routeStopService, OptimizerConfiguration configuration)
        {
            _configuration = configuration;
            _routeStopService = routeStopService;
        }

        /// <summary>
        /// Gets the <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public virtual NodeConnection GetNodeConnection(INode startNode, INode endNode)
        {
            return CreateNodeConnection(startNode, endNode);
        }

        /// <summary>
        /// Creates a <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        public virtual NodeConnection CreateNodeConnection(INode startNode, INode endNode)
        {
            var nodeConnection = new NodeConnection()
                {
                    StartNode = startNode,
                    EndNode = endNode
                };

            // start of connection is last stop of start node
            var startStop = startNode.RouteStops.Last();

            // end of connection is first stop of end node
            var endStop = endNode.RouteStops.First();

            // calculate local route statistics
            var stops = new List<RouteStop> {startStop, endStop};
            nodeConnection.RouteStatistics = _routeStopService.CalculateRouteStatistics(stops, true);
            
            if (startNode.Priority > 1)
            {
                nodeConnection.RouteStatistics += new RouteStatistics()
                {
                    PriorityValue = startNode.Priority,
                };
            }

            return nodeConnection;
        }
        
        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        public virtual NodeTiming GetNodeTiming(INode startNode, INode endNode, TimeSpan currentNodeEndTime, RouteStatistics currentRouteStatistics)
        {
            var connection = GetNodeConnection(startNode, endNode);

            TimeSpan nextNodeArrivalTime = currentNodeEndTime + connection.RouteStatistics.TotalTime;
            TimeSpan nextNodeCompletionTime = endNode.RouteStops != null
                                                  ? nextNodeArrivalTime.Add(
                                                      endNode.RouteStops.FirstOrDefault().StopDelay.Value)
                                                  : nextNodeArrivalTime;

            bool isFirstStop = startNode is DriverNode;
            bool early = nextNodeArrivalTime < endNode.WindowStart;
            bool late = nextNodeArrivalTime > endNode.WindowEnd;

            bool isFeasableTimeWindow = false;
            TimeSpan waitTime = TimeSpan.Zero;

            if (early)
            {
                waitTime = endNode.WindowStart.Subtract(nextNodeArrivalTime);

                TimeSpan maxWaitTime = isFirstStop ? _configuration.MaximumWaitTimeBeforeStart : _configuration.MaximumWaitTimeAtStop;

                isFeasableTimeWindow = waitTime < maxWaitTime;

                if (isFirstStop && isFeasableTimeWindow)
                {
                    waitTime = TimeSpan.Zero;
                    currentNodeEndTime = endNode.WindowStart - connection.RouteStatistics.TotalTime;
                    nextNodeArrivalTime = currentNodeEndTime + connection.RouteStatistics.TotalTime;
                }
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


            var cumulatingCompletionTime = new TimeSpan(nextNodeCompletionTime.Ticks);
            if (isFeasableTimeWindow)
            {
                // make sure that the following subsequent route stops would not be violated                
                if (endNode is JobNode)
                {
                    var jn = endNode as JobNode;
                    
                    for (int i=1; i<jn.RouteStops.Count; i++)
                    {
                        var rs = jn.RouteStops[i];
                        var rsConnection = GetNodeConnection(
                            new JobNode() { RouteStops = new List<RouteStop>() { jn.RouteStops[i - 1] }, },
                            new JobNode() { RouteStops = new List<RouteStop>() { rs } });

                        var nextStopArrivalTime = cumulatingCompletionTime.Add(rsConnection.RouteStatistics.TotalTravelTime);
                        bool isWaitRequired = nextStopArrivalTime <= rs.WindowStart;

                        cumulatingCompletionTime = isWaitRequired
                               ? rs.WindowStart.Add(rs.StopDelay.Value)
                               : nextStopArrivalTime.Add(rs.StopDelay.Value);

                        for (int q=i; q < jn.RouteStops.Count; q++)
                        {
                            var nextStop = jn.RouteStops[q];
                            bool nextStopEarly = cumulatingCompletionTime < nextStop.WindowStart;
                            bool nextStopLate = cumulatingCompletionTime > nextStop.WindowEnd;

                            if (nextStopLate)
                            {
                                isFeasableTimeWindow = false;
                            }
                            else if (nextStopEarly)
                            {
                                // todo - check to see waiting time, driver limits
                            }
                        }
                    }
                }
            }

            TimeSpan nextNodeStartTime = nextNodeArrivalTime + waitTime;
            TimeSpan nextNodeEndTime = nextNodeStartTime + endNode.RouteStatistics.TotalTime;

            RouteStatistics localRouteStatistics = new RouteStatistics() {TotalWaitTime = waitTime};
            RouteStatistics cumulativeRouteStatistics = currentRouteStatistics + connection.RouteStatistics + endNode.RouteStatistics + localRouteStatistics;

            var result = new NodeTiming()
            {
                Node = endNode,
                DepartureTime = currentNodeEndTime,
                ArrivalTime = nextNodeArrivalTime,
                StartExecutionTime = nextNodeStartTime,
                EndExecutionTime = cumulatingCompletionTime,
                IsFeasableTimeWindow = isFeasableTimeWindow,
                CumulativeRouteStatistics = cumulativeRouteStatistics
            };

            return result;
        }
    }
}
