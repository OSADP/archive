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
    public class WebFleetObject
    {
        public string ObjectNumber { get; set; }

        public string ObjectName { get; set; }

        public string ObjectTypeString
        {
            get { return ObjectType.ToString(); }
            set
            {
                try { ObjectType = (WebFleetObjectType) Enum.Parse(typeof (WebFleetObjectType), value, true); }
                catch { ObjectType = WebFleetObjectType.Unspecified; }
            }

        }

        public string LicensePlate { get; set; }

        public WebFleetObjectType ObjectType { get; set; }

        public string Description { get; set; }

        public string PositionText { get; set; }

        public long Odometer { get; set; }

        public WebFleetPosition Position { get; set; }

        public WebFleetPosition DestinationPosition { get; set; }

        public WebFleetDestinationEstimate DestinationEstimate { get; set; }
        
    }
}
