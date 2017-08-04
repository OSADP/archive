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
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using PAI.Drayage.Optimization.Model.Orders;
using PAI.FRATIS.SFL.Common.Infrastructure.Engine;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Integration.Extensions;
using PAI.FRATIS.SFL.Services.Orders;
using Job = PAI.FRATIS.SFL.Domain.Orders.Job;
using RouteStop = PAI.FRATIS.SFL.Domain.Orders.RouteStop;
using StopAction = PAI.FRATIS.SFL.Domain.Orders.StopAction;

namespace PAI.FRATIS.SFL.Services.Integration
{
    public class ImportJobResult
    {
        private static List<string> invalidStreetAddresses = 
            new List<string>
            {
                "c/o fec ramp - miami",
                "c/o fec miami ramp",
                "c/o fec ramp-miami",
            };
        private IGeocodeService _geocodeService;

        private readonly IJobGroupService _jobGroupService;

        const int FecDefaultStopDelay = 30;

        private IDateTimeHelper _dateTimeHelper;
        public IDateTimeHelper DateTimeHelper
        {
            get 
            {
                return _dateTimeHelper ?? new DateTimeHelper();
            }
        }

        private Dictionary<string, ManifestLegs> _dictionary;
        public Dictionary<string, ManifestLegs> Dictionary
        {
            get { return _dictionary; }
        }

        public ImportJobResult(IList<ImportedLeg> jobs, IGeocodeService geocodeService, IJobGroupService jobGroupService)
        {
            _geocodeService = geocodeService;
            _jobGroupService = jobGroupService;
            Set(jobs);
        }

        public ImportJobResult(Dictionary<string, ManifestLegs> legsByManifestNumber, IGeocodeService geocodeService, IJobGroupService jobGroupService)
        {
            _dictionary = legsByManifestNumber;
            _geocodeService = geocodeService;
            _jobGroupService = jobGroupService;
        }

        /// <summary>
        /// Takes all imported legs and sanitizes them and adds them to the dictionary
        /// </summary>
        /// <param name="legs"></param>
        public void Set(IList<ImportedLeg> legs)
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<string, ManifestLegs>(); 
            }

            if (legs != null)
            {
                foreach (var leg in legs)
                {
                    if (!string.IsNullOrEmpty(leg.ManifestNumber))
                    {
                        var manifestNumber = leg.ManifestNumber;
                        if (_dictionary.ContainsKey(manifestNumber))
                        {
                            var element = _dictionary[manifestNumber];
                            var allLegs = element.AllLegs;
                            allLegs.Add(leg);

                            element.AllLegs = allLegs;
                            _dictionary[manifestNumber] = element;
                        }
                        else
                        {
                            _dictionary.Add(manifestNumber, new ManifestLegs(leg));
                        }
                    }
                }
            }
        }

        public IList<Job> GetJobs(Dictionary<string, Location> locationsByCustomerNumber, 
            IList<StopAction> stopActions, int subscriberId)
        {
            var jobGroups = _jobGroupService.GetGroups(subscriberId).Where(p => p.Name != "Unspecified").ToList();

            var result = new List<Job>();
            foreach (var kvp in Dictionary)
            {
                if (!kvp.Value.FilteredLegs.Any()) continue;

                var job = new Job()
                {
                    SubscriberId = subscriberId,
                    ContainerNumber = kvp.Value.FilteredLegs.First().Trailer,
                    ShipperName = kvp.Value.FilteredLegs.First().ShipperName,
                    ConsigneeName = kvp.Value.FilteredLegs.First().ConsigneeName,
                    LegacyId = kvp.Key,
                    OrderNumber = kvp.Value.FilteredLegs.First().ManifestNumber,
                    BillOfLading = kvp.Value.FilteredLegs.First().BillOfLadingNumber,
                    DueDate = null,
                    RouteStops = new List<RouteStop>(),
                    IsValid = true,
                    IsHazmat = kvp.Value.FilteredLegs.First().IsHazmat,
                    IsFlatbed = kvp.Value.IsFlatbed,
                };

                var filteredLegsWithoutRails = kvp.Value.FilteredLegsWithoutRails;
                DateTime? dueDateLocal = null;

                switch (kvp.Value.JobType)
                {
                    case ManifestJobType.CustomerToRamp:
                        #region Customer To Ramp Logic

                        job.RouteStops.Add(
                            CreateRouteStop(stopActions.GetStopAction("PEWC"), "0",
                                locationsByCustomerNumber, FecDefaultStopDelay, subscriberId));

                        for (int i = 0; i < filteredLegsWithoutRails.Count; i++)
                        {
                            var leg = filteredLegsWithoutRails[i];
                            if (!job.DueDate.HasValue)
                            {
                                dueDateLocal = leg.ScheduledDateTime.Date;
                                job.DueDate = this.DateTimeHelper.ConvertLocalToUtcTime(dueDateLocal.Value);
                            }

                            // CBS 190914 - Removing 30 minute padding
                            var tsStart = leg.ScheduledDateTime;//.AddMinutes(-30);
                            var appointmentTimeTicks = dueDateLocal.HasValue
                                ? tsStart.Subtract(dueDateLocal.Value).Ticks
                                : 0;

                            var windowStart = appointmentTimeTicks;
                            var windowEnd = appointmentTimeTicks; 

                            job.RouteStops.Add(GetRouteStopFromLeg(subscriberId, leg, stopActions.GetStopAction("LL"),
                                locationsByCustomerNumber, new TimeSpan(windowStart), new TimeSpan(windowEnd), 60));
                        }

                        var finalStop = kvp.Value.GetMiamiRailLegAfterCustomers();

                        if (finalStop != null)
                        {
                            var windowEnd = new TimeSpan(finalStop.ScheduledDateTime.Hour, finalStop.ScheduledDateTime.Minute, 0);
                            job.RouteStops.Add(GetRouteStopFromLeg(subscriberId, finalStop, stopActions.GetStopAction("DLWC"),
                                locationsByCustomerNumber, new TimeSpan(0, 0, 0), windowEnd, FecDefaultStopDelay));
                        }
                        else
                        {
                            job.RouteStops.Add(
                                CreateRouteStop(stopActions.GetStopAction("DLWC"), "0",
                                    locationsByCustomerNumber, FecDefaultStopDelay, subscriberId, new TimeSpan(job.RouteStops.Last().WindowStart), new TimeSpan(job.RouteStops.Last().WindowStart + new TimeSpan(23, 59, 0).Ticks)));
                        }

                        #endregion

                        break;
                    case ManifestJobType.RampToCustomer:
                        #region Ramp to Customer Logic
                        var firstStop = kvp.Value.GetMiamiRailLegBeforeCustomers();
                        if (firstStop != null)
                        {
                            var windowStart = new TimeSpan(firstStop.ScheduledDateTime.Hour, firstStop.ScheduledDateTime.Minute, 0);
                            job.RouteStops.Add(GetRouteStopFromLeg(subscriberId, firstStop, stopActions.GetStopAction("PLWC"),
                                locationsByCustomerNumber, windowStart, new TimeSpan(23, 59, 0), FecDefaultStopDelay));
                        }
                        else
                        {
                            job.RouteStops.Add(
                                CreateRouteStop(stopActions.GetStopAction("PLWC"), "0",
                                    locationsByCustomerNumber, FecDefaultStopDelay, subscriberId));
                        }

                        for (int i = 0; i < filteredLegsWithoutRails.Count; i++)
                        {
                            var leg = filteredLegsWithoutRails[i];
                            
                            if (!job.DueDate.HasValue)
                            {
                                dueDateLocal = leg.ScheduledDateTime.Date;
                                job.DueDate = this.DateTimeHelper.ConvertLocalToUtcTime(dueDateLocal.Value);
                            }

                            // CBS 190914 - Removing 30 minute padding
                            var tsStart = leg.ScheduledDateTime;//.AddMinutes(-30);
                            var appointmentTimeTicks = dueDateLocal.HasValue
                                ? tsStart.Subtract(dueDateLocal.Value).Ticks
                                : 0;

                            var sa = stopActions.GetStopAction("LU");                        
                            job.RouteStops.Add(GetRouteStopFromLeg(subscriberId, leg, sa,
                                locationsByCustomerNumber,
                                new TimeSpan(appointmentTimeTicks),
                                new TimeSpan(appointmentTimeTicks), 120));
                        }


                        if (job.RouteStops.Last().StopAction.ShortName == "DL")
                        {
                            job.RouteStops.Add(
                                CreateRouteStop(stopActions.GetStopAction("DC"), "0",
                                locationsByCustomerNumber, FecDefaultStopDelay, subscriberId,
                                new TimeSpan(job.RouteStops.Last().WindowStart),
                                new TimeSpan(job.RouteStops.Last().WindowStart + new TimeSpan(23, 59, 0).Ticks)));
                        }
                        else
                        {
                            job.RouteStops.Add(
                                CreateRouteStop(stopActions.GetStopAction("DEWC"), "0",
                                locationsByCustomerNumber, FecDefaultStopDelay, subscriberId,
                                new TimeSpan(job.RouteStops.Last().WindowStart),
                                new TimeSpan(job.RouteStops.Last().WindowStart + new TimeSpan(23, 59, 0).Ticks)));                            
                        }
                        #endregion

                        break;
                    case ManifestJobType.IncompleteOrderType1:
                    case ManifestJobType.IncompleteOrderType2:
                        #region Incomplete Order

                        // add all customers, no actions
                        job.IsValid = false;
                        for (int i = 0; i < filteredLegsWithoutRails.Count; i++)
                        {
                            var leg = filteredLegsWithoutRails[i];
                            if (!job.DueDate.HasValue)
                            {
                                dueDateLocal = leg.ScheduledDateTime.Date;
                                job.DueDate = this.DateTimeHelper.ConvertLocalToUtcTime(dueDateLocal.Value);
                            }

                            // CBS 190914 - Removing 30 minute padding
                            var tsStart = leg.ScheduledDateTime;//.AddMinutes(-30);
                            var appointmentTimeTicks = dueDateLocal.HasValue
                                ? tsStart.Subtract(dueDateLocal.Value).Ticks
                                : 0;

                            job.RouteStops.Add(GetRouteStopFromLeg(subscriberId, leg, stopActions.GetStopAction("NA"),
                                locationsByCustomerNumber, new TimeSpan(appointmentTimeTicks), new TimeSpan(appointmentTimeTicks), 120));
                        }
                        #endregion
                        break;
                    default:
                        continue;   // invalid j
                }

                if (job.RouteStops.Any(f => f.Location.DisplayName.ToLower().Contains("gibson")))
                {
                    job.IsFlatbed = true;
                }

                // fix intermediate routestop durations, and set JobGroup/shift
                Console.WriteLine(job.OrderNumber);
                job = FixJob(job, jobGroups);
                result.Add(job);
            }


            return result;
        }

        private RouteStop CreateRouteStop(StopAction sa, string customerNumber,
            Dictionary<string, Location> locationsByCustomerNumber, int stopDelay, int subscriberId, TimeSpan? windowStart = null, TimeSpan? windowEnd = null)
        {
            Location location;

            try
            {
                location = locationsByCustomerNumber[customerNumber];
            }
            catch (Exception ex)
            {
                location = new Location()
                {
                    DisplayName = "Loc not found",
                };
            }

            // publix should have stop times of 4 hours
            if (location != null && location.DisplayName.ToLower().Contains("publix"))
            {
                stopDelay = 240;
            }

            return new RouteStop()
            {
                SubscriberId = subscriberId,
                Location = location,
                LocationId = location.Id,
                StopAction = sa,
                StopDelay = stopDelay,
                WindowStart = windowStart != null ? windowStart.Value.Ticks : 0,
                WindowEnd = windowEnd != null ? windowEnd.Value.Ticks : new TimeSpan(23, 59, 0).Ticks,
            };

        }
    
        private RouteStop GetRouteStopFromLeg(int subscriberId, ImportedLeg leg, StopAction sa, Dictionary<string, Location> locationsByLegacyId,
            TimeSpan? windowStart = null, TimeSpan? windowEnd = null, int stopDelay = 60)
        {
            Location location;

            try
            {
                location = locationsByLegacyId[leg.CustomerNumber];
            }
            catch (Exception ex)
            {
                location = new Location()
                {
                    DisplayName = "Loc not found",
                };
            }

            if (location != null && (location.DisplayName.ToLower().Contains("publix") || location.DisplayName.ToLower().Contains("associated grocers")))
            {
                stopDelay = 240;
            }

            var result = new RouteStop()
            {
                SubscriberId = subscriberId,
                StopAction = sa,
                LocationId = location.Id,
                Location = location,
                StopDelay = stopDelay,
                WindowStart = windowStart.HasValue ? windowStart.Value.Ticks : 0,
                WindowEnd = windowEnd.HasValue ? windowEnd.Value.Ticks : new TimeSpan(23, 59, 0).Ticks,
            };

            return result;
        }

        public Dictionary<string, Location> GetCompanyLocations(int subscriberId, bool filteredByZip = true)
        {
            var locationsByCustomerNumber = new Dictionary<string, Location>();

            foreach (var manifestLegs in _dictionary.Values)
            {
                var legs = manifestLegs.AllLegs;
                foreach (var l in legs)
                {
                    if (filteredByZip && !ValidValues.IsValidZipCode(l.CompanyZipInt)) continue;

                    if (!locationsByCustomerNumber.ContainsKey(l.CustomerNumber))
                    {
                        var loc = new Location()
                        {
                            SubscriberId = subscriberId,
                            DisplayName = l.CompanyName,
                            LegacyId = l.CustomerNumber,
                            StreetAddress = l.CompanyAddress1,
                            City = l.CompanyCity,
                            State = l.CompanyState,
                            Zip = l.CompanyZip
                        };

                        locationsByCustomerNumber.Add(l.CustomerNumber, loc);
                    }
                }
            }

            return locationsByCustomerNumber;
        }

        private Job FixJob(Job j, IList<JobGroup> jobGroups)
        {
            for (int r = 0; r < j.RouteStops.Count; r++)
            {
                // set ordering
                j.RouteStops[r].SortOrder = r + 1;

                // set live duration
                var liveIndexes = GetLiveIndexes(j);
                if (liveIndexes.Count > 1)
                {
                    for (int l = 0; l < liveIndexes.Count; l++)
                    {
                        j.RouteStops[liveIndexes[l]].StopDelay = 0;

                        if (l + 1 == liveIndexes.Count)
                        {
                            j.RouteStops[liveIndexes[l]].StopDelay = 90;
                        }
                    }
                }

                // make C/O FEC Miami Ramp stops invalid
                if (!j.RouteStops.Any(x => invalidStreetAddresses.Contains(x.Location.Street.ToLower()))) continue;
                foreach (var routeStop in j.RouteStops)
                {
                    routeStop.StopAction = new StopAction
                        {
                            Id = StopActions.NoAction.Id,
                            Name = StopActions.NoAction.Name,
                            ShortName = StopActions.NoAction.ShortName
                        };
                }
                j.IsValid = false;
                j.IsValidNullable = false;
            }

            SetShift(j, jobGroups);    

            return j;
        }

        private List<int> GetLiveIndexes(Job job)
        {
            var result = new List<int>();
            if (job != null && job.RouteStops != null)
            {
                for (int i = 0; i < job.RouteStops.Count; i++)
                {
                    var rs = job.RouteStops[i];
                    if (rs.StopAction != null && (rs.StopAction.ShortName == "LU" || rs.StopAction.ShortName == "LL"))
                    {
                        result.Add(i);
                    }
                }
            }
            return result;
        }

        readonly TimeSpan CompareMin = TimeSpan.FromHours(5);
        readonly TimeSpan CompareMax = TimeSpan.FromHours(17);
        public void SetShift(Job jobWithRoutes, IList<JobGroup> jobGroups)
        {
            if (jobWithRoutes == null || jobGroups == null || !jobGroups.Any() || jobWithRoutes.RouteStops == null) return;

            var stops = jobWithRoutes.RouteStops.OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToList();
            if (stops.Count > 2)
            {
                var stop = stops[stops.Count - 2];
                var ts = new TimeSpan(stop.WindowEnd);

                while (ts.TotalDays > 1.000)
                {
                    ts = ts.Subtract(TimeSpan.FromDays(1));
                }

                JobGroup jg = null;
                jg = ts >= CompareMax || ts <= CompareMin
                    ? jobGroups[1]
                    : jobGroups[0];

                if (jobWithRoutes.JobGroup == null || jobWithRoutes.JobGroup.Id != jg.Id)
                {
                    jobWithRoutes.JobGroup = jg;
                    jobWithRoutes.JobGroupId = jg.Id;
                }
            }
        }

    }
}