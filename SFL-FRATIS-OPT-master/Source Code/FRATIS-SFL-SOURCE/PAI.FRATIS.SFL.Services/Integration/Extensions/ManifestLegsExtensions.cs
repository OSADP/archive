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
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Geography;

namespace PAI.FRATIS.SFL.Services.Integration.Extensions
{
    public static class ManifestLegsExtensions
    {
        public static int AllLegsGetNextStopInZone(this ManifestLegs manifestLegs, int startIndex = 0)
        {
            for (int i = startIndex; i < manifestLegs.AllLegs.Count; i++)
            {
                if (ValidValues.IsValidZipCode(manifestLegs.AllLegs[i].CompanyZipInt))
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool AreFollowingStopsInZone(this ManifestLegs manifestLegs, int startIndex, bool allStopsInZoneRequired, bool failIfNoFollowingStops)
        {
            var result = !failIfNoFollowingStops;
            for (int i = startIndex; i < manifestLegs.AllLegs.Count; i++)
            {
                var leg = manifestLegs.AllLegs[i];
                var isInZone = ValidValues.IsValidZipCode(leg.CompanyZipInt);

                if (!allStopsInZoneRequired && isInZone)
                {
                    return true;
                }
                
                if (allStopsInZoneRequired && !isInZone)
                {
                    return false;
                }

                if (isInZone)
                {
                    result = true;
                }
            }

            return result;
        }

        public static bool ArePreviousStopsInZone(this ManifestLegs manifestLegs, int startIndex, bool allStopsInZoneRequired, bool failIfNoPreviousStops)
        {
            var result = !failIfNoPreviousStops;
            for (int i = startIndex; i >= 0; i--)
            {
                var leg = manifestLegs.AllLegs[i];
                var isInZone = ValidValues.IsValidZipCode(leg.CompanyZipInt);

                if (!allStopsInZoneRequired && isInZone)
                {
                    return true;
                }

                if (allStopsInZoneRequired && !isInZone)
                {
                    return false;
                }

                if (isInZone)
                {
                    result = true;
                }
            }

            return result;
        }

        public static int GetAllLegsIndexOfMiamiRail(this ManifestLegs manifestLegs)
        {
            for (int i = 0; i < manifestLegs.AllLegs.Count; i++)
            {
                var leg = manifestLegs.AllLegs[i];
                if (leg.CompanyNameContains(new List<string>() { "FEC", "MIAMI" }))
                {
                    return i;
                }
            }
            return -1;
        }

        public static ImportedLeg GetMiamiRailLegAfterCustomers(this ManifestLegs manifestLegs)
        {
            bool foundCustomer = false;
            foreach (var leg in manifestLegs.AllLegs)
            {
                if (!leg.IsRail() && ValidValues.IsValidZipCode(leg.CompanyZipInt))
                {
                    foundCustomer = true;
                    continue;
                }

                if (foundCustomer && leg.IsMiamiRail())
                {
                    return leg;
                }
            }

            return null;
        }

        public static ImportedLeg GetMiamiRailLegBeforeCustomers(this ManifestLegs manifestLegs)
        {
            bool foundCustomer = false;
            for (int i = manifestLegs.AllLegs.Count - 1; i >= 0; i--)
            {
                var leg = manifestLegs.AllLegs[i];
                if (!leg.IsRail() && ValidValues.IsValidZipCode(leg.CompanyZipInt))
                {
                    foundCustomer = true;
                    continue;
                }

                if (foundCustomer && leg.IsMiamiRail())
                {
                    return leg;
                }
            }

            return null;
        }

        public static int GetLegsIndexOfMiamiRail(this ManifestLegs manifestLegs)
        {
            for (int i=0; i<manifestLegs.FilteredLegs.Count; i++)
            {
                var leg = manifestLegs.FilteredLegs[i];
                if (leg.CompanyNameContains(new List<string>() {"FEC", "MIAMI"}) || leg.ZoneIs("FMR"))
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool IsCustomersAfterMiamiRail(this ManifestLegs manifestLegs)
        {
            return manifestLegs.GetCustomerCountAfterMiamiRail() > 0;
        }

        public static bool IsCustomersBeforeMiamiRail(this ManifestLegs manifestLegs)
        {
            return manifestLegs.GetCustomerCountBeforeMiamiRail() > 0;
        }

        public static int GetCustomerStopCount(this ManifestLegs manifestLegs)
        {
            return manifestLegs.FilteredLegs.Count(leg => leg.CustomerNumber != "0" 
                && leg.CustomerNumber != "3272");
        }

        public static int GetCustomerCountAfterMiamiRail(this ManifestLegs manifestLegs)
        {
            int count = 0;
            var railIndex = manifestLegs.GetLegsIndexOfMiamiRail();
            if (railIndex > -1)
            {
                count += manifestLegs.FilteredLegs.Where((leg, i) => leg.CustomerNumber != "0" 
                    && leg.CustomerNumber != "3272" && i > railIndex).Count();
            }

            return count;
        }

        public static int GetCustomerCountBeforeMiamiRail(this ManifestLegs manifestLegs)
        {
            int count = 0;
            var railIndex = manifestLegs.GetLegsIndexOfMiamiRail();
            if (railIndex > -1)
            {
                count += manifestLegs.FilteredLegs.Where((leg, i) => leg.CustomerNumber != "0" 
                    && leg.CustomerNumber != "3272" && i < railIndex).Count();
            }

            return count;
        }

        public static ImportedLeg GetSequenceNumber(this ManifestLegs manifestLegs, int sequenceNumber)
        {
            return manifestLegs.AllLegs.FirstOrDefault(p => p.SequenceNumber == sequenceNumber);
        }

        public static bool ContainsSequenceNumber(this ManifestLegs manifestLegs, int sequenceNumber)
        {
            return manifestLegs.GetSequenceNumber(sequenceNumber) != null;
        }

        public static bool ContainsSequenceNumberAndName(this ManifestLegs manifestLegs, int sequenceNumber,
            IList<string> namesToContain)
        {
            return manifestLegs
                .GetSequenceNumber(sequenceNumber)
                .CompanyNameContains(namesToContain);
        }
    }
}