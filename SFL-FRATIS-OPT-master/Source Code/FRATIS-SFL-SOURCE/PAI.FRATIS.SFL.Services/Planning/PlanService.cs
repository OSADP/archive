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

using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Domain.Planning;
using PAI.FRATIS.SFL.Services.Core;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Orders;

namespace PAI.FRATIS.SFL.Services.Planning
{
    public interface IPlanService : IEntitySubscriberServiceBase<Plan>
    {
        Plan GetByIdWithAll(int id);
        IQueryable<Plan> SelectWithAllCompact();
        IQueryable<PlanDriverJob> GetPlanDriverJobsByPlanDriver(int planDriverId);
        PlanDriverJob GetPlanDriverJobsById(int planDriverJobId);
        IQueryable<RouteSegmentMetric> GetPlanDriverMetrics(int planDriverId);
        IEnumerable<int> GetUnsendableJobIds(Plan plan);
        bool AcceptPlan(Plan entity);
    }

    public class PlanService : EntitySubscriberServiceBase<Plan>, IPlanService
    {
        private readonly IRepository<RouteSegmentMetric> _routeSegmentMetricRepository;
        private readonly IRepository<PlanDriverJob> _planDriverJobRepository;
        private readonly IStopActionService _stopActionService;

        public PlanService(IRepository<Plan> repository, ICacheManager cacheManager, 
            IRepository<PlanDriverJob> planDriverJobRepository, IRepository<RouteSegmentMetric> routeSegmentMetricRepository, IStopActionService stopActionService)
            : base(repository, cacheManager)
        {
            _planDriverJobRepository = planDriverJobRepository;
            _routeSegmentMetricRepository = routeSegmentMetricRepository;
            _stopActionService = stopActionService;
        }

        public PlanDriverJob GetPlanDriverJobsById(int planDriverJobId)
        {
            return SelectPlanDriverJob().FirstOrDefault(f => f.Id == planDriverJobId);
        }

        public IQueryable<PlanDriverJob> GetPlanDriverJobsByPlanDriver(int planDriverId)
        {
            return SelectPlanDriverJob().Where(f => f.PlanDriver.Id == planDriverId).OrderBy(f => f.SortOrder);
        }

        private IQueryable<PlanDriverJob> SelectPlanDriverJob()
        {
            return _planDriverJobRepository.SelectWith(
                "Job.Chassis",
                "Job.ChassisOwner",
                "Job.Container",
                "Job.ContainerOwner",
                "Job.RouteStops", 
                "Job.RouteStops.Location", 
                "Job.RouteStops.StopAction");
        }

        public Plan GetByIdWithAll(int id)
        {
            return SelectWithAll().SingleOrDefault(f => f.Id == id);
        }
        
        public IQueryable<RouteSegmentMetric> GetPlanDriverMetrics(int planDriverId)
        {
            return _routeSegmentMetricRepository.SelectWith(
                "StartStop",
                "StartStop.Location",
                "StartStop.StopAction",
                "EndStop",
                "EndStop.Location",
                "EndStop.StopAction")
                .Where(f => f.PlanDriverId == planDriverId)
                .OrderBy(f => f.SortOrder);
        }

        public override IQueryable<Plan> SelectWithAll()
        {
            return _repository.SelectWith(
                "PlanConfig",
                "JobGroup",
                "DriverPlans",
                "DriverPlans.Driver",
                "DriverPlans.Driver.StartingLocation",
                "DriverPlans.JobPlans",
                "DriverPlans.JobPlans.Job",
                "DriverPlans.JobPlans.Job.Chassis",
                "DriverPlans.JobPlans.Job.ChassisOwner",
                "DriverPlans.JobPlans.Job.Container",
                "DriverPlans.JobPlans.Job.ContainerOwner",
                "DriverPlans.JobPlans.Job.RouteStops",
                "DriverPlans.JobPlans.Job.RouteStops.Location",
                "DriverPlans.JobPlans.Job.RouteStops.StopAction",
                "DriverPlans.JobPlans.PlanDriver",
                "PlanConfig",
                "PlanConfig.Drivers",
                "UnassignedJobs",
                "UnassignedJobs.Chassis",
                "UnassignedJobs.ChassisOwner",
                "UnassignedJobs.Container",
                "UnassignedJobs.ContainerOwner",
                "UnassignedJobs.RouteStops",
                "UnassignedJobs.RouteStops.Location",
                "UnassignedJobs.RouteStops.StopAction");
        }

        public IQueryable<Plan> SelectWithAllCompact()
        {
            return _repository.SelectWith(
                "PlanConfig",
                "JobGroup",
                "PlanConfig",
                "PlanConfig.Drivers",
                "PlanConfig.JobGroup");
        }

        /// <summary>
        /// Gets the Job Ids of orders unable to be sent via WebFleet
        /// due to assignment to Placeholder Driver or a Driver
        /// without a WebFleet VehicleId
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public IEnumerable<int> GetUnsendableJobIds(Plan plan)
        {
            var result = new HashSet<int>();
            if (plan != null)
            {
                var placeholderDriverJobIds = plan.DriverPlans.Where(p => p.Driver.IsPlaceholderDriver).SelectMany(p => p.JobPlans).Select(p => p.JobId).ToList();
                foreach (var jobId in placeholderDriverJobIds)
                {
                    result.Add(jobId);
                }

                foreach (var unassignedJobId in plan.UnassignedJobs.Select(p => p.Id))
                {
                    result.Add(unassignedJobId);
                }
            }

            return result.ToList();
        }

        private void UpdateStopActions(Plan plan)
        {
            //foreach (var driverPlan in plan.DriverPlans)
            //{
            //    foreach (var jobPlan in driverPlan.JobPlans)
            //    {
            //        foreach (var rs in jobPlan.RouteStops)
            //        {
            //            var sa = _stopActionService.StopActions.FirstOrDefault(p => p.ShortName == rs.StopAction.ShortName);
            //            rs.StopActionId = sa.Id;
            //        }
            //    }
            //}
        }
        public override void Update(Plan entity, bool saveChanges = true)
        {
            UpdateStopActions(entity);
            base.Update(entity, saveChanges);
        }

        public bool AcceptPlan(Plan entity)        
        {
            var e = InternalGetById(entity.Id);
            e.IsAccepted = true;
            Update(e);
            return true;
        }

        public override void Insert(Plan entity, bool saveChanges = true)
        {
            UpdateStopActions(entity);
            base.Insert(entity, saveChanges);
        }
    }
}