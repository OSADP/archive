using IDTO.Common.Models;
using IDTO.Common;

using System.Collections.Generic;

using System.Threading.Tasks;
using System;

namespace IDTO.Mobile.Manager
{
    public class HomeDataManager
    {
		private TripManager mTripManager;

		public HomeDataManager()
		{
			mTripManager = new TripManager ();
		}

		public async Task<Trip> GetNextTrip(int travelerId)
        {
			List<Trip> inProgressTrips = await mTripManager.GetTripsByType (travelerId, TripType.Type.InProgress);
			if (inProgressTrips.Count > 0)
				return inProgressTrips [0];
			else {

				List<Trip> upcomingTrips = await mTripManager.GetTripsByType (travelerId, TripType.Type.Upcoming);

				if (upcomingTrips.Count > 0)
					return upcomingTrips [0];
			}
			return null;
        }

		public WeatherInfo GetWeather(double lat, double lon)
        {
			WeatherManager weatherManager = new WeatherManager ();
			return weatherManager.GetWeatherInfo (lat, lon);
        }

        public string GetTodayFormattedString()
        {
            DateTime now = DateTime.Now;

			String formattedDateString = now.ToString("MMMM d");

            return formattedDateString;
        }

        public void OnUpcomingTripButtonPress(object sender)
        {

        }

        public void OnTripHistoryButtonPress(object sender)
        {

        }

        public void OnPlanTripButtonPress(object sender)
        {

        }

        public void OnAccounButtonPress(object sender)
        {

        }

    }
}