using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class Plan
    {
        public long date { get; set; }
        public List<Itinerary> itineraries { get; set; }
    }
}
