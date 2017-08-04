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
using System.Text;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Reporting.Model;
using PAI.Drayage.Optimization.Services;
using PAI.Infrastructure;

namespace PAI.Drayage.Optimization.Reporting.Services
{
    /// <summary>
    /// Generates Statistics from generated route solutions
    /// </summary>
    public interface IReportingService
    {
        /// <summary>
        /// Gets the Solution Performance Statistics
        /// </summary>
        SolutionPerformanceStatistics GetSolutionPerformanceStatistics(Solution solution);

        /// <summary>
        /// Gets the Solution Performance Statistics Report
        /// </summary>
        string GetSolutionPerformanceStatisticsReport(Solution solution);

        /// <summary>
        /// Gets a list of Performance Statistics per leg of each generated route
        /// </summary>
        IList<RouteSegmentStatistics> GetRouteSegmentStats(NodeRouteSolution routeSolution);
    }

    /// <summary>
    /// Generates Statistics from generated route solutions
    /// </summary>
    public class ReportingService : IReportingService
    {
        private readonly IRouteService _routeService;
        private readonly IRouteStopService _routeStopService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingService"/> class.
        /// </summary>
        /// <param name="routeStopService">The route stop service.</param>
        /// <param name="routeService">The route service.</param>
        public ReportingService(IRouteStopService routeStopService, IRouteService routeService)
        {
            _routeStopService = routeStopService;
            _routeService = routeService;
        }
        
        /// <summary>
        /// Computes the Solution Performance Statistics
        /// </summary>
        /// <param name="solution">The solution from which the report is generated</param>
        /// <returns>The generated Performance Statistics from the Solution</returns>
        public SolutionPerformanceStatistics GetSolutionPerformanceStatistics(Solution solution)
        {
            var truckStatisticBySolution = new Dictionary<NodeRouteSolution, TruckPerformanceStatistics>();
            var totalRouteStatisticsByTruckState = new Dictionary<TruckState, RouteStatistics>();
            var totalPerformanceStatistics = new PerformanceStatistics();
            var totalRouteStatistics = new RouteStatistics();

            foreach (var routeSolution in solution.RouteSolutions)
            {
                // calculate truck performance statistics
                var truckStatistics = CalculateTruckPerformanceStatistics(routeSolution);
                truckStatisticBySolution[routeSolution] = truckStatistics;

                // calculate total route statistics by truck state
                foreach (var truckState in truckStatistics.RouteStatisticsByTruckState.Keys)
                {
                    if (!totalRouteStatisticsByTruckState.ContainsKey(truckState))
                    {
                        totalRouteStatisticsByTruckState[truckState] = new RouteStatistics();
                    }

                    totalRouteStatisticsByTruckState[truckState] += truckStatistics.RouteStatisticsByTruckState[truckState];
                }

                // sum up performance & route stats
                totalPerformanceStatistics += truckStatistics.PerformanceStatistics;
                totalRouteStatistics += truckStatistics.RouteStatistics;
            }

            double solutionCount = solution.RouteSolutions.Count;

            var result = new SolutionPerformanceStatistics
                {
                    TruckStatistics = truckStatisticBySolution,
                    TotalRouteStatisticsByTruckState = totalRouteStatisticsByTruckState,
                    TotalRouteStatistics = totalRouteStatistics,
                    NumberOfBackhauls = totalPerformanceStatistics.NumberOfBackhauls,
                    AverageNumberOfBackhauls = totalPerformanceStatistics.NumberOfBackhauls/solutionCount,
                    NumberOfLoadmatches = totalPerformanceStatistics.NumberOfLoadmatches,
                    AverageNumberOfLoadmatches = totalPerformanceStatistics.NumberOfLoadmatches/solutionCount,
                    NumberOfJobs = totalPerformanceStatistics.NumberOfJobs,
                    AverageNumberOfJobs = totalPerformanceStatistics.NumberOfJobs/solutionCount,
                    AverageDriverDutyHourUtilization = totalPerformanceStatistics.DriverDutyHourUtilization/solutionCount,
                    AverageDriverDrivingUtilization = totalPerformanceStatistics.DriverDrivingUtilization/solutionCount,
                    DrivingTimePercentage = totalRouteStatistics.TotalTravelTime.TotalHours/totalRouteStatistics.TotalTime.TotalHours,
                    WaitingTimePercentage = totalRouteStatistics.TotalIdleTime.TotalHours/totalRouteStatistics.TotalTime.TotalHours
                };
            
            return result;
        }



        /// <summary>
        /// Computes the TruckPerformanceStatistics for a given route solution
        /// </summary>
        /// <param name="routeSolution">The route from which statistics are generated</param>
        /// <returns>The performance statistics for the truck</returns>
        public TruckPerformanceStatistics CalculateTruckPerformanceStatistics(NodeRouteSolution routeSolution)
        {
            var segmentStats = GetRouteSegmentStats(routeSolution);

            // calculate the statistics by truck state
            var statsByTructState = (from segment in segmentStats
                                     group segment by segment.TruckState into g
                                     select new
                                     {
                                         TruckState = g.Key,
                                         Statistics = g.Aggregate(new RouteStatistics(), (seed, segment) => seed + segment.Statistics)
                                     })
                    .ToDictionary(key => key.TruckState, value => value.Statistics);

            // compute the sum of all the route statistics
            var totalRouteStatistics = segmentStats.Aggregate(new RouteStatistics(), (seed, segment) => seed + segment.Statistics);

            var performanceStatistics = CalculatePerformanceStatistics(routeSolution);

            var truckStatistics = new TruckPerformanceStatistics
            {
                RouteStatisticsByTruckState = statsByTructState,
                RouteStatistics = totalRouteStatistics,
                PerformanceStatistics = performanceStatistics,
                RouteSegmentStatistics = segmentStats
            };

            return truckStatistics;
        }

        /// <summary>
        /// Gets the route segment stats.
        /// </summary>
        /// <param name="routeSolution">The route solution.</param>
        /// <returns>a list of statistics for each leg of the route</returns>
        public IList<RouteSegmentStatistics> GetRouteSegmentStats(NodeRouteSolution routeSolution)
        {
            var startTime = routeSolution.StartTime;
            var routeStops = _routeService.GetRouteStopsForRouteSolution(routeSolution);
            var segmentStats = _routeStopService.CalculateRouteSegmentStatistics(startTime, routeStops);
            return segmentStats;
        }

        /// <summary>
        /// Computes the performance statistics for a given route solution
        /// </summary>
        /// <param name="routeSolution">The solution from which the report is generated</param>
        /// <returns>the statistics generated from the route</returns>
        private PerformanceStatistics CalculatePerformanceStatistics(NodeRouteSolution routeSolution)
        {
            var result = new PerformanceStatistics()
                {
                    NumberOfJobs = routeSolution.JobCount,
                    DriverDutyHourUtilization = routeSolution.RouteStatistics.TotalTime.TotalHours / routeSolution.DriverNode.Driver.AvailableDutyTime.TotalHours,
                    DriverDrivingUtilization = routeSolution.RouteStatistics.TotalTravelTime.TotalHours / routeSolution.DriverNode.Driver.AvailableDrivingTime.TotalHours,
                    DrivingTimePercentage = routeSolution.RouteStatistics.TotalTravelTime.TotalHours / routeSolution.RouteStatistics.TotalTime.TotalHours,
                    WaitingTimePercentage = routeSolution.RouteStatistics.TotalIdleTime.TotalHours / routeSolution.RouteStatistics.TotalTime.TotalHours,
                    NumberOfBackhauls = CalculateNumberOfBackhauls(routeSolution),
                    NumberOfLoadmatches = CalculateNumberOfLoadMatches(routeSolution)
                };

            return result;
        }

        /// <summary>
        /// Computes the simple backhaul count for the given route solution
        /// </summary>
        /// <param name="routeSolution">the route solution from which backhauls are counted</param>
        /// <returns>the sum of JobNodes plus DriverNodes minus 1</returns>
        public int CalculateNumberOfBackhauls(NodeRouteSolution routeSolution)
        {
            int result = 0;
            result = routeSolution.Nodes.Count - 1;
            return result;
        }

        /// <summary>
        /// Computes the number of load maches for the given route solution
        /// </summary>
        /// <param name="routeSolution">the solution from which the count is retrieved</param>
        /// <returns>the count of liveloading followed by live unloading stops along the route</returns>
        public int CalculateNumberOfLoadMatches(NodeRouteSolution routeSolution)
        {
            int result = 0;

            // iterates through node collection
            foreach (var node in routeSolution.Nodes)
            {
                bool hasLiveUnloading = false;
                bool hasBoth = false;

                foreach (var routeStop in node.RouteStops)
                {
                    if (routeStop.StopAction == StopActions.LiveUnloading)
                    {
                        hasLiveUnloading = true;
                    }
                    else
                    {
                        if (hasLiveUnloading && routeStop.StopAction == StopActions.LiveLoading)
                        {
                            hasBoth = true;
                        }
                        else if (hasLiveUnloading && routeStop.StopAction != StopActions.LiveLoading)
                        {
                            hasLiveUnloading = false;
                            hasBoth = false;
                        }
                    }

                    if (hasBoth)
                    {
                        result++;
                        hasLiveUnloading = false;
                        hasBoth = false;
                    }
                }
            }

            return result;
        }
        
        /// <summary>
        /// Gets the Solution Performance Statistics Report
        /// </summary>
        public string GetSolutionPerformanceStatisticsReport(Solution solution)
        {
            var sb = new StringBuilder();

            var statistics = GetSolutionPerformanceStatistics(solution);

            sb.AppendLine("Performance Statistics Report");

            int truckIndex = 1;
            foreach (var truckStatistics in statistics.TruckStatistics.Values)
            {
                sb.AppendLine();
                sb.AppendFormat("Truck {0}\r\n", truckIndex++);
                
                sb.AppendFormat("\tNumberOfJobs: {0}\r\n", truckStatistics.PerformanceStatistics.NumberOfJobs);
                sb.AppendFormat("\tNumberOfBackhauls: {0}\r\n", truckStatistics.PerformanceStatistics.NumberOfBackhauls);
                sb.AppendFormat("\tNumberOfLoadmatches: {0}\r\n", truckStatistics.PerformanceStatistics.NumberOfLoadmatches);
                
                sb.AppendFormat("\tTotalTime: {0:dd\\.hh\\:mm\\:ss}\r\n", truckStatistics.RouteStatistics.TotalTime);
                sb.AppendFormat("\tTotalIdleTime: {0:dd\\.hh\\:mm\\:ss}\r\n", truckStatistics.RouteStatistics.TotalIdleTime);
                sb.AppendFormat("\tTotalTravelTime: {0:dd\\.hh\\:mm\\:ss}\r\n", truckStatistics.RouteStatistics.TotalTravelTime);
                sb.AppendFormat("\tTotalExecutionTime: {0:dd\\.hh\\:mm\\:ss}\r\n", truckStatistics.RouteStatistics.TotalExecutionTime);

                sb.AppendFormat("\tDriverDrivingUtilization: {0:P1}\r\n", truckStatistics.PerformanceStatistics.DriverDrivingUtilization);
                sb.AppendFormat("\tDriverDutyHourUtilization: {0:P1}\r\n", truckStatistics.PerformanceStatistics.DriverDutyHourUtilization);
                sb.AppendFormat("\tDrivingTimePercentage: {0:P1}\r\n", truckStatistics.PerformanceStatistics.DrivingTimePercentage);
                sb.AppendFormat("\tTotalTravelDistance: {0:P1}\r\n", truckStatistics.RouteStatistics.TotalTravelDistance);

                sb.AppendLine();
                sb.AppendLine("\tSegments");

                foreach (var segment in truckStatistics.RouteSegmentStatistics)
                {
                    sb.Append("\t\tSEGMENT - ");

                    sb.AppendFormat("OrderId: {0}", segment.StartStop.Location.DisplayName ?? "-");
                    sb.AppendFormat(", {0} > [{2}] > {1}", segment.StartStop.StopAction.ShortName, segment.EndStop.StopAction.ShortName, segment.TruckState.ToDescriptiveString());
                    sb.Append("\r\n");

                    sb.Append("\t\t\t");
                    sb.AppendFormat("Distance: {0:F1}", segment.Statistics.TotalTravelDistance);
                    sb.AppendFormat(", StartTime: {0:t}", segment.StartTime);
                    sb.AppendFormat(", EndTime: {0:t}", segment.EndTime);
                    sb.AppendFormat(", TotalTime: {0:dd\\.hh\\:mm\\:ss}", segment.Statistics.TotalTime);
                    sb.Append("\r\n");

                    sb.Append("\t\t\t");
                    sb.AppendFormat("IdleTime: {0:dd\\.hh\\:mm\\:ss}", segment.Statistics.TotalIdleTime);
                    sb.AppendFormat(", TravelTime: {0:dd\\.hh\\:mm\\:ss}", segment.Statistics.TotalTravelTime);
                    sb.AppendFormat(", ExecutionTime: {0:dd\\.hh\\:mm\\:ss}", segment.Statistics.TotalExecutionTime);
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
        }

    }
}