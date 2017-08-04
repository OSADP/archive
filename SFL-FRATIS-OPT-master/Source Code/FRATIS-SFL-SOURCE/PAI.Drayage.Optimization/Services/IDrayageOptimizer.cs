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
using PAI.Drayage.Optimization.Common;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// The Route Optimizer Services
    /// </summary>
    public interface IDrayageOptimizer
    {
        /// <summary>The progress changed event handler for the Drayage / Route Optimization iterations</summary>
        event EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>Gets or sets a value indicating whether parallelism is enabled.</summary>
        bool EnableParallelism { get; set; }

        /// <summary>Gets or sets the pheromone update frequency.</summary>
        int PheromoneUpdateFrequency { get; set; }

        /// <summary>Gets or sets the max iterations.</summary>
        int MaxIterations { get; set; }

        /// <summary>Gets or sets the max iteration since best result.</summary>
        int MaxIterationSinceBestResult { get; set; }

        /// <summary>Gets or sets the max execution time.</summary>
        int MaxExecutionTime { get; set; }

        Solution CurrentBestSolution { get; set; }

        bool DisallowPlaceholderDriver { get; set; }

        /// <summary>
        /// Builds a solution using the Optimization Algorithm for 
        /// the provided drivers and jobs.
        /// </summary>
        /// <param name="drivers">The drivers.</param>
        /// <param name="defaultDriver">The default driver.</param>
        /// <param name="jobs">The jobs.</param>
        /// <returns>The <see cref="Solution"/>.</returns>
        Solution BuildSolution(IList<Driver> drivers, Driver defaultDriver, IList<Job> jobs);

        /// <summary>
        /// Initializes Optimizer
        /// </summary>
        void Initialize();

        /// <summary>
        /// Solution Updated Event
        /// </summary>
        event EventHandler<SolutionEventArgs> SolutionUpdated;

        /// <summary>
        /// Gets a list of Infeasible Jobs Ids considering travel time / window feasibility
        /// </summary>
        /// <param name="jobs"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        IList<int> GetInfeasibleJobIds(IList<Job> jobs, Driver driver);

        bool JobNodeTimings(IList<Job> jobs, Driver driver, out List<NodeTiming> nodeTimings, out IList<int> ints);
    }
}