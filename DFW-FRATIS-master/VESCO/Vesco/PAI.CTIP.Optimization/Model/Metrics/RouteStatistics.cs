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
using System.Linq;

namespace PAI.CTIP.Optimization.Model.Metrics
{
    /// <summary>
    /// Represents the statistics for a set of route stops
    /// </summary>
    public struct RouteStatistics
    {
        /// <summary>
        /// Gets or sets the wait time
        /// </summary>
        public TimeSpan TotalWaitTime;

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
                return TotalWaitTime + TotalExecutionTime + TotalTravelTime;
            }
        }

        public double PriorityValue;

        /// <summary>
        /// Gets or sets total capacity
        /// </summary>
        public decimal TotalCapacity;

        public static RouteStatistics operator +(RouteStatistics c1, RouteStatistics c2)
        {
            var result = new RouteStatistics()
                {
                    TotalWaitTime = c1.TotalWaitTime + c2.TotalWaitTime,
                    TotalExecutionTime = c1.TotalExecutionTime + c2.TotalExecutionTime,
                    TotalTravelTime = c1.TotalTravelTime + c2.TotalTravelTime,
                    TotalTravelDistance = c1.TotalTravelDistance + c2.TotalTravelDistance,
                    TotalCapacity = c1.TotalCapacity + c2.TotalCapacity,
                    PriorityValue = c1.PriorityValue + c2.PriorityValue
                };

            return result;
        }
        
        public override string ToString()
        {
            return string.Format("TotalTravelDistance = {0:F1}, TravelTime = {1}, TotalTime = {2}", 
                TotalTravelDistance, TotalTravelTime.ToString(), TotalTime.ToString());
        }
    }
}