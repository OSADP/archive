using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// Class to hold the data relating to a position snapshot
    /// </summary>
    [DataContract]
    public class PositionSnapshot
    {
        /// <summary>
        /// Latitude value
        /// </summary>
        [DataMember]
        public double Latitude;
        /// <summary>
        /// Longitude value
        /// </summary>
        [DataMember]
        public double Longitude;
        /// <summary>
        /// Speed in meters per second
        /// </summary>
        [DataMember]
        public double Speed;
        /// <summary>
        /// Heading value
        /// </summary>
        [DataMember]
        public short Heading;
        /// <summary>
        /// Timestamp in milliseconds since Jan 1st 1970
        /// </summary>
        [DataMember]
        public long TimeStamp;
        /// <summary>
        /// Number of Satellites reported by the GPS
        /// </summary>
        [DataMember]
        public short Satellites;
        /// <summary>
        /// Accuracy in meters
        /// </summary>
        [DataMember]
        public int Accuracy;
        /// <summary>
        /// Altitude in meters
        /// </summary>
        [DataMember]
        public int Altitude;
    }
}