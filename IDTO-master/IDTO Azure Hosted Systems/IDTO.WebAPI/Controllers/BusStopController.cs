using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IDTO.RouteAggregationLibrary.OpenTripPlanner;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;
using RestSharp;
using System.Configuration;

namespace IDTO.WebAPI.Controllers
{
    /// <summary>
    /// Used to get information about the Bus Stops supported by the system.
    /// </summary>
    public class BusStopController : ApiController
    {
        readonly string BaseUrl = ConfigurationManager.AppSettings["OTPBaseUrl"];

        /// <summary>
        /// Gets the stops near point.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public StopList GetStopsNearPoint(float latitude, float longitude, int radius)
        {
            RestClient client = new RestClient
            {
                BaseUrl = BaseUrl
            };

            OpenTripPlannerAdapter otpa = new OpenTripPlannerAdapter(client);

            return otpa.FindStopsNearPoint(latitude, longitude, radius);            
        }

        /// <summary>
        /// Gets the stop times.
        /// </summary>
        /// <param name="stopId">The stop identifier.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public StopTimesList GetStopTimes(string agency, string stopId, long startTime, long endTime)
        {
            RestClient client = new RestClient
            {
                BaseUrl = BaseUrl
            };

            OpenTripPlannerAdapter otpa = new OpenTripPlannerAdapter(client);

            return otpa.FindStopTimes(agency, stopId, startTime, endTime);
        }

    }
}
