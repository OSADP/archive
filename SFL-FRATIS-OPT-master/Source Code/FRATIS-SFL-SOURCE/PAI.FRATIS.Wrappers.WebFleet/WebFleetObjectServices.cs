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

using System.Collections.Generic;
using System.Linq;
using PAI.FRATIS.Wrappers.WebFleet.Mapping;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.ReportingService;
using PAI.FRATIS.Wrappers.WebFleet.Settings;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public interface IWebFleetObjectService
    {
        ICollection<WebFleetObject> GetObjects();
        ICollection<WebFleetDriver> GetDrivers();
        List<WebFleetObject> ShowVehicleReport(string objectNumber = "");
        ICollection<WebFleetObject> GetNearestVehicles(int latitudeInt, int longitudeInt, int maximumDistanceMiles);
    }

    public class WebFleetObjectService : IWebFleetObjectService
    {
        private readonly IWebFleetMappingService _mappingService;

        public WebFleetObjectService(IWebFleetMappingService mappingService)
        {
            _mappingService = mappingService;
        }

        public AuthenticationParameters GetAuthenticationParameters()
        {
            var auth = new AuthenticationParameters()
            {
                accountName = WebFleetSettings.AccountName,
                userName = WebFleetSettings.UserName,
                password = WebFleetSettings.Password,
                apiKey = WebFleetSettings.ApiKey
            };

            return auth;
        }

        public GeneralParameters GetGeneralParameters()
        {
            return new GeneralParameters
            {
                locale = KnownLocales.US,
                timeZone = KnownTimeZones.America_New_York
            };
        }

        public bool HandleResult(ServiceOpResult result)
        {
            return result.statusCode == 0;
            // TODO - Log Errors to Logger Service
        }

        public ICollection<WebFleetObject> GetObjects()
        {
            var result = new List<WebFleetObject>();
            var webService = new objectsAndPeopleReportingClient();
            var response = webService.showObjectReport(GetAuthenticationParameters(), GetGeneralParameters(),
                                        new ObjectFilterParameter()
                                            {
                                                filterCriterion = "",
                                                objectGroupName = "",
                                                @object = new ObjectIdentityParameter() { objectNo = "", objectUid = "" },
                                                ungroupedOnly = false,
                                                ungroupedOnlySpecified = false
                                            });
            
            if (HandleResult(response))
            {
                result.AddRange(from ObjectReport obj in response.results select _mappingService.Map(obj));
            }

            return result;
        }

        public ICollection<WebFleetDriver> GetDrivers()
        {
            var result = new List<WebFleetDriver>();
            var webService = new objectsAndPeopleReportingClient();
            var p = new DriverFilterParameter() {};
            var response = webService.showDriverReport(GetAuthenticationParameters(), GetGeneralParameters(), p);

            if (HandleResult(response))
            {
                result.AddRange(from DriverReport driver in response.results select _mappingService.Map(driver));
            }

            return result;
        }

        public List<WebFleetObject> ShowVehicleReport(string objectNumber)
        {
            var result = new List<WebFleetObject>();
            var webService = new objectsAndPeopleReportingClient();
            var objectFilter = new ObjectFilterParameter();
            if (objectNumber.Length > 0)
            {
                objectFilter.@object = new ObjectIdentityParameter()
                    {
                        objectNo = objectNumber
                    };
            };

            var response = webService.showObjectReport(GetAuthenticationParameters(), GetGeneralParameters(), objectFilter);

            if (HandleResult(response))
            {
                result.AddRange(from ObjectReport vehicle in response.results select _mappingService.Map(vehicle));
            }

            return result;
        }

        public ICollection<WebFleetObject> GetNearestVehicles(int latitudeInt, int longitudeInt, int maximumDistanceMiles)
        {
            var result = new List<WebFleetObject>();
            var webService = new objectsAndPeopleReportingClient();
            var response = webService.showNearestVehicles(GetAuthenticationParameters(), GetGeneralParameters(),
                                                          new ObjectGroupNameParameter(),
                                                          new NearestVehicleParameter()
                                                              {
                                                                  latitudeSpecified = true,
                                                                  longitudeSpecified = true,
                                                                  maxDistanceSpecified = true,
                                                                  latitude = latitudeInt,
                                                                  longitude = longitudeInt,
                                                                  maxDistance = maximumDistanceMiles
                                                              });
            if (HandleResult(response))
            {
                result.AddRange(from ObjectReport obj in response.results select _mappingService.Map(obj));
            }

            return result;
        }
    }
}
