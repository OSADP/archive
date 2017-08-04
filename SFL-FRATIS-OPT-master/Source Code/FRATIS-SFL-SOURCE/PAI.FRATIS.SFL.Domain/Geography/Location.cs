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

using System.Collections.Generic;

namespace PAI.FRATIS.SFL.Domain.Geography
{
    /// <summary>
    /// Represents a geographical location
    /// </summary>
    public partial class Location : EntitySubscriberBase, IArchivedEntity
    {
        /// <summary>
        /// Gets or sets the Web Fleet Location Id
        /// </summary>
        public virtual string WebFleetId { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the street number
        /// </summary>
        public string StreetNumber { get; set; }

        /// <summary>
        /// Gets or sets the address
        /// </summary>
        public virtual string Street { get; set; }

        /// <summary>
        /// Gets the street address
        /// Consists of StreetNumber and Street
        /// </summary>
        public string StreetAddress
        {
            get
            {
                return string.Format("{0} {1}", "", Street).Trim();
            }

            set
            {
                Street = value;
            }
        }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// Gets or sets the state
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// Gets or sets the zip
        /// </summary>
        public virtual string Zip { get; set; }

        /// <summary>
        /// Gets or sets the longitude in degrees
        /// </summary>
        public virtual double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the latitude in degrees
        /// </summary>
        public virtual double? Latitude { get; set; }

        public string Note { get; set; }

        public string LegacyId { get; set; }

        public string Phone { get; set; }

        public int? WaitingTime { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsFailedGeocode{ get; set; }

        /// <summary>
        /// Indicates whether changes detected to this location's details via mapping should be 
        /// persisted to the backend, or ignored - used for integration synchronization
        /// </summary>
        public bool IgnoreLocationChanges { get; set; }

        /// <summary>
        /// Gets or sets whether this location should force pass validation
        /// </summary>
        public bool IsValidated { get; set; }

        public bool PortOfMiami { get; set; }

        public bool PortEverglades { get; set; }
    }
}