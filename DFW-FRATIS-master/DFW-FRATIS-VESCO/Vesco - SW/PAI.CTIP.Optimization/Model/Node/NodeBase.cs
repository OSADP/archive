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
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Model.Node
{
    /// <summary>
    /// Base Node implementation
    /// </summary>
    public abstract class NodeBase : INode
    {
        /// <summary>
        /// Gets the route stops
        /// </summary>
        public IList<RouteStop> RouteStops { get; set; }

        /// <summary>
        /// Gets or sets route statistics
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

        public double Priority { get; set; }

        //public dynamic Properties { get; private set; }

        protected NodeBase()
        {
            WindowStart = TimeSpan.MinValue;
            WindowEnd = TimeSpan.MaxValue;
            RouteStops = new List<RouteStop>();
            Priority = 1.0;
            //Properties = new DynamicDictionary();
        }

        
    }

}