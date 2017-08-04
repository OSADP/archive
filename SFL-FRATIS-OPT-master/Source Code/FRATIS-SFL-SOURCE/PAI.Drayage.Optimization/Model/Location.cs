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

namespace PAI.Drayage.Optimization.Model
{
    /// <summary>
    /// Represents a geographical location
    /// </summary>
    public partial class Location : 
        ModelBase, 
        IComparable<Location>, 
        IEquatable<Location>
    {
        private const double Epsilon = 0.000001;

        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the longitude in degrees
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the latitude in degrees
        /// </summary>
        public double Latitude { get; set; }

        public virtual bool PortOfMiami { get; set; }

        public virtual bool PortEverglades { get; set; }

        public int CompareTo(Location other)
        {
            return this.GetHashCode() - other.GetHashCode();
        }
        
        public bool Equals(Location other)
        {
            return Math.Abs(this.Latitude - other.Latitude) < Epsilon
                   && Math.Abs(this.Longitude - other.Longitude) < Epsilon;
        }

        public override int GetHashCode()
        {
            return Longitude.GetHashCode() + Latitude.GetHashCode();
        }
    }
}