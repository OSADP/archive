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
using PAI.CTIP.Optimization.Geography;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public class RouteStopService : IRouteStopService
    {
        private readonly IDistanceService _distanceService;
        private readonly OptimizerConfiguration _configuration;
        private readonly IRouteStopDelayService _routeStopDelayService;


        public RouteStopService(IDistanceService distanceService, OptimizerConfiguration configuration, IRouteStopDelayService routeStopDelayService)
        {
            _distanceService = distanceService;
            _configuration = configuration;
            _routeStopDelayService = routeStopDelayService;
        }

        /// <summary>
        /// Calculates the trip length between two stops
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TripLength CalculateTripLength(RouteStop start, RouteStop end)
        {
            return _distanceService.CalculateDistance(start.Location, end.Location);
        }
        
        /// <summary>
        /// Calculates the toal route statistics a list of route stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="ignoreFirstStopDelays"> </param>
        /// <returns></returns>
        public RouteStatistics CalculateRouteStatistics(IList<RouteStop> stops, bool ignoreFirstStopDelays = false)
        {
            var result = new RouteStatistics();

            // TODO : This does not take WaitTime into account

            for (int i = 0; i < stops.Count; i++)
            {
                var currentStop = stops[i];

                if (!ignoreFirstStopDelays && (i == 0 || i == stops.Count - 1))
                {
                    var executionTime = _routeStopDelayService.GetDelay(currentStop);

                    var staticStats = new RouteStatistics()
                        {
                            TotalExecutionTime = executionTime,
                        };
                    result += staticStats;
                }

                // add travel cost
                if (i < stops.Count - 1)
                {
                    var nextStop = stops[i + 1];

                    // calculate the trip between the current and next stop
                    var tripLength = CalculateTripLength(currentStop, nextStop);
                    var travelStats = new RouteStatistics()
                        {
                            TotalTravelTime = tripLength.Time,
                            TotalTravelDistance = tripLength.Distance,
                        };
                    result += travelStats;
                }
            }

            return result;
        }
        
        /// <summary>
        /// Calculates the toal route statistics a list of route stops
        /// </summary>
        /// <param name="stops"></param>
        /// <returns></returns>
        public TripLength CalculateTripLength(IList<RouteStop> stops)
        {
            var result = new TripLength();

            for (int i = 0; i < stops.Count - 1; i++)
            {
                // calculate the trip between the current and next stop
                var currentStop = stops[i];
                var nextStop = stops[i + 1];

                var tripLength = CalculateTripLength(currentStop, nextStop);

                result += tripLength;
            }

            return result;
        }
        
        /// <summary>
        /// Calculates the route segment statistics for a list of stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public IList<RouteSegmentStatistics> CalculateRouteSegmentStatistics(IList<RouteStop> stops, TimeSpan startTime)
        {
            var result = new List<RouteSegmentStatistics>();

            var currentTime = startTime;

            for (int i = 0; i < stops.Count - 1; i++)
            {
                var startStop = stops[i];
                var endStop = stops[i + 1];

                var segment = CreateRouteSegmentStatistics(currentTime, startStop, endStop);
                currentTime = segment.EndTime;

                result.Add(segment);
            }

            return result;
        }

        /// <summary>
        /// Creates a route segment statistics
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="startStop"></param>
        /// <param name="endStop"></param>
        /// <returns></returns>
        public RouteSegmentStatistics CreateRouteSegmentStatistics(TimeSpan startTime, RouteStop startStop, RouteStop endStop)
        {
            // determine if time arrived within time window and calculate wait time
            bool early = startTime < endStop.WindowStart;
            bool late = startTime > endStop.WindowEnd;

            TimeSpan waitTime = TimeSpan.Zero;
            bool whiffed = false;

            if (early)
            {
                waitTime = endStop.WindowStart.Subtract(startTime);
                whiffed = waitTime > _configuration.MaximumWaitTimeAtStop;
            }
            else if (late)
            {
                // we started past the time window
                whiffed = true;
            }

            var executionTime = _routeStopDelayService.GetDelay(endStop);

            // calculate the trip between the current and next stop
            var tripLength = CalculateTripLength(startStop, endStop);

            var routeStatistics = new RouteStatistics()
            {
                TotalExecutionTime = executionTime,
                TotalWaitTime = waitTime,
                TotalTravelTime = tripLength.Time,
                TotalTravelDistance = tripLength.Distance,
            };
            
            var result = new RouteSegmentStatistics
                {
                    StartStop = startStop,
                    EndStop = endStop,
                    StartTime = startTime,
                    Statistics = routeStatistics,
                    WhiffedTimeWindow = whiffed
                };
            
            return result;
        }

        

    }
}