namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class ServerVersion
    {
        public string version { get; set; }
        public int major { get; set; }
        public int minor { get; set; }
        public int incremental { get; set; }
        public string qualifier { get; set; }
        public string commit { get; set; }
        public int uid { get; set; }
    }
}