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

using System.Linq;
using System;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Planning;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Orders;

namespace PAI.FRATIS.SFL.Services.Planning
{
    public class ShiftConstants
    {
        public static TimeSpan Midnight = TimeSpan.FromHours(0);
        public static TimeSpan NightShiftStartTime = TimeSpan.FromHours(17);
        public static TimeSpan NightShiftEndTime = TimeSpan.FromHours(5);
    }

    public class PlanConfigService : EntitySubscriberServiceBase<PlanConfig>, IPlanConfigService
    {
        private readonly IRepository<Driver> _driverRepository;
        private readonly IJobService _jobService;


        public PlanConfigService(IRepository<PlanConfig> repository, ICacheManager cacheManager,
            IRepository<Driver> driverRepository, IJobService jobService)
            : base(repository, cacheManager)
        {
            _driverRepository = driverRepository;
            _jobService = jobService;
        }

        public IQueryable<Job> GetPlanConfigJobs(int planConfigId, int subscriberId)
        {
            var jobs = from j in _jobService.SelectWithAll(subscriberId)
                       where j.PlanConfigs.Any(p => p.Id == planConfigId) && j.IsDeleted == false
                       orderby j.DueDate ascending
                       select j;
            return jobs;
        }

        public IQueryable<Job> GetPlanConfigAvailableJobs(int planConfigId, int subscriberId)
        {
            var jobs = from j in _jobService.SelectWithAll(subscriberId)
                       where j.PlanConfigs.All(p => p.Id != planConfigId) && j.JobStatus == JobStatus.Unassigned && j.IsDeleted == false
                       orderby j.DueDate ascending
                       select j;
            return jobs;
        }

        public PlanConfig GetByIdWithAll(int id)
        {
            var planConfig = 
                _repository.SelectWith(
                    "JobGroup",
                    "Drivers",
                    "Drivers.StartingLocation",
                  //"Drivers.StartingLocation.State",
                    "Jobs",
                    "Jobs.Chassis",
                    "Jobs.ChassisOwner",
                    "Jobs.Container",
                    "Jobs.ContainerOwner",
                    "Jobs.RouteStops",
                    "Jobs.RouteStops.Location",
                    "Jobs.RouteStops.StopAction")
                .SingleOrDefault(f => f.Id == id);

            if (planConfig.JobGroup.Name == "Night")
            {
                for (var i = 0; i < planConfig.Jobs.Count; i++)
                {
                    var routeStops = planConfig.Jobs.ElementAt(i).RouteStops;
                    var endStopNumber = routeStops.Max(x => x.SortOrder);
                    var decidingStop = routeStops.FirstOrDefault(x => x.SortOrder == endStopNumber - 1);
                    if (decidingStop != null)
                    {
                        if (ShiftConstants.Midnight.Ticks <= decidingStop.WindowEnd && decidingStop.WindowEnd < ShiftConstants.NightShiftEndTime.Ticks)
                        {
                            for (var j = 0; j < routeStops.Count; j++)
                            {
                                routeStops[j].WindowStart += TimeSpan.FromDays(1).Ticks;
                                routeStops[j].WindowEnd += TimeSpan.FromDays(1).Ticks;
                            }
                        }
                    }
                }
            }

            return planConfig;
        }

        protected override IQueryable<PlanConfig> InternalSelect()
        {
            return _repository.SelectWith(
                "Drivers",
                "Drivers.StartingLocation",
                "Jobs", "Jobs.JobGroup",
                "Jobs.Chassis", 
                "Jobs.ChassisOwner", 
                "Jobs.Container", 
                "Jobs.ContainerOwner", 
                "Jobs.RouteStops", 
                "JobGroup");
        }



    }
}