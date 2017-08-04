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
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    public class RouteStopService : IRouteStopService
    {
        private readonly IDistanceService _distanceService;
        private readonly OptimizerConfiguration _configuration;
        private readonly IRouteStopDelayService _routeStopDelayService;
        readonly IJobNodeService _jobNodeService;


        public RouteStopService(IDistanceService distanceService, OptimizerConfiguration configuration, IRouteStopDelayService routeStopDelayService, IJobNodeService jobNodeService)
        {
            _distanceService = distanceService;
            _configuration = configuration;
            _routeStopDelayService = routeStopDelayService;
            _jobNodeService = jobNodeService;
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
        /// Calculates the trip length between two stops
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public TripLength CalculateTripLength(RouteStop start, RouteStop end, TimeSpan startTime)
        {
            return _distanceService.CalculateDistance(start.Location, end.Location, startTime);
        }
        
        /// <summary>
        /// Calculates the toal route statistics a list of route stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="ignoreFirstStopDelays"> </param>
        /// <returns></returns>
        public RouteStatistics CalculateRouteStatistics(IList<RouteStop> stops, bool ignoreFirstStopDelays, RouteStop lastNodeEndStop = null)
        {
            var result = new RouteStatistics();

            for (int i = 0; i < stops.Count; i++)
            {
                var currentStop = stops[i];
                var previousStop = i > 0 ? stops[i - 1] : null;

                if (!ignoreFirstStopDelays || !(i == 0 || i == stops.Count - 1))
                {
                    var executionTime = _routeStopDelayService.GetExecutionTime(previousStop, currentStop, TimeSpan.Zero);

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
        /// Calculates the route statistics for a list of stops
        /// </summary>
        /// <param name="stops"></param>
        /// <param name="startTime"></param>
        /// <param name="ignoreFirstStopDelays"> </param>
        /// <returns></returns>
        public RouteStatistics CalculateRouteStatistics(IList<RouteStop> stops, TimeSpan startTime, bool ignoreFirstStopDelays, RouteStop lastNodeEndStop = null)
        {
            var result = new RouteStatistics();

            var currentTime = startTime;

            for (int i = 0; i < stops.Count; i++)
            {
                var currentStop = stops[i];
                var previousStop = i > 0 ? stops[i - 1] : lastNodeEndStop;
                var waitTime = TimeSpan.Zero;
                if (currentTime < currentStop.WindowStart)
                {
                    // early
                    waitTime = currentStop.WindowStart.Subtract(currentTime);
                }
                else if (currentTime > currentStop.WindowEnd)
                {
                    ;
                    // late
                    //var lateTime = currentTime.Subtract(currentStop.WindowEnd);
                }

                if (ignoreFirstStopDelays)
                {
                    if (i != 0)
                    {
                        var executionTime = _routeStopDelayService.GetExecutionTime(previousStop, currentStop,
                            currentTime);
                        var staticStats = new RouteStatistics()
                        {
                            TotalExecutionTime = executionTime,
                            TotalIdleTime = waitTime,
                        };

                        result += staticStats;
                    }
                }
                else
                {
                    // todo refactor
                    var executionTime = _routeStopDelayService.GetExecutionTime(previousStop, currentStop, currentTime);
                    var staticStats = new RouteStatistics()
                    {
                        TotalExecutionTime = executionTime,
                        TotalIdleTime = waitTime,
                    };

                    result += staticStats;
                }

                // update current time with accumulated total time
                currentTime = startTime + result.TotalTime;

                // add travel cost
                if (i < stops.Count - 1)
                {
                    var nextStop = stops[i + 1];
                    if (currentStop.Location != null && nextStop.Location != null)
                    {
                        // calculate the trip between the current and next stop
                        var tripLength = CalculateTripLength(currentStop, nextStop, currentTime);
                        var queueTime = _routeStopDelayService.GetQueueTime(currentStop, nextStop, currentTime);
                        var travelStats = new RouteStatistics()
                        {

                            TotalTravelTime = tripLength.Time,
                            TotalTravelDistance = tripLength.Distance,
                            TotalQueueTime = queueTime,
                        };

                        result += travelStats;                        
                    }
                }

                // update current time once again with accumulated total time
                currentTime = startTime + result.TotalTime;
            }

            return result;
        }
        

        /// <summary>
        /// Calculates the route segment statistics for a list of stops
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="stops"></param>
        /// <returns></returns>
        public IList<RouteSegmentStatistics> CalculateRouteSegmentStatistics(TimeSpan startTime, IList<RouteStop> stops)
        {
            var result = new List<RouteSegmentStatistics>();

            var tempJob = new Job
                {
                    RouteStops = stops
                };

            var tempJobNode = _jobNodeService.CreateJobNode(tempJob, startTime, false);

            return tempJobNode.Job.RouteSegmentStatisticses;
            
            var matrix = tempJobNode.RouteStatisticsMatrix;

            
            
            var currentTime = startTime;
            var isFirstClosedWindow = false;

            

            //for (int i = 0; i < stops.Count - 1; i++)
            //{
            //    var startStop = stops[i];
            //    var endStop = stops[i + 1];

            //    //if (currentTime.Days > 0)
            //    //{
            //    //    currentTime = new TimeSpan(currentTime.Hours, currentTime.Minutes, currentTime.Seconds);
            //    //}

            //    var segment = CreateRouteSegmentStatistics(currentTime, startStop, endStop, isFirstClosedWindow, i == 0);
            //    currentTime = segment.EndTime;

            //    if (i > 0 && startStop.WindowStart != TimeSpan.MinValue && startStop.WindowEnd != TimeSpan.MaxValue)
            //    {
            //        var windowDuration = startStop.WindowEnd.Subtract(startStop.WindowStart);
            //        if (windowDuration.Hours < 23)
            //        {
            //            isFirstClosedWindow = true;                        
            //        }
            //    }
                
            //    result.Add(segment);
            //}

            return result;
        }

        /// <summary>
        /// Creates a route segment statistics
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="startStop"></param>
        /// <param name="endStop"></param>
        /// <returns></returns>
        public RouteSegmentStatistics CreateRouteSegmentStatistics(TimeSpan startTime, RouteStop startStop, RouteStop endStop, bool setIdleTime = true)
        {
            return CreateRouteSegmentStatistics(startTime, startStop, endStop, setIdleTime, false);
        }

        public RouteSegmentStatistics CreateRouteSegmentStatistics(TimeSpan startTime, RouteStop startStop, RouteStop endStop, bool setIdleTime = true, bool driverFirstStop = false) 
        {
            if (startTime.Days > 0)
            {
                startTime = new TimeSpan(startTime.Hours, startTime.Minutes, startTime.Seconds);
            }
            if (startStop.WindowStart > endStop.WindowStart)
            {
                endStop.WindowStart = endStop.WindowStart.Add(TimeSpan.FromDays(1));
            }
            if (startStop.WindowEnd > endStop.WindowEnd)
            {
                endStop.WindowEnd = endStop.WindowEnd.Add(TimeSpan.FromDays(1));
            }



            // calculate the trip between the current and next stop
            var tripLength = CalculateTripLength(startStop, endStop);

            // determine if time arrived within time window and calculate wait time
            var adjustedStartTime = startTime.Ticks - tripLength.Time.Ticks;
            if (adjustedStartTime < startTime.Ticks)
            {
                adjustedStartTime = startTime.Ticks;
            }

            var earlyLateFlagTime = new TimeSpan((startTime + tripLength.Time).Hours, (startTime + tripLength.Time).Minutes, (startTime + tripLength.Time).Seconds);

            bool early = earlyLateFlagTime < endStop.WindowStart;
            bool late = earlyLateFlagTime > endStop.WindowEnd;

            TimeSpan idleTime = TimeSpan.Zero;
            bool whiffed = false;

            if (early)
            {
                if (setIdleTime)
                {
                    idleTime = endStop.WindowStart.Subtract(startTime);                    
                }
                var idleTime2 = idleTime.Ticks - tripLength.Time.Ticks;
                if (idleTime2 > startTime.Ticks)
                {
                    // idletime 2 is the new adjusted start time
                    if (setIdleTime)
                    {
                        idleTime = new TimeSpan(idleTime2);                        
                    }
                }
                
                whiffed = idleTime > _configuration.MaximumIdleTimeAtStop;
                // subtract travel time if not driver start time
            }
            else if (late)
            {
                // we started past the time window
                whiffed = true;
            }
            
            bool isDriverFirstSegment = true;
            if (idleTime > tripLength.Time)
            {
                idleTime = idleTime - tripLength.Time;
            }

            var arrivalTimeAtEndStop = new TimeSpan((startTime + tripLength.Time).Hours, (startTime + tripLength.Time).Minutes, (startTime + tripLength.Time).Seconds);

            var executionTime = _routeStopDelayService.GetExecutionTime(startStop, endStop, arrivalTimeAtEndStop, driverFirstStop);

            TimeSpan queueTime = _routeStopDelayService.GetQueueTime(startStop, endStop, arrivalTimeAtEndStop);
            
            var routeStatistics = new RouteStatistics()
            {
                TotalExecutionTime = executionTime,
                TotalIdleTime = idleTime,
                TotalTravelTime = tripLength.Time,
                TotalTravelDistance = tripLength.Distance,
                TotalQueueTime = queueTime
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