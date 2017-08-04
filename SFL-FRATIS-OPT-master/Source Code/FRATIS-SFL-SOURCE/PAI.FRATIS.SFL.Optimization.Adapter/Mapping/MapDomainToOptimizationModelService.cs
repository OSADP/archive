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
using Omu.ValueInjecter;
using PAI.Drayage.EnhancedOptimization.Services;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Services;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Services.Planning;
using PAI.FRATIS.SFL.Services;
using PAI.FRATIS.SFL.Domain.Planning;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;
using PAI.FRATIS.SFL.Optimization.Adapter.Services;
using IRouteStopService = PAI.FRATIS.SFL.Services.Orders.IRouteStopService;
using Job = PAI.FRATIS.SFL.Domain.Orders.Job;
using Location = PAI.FRATIS.SFL.Domain.Geography.Location;
using Plan = PAI.Drayage.Optimization.Model.Planning.Plan;
using PlanConfig = PAI.Drayage.Optimization.Model.Planning.PlanConfig;
using StopAction = PAI.FRATIS.SFL.Domain.Orders.StopAction;
using TruckState = PAI.FRATIS.SFL.Domain.Planning.TruckState;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Mapping
{
    public interface IMapperService
    {
        void MapDomainToModel(Job job, Drayage.Optimization.Model.Orders.Job optimizationJob, DateTime? planDate);

        void MapDomainToModel(Domain.Orders.Driver driver, Drayage.Optimization.Model.Orders.Driver optimizationDriver);

        PlanConfig MapDomainToModel(PAI.FRATIS.SFL.Domain.Planning.PlanConfig planConfig);

        Plan MapDomainToModel(Domain.Planning.Plan plan);

        Domain.Geography.Location MapModelToDomain(Drayage.Optimization.Model.Location locationModel);

        void MapModelToDomain(Plan planModel, Domain.Planning.Plan plan);

        void SetOptimizerLocationQueueDelays(DayOfWeek dayOfWeek);

        Drayage.Optimization.Model.Planning.Plan MapDomainToModelWithoutPlaceHolder(FRATIS.SFL.Domain.Planning.Plan plan);
    }

    public class MapperService : IMapperService
    {
        private readonly IOptimizationDateTimeHelper _optDateTimeHelper;
        private readonly IRouteSanitizer _routeSanitizer;

        private readonly IRouteStopDelayService _routeStopDelayService;
        private readonly IRouteStopService _routeStopDataService;
        private readonly ILocationService _locationService;

        private readonly IJobService _jobService;
        private readonly IPlanService _planService;
        private readonly IDriverService _driverService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStopActionService _stopActionService;

        public MapperService(IRouteSanitizer routeSanitizer, IRouteStopService routeStopDataService,
            ILocationService locationService, IJobService jobService, IPlanService planService, 
            IDriverService driverService, IDateTimeHelper dateTimeHelper, IStopActionService stopActionService, 
            IRouteStopDelayService routeStopDelayService, 
            IOptimizationDateTimeHelper optDateTimeHelper)
        {
            _routeSanitizer = routeSanitizer;
            _routeStopDataService = routeStopDataService;
            _locationService = locationService;
            _jobService = jobService;
            _planService = planService;
            _driverService = driverService;
            _dateTimeHelper = dateTimeHelper;
            _stopActionService = stopActionService;
            _routeStopDelayService = routeStopDelayService;
            _optDateTimeHelper = optDateTimeHelper;
        }


        public Location MapModelToDomain(Drayage.Optimization.Model.Location locationModel)
        {
            var result = new Domain.Geography.Location();
            result.InjectFrom(locationModel);
            return result;
        }

        public void MapModelToDomain(Plan planModel, Domain.Planning.Plan plan)
        {
            // create map so we have reference to same route stop
            var routeStopMap = new Dictionary<RouteStop, Domain.Orders.RouteStop>();

            foreach (var driverPlanModel in planModel.DriverPlans)
            {
                PlanDriver planDriver = null;

                try
                {
                    if (driverPlanModel.Id > 0)
                    {
                        planDriver = plan.DriverPlans.FirstOrDefault(f => f.Id == driverPlanModel.Id);
                    }

                    if (planDriver == null)
                    {
                        planDriver = new PlanDriver
                        {
                            DriverId = driverPlanModel.Driver.Id
                        };

                        plan.DriverPlans.Add(planDriver);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
                // RouteSegmentMetrics persistance disabled - transaction overhead exceeds realtime computation delay

                // Metrics
                planDriver.RouteSegmentMetrics.Clear();
                int routeSegmentStatIndex = 0;
                foreach (var routeSegmentStatModel in driverPlanModel.RouteSegmentStatistics)
                {
                    var startStop = GetOrCreateDomainRouteStop(routeStopMap, routeSegmentStatModel.StartStop);
                    var endStop = GetOrCreateDomainRouteStop(routeStopMap, routeSegmentStatModel.EndStop);

                    int? jobId = null;
                    if (startStop.JobId.HasValue && startStop.JobId > 0)
                    {
                        jobId = startStop.JobId;
                    }
                    if (endStop.JobId.HasValue && endStop.JobId > 0)
                    {  
                        jobId = endStop.JobId;  // prefer the job id of the end stop
                    }

                    var routeSegmentMetric = new RouteSegmentMetric()
                    {
                        JobId = jobId,
                        StartStopId = startStop.LocationId,
                        EndStopId = endStop.LocationId,
                        StartStop = startStop,
                        EndStop = endStop,
                        PlanDriver = planDriver,
                        StartTime = routeSegmentStatModel.StartTime.Ticks,
                        TotalExecutionTime = routeSegmentStatModel.Statistics.TotalExecutionTime.Ticks,
                        TotalTravelDistance = routeSegmentStatModel.Statistics.TotalTravelDistance,
                        TotalTravelTime = routeSegmentStatModel.Statistics.TotalTravelTime.Ticks,
                        TotalIdleTime = routeSegmentStatModel.Statistics.TotalIdleTime.Ticks,
                        TotalQueueTime = routeSegmentStatModel.Statistics.TotalQueueTime.Ticks,
                        TruckState = (TruckState)routeSegmentStatModel.TruckState,
                        SortOrder = routeSegmentStatIndex++,
                    };

                    planDriver.RouteSegmentMetrics.Add(routeSegmentMetric);
                }
                
                // Update job plans
                int jobIndex = 0;
                foreach (var jobPlanModel in driverPlanModel.JobPlans)
                {
                    var planDriverJob = planDriver.JobPlans.FirstOrDefault(jp => jp.JobId == jobPlanModel.Job.Id);

                    if (planDriverJob == null)
                    {
                        planDriverJob = new PlanDriverJob()
                            {
                                JobId = jobPlanModel.Job.Id,
                                SortOrder = jobIndex,
                                DepartureTime = jobPlanModel.DepartureTime
                            };

                        planDriver.JobPlans.Add(planDriverJob);
                    }
                    else
                    {
                        if (planDriverJob.SortOrder != jobIndex)
                        {
                            planDriverJob.SortOrder = jobIndex;
                        }
                    }

                    jobIndex++;
                }

                // remove extra job plans
                var jobPlansToRemove = planDriver.JobPlans.Where(jp => driverPlanModel.JobPlans.All(f => f.Id != jp.Id)).ToList();
                foreach (var jp in jobPlansToRemove)
                {
                    planDriver.JobPlans.Remove(jp);
                }
            }

            foreach (var unassignedJob in planModel.UnassignedJobs)
            {
                var job = _jobService.GetById(unassignedJob.Id);
                plan.UnassignedJobs.Add(job);
            }
        }

        public void SetOptimizerLocationQueueDelays(DayOfWeek dayOfWeek)
        {
            throw new NotImplementedException();
        }

        public Domain.Orders.RouteStop GetOrCreateDomainRouteStop(Dictionary<RouteStop, Domain.Orders.RouteStop> map, RouteStop routeStopModel)
        {
            Domain.Orders.RouteStop routeStop = null;
            map.TryGetValue(routeStopModel, out routeStop);
            if (routeStop == null)
            {
                if (routeStopModel.Id > 0)
                {
                    routeStop = _routeStopDataService.GetById(routeStopModel.Id);
                }

                if (routeStop == null)
                {
                    routeStop = new Domain.Orders.RouteStop
                    {
                        StopActionId = routeStopModel.StopAction.Id,
                        LocationId = routeStopModel.Location != null ? routeStopModel.Location.Id : 0,
                    };
                }
                map[routeStopModel] = routeStop;
            }
            return routeStop;
        }

        public PlanConfig MapDomainToModel(PAI.FRATIS.SFL.Domain.Planning.PlanConfig planConfig)
        {
            var model = new PlanConfig();

            model.InjectFrom<DomainToModelValueInjection>(planConfig);

            foreach (var job in planConfig.Jobs)
            {
                var modelJob = model.Jobs.FirstOrDefault(f => f.Id == job.Id);
                MapDomainToModel(job, modelJob, planConfig.DueDate);
            }

            foreach (var driver in planConfig.Drivers)
            {
                var modelDriver = model.Drivers.FirstOrDefault(d => d.Id == driver.Id);
                var domainDriver = driver;
                MapDomainToModel(domainDriver, modelDriver);
            }

            return model;
        }


        public void MapDomainToModel(Domain.Orders.Driver driver, Drayage.Optimization.Model.Orders.Driver optimizationDriver)
        {
            optimizationDriver.InjectFrom<DomainToModelValueInjection>(driver);
        }

        public void MapDomainToModel(Job job, Drayage.Optimization.Model.Orders.Job optimizationJob, DateTime? planDate)
        {
            optimizationJob.InjectFrom<DomainToModelValueInjection>(job);

            var equipmentConfiguration = new EquipmentConfiguration();
            equipmentConfiguration.InjectFrom<DomainToModelValueInjection>(job);

            optimizationJob.EquipmentConfiguration = equipmentConfiguration;

            var modelRouteStops = optimizationJob.RouteStops.ToList();
            optimizationJob.RouteStops.Clear();

            if (job.RouteStops != null && optimizationJob.RouteStops != null)
            {
                job.RouteStops = job.RouteStops.OrderBy(p => p.SortOrder).ToList();

                
                int addDaysValue = 0;
                if (job.DueDate.HasValue && planDate.HasValue && job.DueDate.Value > planDate)
                {
                    addDaysValue = (int)job.DueDate.Value.Subtract(planDate.Value).TotalDays;
                    for (var x = 0; x < job.RouteStops.Count; x++)
                    {
                        if (addDaysValue >= 1)
                        {
                            //job.RouteStops[x].WindowStart = 
                            //    job.RouteStops[x].WindowStart +
                                                            
                            //    TimeSpan.FromDays(addDaysValue).Ticks;
                            //job.RouteStops[x].WindowEnd = job.RouteStops[x].WindowEnd +
                            //                                TimeSpan.FromDays(addDaysValue).Ticks;
                        }
                    }
                }

                var routeStopIndex = 0;
                foreach (var routeStop in job.RouteStops)
                {
                    var modelRouteStop = modelRouteStops.FirstOrDefault(f => f.Id == routeStop.Id);
                    if (modelRouteStop != null)
                    {
                        routeStop.SortOrder = routeStopIndex++;

                        if (routeStop.Location != null && routeStop.Location.WaitingTime.HasValue)
                        {
                            modelRouteStop.QueueTime = new TimeSpan(0, routeStop.Location.WaitingTime.Value, 0);
                        }

                        // fixed hotspot
                        if (routeStop.LocationId != null && routeStop.Location == null)
                        {
                            var location = _locationService.GetById((int)routeStop.LocationId);
                            routeStop.LocationId = location.Id;
                            routeStop.Location = location;
                        }

                        if (routeStop.StopAction != null)
                        {
                            modelRouteStop.StopAction = StopActions.Actions.FirstOrDefault(p => p.ShortName == routeStop.StopAction.ShortName);
                        }

                        if (routeStop.StopDelay.HasValue)
                        {
                            modelRouteStop.ExecutionTime = new TimeSpan(0, Convert.ToInt32(routeStop.StopDelay.Value), 0);
                        }

                        var dueDateLocal = _dateTimeHelper.ConvertUtcToLocalTime(job.DueDate.Value);


                        modelRouteStop.WindowStart = new TimeSpan(routeStop.WindowStart);
                        modelRouteStop.WindowEnd = new TimeSpan(routeStop.WindowEnd);

                        // map stop action
                        modelRouteStop.StopAction = MapDomainToModel(routeStop.StopAction);
                        // Remove Dynamic stops
                        if (routeStop.IsDynamicStop)
                        {
                            optimizationJob.RouteStops.Remove(modelRouteStop);
                        }

                        optimizationJob.RouteStops.Add(modelRouteStop);
                    }
                    else
                    {
                        var nullModelRouteStop = modelRouteStop;
                    }
                }
            }
            
            _routeSanitizer.PrepareJob(optimizationJob);
        }

        public Drayage.Optimization.Model.Orders.StopAction MapDomainToModel(StopAction stopAction)
        {
            return StopActions.Actions.SingleOrDefault(f => f.ShortName == stopAction.ShortName);
        }

        private static PAI.Drayage.Optimization.Model.Orders.RouteStop ConvertRouteStop(PAI.FRATIS.SFL.Domain.Orders.RouteStop x)
        {
            var routeStop = new PAI.Drayage.Optimization.Model.Orders.RouteStop();

            routeStop.Id = x.Id;
            routeStop.ExecutionTime = 
                x.StopDelay.HasValue
                    ? TimeSpan.FromMinutes(x.StopDelay.Value)
                    : (TimeSpan?)null;
            routeStop.QueueTime = 
                x.Location != null
                    ? TimeSpan.FromMinutes(x.Location.WaitingTime ?? 0) 
                    : (TimeSpan?)null;
            routeStop.PostTruckConfig = new TruckConfiguration();
            routeStop.PreTruckConfig = new TruckConfiguration();
            routeStop.StopAction = 
                x.StopAction != null
                    ? routeStop.StopAction = StopActions.Actions.First(y => y.Name == x.StopAction.Name)
                    : StopActions.Actions.First(y => y.Name == "No Action");
            routeStop.WindowEnd = new TimeSpan(x.WindowEnd);
            routeStop.WindowStart = new TimeSpan(x.WindowStart);

            if (x.Location != null)
            {
                routeStop.Location =
                    new Drayage.Optimization.Model.Location
                        {
                            Id = x.Location.Id,
                            DisplayName = x.Location.DisplayName,
                            Latitude = x.Location.Latitude ?? 0,
                            Longitude = x.Location.Longitude ?? 0
                        };
            }
            else
            {
                routeStop.Location =
                    new Drayage.Optimization.Model.Location
                    {
                        Id = x.LocationId ?? 0,
                        DisplayName = string.Empty,
                        Latitude = 0,
                        Longitude = 0
                    };
            }

            return routeStop;
        }

        public Drayage.Optimization.Model.Planning.Plan MapDomainToModel(FRATIS.SFL.Domain.Planning.Plan plan)
        {
            var model = new Plan();
            model.InjectFrom<DomainToModelValueInjection>(plan);

            foreach (var driverPlan in plan.DriverPlans)
            {
                var modelDriverPlan = model.DriverPlans.FirstOrDefault(f => f.Id == driverPlan.Id);

                if (modelDriverPlan == null) 
                    continue;
                
                var driver = driverPlan.Driver ?? _driverService.GetById(driverPlan.DriverId);

                var driverModel = new Drayage.Optimization.Model.Orders.Driver();
                driverModel.InjectFrom<DomainToModelValueInjection>(driver);

                modelDriverPlan.Driver = driverModel.InjectFrom(driver) as Drayage.Optimization.Model.Orders.Driver;


                modelDriverPlan.JobPlans = modelDriverPlan.JobPlans.OrderBy(f => f.SortOrder).ToList();
                foreach (var jobPlan in driverPlan.JobPlans)
                {
                    //var driverPlanJob = _planService.GetPlanDriverJobsById(jobPlan.Id);

                    var modelJobPlan = modelDriverPlan.JobPlans.FirstOrDefault(f => f.Id == jobPlan.Id);
                    modelJobPlan.Job = modelJobPlan.Job ?? new Drayage.Optimization.Model.Orders.Job();

                    MapDomainToModel(jobPlan.Job, modelJobPlan.Job, plan.PlanConfig.DueDate);
                }
                if (driverPlan.RouteSegmentMetrics == null)
                {
                    driverPlan.RouteSegmentMetrics = new List<RouteSegmentMetric>();
                }
                
                modelDriverPlan.RouteSegmentStatistics = new List<RouteSegmentStatistics>();

                foreach (var x in driverPlan.RouteSegmentMetrics)
                {
                    

                    var rss = new RouteSegmentStatistics();
                    var endStop = ConvertRouteStop(x.EndStop);
                    var startStop = ConvertRouteStop(x.StartStop);
                    var statistics = new RouteStatistics();
                    var startTime = new TimeSpan(x.StartTime ?? 0);

                    statistics.TotalCapacity = 0;
                    statistics.TotalExecutionTime = new TimeSpan(x.TotalExecutionTime);
                    statistics.TotalIdleTime = new TimeSpan(x.TotalIdleTime);
                    statistics.TotalQueueTime = new TimeSpan(x.TotalQueueTime);
                    statistics.TotalTravelDistance = x.TotalTravelDistance;
                    statistics.TotalTravelTime = new TimeSpan(x.TotalTravelTime);

                    rss.EndStop = endStop;
                    rss.StartStop = startStop;
                    rss.StartTime = startTime;
                    rss.Statistics = statistics;
                    rss.WhiffedTimeWindow = false;
                    modelDriverPlan.RouteSegmentStatistics.Add(rss);
                }
            }

            return model;
        }

        public Drayage.Optimization.Model.Planning.Plan MapDomainToModelWithoutPlaceHolder(FRATIS.SFL.Domain.Planning.Plan plan)
        {
            var model = new Plan();
            model.InjectFrom<DomainToModelValueInjection>(plan);

            foreach (var driverPlan in plan.DriverPlans)
            {
                var modelDriverPlan = model.DriverPlans.FirstOrDefault(f => f.Id == driverPlan.Id);

                if (modelDriverPlan == null)
                    continue;

                var driver = driverPlan.Driver ?? _driverService.GetById(driverPlan.DriverId);

                var driverModel = new Drayage.Optimization.Model.Orders.Driver();
                driverModel.InjectFrom<DomainToModelValueInjection>(driver);

                modelDriverPlan.Driver = driverModel.InjectFrom(driver) as Drayage.Optimization.Model.Orders.Driver;

                modelDriverPlan.RouteSegmentStatistics = new List<RouteSegmentStatistics>();

                if (driverPlan.Driver.IsPlaceholderDriver)
                {
                    continue;
                }

                modelDriverPlan.JobPlans = modelDriverPlan.JobPlans.OrderBy(f => f.SortOrder).ToList();
                foreach (var jobPlan in driverPlan.JobPlans)
                {
                    //var driverPlanJob = _planService.GetPlanDriverJobsById(jobPlan.Id);

                    var modelJobPlan = modelDriverPlan.JobPlans.FirstOrDefault(f => f.Id == jobPlan.Id);
                    modelJobPlan.Job = modelJobPlan.Job ?? new Drayage.Optimization.Model.Orders.Job();

                    MapDomainToModel(jobPlan.Job, modelJobPlan.Job, plan.PlanConfig.DueDate);
                }
                if (driverPlan.RouteSegmentMetrics == null)
                {
                    driverPlan.RouteSegmentMetrics = new List<RouteSegmentMetric>();
                }

                foreach (var x in driverPlan.RouteSegmentMetrics)
                {


                    var rss = new RouteSegmentStatistics();
                    var endStop = ConvertRouteStop(x.EndStop);
                    var startStop = ConvertRouteStop(x.StartStop);
                    var statistics = new RouteStatistics();
                    var startTime = new TimeSpan(x.StartTime ?? 0);

                    statistics.TotalCapacity = 0;
                    statistics.TotalExecutionTime = new TimeSpan(x.TotalExecutionTime);
                    statistics.TotalIdleTime = new TimeSpan(x.TotalIdleTime);
                    statistics.TotalQueueTime = new TimeSpan(x.TotalQueueTime);
                    statistics.TotalTravelDistance = x.TotalTravelDistance;
                    statistics.TotalTravelTime = new TimeSpan(x.TotalTravelTime);

                    rss.EndStop = endStop;
                    rss.StartStop = startStop;
                    rss.StartTime = startTime;
                    rss.Statistics = statistics;
                    rss.WhiffedTimeWindow = false;
                    modelDriverPlan.RouteSegmentStatistics.Add(rss);
                }
            }

            return model;
        }

    }
}
