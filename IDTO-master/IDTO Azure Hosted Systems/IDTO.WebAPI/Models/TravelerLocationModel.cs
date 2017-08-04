using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using IDTO.Entity.Models;

namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// The location of the traveler at a point in time.
    /// </summary>
    [DataContract]
    public class TravelerLocationModel
    {
                /// <summary>
        /// Unique identifier created by the database after traveler is added.
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// User authentication id.
        /// </summary>
        [DataMember]
        public string UserId { get; set; }
        /// <summary>
        /// Unique identifier of the traveler (looked up by the backend for the supplied Userid).
        /// </summary>
        [DataMember]
        public int TravelerId { get; set; }
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
        /// Will be a real timestamp in UTC coming from GPS.
        /// </summary>
        [DataMember]
        public DateTime TimeStamp;


         public TravelerLocationModel()
        {
        }
        public TravelerLocationModel(TravelerLocation t, string userId)
        {
           this.Id = t.Id;
           this.UserId = userId;
           this.TravelerId = t.TravelerId;
           this.Latitude = t.Latitude;
           this.Longitude = t.Longitude;
           this.TimeStamp = t.PositionTimestamp;
           
            
       
        }
    }
}