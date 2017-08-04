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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PAI.Drayage.Optimization.Common;
using PAI.Drayage.Optimization.Function;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.Infrastructure;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// The main entry point for per driver route optimization
    /// </summary>
    public class DrayageOptimizer : IDrayageOptimizer
    {
        protected readonly ILogger _logger;
        protected readonly IRouteService _routeService;
        protected readonly IRouteStatisticsService _routeStatisticsService;
        protected readonly IPheromoneMatrix _pheromoneMatrix;
        protected readonly IRouteExitFunction _routeExitFunction;
        protected readonly IProbabilityMatrix _probabilityMatrix;
        private readonly IRandomNumberGenerator _randomNumberGenerator;

        private readonly IRouteStopService _routeStopService;
        private readonly IJobNodeService _jobNodeService;

        protected readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        protected bool _isCancelling = false;
        protected bool _isExecuting = false;
        private bool _initialized = false;
        private Solution _currentBestSolution = null;

        public bool EnableParallelism { get; set; }
        public int PheromoneUpdateFrequency { get; set; }
        public int MaxIterations { get; set; }
        public int MaxIterationSinceBestResult { get; set; }
        public int MaxExecutionTime { get; set; }
        public bool DisallowPlaceholderDriver { get; set; }

        public Solution CurrentBestSolution
        {
            get
            {
                Solution result = null;
                _rwLock.EnterReadLock();
                result = _currentBestSolution;
                _rwLock.ExitReadLock();

                return result;
            }
            set
            {
                _rwLock.EnterWriteLock();
                _currentBestSolution = value;

                if (SolutionUpdated != null)
                {
                    SolutionUpdated.Invoke(this, new SolutionEventArgs() {Solution = _currentBestSolution});
                }

                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrayageOptimizer"/> class.
        /// </summary>
        /// <param name="probabilityMatrix">The probability matrix.</param>
        /// <param name="routeService">The route service.</param>
        /// <param name="routeExitFunction">The route exit function.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="pheromoneMatrix">The pheromone matrix.</param>
        /// <param name="nodeService">The node service.</param>
        /// <param name="randomNumberGenerator">The random number generator.</param>
        /// <param name="nodeFactory">The node factory.</param>
        /// <param name="jobNodeService">The job Node Service.</param>
        /// <param name="routeStopService">The route stop service.</param>
        public DrayageOptimizer(IProbabilityMatrix probabilityMatrix,
            IRouteService routeService,
            IRouteExitFunction routeExitFunction, 
            ILogger logger, 
            IPheromoneMatrix pheromoneMatrix, 
            IRandomNumberGenerator randomNumberGenerator, 
            IRouteStatisticsService routeStatisticsService, 
			IJobNodeService jobNodeService, 
			IRouteStopService routeStopService)
        {
            _probabilityMatrix = probabilityMatrix;
            _routeExitFunction = routeExitFunction;
            _logger = logger;
            _pheromoneMatrix = pheromoneMatrix;
            _randomNumberGenerator = randomNumberGenerator;
            _routeStatisticsService = routeStatisticsService;
            _jobNodeService = jobNodeService;
            _routeStopService = routeStopService;
            _routeService = routeService;

            // default values
            EnableParallelism = true;
            PheromoneUpdateFrequency = 5;
            MaxIterations = 20000;
            MaxIterationSinceBestResult = 1500;
            MaxExecutionTime = 100;
        }

        /// <summary>
        /// Cancels the optimization run.
        /// </summary>
        public virtual void Cancel()
        {
            _rwLock.EnterWriteLock();
            if (_isExecuting)
            {
                _isCancelling = true;
            }
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Initializes Optimizer
        /// </summary>
        public virtual void Initialize()
        {
            _initialized = true;
        }

        public event EventHandler<SolutionEventArgs> SolutionUpdated;

        #region Progress Event
        public event EventHandler<ProgressEventArgs> ProgressChanged = null;

        private void OnProgressEvent(ProgressEventArgs eventArgs)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged.Invoke(this, eventArgs);
            }
        }
        #endregion

        /// <summary>
        /// Creates route assignments from the given trucks and jobs
        /// </summary>
        /// <param name="drivers">the list of drivers available to run routes</param>
        /// <param name="defaultDriver">a default driver to use if there are more routes generated than drivers</param>
        /// <param name="jobs">a list of jobs to optimize</param>
        /// <returns></returns>
        public virtual Solution BuildSolution(IList<Driver> drivers, Driver defaultDriver, IList<Job> jobs)
        {
            if (drivers == null) throw new ArgumentNullException("drivers");
            if (jobs == null) throw new ArgumentNullException("jobs");
            if (defaultDriver == null) throw new ArgumentNullException("defaultDriver");
            if (jobs.Count == 0) throw new OptimizationException("Need at least one job to build solution");

            if (!_initialized)
            {
                throw new OptimizationException("Optimizer not intialized.  Call Initialize() prior to building solutions.");
            }

            if (_isExecuting)
            {
                //throw new OptimizationException("Cannot BuildSolution while another BuildSolution is executing.");
            }

            // begin execution
            _isExecuting = true;
            _isCancelling = false;
            CurrentBestSolution = null;

            // clear pheromone matrix
            _pheromoneMatrix.Clear();

            // Create driver nodes
            var driverNodes = drivers.Select(driver => new DriverNode(driver)).ToList();

            // Create job nodes

            var jobNodes = jobs.Select((job, i) => _jobNodeService.CreateJobNode(job, defaultDriver.EarliestStartTimeSpan)).ToList();

            if (!DisallowPlaceholderDriver)
            {
                // create placeholder drivers (for worst case scenario)
                var placeholderDriversToCreate = jobNodes.Count() - driverNodes.Count();
                if (placeholderDriversToCreate > 0)
                {
                    var placeholderDriverNode = new DriverNode(defaultDriver);
                    for (int i = 0; i < placeholderDriversToCreate; i++)
                    {
                        driverNodes.Add(placeholderDriverNode);
                    }
                }
            }

            // build solution
            var bestSolution = BuildSolution(defaultDriver, jobNodes.Cast<INode>().ToList(), driverNodes);
            
            // done executing
            _isExecuting = false;
            _isCancelling = false;

            CurrentBestSolution = bestSolution;

            return bestSolution;
        }

        /// <summary>
        /// Builds the solution.
        /// </summary>
        /// <param name="placeholderDriver">The PlaceHolder driver.</param>
        /// <param name="jobNodes">The job nodes.</param>
        /// <param name="driverNodes">The driver nodes.</param>
        /// <returns></returns>
        public virtual Solution BuildSolution(Driver placeholderDriver, IList<INode> jobNodes, IList<DriverNode> driverNodes)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            int iterationsSinceBestResult = 0;
            int totalIterations = 0;
            bool stopping = false;

            Solution bestSolution = null;
            var subSolutions = new List<Solution>();

            // Define iteration
            Action<int, ParallelLoopState> iterationAction = (i, loopState) =>
                {
                    var solution = ProcessDrivers(driverNodes, placeholderDriver, jobNodes);
                    
                    _rwLock.EnterWriteLock();

                    try
                    {
                        totalIterations++;
                        iterationsSinceBestResult++;
                        subSolutions.Add(solution);

                        // Update progress
                        OnProgressEvent(
                            new ProgressEventArgs()
                            {
                                PrimaryTotal = MaxIterations,
                                PrimaryValue = totalIterations
                            });

                        var oldBest = bestSolution;

                        bestSolution = bestSolution == null ? solution : _routeService.GetBestSolution(bestSolution, solution);
                        
                        if (bestSolution != oldBest)
                        {
                            _currentBestSolution = bestSolution;
                            iterationsSinceBestResult = 0;
                        }

                        // Check exit criteria
                        if ((loopState == null || !loopState.IsStopped) && iterationsSinceBestResult > MaxIterationSinceBestResult)
                        {
                            _logger.Info("Max iterations since best result reached ({0})", MaxIterationSinceBestResult);
                            stopping = true;
                        }

                        if ((loopState == null || !loopState.IsStopped) && stopWatch.ElapsedMilliseconds > MaxExecutionTime)
                        {
                            _logger.Info("Max execution time {0} reached", MaxExecutionTime);
                            stopping = true;
                        }

                        if (_isCancelling)
                        {
                            stopping = true;
                        }

                        if (stopping)
                        {
                            if (loopState != null) loopState.Stop();
                        }

                    }
                    finally
                    {
                        _rwLock.ExitWriteLock();
                    }

                    // Update Pheromone Matrix 
                    if (!stopping && PheromoneUpdateFrequency > 0 && i > 0 && i % PheromoneUpdateFrequency == 0)
                    {
                        List<Solution> subResultCopy = null;

                        _rwLock.EnterWriteLock();
                        {
                            subResultCopy = subSolutions.ToList();
                            subSolutions.Clear();
                        }
                        _rwLock.ExitWriteLock();

                        foreach (var subSolution in subResultCopy)
                        {
                            _pheromoneMatrix.UpdatePheromoneMatrix(subSolution);
                        }
                    }
                };

            // Run iterations
            //if (EnableParallelism)
            if (false)      // ajh ajh
            {
                Parallel.For(0, MaxIterations, iterationAction);
            }
            else
            {
                for (var i = 0; i < MaxIterations; i++)
                {
                    iterationAction(i, null);

                    if (stopping)
                        break;
                }
            }

            stopWatch.Stop();

            return bestSolution;
        }

        /// <summary>
        /// Creates our route assignments by processing all the drivers and getting their routes
        /// </summary>
        /// <returns></returns>
        public virtual Solution ProcessDrivers(IList<DriverNode> driverNodes, Driver placeholderDriver, IList<INode> jobNodes)
        {
            var solution = new Solution();

            if (jobNodes.Count == 0) return solution;
            
            var availableNodes = jobNodes;
            
            var realDriverNodes = driverNodes.Where(f => f.Driver != placeholderDriver)
                .OrderBy(f => _randomNumberGenerator.Next()); // random sort order

            var placeholderDriverNodes = driverNodes.Where(f => f.Driver == placeholderDriver);

            // going through all the real drivers
            foreach (var driverNode in realDriverNodes)
            {
                // get the route solution for this particular driver
                var nodeRouteSolution = new NodeRouteSolution();
                nodeRouteSolution = GenerateIterativeRouteSolution(availableNodes, driverNode);

                // insert solution
                solution.RouteSolutions.Add(nodeRouteSolution);

                // update available nodes based on new solution
                var selectedIds = nodeRouteSolution.Nodes.Select(p => p.Id).ToList();
                availableNodes = availableNodes.Where(p => !selectedIds.Contains(p.Id)).ToList();
                if (availableNodes.Count == 0) break;
            }

            // try using placeholder drivers
            if (availableNodes.Count > 0)
            {
                foreach (var driverNode in placeholderDriverNodes)
                {
                    // get the route solution for this particular driver
                    var nodeRouteSolution = GenerateIterativeRouteSolution(availableNodes, driverNode);

                    // don't bother if we didn't generate a solution
                    if (nodeRouteSolution.Nodes.Count == 0)
                        break;

                    // insert solution
                    solution.RouteSolutions.Add(nodeRouteSolution);

                    // update available nodes based on new solution
                    availableNodes = availableNodes.Where(n => !nodeRouteSolution.Nodes.Contains(n)).ToList();
                    if (availableNodes.Count == 0) break;
                }
            }

            // Add unassigned job nodes
            solution.UnassignedJobNodes.AddRange(availableNodes);
            
            return solution;
        }

        /// <summary>
        /// Generates node sequence iteration
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="driverNode"> </param>
        /// <returns></returns>
        public virtual NodeRouteSolution GenerateRouteSolution(IList<INode> nodes, DriverNode driverNode)
        {
            IList<INode> processedNodes = new List<INode>();
            INode currentNode = driverNode;
            var startTime = driverNode.Driver.EarliestStartTimeSpan;
            var currentNodeEndTime = startTime;
            var cumulativeRouteStatistics = new RouteStatistics();

            int exitCounter = 0;
            while (exitCounter++ < 1000)
            {
                // getting avaiable nodes that have not been processed
                var feasibleNodeTimings = GetFeasibleNodes(nodes, driverNode, processedNodes, currentNodeEndTime, cumulativeRouteStatistics);
                if (!feasibleNodeTimings.Any())
                {
                    break;
                }

                var feasibleNodeTimingsByNode = feasibleNodeTimings.ToDictionary(f => f.Node);

                // build probability matrix for the available nodes
                var probabilityData = _probabilityMatrix.BuildProbabilityDataMatrix(currentNode, feasibleNodeTimings);

                // find a suitable node based on the cumulative probability
                var selectedNode = (INode)_probabilityMatrix.GetNominatedElement(probabilityData);

                selectedNode.DepartureTime = feasibleNodeTimingsByNode[selectedNode].DepartureTime; // set the Departure Time

                // break if we nominated the driver node
                if (selectedNode == driverNode)
                {
                    break;
                }

                processedNodes.Add(selectedNode);
                currentNode = selectedNode;

                // now we update the current node's end time
                var selectedNodeTiming = feasibleNodeTimingsByNode[selectedNode];

                if (processedNodes.Count == 1 && selectedNodeTiming.DepartureTime != currentNodeEndTime.Ticks)
                {
                    startTime = new TimeSpan(selectedNodeTiming.DepartureTime);
                }

                currentNodeEndTime = selectedNodeTiming.EndExecutionTime;
                cumulativeRouteStatistics = selectedNodeTiming.CumulativeRouteStatistics;

                var processedRouteStops = processedNodes.SelectMany(x => x.RouteStops);
                var provisionalRouteStops = new Queue<RouteStop>();
                foreach (var processedRouteStop in processedRouteStops)
                {
                    provisionalRouteStops.Enqueue(processedRouteStop);
                }
                foreach (var routeStop in selectedNode.RouteStops)
                {
                    provisionalRouteStops.Enqueue(routeStop);
                }

                var tempJob = new Job
                    {
                        RouteStops = provisionalRouteStops.Select(x => x).ToList()
                    };

                var tempJobNode = _jobNodeService.CreateJobNode(tempJob, driverNode.Driver.EarliestStartTimeSpan, false);

                if (!tempJobNode.IsInvalid)
                {
                    var serviceTimeIndex = _jobNodeService.GetMatrixIndex("ServiceTime");
                    var waitTimeIndex = _jobNodeService.GetMatrixIndex("WaitTime");
                    var travelTimeIndex = _jobNodeService.GetMatrixIndex("TravelTime");
                    var entryCount = tempJobNode.RouteStatisticsMatrix.GetLength(1);

                    var travelTime = TimeSpan.Zero;
                    var serviceTime = TimeSpan.Zero;
                    var waitTime = TimeSpan.Zero;

                    for (var i = 0; i < entryCount; i++)
                    {
                        travelTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[travelTimeIndex, i]);
                        serviceTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[serviceTimeIndex, i]);
                        waitTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[waitTimeIndex, i]);
                    }

                    var statistics = new RouteStatistics
                        {
                            TotalExecutionTime = serviceTime,
                            TotalIdleTime = waitTime,
                            TotalTravelTime = travelTime,
                            TotalQueueTime = TimeSpan.Zero
                        };
                    if (!_routeExitFunction.ExeedsExitCriteria(statistics, driverNode.Driver))
                    {
                        processedNodes.Add(selectedNode);
                        driverNode.Driver.EarliestStartTime = tempJobNode.WindowStart.Ticks;
                    }
                }
            }

            // create solution object, adjust start time
            var result = _routeService.CreateRouteSolution(driverNode, processedNodes);
            var driverStartTicks = Math.Max(driverNode.Driver.EarliestStartTime, startTime.Ticks);
            result.StartTime = TimeSpan.FromTicks(driverStartTicks);
            
            var s = string.Join("\t", result.Nodes.Select(f => "[" + f.Id + "]").ToArray());
            Console.WriteLine(s);
            Console.WriteLine(result.RouteStatistics.ToString());

            return result;
        }

        public IEnumerable<INode> SortINodes(IEnumerable<INode> nodes, Driver driver, bool reOrder)
        {
            // promote matching ports
            if (!driver.PortOfMiami)
            {
                nodes = nodes.Where(x => x.RouteStops.All(y => !y.Location.PortOfMiami));
            }
            if (!driver.PortEverglades)
            {
                nodes = nodes.Where(x => x.RouteStops.All(y => !y.Location.PortEverglades));
            }

            // sort by time
            if (reOrder)
            {
                nodes = nodes.OrderBy(x => x.WindowEnd - driver.EarliestStartTimeSpan).ToList();
            }

            return nodes;
        }

        public virtual NodeRouteSolution GenerateIterativeRouteSolution(IList<INode> nodes, DriverNode driverNode, bool reOrder = true)
        {
            IList<INode> processedNodes = new List<INode>();
            INode currentNode = driverNode;
            var startTime = driverNode.Driver.EarliestStartTimeSpan;
            var currentNodeEndTime = startTime;
            var cumulativeRouteStatistics = new RouteStatistics();

            nodes = SortINodes(nodes, driverNode.Driver, reOrder).ToList();

            int exitCounter = 0;
            while (exitCounter++ < 1000)
            {
                nodes = nodes.Where(x => !processedNodes.Select(y => y.Id).ToList().Contains(x.Id)).ToList();

                // getting avaiable nodes that have not been processed
                if (!nodes.Any())
                {
                    break;
                }

                var feasibleNodeTimingsList = SortINodes(nodes, driverNode.Driver, reOrder).ToList();

                var firstHitFeasibleNodeTiming = feasibleNodeTimingsList.FirstOrDefault(y => y.WindowEnd > currentNodeEndTime);

                if (firstHitFeasibleNodeTiming == null)
                {
                    break;
                }

                var firstHitFeasibleNodeTimingNode = firstHitFeasibleNodeTiming;

                var selectedNode = firstHitFeasibleNodeTimingNode;
                var selectedJobNode = selectedNode is JobNode ? selectedNode as JobNode : null;

                var processedRouteStops = processedNodes.SelectMany(x => x.RouteStops);
                var provisionalRouteStops = new Queue<RouteStop>();
                foreach (var processedRouteStop in processedRouteStops)
                {
                    provisionalRouteStops.Enqueue(processedRouteStop);
                }
                foreach (var routeStop in selectedNode.RouteStops)
                {
                    provisionalRouteStops.Enqueue(routeStop);
                }

                var tempJob = new Job
                {
                    RouteStops = provisionalRouteStops.Select(x => x).ToList(),
                    IsHazmat = selectedJobNode != null && selectedJobNode.Job != null ? selectedJobNode.Job.IsHazmat : false
                    
                };

                var tempJobNode = _jobNodeService.CreateJobNode(tempJob, driverNode.Driver.EarliestStartTimeSpan, false);

                if (!tempJobNode.IsInvalid)
                {
                    var serviceTimeIndex = _jobNodeService.GetMatrixIndex("ServiceTime");
                    var waitTimeIndex = _jobNodeService.GetMatrixIndex("WaitTime");
                    var travelTimeIndex = _jobNodeService.GetMatrixIndex("TravelTime");
                    var entryCount = tempJobNode.RouteStatisticsMatrix.GetLength(1);

                    var travelTime = TimeSpan.Zero;
                    var serviceTime = TimeSpan.Zero;
                    var waitTime = TimeSpan.Zero;

                    for (var i = 0; i < entryCount; i++)
                    {
                        travelTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[travelTimeIndex, i]);
                        serviceTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[serviceTimeIndex, i]);
                        waitTime += TimeSpan.FromSeconds(tempJobNode.RouteStatisticsMatrix[waitTimeIndex, i]);
                    }

                    var statistics = new RouteStatistics
                        {
                            TotalExecutionTime = serviceTime,
                            TotalIdleTime = waitTime,
                            TotalTravelTime = travelTime,
                            TotalQueueTime = TimeSpan.Zero
                        };
                    if (!_routeExitFunction.ExeedsExitCriteria(statistics, driverNode.Driver) &&
                         _routeExitFunction.MeetsPortCriteria(tempJobNode.RouteStops, driverNode.Driver) &&
                         PassesFilterCriteria(driverNode, tempJobNode, new List<INode>())
                        )
                    {
                        processedNodes.Add(selectedNode);
                        driverNode.Driver.EarliestStartTime = tempJobNode.WindowStart.Ticks;
                    }
                    else
                    {
                        nodes.RemoveAt(0);
                    }
                }
                else
                {
                    nodes.RemoveAt(0);
                }
            }

            // create solution object, adjust start time
            var result = _routeService.CreateRouteSolution(driverNode, processedNodes);
            var driverStartTicks = Math.Max(driverNode.Driver.EarliestStartTime, startTime.Ticks);
            result.StartTime = TimeSpan.FromTicks(driverStartTicks);

            var s = string.Join("\t", result.Nodes.Select(f => "[" + f.Id + "]").ToArray());
            Console.WriteLine(s);
            Console.WriteLine(result.RouteStatistics.ToString());

            return result;
        }

        public IList<int> GetInfeasibleJobIds(IList<Job> jobs, Driver driver)
        {
            Initialize();
            var result = new List<int>();

            var nodeRouteSolution = GenerateIterativeRouteSolution(jobs.Select(x => _jobNodeService.CreateJobNode(x)).Cast<INode>().ToList(), new DriverNode(driver), false);

            var feasibleIds = nodeRouteSolution.Nodes.Select(x => x.Id).ToList();

            var totalJobIds = jobs.Select(x => x.Id).ToList();

            var infeasibleIds = totalJobIds.Except(feasibleIds).ToList();

            result = infeasibleIds;
            return result;
        }

        public bool JobNodeTimings(IList<Job> jobs, Driver driver, out List<NodeTiming> nodeTimings, out IList<int> ints)
        {
            nodeTimings = new List<NodeTiming>();
            ints = new List<int>();
            if (driver == null || jobs == null || !jobs.Any())
            {
                {
                    return true;
                }
            }

            var jobNodes = jobs.Select((job, i) => _jobNodeService.CreateJobNode(job, null, i != 0)).Cast<INode>().ToList();
            var driverNode = new DriverNode(driver);
            var processedNodes = new List<INode>();
            var cumulativeRouteStatistics = new RouteStatistics();

            //var routeSolution = GenerateRouteSolution(jobNodes, driverNode);

            //GetFeasibleNodes(jobNodes, driverNode, processedNodes, )
            //var isFirstStop = processedNodes.Count == 0;
            //var currentNode = isFirstStop ? driverNode : processedNodes.Last();

            // first check feasibility for driver to first node
            var nodeTimingResult = _routeStatisticsService.GetNodeTiming(
                driverNode, jobNodes.FirstOrDefault(), driverNode.WindowStart, cumulativeRouteStatistics);


            if (!nodeTimingResult.IsFeasableTimeWindow)
            {
                // all are infeasible
                {
                    ints = jobNodes.Select(p => p.Id).ToList();
                    return true;
                }
            }

            nodeTimings = new List<NodeTiming>() {nodeTimingResult};

            for (int i = 1; i < jobNodes.Count; i++)
            {
                var currentNode = (JobNode) jobNodes[i];
                var lastNode = jobNodes[i - 1];
                var iterationStartTime = nodeTimingResult.EndExecutionTime;

                nodeTimingResult = _routeStatisticsService.GetNodeTiming(
                    lastNode, currentNode, iterationStartTime, cumulativeRouteStatistics);

                nodeTimingResult.CumulativeRouteStatistics = cumulativeRouteStatistics + nodeTimingResult.CumulativeRouteStatistics;

                if (_routeExitFunction.ExeedsExitCriteria(nodeTimingResult.CumulativeRouteStatistics, driverNode.Driver))
                {
                    nodeTimingResult.IsFeasableTimeWindow = false;
                }

                nodeTimings.Add(nodeTimingResult);
            }

            var lastLeg = _routeStatisticsService.GetRouteStatistics(nodeTimingResult.Node, driverNode, nodeTimingResult.EndExecutionTime);
            var finalRouteStatistics = nodeTimingResult.CumulativeRouteStatistics + lastLeg;
            if (_routeExitFunction.ExeedsExitCriteria(finalRouteStatistics, driverNode.Driver))
            {
                nodeTimingResult.IsFeasableTimeWindow = false;
            }

            nodeTimings.Add(nodeTimingResult);
            return false;
        }

        private bool IsExecutionTimeIgnored(INode currentStop, INode nextStop)
        {
            try
            {
                var start = currentStop.RouteStops.LastOrDefault();
                var end = nextStop.RouteStops.FirstOrDefault();

                if (start != null && end != null)
                {
                    if ((start.Location.DisplayName == end.Location.DisplayName) ||
                        (start.Location.Latitude == end.Location.Latitude &&
                         start.Location.Longitude == end.Location.Longitude))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }
        private bool PassesFilterCriteria(DriverNode driverNode, JobNode jobNode, List<INode> filteredAvailableNodes)
        {
            if (!driverNode.Driver.IsHazmat && jobNode.Job.IsHazmat)
            {
                return false;
            }

            if (!driverNode.Driver.IsFlatbed && jobNode.Job.IsFlatbed)
            {
                return false;
            }

            if (driverNode.Driver.Priority < jobNode.Job.Priority)
            {
                return false;
            }

            return true;
        }
         
        public virtual List<NodeTiming> GetFeasibleNodes(IList<INode> availableNodes, DriverNode driverNode, IList<INode> processedNodes,
            TimeSpan currentNodeEndTime, RouteStatistics cumulativeRouteStatistics)
        {
            var isFirstStop = (processedNodes.Count == 0);
            var currentNode = isFirstStop ? driverNode : processedNodes.Last();

            var filteredAvailableNodes = new List<INode>();

            var updatedNodes = new List<INode>();
            if (currentNode is JobNode)
            {
                // update nodes to adjust for potentially shortened execution time
                foreach (var n in availableNodes)
                {
                    if (n is JobNode)
                    {
                        var node = _jobNodeService.CreateJobNode(((JobNode) n).Job, currentNodeEndTime,
                            IsExecutionTimeIgnored(currentNode, n));
                        updatedNodes.Add(node);
                    }
                    else
                    {
                        updatedNodes.Add(n);
                    }
                }

                availableNodes = updatedNodes.ToList();
            }

            // filter available nodes based on predefined criteria
            foreach (var availableNode in availableNodes)
            {
                if (availableNode is JobNode)
                {
                    if (PassesFilterCriteria(driverNode, (JobNode) availableNode, filteredAvailableNodes))
                    {
                        filteredAvailableNodes.Add(availableNode);
                    }
                    else
                    {
                        ;
                    }
                }
                else
                {
                    filteredAvailableNodes.Add(availableNode);  // we are only filtering JobNodes
                }
            }

            availableNodes = filteredAvailableNodes.ToList();

            // get the node timings for all of the available nodes
            var unprocessedNodes = availableNodes.Where(x => !processedNodes.Select(y => y.Id).ToList().Contains(x.Id));

            unprocessedNodes = new List<INode>();
            foreach (var availableNode in availableNodes)
            {
                var processedNodesIds = processedNodes.Select(x => x.Id);
                if (!processedNodesIds.Contains(availableNode.Id))
                {
                    unprocessedNodes = unprocessedNodes.Concat(new List<INode> {availableNode});
                }
            }

            var nodeTimings = unprocessedNodes.Select(nextNode => 
                _routeStatisticsService.GetNodeTiming(currentNode, nextNode, currentNodeEndTime, cumulativeRouteStatistics));
                
            var feasibleNodes = new List<NodeTiming>();
            
            foreach (var nodeTiming in nodeTimings.Where(f => f.IsFeasableTimeWindow))
            {
                // calculate for return home to driver node
                var lastLeg = _routeStatisticsService.GetRouteStatistics(nodeTiming.Node, driverNode, nodeTiming.EndExecutionTime);
                var finalRouteStatistics = nodeTiming.CumulativeRouteStatistics + lastLeg;

                if (!_routeExitFunction.ExeedsExitCriteria(finalRouteStatistics, driverNode.Driver))
                {
                    feasibleNodes.Add(nodeTiming);
                }
            }

            return feasibleNodes;
        }
    }
}