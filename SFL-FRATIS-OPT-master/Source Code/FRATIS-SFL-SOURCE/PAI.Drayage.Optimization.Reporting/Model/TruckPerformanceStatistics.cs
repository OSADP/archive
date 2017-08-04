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
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Reporting.Model
{
    /// <summary>
    /// A representation of the performance statistics of a truck
    /// </summary>
    public class TruckPerformanceStatistics
    {
        /// <summary>
        /// Gets or sets the state of the route statistics by truck.
        /// </summary>
        /// <value>
        /// The state of the route statistics by truck.
        /// </value>
        public Dictionary<TruckState, RouteStatistics> RouteStatisticsByTruckState { get; set; }

        /// <summary>
        /// Gets or sets the route statistics.
        /// </summary>
        /// <value>
        /// The route statistics.
        /// </value>
        public RouteStatistics RouteStatistics { get; set; }

        /// <summary>
        /// Gets or sets the performance statistics of the truck.
        /// </summary>
        /// <value>
        /// The performance statistics of the truck.
        /// </value>
        public PerformanceStatistics PerformanceStatistics { get; set; }

        /// <summary>
        /// Gets or sets the route segment statistics of the truck.
        /// </summary>
        /// <value>
        /// The route segment statistics of the truck.
        /// </value>
        public IList<RouteSegmentStatistics> RouteSegmentStatistics { get; set; }
    }
}