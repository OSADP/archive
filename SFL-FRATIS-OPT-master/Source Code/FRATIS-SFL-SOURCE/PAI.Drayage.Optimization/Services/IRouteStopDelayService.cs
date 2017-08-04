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
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    public interface IRouteStopDelayService
    {
        /// <summary>
        /// Gets the execution time for the given stop and location
        /// </summary>
        /// <param name="stopAction"></param>
        /// <param name="location"></param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        TimeSpan GetExecutionTime(StopAction stopAction, Location location, TimeSpan timeOfDay);

        /// <summary>
        /// Gets the execution time for the given <see cref="RouteStop"/>
        /// </summary>
        /// <param name="routeStop"></param>
        /// /// <param name="timeOfDay"></param>
        /// <returns></returns>
        TimeSpan GetExecutionTime(RouteStop routeStop, TimeSpan timeOfDay);

        /// <summary>
        /// Gets the execution time considering the previous routestop
        /// </summary>
        /// <param name="previousRouteStop"></param>
        /// <param name="routeStop"></param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        TimeSpan GetExecutionTime(RouteStop previousRouteStop, RouteStop routeStop, TimeSpan timeOfDay);
        TimeSpan GetExecutionTime(RouteStop previousRouteStop, RouteStop routeStop, TimeSpan timeOfDay, bool driverFirstStop);

        /// <summary>Gets the queue time for  the given <see cref="RouteStop"/></summary>
        /// <param name="startStop">Comment</param>
        /// <param name="endStop">The end Stop.</param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        TimeSpan GetQueueTime(RouteStop startStop, RouteStop endStop, TimeSpan timeOfDay);

        void SetLocationQueueDelays(List<LocationQueueDelay> delays);
    }
}