namespace IDTO.Common
{
    using System;
    using System.Threading.Tasks;
    using IDTO.Common.Models;
    using System.Collections.Generic;

    using RestSharp;

    public class TripManager
    {
        
#if DEBUG
        private static string URI_STRING = "http://idtowebapidev.azurewebsites.net/";
#else
        private static string URI_STRING = "http://idtowebapiv2.azurewebsites.net/";
#endif


#if __IOS__
		private static string USER_AGENT = "iOS RestSharp";
#elif __ANDROID__
		private static string USER_AGENT = "Android RestSharp";
#endif

        public TripManager()
        {
        }

        public async Task<Trip> TestConnection(int id)
        {
            Trip val = await Task.Factory.StartNew<Trip>(() =>
            {
                Trip trip = new Trip();


                var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
                var request = new RestRequest("/api/trip", Method.GET);
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("id", id);

                IRestResponse<Trip> response = client.Execute<Trip>(request);

                return response.Data;
            });

            return val;
        }

        public async Task<TripSearchResult> SearchForTrip(TripSearch search)
        {
            TripSearchResult val = await Task.Factory.StartNew<TripSearchResult>(() =>
            {
                TripSearchResult searchResult = new TripSearchResult();

                var client = new RestClient(URI_STRING);

				client.UserAgent = USER_AGENT;
                var request = new RestRequest("/api/OTP", Method.GET);

                request.RequestFormat = DataFormat.Json;

                request.AddParameter("startLatitude", search.StartLatitude);
                request.AddParameter("startLongitude", search.StartLongitude);
                request.AddParameter("startLocation", search.StartLocation);
                request.AddParameter("endLatitude", search.EndLatitude);
                request.AddParameter("endLongitude", search.EndLongitude);
                request.AddParameter("endLocation", search.EndLocation);
                request.AddParameter("searchByArriveByTime", search.SearchByArriveByTime);
                request.AddParameter("time", search.Time);
                request.AddParameter("needWheelchairAccess", search.NeedWheelchairAccess);
                request.AddParameter("maxWalkMeters", search.MaxWalkMeters);

                IRestResponse<TripSearchResult> response = client.Execute<TripSearchResult>(request);

                searchResult = response.Data;

					if (searchResult != null) {
						foreach (Itinerary itinerary in searchResult.itineraries)
						{
							if (itinerary.legs != null)
							{
								foreach (Leg leg in itinerary.legs)
								{
									if (leg.legGeometry != null)
									{
										if (leg.legGeometry.points != null)
										{
											leg.googlePoints = GooglePoints.Decode(leg.legGeometry.points);
										}
									}
								}
							}
						}

					}

                return searchResult;
            });

            return val;
        }

        public async Task<List<Trip>> GetTripsByType(int travelerId, TripType.Type type)
        {
            List<Trip> list = await Task.Factory.StartNew<List<Trip>>(() =>
            {
                List<Trip> tripList = new List<Trip>();

                var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
                var request = new RestRequest("/api/Trip", Method.GET);
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("travelerID", travelerId);
                request.AddParameter("type", type);

                IRestResponse<List<Trip>> response = client.Execute<List<Trip>>(request);

                tripList = response.Data;


				foreach (Trip trip in tripList)
				{
					if (trip.Steps != null)
					{
						foreach (Step step in trip.Steps)
						{
							if (step.EncodedMapString != null)
							{
								step.googlePoints = GooglePoints.Decode(step.EncodedMapString);
							}
						}
					}
				}
				
                return tripList;
            });

            return list;
        }

        public async Task<List<DateTime>> GetTimesToMonitorLocationForTraveler(int travelerId)
        {
            List<DateTime> dateList = new List<DateTime>();
            List<Trip> upcomingTrips = await GetTripsByType(travelerId, TripType.Type.Upcoming);

            foreach(Trip trip in upcomingTrips)
            {
                foreach(Step step in trip.Steps)
                {
                    if(step.ModeId != (int)ModeType.ModeId.WALK)
                    {
                        String startTimeString = step.StartDate.ToLongTimeString();
                        DateTime startTime = step.StartDate;
                        DateTime endTime = step.EndDate;

                        startTime = startTime.AddMinutes(-1);
                        endTime = endTime.AddMinutes(-1);

                        startTimeString += " " + startTime.ToLongTimeString();
                        dateList.Add(startTime);
                        dateList.Add(endTime);
                        Console.WriteLine(startTimeString);
                    }
                }
            }

            return dateList;
        }

        public async Task<List<DateTime>> GetTimesToMonitorLocationForTrip(int id)
        {
            List<DateTime> list = await Task.Factory.StartNew<List<DateTime>>(() =>
            {
                List<DateTime> dateList = new List<DateTime>();
                Trip trip = new Trip();

                var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
                var request = new RestRequest("/api/trip", Method.GET);
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("id", id);

                IRestResponse<Trip> response = client.Execute<Trip>(request);

                trip = response.Data;

                foreach (Step step in trip.Steps)
                {
                    if (step.ModeId != (int)ModeType.ModeId.WALK)
                    {
                        String startTimeString = step.StartDate.ToLongTimeString();
                        DateTime startTime = step.StartDate;
                        DateTime endTime = step.EndDate;

                        startTime = startTime.AddMinutes(-1);
                        endTime = endTime.AddMinutes(-1);

                        startTimeString += " " + startTime.ToLongTimeString();
                        dateList.Add(startTime);
                        dateList.Add(endTime);
                        Console.WriteLine(startTimeString);
                    }
                }

                return dateList;
            });

            return list;
        }

        public async Task<TripSummaryForDelete> CancelTrip(int tripId)
        {
            TripSummaryForDelete deleteSummary = await Task.Factory.StartNew<TripSummaryForDelete>(() =>
            {
                TripSummaryForDelete tripSummary = new TripSummaryForDelete();

                var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
                var request = new RestRequest("/api/Trip/{id}", Method.DELETE);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("id", tripId.ToString());

                IRestResponse<TripSummaryForDelete> response = client.Execute<TripSummaryForDelete>(request);

                tripSummary = response.Data;

                return tripSummary;
            });

            return deleteSummary;
        }

        public async Task<TravelerLocation> PostTravelerLocation(TravelerLocation loc)
        {
            TravelerLocation val = await Task.Factory.StartNew<TravelerLocation>(() =>
           {
               TravelerLocation result = new TravelerLocation();

               var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
               var request = new RestRequest("/api/TravelerLocation", Method.POST);

               request.RequestFormat = DataFormat.Json;

               request.AddBody(loc);

               IRestResponse<TravelerLocation> response = client.Execute<TravelerLocation>(request);

               result = response.Data;

               return result;
           });

            return val;
        }

        public async Task<Trip> PostTrip(Trip trip)
        {
            Trip val = await Task.Factory.StartNew<Trip>(() =>
           {
               Trip tripResult = new Trip();

               var client = new RestClient(URI_STRING);
					client.UserAgent = USER_AGENT;
               var request = new RestRequest("/api/Trip", Method.POST);

               request.RequestFormat = DataFormat.Json;

               request.AddBody(trip);

			
					Console.WriteLine(request.JsonSerializer.Serialize(trip));
               IRestResponse<Trip> response = client.Execute<Trip>(request);

               tripResult = response.Data;

               return tripResult;
           });

            return val;
        }

        public async Task<Trip> PostTripFromItinerary(Itinerary itineray, int travelerId, string origin, string destination, string priorityCode, bool mobilityFlag, bool bicycleFlag)
        {
            Trip trip = ItineraryToTrip(itineray, travelerId, origin, destination, priorityCode, mobilityFlag, bicycleFlag);
            Trip tripResult = await PostTrip(trip);

            return tripResult;
        }


        public Trip ItineraryToTrip(Itinerary itineray, int travelerId, string origin, string destination, string priorityCode, bool mobilityFlag, bool bicycleFlag)
        {
            Trip trip = new Trip();
            trip.Steps = new List<Step>();

            trip.Id = 1;
            trip.TravelerId = travelerId;
            trip.Origination = origin;
            trip.Destination = destination;
			trip.TripStartDate = itineray.startTime.ToDateTimeUTC();
			trip.TripEndDate = itineray.endTime.ToDateTimeUTC();
            trip.PriorityCode = priorityCode;
            trip.MobilityFlag = mobilityFlag;
            trip.BicycleFlag = bicycleFlag;

			int maxStringLength = 0;

            for (int i = 0; i < itineray.legs.Count; i++)
            {
                Step step = new Step();
                
                step.Id = i + 1;
                step.TripId = trip.Id;
                step.ModeId = ModeType.StringToId(itineray.legs[i].mode);
				step.StartDate = itineray.legs[i].startTime.ToDateTimeUTC();
				step.EndDate = itineray.legs[i].endTime.ToDateTimeUTC();
                step.FromName = itineray.legs[i].from.name;
                step.FromStopCode = itineray.legs[i].from.stopCode;
                step.BlockIdentifier = itineray.legs[i].tripBlockId;
				step.EncodedMapString = itineray.legs [i].legGeometry.points;

				if (step.EncodedMapString.Length > maxStringLength) {
					maxStringLength = step.EncodedMapString.Length;
				}

                if (itineray.legs[i].from.stopId != null)
                {
                    step.FromProviderId = Providers.StringToId(itineray.legs[i].from.stopId.agencyId);
                }

                step.ToName = itineray.legs[i].to.name;
                step.ToStopCode = itineray.legs[i].to.stopCode;

                if (itineray.legs[i].to.stopId != null)
                {
                    step.ToProviderId = Providers.StringToId(itineray.legs[i].to.stopId.agencyId);
                }

                step.Distance = Convert.ToDecimal(itineray.legs[i].distance);
                step.RouteNumber = itineray.legs[i].routeShortName;

                trip.Steps.Add(step);
            }

			var x = maxStringLength;

            return trip;
        }
    }
}