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
    /// <summary>
    /// The queue device segment.
    /// </summary>
    public class QueueDeviceSegment : EntitySubscriberBase, IDatedEntity
    {
        public string DisplayName { get; set; }

        public string Device1Identifier { get; set; }

        public string Device2Identifier { get; set; }

        public int VehicleCount { get; set; }

        public long TotalDelay { get; set; }

        public int CurrentAverage { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// value from 0-47 incicating the 30 minute interval
        /// </summary>
        public int Segment { get; set; }

        public TimeSpan TimeSegmentStart
        {
            get
            {
                return new TimeSpan(0, Segment * 30, 0);
            }
        }

        public TimeSpan TimeSegmentEnd
        {
            get
            {
                return new TimeSpan(0, (Segment + 1) * 30, 0);
            }
        }
    }
}
