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
using System.Linq;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Node;

namespace PAI.CTIP.Optimization.Services
{
    public interface INodeService
    {
        /// <summary>
        /// Gets the <see cref="NodeConnection"/> between two nodes
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <returns></returns>
        NodeConnection GetNodeConnection(INode startNode, INode endNode);

        /// <summary>
        /// Returns the <see cref="NodeTiming"/> for the next node
        /// </summary>
        /// <returns></returns>
        NodeTiming GetNodeTiming(INode startNode, INode endNode, TimeSpan currentNodeEndTime, RouteStatistics currentRouteStatistics);
    }
}
