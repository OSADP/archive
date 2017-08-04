using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.CTIP.Optimization.Reporting.Services;
using PAI.Core;
using PAI.CTIP.Optimization;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Services;
using Ninject;

namespace ConsoleDemo
{
    class Program
    {
        private static JobHelper _helper = null;
        public static JobHelper Helper
        {
            get { return _helper ?? (_helper = new JobHelper()); }
        }


        public static IKernel Kernel{ get; set; }

        private static void Initialize()
        {
            IKernel kernel = new StandardKernel(new MyModule());
            Kernel = kernel;
        }
        
        public static TInterface GetService<TInterface>()
        {
            try
            {
                var result = Kernel.Get<TInterface>();
                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        static void Main(string[] args)
        {
            Initialize();   // Initialization of Ninject - Dependency Injection

            var startLocation = MockData.GetMockLocations(1, "Driver").FirstOrDefault();
            var locations = MockData.GetMockLocations(10);
            var drivers = MockData.GetMockDrivers(3, startLocation);
            var jobs = MockData.GetJobs(4, locations, true);

            // initialize the Optimization service
            var optimizer = GetService<IDrayageOptimizer>();            
            optimizer.Initialize();
            
            // solution iterations
            for (int i = 0; i < 3; i++)
            {
                // build the solution
                var solution = optimizer.BuildSolution(drivers, new Driver()
                    {
                        DisplayName="Placeholder Driver", 
                        StartingLocation = drivers.First().StartingLocation
                    },  jobs);


                // statistics
                var reportingService = GetService<IReportingService>();
                var solutionPerformance = reportingService.GetSolutionPerformanceStatistics(solution);
                var solutionPerformanceReport = reportingService.GetSolutionPerformanceStatisticsReport(solution);

                // output results
                Console.WriteLine("Solution Created");
                Console.WriteLine(solution.RouteSolutions.Count.ToString() + " route solutions.");
                Console.WriteLine(solution.UnassignedJobNodes.Count.ToString() + " unassigned jobs.");
                Console.WriteLine(solution.RouteStatistics.TotalTime.ToString() + " : total time.\n");

                foreach (var routeSolution in solution.RouteSolutions)
                {
                    Console.WriteLine(string.Format("Solution for Driver {0}\n", routeSolution.DriverNode.Driver.DisplayName));
                    Console.WriteLine(string.Format("\tStart Location -\n\t\t{0}", routeSolution.AllNodes.FirstOrDefault().RouteStops.FirstOrDefault().Location.DisplayName));
                    Console.WriteLine(string.Format("\tStart Time -\n\t\t{0}", routeSolution.DriverNode.Driver.EarliestStartTime));
                    
                    int stopCount = 0;
                    var driverStatistics =
                        solutionPerformance.TruckStatistics.FirstOrDefault(
                            p => p.Key.DriverNode.Driver.Id == routeSolution.DriverNode.Driver.Id).Value;

                    foreach (var node in routeSolution.Nodes)
                    {
                        var jn = node as JobNode;
                        Console.WriteLine(string.Format("\n\tOrder {0} - (Priority {1})", jn.Job.DisplayName, jn.Job.Priority));

                        var startStatistics = driverStatistics.RouteSegmentStatistics[0];
                        if (startStatistics.Statistics.TotalWaitTime.TotalSeconds > 0)
                        {
                            Console.WriteLine(string.Format("\t\t\tWait for {0}", startStatistics.Statistics.TotalWaitTime));
                        }

                        Console.WriteLine(string.Format("\t\t\t{0}-{1}", startStatistics.StartTime, startStatistics.EndTime));
                        Console.WriteLine(string.Format("\t\t\t{0} mi", startStatistics.Statistics.TotalTravelDistance));

                        foreach (var rs in node.RouteStops)
                        {
                            Console.WriteLine(string.Format("\t\tStop {0}: {1}", ++stopCount, rs.Location.DisplayName));
                            Console.WriteLine(string.Format("\t\t\tWindow {0}: {1}", rs.WindowStart, rs.WindowEnd));

                            try
                            {
                               
                                var segmentStatistics = driverStatistics.RouteSegmentStatistics[stopCount-1];
                                Console.WriteLine(string.Format("\t\t\tLeave Previous Stop @ {0}", segmentStatistics.StartTime));
                                Console.WriteLine(string.Format("\t\t\tDistance: {0} mi", segmentStatistics.Statistics.TotalTravelDistance));
                                if (segmentStatistics.Statistics.TotalWaitTime.TotalSeconds > 0)
                                {
                                    Console.WriteLine(string.Format("\t\t\tWait for {0}", segmentStatistics.Statistics.TotalWaitTime));                                
                                }
                                Console.WriteLine(string.Format("\t\t\tArrive @ {0}", segmentStatistics.StartTime.Add(segmentStatistics.Statistics.TotalTravelTime).Add(segmentStatistics.Statistics.TotalWaitTime)));
                                Console.WriteLine(string.Format("\t\t\tFinished @ {0}", segmentStatistics.EndTime));

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Unable to get statistics");
                                continue;
                            }                            
                        }
                        
                    }
                    Console.WriteLine(string.Format("\n\tEnd -\n\t\t{0}", routeSolution.AllNodes.LastOrDefault().RouteStops.FirstOrDefault().Location.DisplayName));
                }
                Console.ReadKey();
            }
        }
    }
}
