//    Copyright 2013 Productivity Apex Inc.
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
using System.Threading;
using System.Threading.Tasks;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.Core;

namespace PAI.CTIP.Optimization.Services
{
    public class DrayageOptimizer : IDrayageOptimizer
    {
        protected readonly ILogger _logger;
        protected readonly IRouteService _routeService;
        protected readonly INodeService _nodeService;
        protected readonly IPheromoneMatrix _pheromoneMatrix;
        protected readonly IRouteStopService _routeStopService;
        protected readonly IRouteExitFunction _routeExitFunction;
        protected readonly IProbabilityMatrix _probabilityMatrix;
        protected readonly IRandomNumberGenerator _randomNumberGenerator;
        private readonly INodeFactory _nodeFactory;

        protected readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
        protected bool _isCancelling = false;
        private bool _initialized = false;

        public bool EnableParallelism { get; set; }
        public int PheromoneUpdateFrequency { get; set; }
        public int MaxIterations { get; set; }
        public int MaxIterationSinceBestResult { get; set; }
        public int MaxExecutionTime { get; set; }
        
        public DrayageOptimizer(IProbabilityMatrix probabilityMatrix,
            IRouteService routeService, INodeService nodeService, IRouteStopService routeStopService,
            IRouteExitFunction routeExitFunction, ILogger logger, IPheromoneMatrix pheromoneMatrix, IRandomNumberGenerator randomNumberGenerator, INodeFactory nodeFactory)
        {
            _probabilityMatrix = probabilityMatrix;
            _routeStopService = routeStopService;
            _routeExitFunction = routeExitFunction;
            _logger = logger;
            _pheromoneMatrix = pheromoneMatrix;
            _randomNumberGenerator = randomNumberGenerator;
            this._nodeFactory = nodeFactory;
            _nodeService = nodeService;
            _routeService = routeService;

            // default values
            EnableParallelism = false;
            PheromoneUpdateFrequency = 10;
            MaxIterations = 10000;
            MaxIterationSinceBestResult = 1000;
            MaxExecutionTime = 10000;
        }

        public virtual void Cancel()
        {
            _rwLock.EnterWriteLock();

            _isCancelling = true;
            
            _rwLock.ExitWriteLock();
        }

        /// <summary>
        /// Initializes Optimizer
        /// </summary>
        public virtual void Initialize()
        {
            _initialized = true;
        }


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
        /// <param name="drivers"></param>
        /// <param name="defaultDriver"> </param>
        /// <param name="jobs"></param>
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

            _isCancelling = false;

            // clear pheromone matrix
            _pheromoneMatrix.Clear();

            // Create driver nodes

            var driverNodes = drivers.Select(driver => new DriverNode(driver))
                .OrderBy(f => _randomNumberGenerator.Next())
                .ToList();

            foreach (var driverNode in driverNodes)
            {
                driverNode.Priority = 1; // init priority
            }

            // Create job nodes

            foreach (var job in jobs)
            {
                switch (job.OrderType)
                {
                    case 3:
                        job.Priority = 1;
                        break;
                    case  2:
                        job.Priority = 2;
                        break;
                    case 1:
                        job.Priority = 3;
                        break;
                    default:
                        job.Priority = 1;
                        break;
                }
            }

            var jobNodes = new List<JobNode>();
            foreach (var job in jobs.OrderBy(f => _randomNumberGenerator.Next()))
            {
                var jobNode = _nodeFactory.CreateJobNode(job);  // create node, set priority
                jobNode.RouteStatistics = _routeStopService.CalculateRouteStatistics(jobNode.RouteStops);
                jobNodes.Add(jobNode);
            }

            // create dummy drivers (for worst case scenario)
            var dummyDriversToCreate = jobNodes.Count() - driverNodes.Count();
            if (dummyDriversToCreate > 0)
            {
                var dummyDriverNode = new DriverNode(defaultDriver);
                for (int i = 0; i < dummyDriversToCreate; i++)
                {
                    driverNodes.Add(dummyDriverNode);
                }
            }
            
            // build solution
            var bestSolution = BuildSolution(defaultDriver, jobNodes.Cast<INode>().ToList(), driverNodes);
            
            _isCancelling = false;

            return bestSolution;
        }

        public virtual Solution BuildSolution(Driver dummyDriver, IList<INode> jobNodes, IList<DriverNode> driverNodes)
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
                    var solution = ProcessDrivers(driverNodes, dummyDriver, jobNodes);
                    
                    _rwLock.EnterWriteLock();
                    {
                        totalIterations++;
                        iterationsSinceBestResult++;
                        subSolutions.Add(solution);

                        // Update progress
                        OnProgressEvent(new ProgressEventArgs() { PrimaryTotal = MaxIterations, PrimaryValue = totalIterations });

                        var oldBest = bestSolution;

                        bestSolution = bestSolution == null ? solution : _routeService.GetBestSolution(bestSolution, solution);

                        if(bestSolution != oldBest)
                        {
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
                            if (loopState != null)
                                loopState.Stop();
                        }

                    }
                    _rwLock.ExitWriteLock();

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

            stopWatch.Stop();

            // Run iterations
            if (EnableParallelism)
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

            return bestSolution;
        }

        /// <summary>
        /// Creates our route assignments by processing all the drivers and getting their routes
        /// </summary>
        /// <returns></returns>
        public virtual Solution ProcessDrivers(IList<DriverNode> driverNodes, Driver dummyDriver, IList<INode> jobNodes)
        {
            var solution = new Solution();
            var availableNodes = jobNodes;

            // going through all the drivers
            foreach (var driverNode in driverNodes)
            {
                if (availableNodes.Count == 0)
                    break;

                // get the route solution for this particular driver
                var nodeRouteSolution = GenerateRouteSolution(availableNodes, driverNode);
                    
                if (driverNode.Driver == dummyDriver || nodeRouteSolution.Nodes.Count == 0)
                    break;
                    
                // insert solution
                solution.RouteSolutions.Add(nodeRouteSolution);

                // update available nodes based on new solution
                availableNodes = availableNodes.Where(n => !nodeRouteSolution.Nodes.Contains(n)).ToList();
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
            var startTime = driverNode.Driver.EarliestStartTime;
            var currentNodeEndTime = startTime;
            var cumulativeRouteStatistics = new RouteStatistics();

            int exitCounter = 0;
            while (exitCounter++ < 1000)
            {
                // getting avaiable nodes that have not been processed
                var feasibleNodeTimings = GetFeasibleNodes(nodes, driverNode, processedNodes, currentNodeEndTime, cumulativeRouteStatistics);

                if (!feasibleNodeTimings.Any())
                    break;
                
                var feasibleNodeTimingsByNode = feasibleNodeTimings.ToDictionary(f => f.Node);

                // build probability matrix for the available nodes
                var probabilityData = _probabilityMatrix.BuildProbabilityDataMatrix(currentNode, feasibleNodeTimings);

                // find a suitable node based on the cumulative probability
                var selectedNode = (INode) _probabilityMatrix.GetNominatedElement(probabilityData);
                processedNodes.Add(selectedNode);
                currentNode = selectedNode;
                
                // now we update the current node's end time
                var selectedNodeTiming = feasibleNodeTimingsByNode[selectedNode];

                if (processedNodes.Count == 1 && selectedNodeTiming.DepartureTime != currentNodeEndTime)
                {
                    startTime = selectedNodeTiming.DepartureTime;
                }

                currentNodeEndTime = selectedNodeTiming.EndExecutionTime;
                cumulativeRouteStatistics = selectedNodeTiming.CumulativeRouteStatistics;
            }

            // create solution object
            var result = _routeService.CreateRouteSolution(processedNodes, driverNode);
            result.StartTime = startTime;
            return result;
        }


        private bool PassesFilterCriteria(DriverNode driverNode, JobNode jobNode)
        {
            if (!driverNode.Driver.IsHazmatEligible && jobNode.Job.IsHazmat)
            {
                return false;
            }

            if (!driverNode.Driver.IsShortHaulEligible && jobNode.Job.IsShortHaul)
            {
                return false;
            }

            if (!driverNode.Driver.IsLongHaulEligible && jobNode.Job.IsLongHaul)
            {
                return false;
            }

            if (driverNode.Driver.OrderType > 0 && driverNode.Driver.OrderType > jobNode.Job.OrderType)
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

            if (currentNode is JobNode)
            {
                ;
            }
            var filteredAvailableNodes = new List<INode>();

            // filter available nodes based on predefined criteria
            foreach (var availableNode in availableNodes)
            {
                if (availableNode is JobNode)
                {
                    //if (PassesFilterCriteria(driverNode, (JobNode)availableNode))
                    {
                        filteredAvailableNodes.Add(availableNode);
                    }
                }
                else
                {
                    filteredAvailableNodes.Add(availableNode);  // we are only filtering JobNodes
                }
            }

            availableNodes = filteredAvailableNodes.ToList();

            // get the node timings for all of the available nodes
            var nodeTimings = availableNodes.Except(processedNodes)
                .Select(nextNode => _nodeService.GetNodeTiming(currentNode, nextNode, currentNodeEndTime, cumulativeRouteStatistics))
                .ToList();

            var feasibleNodes = new List<NodeTiming>();
            foreach (var nodeTiming in nodeTimings.Where(f => f.IsFeasableTimeWindow))
            {
                var finalConnection = _nodeService.GetNodeConnection(nodeTiming.Node, driverNode);
                var finalRouteStatistics = nodeTiming.CumulativeRouteStatistics + finalConnection.RouteStatistics;

                if (!_routeExitFunction.ExeedsExitCriteria(finalRouteStatistics, driverNode.Driver))
                {
                    feasibleNodes.Add(nodeTiming);
                }
            }

            return feasibleNodes;
        }
    }

}