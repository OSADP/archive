using IDTO.Common.Models;
using IDTO.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;

namespace IDTO.Mobile.Manager
{
    public class UserTripDataManager
    {
		private TripManager mTripManager;
		private const double QUARTER_MILE_IN_METERS = 402.336;
		public UserTripDataManager()
		{
			mTripManager = new TripManager ();
		}
        //async public List<Trip> GetUpcomingTrips(UserId)
    //   {
        //}

		public static DateTime convertEpochTimeToDateTime(long time_ms)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(time_ms);
		}

		public async Task<TripSearchResult> SearchForTrips(string startLocation, string endLocation, DateTime date, bool isDeparture, double maxWalkDistanceMeters = QUARTER_MILE_IN_METERS)
		{
			TripSearchResult searchResult = await SearchForTrips (startLocation, endLocation, maxWalkDistanceMeters, date, isDeparture, "", "");

			return searchResult;

		}

		public async Task<TripSearchResult> SearchForTrips(string startLocation, string endLocation, double maxWalkMeters, DateTime date, bool isDeparture, String cityString, String stateString)
		{

			TripSearch criteria = new TripSearch();
			criteria.MaxWalkMeters = maxWalkMeters;
			criteria.NeedWheelchairAccess = false;
			criteria.SearchByArriveByTime = !isDeparture;
			criteria.Time = date;

			criteria = parseLocationString (criteria, startLocation, true, cityString, stateString);
			criteria = parseLocationString (criteria, endLocation, false, cityString, stateString);

			TripSearchResult searchResult = await mTripManager.SearchForTrip (criteria);

			if (searchResult != null) {
				searchResult.searchCriteria = criteria;
				searchResult.itineraries.Sort ((a, b) => a.startTime.CompareTo (b.startTime));
			}
			return searchResult;

		}

		private TripSearch parseLocationString(TripSearch criteria, string searchString, bool isStart, String cityString, String stateString)
		{
			double latitude = 0;
			double longitude = 0;
			string locationString = "";

			string latLonRegExPattern = "^(\\-?\\d+(\\.\\d+)?),\\s*(\\-?\\d+(\\.\\d+)?)$";

			Regex r = new Regex(latLonRegExPattern, RegexOptions.IgnoreCase);

			// Match the regular expression pattern against start location.
			Match matches = r.Match(searchString);
			if (matches.Success) {
				string[] inputSplitArray = searchString.Split (",".ToCharArray ());

				latitude = Convert.ToDouble (inputSplitArray [0].Trim ());
				longitude = Convert.ToDouble (inputSplitArray [1].Trim ());

			} else {
				locationString = searchString;

				string zipRegExPattern = "\\d{5}(-\\d{4})?$";

				r = new Regex(zipRegExPattern, RegexOptions.IgnoreCase);

				// Match the regular expression pattern for zip
				matches = r.Match(searchString);
				bool isLocalized = false;
				isLocalized = matches.Success;

				if (!isLocalized) {
					string stateRegExPattern = "(A[LKSZRAEP]|C[AOT]|D[EC]|F[LM]|G[AU]|HI|I[ADLN]|K[SY]|LA|M[ADEHINOPST]|N[CDEHJMVY]|O[HKR]|P[ARW]|RI|S[CD]|T[NX]|UT|V[AIT]|W[AIVY])$";
					r = new Regex(stateRegExPattern, RegexOptions.IgnoreCase);
					string stringToMatch = searchString.ToUpper ();
					// Match the regular expression pattern for state
					matches = r.Match(stringToMatch);
					isLocalized = matches.Success;

					if(!isLocalized)
						locationString =  locationString + " " + cityString + " " + stateString;
				}


			}

			if (isStart) {
				criteria.StartLocation = locationString;
				criteria.StartLatitude = latitude;
				criteria.StartLongitude = longitude;
			} else {
				criteria.EndLocation = locationString;
				criteria.EndLatitude = latitude;
				criteria.EndLongitude = longitude;
			}

			return criteria;
		}
			
		async public Task<bool> SaveTripForUser(int travelerId, Itinerary selectedItinerary, string origin, string destination, string prioritycode, bool isWheelchariNeeded, bool isBikeRackNeeded)
        {
			Trip postedTrip = await mTripManager.PostTripFromItinerary (selectedItinerary, travelerId, origin, destination, prioritycode, isWheelchariNeeded, isBikeRackNeeded);

			if (postedTrip != null)
				return true;

			return false;
        }

		async public Task<bool> CancelTripForUser(int travelerId, Trip trip)
        {
			TripSummaryForDelete deleteSummary = await mTripManager.CancelTrip (trip.Id);

			if (deleteSummary != null)
				return true;

			return false;
        }
       
		async public Task<List<Trip>> GetPastTrips(int travelerId, int maxResults)
        {
			List<Trip> trips = await mTripManager.GetTripsByType(travelerId, TripType.Type.Past);

			if (trips.Count > maxResults)
				trips = trips.GetRange (0, maxResults);

			return trips;
        }

		async public Task<List<Trip>> GetUpcomingTrips(int travelerId, int maxResults)
		{
			List<Trip> trips = await mTripManager.GetTripsByType(travelerId, TripType.Type.Upcoming);

			if (trips.Count > maxResults)
				trips = trips.GetRange (0, maxResults);
				
			return trips;
		}

		async public Task<int> GetUpcomingTripCount(int travelerId)
		{
			List<Trip> trips = await mTripManager.GetTripsByType(travelerId, TripType.Type.Upcoming);

			return trips.Count;
		}

		async public Task<List<Trip>> GetUpcomingTripsAfterNext(int travelerId, int maxResults)
		{
			List<Trip> trips = await mTripManager.GetTripsByType(travelerId, TripType.Type.Upcoming);

			List<Trip> inProgressTrips = await mTripManager.GetTripsByType (travelerId, TripType.Type.InProgress);
			if (inProgressTrips.Count <= 0)
			{
				if(trips.Count>0)
					trips.RemoveAt (0);
			}


			if (trips.Count > maxResults)
				trips = trips.GetRange (0, maxResults);

			return trips;
		}
       
    }
}