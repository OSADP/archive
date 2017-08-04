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
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization.Reporting.Model
{
    /// <summary>
    /// A representation of the permormance statistics of a solution
    /// </summary>
    public class SolutionPerformanceStatistics
    {
        /// <summary>
        /// Gets or sets the truck statistics per solution.
        /// </summary>
        /// <value>
        /// The truck statistics indexed by solution.
        /// </value>
        public Dictionary<NodeRouteSolution, TruckPerformanceStatistics> TruckStatistics { get; set; }

        /// <summary>
        /// Gets or sets the total statistics of the route by truck state.
        /// </summary>
        /// <value>
        /// The total statistics of the route by truck state.
        /// </value>
        public Dictionary<TruckState, RouteStatistics> TotalRouteStatisticsByTruckState { get; set; }

        /// <summary>
        /// Gets or sets the total route statistics.
        /// </summary>
        /// <value>
        /// The total route statistics.
        /// </value>
        public RouteStatistics TotalRouteStatistics { get; set; }

        /// <summary>
        /// Gets or sets the number of jobs.
        /// </summary>
        /// <value>
        /// The number of jobs.
        /// </value>
        public int NumberOfJobs { get; set; }

        /// <summary>
        /// Gets or sets the average number of jobs.
        /// </summary>
        /// <value>
        /// The average number of jobs.
        /// </value>
        public double AverageNumberOfJobs { get; set; }

        /// <summary>
        /// Gets or sets the number of backhauls.
        /// </summary>
        /// <value>
        /// The number of backhauls.
        /// </value>
        public int NumberOfBackhauls { get; set; }

        /// <summary>
        /// Gets or sets the average number of backhauls.
        /// </summary>
        /// <value>
        /// The average number of backhauls.
        /// </value>
        public double AverageNumberOfBackhauls { get; set; }

        /// <summary>
        /// Gets or sets the number of loadmatches.
        /// </summary>
        /// <value>
        /// The number of loadmatches.
        /// </value>
        public int NumberOfLoadmatches { get; set; }

        /// <summary>
        /// Gets or sets the average number of loadmatches.
        /// </summary>
        /// <value>
        /// The average number of loadmatches.
        /// </value>
        public double AverageNumberOfLoadmatches { get; set; }

        /// <summary>
        /// Gets or sets the average driver duty hour utilization.
        /// </summary>
        /// <value>
        /// The average driver duty hour utilization.
        /// </value>
        public double AverageDriverDutyHourUtilization { get; set; }

        /// <summary>
        /// Gets or sets the average driver driving utilization.
        /// </summary>
        /// <value>
        /// The average driver driving utilization.
        /// </value>
        public double AverageDriverDrivingUtilization { get; set; }

        /// <summary>
        /// Gets or sets the driving time percentage.
        /// </summary>
        /// <value>
        /// The driving time percentage.
        /// </value>
        public double DrivingTimePercentage { get; set; }

        /// <summary>
        /// Gets or sets the waiting time percentage.
        /// </summary>
        /// <value>
        /// The waiting time percentage.
        /// </value>
        public double WaitingTimePercentage { get; set; }
    }
}