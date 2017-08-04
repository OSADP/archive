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
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public class RouteStopDelayService : IRouteStopDelayService
    {
        private readonly OptimizerConfiguration _optimizerConfiguration;

        public RouteStopDelayService(OptimizerConfiguration optimizerConfiguration)
        {
            _optimizerConfiguration = optimizerConfiguration;
        }

        /// <summary>
        /// Gets the approximate delay (minutes) for the given stop
        /// </summary>
        /// <param name="stopAction"></param>
        /// <returns></returns>
        public TimeSpan GetDelay(StopAction stopAction)
        {
            if (stopAction == StopActions.NoAction)
                return TimeSpan.Zero;

            return _optimizerConfiguration.DefaultStopDelay;
        }

        /// <summary>
        /// Gets the approximate delay (minutes) for the given stop and location
        /// </summary>
        /// <param name="stopAction"> </param>
        /// <param name="location"></param>
        /// <returns></returns>
        public TimeSpan GetDelay(StopAction stopAction, Location location)
        {
            return GetDelay(stopAction);
        }

        /// <summary>
        /// Gets the approximate delay (minutes) for the given <see cref="RouteStop"/>
        /// </summary>
        /// <param name="routeStop"></param>
        /// <returns></returns>
        public TimeSpan GetDelay(RouteStop routeStop)
        {
            if (routeStop.StopDelay.HasValue)
                return routeStop.StopDelay.Value;

            return GetDelay(routeStop.StopAction, routeStop.Location);
        }
    }
}