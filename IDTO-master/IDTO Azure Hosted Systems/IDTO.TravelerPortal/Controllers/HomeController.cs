using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.Mvc;
using System.Threading.Tasks;
using IDTO.TravelerPortal.Models;
using IDTO.TravelerPortal.Common;
using IDTO.TravelerPortal.Common.Models;
using Microsoft.AspNet.Identity;

namespace IDTO.TravelerPortal.Controllers
{
    public class HomeController : Controller
    {
        private IHomeDataManager mHomeDataManager;

        public HomeController(IHomeDataManager homeDataMgr)
        {
            mHomeDataManager = homeDataMgr;
        }

        //
        // GET: /NextTrip/
        [ChildActionOnly]
        [AsyncTimeout(5000)]
        public ActionResult NextTripPartial()
        {
            Trip nextTrip = mHomeDataManager.GetNextTrip(User.Identity.GetUserName());
            NextTripViewModel model = new NextTripViewModel(nextTrip);
            return nextTrip == null ? PartialView(null) : PartialView(model);
        }

        //This is the landing page when not logged in and the default page
        [AllowAnonymous]
        public ActionResult Index()
        {
            Trace.WriteLine("In HomeController Index");

            return View();
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [AllowAnonymous]
        public ActionResult UserTips()
        {
            ViewBag.Message = "C-Ride User Tips";

            return View();
        }

        [AllowAnonymous]
        public ActionResult RegistrationAgreement()
        {
            return View();
        }

        [AllowAnonymous]
        public PartialViewResult RegistrationAgreementPartial()
        {
            return PartialView();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}