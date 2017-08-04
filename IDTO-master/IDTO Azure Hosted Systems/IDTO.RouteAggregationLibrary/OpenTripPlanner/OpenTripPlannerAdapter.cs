using System;
using System.Collections.Generic;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;
using RestSharp;

namespace IDTO.RouteAggregationLibrary.OpenTripPlanner
{
    public class OpenTripPlannerAdapter : IRouteProvider
    {
        private IRestClient restClient;

        public OpenTripPlannerAdapter(IRestClient newRestClient)
        {
            restClient = newRestClient;
        }

        public void FindAvailableRoutes(decimal fromPlace, decimal toPlace)
        {
            throw new NotImplementedException();
        }

        public StopTimesList FindStopTimes(string agency, string stopId, DateTime startTime, DateTime endTime)
        {
            return this.FindStopTimes(agency, stopId, startTime.ConvertToMillisecondsSinceJan1970(), endTime.ConvertToMillisecondsSinceJan1970());
        }

        public StopTimesList FindStopTimes(string agency, string stopId, long startTime, long endTime)
        {
            var request = new RestRequest();
            request.Resource = "transit/stopTimesForStop";
            request.AddParameter("agency", agency, ParameterType.QueryString);
            request.AddParameter("id", stopId, ParameterType.QueryString);
            request.AddParameter("startTime", startTime, ParameterType.QueryString);
            request.AddParameter("endTime", endTime, ParameterType.QueryString);
            var response = restClient.Execute<StopTimesList>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var otpException = new ApplicationException(message, response.ErrorException);
                throw otpException;
            }

            StopTimesList departuresOnly = new StopTimesList();
            foreach (var stopTime in response.Data.stopTimes)
            {
                if (stopTime.phase == "departure")
                {
                    departuresOnly.stopTimes.Add(stopTime);
                }                
            }

            return departuresOnly;

        }

        public StopList FindStopsNearPoint(float latitude, float longitude, int radius)
        {
            var request = new RestRequest();
            request.Resource = "transit/stopsNearPoint";
            request.AddParameter("lat", latitude, ParameterType.QueryString);
            request.AddParameter("lon", longitude, ParameterType.QueryString);
            request.AddParameter("radius", radius, ParameterType.QueryString);
            var response = restClient.Execute<StopList>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var otpException = new ApplicationException(message, response.ErrorException);
                throw otpException;
            }
            return response.Data;
        }

        public Planner PlanTrip(float startLatitude, float startLongitude, float endLatitude, float endLongitude, String mode, DateTime startTime)
        {
            var request = LoadBaseParameters(startLatitude, startLongitude, endLatitude, endLongitude, mode, startTime);

            return ExecuteOTPQuery(request);
        }
        public Planner PlanAdvancedTrip(float startLatitude, float startLongitude, float endLatitude, float endLongitude, 
            String mode, DateTime time,
            bool searchByArriveByTime, bool needWheelchairAccess, float maxWalkMeters)
        {
            var request = LoadBaseParameters(startLatitude, startLongitude, endLatitude, endLongitude, mode, time);
           //Add additional parameters
            request.AddParameter("arriveBy", searchByArriveByTime, ParameterType.QueryString); //ArriveBy default is false
            request.AddParameter("wheelchair", needWheelchairAccess, ParameterType.QueryString);
            request.AddParameter("maxWalkDistance", maxWalkMeters, ParameterType.QueryString);

            return ExecuteOTPQuery(request);
        }
        private Planner ExecuteOTPQuery(RestRequest request)
        {
            var response = restClient.Execute<Planner>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var otpException = new ApplicationException(message, response.ErrorException);
                throw otpException;
            }
            return response.Data;
        }

        private static RestRequest LoadBaseParameters(float startLatitude, float startLongitude, float endLatitude, float endLongitude, String mode, DateTime startTime)
        {
            var request = new RestRequest();
            request.Resource = "plan";
            //http://docs.opentripplanner.org/apidoc/0.9.2/resource_Planner.html
            String startLocationString = startLatitude.ToString() + "," + startLongitude.ToString();
            request.AddParameter("fromPlace", startLocationString, ParameterType.QueryString);

            String endLocationString = endLatitude.ToString() + "," + endLongitude.ToString();
            request.AddParameter("toPlace", endLocationString, ParameterType.QueryString);

            String dateString = startTime.ToString("MM/dd/yyyy");
            request.AddParameter("date", dateString, ParameterType.QueryString);

            String timeString = startTime.ToString("HH:mm");
            request.AddParameter("time", timeString, ParameterType.QueryString);
            request.AddParameter("mode", mode, ParameterType.QueryString);
            return request;
        }

        public ServerInfo GetServerInfo()
        {
            var request = new RestRequest();
            request.Resource = "serverinfo";
            var response = restClient.Execute<ServerInfo>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var otpException = new ApplicationException(message, response.ErrorException);
                throw otpException;
            }
            return response.Data;
            
        }
    }
}
