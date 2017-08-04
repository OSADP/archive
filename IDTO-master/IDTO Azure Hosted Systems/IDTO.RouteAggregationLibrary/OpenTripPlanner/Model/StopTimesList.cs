using System.Collections.Generic;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class StopTimesList
    {
        public List<StopTime> stopTimes { get; set; }

        public StopTimesList()
        {
            stopTimes = new List<StopTime>();
        }
    }
}