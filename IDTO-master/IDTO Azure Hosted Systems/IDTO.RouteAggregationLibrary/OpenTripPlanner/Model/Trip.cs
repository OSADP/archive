namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class Trip
    {
        public Id id { get; set; }
        public object serviceId { get; set; }
        public object tripShortName { get; set; }
        public string tripHeadsign { get; set; }
        public object routeId { get; set; }
        public object directionId { get; set; }
        public object blockId { get; set; }
        public object shapeId { get; set; }
        public object wheelchairAccessible { get; set; }
        public object tripBikesAllowed { get; set; }
        public object route { get; set; }
    }
}