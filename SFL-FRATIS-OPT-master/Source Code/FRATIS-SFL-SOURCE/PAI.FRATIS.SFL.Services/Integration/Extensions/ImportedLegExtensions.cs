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
using PAI.FRATIS.ExternalServices.NokiaMaps.Model.TrafficItems;
using Location = PAI.FRATIS.SFL.Domain.Geography.Location;

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    public static class ImportedLegExtensions
    {
        public static bool CompanyNameContains(this ImportedLeg leg, IList<string> namesToContain)
        {
            return leg != null && namesToContain.All(name => leg.CompanyName.ToLower().Contains(name.ToLower()));
        }


        public static bool IsMiamiRail(this ImportedLeg leg)
        {
            return leg.CompanyNameContains(new List<string>() {"FEC", "MIAMI"});
        }


        public static bool IsRail(this ImportedLeg leg)
        {
            return leg.CustomerNumber == "0" || leg.CustomerNumber == "3272";
        }

        public static bool CompanyNameDoesNotContain(this ImportedLeg leg, IList<string> namesToNotContain)
        {
            return !CompanyNameContains(leg, namesToNotContain);
        }

        public static bool MatchesZipRange(this ImportedLeg leg)
        {
            return leg.CompanyZipInt > 0 && ValidValues.IsValidZipCode(leg.CompanyZipInt);
        }


        public static bool ZoneIs(this ImportedLeg leg, string zone)
        {
            zone = zone.ToLower();
            return leg.DestinationZone.ToLower() == zone || leg.OriginZone == zone;
        }

        public static bool OriginZoneIs(this ImportedLeg leg, string zone)
        {
            zone = zone.ToLower();
            return leg.OriginZone == zone;
        }

        public static bool DestinationZoneIs(this ImportedLeg leg, string zone)
        {
            zone = zone.ToLower();
            return leg.DestinationZone.ToLower() == zone;
        }

        public static Location GetLocation(this ImportedLeg leg)
        {
            return new Location()
            {
                DisplayName = leg.CompanyName,
                LegacyId = leg.CustomerNumber,
                StreetAddress = leg.CompanyAddress1,
                City = leg.CompanyCity,
                State = leg.CompanyState,
                Zip = leg.CompanyZip
            };

        }
    }
}