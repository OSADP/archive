namespace IDTO.BusScheduleInterface
{
    public interface IStop
    {
        string Direction { get; }
        int ID { get; }
        double Latitude { get; }
        double Longitude { get; }
        string Name { get; }
        string RouteID { get; }
    }
}
