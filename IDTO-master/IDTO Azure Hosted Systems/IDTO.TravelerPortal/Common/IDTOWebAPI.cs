using IDTO.TravelerPortal.Common.Models;
using IDTO.TravelerPortal.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

namespace IDTO.TravelerPortal.Common
{
    public class IDTOWebAPI
    {
        readonly string BaseUrl = Config.IDTOWebApiBaseUrl;

        readonly string _accountSid;
        readonly string _secretKey;

        public IDTOWebAPI()
        {

        }

        public async Task<T> ExecuteGetTaskAsync<T>(IRestRequest request) where T : new()
        {
            var client = new RestClient();
            client.UserAgent = "Web RestSharp";
            client.BaseUrl = BaseUrl;

            //string authInfo = "idto" + ":" + "idto";
            //authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            //client. = new AuthenticationHeaderValue("Basic", authInfo);
            client.Authenticator = new HttpBasicAuthenticator("idto", "idto");

            try
            {
                var response = await client.ExecuteTaskAsync(request);

                var ob = JsonConvert.DeserializeObject<T>(response.Content);

                if (response.ErrorException != null)
                {
                    const string message = "Error retrieving response. Check inner details for more info.";
                    var idtoWebException = new ApplicationException(message, response.ErrorException);
                    throw idtoWebException;
                }

                return ob;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                throw;
            }



        }

        public async Task<IRestResponse<T>> ExecutePostTaskAsync<T>(IRestRequest request)
        {
            var client = new RestClient();
            client.BaseUrl = BaseUrl;

            try
            {
                var response = await client.ExecutePostTaskAsync<T>(request);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                throw;
            }
        }

        public async Task<Plan> GetPlan(TripSearchCriteria searchCriteria)
        {
            var request = new RestRequest();
            request.Resource = "OTP";
            request.RootElement = "Plan";

            request.AddParameter("startLatitude", searchCriteria.startLatitude, ParameterType.GetOrPost);
            request.AddParameter("startLongitude", searchCriteria.startLongitude, ParameterType.GetOrPost);
            request.AddParameter("startLocation", searchCriteria.startLocation, ParameterType.GetOrPost);

            request.AddParameter("endLatitude", searchCriteria.endLatitude, ParameterType.GetOrPost);
            request.AddParameter("endLongitude", searchCriteria.endLongitude, ParameterType.GetOrPost);
            request.AddParameter("endLocation", searchCriteria.endLocation, ParameterType.GetOrPost);

            request.AddParameter("searchByArriveByTime", searchCriteria.searchByArrivByTime, ParameterType.GetOrPost);
            request.AddParameter("time", searchCriteria.time, ParameterType.GetOrPost);
            request.AddParameter("needWheelchairAccess", searchCriteria.needWheelchairAccess, ParameterType.GetOrPost);
            request.AddParameter("maxWalkMeters", searchCriteria.maxWalkMeters, ParameterType.GetOrPost);

            return await ExecuteGetTaskAsync<Plan>(request);
        }

        public async Task<Trip> SaveTrip(Trip trip)
        {
            var request = new RestRequest();
            request.Resource = "Trip";
            request.Method = Method.POST;
            request.RequestFormat = DataFormat.Json;

            request.AddBody(trip);

            IRestResponse<Trip> response = await ExecutePostTaskAsync<Trip>(request);

            var tripResult = response.Data;

            return tripResult;
        }

        public async Task<TravelerModel> GetTravelerByEmail(string email)
        {
            var request = new RestRequest();
            request.Resource = "Traveler";

            request.AddParameter("email", email);

            var result = await ExecuteGetTaskAsync<TravelerModel>(request);

            return result;
        }
    }
}