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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Geography
{
    public class CachedDistanceService : IDistanceService
    {
        private readonly IDistanceService _distanceService;

        private readonly ConcurrentDictionary<int, TripLength> _dictionary; 

        public CachedDistanceService(IDistanceService distanceService)
        {
            _distanceService = distanceService;
            _dictionary = new ConcurrentDictionary<int, TripLength>();
        }

        public TripLength CalculateDistance(Location startLocation, Location endLocation)
        {
            var result = TripLength.Zero;

            try
            {
                if (!startLocation.Equals(endLocation))
                {
                    var locationsTuple = GetLocationsTuple(startLocation, endLocation);
                    var hash = locationsTuple.GetHashCode();

                    result = _dictionary.GetOrAdd(
                        hash, i => _distanceService.CalculateDistance(startLocation, endLocation));
                }
            }
            catch
            {
                result = TripLength.Zero;
            }

            return result;
        }

        /// <summary>
        /// Calculates the trip length
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <param name="startTime"></param>
        /// <returns></returns>
        public TripLength CalculateDistance(Location startLocation, Location endLocation, TimeSpan startTime)
        {
            var result = TripLength.Zero;
            
            try
            {
                if (!startLocation.Equals(endLocation))
                {
                    var locationsTuple = GetLocationsTuple(startLocation, endLocation);
                    var hash = locationsTuple.GetHashCode();

                    result = _dictionary.GetOrAdd(
                        hash, i => _distanceService.CalculateDistance(startLocation, endLocation, startTime));
                }
            }
            catch
            {
                result = TripLength.Zero;
            }

            return result;
        }

        /// <summary>
        /// Gets a Tuple in the pre-defined required sequence
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        private Tuple<Location, Location> GetLocationsTuple(Location startLocation, Location endLocation)
        {
            var result = new Tuple<Location, Location>(startLocation, endLocation);
            
            if (startLocation.CompareTo(endLocation) < 1)
            {
                result = new Tuple<Location, Location>(endLocation, startLocation);
            }

            return result;
        }

    }
}