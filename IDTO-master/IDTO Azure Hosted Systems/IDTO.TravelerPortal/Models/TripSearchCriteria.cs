using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Models
{
    public class TripSearchCriteria
    {
        //public string StartLoc { get; set; }
        //public string EndLoc { get; set; }
        //public DateTime StartTime { get; set; }
        //public bool IsDeparture { get; set; }
        //public bool IsTaxiCABS { get; set; }
        //public bool IsZimRide { get; set; }
        //public bool HasBike { get; set; }
        public float startLatitude { get; set; }
        public float startLongitude { get; set; }
        [DisplayName("Start Location")]
        public string startLocation { get; set; }
        public float endLatitude { get; set; }
        public float endLongitude { get; set; }
        [DisplayName("End Location")]
        public string endLocation { get; set; }
        public bool searchByArrivByTime { get; set; }
        [DisplayName("Time")]
        public DateTime time { get; set; }
        public bool needWheelchairAccess { get; set; }
        [DisplayName("Max Walk (meters)")]
        public long maxWalkMeters { get; set; }

    }
}