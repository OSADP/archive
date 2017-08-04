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

namespace PAI.Drayage.Optimization.Model.Metrics
{
    /// <summary>
    /// Represents a trip length
    /// </summary>
    public struct TripLength
    {
        /// <summary>
        /// Gets or sets the distance (mi)
        /// </summary>
        public decimal Distance;

        /// <summary>
        /// Gets or sets the time
        /// </summary>
        public TimeSpan Time;

        
        public TripLength(decimal distance, TimeSpan time)
        {
            Distance = distance;
            Time = time;
        }

        public override string ToString()
        {
            return String.Format("{0}mi {1}", Distance, Time.ToString());
        }

        #region Operators

        public static TripLength operator +(TripLength c1, TripLength c2)
        {
            return new TripLength(c1.Distance + c2.Distance, c1.Time + c2.Time);
        }

        public static bool operator ==(TripLength c1, TripLength c2)
        {
            return (c1.Distance == c2.Distance && c1.Time == c2.Time);
        }

        public static bool operator !=(TripLength c1, TripLength c2)
        {
            return (c1.Distance != c2.Distance || c1.Time != c2.Time);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Distance.GetHashCode()*397) ^ Time.GetHashCode();
            }
        }

        public bool Equals(TripLength other)
        {
            return Distance == other.Distance && Time.Equals(other.Time);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TripLength && Equals((TripLength) obj);
        }

        #endregion


        public static TripLength MaxValue = new TripLength(decimal.MaxValue, TimeSpan.MaxValue);
        public static TripLength MinValue = new TripLength(decimal.MinValue, TimeSpan.MinValue);
        public static TripLength Zero = new TripLength(decimal.Zero, TimeSpan.Zero);
        
    }
}