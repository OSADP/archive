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
    /// Used to get trip duration from two points using open trip planner's planning api
    /// </summary>
    public class ETAController : ApiController
    {
        readonly string BaseUrl = ConfigurationManager.AppSettings["OTPBaseUrl"];

        /// <summary>
        /// Gets the stops near point.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public Itinerary GetEta(float startLatitude, float startLongitude, float endLatitude, float endLongitude)
        {
            RestClient client = new RestClient
            {
                BaseUrl = BaseUrl
            };

            OpenTripPlannerAdapter otpa = new OpenTripPlannerAdapter(client);

            return otpa.PlanTrip(startLatitude, startLongitude, endLatitude, endLongitude, "CAR", DateTime.Now).plan.itineraries.FirstOrDefault();
        }
    }
}
