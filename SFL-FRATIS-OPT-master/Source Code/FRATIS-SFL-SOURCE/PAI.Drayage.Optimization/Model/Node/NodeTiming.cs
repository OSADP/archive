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
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// Represents the time values associated with an trip
    /// </summary>
    public class NodeTiming
    {
        /// <summary>
        /// Get or set the node
        /// </summary>
        public INode Node { get; set; }

        /// <summary>
        /// Get or set the node departure time in order to arrive at Node
        /// </summary>
        public long DepartureTime { get; set; }

        /// <summary>
        /// Get or set the node arrival time at Node
        /// </summary>
        public TimeSpan ArrivalTime { get; set; }

        /// <summary>
        /// Get or set the node start execution time
        /// Represents the Arrival Time plus the Waiting time 
        /// </summary>
        public TimeSpan StartExecutionTime { get; set; }

        /// <summary>
        /// Get or set the node end execution time
        /// Represents the total time in which all Route Stops will be completed
        /// Represents the Arrival Time + Waiting Time + RouteStop Travel Time
        /// </summary>
        public TimeSpan EndExecutionTime { get; set; }

        /// <summary>
        /// Get or set the idle time (time spent before you can start executing stop)
        /// </summary>
        public TimeSpan IdleTime { get; set; }

        /// <summary>
        /// Gets or sets the time in queue (first portion of job execution)
        /// </summary>
        public TimeSpan QueueTime { get; set; }
        
        /// <summary>
        /// Gets or sets the cumulative route statistics
        /// </summary>
        public RouteStatistics CumulativeRouteStatistics { get; set; }

        /// <summary>
        /// Gets or sets whether time window is feasable
        /// </summary>
        public bool IsFeasableTimeWindow { get; set; }
    }
}