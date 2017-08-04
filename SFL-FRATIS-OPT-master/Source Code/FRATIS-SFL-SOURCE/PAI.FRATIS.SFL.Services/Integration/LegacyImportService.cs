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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Markup;
using PAI.Drayage.EnhancedOptimization.Services;
using PAI.Drayage.Optimization.Geography;
using PAI.Drayage.Optimization.Model.Equipment;
using PAI.FRATIS.ExternalServices.NokiaMaps.Model.TrafficItems;
using PAI.FRATIS.SFL.Domain.Orders;
using PAI.FRATIS.SFL.Domain.Subscribers;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.SFL.Services.Integration.Extensions;
using PAI.FRATIS.SFL.Services.Logging;
using PAI.FRATIS.SFL.Services.Orders;
using Location = PAI.FRATIS.SFL.Domain.Geography.Location;

namespace PAI.FRATIS.SFL.Services.Integration
{
    public class ReportErrors
    {
        public static List<string> Errors = new List<string>();

        public ReportErrors(string fileName)
        {
            Errors.Add(string.Format("The following is a list of errors for file {0}", fileName));
        }

        public static void AddError(string error)
        {
            Errors.Add(error);
        }

        public static void SendErrors(string subject = "")
        {
            var mail = new MailMessage();
            var client = new SmtpClient
                {
                    Port = 587,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("chris@productivityapex.com", "p05tr1ng")
                };
            mail.To.Add(new MailAddress("chris@productivityapex.com"));
            mail.To.Add(new MailAddress("fabio@productivityapex.com"));
            mail.From = new MailAddress("Miami_File_Importer@productivityapex.com");

            var firstError = Errors.FirstOrDefault().Replace('\r', ' ').Replace('\n', ' ');

            if (firstError.StartsWith("The following is a list of errors for file"))
            {
                mail.Subject = Errors.FirstOrDefault().Replace('\r', ' ').Replace('\n', ' ');
                Errors.RemoveAt(0);
            }
            if (!string.IsNullOrEmpty(subject))
            {
                mail.Subject = subject + " " + mail.Subject;
            }
            if (string.IsNullOrEmpty(mail.Subject))
            {
                mail.Subject = "Error report from SFL FARTIS Import App";
            }
            if (Errors.Count > 0)
            {
                mail.Body = Errors.Aggregate((a, b) => a + "\n" + b);
                client.Send(mail);
            }
            Errors = new List<string>();
        }
    }

    public class LegacyImportService : ImportServiceBase, ILegacyImportService
    {
        private readonly IDriverService _driverService;

        private readonly IDistanceService _distanceService;

        private readonly IGeocodeService _geocodeService;

        private readonly IJobGroupService _jobGroupService;

        private readonly ILocationService _locationService;

        private readonly IStopActionService _stopActionService;

        private readonly IRouteStopService _routeStopService;

        private readonly IJobService _jobService;

        private readonly ISyncLogEntryService _syncLogEntryService;

        readonly IValidationService _validationService;

        private IList<StopAction> _stopActions;
        private IList<StopAction> StopActions
        {
            get { return _stopActions ?? (_stopActions = _stopActionService.GetStopActions().ToList()); }
        } 

        #region Fields

        private int _manifestNumberIndex,
            _billOfLadingNumberIndex,
            _sealNumberIndex,
            _legNumberIndex,
            _manifestTypeIndex,
            _legTypeIndex,
            _scheduledLoadTimeIndex,
            _numberOfLegsIndex,
            _scheduledDateStringIndex,
            _scheduledTimeStringIndex,
            _loadDateStringIndex,
            _loadTimeStringIndex,
            _deliveryDateStringIndex,
            _deliveryTimeStringIndex,
            _legNumberCreditedIndex,
            _sequenceNumberIndex,

            // customer and company
            _customerNumberIndex,
            _companyNameIndex,
            _companyAddress1Index,
            _companyAddress2Index,
            _companyCityIndex,
            _companyStateIndex,
            _companyZipIndex,

            _shipperNumberIndex,
            _shipperNameIndex,
            _consigneeNumberIndex,
            _consigneeNameIndex,

            _orderOriginCityIndex,
            _orderOriginStateIndex,
            _orderOriginZipIndex,

            _orderDestinationCityIndex,
            _orderDestinationStateIndex,
            _orderDestinationZipIndex,

            _legDestinationCityIndex,
            _legDestinationStateIndex,
            _legDestinationZipIndex,

            _legOriginCityIndex,
            _legOriginStateIndex,
            _legOriginZipIndex,

            _serviceTypeIndex,

            _stopOffCityIndex,
            _stopOffStateIndex,
            _recordTypeIndex,
            _scheduledStopIndex,

            _originZoneIndex,
            _destinationZoneIndex,
            _trailerIndex,
            _isHazmatIndex;

        private string _dictionary;
        private ReportErrors reportErrors;

        #endregion

        public LegacyImportService(ILocationService locationService, IStopActionService stopActionService, IJobService jobService, IRouteStopService routeStopService, IGeocodeService geocodeService, IJobGroupService jobGroupService, IDriverService driverService, ISyncLogEntryService syncLogEntryService, IDistanceService distanceService)
        {
            _locationService = locationService;
            _stopActionService = stopActionService;
            _jobService = jobService;
            _routeStopService = routeStopService;
            _geocodeService = geocodeService;
            _jobGroupService = jobGroupService;
            _driverService = driverService;
            _syncLogEntryService = syncLogEntryService;
            _distanceService = distanceService;
            _validationService = new ValidationService(
                    new[]
                    {
                        new TimeSpan(11, 11, 0) 
                    }
                );
        }

        private void AddToLocationDictionary(Location location, Dictionary<string, IList<Location>> dict)
        {
            var key = string.Format("{0},{1}", location.Latitude, location.Longitude);
            if (dict.ContainsKey(key))
            {
                var items = dict[key];
                items.Add(location);
                dict[key] = items;
            }
            else
            {
                dict.Add(key, new List<Location>() { location });
            }
        } 
        public IList<Location> SyncLocations(IEnumerable<Location> locations, int subscriberId, out int locationErrorCount)
        {
            Console.WriteLine("Processing locations.");
            var result = new List<Location>();
            var existingLocations = _locationService.GetBySubscriberId(subscriberId).ToList();
            foreach (var location in locations)
            {
                if (string.IsNullOrEmpty(location.LegacyId) || string.IsNullOrEmpty(location.StreetAddress)) continue;

                // proceed
                var matchedLocation = existingLocations.FirstOrDefault(p => p.LegacyId == location.LegacyId);
                if (matchedLocation != null)
                {
                    if (matchedLocation.IsChangedFrom(location) && location.LegacyId != "0")
                    {
                        if (!matchedLocation.IsValidated)
                        {
                            // user has not flagged this order as verified, map new changes
                            location.MapTo(matchedLocation);
                            matchedLocation.IsFailedGeocode = false;
                            GeocodeLocationAndSave(matchedLocation);                            
                        }
                    }
                }
                else
                {
                    matchedLocation = new Location();
                    location.MapTo(matchedLocation);
                    matchedLocation.SubscriberId = subscriberId;
                    GeocodeLocationAndSave(matchedLocation);
                }

                result.Add(matchedLocation);
            }
            
            var dict = new Dictionary<string, IList<Location>>();
            foreach (var location in result)
            {
                AddToLocationDictionary(location, dict);
            }

            var dupes = dict.Where(p => p.Value.Count > 1).ToList();
            foreach (var dupeLocations in dupes)
            {
                foreach (var dupe in dupeLocations.Value)
                {
                    dupe.IsFailedGeocode = true;
                    _locationService.Update(dupe, false);
                }
            }
            _locationService.SaveChanges();

            locationErrorCount = dict.SelectMany(kvp => kvp.Value).Count(v => v.IsFailedGeocode);
            return result;
        }


        private void GeocodeLocationAndSave(Location matchedLocation)
        {
            // geocode - limit 900 per hour / one every 4 seconds
            var geocodeResult = new GeocodeResult();
            var cityGeocodeResult = new GeocodeResult();
            var successFlag = false;
            try
            {
                geocodeResult = _geocodeService.Geocode("", matchedLocation.StreetAddress, matchedLocation.City,
                                                        matchedLocation.State, matchedLocation.Zip);
                successFlag = true;
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format("Unable to fetch GeocodeResult for {0} {1} {2} {3}", matchedLocation.StreetAddress, matchedLocation.City, matchedLocation.State, matchedLocation.Zip));
            }
            try
            {
                cityGeocodeResult = _geocodeService.Geocode("", "", matchedLocation.City, matchedLocation.State,
                                                            matchedLocation.Zip);
                successFlag &= true;
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format("Unable to fetch CityGeocodeResult for {0} {1} {2} {3}", matchedLocation.StreetAddress, matchedLocation.City, matchedLocation.State, matchedLocation.Zip));
            }

            if (successFlag)
            {
                geocodeResult.SaveTo(matchedLocation);

                if (geocodeResult.IsSameAs(cityGeocodeResult))
                {
                    // geocode failure likely
                    ReportErrors.AddError(string.Format("Geocode failure for location {0}", matchedLocation.LegacyId));
                    matchedLocation.IsFailedGeocode = true;
                }

                if (matchedLocation.Id == 0 && matchedLocation.LegacyId != "0")
                {
                    _locationService.Insert(matchedLocation);
                }
                else
                {
                    _locationService.Update(matchedLocation);
                }
            }
        }

        public ImportJobResult ImportJobs(string filePath, int subscriberId)
        {
            var readResult = this.Read(filePath);

            // order
            _manifestNumberIndex = 0; //readResult.GetColumnIndex("Manifest #");
            _manifestTypeIndex = 2; //readResult.GetColumnIndex("Manifest Type");
            _billOfLadingNumberIndex = 37; //readResult.GetColumnIndex("Bill of lading number");
            _sequenceNumberIndex = 8; //readResult.GetColumnIndex("Sequence #");
            _scheduledLoadTimeIndex = 60; // readResult.GetColumnIndex("Scheduled Load Time");
            _scheduledStopIndex = 10; //readResult.GetColumnIndex("Scheduled Stop");
            _trailerIndex = 49; //readResult.GetColumnIndex("1st Trailer assigned");
            
            // date times
            _scheduledDateStringIndex = 17; //readResult.GetColumnIndex("Scheduled Date");
            _scheduledTimeStringIndex = 18; //readResult.GetColumnIndex("Scheduled Time");

            _isHazmatIndex = 68;
            _loadDateStringIndex = 59; //readResult.GetColumnIndex("Scheduled Load Date");
            _loadTimeStringIndex = 60; //readResult.GetColumnIndex("Scheduled Load Time");
            _deliveryDateStringIndex = 66; // readResult.GetColumnIndex("Scheduled Delivery Date");
            _deliveryTimeStringIndex = 67; // readResult.GetColumnIndex("Scheduled Delivery Time");

            // leg 
            _legDestinationCityIndex = 62; //readResult.GetColumnIndex("Leg Destination City");
            _legDestinationStateIndex = 63; //readResult.GetColumnIndex("Leg Destination State");
            _legDestinationZipIndex = 65; //readResult.GetColumnIndex("Leg Destination Zip");

            _legOriginCityIndex = 53; //readResult.GetColumnIndex("Leg Origin City");
            _legOriginStateIndex = 54; //readResult.GetColumnIndex("Leg Origin State");
            _legOriginZipIndex = 56; //readResult.GetColumnIndex("Leg Origin Zip");

            _stopOffCityIndex = 11; //readResult.GetColumnIndex("Stopoff City");
            _stopOffStateIndex = 12; //readResult.GetColumnIndex("Stopoff State");

            _legNumberIndex = 1; //readResult.GetColumnIndex("Leg #");
            _legTypeIndex = 4; //readResult.GetColumnIndex("Leg Type");
            _numberOfLegsIndex = 5;//readResult.GetColumnIndex("No. of Legs");
            _legNumberCreditedIndex = 6;// readResult.GetColumnIndex("Leg # credited");

            // company
            _customerNumberIndex = 19; //readResult.GetColumnIndex("Customer #");
            _companyNameIndex = 20; //readResult.GetColumnIndex("Company Name");
            _companyAddress1Index = 21;//readResult.GetColumnIndex("Address Line 1");
            _companyAddress2Index = 22;//readResult.GetColumnIndex("Address Line 2");
            _companyCityIndex = 11; //readResult.GetColumnIndex("Stopoff City");
            _companyStateIndex = 12; //readResult.GetColumnIndex("Stopoff State");
            _companyZipIndex = 23; //readResult.GetColumnIndex("Zip Code");

            // order origin and destination
            _orderOriginCityIndex = 38; //readResult.GetColumnIndex("Order origin city");
            _orderOriginStateIndex = 39; //readResult.GetColumnIndex("Order origin state");
            _orderOriginZipIndex = 40; //readResult.GetColumnIndex("Order origin zip");

            _orderDestinationCityIndex = 43; //readResult.GetColumnIndex("Order Destination city");
            _orderDestinationStateIndex = 44; //readResult.GetColumnIndex("Order Destination state");
            _orderDestinationZipIndex = 45;//readResult.GetColumnIndex("Order Destination zip");

            _shipperNumberIndex = 28; //readResult.GetColumnIndex("Shipper #");
            _shipperNameIndex = 35; //readResult.GetColumnIndex("Shipper Name");
            _consigneeNumberIndex = 30; //readResult.GetColumnIndex("Consignee #");
            _consigneeNameIndex = 36; //readResult.GetColumnIndex("Consignee Name");

            _serviceTypeIndex = 50; //readResult.GetColumnIndex("Service type");
            _legOriginCityIndex = 53; //readResult.GetColumnIndex("Leg origin city");
            _legOriginZipIndex = 56; //readResult.GetColumnIndex("Leg origin zip");
            _legDestinationCityIndex = 62; //readResult.GetColumnIndex("Leg Destination city");

            _originZoneIndex = 52; //readResult.GetColumnIndex("Dispatch origin term/zone");
            _destinationZoneIndex = 61; //readResult.GetColumnIndex("Dispatch dest. term/zone");

            var importedJobs = new List<ImportedLeg>();
            if (readResult.Values != null)
            {
                try
                {
                    foreach (var stringse in readResult.Values)
                    {
                        try
                        {
                            var importedJob = ExtractJob(stringse);
                            importedJobs.Add(importedJob);
                        }
                        catch (Exception e)
                        {
                            ReportErrors.AddError(filePath + "\n" + stringse + "\n" + e.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ReportErrors.AddError(filePath + "\n" + ex.Message + "\n" + ex.StackTrace);
                }
            }
            return new ImportJobResult(importedJobs, _geocodeService, _jobGroupService);
        }

        public void ImportDrivers(string filePath, int subscriberId, int defaultLocationId = 0)
        {
            var readResult = this.Read(filePath);

            // order
            var driverLegacyIdIndex = 0;
            var driverNameIndex = 1;
            var earliestStartTimeIndex = 3;
            var maxOnDutyTimeIndex = 4;
            var maxDrivingTimeIndex = 5;
            
            var existingDrivers =_driverService.Select().ToList();
            foreach (var row in readResult.Values)
            {
                try
                {
                    var names = row[driverNameIndex].Split(',');
                    if (names.Length == 1)
                    {
                        names = row[driverNameIndex].Split(' ');

                        if (names.Length < 2)
                        {
                            continue;
                        }
                        var first = names[0];
                        var last = names[1];
                        names[0] = last;
                        names[1] = first;
                    }

                    if (names.Length < 2)
                    {
                        continue;
                    }

                    var d = new Driver()
                    {
                        SubscriberId = subscriberId,
                        LegacyId = row.GetString(driverLegacyIdIndex),
                        FirstName = names[1].Trim(),
                        LastName = names[0].Trim(),
                        IsPlaceholderDriver = false,
                        StartingLocationId = defaultLocationId
                    };

                    var earliestTimeString = row.GetString(earliestStartTimeIndex);
                    var x = earliestTimeString.Substring(0, earliestTimeString.IndexOf(':'));
                    var xx = 0;
                    Int32.TryParse(x, out xx);
                    if (earliestTimeString.EndsWith("PM"))
                    {
                        xx = xx + 12;
                    }

                    d.EarliestStartTime = new TimeSpan(xx, 0, 0).Ticks;
                    d.AvailableDutyHours = row.GetInt(maxOnDutyTimeIndex);
                    d.AvailableDrivingHours = row.GetInt(maxDrivingTimeIndex);

                    _driverService.Insert(d, true);
                }
                catch (Exception e)
                {
                    ReportErrors.AddError(string.Format("Count not parse a driver from the row  because of {0}", e.Message));
                    continue;
                }
            }

        
        }

        public ImportJobResult Process(string filePath, int subscriberId, bool syncLocations, bool syncJobs)
        {
            reportErrors = new ReportErrors(filePath);
            var result = Process(filePath, subscriberId, syncLocations, syncJobs, true);
            Console.WriteLine("Sending Errors");
            ReportErrors.SendErrors();
            return result;
        }

        private ImportedLeg ExtractJob(string[] values)
        {
            const string ErrorTemplate = @"Error retrieving field {0} for record {1} with error of:\n{2}";
            _dictionary = values.GetString(_shipperNameIndex);
            var result = new ImportedLeg();
            try
            {
                result.ManifestNumber = values.GetString(_manifestNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ManifestNumber", "No Manifest Number Read", e.Message));
            }
            try
            {
                result.SealNumber = values.GetString(_sealNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "SealNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.BillOfLadingNumber = values.GetString(_billOfLadingNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "BillOfLadingNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.Trailer = values.GetString(_trailerIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "Trailer", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ManifestType = values.GetString(_manifestTypeIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ManifestType", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ServiceType = values.GetString(_serviceTypeIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ServiceType", result.ManifestNumber, e.Message));
            }
            try
            {
                result.RecordType = values.GetString(_recordTypeIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "RecordType", result.ManifestNumber, e.Message));
            }
            try
            {
                result.SequenceNumber = values.GetInt(_sequenceNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "SequenceNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ScheduledStop = values.GetBool(_scheduledStopIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ScheduledStop", result.ManifestNumber, e.Message));
            }
            try
            {
                result.IsHazmat = values.GetBool(_isHazmatIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "IsHazmat", result.ManifestNumber, e.Message));
            }

                // leg details
            try
            {
                result.NumberOfLegs = values.GetInt(_numberOfLegsIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "NumberOfLegs", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegNumber = values.GetString(_legNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegNumberCredited = values.GetString(_legNumberCreditedIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegNumberCredited", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegType = values.GetString(_legTypeIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegType", result.ManifestNumber, e.Message));
            }

            try
            {
                result.LegOriginCity = values.GetString(_legOriginCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegOriginCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegOriginState = values.GetString(_legOriginStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegOriginState", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegOriginZip = TryGetSubstring(values.GetString(_legOriginZipIndex), 0, 5);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegOriginZip", result.ManifestNumber, e.Message));
            }

            try
            {
                result.LegDestinationCity = values.GetString(_legDestinationCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegDestinationCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegDestinationState = values.GetString(_legDestinationStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegDestinationState", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LegDestinationZip = TryGetSubstring(values.GetString(_legDestinationZipIndex), 0, 5);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LegDestinationZip", result.ManifestNumber, e.Message));
            }

            try
            {
                // times
                result.LoadTime = values.GetInt(_scheduledLoadTimeIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LoadTime", result.ManifestNumber, e.Message));
            }
            try
            {
                result.LoadDateTime = values.GetCustomDateTime(_loadDateStringIndex, _loadTimeStringIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "LoadDateTime", result.ManifestNumber, e.Message));
            }
            try
            {
                result.DeliveryDateTime = values.GetCustomDateTime(_deliveryDateStringIndex, _deliveryTimeStringIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "DeliveryDateTime", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ScheduledDateTime = values.GetCustomDateTime(_scheduledDateStringIndex, _scheduledTimeStringIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ScheduledDateTime", result.ManifestNumber, e.Message));
            }

            try
            {
                // company
                result.CustomerNumber = values.GetString(_customerNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CustomerNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyName = values.GetString(_companyNameIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyName", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyAddress1 = SanitizeAddress(values.GetString(_companyAddress1Index));
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyAddress1", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyAddress2 = values.GetString(_companyAddress2Index);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyAddress2", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyCity = values.GetString(_companyCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyState = values.GetString(_companyStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyState", result.ManifestNumber, e.Message));
            }
            try
            {
                result.CompanyZip = TryGetSubstring(values.GetString(_companyZipIndex), 0, 5);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "CompanyZip", result.ManifestNumber, e.Message));
            }

            try
            {
                // consignee
                result.ConsigneeNumber = values.GetInt(_consigneeNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ConsigneeNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ConsigneeName = values.GetString(_consigneeNameIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ConsigneeName", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ShipperNumber = values.GetInt(_shipperNumberIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ShipperNumber", result.ManifestNumber, e.Message));
            }
            try
            {
                result.ShipperName = values.GetString(_shipperNameIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "ShipperName", result.ManifestNumber, e.Message));
            }

            try
            {
                // order origin
                result.OriginZone = values.GetString(_originZoneIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OriginZone", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderOriginCity = values.GetString(_orderOriginCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderOriginCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderOriginState = values.GetString(_orderOriginStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderOriginState", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderOriginZip = TryGetSubstring(values.GetString(_orderOriginZipIndex), 0, 5);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderOriginZip", result.ManifestNumber, e.Message));
            }

            try
            {
                // order destination
                result.DestinationZone = values.GetString(_destinationZoneIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "DestinationZone", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderDestinationCity = values.GetString(_orderDestinationCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderDestinationCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderDestinationState = values.GetString(_orderDestinationStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderDestinationState", result.ManifestNumber, e.Message));
            }
            try
            {
                result.OrderDestinationZip = TryGetSubstring(values.GetString(_orderDestinationZipIndex), 0, 5);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "OrderDestinationZip", result.ManifestNumber, e.Message));
            }

            try
            {
                result.StopOffCity = values.GetString(_stopOffCityIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "StopOffCity", result.ManifestNumber, e.Message));
            }
            try
            {
                result.StopOffState = values.GetString(_stopOffStateIndex);
            }
            catch (Exception e)
            {
                ReportErrors.AddError(string.Format(ErrorTemplate, "StopOffState", result.ManifestNumber, e.Message));
            }
            return result;
        }

        public string TryGetSubstring(string x, int startPos, int count)
        {
            try
            {
                return x.Substring(startPos, count);
            }
            catch(Exception ex)
            {
                return x;
            }
        }

        private string SanitizeAddress(string address)
        {
            var result = address;
            if (result.IndexOf("STE", System.StringComparison.Ordinal) > 0)
            {
                result = result.Substring(0, result.IndexOf("STE", System.StringComparison.Ordinal));
            }

            if (result.IndexOf("VENDOR", System.StringComparison.Ordinal) > 0)
            {
                result = result.Substring(0, result.IndexOf("VENDOR", System.StringComparison.Ordinal));
            }

            result = result.Trim();

            if (result.EndsWith(","))
            {
                result = result.Substring(0, result.Length - 1).Trim();
            }

            return result;

        }

        public ImportJobResult Process(string filePath, int subscriberId, bool syncLocations, bool syncJobs, bool saveToDatabase)
        {
            int locationErrorCount = 0;
            int createdJobCount = 0;
            var errorJobCount = 0;
            int updatedJobCount = 0;
            int existingJobCount = 0;
            int recreatedRouteStopCount = 0;

            var result = ImportJobs(filePath, subscriberId);
            var locationsByCustomerNumber = result.GetCompanyLocations(subscriberId, true);

            Location matchingLocation = null;
            locationsByCustomerNumber.TryGetValue("1863", out matchingLocation);

            if (syncLocations)
            {
                var persistedLocations = SyncLocations(locationsByCustomerNumber.Values, subscriberId, out locationErrorCount);
                if (syncJobs)
                {
                    var extractedJobs = result.GetJobs(persistedLocations.ToDictionary(p => p.LegacyId), StopActions, subscriberId);
                    Console.WriteLine("Detected {0} jobs", extractedJobs.Count);
                    foreach (var job in extractedJobs)
                    {
                        var routeStops = job.RouteStops.Select(x =>
                                                               new PAI.Drayage.Optimization.Model.Orders.RouteStop
                                                                   {
                                                                       Id = x.Id,
                                                                       ExecutionTime =
                                                                           x.StopDelay.HasValue ? new TimeSpan(x.StopDelay.Value) : (TimeSpan?) null,
                                                                       Location = new PAI.Drayage.Optimization.Model.Location
                                                                           {
                                                                               Id = x.Location.Id,
                                                                               DisplayName = x.Location.DisplayName,
                                                                               Latitude = x.Location.Latitude.HasValue ? x.Location.Latitude.Value : 0,
                                                                               Longitude = x.Location.Longitude.HasValue ? x.Location.Longitude.Value : 0
                                                                           },
                                                                       PostTruckConfig = new TruckConfiguration(),
                                                                       PreTruckConfig = new TruckConfiguration(),
                                                                       QueueTime = null,
                                                                       StopAction =
                                                                           PAI.Drayage.Optimization.Model.Orders.StopActions.Actions.First(
                                                                               y => y.ShortName == x.StopAction.ShortName),
                                                                       WindowEnd = new TimeSpan(x.WindowEnd),
                                                                       WindowStart = new TimeSpan(x.WindowStart)
                                                                   }).ToList();

                        var drayageJob = new PAI.Drayage.Optimization.Model.Orders.Job
                        {
                            Id = job.Id,
                            DisplayName = job.OrderNumber,
                            EquipmentConfiguration = new EquipmentConfiguration(),
                            IsFlatbed = job.IsFlatbed,
                            IsHazmat = job.IsHazmat,
                            RouteStops = routeStops
                        };

                        var validationResult = _validationService.ValidateJob(drayageJob, true, _distanceService);
                        job.IsValid = validationResult.Successful;
                        if (!validationResult.Successful)
                        {
                            if (job.RouteStops.Any(x => _validationService.InvalidTimes.Contains(new TimeSpan(x.WindowStart)) || _validationService.InvalidTimes.Contains(new TimeSpan(x.WindowEnd))))
                            {
                                job.RouteStops.First().StopAction = _stopActionService.GetById(Drayage.Optimization.Model.Orders.StopActions.DropOffLoadedWithChassis.Id);
                            }
                            ReportErrors.AddError(job.LegacyId + "\n" + validationResult.Errors.Aggregate((a, b) => a + "\n" + b));
                            errorJobCount++;
                        }
                        Job matchingJob = _jobService.SelectWithAll().FirstOrDefault(p => p.LegacyId == job.LegacyId);
                        if (matchingJob != null)
                        {
                            if (job.JobStatus == JobStatus.Completed)
                            {
                                Console.WriteLine("Order is already complete: {0}", job.LegacyId);
                                continue;
                            }

                            if (job.IsChangedFrom(matchingJob))
                            {
                                var updateResult = matchingJob.UpdateFrom(job);
                                if (updateResult.IsRouteStopsRecreated)
                                {
                                    recreatedRouteStopCount++;
                                }

                                if (updateResult.IsJobError)
                                {
                                    errorJobCount++;
                                }

                                if (saveToDatabase)
                                {
                                    _jobService.Update(matchingJob);
                                }

                                updatedJobCount++;
                            }
                            else
                            {
                                existingJobCount++;
                            }

                            continue;
                        }

                        Console.WriteLine("Adding new job");
                        createdJobCount++;

                        if (saveToDatabase)
                        {
                            _jobService.Insert(job);
                        }
                    }
                }
            }

            _syncLogEntryService.AddEntry(subscriberId, "Import Complete", "File: " + new FileInfo(filePath).Name,
                errorJobCount, updatedJobCount, createdJobCount, existingJobCount, recreatedRouteStopCount,
                locationErrorCount);

            return result;
        }
    }
}