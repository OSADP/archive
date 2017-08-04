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
using System.Linq;

namespace PAI.FRATIS.SFL.Domain.Information
{
    public interface IPositionInfo
    {
        int LegacyId { get; set; }

        double? Latitude { get; set; }

        double? Longitude { get; set; }

        string PositionText { get; set; }

        DateTime? TimeStamp { get; set; }

        string Description { get; set; }
    }

    public class PositionInfo : IPositionInfo
    {
        public int LegacyId { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public string PositionText { get; set; }

        public DateTime? TimeStamp { get; set; }

        public string Description { get; set; }
    }
}
