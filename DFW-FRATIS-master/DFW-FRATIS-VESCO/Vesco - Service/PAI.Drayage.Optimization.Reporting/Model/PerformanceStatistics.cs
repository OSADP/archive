using System;
using System.Linq;

namespace PAI.CTIP.Optimization.Reporting.Model
{
    public class PerformanceStatistics
    {
        public int NumberOfJobs { get; set; }

        public int NumberOfBackhauls { get; set; }

        public int NumberOfLoadmatches { get; set; }

        public double DriverDutyHourUtilization { get; set; }

        public double DriverDrivingUtilization { get; set; }

        public double DrivingTimePercentage { get; set; }

        public double WaitingTimePercentage { get; set; }

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