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
    public class WebFleetPosition
    {
        public DateTime? TimeStamp { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double Score { get; set; }

        public int? LatitudeInt
        {
            get { return Latitude != null ? Convert.ToInt32(Latitude*1000000) : 0; }
            set { if (value != null) Latitude = value*.000001; }
        }

        public int? LongitudeInt
        {
            get { return Longitude != null ? Convert.ToInt32(Longitude * 1000000) : 0; }
            set { if (value != null) Longitude = value * .000001; }
        }

        public string PositionText { get; set; }

        public string PositionTextShort { get; set; }

        public WebFleetPositionMovement PositionMovement { get; set; }

        public bool HasValidPoints()
        {
            return Latitude != null && Longitude != null;
        }
    }
}
