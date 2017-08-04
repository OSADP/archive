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

namespace PAI.CTIP.Optimization.Geography
{
    public class TravelTimeEstimator : ITravelTimeEstimator
    {
        public double AverageCitySpeed { get; set; }
        public double AverageHighwaySpeed { get; set; }
        public double SpeedThreshold { get; set; }

        public TravelTimeEstimator()
        {
            AverageCitySpeed = 35;
            AverageHighwaySpeed = 55;
            SpeedThreshold = 60;
        }

        /// <summary>
        /// Calculates the travel time 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public TimeSpan CalculateTravelTime(double distance)
        {
            double speed = distance < SpeedThreshold ? AverageCitySpeed : AverageHighwaySpeed;
            var travelTime =  distance / speed;
            return TimeSpan.FromHours(travelTime);
        }
    }
}