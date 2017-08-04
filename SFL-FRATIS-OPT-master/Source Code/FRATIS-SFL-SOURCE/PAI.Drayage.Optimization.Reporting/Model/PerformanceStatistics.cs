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
using System.Linq;

namespace PAI.Drayage.Optimization.Reporting.Model
{
    /// <summary>
    /// A collection of statistics used to compare performance
    /// </summary>
    public class PerformanceStatistics
    {
        /// <summary>
        /// Gets or sets the number of jobs.
        /// </summary>
        /// <value>
        /// The number of jobs.
        /// </value>
        public int NumberOfJobs { get; set; }

        /// <summary>
        /// Gets or sets the number of backhauls.
        /// </summary>
        /// <value>
        /// The number of backhauls.
        /// </value>
        public int NumberOfBackhauls { get; set; }

        /// <summary>
        /// Gets or sets the number of live loading stops followed by live unloading stops.
        /// </summary>
        /// <value>
        /// The number of live loading stops followed by live unloading stops.
        /// </value>
        public int NumberOfLoadmatches { get; set; }

        /// <summary>
        /// Gets or sets the driver duty hour utilization.
        /// </summary>
        /// <value>
        /// The driver duty hour utilization.
        /// </value>
        public double DriverDutyHourUtilization { get; set; }

        /// <summary>
        /// Gets or sets the driver driving utilization.
        /// </summary>
        /// <value>
        /// The driver driving utilization.
        /// </value>
        public double DriverDrivingUtilization { get; set; }

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

        /// <summary>
        /// Implements the addition operator.
        /// </summary>
        /// <param name="c1">The left hand operand.</param>
        /// <param name="c2">The right hand operand.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static PerformanceStatistics operator + (PerformanceStatistics c1, PerformanceStatistics c2)
        {
            var result = new PerformanceStatistics()
                {
                    NumberOfJobs = c1.NumberOfJobs + c2.NumberOfJobs,
                    NumberOfBackhauls = c1.NumberOfBackhauls + c2.NumberOfBackhauls,
                    NumberOfLoadmatches = c1.NumberOfLoadmatches + c2.NumberOfLoadmatches,
                    DriverDutyHourUtilization = c1.DriverDutyHourUtilization + c2.DriverDutyHourUtilization,
                    DriverDrivingUtilization = c1.DriverDrivingUtilization + c2.DriverDrivingUtilization,
                    DrivingTimePercentage = c1.DrivingTimePercentage + c2.DrivingTimePercentage,
                    WaitingTimePercentage = c1.WaitingTimePercentage + c2.WaitingTimePercentage
                };

            return result;
        }

    }
}