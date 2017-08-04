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
    public class WebFleetRouteEstimate
    {
        public DateTime? ArrivalDateTime { get; set; }

        public string TripDurationSeconds { get; set; }

        public TimeSpan TripDuration
        {
            get
            {
                var seconds = 0;
                Int32.TryParse(TripDurationSeconds, out seconds);
                return new TimeSpan(0, 0, seconds);
            }
            set
            {
                TripDurationSeconds = value.TotalSeconds.ToString();
            }
        }

        public WebFleetDistance Distance { get; set; }
        
        public long DelaySeconds { get; set; }

        public TimeSpan Delay
        {
            get
            {
                return new TimeSpan(0, 0, Convert.ToInt32(DelaySeconds));
            }
        }
    }
}
