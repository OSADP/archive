namespace IDTO.BusScheduleInterface
{
    public interface IVehicle
    {
        bool blkSpecified { get; }
        int BlockID { get; }
        string Destination { get; }
        int Heading { get; }
        string ID { get; }
        bool IsDelayed { get; }
        double Latitude { get; }
        double Longitude { get; }
        int PatternDistance { get; }
        int PatternID { get; }
        string RouteID { get; }
        string ServerTimeStamp { get; }
        int Speed { get; }
        string tablockid { get; }
        string tatripid { get; }
        string TimeStamp { get; }
        int TripID { get; }
        bool tripidSpecified { get; }
        string Zone { get; }
    }
}
