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
using System.Linq;
using System;
using PAI.CTIP.Optimization.Model.Metrics;

namespace PAI.CTIP.Optimization.Model.Node
{
    public class NodeTiming
    {
        /// <summary>
        /// Get or set the node
        /// </summary>
        public INode Node { get; set; }

        /// <summary>
        /// Get or set the node arrival time
        /// </summary>
        public TimeSpan DepartureTime { get; set; }

        /// <summary>
        /// Get or set the node arrival time
        /// </summary>
        public TimeSpan ArrivalTime { get; set; }

        /// <summary>
        /// Get or set the node start time
        /// </summary>
        public TimeSpan StartExecutionTime { get; set; }

        /// <summary>
        /// Get or set the node end time
        /// </summary>
        public TimeSpan EndExecutionTime { get; set; }

        /// <summary>
        /// Get or set the wait time
        /// </summary>
        public TimeSpan WaitTime { get; set; }

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