using IDTO.TravelerPortal.Common;
using IDTO.TravelerPortal.Models;
using IDTO.TravelerPortal.Common.ExtensionMethods;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IDTO.TravelerPortal.Controllers
{
    public class TripDashboardController : Controller
    {
        private IHomeDataManager mHomeDataManager;

        public TripDashboardController(IHomeDataManager homeDataManager)
        {
            mHomeDataManager = homeDataManager;
        }

        //
        // GET: /TripDashboard/
        public async Task<ActionResult> Index()
        {
            TripDashboardViewModel tripDashboardModel = new TripDashboardViewModel();
            tripDashboardModel.SearchCriteria = new TripSearchCriteria()
            {
            };

            tripDashboardModel.PastTrips = await mHomeDataManager.GetPastTrips(User.Identity.GetUserName());
            tripDashboardModel.UpcomingTrips = await mHomeDataManager.GetUpcomingTrips(User.Identity.GetUserName());

            TempData["CurrentUpcomingTrips"] = tripDashboardModel.UpcomingTrips;

            return View("TripDashboard", tripDashboardModel);
        }
    }
}