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
using PAI.CTIP.Optimization.Model.Node;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Services
{
    public interface IRouteService
    {
        /// <summary>
        /// Calculates the trip length between two nodes
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        RouteStatistics CalculateRouteStatistics(INode origin, INode destination);
        
        ///// <summary>
        ///// Creates a route solution from a list of nodes
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <returns></returns>
        NodeRouteSolution CreateRouteSolution(IEnumerable<INode> nodes, DriverNode driverNode);

        ///// <summary>
        ///// Gets the best solution between a new list of <see cref="INode"/> and a current best <see cref="NodeRouteSolution"/>
        ///// </summary>
        ///// <param name="nodes"></param>
        ///// <param name="driverNode"> </param>
        ///// <param name="bestSolution"></param>
        ///// <returns>the best solution</returns>
        NodeRouteSolution GetBestFeasableSolution(IList<INode> nodes, DriverNode driverNode, NodeRouteSolution bestSolution);
        
        NodeRouteSolution GetBestSolution(NodeRouteSolution left, NodeRouteSolution right);

        Solution GetBestSolution(Solution left, Solution right);
        
        IList<RouteStop> GetRouteStopsForRouteSolution(NodeRouteSolution routeSolution);
    }
}