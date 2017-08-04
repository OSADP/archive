namespace IDTO.TravelerPortal.Common
{
    using System;
    using System.Threading.Tasks;
    using IDTO.TravelerPortal.Common.Models;
    using System.Collections.Generic;

    using RestSharp;

    public class TripManager
    {
        readonly string BaseUrl = Config.IDTOWebApiBaseUrl;

        public TripManager()
        {
        }

        public List<Trip> GetTripsByType(int travelerId, TripType.Type type)
        {
            List<Trip> tripList = new List<Trip>();

            string uriString = BaseUrl;

            var client = new RestClient(uriString);
            client.UserAgent = "Web RestSharp";
            var request = new RestRequest();
            request.Resource = "Trip";
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("travelerID", travelerId);
            request.AddParameter("type", type);

            IRestResponse<List<Trip>> response = client.Execute<List<Trip>>(request);

            tripList = response.Data;

            return tripList;
        }

        public async Task<List<Trip>> GetTripsByTypeAsync(int travelerId, TripType.Type type)
        {
            List<Trip> list = await Task.Factory.StartNew<List<Trip>>(() =>
            {
                List<Trip> tripList = new List<Trip>();

                string uriString = BaseUrl;

                var client = new RestClient(uriString);
                client.UserAgent = "Web RestSharp";
                var request = new RestRequest();
                request.Resource = "Trip";
                request.Method = Method.GET;
                request.RequestFormat = DataFormat.Json;

                request.AddParameter("travelerID", travelerId);
                request.AddParameter("type", type);

                IRestResponse<List<Trip>> response = client.Execute<List<Trip>>(request);

                tripList = response.Data;

                return tripList;
            });

            return list;
        }


        public async Task<TripSummaryForDelete> CancelTrip(int tripId)
        {
            TripSummaryForDelete deleteSummary = await Task.Factory.StartNew<TripSummaryForDelete>(() =>
            {
                TripSummaryForDelete tripSummary = new TripSummaryForDelete();

                string uriString = BaseUrl;

                var client = new RestClient(uriString);
                client.UserAgent = "Web RestSharp";
                var request = new RestRequest("/Trip/{id}", Method.DELETE);
                request.RequestFormat = DataFormat.Json;
                request.AddUrlSegment("id", tripId.ToString());

                IRestResponse<TripSummaryForDelete> response = client.Execute<TripSummaryForDelete>(request);

                tripSummary = response.Data;

                return tripSummary;
            });

            return deleteSummary;
        }

    }
}