using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    public interface IRouteStatisticsService
    {
        /// <summary>
        /// Calculates the trip length between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [Obsolete("Use method with startTime")]
        RouteStatistics GetRouteStatistics(INode origin, INode destination);

        /// <summary>
        /// Calculates the RouteStatistics for a given node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        [Obsolete("Use method with startTime")]
        RouteStatistics GetRouteStatistics(IRouteSegment node, IRouteSegment previousNode);

        /// <summary>
        /// Calculates the trip length between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        RouteStatistics GetRouteStatistics(INode origin, INode destination, TimeSpan startTime);
        
        /// <summary>
        /// Calculates the RouteStatistics for a given node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        RouteStatistics GetRouteStatistics(IRouteSegment node, TimeSpan startTime, IRouteSegment previousNode);

        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        NodeTiming GetNodeTiming(INode startNode, INode endNode, TimeSpan startNodeEndTime, RouteStatistics currentRouteStatistics);

        IList<RouteSegmentStatistics> CalculateRouteSegmentStatistics(TimeSpan startTime, IList<RouteStop> stops);
    }
}