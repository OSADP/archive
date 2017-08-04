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

using PAI.FRATIS.SFL.Domain.Geography.NokiaMaps.TrafficItems;

namespace PAI.FRATIS.SFL.Domain.Geography.NokiaMaps
{
    public class TrafficItem
    {
        public long TrafficItemId { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public string EntryTime { get; set; }

        public bool Verified { get; set; }

        public string Comments { get; set; }

        public string TrafficItemTypeDesc { get; set; }

        public string TrafficItemStatusShortDesc { get; set; }

        public Criticality Criticality { get; set; }

        public Location Location { get; set; }

        public TrafficItemDetail TrafficItemDetail { get; set; }

        public TrafficItemDescription[] TrafficItemDescription { get; set; }
    }
}