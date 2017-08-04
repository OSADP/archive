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
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Geography
{
    /// <summary>
    /// Calculates the distance between two locations
    /// </summary>
    public interface IDistanceService
    {
        /// <summary>
        /// Calculates the trip length
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        TripLength CalculateDistance(Location startLocation, Location endLocation);

        /// <summary>
        /// Calculates the trip length
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        TripLength CalculateDistance(Location startLocation, Location endLocation, TimeSpan startTime);
    }
}
