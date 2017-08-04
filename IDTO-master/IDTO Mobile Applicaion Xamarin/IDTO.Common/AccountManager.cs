namespace IDTO.Common
{
    using System;
    using System.Threading.Tasks;
    using IDTO.Common.Models;

    using RestSharp;
    
    public class AccountManager
    {
        const string HttpAuthUsername = "tadpole";
        const string HttpAuthPassword = "tadpole";

#if DEBUG
        private static string URI_STRING = "http://idtowebapidev.azurewebsites.net/";
#else
        private static string URI_STRING = "http://idtowebapiv2.azurewebsites.net/";
#endif

        public AccountManager()
        {
        }

        public async Task<TravelerModel> CreateTraveler(TravelerModel traveler)
        {
			TravelerModel travelerResult = await Task.Factory.StartNew<TravelerModel> (() => {

                var client = new RestClient(URI_STRING);
				client.Authenticator = new HttpBasicAuthenticator (HttpAuthUsername, HttpAuthPassword);

				var request = new RestRequest ("/api/Traveler", Method.POST);

				request.RequestFormat = DataFormat.Json;

				request.AddBody (traveler);

				IRestResponse<TravelerModel> response = client.Execute<TravelerModel> (request);

				return response.Data;
			});

			return travelerResult;
        }

        public async Task<TravelerModel> UpdateTraveler(TravelerModel traveler)
        {
            TravelerModel travelerResult = await Task.Factory.StartNew<TravelerModel>(() =>
            {

                var client = new RestClient(URI_STRING);
                client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

                var request = new RestRequest("/api/Traveler/" + traveler.Id, Method.PUT);

                request.RequestFormat = DataFormat.Json;

                request.AddBody(traveler);

                IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new Exception(response.StatusDescription);

                return response.Data;
            });

            return travelerResult;
        }

        public async Task<TravelerModel> GetTravelerById(int id)
        {
			TravelerModel traveler = await Task.Factory.StartNew<TravelerModel> (() => {

                var client = new RestClient(URI_STRING);
				client.Authenticator = new HttpBasicAuthenticator (HttpAuthUsername, HttpAuthPassword);

				var request = new RestRequest ("/api/Traveler", Method.GET);
				request.RequestFormat = DataFormat.Json;

				request.AddParameter ("id", id);

				IRestResponse<TravelerModel> response = client.Execute<TravelerModel> (request);

				return response.Data;
			});

			return traveler;
        }

        public async Task<TravelerModel> GetTravelerByEmail(string email)
        {
			TravelerModel traveler = await Task.Factory.StartNew<TravelerModel> (() => {

                var client = new RestClient(URI_STRING);
				client.Authenticator = new HttpBasicAuthenticator (HttpAuthUsername, HttpAuthPassword);

				var request = new RestRequest ("/api/Traveler", Method.GET);
				request.RequestFormat = DataFormat.Json;

				request.AddParameter ("email", email);

				IRestResponse<TravelerModel> response = client.Execute<TravelerModel> (request);

				return response.Data;
			});

            return traveler;
        }

        public async Task<TravelerModel> GetTravelerByLoginId(string loginid)
        {
			TravelerModel traveler = await Task.Factory.StartNew<TravelerModel> (() => {

                var client = new RestClient(URI_STRING);
				client.Authenticator = new HttpBasicAuthenticator (HttpAuthUsername, HttpAuthPassword);

				var request = new RestRequest ("/api/Traveler", Method.GET);
				request.RequestFormat = DataFormat.Json;

				request.AddParameter ("loginid", loginid);

				IRestResponse<TravelerModel> response = client.Execute<TravelerModel> (request);

				return response.Data;
			});

            return traveler;
        }
    }
}