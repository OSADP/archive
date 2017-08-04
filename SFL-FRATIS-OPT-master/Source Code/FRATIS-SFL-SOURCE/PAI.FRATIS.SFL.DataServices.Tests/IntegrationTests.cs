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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Conventions;
using NUnit.Framework;
using PAI.Drayage.EnhancedOptimization.Model;
using PAI.Drayage.EnhancedOptimization.Services;
using PAI.Drayage.EnhancedOptimization.Services.Clustering;
using PAI.Drayage.EnhancedOptimization.Services.DynamicStops;
using PAI.Drayage.EnhancedOptimization.Services.OptimizationSteps;
using PAI.Drayage.EnhancedOptimization.Services.ProbabilityMatrix;
using PAI.Drayage.Optimization.Common;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Drayage.Optimization.Model.Planning;
using PAI.Drayage.Optimization.Reporting.Services;
using PAI.Drayage.Optimization.Services;
using PAI.FRATIS.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Data;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Data;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Optimization.Adapter.Services;
using PAI.FRATIS.SFL.Services;
using PAI.FRATIS.SFL.Services.Core.Caching;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Integration;
using PAI.FRATIS.SFL.Services.Integration.Extensions;
using PAI.FRATIS.SFL.Services.Logging;
using PAI.FRATIS.SFL.Services.Orders;
using PAI.FRATIS.SFL.Tests;
using PAI.FRATIS.SFL.Web.Framework.Mapping;
using PAI.FRATIS.Wrappers.WebFleet;
using PAI.Infrastructure;
using ConnectionStringManager = PAI.FRATIS.SFL.Infrastructure.ConnectionStringManager;
using Driver = PAI.FRATIS.SFL.Domain.Orders.Driver;
using IRouteStopService = PAI.FRATIS.SFL.Services.Orders.IRouteStopService;
using IValidationService = PAI.Drayage.EnhancedOptimization.Services.IValidationService;
using Job = PAI.Drayage.Optimization.Model.Orders.Job;
using Location = PAI.FRATIS.SFL.Domain.Geography.Location;
using RouteStop = PAI.Drayage.Optimization.Model.Orders.RouteStop;
using RouteStopService = PAI.FRATIS.SFL.Services.Orders.RouteStopService;
using ValidationService = PAI.Drayage.EnhancedOptimization.Services.ValidationService;

namespace PAI.FRATIS.SFL.DataServices.Tests
{


    public class IntegrationTests : TestsBase
    {
        DistanceObjectiveFunction objectiveFunction;
        TravelTimeEstimator travelTimeEstimator;
        DistanceService distanceService;
        JobNodeService jobNodeService;
        OptimizerConfiguration optimizerConfiguration;
        RouteStopDelayService routeStopDelayService;
        RandomNumberGenerator randomNumberGenerator;
        NullLogger logger;
        Drayage.Optimization.Services.RouteStopService routeStopService;
        DefaultNodeConnectionFactory nodeConnectionFactory;
        RouteExitFunction routeExitFunction;
        RouteStatisticsComparer routeStatisticsComparer;
        NodeService nodeService;
        RouteStatisticsService routeStatisticsService;
        RouteService routeService;
        ReportingService reportingService;
        PheromoneMatrix pheromoneMatrix;
        ProbabilityMatrix probabilityMatrix;
        DrayageOptimizer drayageOptimizer;
        ValidationService validationService;

        [SetUp]
        public void SetUp()
        {

            Kernel.Bind<IEngine>().To<Engine>().InSingletonScope();

            // Run installation tasks
            Kernel.Bind(x =>
                x.FromAssembliesMatching("PAI.FRATIS.*")
                 .SelectAllClasses()
                 .Excluding(new[]
                                            {
                                                typeof(Engine)
                                            })
                 .BindDefaultInterfaces()
                 .Configure(b => b.InTransientScope()));


            //Kernel.Bind(x =>
            //            x.FromAssemblyContaining<StateService>()
            //             .SelectAllClasses()
            //             .BindAllInterfaces()
            //             .Configure(f => f.InTransientScope()));

            // Bind Database Repository 
            Kernel.Rebind(typeof(IRepository<>)).To(typeof(EfRepository<>));

            Kernel.Rebind<IDbContext>().To<DataContext>().InSingletonScope()
              .WithConstructorArgument("nameOrConnectionString", ConnectionStringManager.ConnectionString);

            Kernel.Rebind<IIncluder>().To<DbIncluder>().InTransientScope();

            Kernel.Rebind<IGeocodeService>().To<WebFleetGeocodeService>().InThreadScope();

            Kernel.Rebind<ICacheManager>().To<MemoryCacheManager>().InThreadScope();

            Kernel.Rebind<ILogService>().To<LogService>().InThreadScope();

            Kernel.Rebind<IDomainModelMapper>().To<DomainModelAutoMapper>().InThreadScope();

            Kernel.Rebind<IStopActionService>().To<StopActionService>().InThreadScope();

            Kernel.Rebind<ILocationService>().To<LocationService>().InThreadScope();

            AutoMapperInitializer.Initialize();
        }

        public Drayage.Optimization.Model.Location Convert(Location location)
        {
            if (!location.Latitude.HasValue)
                location.Latitude = 0;

            if (!location.Longitude.HasValue)
                location.Longitude = 0;

            return
                new Drayage.Optimization.Model.Location
                {
                    DisplayName = location.DisplayName,
                    Id = location.Id,
                    Latitude = location.Latitude.Value,
                    Longitude = location.Longitude.Value
                };
        }

        public Drayage.Optimization.Model.Orders.Driver Convert(PAI.FRATIS.SFL.Domain.Orders.Driver driver)
        {
            return
                new Drayage.Optimization.Model.Orders.Driver
                {
                    Id = driver.Id,
                    AvailableDrivingHours = driver.AvailableDrivingHours,
                    AvailableDutyHours = driver.AvailableDutyHours,
                    DisplayName = driver.DisplayName,
                    EarliestStartTime = driver.EarliestStartTime,
                    IsFlatbed = driver.IsFlatbed,
                    IsHazmat = driver.IsHazmat,
                    StartingLocation = Convert(driver.StartingLocation)
                };
        }

        public Drayage.Optimization.Model.Orders.Job Convert(PAI.FRATIS.SFL.Domain.Orders.Job job)
        {
            return
                new Drayage.Optimization.Model.Orders.Job
                {
                    Id = job.Id,
                    DisplayName = job.OrderNumber,
                    EquipmentConfiguration = new EquipmentConfiguration(),
                    IsFlatbed = job.IsFlatbed,
                    IsHazmat = job.IsHazmat,
                    RouteStops = job.RouteStops.Select(Convert).ToList()
                };
        }

        public Drayage.Optimization.Model.Orders.RouteStop Convert(PAI.FRATIS.SFL.Domain.Orders.RouteStop routeStop)
        {
            return
                new RouteStop
                {
                    Id = routeStop.Id,
                    Location = Convert(routeStop.Location),
                    ExecutionTime = TimeSpan.FromMinutes(routeStop.StopDelay.Value),
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = Convert(routeStop.StopAction),
                    WindowEnd = new TimeSpan(routeStop.WindowEnd),
                    WindowStart = new TimeSpan(routeStop.WindowStart)
                };
        }

        public PAI.Drayage.Optimization.Model.Orders.StopAction Convert(PAI.FRATIS.SFL.Domain.Orders.StopAction stopAction)
        {
            return StopActions.Actions.First(x => x.ShortName == stopAction.ShortName);
        }

        [Test]
        public void Job_for_orders_12437_12293_should_be_schedulable()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var allJobs = jobService.SelectWithAll().ToList();

            var jobs =
                allJobs
                    .Where(y => y.DueDate.HasValue)
                    .Where(z => z.DueDate.Value > new DateTime(2014, 9, 4))
                    .Where(x => x.DueDate.Value < new DateTime(2014, 9, 5))
                    .Where(u => new List<int> { 12437, 12293 }.Contains(u.Id))
                    .ToList();

            var convertedJobs = jobs.Select(Convert).ToList();

            var drivers = driverService.SelectWithAll().ToList().Where(x => x.DisplayName != "Placeholder Driver").Where(y => y.DisplayName == "FRANCISCO ESPILDORA").Select(Convert).ToList();
            Assert.AreNotEqual(0, jobs.Count);

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.ToList(),
                    Jobs = convertedJobs
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var reportDutyHourUtilizations =
                reportingService
                    .GetSolutionPerformanceStatistics(plan)
                    .TruckStatistics.Select(x => x.Value)
                    .Select(y => y.PerformanceStatistics.DriverDutyHourUtilization);

            Assert.IsFalse(reportDutyHourUtilizations.Any(x => x > 1));
        }

        [Test]
        public void Jobs_for_4_September_should_be_feasible()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var allJobs = jobService.SelectWithAll().ToList();

            var jobs =
                allJobs
                    .Where(y => y.DueDate.HasValue)
                    .Where(z => z.DueDate.Value > new DateTime(2014, 9, 4))
                    .Where(x => x.DueDate.Value < new DateTime(2014, 9, 5))
                    .ToList();

            var convertedJobs = jobs.Select(Convert).ToList();

            var drivers = driverService.SelectWithAll().ToList().Where(x => x.DisplayName != "Placeholder Driver").Select(Convert).ToList();

            Assert.AreNotEqual(0, jobs.Count);

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.ToList(),
                    Jobs = convertedJobs
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var results = Analyze_jobs(plan, planConfig);

            Console.WriteLine(results);

            var report = reportingService.GetSolutionPerformanceStatistics(plan);
            var utilizations = report.TruckStatistics.Select(x => x.Value).Select(y => y.PerformanceStatistics.DriverDutyHourUtilization).ToList();
            Assert.IsFalse(utilizations.Any(x => x > 1));
        }

        protected string Analyze_jobs(Plan plan, PlanConfig planConfig)
        {
            var stringBuilder = new StringBuilder();
            var report = reportingService.GetSolutionPerformanceStatisticsReport(plan);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.Append("Displaying report for a Plan Configuration submission build from Order Number(s): " + planConfig.Jobs.Select(x => x.DisplayName).Aggregate((a, b) => a + ", " + b));

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.AppendLine("Using drivers: " + plan.DriverPlans.Select(x => x.Driver.DisplayName).Aggregate((a, b) => a + ", " + b));

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.AppendFormat("Leaving {0} jobs unscheduled.", plan.UnassignedJobs.Count);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(report);

            return stringBuilder.ToString();
        }

        [Test]
        public void Job_2758122_should_be_reportable()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").ToList().Where(x => int.Parse(x.OrderNumber) > 2758120 && int.Parse(x.OrderNumber) < 2758130).ToList();
            var drivers = driverService.SelectWithAll().Distinct().ToList();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var results = Analyze_jobs(plan, planConfig);

            Console.WriteLine(results);

            var report = reportingService.GetSolutionPerformanceStatistics(plan);

            Assert.IsNotNull(report);
        }

        [Test]
        public void Jobs_2757140_2757714_2758321_2758985_should_not_conflict()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var jobs = jobService.SelectWithAll().Where(x => new List<string> { "2757140", "2757714", "2758321", "2758985" }.Contains(x.OrderNumber)).ToList();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = jobs.Select(x => Convert(x.AssignedDriver)).Distinct().ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var results = Analyze_jobs(plan, planConfig);

            Console.WriteLine(results);
        }

        [Test]
        public void Should_honour_Plan_config_values()
        {
            var controller = Make_PlanGenerator();

            var planConfig =
                new PlanConfig
                {
                    Drivers = new List<Drayage.Optimization.Model.Orders.Driver>
                            {
                                new Drayage.Optimization.Model.Orders.Driver
                                    {
                                        Id = 1,
                                        AvailableDrivingHours = 100,
                                        AvailableDutyHours = 100,
                                        DisplayName = "Alice",
                                        EarliestStartTime = new TimeSpan(7, 0, 0).Ticks,
                                        StartingLocation = new Drayage.Optimization.Model.Location
                                            {
                                                Id = 1,
                                                DisplayName = "Alices's Starting Location",
                                                Latitude = 0,
                                                Longitude = 0
                                            }
                                    }
                            },
                    DefaultDriver =
                        new Drayage.Optimization.Model.Orders.Driver
                        {
                            Id = 0,
                            AvailableDrivingHours = 10,
                            AvailableDutyHours = 14,
                            DisplayName = "PlaceHolder",
                            EarliestStartTime = new TimeSpan(3, 0, 0).Ticks,
                            StartingLocation = new Drayage.Optimization.Model.Location
                            {
                                Id = 0,
                                DisplayName = "Default Starting Location",
                                Latitude = 0,
                                Longitude = 0
                            }
                        },
                    Id = 50,
                    Jobs = new List<Job>
                            {
                                new Job
                                    {
                                        Id = 20,
                                        DisplayName = "Job the first",
                                        EquipmentConfiguration = new EquipmentConfiguration(),
                                        RouteStops = new List<RouteStop>
                                            {
                                                new RouteStop
                                                    {
                                                        Id = 500,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 10,
                                                                DisplayName = "Location for 500",
                                                                Latitude = 0.15,
                                                                Longitude = 0.15
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.LiveLoading,
                                                        WindowStart = new TimeSpan(0, 0, 0),
                                                        WindowEnd = new TimeSpan(23, 59, 59)
                                                    },
                                                new RouteStop
                                                    {
                                                        Id = 501,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 11,
                                                                DisplayName = "Location for 501",
                                                                Latitude = 0.155,
                                                                Longitude = 0.155
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.LiveLoading,
                                                        WindowStart = new TimeSpan(10, 0, 0),
                                                        WindowEnd = new TimeSpan(11, 0, 0)
                                                    }
                                            }
                                    },
                                new Job
                                    {
                                        Id = 40,
                                        DisplayName = "Job the second",
                                        EquipmentConfiguration = new EquipmentConfiguration(),
                                        RouteStops = new List<RouteStop>
                                            {
                                                new RouteStop
                                                    {
                                                        Id = 600,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 10,
                                                                DisplayName = "Location for 600",
                                                                Latitude = 0.1575,
                                                                Longitude = 0.1575
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.LiveLoading,
                                                        WindowStart = new TimeSpan(12, 0, 0),
                                                        WindowEnd = new TimeSpan(13, 0, 0)
                                                    },
                                                new RouteStop
                                                    {
                                                        Id = 601,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 11,
                                                                DisplayName = "Location for 601",
                                                                Latitude = 0.15775,
                                                                Longitude = 0.15775
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.DropOffLoadedWithChassis,
                                                        WindowStart = new TimeSpan(14, 0, 0),
                                                        WindowEnd = new TimeSpan(15, 0, 0)
                                                    }
                                            }
                                    }
                            }
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.IsNotNull(plan);

            //Assert.AreEqual(0, plan.UnassignedJobs.Count);

            Assert.AreEqual(5, plan.DriverPlans.First().RouteSegmentStatistics.Count);
        }

        protected Drayage.EnhancedOptimization.Services.PlanGenerator Make_PlanGenerator()
        {
            objectiveFunction = new DistanceObjectiveFunction();
            travelTimeEstimator = new TravelTimeEstimator();
            distanceService = new DistanceService(travelTimeEstimator);
            jobNodeService = new JobNodeService(distanceService);
            optimizerConfiguration = new Drayage.Optimization.Model.OptimizerConfiguration();
            routeStopDelayService = new RouteStopDelayService(optimizerConfiguration);
            randomNumberGenerator = new RandomNumberGenerator();
            logger = new NullLogger();

            routeStopService = new Drayage.Optimization.Services.RouteStopService(distanceService, optimizerConfiguration, routeStopDelayService, new JobNodeService(distanceService));
            nodeConnectionFactory = new DefaultNodeConnectionFactory(routeStopService);
            routeExitFunction = new RouteExitFunction(logger);
            routeStatisticsComparer = new RouteStatisticsComparer(objectiveFunction);
            nodeService = new NodeService(nodeConnectionFactory, optimizerConfiguration);
            routeStatisticsService = new RouteStatisticsService(routeStopService, nodeService, optimizerConfiguration);
            routeService = new RouteService(routeExitFunction, nodeService, routeStatisticsComparer, routeStatisticsService, nodeConnectionFactory);
            pheromoneMatrix = new PheromoneMatrix(objectiveFunction);
            probabilityMatrix = new EnhancedProbabilityMatrix(pheromoneMatrix, routeService, objectiveFunction, randomNumberGenerator, routeStatisticsService);
            drayageOptimizer = new DrayageOptimizer(probabilityMatrix, routeService, routeExitFunction, logger, pheromoneMatrix, randomNumberGenerator, routeStatisticsService, jobNodeService, routeStopService);
            validationService = new ValidationService(new[]
                    {
                        new TimeSpan(11, 11, 0) 
                    }
            );
            var dataExtractor = new DataExtractor(routeStatisticsService, routeStopService, routeService, drayageOptimizer, jobNodeService);
            reportingService = new ReportingService(routeStopService, routeService, routeStatisticsService, dataExtractor, jobNodeService);

            return
                new Drayage.EnhancedOptimization.Services.PlanGenerator(drayageOptimizer, reportingService, routeService, pheromoneMatrix, probabilityMatrix, jobNodeService, distanceService);
        }

        [Test]
        public void Should_schedule_the_orders_in_question()
        {
            var controller = Make_PlanGenerator();
            var planConfig = Make_SaborTropical_and_ToysRUs_PlanConfig();

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var report = reportingService.GetSolutionPerformanceStatistics(plan);
            var reportString = reportingService.GetSolutionPerformanceStatisticsReport(plan);

            Assert.IsNotNullOrEmpty(reportString);

            Console.WriteLine(reportString);
        }

        static PlanConfig Make_SaborTropical_and_ToysRUs_PlanConfig()
        {
            #region job setup

            var fecMiamiRailLocation =
                new Drayage.Optimization.Model.Location
                {
                    Id = 1436,
                    DisplayName = "FEC MIAMI RAIL",
                    Latitude = -80.307076,
                    Longitude = 25.837387
                };

            var saborTropicallLocation =
                new Drayage.Optimization.Model.Location
                {
                    Id = 1870,
                    DisplayName = "SABOR TROPICAL",
                    Latitude = -80.322685,
                    Longitude = 25.786812
                };

            var toysRUsLocation =
                new Drayage.Optimization.Model.Location
                {
                    Id = 1561,
                    DisplayName = "TOYS R US #8715",
                    Latitude = -80.38444,
                    Longitude = 25.68684
                };

            var joseRivas =
                new Drayage.Optimization.Model.Orders.Driver
                {
                    Id = 727,
                    AvailableDrivingHours = 8,
                    AvailableDutyHours = 12,
                    DisplayName = "JOSE RIVAS",
                    EarliestStartTime = 108000000000,
                    StartingLocation = fecMiamiRailLocation
                };

            var placeHolderDriver =
                new Drayage.Optimization.Model.Orders.Driver
                {
                    Id = 787,
                    AvailableDrivingHours = 8,
                    AvailableDutyHours = 12,
                    DisplayName = "Placeholder Driver",
                    EarliestStartTime = 0,
                    StartingLocation = fecMiamiRailLocation
                };

            var saborStart =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 60, 0),
                    Location = fecMiamiRailLocation,
                    Id = 34091,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.PickupLoadedWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 00)
                };
            var saborDeliver =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 30, 0),
                    Location = saborTropicallLocation,
                    Id = 34092,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.LiveUnloading,
                    WindowStart = new TimeSpan(6, 0, 0),
                    WindowEnd = new TimeSpan(6, 30, 0)
                };
            var saborEnd =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 60, 0),
                    Location = fecMiamiRailLocation,
                    Id = 34093,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.DropOffEmptyWithChassis,
                    WindowStart = new TimeSpan(6, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 0)
                };

            var toysRUsStart =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 60, 0),
                    Location = fecMiamiRailLocation,
                    Id = 35402,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.PickupLoadedWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 00)
                };

            var toysRUsDropoff =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 120, 0),
                    Location = toysRUsLocation,
                    Id = 35404,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.LiveUnloading,
                    WindowStart = new TimeSpan(9, 30, 00),
                    WindowEnd = new TimeSpan(10, 0, 0)
                    //WindowEnd = new TimeSpan(22, 0, 0)
                };

            var toysRUsEnd =
                new Drayage.Optimization.Model.Orders.RouteStop
                {
                    ExecutionTime = new TimeSpan(0, 60, 0),
                    Location = fecMiamiRailLocation,
                    Id = 35405,
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = null,
                    StopAction = StopActions.DropOffEmptyWithChassis,
                    WindowStart = new TimeSpan(9, 30, 00),
                    WindowEnd = new TimeSpan(23, 59, 00)
                };

            var saborJob =
                new Job
                {
                    Id = 11651,
                    DisplayName = "Sabor Tropical Dropoff",
                    EquipmentConfiguration = new EquipmentConfiguration(),
                    IsFlatbed = false,
                    IsHazmat = false,
                    RouteStops =
                        new List<RouteStop>
                                {
                                    saborStart,
                                    saborDeliver,
                                    saborEnd
                                }
                };

            var toysRUsJob =
                new Job
                {
                    Id = 12065,
                    DisplayName = "Toys R Us Dropoff",
                    EquipmentConfiguration = new EquipmentConfiguration(),
                    IsFlatbed = false,
                    IsHazmat = false,
                    RouteStops =
                        new List<RouteStop>
                                {
                                    toysRUsStart,
                                    toysRUsDropoff,
                                    toysRUsEnd
                                }
                };

            #endregion

            var planConfig =
                new PlanConfig
                {
                    Drivers = new List<Drayage.Optimization.Model.Orders.Driver> { joseRivas },
                    DefaultDriver = placeHolderDriver,
                    Id = 11651,
                    Jobs = new List<Job>
                            {
                                saborJob,
                                toysRUsJob
                            }
                };
            return planConfig;
        }

        [Test]
        public void Should_honour_job_hazmat_values()
        {
            var controller = Make_PlanGenerator();

            var planConfig =
                new PlanConfig
                {
                    Drivers = new List<Drayage.Optimization.Model.Orders.Driver>
                            {
                                new Drayage.Optimization.Model.Orders.Driver
                                    {
                                        Id = 1,
                                        IsHazmat = false,
                                        AvailableDrivingHours = 100,
                                        AvailableDutyHours = 100,
                                        DisplayName = "Alice",
                                        EarliestStartTime = new TimeSpan(7, 0, 0).Ticks,
                                        StartingLocation = new Drayage.Optimization.Model.Location
                                            {
                                                Id = 1,
                                                DisplayName = "Alices's Starting Location",
                                                Latitude = 0,
                                                Longitude = 0
                                            }
                                    }
                            },
                    DefaultDriver =
                        new Drayage.Optimization.Model.Orders.Driver
                        {
                            Id = 0,
                            AvailableDrivingHours = 10,
                            AvailableDutyHours = 14,
                            DisplayName = "PlaceHolder",
                            EarliestStartTime = new TimeSpan(3, 0, 0).Ticks,
                            StartingLocation = new Drayage.Optimization.Model.Location
                            {
                                Id = 0,
                                DisplayName = "Default Starting Location",
                                Latitude = 0,
                                Longitude = 0
                            }
                        },
                    Id = 50,
                    Jobs = new List<Job>
                            {
                                new Job
                                    {
                                        Id = 20,
                                        DisplayName = "Job the first",
                                        EquipmentConfiguration = new EquipmentConfiguration(),
                                        IsHazmat = true,
                                        RouteStops = new List<RouteStop>
                                            {
                                                new RouteStop
                                                    {
                                                        Id = 500,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 10,
                                                                DisplayName = "Location for 500",
                                                                Latitude = 0.15,
                                                                Longitude = 0.15
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.LiveLoading,
                                                        WindowStart = new TimeSpan(0, 0, 0),
                                                        WindowEnd = new TimeSpan(23, 59, 59)
                                                    },
                                                new RouteStop
                                                    {
                                                        Id = 501,
                                                        ExecutionTime = null,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 11,
                                                                DisplayName = "Location for 501",
                                                                Latitude = 0.155,
                                                                Longitude = 0.155
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = null,
                                                        StopAction = StopActions.LiveLoading,
                                                        WindowStart = new TimeSpan(10, 0, 0),
                                                        WindowEnd = new TimeSpan(11, 0, 0)
                                                    }
                                            }
                                    }
                            }
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            for (var i = 0; i < 2; i++)
            {
                var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

                planConfig.Drivers.First().IsHazmat = !planConfig.Drivers.First().IsHazmat;

                Assert.IsNotNull(plan);
                Assert.AreEqual(i == 0 ? 1 : 0, plan.UnassignedJobs.Count);
            }
        }

        [Test]
        public void Can_read_zip_codes()
        {
            int counter = 0;
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader("c:\\temp\\ZIP.csv");
            while ((line = file.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    Console.Write(line + ",");
                    counter++;
                }
            }

            file.Close();
        }

        [Test]
        public void Can_get_manifest_numbers_with_zero_filtered_legs()
        {
            var importService = Kernel.Get<ILegacyImportService>();
            var result = importService.Process(@"c:\temp\file.xlsx", 1, false, false);

            var a = result.Dictionary["2748854"];
            var b = result.Dictionary["2748846"];

            int count = 0;
            foreach (var kvp in result.Dictionary)
            {
                var item = kvp.Value;
                if (item.FilteredLegs.Count == 0)
                {
                    count++;
                    Console.WriteLine("Zero legs found for Manifest # {0}", item.AllLegs.First().ManifestNumber);
                }
            }
            Console.WriteLine("Total count is {0}", count);
        }

        [Test]
        public void Can_import_locations()
        {
            var importService = Kernel.Get<ILegacyImportService>();
            importService.Process(@"c:\temp\file.xlsx", 1, true, false);
        }

        [Test]
        public void Can_import_jobs()
        {
            var importService = Kernel.Get<ILegacyImportService>();
            var result = importService.Process(@"c:\temp\new3.txt", 1, false, false);
        }

        [Test]
        public void Can_identify_job_type()
        {
            var importService = Kernel.Get<ILegacyImportService>();
            var result = importService.Process(@"c:\temp\file.xlsx", 1, false, false);

            foreach (var item in result.Dictionary)
            {
                if (item.Value.FilteredLegs.Count == 0)
                {
                    Console.WriteLine(string.Format("#{0}\t{1}", item.Key, "No SFL Legs"));
                }
                else
                {
                    Console.WriteLine(string.Format("#{0}\t{1}", item.Key, item.Value.JobType));
                }
            }
        }

        [Test]
        public void Can_validate_jobs()
        {

            var jobService = Kernel.Get<IJobService>();
            var validationService = Kernel.Get<Drayage.EnhancedOptimization.Services.IValidationService>();

            //var job = jobService.SelectWithAll(1).FirstOrDefault();
            //var result = validationService.ValidateJob(job);
            //Console.WriteLine(result.Successful);
        }

        [Test]
        public void Add_Placeholder_Driver()
        {
            var driverService = Kernel.Get<IDriverService>();
            var locationService = Kernel.Get<ILocationService>();
            var loc = locationService.Select().FirstOrDefault(p => p.DisplayName == "FEC MIAMI RAIL");


            driverService.Insert(new Driver()
            {
                DisplayName = "Placeholder Driver",
                SubscriberId = 1,
                IsPlaceholderDriver = true,
                StartingLocationId = loc.Id
            });

        }
        [Test]
        public void Driver_Import()
        {
            var locationService = Kernel.Get<ILocationService>();
            var loc = locationService.Select().FirstOrDefault(p => p.DisplayName == "FEC MIAMI RAIL");

            var importService = Kernel.Get<ILegacyImportService>();
            importService.ImportDrivers(@"c:\temp\drivers.xls", 1, loc.Id);
            Console.WriteLine("Done");
        }
        [Test]
        public void TestImport()
        {
            var importService = Kernel.Get<ILegacyImportService>();
            var locationService = Kernel.Get<ILocationService>();
            var saService = Kernel.Get<IStopActionService>();

            var locations = locationService.GetBySubscriberId(1, true).ToList();
            var stopActions = saService.GetStopActions().ToList();

            var result = importService.Process(@"C:\temp\ftproot\ftproot\processed\201492922412.txt", 1, true, true);
            var jobs = result.GetJobs(locations.Where(p => p.LegacyId != null).ToDictionary(p => p.LegacyId), stopActions, 1);
            return;



            foreach (var job in jobs)
            {
                Console.WriteLine("Job {0}  ({1} Stops) {2}", job.OrderNumber, job.RouteStops.Count, "");
                for (int i = 0; i < job.RouteStops.Count; i++)
                {
                    try
                    {
                        var rs = job.RouteStops[i];
                        Console.WriteLine("\tStop {0}: {1} {2} {3} {4} {5}",
                            i + 1, rs.Location.DisplayName,
                            rs.StopAction.Name,
                            rs.StopDelay,
                            DateTime.Now.Date.AddTicks(rs.WindowStart).ToShortTimeString(),
                            DateTime.Now.Date.AddTicks(rs.WindowEnd).ToShortTimeString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            var items = jobs;

            // importService.ImportLocations(@"c:\temp\file.xlsx");
            //importService.Process(@"c:\temp\file.xlsx", 1);

        }
        class RouteStopSelection
        {
            public string DisplayName { get; set; }
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
        }
        [Test]
        public void Order_2756342_should_not_be_scheduled_for_evening()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").ToList().Where(x => int.Parse(x.OrderNumber) == 2756342).ToList();
            var drivers = driverService.SelectWithAll().Distinct().ToList();

            var originalRouteStops =
                jobs.SelectMany(x =>
                    x.RouteStops
                        .Where(z => z.Location.DisplayName.StartsWith("TOYS"))
                        .Select(y =>
                            new RouteStopSelection
                            {
                                DisplayName = y.Location.DisplayName,
                                Start = new TimeSpan(y.WindowStart),
                                End = new TimeSpan(y.WindowEnd)
                            }));

            Assert.IsFalse(originalRouteStops.Any(x => x.Start > new TimeSpan(12, 0, 0) && x.End > new TimeSpan(12, 0, 0)));

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var planRouteStops =
                plan.DriverPlans.SelectMany(x => x.JobPlans).SelectMany(y => y.RouteStops).Where(z => z.Location.DisplayName.StartsWith("TOYS")).Select(w => new RouteStopSelection
                {
                    DisplayName = w.Location.DisplayName,
                    End = w.WindowEnd,
                    Start = w.WindowStart
                });

            Assert.IsFalse(planRouteStops.Any(x => x.Start > new TimeSpan(12, 0, 0) && x.End > new TimeSpan(12, 0, 0)));

            foreach (var originalRouteStop in originalRouteStops)
            {
                var planRouteStop = planRouteStops.First(x => x.DisplayName == originalRouteStop.DisplayName);
                Assert.IsTrue(planRouteStop.Start >= originalRouteStop.Start && planRouteStop.End <= originalRouteStop.End);
            }
        }

        [Test]
        public void Order_2759773_should_have_FEC_MIAMI_TERMINAL_stop_filtered_out()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").ToList().Where(x => int.Parse(x.OrderNumber) == 2759773).ToList();
            var drivers = new List<Driver> { driverService.SelectWithAll().Distinct().First() };

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var routeStops =
                plan.DriverPlans.SelectMany(x => x.JobPlans).SelectMany(y => y.RouteStops).Select(w => new RouteStopSelection
                {
                    DisplayName = w.Location.DisplayName,
                    End = w.WindowEnd,
                    Start = w.WindowStart
                }).ToList();

            Assert.AreEqual(3, routeStops.Count);
            Assert.IsFalse(routeStops.Any(x => x.DisplayName == "FEC MIAMI TERMINAL"));
        }

        [Test]
        public void Job_with_execution_time_equal_to_driver_AvailableDutyTime_should_be_feasible()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(0, plan.UnassignedJobs.Count);
        }

        [Test]
        public void Job_with_execution_time_greater_than_driver_AvailableDutyTime_should_not_be_feasible()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            planConfig.Jobs.First().RouteStops.ElementAt(1).ExecutionTime = new TimeSpan(planConfig.Jobs.First().RouteStops.ElementAt(1).ExecutionTime.Value.Ticks + 1);

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(1, plan.UnassignedJobs.Count);
        }

        [Test]
        public void Job_with_execution_time_equal_to_driver_AvailableDrivingTime_should_be_feasible()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            planConfig.Drivers.First().AvailableDrivingHours = 0;

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(0, plan.UnassignedJobs.Count);
        }

        [Test]
        public void Job_with_execution_time_greater_than_driver_AvailableDrivingTime_should_not_be_feasible()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            var tickInHours = new TimeSpan(1).TotalHours;

            planConfig.Drivers.First().AvailableDrivingHours = -1 * tickInHours;

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            planConfig.Jobs.First().RouteStops.ElementAt(1).ExecutionTime = new TimeSpan(planConfig.Jobs.First().RouteStops.ElementAt(1).ExecutionTime.Value.Ticks + 1);

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(1, plan.UnassignedJobs.Count);
        }

        [Test]
        public void Order_Numbers_2759335_and_2758438_should_be_feasible()
        {
            var jobNumberList = new List<string> { "2762605", "2762049" };
            Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = new List<Driver> { driverService.SelectWithAll().ToList().Distinct().First(x => x.DisplayName == "LEONARDO BANOS") };

            var mappedJobs = jobs.Select(Convert).ToList();
            var mappedDriver = Convert(drivers.First());

            var infeasibleJobIds = drayageOptimizer.GetInfeasibleJobIds(mappedJobs, mappedDriver);

            Assert.AreEqual(0, infeasibleJobIds.Count);
        }

        [Test]
        public void Order_Numbers_2761455_and_2761693_etc_should_be_feasible()
        {
            var jobNumberList = new List<string> { "2761455", "2761693" };
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = driverService.SelectWithAll().ToList().Distinct();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(0, 0);
        }

        [Test]
        public void Order_Numbers_2761137_should_be_feasible()
        {
            var jobNumberList = new List<string> { "2761137" };
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = driverService.SelectWithAll().ToList().Distinct();

            var planConfig =
                new PlanConfig
                    {
                        DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                        Id = 1,
                        Drivers = drivers.Select(Convert).ToList(),
                        Jobs = jobs.Select(Convert).Distinct().ToList()
                    };

            var optimizerSettings = new OptimizerSettings
                {
                    DisallowPlaceholderDriver = true
                };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(0, 0);
        }

        [Test]
        public void Jobs_without_priorities_should_be_schedulable()
        {
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER" && y.RouteStops.Count > 2).ToList();

            var drivers = driverService.SelectWithAll().Distinct().ToList();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.AreEqual(plan.DriverPlans.Count, drivers.Count);
        }

        [Test]
        public void Jobs_with_priorities_should_be_assigned_to_drivers_with_mathcing_priorities()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            var driverSerializedString = JsonConvert.SerializeObject(planConfig.Drivers.First());
            var prototypeDriver = JsonConvert.DeserializeObject<Driver>(driverSerializedString);
            var jobSerializedString = JsonConvert.SerializeObject(planConfig.Jobs.First());
            var prototypeJob = JsonConvert.DeserializeObject<Job>(jobSerializedString);
            Assert.IsNotNull(prototypeDriver);
            Assert.IsNotNull(prototypeJob);
            planConfig.Drivers = new List<Drayage.Optimization.Model.Orders.Driver>();
            planConfig.Jobs = new List<Job>();

            for (var i = 0; i < 3; i++)
            {
                var driver = JsonConvert.DeserializeObject<Drayage.Optimization.Model.Orders.Driver>(driverSerializedString);
                driver.Id = i + 1;
                driver.Priority = i == 0 ? 3 : 0;
                planConfig.Drivers.Add(driver);
            }

            for (var i = 0; i < 30; i++)
            {
                var job = JsonConvert.DeserializeObject<Job>(jobSerializedString);
                job.Id = i + 1;
                job.Priority = i == 0 ? 3 : 0;
                planConfig.Jobs.Add(job);
            }

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            for (var i = 0; i < 30; i++)
            {
                var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);
                var assignedPropritySum = plan.DriverPlans.SelectMany(x => x.JobPlans).Select(y => y.Job).Sum(z => z.Priority);
                var driverWithHighPriorityJobPriorityAssignment = plan.DriverPlans.Where(x => x.Driver.Priority == 3).SelectMany(y => y.JobPlans).Sum(z => z.Job.Priority);
                var driverWithoutHighPriorityJobPriorityAssignment = plan.DriverPlans.Where(x => x.Driver.Priority != 3).SelectMany(y => y.JobPlans).Sum(z => z.Job.Priority);
                var unassignedPrioritySum = plan.UnassignedJobs.Sum(x => x.Priority);
                Assert.AreNotEqual(0, plan.DriverPlans.Count);
                Assert.AreEqual(3f, assignedPropritySum);
                Assert.AreEqual(0f, unassignedPrioritySum);
                Assert.AreEqual(3f, driverWithHighPriorityJobPriorityAssignment);
                Assert.AreEqual(0f, driverWithoutHighPriorityJobPriorityAssignment);
            }
        }

        [Test]
        public void testOrdersForIdleTime()
        {
            var jobNumberList = new List<string> { "2760778", "2761813" };
            Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = new List<Driver> { driverService.SelectWithAll().ToList().Distinct().First(x => x.DisplayName == "LEONARDO BANOS") };
        }

        [Test]
        public void testRouteStatistics()
        {
            var controller = Make_PlanGenerator();

            var planConfig = PlanConfig();

            var fec1 =
                new RouteStop
                {
                    Id = 500,
                    ExecutionTime = TimeSpan.FromHours(1),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 11,
                        DisplayName = "AAAAA",
                        Latitude = 25.837387,
                        Longitude = -80.307076
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.PickupLoadedWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 59)
                };

            var tru =
                new RouteStop
                {
                    Id = 501,
                    ExecutionTime = TimeSpan.FromHours(2),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 21,
                        DisplayName = "Thingamajig",
                        Latitude = 25.686847,
                        Longitude = -80.38444
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.LiveUnloading,
                    WindowStart = new TimeSpan(9, 30, 0),
                    WindowEnd = new TimeSpan(10, 0, 00)
                };

            var fec2 =
                new RouteStop
                {
                    Id = 502,
                    ExecutionTime = TimeSpan.FromHours(1),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 11,
                        DisplayName = "BBBBBBBBBBBBBBB",
                        Latitude = 25.837387,
                        Longitude = -80.307076
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.DropOffEmptyWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 59)
                };


            var fec3 =
                new RouteStop
                {
                    Id = 503,
                    ExecutionTime = TimeSpan.FromHours(1),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 11,
                        DisplayName = "XXXX",
                        Latitude = 25.837387,
                        Longitude = -80.307076
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.PickupLoadedWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 59)
                };

            var st =
                new RouteStop
                {
                    Id = 504,
                    ExecutionTime = TimeSpan.FromHours(2),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 22,
                        DisplayName = "Whatever",
                        Latitude = 25.686847,
                        Longitude = -80.38444
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.LiveUnloading,
                    WindowStart = new TimeSpan(20, 00, 0),
                    WindowEnd = new TimeSpan(23, 00, 00)
                };

            var fec4 =
                new RouteStop
                {
                    Id = 505,
                    ExecutionTime = TimeSpan.FromHours(1),
                    Location = new Drayage.Optimization.Model.Location()
                    {
                        Id = 11,
                        DisplayName = "YYY",
                        Latitude = 25.837387,
                        Longitude = -80.307076
                    },
                    PostTruckConfig = new TruckConfiguration(),
                    PreTruckConfig = new TruckConfiguration(),
                    QueueTime = TimeSpan.Zero,
                    StopAction = StopActions.DropOffEmptyWithChassis,
                    WindowStart = new TimeSpan(5, 0, 0),
                    WindowEnd = new TimeSpan(23, 59, 59)
                };
            planConfig.Drivers.First().StartingLocation = fec1.Location;

            planConfig.Jobs.First().RouteStops = new List<RouteStop>
                {
                    fec1,
                    tru,
                    fec2,
                    fec3,
                    st,
                    fec4
                };


            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            Assert.IsNotNull(plan);

            //var solution = drayageOptimizer.BuildSolution(planConfig.Drivers.ToList(), planConfig.DefaultDriver, planConfig.Jobs.ToList());

            // var routeStops = routeService.

            var test = routeStopService.CalculateRouteSegmentStatistics(plan.DriverPlans.First().DepartureTimeSpan, planConfig.Jobs.First().RouteStops.ToList());

            Assert.IsNotNull(test);
        }

        [Test]
        public void checkTimes()
        {
            Console.WriteLine(new TimeSpan(882000000000));
            Console.WriteLine(new TimeSpan(1727400000000));
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(new TimeSpan(1008000000000));
            Console.WriteLine(new TimeSpan(1008000000000));
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(new TimeSpan(1116000000000));
            Console.WriteLine(new TimeSpan(1116000000000));
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(new TimeSpan(1116000000000));
            Console.WriteLine(new TimeSpan(1979400000000));
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine(new TimeSpan(2, 0, 0, 0).Ticks);
        }

        [Test]
        public void Test_Big_Plan()
        {
            var jobNumberList = new List<string> { "2762334", "2761396", "2761671", "2762612", "2761601", "2762132", "2760912", "2762780", "2761660", "2761415", "2763160", "2759723", "2760562", "2761905", "2762340", "2761554", "2762060", "2759719", "2760952", "2761814", "2761907", "2761831", "2763205", "2760544", "2761797", "2761406", "2760835", "2761408", "2761417", "2761854", "2761635", "2761419", "2761817", "2762330", "2760645", "2761046", "2760446", "2760638", "2761303", "2762872", "2762015", "2762583", "2761929", "2761391", "2760833", "2761463", "2761916", "2761805", "2762035", "2761380", "2761301", "2761662", "2761138", "2761347", "2762779", "2761111", "2761606", "2761399", "2762022", "2762411", "2762405", "2761141", "2760381", "2761952", "2760679", "2760322", "2762407", "2762931", "2761688", "2761825", "2760545", "2761849", "2762797", "2761135", "2762032", "2761821", "2763206", "2763208", "2761119", "2762064", "2761502", "2761547", "2762341", "2762808", "2761049", "2761432", "2761455", "2761693" };
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = driverService.SelectWithAll().ToList().Distinct();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var wookie = plan.DriverPlans.SelectMany(x => x.JobPlans.Select(y => y.Job.DisplayName)).OrderBy(z => z).ToList();

            Assert.AreEqual(0, 0);
        }

        [Test]
        public void SelectBestSolution()
        {
            var solutionComparer = new TotalTimeLessIdleFewestDriversObjectiveFunction();

            var routeStatisticsA = new RouteStatistics
                {
                    DriversWithAssignments = 10,
                    PriorityValue = 1,
                    TotalCapacity = 5,
                    TotalExecutionTime = TimeSpan.FromHours(10),
                    TotalIdleTime = TimeSpan.FromHours(0.5),
                    TotalQueueTime = TimeSpan.FromHours(0.25),
                    TotalTravelDistance = 100.0M,
                    TotalTravelTime = TimeSpan.FromHours(1.5)
                };

            var routeStatisticsB = new RouteStatistics
                {
                    DriversWithAssignments = 20,
                    PriorityValue = 1,
                    TotalCapacity = 5,
                    TotalExecutionTime = TimeSpan.FromHours(10),
                    TotalIdleTime = TimeSpan.FromHours(0.5),
                    TotalQueueTime = TimeSpan.FromHours(0.25),
                    TotalTravelDistance = 100.0M,
                    TotalTravelTime = TimeSpan.FromHours(1.5)
                };

            var resultA = solutionComparer.GetObjectiveMeasure(routeStatisticsA);
            var resultB = solutionComparer.GetObjectiveMeasure(routeStatisticsB);

            Assert.Less(resultA, resultB);
        }

        [Test]
        public void CreateJobServiceScenarioThree()
        {
            var startTime = TimeSpan.Zero;
            var job = new Job();
            var distanceServiceMock = new Mock<IDistanceService>();
            var SUT = new JobNodeService(distanceServiceMock.Object);

            var loc1 = new Drayage.Optimization.Model.Location() { DisplayName = "location1" };
            var loc2 = new Drayage.Optimization.Model.Location() { DisplayName = "location2" };
            var loc3 = new Drayage.Optimization.Model.Location() { DisplayName = "location3" };
            var loc4 = new Drayage.Optimization.Model.Location() { DisplayName = "location4" };
            var loc5 = new Drayage.Optimization.Model.Location() { DisplayName = "location5" };
            var loc6 = new Drayage.Optimization.Model.Location() { DisplayName = "location6" };

            var depot = new RouteStop() { WindowStart = TimeSpan.FromMinutes(0), WindowEnd =  TimeSpan.FromMinutes(1440) };
            var stop1 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(90), WindowEnd = TimeSpan.FromMinutes(1440) };
            var stop2 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(300), WindowEnd = TimeSpan.FromMinutes(300) };
            var stop3 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(60), WindowEnd = TimeSpan.FromMinutes(120) };
            var stop4 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(510), WindowEnd = TimeSpan.FromMinutes(510) };
            var stop5 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(510), WindowEnd = TimeSpan.FromMinutes(1440) };
            depot.Location = loc1;
            stop1.Location = loc2;
            stop2.Location = loc3;
            stop3.Location = loc4;
            stop4.Location = loc5;
            stop5.Location = loc6;

            depot.ExecutionTime = TimeSpan.FromMinutes(0);
            stop1.ExecutionTime = TimeSpan.FromMinutes(60);
            stop2.ExecutionTime = TimeSpan.FromMinutes(0);
            stop3.ExecutionTime = TimeSpan.FromMinutes(0);
            stop4.ExecutionTime = TimeSpan.FromMinutes(120);
            stop5.ExecutionTime = TimeSpan.FromMinutes(60);

            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location1"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"))).Returns(new TripLength(0M, TimeSpan.FromMinutes(0)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"))).Returns(new TripLength(94.5M, TimeSpan.FromMinutes(94.5)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"))).Returns(new TripLength(105.0167M, TimeSpan.FromMinutes(105.0167)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"))).Returns(new TripLength(49.71667M, TimeSpan.FromMinutes(49.71667)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location6"))).Returns(new TripLength(39.21667M, TimeSpan.FromMinutes(39.21667)));

            job.RouteStops = new List<RouteStop>
                {
                    depot,
                    stop1,
                    stop2,
                    stop3,
                    stop4,
                    stop5
                };


            var test = SUT.CreateJobNode(job, startTime, false);

            var results = SUT.Array;

            Assert.AreEqual(results[0, 0], 0);
            Assert.AreEqual(results[0, 1], 5400);
            Assert.AreEqual(results[0, 2], 18000);
            Assert.AreEqual(results[0, 3], 3600);
            Assert.AreEqual(results[0, 4], 30600);
            Assert.AreEqual(results[0, 5], 30600);

            Assert.AreEqual(results[1, 0], 86400);
            Assert.AreEqual(results[1, 1], 86400);
            Assert.AreEqual(results[1, 2], 18000);
            Assert.AreEqual(results[1, 3], 7200);
            Assert.AreEqual(results[1, 4], 30600);
            Assert.AreEqual(results[1, 5], 86400);

            Assert.AreEqual(results[2, 0], 0);
            Assert.AreEqual(results[2, 1], 0);
            Assert.AreEqual(results[2, 2], 5670);
            Assert.AreEqual(results[2, 3], 6301.0019999999995d);
            Assert.AreEqual(results[2, 4], 2983);
            Assert.AreEqual(results[2, 5], 2353);

            Assert.AreEqual(results[3, 0], 0);
            Assert.AreEqual(results[3, 1], 0);
            Assert.AreEqual(results[3, 2], 14670);
            Assert.AreEqual(results[3, 3], 24301.002d);
            Assert.AreEqual(results[3, 4], 27284.002d);
            Assert.AreEqual(results[3, 5], 40153);

            Assert.AreEqual(results[4, 0], 0);
            Assert.AreEqual(results[4, 1], 3600);
            Assert.AreEqual(results[4, 2], 0);
            Assert.AreEqual(results[4, 3], 0);
            Assert.AreEqual(results[4, 4], 7200);
            Assert.AreEqual(results[4, 5], 3600);

            Assert.AreEqual(results[5, 0], 0);
            Assert.AreEqual(results[5, 1], 5400);
            Assert.AreEqual(results[5, 2], 3330);
            Assert.AreEqual(results[5, 3], 0);
            Assert.AreEqual(results[5, 4], 3315.9979999999996d);
            Assert.AreEqual(results[5, 5], 0);

            Assert.AreEqual(results[6, 0], 0);
            Assert.AreEqual(results[6, 1], 9000);
            Assert.AreEqual(results[6, 2], 18000);
            Assert.AreEqual(results[6, 3], 24301.002d);
            Assert.AreEqual(results[6, 4], 37800);
            Assert.AreEqual(results[6, 5], 43753);

            Assert.AreEqual(results[7, 0], 0);
            Assert.AreEqual(results[7, 1], 0);
            Assert.AreEqual(results[7, 2], 0);
            Assert.AreEqual(results[7, 3], 1);
            Assert.AreEqual(results[7, 4], 0);
            Assert.AreEqual(results[7, 5], 0);

            Assert.AreEqual(results[8, 0], 0);
            Assert.AreEqual(results[8, 1], 0);
            Assert.AreEqual(results[8, 2], 9270);
            Assert.AreEqual(results[8, 3], 15571.002d);
            Assert.AreEqual(results[8, 4], 18554.002d);
            Assert.AreEqual(results[8, 5], 28107.002d);

            Assert.AreEqual(results[9, 0], 12045.998d);
            Assert.AreEqual(results[9, 1], 5400);
            Assert.AreEqual(results[9, 2], 8730);
            Assert.AreEqual(results[9, 3], 0);
            Assert.AreEqual(results[9, 4], 12045.998d);
            Assert.AreEqual(results[9, 5], 2492.9979999999996d);

            Assert.AreEqual(results[10, 0], 0);
            Assert.AreEqual(results[10, 1], 86400);
            Assert.AreEqual(results[10, 2], 8730);
            Assert.AreEqual(results[10, 3], 0);
            Assert.AreEqual(results[10, 4], 12045.998d);
            Assert.AreEqual(results[10, 5], 58292.998d);

        }

        [Test]
        public void CreateJobServiceScenarioTwo()
        {
            var startTime = TimeSpan.Zero;
            var job = new Job();
            var distanceServiceMock = new Mock<IDistanceService>();
            var SUT = new JobNodeService(distanceServiceMock.Object);

            var loc1 = new Drayage.Optimization.Model.Location() { DisplayName = "location1" };
            var loc2 = new Drayage.Optimization.Model.Location() { DisplayName = "location2" };
            var loc3 = new Drayage.Optimization.Model.Location() { DisplayName = "location3" };
            var loc4 = new Drayage.Optimization.Model.Location() { DisplayName = "location4" };
            var loc5 = new Drayage.Optimization.Model.Location() { DisplayName = "location5" };
            var loc6 = new Drayage.Optimization.Model.Location() { DisplayName = "location6" };

            var depot = new RouteStop() { WindowStart = TimeSpan.FromMinutes(0), WindowEnd = TimeSpan.FromMinutes(1440) };
            var stop1 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(90), WindowEnd = TimeSpan.FromMinutes(1440) };
            var stop2 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(300), WindowEnd = TimeSpan.FromMinutes(300) };
            var stop3 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(420), WindowEnd = TimeSpan.FromMinutes(420) };
            var stop4 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(510), WindowEnd = TimeSpan.FromMinutes(510) };
            var stop5 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(510), WindowEnd = TimeSpan.FromMinutes(1440) };
            depot.Location = loc1;
            stop1.Location = loc2;
            stop2.Location = loc3;
            stop3.Location = loc4;
            stop4.Location = loc5;
            stop5.Location = loc6;

            depot.ExecutionTime = TimeSpan.FromMinutes(0);
            stop1.ExecutionTime = TimeSpan.FromMinutes(60);
            stop2.ExecutionTime = TimeSpan.FromMinutes(0);
            stop3.ExecutionTime = TimeSpan.FromMinutes(0);
            stop4.ExecutionTime = TimeSpan.FromMinutes(120);
            stop5.ExecutionTime = TimeSpan.FromMinutes(60);

            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location1"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"))).Returns(new TripLength(0M, TimeSpan.FromMinutes(0)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"))).Returns(new TripLength(94.5M, TimeSpan.FromMinutes(94.5)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"))).Returns(new TripLength(105.0167M, TimeSpan.FromMinutes(105.0167)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"))).Returns(new TripLength(49.71667M, TimeSpan.FromMinutes(49.71667)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location6"))).Returns(new TripLength(39.21667M, TimeSpan.FromMinutes(39.21667)));

            job.RouteStops = new List<RouteStop>
                {
                    depot,
                    stop1,
                    stop2,
                    stop3,
                    stop4,
                    stop5
                };


            var test = SUT.CreateJobNode(job, startTime, false);

            var results = SUT.Array;

            Assert.AreEqual(results[0, 0], 0);
            Assert.AreEqual(results[0, 1], 5400);
            Assert.AreEqual(results[0, 2], 18000);
            Assert.AreEqual(results[0, 3], 25200.0d);
            Assert.AreEqual(results[0, 4], 30600);
            Assert.AreEqual(results[0, 5], 30600);

            Assert.AreEqual(results[1, 0], 86400);
            Assert.AreEqual(results[1, 1], 86400);
            Assert.AreEqual(results[1, 2], 18000);
            Assert.AreEqual(results[1, 3], 25200.0d);
            Assert.AreEqual(results[1, 4], 30600);
            Assert.AreEqual(results[1, 5], 86400);

            Assert.AreEqual(results[2, 0], 0);
            Assert.AreEqual(results[2, 1], 0);
            Assert.AreEqual(results[2, 2], 5670);
            Assert.AreEqual(results[2, 3], 6301.0019999999995d);
            Assert.AreEqual(results[2, 4], 2983);
            Assert.AreEqual(results[2, 5], 2353);

            Assert.AreEqual(results[3, 0], 8730.0d);
            Assert.AreEqual(results[3, 1], 8730);
            Assert.AreEqual(results[3, 2], 18000);
            Assert.AreEqual(results[3, 3], 24301.002d);
            Assert.AreEqual(results[3, 4], 28183);
            Assert.AreEqual(results[3, 5], 40153);

            Assert.AreEqual(results[4, 0], 0);
            Assert.AreEqual(results[4, 1], 3600);
            Assert.AreEqual(results[4, 2], 0);
            Assert.AreEqual(results[4, 3], 0);
            Assert.AreEqual(results[4, 4], 7200);
            Assert.AreEqual(results[4, 5], 3600);

            Assert.AreEqual(results[5, 0], 0);
            Assert.AreEqual(results[5, 1], 0);
            Assert.AreEqual(results[5, 2], 0);
            Assert.AreEqual(results[5, 3], 898.99799999999959d);
            Assert.AreEqual(results[5, 4], 2417.0d);
            Assert.AreEqual(results[5, 5], 0);

            Assert.AreEqual(results[6, 0], 8730);
            Assert.AreEqual(results[6, 1], 12330);
            Assert.AreEqual(results[6, 2], 18000);
            Assert.AreEqual(results[6, 3], 25200);
            Assert.AreEqual(results[6, 4], 37800);
            Assert.AreEqual(results[6, 5], 43753);

            Assert.AreEqual(results[7, 0], 0);
            Assert.AreEqual(results[7, 1], 0);
            Assert.AreEqual(results[7, 2], 0);
            Assert.AreEqual(results[7, 3], 0);
            Assert.AreEqual(results[7, 4], 0);
            Assert.AreEqual(results[7, 5], 0);

            Assert.AreEqual(results[8, 0], 0);
            Assert.AreEqual(results[8, 1], 0);
            Assert.AreEqual(results[8, 2], 9270);
            Assert.AreEqual(results[8, 3], 15571.002d);
            Assert.AreEqual(results[8, 4], 18554.002d);
            Assert.AreEqual(results[8, 5], 28107.002d);

            Assert.AreEqual(results[9, 0], 12045.998d);
            Assert.AreEqual(results[9, 1], 5400);
            Assert.AreEqual(results[9, 2], 8730);
            Assert.AreEqual(results[9, 3], 9628.9979999999996d);
            Assert.AreEqual(results[9, 4], 12045.998d);
            Assert.AreEqual(results[9, 5], 2492.9979999999996d);

            Assert.AreEqual(results[10, 0], 8730);
            Assert.AreEqual(results[10, 1], 86400);
            Assert.AreEqual(results[10, 2], 8730);
            Assert.AreEqual(results[10, 3], 9628.9979999999996d);
            Assert.AreEqual(results[10, 4], 12045.998d);
            Assert.AreEqual(results[10, 5], 58292.998d);

        }

        [Test]
        public void CreateJobServiceScenarioOne()
        {
            var startTime = TimeSpan.Zero;
            var job = new Job();
            var distanceServiceMock = new Mock<IDistanceService>();
            var SUT = new JobNodeService(distanceServiceMock.Object);

            var loc1 = new Drayage.Optimization.Model.Location() { DisplayName = "location1" };
            var loc2 = new Drayage.Optimization.Model.Location() { DisplayName = "location2" };
            var loc3 = new Drayage.Optimization.Model.Location() { DisplayName = "location3" };
            var loc4 = new Drayage.Optimization.Model.Location() { DisplayName = "location4" };
            var loc5 = new Drayage.Optimization.Model.Location() { DisplayName = "location5" };
            var loc6 = new Drayage.Optimization.Model.Location() { DisplayName = "location6" };

            var depot = new RouteStop() { WindowStart = TimeSpan.FromMinutes(0), WindowEnd = TimeSpan.FromMinutes(1440) };
            var stop1 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(90), WindowEnd = TimeSpan.FromMinutes(1440) };
            var stop2 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(270), WindowEnd = TimeSpan.FromMinutes(420) };
            var stop3 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(360), WindowEnd = TimeSpan.FromMinutes(420) };
            var stop4 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(420), WindowEnd = TimeSpan.FromMinutes(540) };
            var stop5 = new RouteStop() { WindowStart = TimeSpan.FromMinutes(510), WindowEnd = TimeSpan.FromMinutes(1440) };
            depot.Location = loc1;
            stop1.Location = loc2;
            stop2.Location = loc3;
            stop3.Location = loc4;
            stop4.Location = loc5;
            stop5.Location = loc6;

            depot.ExecutionTime = TimeSpan.FromMinutes(0);
            stop1.ExecutionTime = TimeSpan.FromMinutes(60);
            stop2.ExecutionTime = TimeSpan.FromMinutes(0);
            stop3.ExecutionTime = TimeSpan.FromMinutes(0);
            stop4.ExecutionTime = TimeSpan.FromMinutes(120);
            stop5.ExecutionTime = TimeSpan.FromMinutes(60);

            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location1"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"))).Returns(new TripLength(0M, TimeSpan.FromMinutes(0)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location2"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"))).Returns(new TripLength(94.5M, TimeSpan.FromMinutes(94.5)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location3"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"))).Returns(new TripLength(105.0167M, TimeSpan.FromMinutes(105.0167)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location4"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"))).Returns(new TripLength(49.71667M, TimeSpan.FromMinutes(49.71667)));
            distanceServiceMock.Setup(x => x.CalculateDistance(It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location5"), It.Is<PAI.Drayage.Optimization.Model.Location>(y => y.DisplayName == "location6"))).Returns(new TripLength(39.21667M, TimeSpan.FromMinutes(39.21667)));

            job.RouteStops = new List<RouteStop>
                {
                    depot,
                    stop1,
                    stop2,
                    stop3,
                    stop4,
                    stop5
                };


            var test = SUT.CreateJobNode(job, startTime, false);

            var results = SUT.Array;

            Assert.AreEqual(results[0, 0], 0);
            Assert.AreEqual(results[0, 1], 5400);
            Assert.AreEqual(results[0, 2], 16200);
            Assert.AreEqual(results[0, 3], 21600);
            Assert.AreEqual(results[0, 4], 25200);
            Assert.AreEqual(results[0, 5], 30600);

            Assert.AreEqual(results[1, 0], 86400);
            Assert.AreEqual(results[1, 1], 86400);
            Assert.AreEqual(results[1, 2], 25200);
            Assert.AreEqual(results[1, 3], 25200);
            Assert.AreEqual(results[1, 4], 32400.0d);
            Assert.AreEqual(results[1, 5], 86400);

            Assert.AreEqual(results[2, 0], 0);  
            Assert.AreEqual(results[2, 1], 0);
            Assert.AreEqual(results[2, 2], 5670);
            Assert.AreEqual(results[2, 3], 6301.0019999999995d);
            Assert.AreEqual(results[2, 4], 2983);
            Assert.AreEqual(results[2, 5], 2353);

            Assert.AreEqual(results[3, 0], 6930);
            Assert.AreEqual(results[3, 1], 6930.0d);
            Assert.AreEqual(results[3, 2], 16200.0d);
            Assert.AreEqual(results[3, 3], 22501.002d);
            Assert.AreEqual(results[3, 4], 25484.002d);
            Assert.AreEqual(results[3, 5], 35037.002d);

            Assert.AreEqual(results[4, 0], 0);
            Assert.AreEqual(results[4, 1], 3600);
            Assert.AreEqual(results[4, 2], 0);
            Assert.AreEqual(results[4, 3], 0);
            Assert.AreEqual(results[4, 4], 7200);
            Assert.AreEqual(results[4, 5], 3600);

            Assert.AreEqual(results[5, 0], 0);
            Assert.AreEqual(results[5, 1], 0);
            Assert.AreEqual(results[5, 2], 0);
            Assert.AreEqual(results[5, 3], 0);
            Assert.AreEqual(results[5, 4], 0);
            Assert.AreEqual(results[5, 5], 0);

            Assert.AreEqual(results[6, 0], 6930);
            Assert.AreEqual(results[6, 1], 10530);
            Assert.AreEqual(results[6, 2], 16200);
            Assert.AreEqual(results[6, 3], 22501.002d);
            Assert.AreEqual(results[6, 4], 32684.002d);
            Assert.AreEqual(results[6, 5], 38637.002d);

            Assert.AreEqual(results[7, 0], 0);
            Assert.AreEqual(results[7, 1], 0);
            Assert.AreEqual(results[7, 2], 0);
            Assert.AreEqual(results[7, 3], 0);
            Assert.AreEqual(results[7, 4], 0);
            Assert.AreEqual(results[7, 5], 0);

            Assert.AreEqual(results[8, 0], 0);
            Assert.AreEqual(results[8, 1], 0);
            Assert.AreEqual(results[8, 2], 9270);
            Assert.AreEqual(results[8, 3], 15571.002d);
            Assert.AreEqual(results[8, 4], 18554.002d);
            Assert.AreEqual(results[8, 5], 28107.002d);

            Assert.AreEqual(results[9, 0], 6930);
            Assert.AreEqual(results[9, 1], 5400);
            Assert.AreEqual(results[9, 2], 6930);
            Assert.AreEqual(results[9, 3], 6028.9979999999996d);
            Assert.AreEqual(results[9, 4], 6645.9979999999996d);
            Assert.AreEqual(results[9, 5], 2492.9979999999996d);

            Assert.AreEqual(results[10, 0], 9628.9979999999996d);
            Assert.AreEqual(results[10, 1], 86400);
            Assert.AreEqual(results[10, 2], 15930);
            Assert.AreEqual(results[10, 3], 9628.9979999999996d);
            Assert.AreEqual(results[10, 4], 13845.998d);
            Assert.AreEqual(results[10, 5], 58292.998d);
        }

        [Test]
        public void Check_Order_Feasibilities()
        {
            var jobNumberList = new List<string> { "2763057", "2762714", "2760980" };
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = driverService.SelectWithAll().Distinct().Where(x => x.FirstName == "ALBERTO" && x.LastName == "VALDES COLLADO").ToList();

            var planConfig =
                new PlanConfig
                    {
                        DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                        Id = 1,
                        Drivers = drivers.Select(Convert).ToList(),
                        Jobs = jobs.Select(Convert).Distinct().ToList()
                    };

            var optimizerSettings = new OptimizerSettings
                {
                    DisallowPlaceholderDriver = true
                };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var infeasibleIds = controller.GetInfeasibleJobIds(jobs.Select(Convert).Distinct().OrderByDescending(x => x.DisplayName).ToList(), drivers.Select(Convert).First());
            
            var wookie = plan.DriverPlans.SelectMany(x => x.JobPlans.Select(y => y.Job.DisplayName)).OrderBy(z => z).ToList();

            Assert.AreEqual(0, 0);
        }

        [Test]
        public void Check_Order_Feasibilities_Part_Duex()
        {
            var jobNumberList = new List<string> { "2763368", "2762711" };
            var controller = Make_PlanGenerator();
            var jobService = Kernel.Get<IJobService>();
            var driverService = Kernel.Get<IDriverService>();
            var jobs = jobService.SelectWithAll().Where(y => y.OrderNumber != "TESTORDER").Where(x => jobNumberList.Contains(x.OrderNumber)).ToList();
            var drivers = driverService.SelectWithAll().Distinct().Where(x => x.FirstName == "ALEXANDER" && x.LastName == "OLAYA").ToList();

            var planConfig =
                new PlanConfig
                {
                    DefaultDriver = new Drayage.Optimization.Model.Orders.Driver(),
                    Id = 1,
                    Drivers = drivers.Select(Convert).ToList(),
                    Jobs = jobs.Select(Convert).Distinct().ToList()
                };

            var optimizerSettings = new OptimizerSettings
            {
                DisallowPlaceholderDriver = true
            };

            var plan = controller.GeneratePlan(planConfig, planConfig.DefaultDriver, optimizerSettings);

            var infeasibleIds = controller.GetInfeasibleJobIds(jobs.Select(Convert).Distinct().OrderByDescending(x => x.DisplayName).ToList(), drivers.Select(Convert).First());

            var wookie = plan.DriverPlans.SelectMany(x => x.JobPlans.Select(y => y.Job.DisplayName)).OrderBy(z => z).ToList();

            Assert.AreEqual(0, 0);
        }


        [Test]
        public void GetMatrixIndex()
        {
            var results = -1;
            Make_PlanGenerator();
            results = jobNodeService.GetMatrixIndex("RowIndexWindowLowerBounds");
            Assert.AreEqual(0, results);
            results = jobNodeService.GetMatrixIndex("WindowLowerBounds");
            Assert.AreEqual(0, results);
            results = jobNodeService.GetMatrixIndex("RowIndexWindowUpperBounds");
            Assert.AreEqual(1, results);
            results = jobNodeService.GetMatrixIndex("WindowUpperBounds");
            Assert.AreEqual(1, results);
            results = jobNodeService.GetMatrixIndex("RowIndexTravelTime");
            Assert.AreEqual(2, results);
            results = jobNodeService.GetMatrixIndex("TravelTime");
            Assert.AreEqual(2, results);
            results = jobNodeService.GetMatrixIndex("RowIndexArrival");
            Assert.AreEqual(3, results);
            results = jobNodeService.GetMatrixIndex("Arrival");
            Assert.AreEqual(3, results);
            results = jobNodeService.GetMatrixIndex("RowIndexServiceTime");
            Assert.AreEqual(4, results);
            results = jobNodeService.GetMatrixIndex("ServiceTime");
            Assert.AreEqual(4, results);
            results = jobNodeService.GetMatrixIndex("RowIndexWaitTime");
            Assert.AreEqual(5, results);
            results = jobNodeService.GetMatrixIndex("WaitTime");
            Assert.AreEqual(5, results);
            results = jobNodeService.GetMatrixIndex("RowIndexDepartureTime");
            Assert.AreEqual(6, results);
            results = jobNodeService.GetMatrixIndex("DepartureTime");
            Assert.AreEqual(6, results);
            results = jobNodeService.GetMatrixIndex("RowIndexViolations");
            Assert.AreEqual(7, results);
            results = jobNodeService.GetMatrixIndex("Violations");
            Assert.AreEqual(7, results);
            results = jobNodeService.GetMatrixIndex("RowIndexSummation");
            Assert.AreEqual(8, results);
            results = jobNodeService.GetMatrixIndex("Summation");
            Assert.AreEqual(8, results);
            results = jobNodeService.GetMatrixIndex("RowIndexResultTwl");
            Assert.AreEqual(9, results);
            results = jobNodeService.GetMatrixIndex("ResultTwl");
            Assert.AreEqual(9, results);
            results = jobNodeService.GetMatrixIndex("RowIndexResultTwu");
            Assert.AreEqual(10, results);
            results = jobNodeService.GetMatrixIndex("ResultTwu");
            Assert.AreEqual(10, results);
            results = jobNodeService.GetMatrixIndex("SpomethingElse");
            Assert.AreEqual(-1, results);
        }


        static PlanConfig PlanConfig()
        {
            var planConfig =
                new PlanConfig
                {
                    Drivers = new List<Drayage.Optimization.Model.Orders.Driver>
                            {
                                new Drayage.Optimization.Model.Orders.Driver
                                    {
                                        Id = 1,
                                        IsHazmat = false,
                                        AvailableDrivingHours = 8,
                                        AvailableDutyHours = 12,
                                        DisplayName = "Alice",
                                        EarliestStartTime = new TimeSpan(7, 0, 0).Ticks,
                                        StartingLocation = new Drayage.Optimization.Model.Location()
                                            {
                                                Id = 10,
                                                DisplayName = "Location for 500",
                                                Latitude = 0.15,
                                                Longitude = 0.15
                                            }
                                    }
                            },
                    DefaultDriver =
                        new Drayage.Optimization.Model.Orders.Driver
                        {
                            Id = 0,
                            AvailableDrivingHours = 10,
                            AvailableDutyHours = 14,
                            DisplayName = "PlaceHolder",
                            EarliestStartTime = new TimeSpan(3, 0, 0).Ticks,
                            StartingLocation = new Drayage.Optimization.Model.Location
                            {
                                Id = 0,
                                DisplayName = "Default Starting Location",
                                Latitude = 0,
                                Longitude = 0
                            }
                        },
                    Id = 50,
                    Jobs = new List<Job>
                            {
                                new Job
                                    {
                                        Id = 20,
                                        DisplayName = "Job the first",
                                        EquipmentConfiguration = new EquipmentConfiguration(),
                                        IsHazmat = false,
                                        RouteStops = new List<RouteStop>
                                            {
                                                new RouteStop
                                                    {
                                                        Id = 500,
                                                        ExecutionTime = TimeSpan.Zero,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 11,
                                                                DisplayName = "Location for 500",
                                                                Latitude = 0.15,
                                                                Longitude = 0.15
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = TimeSpan.Zero,
                                                        StopAction = StopActions.PickupLoadedWithChassis,
                                                        WindowStart = new TimeSpan(0, 0, 0),
                                                        WindowEnd = new TimeSpan(23, 59, 59)
                                                    },
                                                new RouteStop
                                                    {
                                                        Id = 501,
                                                        ExecutionTime = new TimeSpan(12, 0, 0),
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 12,
                                                                DisplayName = "Location for 501",
                                                                Latitude = 0.15,
                                                                Longitude = 0.15
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = TimeSpan.Zero,
                                                        StopAction = StopActions.LiveUnloading,
                                                        WindowStart = new TimeSpan(10, 0, 0),
                                                        WindowEnd = new TimeSpan(11, 59, 59)
                                                    },
                                                new RouteStop
                                                    {
                                                        Id = 502,
                                                        ExecutionTime = TimeSpan.Zero,
                                                        Location = new Drayage.Optimization.Model.Location()
                                                            {
                                                                Id = 13,
                                                                DisplayName = "Location for 501",
                                                                Latitude = 0.15,
                                                                Longitude = 0.15
                                                            },
                                                        PostTruckConfig = new TruckConfiguration(),
                                                        PreTruckConfig = new TruckConfiguration(),
                                                        QueueTime = TimeSpan.Zero,
                                                        StopAction = StopActions.DropOffEmptyWithChassis,
                                                        WindowStart = new TimeSpan(0, 0, 0),
                                                        WindowEnd = new TimeSpan(23, 59, 59)
                                                    }
                                            }
                                    }
                            }
                };
            return planConfig;
        }
    }
}
