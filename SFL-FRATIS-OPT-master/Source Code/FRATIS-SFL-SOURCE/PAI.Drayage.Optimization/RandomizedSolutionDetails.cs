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
using PAI.Drayage.Optimization.Model.Node;

namespace PAI.Drayage.Optimization
{
    public class RandomizedSolutionDetails
    {
        public IList<INode> Nodes { get; set; }

        public int TotalDriversAssignedJobs { get; set; }

        public IList<KeyValuePair<DriverNode, IList<JobNode>>> DriverJobs { get; set; }

        public RandomizedSolutionDetails(IList<INode> nodes)
        {
            ProcessNodes(nodes, true);
        }

        public RandomizedSolutionDetails()
        {
            Clear();
        }

        public static RandomizedSolutionDetails GetCopyOf(RandomizedSolutionDetails randomizedSolution)
        {
            return new RandomizedSolutionDetails(randomizedSolution.Nodes.ToList());
        }
        private void ProcessNodes(IList<INode> nodes, bool clearPreviousNodes = true)
        {
            if (clearPreviousNodes)
            {
                Clear();
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node is DriverNode)
                {
                    var driver = node as DriverNode;
                    var driverJobs = new List<JobNode>();
                    for (var ii = i + 1; ii < nodes.Count; ii++)
                    {
                        var curNode = nodes[ii];
                        if (curNode is DriverNode)
                        {
                            i = ii - 1;
                            break;
                        }
                        else
                        {
                            var driverJob = curNode as JobNode;
                            driverJobs.Add(driverJob);                            
                        }
                    }

                    if (driverJobs.Count > 0)
                    {
                        AddJob(driver, driverJobs);
						Nodes.Add(driver);

						foreach (var n in driverJobs)
						{
						    Nodes.Add(n);
						}
                    }
                }

            }

        }

        public void AddJob(DriverNode driver, IList<JobNode> jobs)
        {
            if (DriverJobs != null)
            {
                DriverJobs.Add(new KeyValuePair<DriverNode, IList<JobNode>>(driver, jobs));
            }
        }

        public void Clear()
        {
            Nodes = new List<INode>();
            DriverJobs = new List<KeyValuePair<DriverNode, IList<JobNode>>>();
        }
    }
}
