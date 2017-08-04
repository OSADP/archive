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
using PAI.FRATIS.ExternalServices.NokiaMaps.Model.TrafficItems;
using PAI.FRATIS.SFL.Services.Geography;

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    public static class GeocodeExtensions
    {
        public static bool IsSameAs(this GeocodeResult result, GeocodeResult compareResult)
        {
            return Math.Round(result.Latitude, 4) == Math.Round(compareResult.Latitude, 4) &&
                   Math.Round(result.Longitude, 4) == Math.Round(compareResult.Longitude, 4);
        }

        public static void SaveTo(this GeocodeResult geocodeResult, Domain.Geography.Location location)
        {
            location.Latitude = geocodeResult.Latitude;
            location.Longitude = geocodeResult.Longitude;
        }
    }
}