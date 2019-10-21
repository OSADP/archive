using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.Extensions.Conventions;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Geography;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.CTIP.Optimization.Reporting.Services;
using PAI.CTIP.Optimization.Services;
using PAI.Core;

namespace Tests
{
    public class OptimizerTests : TestBase
    {
        private IList<Location> Locations { get; set; }
        private IList<Driver> Drivers { get; set; }
        private IList<Job> Jobs { get; set; }
        private Location StartLocation { get; set; }

        private IDrayageOptimizer _optimizer;
        private IDrayageOptimizer Optimizer
        {
            get
            {
               if (_optimizer == null)
               {
                   _optimizer = Kernel.Get<IDrayageOptimizer>();
                   _optimizer.Initialize();
               }
                return _optimizer;
            }
        }

        public Driver PlaceholderDriver
        {
            get
            {
                return new Driver()
                    {
                        IsHazmatEligible = true,
                        IsShortHaulEligible = true,
                        IsLongHaulEligible = true,
                        DisplayName = "Placeholder Driver",
                        StartingLocation = StartLocation
                    };
            }
        }

        [SetUp]
        public void SetUp()
        {
            Kernel.Bind<IDrayageOptimizer>().To<DrayageOptimizer>().InSingletonScope();
            Kernel.Bind<IPheromoneMatrix>().To<PheromoneMatrix>().InSingletonScope()
                .WithConstructorArgument("initialPheromoneValue", 0.0)
                .WithConstructorArgument("rho", 0.5)
                .WithConstructorArgument("q", 1000.0);
            Kernel.Bind<IRouteExitFunction>().To<RouteExitFunction>().InSingletonScope();
            Kernel.Bind<IRouteService>().To<RouteService>().InSingletonScope();
            Kernel.Bind<IRouteStopDelayService>().To<RouteStopDelayService>().InSingletonScope();
            Kernel.Bind<IRouteStopService>().To<RouteStopService>().InSingletonScope();
            Kernel.Bind<IStatisticsService>().To<StatisticsService>().InSingletonScope();
            Kernel.Bind<IObjectiveFunction>().To<DistanceObjectiveFunction>().InSingletonScope();
            Kernel.Bind<IRandomNumberGenerator>().To<RandomNumberGenerator>().InSingletonScope();
            Kernel.Bind<IProbabilityMatrix>().To<ProbabilityMatrix>().InSingletonScope();
            Kernel.Bind<INodeFactory>().To<NodeFactory>().InSingletonScope();

            Kernel.Bind<ILogger>().To<NullLogger>().InSingletonScope();
            Kernel.Bind<INodeService>()
                .To<NodeService>()
                .InSingletonScope()
                .WithConstructorArgument("configuration", new OptimizerConfiguration());
            Kernel.Bind<IDistanceService>().To<DistanceService>().InSingletonScope();
            Kernel.Bind<ITravelTimeEstimator>().To<TravelTimeEstimator>().InSingletonScope();

            Kernel.Bind<IReportingService>().To<ReportingService>().InSingletonScope();    // todo verify scope
            Kernel.Bind<IRouteSanitizer>().To<RouteSanitizer>().InSingletonScope();    // todo verify scope

            // prepare mock data
            StartLocation = MockData.GetMockLocations(1, "Driver").FirstOrDefault();
            Locations = MockData.GetMockLocations(20);
            Drivers = MockData.GetMockDrivers(10, StartLocation);
            Jobs = MockData.GetJobs(50, Locations, true);
        }

        
        [Test]
        public void Can_prepare_mock_data()
        {
            Assert.That(StartLocation != null);
            Assert.That(Locations != null && Locations.Any(), "Mock Locations not ready");
            Assert.That(Drivers != null && Drivers.Any(), "Mock Drivers not ready");
            Assert.That(Jobs != null && Jobs.Any(), "Mock Jobs not ready");

            var hazmatJobs = Jobs.Where(p => p.IsHazmat);
            var shortHaulJobs = Jobs.Where(p => p.IsShortHaul);
            var longHaulJobs = Jobs.Where(p => p.IsLongHaul);
            var type1Jobs = Jobs.Where(p => p.OrderType == 1);
            var type2Jobs = Jobs.Where(p => p.OrderType == 2);
            var type3Jobs = Jobs.Where(p => p.OrderType == 3);
            var p1Jobs = Jobs.Where(p => p.Priority == 1);
            var p2Jobs = Jobs.Where(p => p.Priority == 2);
            var p3Jobs = Jobs.Where(p => p.Priority == 3);

            Assert.That(hazmatJobs != null && hazmatJobs.Any());
            Assert.That(shortHaulJobs != null && shortHaulJobs.Any());
            Assert.That(longHaulJobs != null && longHaulJobs.Any());
            Assert.That(type1Jobs != null && type1Jobs.Any());
            Assert.That(type2Jobs != null && type2Jobs.Any());
            Assert.That(type3Jobs != null && type3Jobs.Any());
            Assert.That(p1Jobs != null && p1Jobs.Any());
            Assert.That(p2Jobs != null && p2Jobs.Any());
            Assert.That(p3Jobs != null && p3Jobs.Any());

            var hazmatDrivers = Drivers.Where(p => p.IsHazmatEligible);
            var shortHaulDrivers = Drivers.Where(p => p.IsShortHaulEligible);
            var longHaulDrivers = Drivers.Where(p => p.IsLongHaulEligible);
            var type1Drivers = Drivers.Where(p => p.OrderType == 1);
            var type2Drivers = Drivers.Where(p => p.OrderType == 2);
            var type3Drivers = Drivers.Where(p => p.OrderType == 3);

            Assert.That(hazmatDrivers != null && hazmatDrivers.Any());
            Assert.That(shortHaulDrivers != null && shortHaulDrivers.Any());
            Assert.That(longHaulDrivers != null && longHaulDrivers.Any());
            Assert.That(type1Drivers != null && type1Drivers.Any());
            Assert.That(type2Drivers != null && type2Drivers.Any());
            Assert.That(type3Drivers != null && type3Drivers.Any());
        }


        [Test]
        public void Can_only_send_hazmat_jobs_to_hazmat_drivers()
        {
            var hazmatJobs = Jobs.Where(p => p.IsHazmat).ToList();
            var hazmatIneligibleDrivers = Drivers.Where(p => !p.IsHazmatEligible).ToList();

            var solution = Optimizer.BuildSolution(
                hazmatIneligibleDrivers,
                PlaceholderDriver,
                hazmatJobs);

            Assert.That(hazmatJobs.Any(), "No hazmat mock jobs identified");
            Assert.That(hazmatIneligibleDrivers.Any(), "No hazmat ineligible mock drivers identified");
            Assert.That(solution.UnassignedJobNodes.Count > 0 && solution.RouteSolutions.Count == 0, "Ineligible driver has been assigned a hazmat job");
        }

        [Test]
        public void Can_driver_only_do_appropriate_order_types()
        {
            var type3Drivers = SetMaximumDriverEligibility(Drivers.Where(p => p.OrderType == 3));
            var type2Orders = Jobs.Where(p => p.OrderType == 2).Take(4).ToList();
            var type3Orders = Jobs.Where(p => p.OrderType == 3).Take(4).ToList();

            Assert.That(type3Drivers.Any(), "No mock drivers identified");
            Assert.That(type2Orders.Any(), "No mock jobs - order type 2 identified");
            Assert.That(type3Orders.Any(), "No mock jobs - order type 3 identified");

            var solutionUnfeasible = Optimizer.BuildSolution(
                type3Drivers,
                PlaceholderDriver,
                type2Orders);

            var solutionFeasible = Optimizer.BuildSolution(
                type3Drivers,
                PlaceholderDriver,
                type3Orders);

            Assert.That(solutionUnfeasible.UnassignedJobNodes.Count > 0 && solutionUnfeasible.RouteSolutions.Count == 0, "Ineligible driver has been assigned a job");
            Assert.That(solutionFeasible.UnassignedJobNodes.Count == 0 && solutionFeasible.RouteSolutions.Count > 0, "A job was unassigned");

        }

        [Test]
        public void Can_create_jobnode_with_jobnodefactory()
        {
            var job = Jobs.FirstOrDefault();
            var jobNodeFactory = Kernel.Get<INodeFactory>();

            var jobNode = jobNodeFactory.CreateJobNode(job);

            Assert.That(job != null);
            Assert.That(job.RouteStops != null);
            Assert.That(job.RouteStops.Count > 1);
            Assert.That(jobNode != null);            
        }

        [Test]
        public void Can_prioritize_orders()
        {
            var priorityJobs = Jobs.Where(p => p.Priority >= 3).Take(5).ToList();
            var otherJobs = Jobs.Where(p => p.Priority < 3).Take(15).ToList();
            var drivers = SetMaximumDriverEligibility(Drivers, true).Take((priorityJobs.Count / 5) + 1).ToList();

            var solution = Optimizer.BuildSolution(
                drivers,
                PlaceholderDriver,
                priorityJobs.Concat(otherJobs).ToList());

            var priorityJobIds = new HashSet<int>(priorityJobs.Select(p => p.Id));
            foreach (var rs in solution.RouteSolutions)
            {
                foreach (JobNode node in rs.Nodes)
                {
                    priorityJobIds.Remove(node.Job.Id);
                }
            }

            Assert.That(priorityJobIds.Count == 0, "Not all priority jobs were given in a route solution");
        }

        /// <summary>
        /// Can build a simple optimization solution
        /// </summary>
        [Test]
        public void Can_build_solution()
        {
            var solution = Optimizer.BuildSolution(
                SetMaximumDriverEligibility(Drivers.Take(1).ToList(), true),
                PlaceholderDriver,
                Jobs.Take(5).ToList());

            Assert.That(solution != null, "Solution is null");
            Assert.That(solution.RouteSolutions.Any(), "Solution has no route solutions");
        }

        /// <summary>
        /// Set the drivers eligibility status to true
        /// </summary>
        /// <param name="drivers"></param>
        /// <param name="resetOrderTypeValue"></param>
        /// <returns></returns>
        private IList<Driver> SetMaximumDriverEligibility(IEnumerable<Driver> drivers, bool resetOrderTypeValue = false)
        {
            var result = drivers.ToList();
            foreach (var d in result)
            {
                d.IsHazmatEligible = true;
                d.IsLongHaulEligible = true;
                d.IsShortHaulEligible = true;
                
                if (resetOrderTypeValue)
                {
                    d.OrderType = 1;    // highest setting, eligible for all order types
                }

            }
            return result;
        }


        [Test]
        public void Can_test_route_stop_second_window()
        {
            var drivers = SetMaximumDriverEligibility(Drivers.Where(p => p.OrderType == 1)).Take(1).ToList();
            var job = Jobs.FirstOrDefault(p => p.OrderType == 3);

            var solution = Optimizer.BuildSolution(drivers, PlaceholderDriver, new List<Job>() { job });

            foreach (var rs in solution.RouteSolutions)
            {
                Console.WriteLine("Driver {0}", rs.DriverNode.Driver.DisplayName);
                foreach (var node in rs.Nodes)
                {
                    Console.WriteLine("\tJob Priority {0}", node.Priority);
                    
                }
            }
        }

        [Test]
        public void Can_test_prioritization_of_orders()
        {
            var type1Drivers = SetMaximumDriverEligibility(Drivers.Where(p => p.OrderType == 1));

            var priorityOrders = Jobs.Where(p => p.OrderType == 1).Take(32).ToList();
            var normalOrders = Jobs.Where(p => p.OrderType == 3).Take(4).ToList();

            var jobs = new List<Job>();
            jobs.AddRange(priorityOrders);
            jobs.AddRange(normalOrders);

            var solution = Optimizer.BuildSolution(type1Drivers, PlaceholderDriver, jobs);

            int normalJobCount = 0;
            int priorityJobCount = 0;

            foreach (var rs in solution.RouteSolutions)
            {
                Console.WriteLine("Driver {0}", rs.DriverNode.Driver.DisplayName);
                foreach (var node in rs.Nodes)
                {
                    Console.WriteLine("\tJob Priority {0}", node.Priority);
                    if (node.Priority == 3)
                    {
                        priorityJobCount++;
                    }
                    else
                    {
                        normalJobCount++;
                    }
                }

                Console.WriteLine("\n\tNormal Count: {0}     Priority Count: {1}", normalJobCount, priorityJobCount);
            }
        }
    }
}