using IDTO.TravelerPortal.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IDTO.TravelerPortal.Common
{
    public class HomeDataManager : IDTO.TravelerPortal.Common.IHomeDataManager
    {
        private TripManager mTripManager;
        private WeatherManager weatherManager;
        private AccountManager accountManager;

        public HomeDataManager()
        {
            mTripManager = new TripManager ();
            weatherManager = new WeatherManager();
            accountManager = new AccountManager();

        }

        public  Trip GetNextTrip(string email)
        {
            TravelerModel traveler = accountManager.GetTravelerByEmail(email);
            if (traveler == null)
                return null;

            List<Trip> upcomingTrips = mTripManager.GetTripsByType(traveler.Id, TripType.Type.Upcoming);

            if (upcomingTrips.Count > 0)
                return upcomingTrips[0];

            return null;
        }

        public async Task<TripSummaryForDelete> CancelTrip(int tripId)
        {
            TripSummaryForDelete deletedTrip = await mTripManager.CancelTrip(tripId);
            return deletedTrip;
        }

        public async Task<Trip> GetNextTripAsync(string email)
        {
            TravelerModel traveler = accountManager.GetTravelerByEmail(email);
            List<Trip> upcomingTrips = await mTripManager.GetTripsByTypeAsync(traveler.Id, TripType.Type.Upcoming);

            if (upcomingTrips.Count > 0)
                return upcomingTrips[0];

            return null;
        }

        public async Task<List<Trip>> GetUpcomingTrips(string email)
        {
            TravelerModel traveler = accountManager.GetTravelerByEmail(email);
            List<Trip> upcomingTrips = await mTripManager.GetTripsByTypeAsync(traveler.Id, TripType.Type.Upcoming);

            if (upcomingTrips.Count > 0)
                return upcomingTrips;

            return null;
        }

        public async Task<List<Trip>> GetPastTrips(string email)
        {
            TravelerModel traveler = accountManager.GetTravelerByEmail(email);
            List<Trip> upcomingTrips = await mTripManager.GetTripsByTypeAsync(traveler.Id, TripType.Type.Past);

            if (upcomingTrips.Count > 0)
                return upcomingTrips;

            return null;
        }

        public WeatherInfo GetWeather(double lat, double lon)
        {
            return weatherManager.GetWeatherInfo (lat, lon);
        }

        public string GetTodayFormattedString()
        {
            DateTime now = DateTime.Now;

            String formattedDateString = now.ToString("MMMM d");

            return formattedDateString;
        }
    }
}