namespace IDTO.TravelerPortal.Common
{
    using System.Threading.Tasks;
    using IDTO.TravelerPortal.Common.Models;

    using RestSharp;
    using System.Configuration;
    using System;
    
    public class AccountManager
    {
        const string HttpAuthUsername = "tadpole";
        const string HttpAuthPassword = "tadpole";
        readonly string BaseUrl = Config.IDTOWebApiBaseUrl;
        
        public AccountManager()
        {
        }

        public TravelerModel CreateTraveler(TravelerModel traveler)
        {
            TravelerModel travelerResult = new TravelerModel();

            var client = new RestClient(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

            var request = new RestRequest("/Traveler", Method.POST);

            request.RequestFormat = DataFormat.Json;

            request.AddBody(traveler);

            IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

            travelerResult = response.Data;

            return travelerResult;
        }


        public TravelerModel UpdateTraveler(TravelerModel traveler)
        {
            TravelerModel travelerResult = new TravelerModel();
            var client = new RestClient(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

            var request = new RestRequest("Traveler", Method.PUT);

            request.RequestFormat = DataFormat.Json;

            request.AddParameter("id", traveler.Id, ParameterType.QueryString); 
            request.AddBody(traveler);

            IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

            if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                throw new Exception("Invalid Promo Code");
            }

            travelerResult = response.Data;

            return travelerResult;
        }


        public TravelerModel GetTravelerById(int id)
        {
            TravelerModel traveler = new TravelerModel();

            var client = new RestClient(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

            var request = new RestRequest("/Traveler", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("id", id);

            IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

            traveler = response.Data;

            return traveler;
        }

        public TravelerModel GetTravelerByEmail(string email)
        {
            TravelerModel traveler = new TravelerModel();

            var client = new RestClient(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

            var request = new RestRequest("/Traveler", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("email", email);

            IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

            traveler = response.Data;

            return traveler;
        }

        public TravelerModel GetTravelerByLoginId(string loginid)
        {
            TravelerModel traveler = new TravelerModel();

            var client = new RestClient(BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(HttpAuthUsername, HttpAuthPassword);

            var request = new RestRequest("/Traveler", Method.GET);
            request.RequestFormat = DataFormat.Json;

            request.AddParameter("loginid", loginid);

            IRestResponse<TravelerModel> response = client.Execute<TravelerModel>(request);

            traveler = response.Data;

            return traveler;
        }
    }
}