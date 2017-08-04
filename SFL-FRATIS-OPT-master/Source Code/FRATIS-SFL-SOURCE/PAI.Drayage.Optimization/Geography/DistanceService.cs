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
using PAI.Drayage.Optimization.Model;
using PAI.Drayage.Optimization.Model.Metrics;

namespace PAI.Drayage.Optimization.Geography
{
    public class DistanceService : IDistanceService
    {
        private const double EarthRadius = 3963.2;

        private readonly ITravelTimeEstimator _travelTimeEstimator;

        public DistanceService(ITravelTimeEstimator travelTimeEstimator)
        {
            _travelTimeEstimator = travelTimeEstimator;
        }

        /// <summary>
        /// calculates the distance using the average of the taxicab and euclidian distances between the locations
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        public TripLength CalculateDistance(Location startLocation, Location endLocation)
        {
            if (startLocation == null)
            {
                throw new ArgumentNullException("startLocation");
            }
            if (endLocation == null) throw new ArgumentNullException("endLocation");

            if (startLocation.Latitude == 0 && startLocation.Longitude == 0) return new TripLength(0, TimeSpan.Zero);
            if (endLocation.Latitude == 0 && endLocation.Longitude == 0) return new TripLength(0, TimeSpan.Zero);

            var tDistance = CalculateTaxicabDistance(startLocation, endLocation);
            var eDistance = CalculateEuclidianDistance(startLocation, endLocation);
            var totalDistance = tDistance + eDistance;
            return new TripLength(totalDistance.Distance / 2, new TimeSpan(totalDistance.Time.Ticks / 2));
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
            return CalculateDistance(startLocation, endLocation);
        }
        
        /// <summary>
        /// Convert degrees to Radians
        /// </summary>
        /// <param name="x">Degrees</param>
        /// <returns>The equivalent in radians</returns>
        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        /// <summary>
        /// Calculates the taxi cab trip length
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        public TripLength CalculateTaxicabDistance(Location startLocation, Location endLocation)
        {
            double lon1 = startLocation.Longitude;
            double lat1 = startLocation.Latitude;
            double lon2 = endLocation.Longitude;
            double lat2 = endLocation.Latitude;

            double maxLat = Math.Max(lat1, lat2);
            double k = Math.Cos(Radians(maxLat));
            var distance = 69.172 * (Math.Abs(lat2 - lat1) + k * Math.Abs(lon2 - lon1));

            var result = new TripLength()
            {
                Distance = (decimal)distance,
                Time = _travelTimeEstimator.CalculateTravelTime(distance)
            };

            return result;
        }

        /// <summary>
        /// calculates the euclidian trip length
        /// </summary>
        /// <param name="startLocation"></param>
        /// <param name="endLocation"></param>
        /// <returns></returns>
        public TripLength CalculateEuclidianDistance(Location startLocation, Location endLocation)
        {
            double lon1 = startLocation.Longitude;
            double lat1 = startLocation.Latitude;
            double lon2 = endLocation.Longitude;
            double lat2 = endLocation.Latitude;

            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2)) +
                       Math.Cos(Radians(lat1)) * Math.Cos(Radians(lat2)) * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));
            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = angle * EarthRadius;

            var result = new TripLength()
            {
                Distance = (decimal)distance,
                Time = _travelTimeEstimator.CalculateTravelTime(distance)
            };

            return result;
        }
    }
}
