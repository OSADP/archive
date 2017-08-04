using IDTO.TravelerPortal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IDTO.TravelerPortal.Controllers
{
    [AllowAnonymous] 
    public class WeatherController : Controller
    {
        IHomeDataManager mHomeDataManager;

        public WeatherController(IHomeDataManager homeDataMgr)
        {
            mHomeDataManager = homeDataMgr;
        }
        //
        // GET: /Weather/
        public ActionResult WeatherPartial()
        {
            return PartialView("_WeatherPartial", mHomeDataManager);
        }
    }
}