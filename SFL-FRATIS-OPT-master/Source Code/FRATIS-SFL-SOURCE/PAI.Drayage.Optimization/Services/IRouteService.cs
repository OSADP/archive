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
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.Drayage.Optimization.Model.Node;
using PAI.Drayage.Optimization.Model.Orders;

namespace PAI.Drayage.Optimization.Services
{
    /// <summary>
    /// Interface for interacting with and between routes
    /// </summary>
    public interface IRouteService
    {
        ///// <summary>
        ///// Creates a route solution from a list of nodes
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <returns></returns>
        NodeRouteSolution CreateRouteSolution(DriverNode driverNode, IEnumerable<INode> nodes);

        ///// <summary>
        ///// Gets the best solution between a new list of <see cref="INode"/> and a current best <see cref="NodeRouteSolution"/>
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <param name="driverNode"> </param>
        ///// <param name="bestSolution"></param>
        ///// <returns>the best solution</returns>
        NodeRouteSolution GetBestFeasableSolution(IEnumerable<INode> nodes, DriverNode driverNode, NodeRouteSolution bestSolution);
        
        /// <summary>
        /// Gets a list of <see cref="RouteStop"/> for the given <see cref="NodeRouteSolution"/>
        /// </summary>
        /// <param name="routeSolution"></param>
        /// <returns></returns>
        IList<RouteStop> GetRouteStopsForRouteSolution(NodeRouteSolution routeSolution);        
        [Obsolete("Use IRouteStatisticsComparer")]
        NodeRouteSolution GetBestSolution(NodeRouteSolution left, NodeRouteSolution right);

        [Obsolete("Use IRouteStatisticsComparer")]
        Solution GetBestSolution(Solution left, Solution right);
    }
}