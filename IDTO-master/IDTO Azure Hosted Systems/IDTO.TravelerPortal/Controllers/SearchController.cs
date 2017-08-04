using IDTO.TravelerPortal.Common;
using IDTO.TravelerPortal.Common.ExtensionMethods;
using IDTO.TravelerPortal.Common.Models;
using IDTO.TravelerPortal.Models;
using IDTO.TravelerPortal.Views.Search;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IDTO.TravelerPortal.Controllers
{
    public class SearchController : Controller
    {
        readonly int maxWalkDistanceDefault = Config.MaxWalkDistanceDefault;
        private IHomeDataManager mHomeDataManager;

        public SearchController(IHomeDataManager homeDataMgr)
        {
            mHomeDataManager = homeDataMgr;
        }

        public ActionResult SearchPartial()
        {
            return PartialView("_SearchPartial", new TripSearchCriteria() { time = DateTime.Now });
        }

        //
        // GET: /Search/
        [HttpGet]
        public ActionResult Search()
        {
            var trip = new TripSearchCriteria()
            {
            };
            return View(trip);
        }

        //
        // GET: /UpcomingTrips/
        [HttpGet]
        public async Task<ActionResult> UpcomingTrips()
        {
            UpcomingTripsViewModel tripsModel = new UpcomingTripsViewModel();

            tripsModel.Trips = await mHomeDataManager.GetUpcomingTrips(User.Identity.GetUserName());
            TempData["CurrentUpcomingTrips"] = tripsModel.Trips;
            return View(tripsModel);
        }

        //
        // GET: /PreviousTrips/
        [HttpGet]
        public async Task<ActionResult> PreviousTrips()
        {
            PreviousTripsViewModel tripsModel = new PreviousTripsViewModel();

            tripsModel.Trips = await mHomeDataManager.GetPastTrips(User.Identity.GetUserName());
            TempData["CurrentPreviousTrips"] = tripsModel.Trips;
            return View(tripsModel);
        }

        //
        // GET: /UpcomingTripDetails/
        [HttpGet]
        public ActionResult UpcomingTripDetails(int tripId)
        {
            Trip trip;
            if (TempData.ContainsKey("CurrentUpcomingTrips"))
            {
                trip = ((List<Trip>) TempData["CurrentUpcomingTrips"]).FirstOrDefault(i => i.Id == tripId);
                TempData.Keep("CurrentUpcomingTrips");

                if (trip != null)
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

                    return View(trip);
                }
            }
            return RedirectToAction("Index", "TripDashboard");
        }

        //
        // GET: /PreviousTripDetails/
        [HttpGet]
        public ActionResult PreviousTripDetails(int tripId)
        {
            Trip trip;
            if (TempData.ContainsKey("CurrentPreviousTrips"))
            {
                trip = ((List<Trip>)TempData["CurrentPreviousTrips"]).FirstOrDefault(i => i.Id == tripId);
                TempData.Keep("CurrentPreviousTrips");

                if (trip != null)
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

                    return View(trip);
                }
            }
            return RedirectToAction("Index", "TripDashboard");
        }

        [HttpPost]
        public async Task<ActionResult> Search(TripSearchCriteria criteria)
        {
            criteria.maxWalkMeters = maxWalkDistanceDefault;

            var plan = await GetPlanAsync(criteria);
            if (plan != null)
            {
                // Give each itinerary a temp id
                if (plan.itineraries != null)
                {
                    var itineraries = plan.itineraries.OrderBy(i => i.startTime);
                    foreach (var i in itineraries)
                    {
                        i.id = Guid.NewGuid();
                    }
                }

                TempData["CurrentPlan"] = plan;
            }

            return View("SearchResults", new SearchResultsViewModel(criteria, plan));
        }

        [HttpPost]
        public async Task<ActionResult> SearchUpdate(SearchResultsViewModel criteria)
        {
            return await Search(criteria.TripCriteria);
        }

        [HttpPost]
        public async Task<ActionResult> SaveTrip(Guid itineraryId)
        {
            Plan plan = (Plan)TempData["CurrentPlan"];
            var itinerary = plan.GetItinerary(itineraryId);

            string email = User.Identity.GetUserName();

            var webapi = new IDTOWebAPI();

            var traveler = await webapi.GetTravelerByEmail(email);

            Trip savedTrip = await PostTripFromItinerary(itinerary, traveler.Id, plan.FromLocation, plan.ToLocation, "1", traveler.DefaultMobilityFlag, traveler.DefaultBicycleFlag);

            TripSaveResultViewModel saveResultVM;
            if (savedTrip.Steps == null)
            {
                saveResultVM = null;
            }
            else
            {
                saveResultVM = new TripSaveResultViewModel(savedTrip);

            }

            return View("TripSaveResult", saveResultVM);
        }

        //
        // GET: /RepeatTrip/
        [HttpGet]
        public ActionResult RepeatTrip(int tripId)
        {
            Trip trip = ((List<Trip>)TempData["CurrentPreviousTrips"]).FirstOrDefault(i => i.Id == tripId);
            TempData.Keep("CurrentPreviousTrips");
            TripSearchCriteria criteria = new TripSearchCriteria();
            criteria.startLocation = trip.Origination;
            criteria.endLocation = trip.Destination;
            criteria.time = DateTime.Now;
            return View(criteria);
        }

        [HttpPost]
        public async Task<ActionResult> RepeatTrip(TripSearchCriteria criteria)
        {
            return await Search(criteria);
        }


        [HttpPost]
        public async Task<ActionResult> CancelTrip(int tripId)
        {
            TripSummaryForDelete deletedTrip = await mHomeDataManager.CancelTrip(tripId);
            return RedirectToAction("UpcomingTrips", "Search");
        }

        public ActionResult TripDetails(Itinerary itinerary)
        {
            return View(itinerary);
        }

        public ActionResult ItineraryDetails(Guid itineraryId)
        {
            Plan plan;
            if (TempData.ContainsKey("CurrentPlan"))
            {
                plan = (Plan)TempData["CurrentPlan"];
                TempData.Keep("CurrentPlan");

                if (plan.GetItinerary(itineraryId) != null)
                {
                    Itinerary itinerary = plan.GetItinerary(itineraryId);

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

                    return View(itinerary);
                }
            }

            return RedirectToAction("Index", "TripDashboard");
        }

        private async Task<Trip> PostTripFromItinerary(Itinerary itineray, int travelerId, string origin, string destination, string priorityCode, bool mobilityFlag, bool bicycleFlag)
        {
            Trip trip = ItineraryToTrip(itineray, travelerId, origin, destination, priorityCode, mobilityFlag, bicycleFlag);

            var webapi = new IDTOWebAPI();

            //Trip tripResult = await PostTrip(trip);
            var tripResult = await webapi.SaveTrip(trip);

            return tripResult;
        }

        private async Task<Plan> GetPlanAsync(TripSearchCriteria searchCriteria)
        {
            var webapi = new IDTOWebAPI();

            var plan = await webapi.GetPlan(searchCriteria);

            if (plan != null)
            {
                plan.FromLocation = searchCriteria.startLocation;
                plan.ToLocation = searchCriteria.endLocation;
            }

            return plan;
        }

        private Trip ItineraryToTrip(Itinerary itineray, int travelerId, string origin, string destination, string priorityCode, bool mobilityFlag, bool bicycleFlag)
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
                step.EncodedMapString = itineray.legs[i].legGeometry.points;

                if (step.EncodedMapString.Length > maxStringLength)
                {
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