namespace IDTO.RouteAggregationLibrary.OpenTripPlanner.Model
{
    public class ServerInfo
    {
        public ServerVersion serverVersion { get; set; }
        public string cpuName { get; set; }
        public int nCores { get; set; }

        public ServerInfo()
        {
            serverVersion = new ServerVersion();
        }
    }
}