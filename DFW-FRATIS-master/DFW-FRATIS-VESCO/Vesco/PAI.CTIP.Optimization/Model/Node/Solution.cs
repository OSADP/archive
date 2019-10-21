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
using PAI.CTIP.Optimization.Model.Metrics;

namespace PAI.CTIP.Optimization.Model.Node
{
    public class Solution
    {

        public List<NodeRouteSolution> RouteSolutions { get; set; }

        public List<INode> UnassignedJobNodes { get; set; }
        
        public Solution()
        {
            RouteSolutions = new List<NodeRouteSolution>();
            UnassignedJobNodes = new List<INode>();
        }

        public RouteStatistics RouteStatistics
        {
            get
            {
                var result = new RouteStatistics();

                foreach (var solution in RouteSolutions)
                {
                    result += solution.RouteStatistics;
                }

                return result;
            }
        }

        public Solution Clone()
        {
            var clone = new Solution();
            foreach (var routeSolution in RouteSolutions)
            {
                clone.RouteSolutions.Add(routeSolution.Clone());
            }
            return clone;
        }
    }
}
