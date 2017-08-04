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

namespace PAI.FRATIS.Wrappers.WebFleet.Model
{
    public class WebFleetDriver
    {
        public string ObjectNumber { get; set; }

        public string DriverNumber { get; set; }

        public string Name { get; set; }

        public string FirstName
        {
            get
            {
                if (Name != null)
                {
                    if (Name.IndexOf(",", System.StringComparison.Ordinal) > 0)
                    {
                        return Name.Substring(0, Name.IndexOf(",", System.StringComparison.Ordinal)).Trim();
                    }
                    else if (Name.IndexOf(' ') > 0)
                    {
                        return Name.Substring(0, Name.IndexOf(" ", System.StringComparison.Ordinal)).Trim();
                    }
                }
                return string.Empty;
            }
        }

        public string LastName
        {
            get
            {
                if (Name != null)
                {
                    if (Name.IndexOf(",", System.StringComparison.Ordinal) > 0)
                    {
                        return Name.Substring(Name.IndexOf(",", System.StringComparison.Ordinal) + 1).Trim();
                    }
                    else if (Name.IndexOf(' ') > 0)
                    {
                        return Name.Substring(Name.IndexOf(" ", System.StringComparison.Ordinal)).Trim();
                    }
                }
                return string.Empty;
            }
        }
        public string Company { get; set; }

        public string Pin { get; set; }

        public string Code { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string CurrentVehicleObjectNumber { get; set; }

        public WebFleetPosition CurrentPosition { get; set; }

        public WebFleetDriverWorkInfo WorkInfo { get; set; }
    }
}
