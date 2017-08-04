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

namespace PAI.Drayage.Optimization.Geography
{
    /// <summary>
    /// Estimates the duration of travel
    /// </summary>
    public class TravelTimeEstimator : ITravelTimeEstimator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TravelTimeEstimator"/> class.
        /// </summary>
        public TravelTimeEstimator()
        {
            AverageCitySpeed = 30;
            AverageHighwaySpeed = 50;
            SpeedThreshold = 50;
        }

        /// <summary>
        /// Gets or sets the average city speed.
        /// </summary>
        /// <value>
        /// The average city speed.
        /// </value>
        public double AverageCitySpeed { get; set; }
        /// <summary>
        /// Gets or sets the average highway speed.
        /// </summary>
        /// <value>
        /// The average highway speed.
        /// </value>
        public double AverageHighwaySpeed { get; set; }

        /// <summary>
        /// Gets or sets the speed threshold.
        /// </summary>
        /// <value>
        /// The speed threshold.
        /// </value>
        public double SpeedThreshold { get; set; }

        /// <summary>
        /// Calculates the travel time 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>Total travel time represented as TimeSpan</returns>
        public TimeSpan CalculateTravelTime(double distance)
        {
            var speed = distance < SpeedThreshold ? AverageCitySpeed : AverageHighwaySpeed;
            var travelTime = distance / speed;
            return TimeSpan.FromHours(travelTime);
        }
    }
}