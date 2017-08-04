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
using System.Linq;

namespace PAI.FRATIS.SFL.Domain.Geography
{
    /// <summary>
    /// Represents a trip length between locations 
    /// This entity is used to store expensive trip lengths between locations
    /// </summary>
    public partial class LocationDistance : EntitySubscriberBase, IDatedEntity
    {
        public virtual Location StartLocation { get; set; }

        public virtual Location EndLocation { get; set; }

        public int? DayOfWeek { get; set; }

        public DateTime? DepartureDay { get; set; }

        /// <summary>
        /// Gets or sets the start location id
        /// </summary>
        public int? EndLocationId { get; set; }

        public int? StartLocationId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        /// <summary>Gets or sets the distance in miles.</summary>
        public decimal? Distance { get; set; }

        /// <summary>
        /// Gets or sets the average travel time without traffic considerations
        /// </summary>
        public long? TravelTime { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether all Travel Times and Distances
        /// have been set
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return Hours.All(t => t.TravelTime != null) 
                    && Distance != null 
                    && TravelTime != null;
            }
        }
        public HourDistanceInfo Hour0 { get { return _hours[0]; } set { _hours[0] = value; } }
        public HourDistanceInfo Hour1 { get { return _hours[1]; } set { _hours[1] = value; } }
        public HourDistanceInfo Hour2 { get { return _hours[2]; } set { _hours[2] = value; } }
        public HourDistanceInfo Hour3 { get { return _hours[3]; } set { _hours[3] = value; } }
        public HourDistanceInfo Hour4 { get { return _hours[4]; } set { _hours[4] = value; } }
        public HourDistanceInfo Hour5 { get { return _hours[5]; } set { _hours[5] = value; } }
        public HourDistanceInfo Hour6 { get { return _hours[6]; } set { _hours[6] = value; } }
        public HourDistanceInfo Hour7 { get { return _hours[7]; } set { _hours[7] = value; } }
        public HourDistanceInfo Hour8 { get { return _hours[8]; } set { _hours[8] = value; } }
        public HourDistanceInfo Hour9 { get { return _hours[9]; } set { _hours[9] = value; } }
        public HourDistanceInfo Hour10 { get { return _hours[10]; } set { _hours[10] = value; } }
        public HourDistanceInfo Hour11 { get { return _hours[11]; } set { _hours[11] = value; } }
        public HourDistanceInfo Hour12 { get { return _hours[12]; } set { _hours[12] = value; } }
        public HourDistanceInfo Hour13 { get { return _hours[13]; } set { _hours[13] = value; } }
        public HourDistanceInfo Hour14 { get { return _hours[14]; } set { _hours[14] = value; } }
        public HourDistanceInfo Hour15 { get { return _hours[15]; } set { _hours[15] = value; } }
        public HourDistanceInfo Hour16 { get { return _hours[16]; } set { _hours[16] = value; } }
        public HourDistanceInfo Hour17 { get { return _hours[17]; } set { _hours[17] = value; } }
        public HourDistanceInfo Hour18 { get { return _hours[18]; } set { _hours[18] = value; } }
        public HourDistanceInfo Hour19 { get { return _hours[19]; } set { _hours[19] = value; } }
        public HourDistanceInfo Hour20 { get { return _hours[20]; } set { _hours[20] = value; } }
        public HourDistanceInfo Hour21 { get { return _hours[21]; } set { _hours[21] = value; } }
        public HourDistanceInfo Hour22 { get { return _hours[22]; } set { _hours[22] = value; } }
        public HourDistanceInfo Hour23 { get { return _hours[23]; } set { _hours[23] = value; } }

        private List<HourDistanceInfo> _hours;
        public List<HourDistanceInfo> Hours
        {
            get
            {
                return _hours;
            }
            set
            {
                _hours = value;
            }
        }

        /// <summary>Initializes a new instance of the <see cref="LocationDistance"/> class.</summary>
        public LocationDistance()
        {
            _hours = new List<HourDistanceInfo>();
            for (int i = 0; i < 24; i++)
            {
                _hours.Add(new HourDistanceInfo());
            } 
        }
    }
}