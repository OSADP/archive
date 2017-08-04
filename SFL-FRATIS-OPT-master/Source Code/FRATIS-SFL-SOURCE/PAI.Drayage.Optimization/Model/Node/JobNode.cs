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
using System.Linq;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Model.Node
{
    /// <summary>
    /// Represents a Job Node
    /// </summary>
    public class JobNode : NodeBase
    {
        /// <summary>
        /// Gets or sets the job.
        /// </summary>
        /// <value>
        /// The job.
        /// </value>
        public Job Job { get; set; }

        public bool IsInvalid { get; set; }

        public double[,] RouteStatisticsMatrix { get; private set; }

        public RouteStatistics RouteStatistics { get; private set; }

        public JobNode(Job job)
        {
            Job = job;
            Id = job.Id;
            RouteStops = Job.RouteStops.ToList();

            if(RouteStops.Count > 0)
            {
                // This assumes that the internal time widows of the job are feasable
                var firstStop = RouteStops.First();
                RouteStop secondStop = null;
                if (RouteStops.Count > 1)
                {
                    secondStop = RouteStops[1];
                }

                WindowStart = firstStop.WindowStart;
                WindowEnd = secondStop != null 
                    && secondStop.WindowStart < firstStop.WindowEnd ? secondStop.WindowStart : firstStop.WindowEnd;
            }
        }

        public JobNode(Job job, double[,] statisticsMatrix) : this(job)
        {
            RouteStatisticsMatrix = statisticsMatrix;
            RouteStatistics = new RouteStatistics
                {
                    DriversWithAssignments = 1,
                    PriorityValue = job.Priority,
                    TotalCapacity = 0,
                    //TotalExecutionTime = statisticsMatrix[]
                };


        }

        public override string ToString()
        {
            return Job.DisplayName;
        }
    }
}