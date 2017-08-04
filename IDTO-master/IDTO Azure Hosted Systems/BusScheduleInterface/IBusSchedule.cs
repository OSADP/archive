using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDTO.Common;
using IDTO.Entity.Models;


namespace IDTO.BusScheduleInterface
{
    public interface IBusSchedule
    {
        Task<string> GetNextVehicleIDAsync(Step inboundStep);
        Task<List<IPrediction>> GetPredictionsAsync(string stopID, string routeID);
        Task<List<IPrediction>> GetPredictionsForStopsAsync(string[] stopIDs);
        Task<List<IPrediction>> GetPredictionsForVehiclesAsync(string[] vehicleIDs);
        Task<List<string>> GetRouteDirectionsAsync(string route);
        Task<List<IBusRoute>> GetRoutesAsync();
        Task<List<IStop>> GetStopsAsync(string route, string direction);
        DateTime GetTime();
        //Task<List<IVehicle>> GetVehiclesAsync(string[] vehicleIDs);
        Task<List<IVehicle>> GetVehiclesAsync(string route);
        Providers GetProviderId();
    }
}
