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
using System.Security.Cryptography.X509Certificates;
using Omu.ValueInjecter;
using PAI.Drayage.EnhancedOptimization.Model;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Model.Planning;
using PAI.Drayage.Optimization.Reporting.Model;
using PAI.Drayage.Optimization.Reporting.Services;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Services.Configuration;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Services.Planning;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;

using Driver = PAI.FRATIS.SFL.Domain.Orders.Driver;
using Job = PAI.FRATIS.SFL.Domain.Orders.Job;
using Plan = PAI.FRATIS.SFL.Domain.Planning.Plan;
using PlanConfig = PAI.FRATIS.SFL.Domain.Planning.PlanConfig;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public class PlanGenerator : IPlanGenerator
    {
        private readonly IPlanService _planService;
        private readonly IReportingService _reportingService;
        private readonly IDriverService _driverService;
        private readonly IMapperService _mapperService;
        private readonly IConfigurationProvider<OptimizerSettings> _settingsConfigurationProvider;
        private readonly Drayage.EnhancedOptimization.Services.IPlanGenerator _planGenerator;
        private readonly IDataExtractor dataExtractor;
        private readonly IEnumerable<IPlanGeneratorInitializer> _planGeneratorInitializers;

        public PlanGenerator(
            IPlanService planService,
            IDriverService driverService, 
            IMapperService mapperService, 
            IConfigurationProvider<OptimizerSettings> settingsConfigurationProvider, 
            Drayage.EnhancedOptimization.Services.IPlanGenerator planGenerator, 
            IDataExtractor dataExtractor,
            IEnumerable<IPlanGeneratorInitializer> planGeneratorInitializers, 
            IReportingService reportingService)
        {
            _planService = planService;
            _driverService = driverService;
            _mapperService = mapperService;
            _settingsConfigurationProvider = settingsConfigurationProvider;
            _planGenerator = planGenerator;
            this.dataExtractor = dataExtractor;
            _planGeneratorInitializers = planGeneratorInitializers;
            _reportingService = reportingService;
        }

        public PlanGenerationResult GeneratePlan(PlanConfig planConfig)
        {
            if (planConfig == null)
            {
                throw new ArgumentNullException("planConfig");
            }
            
            PlanGenerationResult result;
            try
            {
                var runNumber = _planService.Select().Count(f => f.PlanConfig.Id == planConfig.Id);

                // select placeholder driver
                var placeholderDriverEntity = _driverService.Select().FirstOrDefault(f => f.IsPlaceholderDriver);

                var placeholderDriver = new Drayage.Optimization.Model.Orders.Driver();
                placeholderDriver.InjectFrom<DomainToModelValueInjection>(placeholderDriverEntity);

                var optimizerSettings = _settingsConfigurationProvider.Settings;
                
                // Prefetch and stuff 
                foreach (var planGeneratorInitializer in _planGeneratorInitializers)
                {
                    planGeneratorInitializer.Initialize(planConfig);
                }

                // Domain Model > Optimization Model
                var planConfigModel = _mapperService.MapDomainToModel(planConfig);

                // set placeholder driver earliest start time
                placeholderDriver.EarliestStartTime = planConfig.JobGroup.ShiftStartTime.HasValue
                    ? planConfig.JobGroup.ShiftStartTime.Value.Ticks
                    : 0;
                
                // Run Optimization
                var planModel = _planGenerator.GeneratePlan(planConfigModel, placeholderDriver, optimizerSettings);

                var driverIds = planModel.DriverPlans.Select(p => p.Driver.Id).ToList();
                var driversWithoutPlans = planConfigModel.Drivers.Where(p => !driverIds.Contains(p.Id));                
                foreach (var d in driversWithoutPlans)
                {
                    planModel.DriverPlans.Add(new PlanDriver()
                    {
                        Driver = d,
                        JobPlans = new List<PlanDriverJob>(),
                        RouteSegmentStatistics = new List<RouteSegmentStatistics>()
                    });
                }

                if (planModel.UnassignedJobs.Any())
                {
                    var unassignablePlan = new PlanDriver()
                    {
                        Driver = placeholderDriver,
                        JobPlans = new List<PlanDriverJob>(),
                        RouteSegmentStatistics = new List<RouteSegmentStatistics>()
                    };

                    foreach (var j in planModel.UnassignedJobs)
                    {
                        unassignablePlan.JobPlans.Add(
                            new PlanDriverJob()
                            {
                                Job = j,
                                RouteStops = j.RouteStops.ToList()
                            });
                    }

                    planModel.DriverPlans.Add(unassignablePlan);
                }

                // Optimization Model > Domain Model
                var plan = CreateDomainPlan(planConfig, planModel, runNumber, false);
                var plan2 = CreateDomainPlan(planConfig, planModel, runNumber, true);
                
                result = new PlanGenerationResult()
                {
                    Success = true,
                    Plan = plan
                };
            }
            catch (Exception ex)
            {
                result = new PlanGenerationResult()
                {
                    Success = false,
                    Errors = new string[] { ex.Message }
                };

                throw;
            }

            return result;
        }

        public Domain.Planning.Plan CreateDomainPlan(
            PAI.FRATIS.SFL.Domain.Planning.PlanConfig planConfig, 
            Drayage.Optimization.Model.Planning.Plan planModel, 
            int run, 
            bool userCreated)
        {
            var plan = new PAI.FRATIS.SFL.Domain.Planning.Plan()
            {
                Run = run,
                UserCreated = userCreated,
                PlanConfig = planConfig
            };

            _mapperService.MapModelToDomain(planModel, plan);

            plan.SubscriberId = planConfig.SubscriberId;
            _planService.Insert(plan);

            return plan;
        }

        public void RecalculatePlanStatistics(Drayage.Optimization.Model.Planning.Plan plan)
        {
            _planGenerator.RecalculatePlanStatistics(plan);
        }

        public void RecalculatePlanStatisticsWithoutPlaceHolder(FRATIS.SFL.Domain.Planning.Plan plan)
        {
            // domain > plan model
            var planModel = _mapperService.MapDomainToModelWithoutPlaceHolder(plan);

            _planGenerator.RecalculatePlanStatistics(planModel);

            _mapperService.MapModelToDomain(planModel, plan);
        }

        public void RecalculatePlanStatistics(FRATIS.SFL.Domain.Planning.Plan plan)
        {
            // domain > plan model
            var planModel = _mapperService.MapDomainToModel(plan);

            _planGenerator.RecalculatePlanStatistics(planModel);

            _mapperService.MapModelToDomain(planModel, plan);
        }

        public Solution GetSolutionFromPlan(Plan plan)
        {
            var planModel = _mapperService.MapDomainToModel(plan);
            return _planGenerator.GetSolutionFromPlan(planModel);
        }

        public SolutionPerformanceStatistics GetSolutionPerformanceStatistics(Plan plan)
        {
            var solution = GetSolutionFromPlan(plan);
            return this.dataExtractor.GetSolutionPerformanceStatistics(solution);
        }

        public IList<int> GetInfeasibleJobIds(IList<Job> jobs, Driver driver, DateTime? planDate)
        {
            var optimizationJobs = new List<Drayage.Optimization.Model.Orders.Job>();
            foreach (var job in jobs)
            {
                var optimizationJob = new Drayage.Optimization.Model.Orders.Job();
                _mapperService.MapDomainToModel(job, optimizationJob, planDate);
                optimizationJobs.Add(optimizationJob);
            }

            var optimizationDriver = new Drayage.Optimization.Model.Orders.Driver();
            _mapperService.MapDomainToModel(driver, optimizationDriver);

            var result = _planGenerator.GetInfeasibleJobIds(optimizationJobs, optimizationDriver);
            return result;
        }

        public Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Plan plan)
        {
            var solution = GetSolutionFromPlan(plan);
            var intermediateResults = new Dictionary<Domain.Planning.PlanDriver, IList<RouteSegmentStatistics>>();
            foreach (var nodeRouteSolution in solution.RouteSolutions)
            {
                nodeRouteSolution.StartTime = nodeRouteSolution.Nodes.First().WindowStart;
                var routeSegmentStatistics = _reportingService.GetRouteSegmentStats(nodeRouteSolution);
                var planDriver = plan.DriverPlans.First(x => x.DriverId == nodeRouteSolution.DriverNode.Driver.Id);
                if (!intermediateResults.ContainsKey(planDriver))
                {
                    intermediateResults.Add(planDriver, routeSegmentStatistics);
                }
                else
                {
                    intermediateResults[planDriver] = intermediateResults[planDriver].Concat(routeSegmentStatistics).ToList();
                }
            }

            var results = GetDriverPlanVerboseRoutes(intermediateResults);

            return results;
        }

        private string FormatSegmentTimeForUIDisplay(TimeSpan timeSpanToFormat)
        {
            const string outputTemplate = "{0} hrs {1} mi";
            var formatHours = timeSpanToFormat.Hours;
            var formatMinutes = timeSpanToFormat.Minutes;
            if (timeSpanToFormat.Seconds >= 30)
            {
                formatMinutes++;
            }
            return string.Format(outputTemplate, formatHours, formatMinutes);
        }

        public Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Dictionary<Domain.Planning.PlanDriver, IList<RouteSegmentStatistics>> statisticsByNodeRouteSolutionDictionary)
        {
            var result = new Dictionary<int, List<string>>();

            foreach (var statisticsByNodeRouteSolution in statisticsByNodeRouteSolutionDictionary)
            {
                var planDriver = statisticsByNodeRouteSolution.Key;
                var routeSegmentStatistics = statisticsByNodeRouteSolution.Value;

                var previousDepartureTime = TimeSpan.Zero;

                var sequence = new List<string>();

                for (var i = 0; i < routeSegmentStatistics.Count; i++)
                {
                    var statisticTemplate = string.Empty;
                    var rsm = routeSegmentStatistics[i];
                    var previousDepartTimeString = string.Empty;
                    var arrivalTimeString = string.Empty;

                    if (i == 0)
                    {
                        statisticTemplate = "Starting from {0} at {1} going to {2} ({3}) - (waited {4}) Travelling {5} miles in {6} | queue {7}";
                    }
                    else
                    {
                        var prsm = routeSegmentStatistics[i - 1];
                        previousDepartureTime = prsm.EndTime - prsm.Statistics.TotalTravelTime;
                        previousDepartTimeString = previousDepartureTime.ToString(@"hh\:mm");
                        arrivalTimeString = rsm.StartTime.ToString(@"hh\:mm");

                        if (i > 0 && i < routeSegmentStatistics.Count - 1)
                        {
                            statisticTemplate = "Next stop @ {2} ({3}) departing prev stop @ {8}, arriving @ {9} - (waited {4}) travelling {5} miles in {6} | queue {7}";
                        }
                        if (i == routeSegmentStatistics.Count - 1)
                        {
                            statisticTemplate = "Return HOME, leaving prev stop at {8} and arriving at {9} ({5} miles).";
                        }
                    }

                    var line = string.Format(
                        statisticTemplate,
                        planDriver.Driver.StartingLocation.DisplayName.ToUpper(),       //0
                        routeSegmentStatistics[0].StartTime.ToString(@"hh\:mm"),        //1
                        rsm.EndStop.Location.DisplayName,                               //2
                        rsm.EndStop.StopAction.Name,                                    //3
                        FormatSegmentTimeForUIDisplay(rsm.Statistics.TotalIdleTime),    //4
                        Math.Round(rsm.Statistics.TotalTravelDistance, 2),              //5
                        FormatSegmentTimeForUIDisplay(rsm.Statistics.TotalTravelTime),  //6
                        FormatSegmentTimeForUIDisplay(rsm.Statistics.TotalQueueTime),   //7
                        previousDepartTimeString,                                       //8
                        arrivalTimeString);                                             //9
                    sequence.Add(line);
                }

                result[planDriver.Id] = sequence;
            }
            
            return result;
        }
    }
}
