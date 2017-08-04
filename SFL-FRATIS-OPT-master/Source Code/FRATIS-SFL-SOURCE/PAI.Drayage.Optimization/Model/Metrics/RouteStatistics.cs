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

namespace PAI.Drayage.Optimization.Model.Metrics
{
    /// <summary>
    /// Represents the statistics for a set of route stops
    /// </summary>
    public struct RouteStatistics
    {
        /// <summary>
        /// Gets the number of drivers with job assignments - manually set
        /// </summary>
        public int DriversWithAssignments;

        /// <summary>
        /// Gets or sets the idle time
        /// </summary>
        public TimeSpan TotalIdleTime;

        /// <summary>
        /// Gets or sets the queue time
        /// </summary>
        public TimeSpan TotalQueueTime;

        /// <summary>
        /// Gets or sets the execution time
        /// </summary>
        public TimeSpan TotalExecutionTime;

        /// <summary>
        /// Gets or sets the total time
        /// </summary>
        public TimeSpan TotalTravelTime;

        /// <summary>
        /// Gets or sets the distance (mi)
        /// </summary>
        public decimal TotalTravelDistance;

        /// <summary>
        /// Gets or sets the total time
        /// </summary>
        public TimeSpan TotalTime
        {
            get
            {
                return TotalIdleTime + TotalQueueTime + TotalExecutionTime + TotalTravelTime;
            }
        }

        public TimeSpan TotalNonIdleTime
        {
            get
            {
                return TotalQueueTime + TotalExecutionTime + TotalTravelTime;
            }
        }

        public double PriorityValue;

        /// <summary>
        /// Gets or sets total capacity
        /// </summary>
        public decimal TotalCapacity;

        public static RouteStatistics operator + (RouteStatistics c1, RouteStatistics c2)
        {
            var result = new RouteStatistics()
                {
                    TotalIdleTime = c1.TotalIdleTime + c2.TotalIdleTime,
                    TotalQueueTime = c1.TotalQueueTime + c2.TotalQueueTime,
                    TotalExecutionTime = c1.TotalExecutionTime + c2.TotalExecutionTime,
                    TotalTravelTime = c1.TotalTravelTime + c2.TotalTravelTime,
                    TotalTravelDistance = c1.TotalTravelDistance + c2.TotalTravelDistance,
                    TotalCapacity = c1.TotalCapacity + c2.TotalCapacity
                };

            return result;
        }

        public int UnassignedJobs { get; set; }
        
        public override string ToString()
        {
            return string.Format("TotalTravelDistance = {0:F1}, TravelTime = {1}, TotalTime = {2}", 
                TotalTravelDistance, TotalTravelTime.ToString(), TotalTime.ToString());
        }
    }
}