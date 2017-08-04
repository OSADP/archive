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
using System.Threading.Tasks;
using PAI.FRATIS.SFL.Domain.Planning;
using PAI.FRATIS.SFL.Services;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public struct TerminalJobs
    {
        private IEnumerable<TerminalJob> Jobs { get; set; }
    }

    public struct TerminalJob
    {
        public int? TerminalLocationId { get; set; }

        public int? JobId { get; set; }

        public int? RouteStopId { get; set; }

        public DateTime? ETA { get; set; }
    }

    public interface IPlanAnalyzerService
    {
        ICollection<TerminalArrivalEstimation> GetArrivalEstimations(
            Plan plan,
            IEnumerable<int> locationIds,
            bool recalculateStatistics = true,
            PlanAnalyzerService.TimeZonePreference timeZone = PlanAnalyzerService.TimeZonePreference.Utc);

    }
    
    public struct TerminalArrivalEstimation
    {
        public int? JobId { get; set; }

        public int? RouteStopId { get; set; }

        public int? StartLocationId { get; set; }

        public int? EndLocationId { get; set; }

        public DateTime? ArrivalTime { get; set; }
    }

    public class PlanAnalyzerService : IPlanAnalyzerService
    {
        private readonly IPlanGenerator _planGenerator;

        private readonly IDateTimeHelper _dateTimeHelper;

        public PlanAnalyzerService(IPlanGenerator planGenerator, IDateTimeHelper dateTimeHelper)
        {
            _planGenerator = planGenerator;
            _dateTimeHelper = dateTimeHelper;
        }

        public enum TimeZonePreference
        {
            Local,
            Utc
        }
        public ICollection<TerminalArrivalEstimation> GetArrivalEstimations(Plan plan, IEnumerable<int> locationIds, bool recalculateStatistics = true, TimeZonePreference timeZone = TimeZonePreference.Utc)
        {
            var result = new List<TerminalArrivalEstimation>();

            if (plan != null)
            {
                // force recalculation of statistics if requested or if DriverPlan contains a Metric object that is null
                if (recalculateStatistics || 
                    (plan.DriverPlans.Any() && plan.DriverPlans.Where(p => p.RouteSegmentMetrics == null).ToList().Any()))
                {
                    _planGenerator.RecalculatePlanStatistics(plan);
                }

                foreach (var driverPlans in plan.DriverPlans)
                {
                    var driverMetrics = driverPlans.RouteSegmentMetrics.ToList();
                    for (int i=0; i<driverMetrics.Count; i++)
                    {
                        var m = driverMetrics[i];
                        if (m.EndStop.LocationId.HasValue && (locationIds == null || locationIds.Contains(m.EndStop.LocationId.Value)))
                        {
                            if (m.StartTime.HasValue == false) continue;

                            var match = new TerminalArrivalEstimation()
                                {
                                    JobId = m.EndStop.JobId,
                                    RouteStopId = m.EndStop.Id,
                                    StartLocationId = m.StartStop.LocationId,
                                    EndLocationId = m.EndStop.LocationId.Value,
                                    ArrivalTime = plan.PlanConfig.DueDate.Date.AddTicks(m.StartTime.Value + m.TotalTravelTime),
                                };

                            if (i > 0)
                            {
                                match.ArrivalTime = match.ArrivalTime.Value.AddTicks(m.TotalExecutionTime);
                            }

                            if (timeZone == TimeZonePreference.Utc && match.ArrivalTime.HasValue)
                            {
                                match.ArrivalTime = _dateTimeHelper.ConvertLocalToUtcTime(match.ArrivalTime.Value);
                            }

                            result.Add(match);
                        }
                    }

                }
            }

            return result;
        }
    }
}
