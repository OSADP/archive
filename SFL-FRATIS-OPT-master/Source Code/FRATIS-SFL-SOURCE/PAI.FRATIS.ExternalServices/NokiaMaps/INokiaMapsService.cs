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
using PAI.FRATIS.ExternalServices.NokiaMaps.Model;

namespace PAI.FRATIS.ExternalServices.NokiaMaps
{
    /// <summary>The NokiaMapsService interface.</summary>
    public interface INokiaMapsService
    {
        /// <summary>The get route summary.</summary>
        /// <param name="pos1Lat">The pos 1 lat.</param>
        /// <param name="pos1Long">The pos 1 long.</param>
        /// <param name="pos2Lat">The pos 2 lat.</param>
        /// <param name="pos2Long">The pos 2 long.</param>
        /// <returns>The <see cref="Summary"/>.</returns>
        RouteSummary GetRouteSummary(double pos1Lat, double pos1Long, double pos2Lat, double pos2Long, DateTime? departureTime = null);

        TimeSpan? GetTravelTime(double pos1Lat, double pos1Long, double pos2Lat, double pos2Long, DateTime? departureTime = null);

        List<TrafficItem> GetIncidents(double pos1Lat, double pos1Long);

        string GetMapTileUrl(double latitude, double longitude, int levelOfDetail = 11);

        string GetMapTrafficTileUrl(double latitude, double longitude, int levelOfDetail = 11, DateTime? dt = null);

        string GetRouteMapTileUrl(double startLatitude, double startLongitude, double endLatitude, double endLongitude, int? width = null, int? height = null);
    }

    /// <summary>The nokia maps service.</summary>
    public class NokiaMapsService : INokiaMapsService
    {
        /// <summary>The get route summary.</summary>
        /// <param name="pos1Lat">The pos 1 lat.</param>
        /// <param name="pos1Long">The pos 1 long.</param>
        /// <param name="pos2Lat">The pos 2 lat.</param>
        /// <param name="pos2Long">The pos 2 long.</param>
        /// <returns>The <see cref="Summary"/>.</returns>
        public RouteSummary GetRouteSummary(double pos1Lat, double pos1Long, double pos2Lat, double pos2Long, DateTime? departureTime = null)
        {
            // TODO Urgent - hour of day
            var wrapper = new NokiaMapWrapper();
            return wrapper.GetRouteSummary(pos1Lat, pos1Long, pos2Lat, pos2Long, departureTime);
        }

        public List<TrafficItem> GetIncidents(double pos1Lat, double pos1Long)
        {
            var wrapper = new NokiaMapWrapper();
            var result = wrapper.GetIncidents(pos1Lat, pos1Long);
            return result.TrafficItem != null ? result.TrafficItem.ToList() : new List<TrafficItem>();
        }

        public string GetMapTileUrl(double latitude, double longitude, int levelOfDetail = 11)
        {
            var wrapper = new NokiaMapWrapper();
            return wrapper.GetMapTileUrl(latitude, longitude, levelOfDetail);
        }

        public string GetMapTrafficTileUrl(double latitude, double longitude, int levelOfDetail = 11, DateTime? dt = null)
        {
            var wrapper = new NokiaMapWrapper();
            return wrapper.GetTrafficMapTileUrl(latitude, longitude, levelOfDetail, dt);
        }

        public string GetRouteMapTileUrl(
            double startLatitude,
            double startLongitude,
            double endLatitude,
            double endLongitude,
            int? width = null,
            int? height = null)
        {
            var wrapper = new NokiaMapWrapper();
            return wrapper.GetRouteMapTileUrl(startLatitude, startLongitude, endLatitude, endLongitude, width, height);
        }

        public TimeSpan? GetTravelTime(double pos1Lat, double pos1Long, double pos2Lat, double pos2Long, DateTime? departureTime = null)
        {
            var summary = GetRouteSummary(pos1Lat, pos1Long, pos2Lat, pos2Long, departureTime);

            //var allowedDelay = summary.TravelTime.Value.TotalSeconds * .05;

            //if (summary.TrafficDelay.Value.TotalSeconds < allowedDelay)
            //{
            //    Console.WriteLine("No different for hour " + departureTime.Value.Hour);
            //}

            return summary.TrafficTravelTime;
        }

        
    }
}