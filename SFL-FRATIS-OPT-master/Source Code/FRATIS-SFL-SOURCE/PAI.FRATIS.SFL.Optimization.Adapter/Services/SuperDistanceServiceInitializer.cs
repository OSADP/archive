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

using System.Collections.Generic;
using System.Linq;
using System;

using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Domain.Planning;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public class SuperDistanceServiceInitializer : IPlanGeneratorInitializer
    {
        private readonly ILocationDistanceService _locationDistanceService;

        public SuperDistanceServiceInitializer(ILocationDistanceService locationDistanceService)
        {
            _locationDistanceService = locationDistanceService;
        }

        public void Initialize(PlanConfig planConfig)
        {
            var locations = new List<PAI.FRATIS.SFL.Domain.Geography.Location>();
            var routeStops = planConfig.Jobs.Select(p => p.RouteStops).ToList();
            var locationsSet = new HashSet<Domain.Geography.Location>();

            foreach (var routeStopList in routeStops.ToList())
            {
                foreach (var rs in routeStopList)
                {
                    locationsSet.Add(rs.Location);
                }
            }

            foreach (var driver in planConfig.Drivers)
            {
                if (driver.StartingLocation != null)
                {
                    locationsSet.Add(driver.StartingLocation);
                }
            }
            
            _locationDistanceService.Prefetch(locationsSet.ToList());
        }
    }
}