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
    /// <summary>
    /// Provides resources for interacting with and determining time spent at stops
    /// </summary>
    public class RouteStopDelayService : IRouteStopDelayService
    {
        private readonly OptimizerConfiguration _optimizerConfiguration;

        public List<LocationQueueDelay> LocationQueueDelays { get; set; }

        public RouteStopDelayService(OptimizerConfiguration optimizerConfiguration)
        {
            _optimizerConfiguration = optimizerConfiguration;
        }

        /// <summary>
        /// Gets the approximate delay (minutes) for the given stop and location
        /// </summary>
        /// <param name="stopAction"> </param>
        /// <param name="location"></param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        public TimeSpan GetExecutionTime(StopAction stopAction, Location location, TimeSpan timeOfDay)
        {
            if (stopAction == StopActions.NoAction)
                return TimeSpan.Zero;

            return _optimizerConfiguration.DefaultStopDelay;
        }

        /// <summary>
        /// Gets the approximate delay (minutes) for the given <see cref="RouteStop"/>
        /// </summary>
        /// <param name="routeStop"></param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        public TimeSpan GetExecutionTime(RouteStop routeStop, TimeSpan timeOfDay)
        {
            if (routeStop.ExecutionTime.HasValue)
                return routeStop.ExecutionTime.Value;

            return GetExecutionTime(routeStop.StopAction, routeStop.Location, timeOfDay);
        }

        public TimeSpan GetExecutionTime(RouteStop previousRouteStop, RouteStop routeStop, TimeSpan timeOfDay)
        {
            return GetExecutionTime(previousRouteStop, routeStop, timeOfDay, false);
        }

        public TimeSpan GetExecutionTime(RouteStop previousRouteStop, RouteStop routeStop, TimeSpan timeOfDay, bool driverFirstStop)
        {
            //if (previousRouteStop != null && routeStop != null
            //    && previousRouteStop.Location != null
            //    && routeStop.Location != null 
            //    && (previousRouteStop.Location.Id == routeStop.Location.Id || (previousRouteStop.Location.Latitude == routeStop.Location.Latitude && previousRouteStop.Location.Longitude == routeStop.Location.Longitude))
            //    && !driverFirstStop && previousRouteStop.StopAction != null && routeStop.StopAction != null && (previousRouteStop.StopAction.ShortName != "NA" && routeStop.StopAction.ShortName != "NA"))
            //{
            //    return TimeSpan.Zero;
            //}

            var result = GetExecutionTime(routeStop, timeOfDay);
            return result;
        }


        /// <summary>
        /// Gets the queue time for  the given <see cref="RouteStop"/>
        /// </summary>
        /// <param name="routeStop"></param>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        public TimeSpan GetQueueTime(RouteStop startStop, RouteStop endStop, TimeSpan timeOfDay)
        {
            if (endStop == null)
            {
                throw new ArgumentNullException("endStop");
            }

            if (LocationQueueDelays != null)
            {
                bool shouldGetQueueTime = !(startStop != null && startStop.Location.Id == endStop.Location.Id);

                if (shouldGetQueueTime)
                {
                    var queueDelays = LocationQueueDelays.Where(p => p.LocationId == endStop.Location.Id);
                    var match = queueDelays.FirstOrDefault(
                        p => p.DelayStartTime <= timeOfDay.Ticks && p.DelayEndTime >= timeOfDay.Ticks);
                    if (match != null)
                    {
                        endStop.QueueTime = new TimeSpan(0, match.QueueDelay, 0);
                        return endStop.QueueTime.Value;
                    }
                }
            }
            
            return TimeSpan.Zero;
        }

		/// <summary>
        /// Sets the delays
        /// </summary>
        /// <param name="delays">the stop to be estimated</param>
        public void SetLocationQueueDelays(List<LocationQueueDelay> delays)
        {
            LocationQueueDelays = delays;
        }
    }
}