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
using PAI.FRATIS.SFL.Services.Integration.Extensions;

namespace PAI.FRATIS.SFL.Services.Integration
{
    public class ManifestLegs
    {
        public ManifestLegs()
        {
            _allLegs = new List<ImportedLeg>();
        }

        public ManifestLegs(ImportedLeg leg) : this(new List<ImportedLeg>() { leg })
        {
        }

        public ManifestLegs(IList<ImportedLeg> legs)
        {
            Set(legs);
        }

        public ManifestJobType GetJobType()
        {
            var miamiRailIndex = this.GetAllLegsIndexOfMiamiRail();
            if (miamiRailIndex >= 0)
            {
                #region Stops at FEC Miami Found

                // stop in FEC Miami
                if (AllLegs.Count > 1)
                {
                    #region More Than One Sequence

                    // more than 1 stop found, check following stops

                    if (this.AreFollowingStopsInZone(miamiRailIndex + 1, true, true))
                    {
                        return ManifestJobType.RampToCustomer;
                    }

                    if (this.ArePreviousStopsInZone(miamiRailIndex - 1, true, true))
                    {
                        return ManifestJobType.CustomerToRamp;
                    }

                    return ManifestJobType.Error1;

                    #endregion
                }
                else if (AllLegs.Count == 1)
                {
                    #region Only One Sequence Stop

                    // if address is not FEC and within zone
                    return ManifestJobType.Error2;

                    #endregion
                }

                #endregion
            }
            else
            {
                #region No Stops at FEC Miami Found

                // no FEC Miami stop found
                var nextStopInZone = this.AllLegsGetNextStopInZone(0);
                var originZone = AllLegs.First().OriginZone;

                if (nextStopInZone == -1)
                {
                    // no stops in zone
                    return ManifestJobType.Ignore;
                }

                if (AllLegs.Count == 1)
                {
                    #region Only One Sequence Record

                    switch (originZone)
                    {
                        case "FMR":
                            return ManifestJobType.RampToCustomer;
                        case "MIA":
                        case "MIAMI":
                            return AllLegs.First().DestinationZone == "FMR" 
                                ? ManifestJobType.CustomerToRamp 
                                : ManifestJobType.IncompleteOrderType1; // create stop just for customer, nothing else
                        default:
                            return ManifestJobType.Error3;
                    }

                    #endregion 
                }
                else
                {
                    #region Multiple Sequence Records

                    if (originZone == "FMR")
                    {
                        return ManifestJobType.RampToCustomer;
                    }

                    switch (AllLegs.Last().DestinationZone)
                    {
                        case "FMR":
                            return ManifestJobType.CustomerToRamp;
                        case "MIA":
                        case "MIAMI":
                            return ManifestJobType.Incomplete;
                        default:
                            return ManifestJobType.Ignore;
                    }

                    #endregion
                }

                #endregion
            }

            return ManifestJobType.Unspecified;

        }

        public ManifestJobType JobType
        {
            get
            {
                try
                {
                    return GetJobType();
                }
                catch (Exception e)
                {
                    return GetJobType();
                }
            }
        }

        public bool IsFlatbed
        {
            get
            {
                if (this.AllLegs != null)
                {
                    if (this.AllLegs.Any(leg => leg.CustomerNumber == "1863"))
                    {
                        return true;
                    }                    
                }

                return false;
            }
        }

        private IList<ImportedLeg> _allLegs;
        public IList<ImportedLeg> AllLegs
        {
            get 
            { 
                return _allLegs != null 
                    ? _allLegs.OrderBy(p => p.SequenceNumber).ToList() 
                    : new List<ImportedLeg>(); 
            }

            set { _allLegs = value; }
        }

        public IList<ImportedLeg> FilteredLegs
        {
            get
            {
                var filteredByZip = AllLegs
                    .Where(p => p.CompanyZipInt > 0 
                        && ValidValues.IsValidZipCode(p.CompanyZipInt)).ToList();
                return filteredByZip;
            }
        }
        //leg.CompanyNameContains(new List<string>() { "FEC", "MIAMI" })
        public IList<ImportedLeg> FilteredLegsWithoutRails
        {
            get { return FilteredLegs.Where(p => p.CustomerNumber != "0" && p.CustomerNumber != "3272").ToList(); }
        }


        public void Set(IList<ImportedLeg> legs)
        {
            _allLegs = legs;
        }

        public int LegsIndexOfCustomer(IList<string> nameContains)
        {
            for (int i=0; i<FilteredLegs.Count;i++)
            {
                if (FilteredLegs[i].CompanyNameContains(nameContains))
                    return i;
            }
            return -1;
        }

        public int AllLegsIndexOfCustomer(IList<string> nameContains)
        {
            for (int i = 0; i < AllLegs.Count; i++)
            {
                if (AllLegs[i].CompanyNameContains(nameContains))
                    return i;
            }
            return -1;
        }

    }
}
