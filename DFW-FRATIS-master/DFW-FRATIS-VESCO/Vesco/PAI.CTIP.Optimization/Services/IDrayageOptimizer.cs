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
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public interface IDrayageOptimizer
    {

        Solution BuildSolution(IList<Driver> drivers, Driver defaultDriver, IList<Job> jobs);
        
        bool EnableParallelism { get; set; }
        int PheromoneUpdateFrequency { get; set; }
        int MaxIterations { get; set; }
        int MaxIterationSinceBestResult { get; set; }
        int MaxExecutionTime { get; set; }

        event EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Initializes Optimizer
        /// </summary>
        void Initialize();
    }
}
