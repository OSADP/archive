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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using PAI.FRATIS.SFL.Domain.Equipment;
using PAI.FRATIS.SFL.Domain.Geography;
using PAI.FRATIS.SFL.Domain.Planning;

namespace PAI.FRATIS.SFL.Domain.Orders
{
    /// <summary>
    /// Represents a driver
    /// </summary>
    public sealed class Driver : EntitySubscriberBase, IArchivedEntity
    {
        public string DisplayName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName).Trim();
            }
            set
            {
                if (value.IndexOf(',') > 0)
                {
                    LastName = value.Substring(0, value.IndexOf(',')).Trim();
                    FirstName = value.Substring(value.IndexOf(',') + 1).Trim();
                }
                else if (value.IndexOf(' ') > 0)
                {
                    FirstName = value.Substring(0, value.IndexOf(' ')).Trim();
                    LastName = value.Substring(value.IndexOf(' ') + 1).Trim();
                }
                else
                {
                    FirstName = value;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the Driver First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the Driver Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the starting location of the driver
        /// </summary>
        public Location StartingLocation { get; set; }

        /// <summary>Gets or sets the driver type.</summary>
        public string DriverType { get; set; }

        /// <summary>Gets or sets the phone.</summary>
        public string Phone { get; set; }

        /// <summary>Gets or sets the email.</summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the starting location id
        /// </summary>
        public int? StartingLocationId { get; set; }

        /// <summary>
        /// Gets or sets the earliest start time
        /// </summary>
        public long EarliestStartTime { get; set; }

        public TimeSpan EarliestStartTimeSpan
        {
            get { return new TimeSpan(EarliestStartTime); }
        }

        public DateTime EarliestStartDateTime
        {
            get
            {
                return DateTime.UtcNow.Date.AddTicks(EarliestStartTime).AddHours(8);
            }
        }

        /// <summary>
        /// Gets or sets the available duty time
        /// </summary>
        public double AvailableDutyHours { get; set; }

        /// <summary>
        /// Gets or sets the available driving time
        /// </summary>
        public double AvailableDrivingHours { get; set; }

        /// <summary>
        /// Gets or sets a value representing if this is a dummy/placeholder driver
        /// </summary>
        public bool IsPlaceholderDriver { get; set; }

        public string LegacyId { get; set; }

        public Driver()
        {
            AvailableDrivingHours = 11;
            AvailableDutyHours = 14;
            JobGroups = new Collection<JobGroup>();
            Position = new WebFleetLocation();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2}", FirstName, LastName, Id);
        }

        /// <summary>
        /// Gets or sets the JobGroups that the given driver is a member of
        /// </summary>
        public ICollection<JobGroup> JobGroups { get; set; } 

        /// <summary>
        /// Represents the most recently reported Driver position
        /// </summary>
        public WebFleetLocation Position { get; set; }

        public bool IsHazmat { get; set; }

        public bool PortOfMiami { get; set; }

        public bool PortEverglades { get; set; }

        public bool IsFlatbed { get; set; }

        public bool IsDeleted { get; set; }

        //TODO remove when DB is updated with a Priority field
        [NotMapped]
        public double Priority { get; set; }
    }
}