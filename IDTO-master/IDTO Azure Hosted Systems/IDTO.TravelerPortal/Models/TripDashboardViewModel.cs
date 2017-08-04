using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDTO.TravelerPortal.Common.Models;

namespace IDTO.TravelerPortal.Models
{
    public class TripDashboardViewModel
    {
        public List<Trip> UpcomingTrips { get; set; }
        public List<Trip> PastTrips { get; set; }

        public TripSearchCriteria SearchCriteria { get; set; }

        public TripDashboardViewModel()
        {

        }
    }
}