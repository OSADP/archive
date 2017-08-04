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
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Infrastructure;
using PAI.Infrastructure;

namespace PAI.Drayage.Optimization.Function
{
    /// <summary>
    /// Determins when a computed route exceeds selection criteria
    /// </summary>
    public class RouteExitFunction : IRouteExitFunction
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteExitFunction"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public RouteExitFunction(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Returns true if the route statistics exceed our exit criteria
        /// </summary>
        /// <param name="routeStatistics">the generated metrics for the route</param>
        /// <param name="driver">the driver of the route</param>
        /// <returns>true if route exceeds criteria, otherwise false</returns>
        public bool ExeedsExitCriteria(RouteStatistics routeStatistics, Driver driver)
        {
            var result = routeStatistics.TotalTravelTime > driver.AvailableDrivingTime || routeStatistics.TotalTime > driver.AvailableDutyTime;

            if (result && this._logger.IsDebugEnabled)
            {
                if (routeStatistics.TotalTravelTime > driver.AvailableDrivingTime)
                {
                    this._logger.Debug("Route exceeds AvailableDrivingTime. TotalTravelTime={0}, AvailableDrivingTime={1}",
                        routeStatistics.TotalTravelTime.TotalHours,
                        driver.AvailableDrivingTime.TotalHours);
                }

                if (routeStatistics.TotalTime > driver.AvailableDutyTime)
                {
                    this._logger.Debug("Route exceeds AvailableDutyTime. TotalTime={0}, AvailableDutyTime={1}",
                         routeStatistics.TotalTime.TotalHours,
                         driver.AvailableDutyTime.TotalHours); 
                }
            }

            if (result)
            {
                return result;
            }

            return result;
        }

        public bool MeetsPortCriteria(IEnumerable<RouteStop> routeStops, Driver driver)
        {
            var result = true;

            if (routeStops.Any(x => x.Location.PortOfMiami) || routeStops.Any(x => x.Location.PortEverglades))
            {
                if (routeStops.Any(x => x.Location.PortOfMiami))
                {
                    result &= driver.PortOfMiami;
                }
                if (routeStops.Any(x => x.Location.PortEverglades))
                {
                    result &= driver.PortEverglades;
                }
            }

            return result;
        }
    }
}