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

namespace PAI.CTIP.Optimization.Services
{
    /// <summary>
    /// Delay Service Interface
    /// </summary>
    public interface IRouteStopService
    {
        /// <summary>
        /// Calculates the trip length between two stops
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        TripLength CalculateTripLength(RouteStop start, RouteStop end);
        
        /// <summary>
        /// Calculates the route statistics for a list of stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="ignoreFirstStopDelays"> </param>
        /// <returns></returns>
        RouteStatistics CalculateRouteStatistics(IList<RouteStop> stops, bool ignoreFirstStopDelays = false);
        
        /// <summary>
        /// Calculates the route segment statistics for a list of stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        IList<RouteSegmentStatistics> CalculateRouteSegmentStatistics(IList<RouteStop> stops, TimeSpan startTime);

        /// <summary>
        /// Creates a route segment statistics
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="startStop"></param>
        /// <param name="endStop"></param>
        /// <returns></returns>
        RouteSegmentStatistics CreateRouteSegmentStatistics(TimeSpan startTime, RouteStop startStop, RouteStop endStop);

    }
}
