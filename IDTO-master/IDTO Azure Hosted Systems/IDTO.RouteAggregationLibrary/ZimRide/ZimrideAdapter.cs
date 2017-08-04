using System;
using System.Collections.Generic;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;
using RestSharp;

namespace IDTO.RouteAggregationLibrary.ZimRide
{

    public class ZimrideAdapter : IRouteProvider
    {
        private IRestClient restClient;

        public ZimrideAdapter(IRestClient newRestClient)
        {
            restClient = newRestClient;
        }

        public void FindAvailableRoutes(decimal fromPlace, decimal toPlace)
        {
            throw new NotImplementedException();
        }

        public StopTimesList FindStopTimes(string agency, string stopId, DateTime startTime, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public StopTimesList FindStopTimes(string agency, string stopId, long startTime, long endTime)
        {
            throw new NotImplementedException();
        }

        public StopList FindStopsNearPoint(float latitude, float longitude, int radius)
        {
            throw new NotImplementedException();
        }

        public Planner PlanTrip(float startLatitude, float startLongitude, float endLatitude, float endLongitude, String mode, DateTime startTime, int platform_id)
        {
            var request = LoadBaseParameters(startLatitude, startLongitude, endLatitude, endLongitude, mode, startTime, platform_id);

            return ExecuteOTPQuery(request);
        }
        public Planner PlanAdvancedTrip(float startLatitude, float startLongitude, float endLatitude, float endLongitude,
            String mode, DateTime time,
            bool searchByArriveByTime, bool needWheelchairAccess, float maxWalkMeters, int platform_id)
        {
            var request = LoadBaseParameters(startLatitude, startLongitude, endLatitude, endLongitude, mode, time, platform_id);
            request.AddHeader("X-ZIMRIDE-API-TOKEN", "d3a56a4cef86443bc89910264a4d8887959f5e68");
            //Add additional parameters
            //request.AddParameter("arriveBy", searchByArriveByTime, ParameterType.QueryString); //ArriveBy default is false
            //request.AddParameter("wheelchair", needWheelchairAccess, ParameterType.QueryString);
            request.AddParameter("maxWalkDistance", maxWalkMeters, ParameterType.QueryString);
            
            return ExecuteOTPQuery(request);
             
            throw new NotImplementedException();
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

        private static RestRequest LoadBaseParameters(float startLatitude, float startLongitude, float endLatitude, float endLongitude, String mode, DateTime startTime, int platform_id)
        {
            var request = new RestRequest();
            request.Resource = "trips";
            //
            String startLocationString = startLatitude.ToString() + "," + startLongitude.ToString();
            request.AddParameter("fromPlace", startLocationString, ParameterType.QueryString);

            String endLocationString = endLatitude.ToString() + "," + endLongitude.ToString();
            request.AddParameter("toPlace", endLocationString, ParameterType.QueryString);

            String dateString = startTime.ToString("yyyy-MM-dd");
            request.AddParameter("date", dateString, ParameterType.QueryString);

            request.AddParameter("platform_id", platform_id, ParameterType.QueryString);
            
            return request;
        }

    }
    
}
