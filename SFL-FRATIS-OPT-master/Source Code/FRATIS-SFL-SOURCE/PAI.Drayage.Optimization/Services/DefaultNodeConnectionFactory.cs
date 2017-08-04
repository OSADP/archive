using System;
using System.Collections.Generic;
using System.Linq;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    public class DefaultNodeConnectionFactory : INodeConnectionFactory
    {
        protected readonly IRouteStopService _routeStopService;

        public DefaultNodeConnectionFactory(IRouteStopService routeStopService)
        {
            _routeStopService = routeStopService;
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


            if (startNode.Priority > 1)
            {
                nodeConnection.RouteStatistics += new RouteStatistics()
                {
                    PriorityValue = startNode.Priority,
                };
            }

            // start of connection is last stop of start node
            var startStop = startNode.RouteStops.Last();

            // end of connection is first stop of end node
            var endStop = endNode.RouteStops.First();

            // calculate local route statistics
            var stops = new List<RouteStop> { startStop, endStop };
            nodeConnection.RouteStops = stops;


            return nodeConnection;
        }
    }
}