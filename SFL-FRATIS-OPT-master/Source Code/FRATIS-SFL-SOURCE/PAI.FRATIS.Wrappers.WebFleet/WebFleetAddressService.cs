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
using System.Globalization;
using System.Linq;
using PAI.FRATIS.SFL.Services.Geography;
using PAI.FRATIS.Wrappers.WebFleet.Mapping;
using PAI.FRATIS.Wrappers.WebFleet.Model;
using PAI.FRATIS.Wrappers.WebFleet.Settings;
using PAI.FRATIS.Wrappers.WebFleet.AddressService;

namespace PAI.FRATIS.Wrappers.WebFleet
{
    public class WebFleetGeocodeService : WebFleetAddressService, IGeocodeService
    {
        public WebFleetGeocodeService(IWebFleetMappingService mappingService) : base(mappingService)
        {
        }

        public GeocodeResult Geocode(string completeAddress)
        {
            throw new NotImplementedException();
        }

        public GeocodeResult Geocode(string houseNumber, string address, string city, string state, string zip)
        {
            var webFleetGeocode = this.GeocodeAddress(city, zip, address, houseNumber);
            return new GeocodeResult()
            {
                Latitude = webFleetGeocode.Latitude.HasValue ? webFleetGeocode.Latitude.Value : 0,
                Longitude = webFleetGeocode.Longitude.HasValue ? webFleetGeocode.Longitude.Value : 0,
                LocationString = webFleetGeocode.PositionText
            };
        }
    }

    public interface IWebFleetAddressService
    {
        WebFleetPosition GeocodeAddress(string city, string zip, string street, string streetNumber);

        WebFleetRouteEstimate CalculateRoute(WebFleetSettings settings, WebFleetPosition startPosition, WebFleetPosition endPosition,
                                                   bool useTraffic, DateTime? startDateTime = null);

        bool UpdateAddress(string identifier, string address1, string address2, string address3,
                           string city, string state, string zip, string email, string telephoneOffice, string telephoneMobile,
                           string telephonePrivate, string telephoneFax,
                           int? latitudeInt = null, int? longitudeInt = null);

        bool InsertAddress(
            string identifier,
            string name,
            string address1,
            string address2,
            string address3,
            string city,
            string state,
            string zip,
            string email,
            string telephoneOffice,
            string telephoneMobile,
            string telephonePrivate,
            string telephoneFax,
            string groupName = "",
            int? latitudeInt = null,
            int? longitudeInt = null,
            WebFleetAddressService.AddressColor color = WebFleetAddressService.AddressColor.Unspecified);

        bool InsertAddresses(
            ICollection<WebFleetAddress> addresses,
            string groupName = "",
            WebFleetAddressService.AddressColor color = WebFleetAddressService.AddressColor.Unspecified);

        bool DeleteAddress(string identifier);

        bool AttachAddressToGroup(string identifier, string groupName);

        bool DetachAddressToGroup(string identifier, string groupName);

        bool InsertAddressGroup(string groupName);

        bool DeleteAddressGroup(string groupName, bool deleteAddresses);

        ICollection<WebFleetAddress> GetAddresses(string filter = "", string groupName = "", bool ungroupedOnly = false);

        WebFleetAddress GetAddress(string id);

        ICollection<string> GetWebFleetAddressIdsByGroupName(string groupName);

        ICollection<WebFleetAddress> GetWebFleetAddressesByGroupName(string groupName);

    }



    public class WebFleetAddressService : IWebFleetAddressService
    {
        private readonly IWebFleetMappingService _mappingService;

        public WebFleetAddressService(IWebFleetMappingService mappingService)
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

        public WebFleetPosition GeocodeAddress(string city, string zip, string street, string streetNumber)
        {
            var webService = new addressClient();
            var result = new List<WebFleetPosition>();

            var response = webService.geocodeAddress(GetAuthenticationParameters(), GetGeneralParameters(),
                                      new GeocodingByProviderParameter()
                                          {
                                              city = city,
                                              countryCode = "US",
                                              postcode = zip,
                                              provider = "0",   // 1 for new TomTom geocoding service, 0 for legacy
                                              street = street,
                                              streetNumber = streetNumber
                                          });

            if (HandleResult(response))
            {
                result.AddRange(from CompleteLocationWithAdditionalInformation loc in response.results select _mappingService.Map(loc));
            }
            return result.Count > 0 ? result[0] : null;
        }

        public WebFleetRouteEstimate CalculateRoute(WebFleetSettings settings, WebFleetPosition startPosition, WebFleetPosition endPosition, bool useTraffic, DateTime? startDateTime = null)
        {
            if (!startPosition.HasValidPoints() || !endPosition.HasValidPoints())
                throw new Exception("Lat Long must be specified for start and end locations");

            var webService = new addressClient();
            var result = new List<WebFleetRouteEstimate>();

            var routingParam = new RoutingParameter()
                {
                    useTraffic = true,
                    useTrafficSpecified = true,
                    endLatitude = endPosition.LatitudeInt.Value,
                    endLongitude = endPosition.LongitudeInt.Value,
                    startLatitude = startPosition.LatitudeInt.Value,
                    startLongitude = startPosition.LongitudeInt.Value,
                    routeType = RouteType.Quickest
                };

            if (startDateTime != null)
            {
                routingParam.startDateTimeSpecified = true;
                routingParam.startDateTime = startDateTime;
            }

            var response = webService.calcRouteSimple(GetAuthenticationParameters(), GetGeneralParameters(), routingParam);
            
            if (HandleResult(response))
            {
                result.AddRange(from RoutingData route in response.results select _mappingService.Map(route));
            }

            return result.FirstOrDefault();
        }

        public bool InsertAddress(string identifier, string name, string address1, string address2, string address3,
                               string city, string state, string zip, string email, string telephoneOffice, string telephoneMobile,
                               string telephonePrivate, string telephoneFax, string groupName = "",
                               int? latitudeInt = null, int? longitudeInt = null, AddressColor color = AddressColor.Unspecified)
        {
            var webService = new addressClient();

            var isLatLongProvided = latitudeInt.HasValue && longitudeInt.HasValue;

            AddressGroup addressGroup = null;
            if (groupName.Length > 0)
            {
                addressGroup = new AddressGroup() { uniqueName = groupName };
            }

            var contact = new ContactData()
                              {
                                  phoneMobile = telephoneMobile,
                                  phoneBusiness = telephoneOffice,
                                  phonePersonal = telephonePrivate,
                                  emailAddress = email
                              };

            var geoPosition = new GeoPosition()
                                  {
                                      latitudeSpecified = isLatLongProvided,
                                      longitudeSpecified = isLatLongProvided
                                  };
            if (isLatLongProvided)
            {
                geoPosition.latitude = latitudeInt.Value;
                geoPosition.longitude = longitudeInt.Value;

            }

            var response = webService.insertAddress(
                GetAuthenticationParameters(),
                GetGeneralParameters(),
                new Address()
                    {
                        info = string.Empty,
                        addressNo = identifier,
                        name1 = name,
                        name2 = "",
                        name3 = "",
                        contact = contact,
                        location = new DescribedLocation()
                        {
                            city = city,
                            street = address1,
                            postcode = zip,
                            addrRegion = state,
                            geoPosition = geoPosition,
                        },
                        colourSpecified = true,
                        colour = GetAddressColour(color)
                    },
                addressGroup);
            return HandleResult(response);
        }


        public enum AddressColor
        {
            Unspecified,
            Green,
            Red,
            Blue,
            Orange

        }

        public AddressColour GetAddressColour(AddressColor color)
        {
            return GetAddressColour(color.ToString().ToLower());
        }

        private AddressColour GetAddressColour(string name)
        {
            switch (name.ToLower())
            {
                case "green":
                case "lightgreen":
                case "grassgreen":
                    return AddressColour.grassgreen;
                case "red":
                case "darkred":
                    return AddressColour.darkred;
                case "blue":
                    return AddressColour.brightblue;
                case "orange":
                    return AddressColour.brightorange;
            }

            return AddressColour.khaki;
        }

        public bool InsertAddresses(ICollection<WebFleetAddress> addresses, string groupName = "", AddressColor color = AddressColor.Unspecified)
        {
            var webService = new addressClient();
            var groupNameFailure = false;

            var x = new Address();
            var failCount = addresses.Count(address => !InsertAddress(address.WebFleetId, address.DisplayName, 
                address.StreetAddress, string.Empty, string.Empty, address.City, address.State, address.Zip, 
                address.Email, string.Empty, address.Phone, string.Empty, string.Empty, groupName, address.LatitudeInt, address.LongitudeInt, color));

            return failCount == 0;
        }

        public bool UpdateAddress(string identifier, string address1, string address2, string address3,
                       string city, string state, string zip, string email, string telephoneOffice, string telephoneMobile,
                       string telephonePrivate, string telephoneFax,
                       int? latitudeInt = null, int? longitudeInt = null)
        {
            var webService = new addressClient();

            var contact = new ContactData()
                              {
                                  phoneBusiness = telephoneOffice,
                                  phonePersonal = telephonePrivate,
                                  phoneMobile = telephoneMobile,
                                  emailAddress = email
                              };

            var response = webService.updateAddress(GetAuthenticationParameters(), GetGeneralParameters(),
                                                    new Address()
                                                        {
                                                            info = string.Empty,
                                                            addressNo = identifier,
                                                            name1 = address1,
                                                            name2 = address2,
                                                            name3 = address3,
                                                            location = new DescribedLocation()
                                                                {
                                                                    street = address1,
                                                                    city = city,
                                                                    addrRegion = state,
                                                                    postcode = zip,
                                                                    geoPosition = new GeoPosition()
                                                                        {
                                                                            latitudeSpecified = latitudeInt.HasValue,
                                                                            latitude = latitudeInt,
                                                                            longitudeSpecified = longitudeInt.HasValue,
                                                                            longitude = longitudeInt
                                                                        }
                                                                },
                                                                contact = contact
                                                        });
            return HandleResult(response);
        }

        public bool DeleteAddress(string identifier)
        {
            var webService = new addressClient();
            var response = webService.deleteAddress(GetAuthenticationParameters(), GetGeneralParameters(),
                                                    new AddressIdentity() {addressNo = identifier});
            return HandleResult(response);            
        }

        public bool AttachAddressToGroup(string identifier, string groupName)
        {
            var webService = new addressClient();
            var response = webService.attachAddressToGroup(GetAuthenticationParameters(), GetGeneralParameters(),
                                                           new AddressToGroupRelationship()
                                                               {
                                                                   address = new AddressIdentity() {addressNo = identifier},
                                                                   group = new AddressGroup() {uniqueName = groupName}
                                                               });
            return HandleResult(response);
        }

        public bool DetachAddressToGroup(string identifier, string groupName)
        {
            var webService = new addressClient();
            var response = webService.detachAddressFromGroup(GetAuthenticationParameters(), GetGeneralParameters(),
                                                           new AddressToGroupRelationship()
                                                           {
                                                               address = new AddressIdentity() { addressNo = identifier },
                                                               group = new AddressGroup() { uniqueName = groupName }
                                                           });
            return HandleResult(response);
        }

        public bool InsertAddressGroup(string groupName)
        {
            var webService = new addressClient();
            var response = webService.insertAddressGroup(GetAuthenticationParameters(), GetGeneralParameters(),
                                                         new AddressGroup() {uniqueName = groupName});
            return HandleResult(response);
        }

        public bool DeleteAddressGroup(string groupName, bool deleteAddresses)
        {
            var webService = new addressClient();
            var response = webService.deleteAddressGroup(GetAuthenticationParameters(), GetGeneralParameters(),
                                                         new AddressGroup() {uniqueName = groupName},
                                                         new AdvancedDeleteAddressGroupParameter()
                                                             {
                                                                 deleteAddresses = deleteAddresses,
                                                                 deleteAddressesSpecified = true
                                                             });
            return HandleResult(response);
        }

        public ICollection<WebFleetAddress> GetAddresses(string filter = "", string groupName = "", bool ungroupedOnly = false)
        {
            var webService = new addressClient();
            var result = new List<WebFleetAddress>();

            var addressFilter = new AddressFilterParameter
                {
                    filterCriterion = filter,  // filters only display name
                    addressGroupName = groupName
                };

            if (ungroupedOnly)
            {
                addressFilter.ungroupedOnlySpecified = true;
                addressFilter.ungroupedOnly = true;
            }

            var response = webService.showAddressReport(GetAuthenticationParameters(), GetGeneralParameters(), addressFilter);
            if (HandleResult(response))
            {
                result.AddRange(from Address address in response.results select _mappingService.Map(address));
            }
            return result;
        }

        public WebFleetAddress GetAddress(string id)
        {
            return GetAddresses(id, string.Empty).FirstOrDefault();
        }

        public ICollection<string> GetWebFleetAddressIdsByGroupName(string groupName)
        {
            var webService = new addressClient();
            var result = new List<string>();

            var response = webService.showAddressGroupAddressReport(
                GetAuthenticationParameters(),
                GetGeneralParameters(),
                new FilterParameter() { filterCriterion = groupName });

            if (HandleResult(response))
            {
                result.AddRange(from AddressToGroupRelationship rel in response.results select rel.address.addressNo);
            }

            return result;
        }

        public ICollection<WebFleetAddress> GetWebFleetAddressesByGroupName(string groupName)
        {
            var allAddresses = GetAddresses();
            var matchingAddressIds = GetWebFleetAddressIdsByGroupName(groupName);

            return allAddresses.Where(p => matchingAddressIds.Contains(p.WebFleetId)).ToList();
            
        }

    }
}
