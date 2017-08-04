namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class StopTime
    {
        public int time { get; set; }
        public string phase { get; set; }
        public Trip trip { get; set; }
    }

    //public class StopTime2
    //{
    //    public StopTime StopTime { get; set; }

    //    public StopTime2()
    //    {
    //        StopTime = new StopTime {trip = new Trip()};
    //    }
    //}

}