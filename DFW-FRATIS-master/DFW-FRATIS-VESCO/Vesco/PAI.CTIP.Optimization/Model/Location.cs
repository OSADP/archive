//    Copyright 2013 Productivity Apex Inc.
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

namespace PAI.CTIP.Optimization.Model
{
    /// <summary>
    /// Represents a geographical location
    /// </summary>
    public partial class Location : ModelBase
    {
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the Street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the Zip
        /// </summary>
        public string Zip { get; set; }

        public string Address
        {
            get
            {
                return string.Format("{0} {1}, {2} {3}", Street, City, State, Zip);
            }
        }
        
        /// <summary>
        /// Gets or sets the longitude in degrees
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the latitude in degrees
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Vesco attributes
        /// </summary>
        public int LiveLoadPad { get; set; }
        public int Ranking { get; set; }
        public int OrderType { get; set; }
        public bool LongHaul { get; set; }
        public bool ShortHaul { get; set; }
    }
}