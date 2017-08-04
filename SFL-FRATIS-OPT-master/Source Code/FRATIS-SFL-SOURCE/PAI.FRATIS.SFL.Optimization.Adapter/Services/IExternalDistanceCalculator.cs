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
using System.Text;
using System.Threading.Tasks;

using PAI.FRATIS.SFL.Domain.Geography;

using LocationDistance = PAI.FRATIS.SFL.Domain.Geography.LocationDistance;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    /// <summary>The ExternalDistanceCalculator interface.</summary>
    public interface IExternalDistanceCalculator
    {
        /// <summary>The get.</summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="endLocation">The end location.</param>
        /// <returns>The <see cref="LocationDistance"/>.</returns>
        LocationDistance Get(Location startLocation, Location endLocation, DateTime departureTime, LocationDistance record = null);

        LocationDistance Get(LocationDistance locationDistance, DateTime departureTime, LocationDistance record = null);
    }
}
