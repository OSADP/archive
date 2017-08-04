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

using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Reporting.Model;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Planning;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    /// <summary>
    /// The Plan Generator interface
    /// </summary>
    public interface IPlanGenerator
    {
        /// <summary>
        /// Generates a plan
        /// </summary>
        /// <param name="planConfig"></param>
        /// <returns>A <see cref="PlanGenerationResult"/></returns>
        PlanGenerationResult GeneratePlan(Domain.Planning.PlanConfig planConfig);

        /// <summary>The recalculate plan statistics operation for the optimization plan model.</summary>
        /// <param name="plan">The plan.</param>
        void RecalculatePlanStatistics(Drayage.Optimization.Model.Planning.Plan plan);

        /// <summary>
        /// Recalculates the statistics for the given domain plan
        /// </summary>
        /// <param name="plan"></param>
        void RecalculatePlanStatistics(Domain.Planning.Plan plan);

        void RecalculatePlanStatisticsWithoutPlaceHolder(FRATIS.SFL.Domain.Planning.Plan plan);

        /// <summary>
        /// Gets a Solution from the persisted Plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        Solution GetSolutionFromPlan(Plan plan);

        /// <summary>
        /// Gets Solution Performance statistics from a persisted Plan
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        SolutionPerformanceStatistics GetSolutionPerformanceStatistics(Plan plan);

        /// <summary>The get infeasible job ids.</summary>
        /// <param name="jobs">The jobs.</param>
        /// <param name="driver">The driver.</param>
        /// <returns>The <see cref="IList"/>.</returns>
        IList<int> GetInfeasibleJobIds(IList<Job> jobs, Driver driver, DateTime? planDate);

        Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Plan plan);

        Dictionary<int, List<string>> GetDriverPlanVerboseRoutes(Dictionary<PlanDriver, IList<PAI.Drayage.Optimization.Model.Metrics.RouteSegmentStatistics>> statisticsByNodeRouteSolutionDictionary);

    }
}