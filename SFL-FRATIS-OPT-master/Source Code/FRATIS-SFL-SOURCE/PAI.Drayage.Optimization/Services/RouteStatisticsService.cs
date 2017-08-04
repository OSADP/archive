using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    public class RouteStatisticsService : IRouteStatisticsService
    {
        private readonly IRouteStopService _routeStopService;
        private readonly INodeService _nodeService;
        private readonly OptimizerConfiguration _configuration;
        
        public RouteStatisticsService(IRouteStopService routeStopService, INodeService nodeService, OptimizerConfiguration configuration)
        {
            _routeStopService = routeStopService;
            _nodeService = nodeService;
            _configuration = configuration;
        }
        
        ///// <summary>
        ///// Calculates the RouteStatistics for a given node
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        public RouteStatistics GetRouteStatistics(IRouteSegment node, IRouteSegment previousNode = null)
        {
            var routeStatistics = _routeStopService.CalculateRouteStatistics(node.RouteStops, TimeSpan.Zero, node.IsConnection, GetNodeEndRouteStop(previousNode));
            return routeStatistics;
        }

        /// <summary>
        /// Calculates the RouteStatistics for a given node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public RouteStatistics GetRouteStatistics(IRouteSegment node, TimeSpan startTime, IRouteSegment previousNode = null)
        {
            var routeStatistics = _routeStopService.CalculateRouteStatistics(node.RouteStops, startTime, node.IsConnection, GetNodeEndRouteStop(previousNode));
            return routeStatistics;
        }

        private RouteStop GetNodeEndRouteStop(IRouteSegment node)
        {
            RouteStop result = null;
            if (node != null && node is JobNode)
            {
                var jn = node as JobNode;
                if (jn.Job != null && jn.Job.RouteStops != null && jn.Job.RouteStops.Any())
                {
                    result = jn.Job.RouteStops.Last();
                }
            }

            return result;
        }

        ///// <summary>
        ///// Calculates the RouteStatistics between two nodes
        ///// </summary>
        ///// <param name="origin"></param>
        ///// <param name="destination"></param>
        ///// <returns></returns>
        public RouteStatistics GetRouteStatistics(INode origin, INode destination)
        {
            var nodeConnection = _nodeService.GetNodeConnection(origin, destination);
            var routeStatistics = _routeStopService.CalculateRouteStatistics(nodeConnection.RouteStops, TimeSpan.Zero, true, GetNodeEndRouteStop(origin));
            return routeStatistics;
        }

        /// <summary>
        /// Calculates the trip length between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public RouteStatistics GetRouteStatistics(INode origin, INode destination, TimeSpan startTime)
        {
            var nodeConnection = _nodeService.GetNodeConnection(origin, destination);
            var routeStatistics = _routeStopService.CalculateRouteStatistics(nodeConnection.RouteStops, startTime, true, GetNodeEndRouteStop(origin));
            return routeStatistics;
        }

        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        public virtual NodeTiming GetNodeTiming(INode startNode, INode endNode, TimeSpan startNodeEndTime, RouteStatistics currentRouteStatistics)
        {
            bool isFirstStop = startNode is DriverNode;
            var connectionRouteStatistics = GetRouteStatistics(startNode, endNode, startNodeEndTime);
            var originalStartNodeEndTime = startNodeEndTime.Ticks;

            if (isFirstStop)
            {
                var travelTime = connectionRouteStatistics.TotalTravelTime.Ticks;
                var driverStartTime = originalStartNodeEndTime;                
                var jobNodeEnd = endNode.WindowEnd.Ticks;
                var jobNodeStart = endNode.WindowStart.Ticks;

                startNodeEndTime = TimeSpan.FromTicks(Math.Max(jobNodeEnd - travelTime, driverStartTime));
                startNodeEndTime = TimeSpan.FromTicks(Math.Max(jobNodeStart - travelTime, driverStartTime));
            }

            TimeSpan endNodeArrivalTime = startNodeEndTime + connectionRouteStatistics.TotalTravelTime;

            // cbs 16 Sep 14 Scheduling Night Shift starts with day orders
            if (endNodeArrivalTime.Days > 0)
            {
                endNodeArrivalTime = endNodeArrivalTime.Add(TimeSpan.FromDays(endNodeArrivalTime.Days * -1));
            }

            // determine if time arrived within time window and calculate wait time
            var firstStopWaitMinutes = isFirstStop ? endNodeArrivalTime.Subtract(TimeSpan.FromTicks(originalStartNodeEndTime)).TotalMinutes : 0;
            var isDriverAlreadyLate = startNodeEndTime > endNode.WindowEnd;
            var isFirstStopWithinDelayPeriod = isFirstStop && !isDriverAlreadyLate &&
                                               firstStopWaitMinutes <= _configuration.MaximumIdleTimeBeforeStart.TotalMinutes;

            

            bool early = endNodeArrivalTime <= endNode.WindowStart;
            bool late = (endNodeArrivalTime > endNode.WindowEnd) || (isFirstStop && !isFirstStopWithinDelayPeriod);
            
            bool isFeasableTimeWindow = false;
            TimeSpan idleTime = TimeSpan.Zero;
            

            if (early)
            {
                // if we are early to make it to the first stop, there's no need to wait, we will 
                // adjust the nextNodeArrivalTime to reflect the windowStartTime of the first 
                // location to goto.

                if (!isFirstStop)
                {
                    idleTime = endNode.WindowStart.Subtract(endNodeArrivalTime);
                }
                else
                {
                    startNodeEndTime = endNode.WindowStart - connectionRouteStatistics.TotalTime;
                    startNodeEndTime = endNode.WindowStart - connectionRouteStatistics.TotalNonIdleTime;
                    endNodeArrivalTime = startNodeEndTime + connectionRouteStatistics.TotalTravelTime;
                    idleTime = endNode.WindowStart.Subtract(endNodeArrivalTime);
                }

                var maxIdleTime = isFirstStop ? _configuration.MaximumIdleTimeBeforeStart : _configuration.MaximumIdleTimeAtStop;
                isFeasableTimeWindow = idleTime < maxIdleTime;
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

            var endNodeRouteStatistics = GetRouteStatistics(endNode, endNodeArrivalTime);

            TimeSpan queueTime = endNodeRouteStatistics.TotalQueueTime;
            
            TimeSpan nextNodeStartTime = endNodeArrivalTime + idleTime;

            TimeSpan nextNodeEndTime = nextNodeStartTime + endNodeRouteStatistics.TotalTime;

            // wait?
            RouteStatistics localRouteStatistics = new RouteStatistics() { TotalIdleTime = idleTime };

            RouteStatistics cumulativeRouteStatistics = 
                currentRouteStatistics + connectionRouteStatistics + endNodeRouteStatistics + localRouteStatistics;

            var result = new NodeTiming()
            {
                Node = endNode,
                DepartureTime = startNodeEndTime.Ticks,
                ArrivalTime = endNodeArrivalTime,
                StartExecutionTime = nextNodeStartTime,
                IdleTime = isFirstStop ? TimeSpan.Zero : idleTime,
                QueueTime = queueTime,
                EndExecutionTime = nextNodeEndTime,
                IsFeasableTimeWindow = isFeasableTimeWindow,
                CumulativeRouteStatistics = cumulativeRouteStatistics
            };

            return result;
        }

        public IList<RouteSegmentStatistics> CalculateRouteSegmentStatistics(TimeSpan startTime, IList<RouteStop> stops)
        {
            return _routeStopService.CalculateRouteSegmentStatistics(startTime, stops);
        }
    }
}