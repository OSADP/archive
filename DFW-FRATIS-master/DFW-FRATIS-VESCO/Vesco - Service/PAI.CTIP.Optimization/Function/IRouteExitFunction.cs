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
using System.Linq;
using System;
using PAI.CTIP.Optimization.Model.Metrics;
using PAI.CTIP.Optimization.Model.Orders;

namespace PAI.CTIP.Optimization.Function
{
    public interface IRouteExitFunction
    {
        /// <summary>
        /// Returns true if the route statistics exceed our exit criteria
        /// </summary>
        /// <param name="routeStatistics"></param>
        /// <param name="driver"></param>
        /// <returns></returns>
        bool ExeedsExitCriteria(RouteStatistics routeStatistics, Driver driver);
    }
}