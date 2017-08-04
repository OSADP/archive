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
using System.Globalization;

namespace PAI.FRATIS.Wrappers.WebFleet.Model
{
    public class WebFleetAddress
    {
        public string WebFleetId { get; set; }
        public string DisplayName { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        public int? LatitudeInt { get; set; }
        public int? LongitudeInt { get; set; }

        public double? Latitude
        {
            get
            {
                return LatitudeInt.HasValue ? LatitudeInt * .000001 : null;
            }
        }

        public double? Longitude
        {
            get
            {
                return LongitudeInt.HasValue ? LongitudeInt * .000001 : null;
            }
        }

        public string ContactName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        private int? _streetNumber = null;
        public int StreetNumber
        {
            get
            {
                if (!_streetNumber.HasValue)
                {
                    int result = 0;
                    if (StreetAddress != null && StreetAddress.IndexOf(' ') > 0)
                    {
                        Int32.TryParse(StreetAddress.Substring(0, StreetAddress.IndexOf(' ')).Trim(), out result);
                    }
                    _streetNumber = result;
                }
                return _streetNumber.Value;
            }
        }

        public string Street
        {
            get
            {
                if (StreetNumber > 0)
                {
                    var i = StreetAddress.IndexOf(StreetNumber.ToString(CultureInfo.InvariantCulture), System.StringComparison.Ordinal);
                    if (i >= 0)
                    {
                        return StreetAddress.Substring(i + StreetNumber.ToString().Length).Trim();
                    }
                }
                return StreetAddress;
            }
        }

        public WebFleetAddress()
        {
            WebFleetId = string.Empty;
            DisplayName = string.Empty;
            Name2 = string.Empty;
            Name3 = string.Empty;
            StreetAddress = string.Empty;
            City = string.Empty;
            State = string.Empty;
            Zip = string.Empty;
            ContactName = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
        }
    }
}
