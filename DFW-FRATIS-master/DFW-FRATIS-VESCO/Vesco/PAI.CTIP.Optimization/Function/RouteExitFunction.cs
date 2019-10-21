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
using PAI.CTIP.Optimization.Model.Orders;
using PAI.Core;

namespace PAI.CTIP.Optimization.Function
{
    public class RouteExitFunction : IRouteExitFunction
    {
        protected readonly ILogger _logger;

        public RouteExitFunction(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns true if the route statistics exceed our exit criteria
        /// </summary>
        /// <param name="routeStatistics"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        public bool ExeedsExitCriteria(RouteStatistics routeStatistics, Driver driver)
        {
            var result = routeStatistics.TotalTravelTime > driver.AvailableDrivingTime || routeStatistics.TotalTime > driver.AvailableDutyTime;

            if (result && _logger.IsDebugEnabled)
            {
                if (routeStatistics.TotalTravelTime > driver.AvailableDrivingTime)
                {
                    _logger.Debug("Route exceeds AvailableDrivingTime. TotalTravelTime={0}, AvailableDrivingTime={1}",
                        routeStatistics.TotalTravelTime.TotalHours,
                        driver.AvailableDrivingTime.TotalHours);
                }

                if (routeStatistics.TotalTime > driver.AvailableDutyTime)
                {
                    _logger.Debug("Route exceeds AvailableDutyTime. TotalTime={0}, AvailableDutyTime={1}",
                         routeStatistics.TotalTime.TotalHours,
                         driver.AvailableDutyTime.TotalHours); 
                }
            }

            return result;
        }
    }
}