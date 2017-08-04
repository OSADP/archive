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
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model.Metrics;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Optimization.Adapter.Mapping;

using Location = PAI.Drayage.Optimization.Model.Location;

namespace PAI.FRATIS.SFL.Optimization.Adapter.Services
{
    public class SuperDistanceService : IDistanceService
    {
        private readonly IDistanceService _fallbackDistanceService;
        private readonly IMapperService _mapperService;
        private readonly ILocationDistanceService _locationDistanceService;

        public SuperDistanceService(IMapperService mapperService, ILocationDistanceService locationDistanceService, IDistanceService fallbackDistanceService)
        {
            _mapperService = mapperService;
            _locationDistanceService = locationDistanceService;
            _fallbackDistanceService = fallbackDistanceService;
        }

        public TripLength CalculateDistance(Location startLocation, Location endLocation)
        {
            return CalculateDistance(startLocation, endLocation, TimeSpan.Zero);
        }

        public TripLength CalculateDistance(Location startLocation, Location endLocation, TimeSpan startTime)
        {
            var startLocationDomain = _mapperService.MapModelToDomain(startLocation);
            startLocationDomain.Latitude = startLocation.Latitude;
            startLocationDomain.Longitude = startLocation.Longitude;
            var endLocationDomain = _mapperService.MapModelToDomain(endLocation);
            endLocationDomain.Latitude = endLocation.Latitude;
            endLocationDomain.Longitude = endLocation.Longitude;
            
            var locationDistance = _locationDistanceService.Get(startLocationDomain, endLocationDomain);

            TripLength result;

            if (locationDistance != null)
            {
                var hourIndex = (int)startTime.TotalHours;

                while (hourIndex >= 24)
                {
                    hourIndex = hourIndex - 24;
                }

                long travelTime = locationDistance.Hours[hourIndex].TravelTime.Value;
                if (travelTime == 0 && locationDistance.TravelTime.HasValue)
                {
                    travelTime = locationDistance.TravelTime.Value;
                }

                long distance = 0;
                if (locationDistance.Distance.HasValue)
                    distance = (long)locationDistance.Distance.Value;

                result = new TripLength(distance, TimeSpan.FromSeconds(travelTime));
            }
            else
            {
                // Use fallback service
                result = _fallbackDistanceService.CalculateDistance(startLocation, endLocation, startTime);
            }

            return result;
        }
    }
}
