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
using System.Linq;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Metrics
{
    /// <summary>
    /// A representation of the metrics for a leg of a generated route
    /// </summary>
    public class RouteSegmentStatistics
    {
        /// <summary>
        /// Gets or sets the start stop
        /// </summary>
        public RouteStop StartStop { get; set; }

        /// <summary>
        /// Gets or sets the end stop
        /// </summary>
        public RouteStop EndStop { get; set; }

        /// <summary>
        /// Gets or sets the start time of this segment
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets the end time of this segment
        /// </summary>
        public TimeSpan EndTime { get { return StartTime.Add(Statistics.TotalTime); } }

        /// <summary>
        /// Gets or sets the statistics of this segment
        /// </summary>
        public RouteStatistics Statistics { get; set; }

        /// <summary>
        /// Gets or sets whether or not the time window got whiffed
        /// </summary>
        public bool WhiffedTimeWindow { get; set; }

        /// <summary>
        /// Gets the truck state for this segment
        /// </summary>
        public TruckState TruckState
        {
            get
            {
                return StartStop.PostTruckConfig.TruckState;
            }
        }
    }
}