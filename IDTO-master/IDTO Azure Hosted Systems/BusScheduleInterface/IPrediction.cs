namespace IDTO.BusScheduleInterface
{
    public interface IPrediction
    {
        int DistanceToStop { get; }
        string FinalDestination { get; }
        bool IsDelayed { get; }
        bool IsDelaySpecified { get; }
        string PredictedTimeOfArrivalOrDeparture { get; }
        string RouteDirection { get; }
        string RouteID { get; }
        int StopID { get; }
        string StopName { get; }
        string TABlockID { get; }
        string TATripID { get; }
        string TimeStamp { get; }
        string Type { get; }
        string VehicleID { get; }
        string Zone { get; }
    }
}
