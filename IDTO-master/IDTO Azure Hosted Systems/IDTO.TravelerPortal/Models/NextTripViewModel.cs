using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDTO.TravelerPortal.Common.Models;

namespace IDTO.TravelerPortal.Models
{
    public class NextTripViewModel
    {
        public string Destination { get; set; }

        public DateTime? TripStartDate { get; set; }

        public int? TotalDurationMin { get; set; }

        public string FirstStopString { get; set; }


        public NextTripViewModel()
        {
        }
        
        public NextTripViewModel(Trip trip)
        {
            if (trip == null)
                return;

            this.Destination = trip.Destination;


            this.TripStartDate = trip.TripStartDate;
            this.TotalDurationMin = trip.Duration_min();
            this.FirstStopString = trip.GetFirstStepString();


        }


    }
}