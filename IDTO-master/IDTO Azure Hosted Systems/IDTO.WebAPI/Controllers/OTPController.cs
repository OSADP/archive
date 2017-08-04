using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using IDTO.RouteAggregationLibrary.OpenTripPlanner;
using IDTO.RouteAggregationLibrary.OpenTripPlanner.Model;
using IDTO.WebAPI.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Web.Http.Description;
using System.Configuration;
using IDTO.WebAPI;
using Repository.Providers.EntityFramework;
using IDTO.Entity.Models;
using IDTO.RouteAggregationLibrary.ZimRide;

namespace IDTO.WebAPI.Controllers
{
    /// <summary>
    /// For searching and interfacing to OTP.
    /// </summary>
    public class OTPController : BaseController
    {
        public OTPController(IDbContext context)
            : base(context)
        { }

        readonly string BaseUrl = ConfigurationManager.AppSettings["OTPBaseUrl"];
        readonly string ZimRideBaseUrl = ConfigurationManager.AppSettings["ZimRideBaseUrl"];
        

        /// <summary>
        /// Search Open Trip Planner for a trip matching the criteria.
        /// Must enter either the lat/long of the endpoint or the string name of the lookup.
        /// </summary>
        /// <param name="startLatitude">Latitude Location of start of trip.</param>
        /// <param name="startLongitude">Longitude Location of start of trip.</param>
        /// <param name="startLocation">Instead of lat/long, enter string Location of start of trip.</param>
        /// <param name="endLatitude">Latitude Location of end of trip.</param>
        /// <param name="endLongitude">Longitude Location of end of trip.</param>
        /// <param name="endLocation">Instead of lat/long, enter string Location of end of trip.</param>
        /// <param name="searchByArriveByTime">If true, searches by Arrival Time provided in Time parameter.  If false,
        /// searches by Departure Time provided in Time parameter</param>
        /// <param name="time">Specifies either Time of Departure or Time of Arrival based on searchByDepartureTime boolean.</param>
        /// <param name="needWheelchairAccess">If true, require that trip selections provide for wheelchair accessiblity.</param>
        /// <param name="maxWalkMeters">Maximum distance in meters traveler is willing to walk between stops.</param>
        /// <returns>Json of the trip</returns>
        [ResponseType(typeof(Plan))]
        public IHttpActionResult GetTrip(float startLatitude, float startLongitude, string startLocation,
            float endLatitude, float endLongitude, string endLocation,
            bool searchByArriveByTime, DateTime time, bool needWheelchairAccess, float maxWalkMeters)
        {
            System.Net.Http.HttpRequestMessage currentRequest = this.Request;

            DateTime dtStart = DateTime.UtcNow;
            WebApiGetTripUsage webApiGetTripUsage = new WebApiGetTripUsage();
            webApiGetTripUsage.FromPlace = startLocation;
            webApiGetTripUsage.ToPlace = endLocation;
            webApiGetTripUsage.CreatedDate = dtStart;
            webApiGetTripUsage.MaxWalkMeters = maxWalkMeters;
            webApiGetTripUsage.SearchDate = time;
            webApiGetTripUsage.Platform = System.Web.HttpContext.Current.Request.UserAgent;
           

            string json = "";
            RestClient client = new RestClient
            {
                BaseUrl = BaseUrl
            };

            RestClient zimrideclient = new RestClient
            {
                BaseUrl = ZimRideBaseUrl
            };

            //Validate that the caller gave either the lat/long or the string for each endpoint.
            if (String.IsNullOrEmpty(startLocation))
            {
                //string not provided, so lat/long had better be valid or it is an error.
                if (startLatitude == 0 || startLongitude == 0)
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Lat/Long must be specified for start location if address option not specified. ");
                    throw new HttpResponseException(responseMessage);
                }
            }
            if (String.IsNullOrEmpty(endLocation))
            {
                //string not provided, so lat/long had better be valid or it is an error.
                if (endLatitude == 0 || endLongitude == 0)
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "Lat/Long must be specified for end location if address option not specified. ");
                    throw new HttpResponseException(responseMessage);
                }
            }

            //Create trip start and end lat/long pairs
            if (String.IsNullOrWhiteSpace(startLocation))
            {
                //HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                //       "Start Location empty cannot obtain trip start lat/long.");
               // throw new HttpResponseException(responseMessage);
            }
            else
            {
                GeoLocation.GeolookupResult geoLocResult = new GeoLocation.GeolookupResult();
                GeoLocation geoLoc = new GeoLocation();
                geoLocResult = geoLoc.GetLatLongForAddress(startLocation);
                if (geoLocResult.APIReturnStatus == "OK")
                {
                    startLatitude = geoLocResult.position[0];
                    startLongitude = geoLocResult.position[1];
                }
                else
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                           "Invalid trip starting location - geocode return value: " + geoLocResult.APIReturnStatus);
                    throw new HttpResponseException(responseMessage);
                }
            }

            if (String.IsNullOrWhiteSpace(endLocation))
            {
                //HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                //       "End Location empty cannot obtain trip end lat/long.");
                //throw new HttpResponseException(responseMessage);
            }
            else
            {
                GeoLocation.GeolookupResult geoLocResult = new GeoLocation.GeolookupResult();
                GeoLocation geoLoc = new GeoLocation();
                geoLocResult = geoLoc.GetLatLongForAddress(endLocation);
                if (geoLocResult.APIReturnStatus == "OK")
                {
                    endLatitude = geoLocResult.position[0];
                    endLongitude = geoLocResult.position[1];
                }
                else
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                           "Invalid trip ending location - geocode return value: " + geoLocResult.APIReturnStatus);
                    throw new HttpResponseException(responseMessage);
                }
            }
          

            OpenTripPlannerAdapter otpa = new OpenTripPlannerAdapter(client);
            ZimrideAdapter zra = new ZimrideAdapter(zimrideclient);
            
            //TODO: add more of the parameters.
            //Query trip
           // json = otpa.PlanTrip(startLatitude, startLongitude, endLatitude, endLongitude, "TRANSIT,WALK", time).ToString();

            //return json
            //return JsonConvert.SerializeObject(json, Formatting.Indented);

            webApiGetTripUsage.FromLatitude = startLatitude;
            webApiGetTripUsage.FromLongitude = startLongitude;
            webApiGetTripUsage.ToLatitude = endLatitude;
            webApiGetTripUsage.ToLongitude = endLongitude;

            var response = otpa.PlanAdvancedTrip(startLatitude, startLongitude, endLatitude, endLongitude, "TRANSIT,WALK", time, searchByArriveByTime, needWheelchairAccess, maxWalkMeters);
            if (response == null || response.plan == null)
                return NotFound();

            var zimrides = zra.PlanAdvancedTrip(startLatitude, startLongitude, endLatitude, endLongitude, "ZIMRIDE", time, searchByArriveByTime, needWheelchairAccess, maxWalkMeters, 174);


            List<Itinerary> itList = new List<Itinerary>(response.plan.itineraries);

            foreach (var itinerary in itList)
            {
                DateTime dtUCTNow = DateTime.UtcNow;
                DateTime tripStart = itinerary.startTime.ToDateTimeUTC();
                if (tripStart <= dtUCTNow)
                {
                    //trip starts in past remove from list
                    response.plan.itineraries.Remove(itinerary);
                }
            }

            response.plan.itineraries.AddRange(zimrides.plan.itineraries);

            itList = new List<Itinerary>(response.plan.itineraries);
            foreach (var itinerary in itList)
            {
                foreach(var leg in itinerary.legs)
                {
                    if(String.IsNullOrEmpty(leg.from.stopCode))
                    {
                        if(leg.from.stopId!=null)
                            leg.from.stopCode = leg.from.stopId.id;
                    }

                    if(String.IsNullOrEmpty(leg.to.stopCode))
                    {
                        if(leg.to.stopId!=null)
                            leg.to.stopCode = leg.to.stopId.id;
                    }

                    if(leg!=null && leg.from !=null && leg.from.name!=null && leg.from.name.StartsWith("way"))
                    {
                        leg.from.name = "Unnamed Road";
                    }

                    if (leg != null && leg.to != null && leg.to.name != null && leg.to.name.StartsWith("way"))
                    {
                        leg.to.name = "Unnamed Road";
                    }
                }
            }

            try
            {
                DateTime dtEnd = DateTime.UtcNow;
                TimeSpan tsExecutionTime = dtEnd - dtStart;

                webApiGetTripUsage.NumberOfResults = response.plan.itineraries.Count;
                webApiGetTripUsage.ExecutionTimeSeconds = Convert.ToInt32(tsExecutionTime.TotalSeconds);

                Uow.Repository<WebApiGetTripUsage>().Insert(webApiGetTripUsage);
                Uow.Save();
            }
            catch (Exception ex) { }

            return Ok(response.plan);
        }
     

    }
}
