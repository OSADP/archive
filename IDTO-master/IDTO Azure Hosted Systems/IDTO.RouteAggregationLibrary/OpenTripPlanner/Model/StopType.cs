using System.Collections.Generic;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class Stop
    {
        public Id id { get; set; }
        public string stopName { get; set; }
        public double stopLat { get; set; }
        public double stopLon { get; set; }
        public string stopCode { get; set; }
        public object stopDesc { get; set; }
        public object zoneId { get; set; }
        public object stopUrl { get; set; }
        public object locationType { get; set; }
        public object parentStation { get; set; }
        public object wheelchairBoarding { get; set; }
        public object direction { get; set; }
        public List<Route> routes { get; set; }
    }
}