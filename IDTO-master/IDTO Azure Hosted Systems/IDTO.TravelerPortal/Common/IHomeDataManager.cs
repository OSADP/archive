using System;
namespace IDTO.TravelerPortal.Common
{
    public interface IHomeDataManager
    {
        System.Threading.Tasks.Task<IDTO.TravelerPortal.Common.Models.TripSummaryForDelete> CancelTrip(int tripId);
        IDTO.TravelerPortal.Common.Models.Trip GetNextTrip(string email);
        System.Threading.Tasks.Task<IDTO.TravelerPortal.Common.Models.Trip> GetNextTripAsync(string email);
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDTO.TravelerPortal.Common.Models.Trip>> GetPastTrips(string email);
        string GetTodayFormattedString();
        System.Threading.Tasks.Task<System.Collections.Generic.List<IDTO.TravelerPortal.Common.Models.Trip>> GetUpcomingTrips(string email);
        IDTO.TravelerPortal.Common.Models.WeatherInfo GetWeather(double lat, double lon);
    }
}
