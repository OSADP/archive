using System;
using System.Collections.Generic;
using System.Linq;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Reporting.Model;

namespace PAI.CTIP.Optimization.Reporting.Model
{
    public class TruckPerformanceStatistics
    {
        public Dictionary<TruckState, RouteStatistics> RouteStatisticsByTruckState { get; set; }

        public RouteStatistics RouteStatistics { get; set; }

        public PerformanceStatistics PerformanceStatistics { get; set; }

        public IList<RouteSegmentStatistics> RouteSegmentStatistics { get; set; }
    }
}