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

namespace PAI.FRATIS.SFL.Domain.Geography.NokiaMaps
{
    public class RouteSummary
    {
        public double Distance { get; set; }

        public double TrafficTime { get; set; }

        public double BaseTime { get; set; }

        public string[] Flags { get; set; }

        public TimeSpan? TravelTime
        {
            get
            {
                return new TimeSpan(0, 0, Convert.ToInt32(this.BaseTime));
            }
        }

        public TimeSpan? TrafficTravelTime
        {
            get
            {
                return new TimeSpan(0, 0, Convert.ToInt32(this.TrafficTime));
            }
        }

        public TimeSpan? TrafficDelay
        {
            get
            {
                TimeSpan? result = null;
                if (this.TravelTime.HasValue && this.TrafficTravelTime.HasValue)
                {
                    result = this.TrafficTravelTime.Value - this.TravelTime.Value;
                }
                return result;
            }
        }

        public DateTime BaseDateTime
        {
            get
            {
                return new DateTime(long.Parse(this.BaseTime.ToString()));
            }
        }
        public double DistanceMiles
        {
            get
            {
                return Math.Floor(this.Distance / 160.934) / 10;
            }
        }

        public double Miles
        {
            get
            {
                return Math.Floor(this.Distance / 160.934) / 10;
            }
        }

    }
}
