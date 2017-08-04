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

using PAI.FRATIS.SFL.Domain.Geography;

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    public static class DomainCompareExtensions
    {
        public static void MapTo(this Location originalLocation, Location targetLocation)
        {
            if (targetLocation != null)
            {
                targetLocation.SubscriberId =originalLocation.SubscriberId;
                targetLocation.LegacyId=originalLocation.LegacyId;
                targetLocation.StreetAddress=originalLocation.StreetAddress;
                targetLocation.City=originalLocation.City;
                targetLocation.State=originalLocation.State;
                targetLocation.Zip=originalLocation.Zip;
                targetLocation.IgnoreLocationChanges=originalLocation.IgnoreLocationChanges;
                targetLocation.Latitude=originalLocation.Latitude;
                targetLocation.Longitude=originalLocation.Longitude;
                targetLocation.DisplayName=originalLocation.DisplayName;
            }
        }

        public static bool IsChangedFrom(this Location originalLocation, Location location)
        {
            if (location.IgnoreLocationChanges)
            {
                return false;
            }

            if (originalLocation.StreetAddress != location.StreetAddress ||
                originalLocation.City != location.City ||
                originalLocation.State != location.State ||
                originalLocation.Zip != location.Zip)
            {
                return true;
            }

            if (!originalLocation.Latitude.HasValue && location.Latitude.HasValue ||
                !originalLocation.Longitude.HasValue && location.Longitude.HasValue)
            {
                return true;
            }

            if ((originalLocation.Latitude.HasValue && location.Latitude.HasValue && originalLocation.Latitude.Value != location.Latitude.Value) ||
                originalLocation.Longitude.HasValue && location.Longitude.HasValue && originalLocation.Longitude.Value != location.Longitude.Value)
            {
                return true;
            }

            return false;
        }
    }
}