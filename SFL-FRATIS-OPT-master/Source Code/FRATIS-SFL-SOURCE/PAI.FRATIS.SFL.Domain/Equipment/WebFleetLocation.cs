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
using PAI.FRATIS.SFL.Domain.Information;

namespace PAI.FRATIS.SFL.Domain.Equipment
{
    /// <summary>
    /// Represents the location of an object as reported by WEBFLEET
    /// </summary>
    public class WebFleetLocation : IPositionInfo
    {
        public DateTime? TimeStamp { get; set; }

        public string Description { get; set; }

        public string PositionText { get; set; }

        public int LegacyId { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}