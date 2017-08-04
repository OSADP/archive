using System.Collections.Generic;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class StopList   
    {
        public List<Stop> stops { get; set; }

        public StopList()
        {
            stops = new List<Stop>();
        }
    }
}