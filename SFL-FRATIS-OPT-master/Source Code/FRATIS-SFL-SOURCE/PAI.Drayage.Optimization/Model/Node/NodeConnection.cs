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
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// Represents a connection between two nodes
    /// </summary>
    public class NodeConnection : IRouteSegment
    {
        /// <summary>
        /// Gets or sets the route stops
        /// </summary>
        public IList<RouteStop> RouteStops { get; set; }

        /// <summary>
        /// Gets or sets the route statistics
        /// </summary>
        public RouteStatistics RouteStatistics { get; set; }
        /// <summary>
        /// Gets or sets the lower bound of the time window
        /// </summary>
        public TimeSpan WindowStart { get; set; }

        /// <summary>
        /// Gets or sets the upper bound of the time window
        /// </summary>
        public TimeSpan WindowEnd { get; set; }

        /// <summary>Gets or sets the departure time from the first stop.</summary>
        public long DepartureTime { get; set; }

        public bool IsConnection { get { return true; } }

        /// <summary>
        /// Gets or sets the start node
        /// </summary>
        public INode StartNode { get; set; }

        /// <summary>
        /// Gets or sets the end node
        /// </summary>
        public INode EndNode { get; set; }
    }
}