using System;
using System.Collections.Generic;
using System.Linq;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Node;

namespace PAI.CTIP.Optimization.Reporting.Model
{
    public class SolutionPerformanceStatistics
    {
        public Dictionary<NodeRouteSolution, TruckPerformanceStatistics> TruckStatistics { get; set; }

        public Dictionary<TruckState, RouteStatistics> TotalRouteStatisticsByTruckState { get; set; }

        public RouteStatistics TotalRouteStatistics { get; set; }
        
        public int NumberOfJobs { get; set; }

        public double AverageNumberOfJobs { get; set; }

        public int NumberOfBackhauls { get; set; }

        public double AverageNumberOfBackhauls { get; set; }

        public int NumberOfLoadmatches { get; set; }

        public double AverageNumberOfLoadmatches { get; set; }

        public double AverageDriverDutyHourUtilization { get; set; }

        public double AverageDriverDrivingUtilization { get; set; }

        public double DrivingTimePercentage { get; set; }

        public double WaitingTimePercentage { get; set; }
    }
}