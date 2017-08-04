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

namespace PAI.Drayage.Optimization.Model.Orders
{
    /// <summary>
    /// Represents a driver
    /// </summary>
    public partial class Driver : ModelBase
    {
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the starting location
        /// </summary>
        public virtual Location StartingLocation { get; set; }

        /// <summary>
        /// Gets or sets the earliest start time as a TimeSpan
        /// </summary>
        public virtual TimeSpan EarliestStartTimeSpan
        {
            get { return new TimeSpan(EarliestStartTime); }
        }

        /// <summary>
        /// Gets or sets the earliest start time
        /// </summary>
        public virtual long EarliestStartTime { get; set; }

        /// <summary>
        /// Gets or sets the available duty time
        /// </summary>
        public virtual double AvailableDutyHours { get; set; }

        /// <summary>
        /// Gets or sets the available driving time
        /// </summary>
        public virtual double AvailableDrivingHours { get; set; }

        /// <summary>
        /// Gets or sets the available duty time
        /// </summary>
        public TimeSpan AvailableDutyTime
        {
            get { return TimeSpan.FromHours(AvailableDutyHours); }
        }

        /// <summary>
        /// Gets or sets the available driving time
        /// </summary>
        public TimeSpan AvailableDrivingTime
        {
            get { return TimeSpan.FromHours(AvailableDrivingHours); }
        }

        public bool IsHazmat { get; set; }

        public virtual bool PortOfMiami { get; set; }

        public virtual bool PortEverglades { get; set; }

        public bool IsFlatbed { get; set; }

        public double Priority { get; set; }
     
    }
}