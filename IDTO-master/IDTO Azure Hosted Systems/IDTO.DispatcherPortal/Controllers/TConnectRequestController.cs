using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using System.Configuration;
using IDTO.DispatcherPortal.Models;
using IDTO.Common;


namespace IDTO.DispatcherPortal.Controllers
{
    public class TConnectRequestController : Controller
    {
        readonly IUnitOfWork Uow;
        // private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string connectionString = String.Empty;

        public TConnectRequestController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
            IDTOContext idto = new IDTOContext(connectionString);
            this.Uow = new UnitOfWork(idto);





        }
        // GET: /TConnectRequest/
        public ActionResult Index()
        {
            int hours=1;
            TConnRequestViewModel model = LoadModelData(hours);
            return View(model);
        }

        private TConnRequestViewModel LoadModelData(int hours)
        {
            hours *= -1;//make negative.
            DateTime datefrom = DateTime.UtcNow.AddHours(hours);
            var tconnectrequests = Uow.Repository<TConnectRequest>().Query().Include(t => t.TConnect)
                .Include(t => t.TConnectedVehicle).Include(t => t.TConnectStatus).Get()
                .Where(t => t.EstimatedTimeArrival > datefrom)
                .OrderByDescending(t => t.EstimatedTimeArrival).Select(t => new TConnRequestViewModel.Rows
                {
                    EstimatedTimeArrival = t.EstimatedTimeArrival,
                    InboundVehicle = t.TConnect.InboundVehicle,
                    RequestedHoldMinutes = t.RequestedHoldMinutes,
                    TConnectStopCode = t.TConnectedVehicle.TConnectStopCode,
                    AcceptedWaitTime = t.TConnectedVehicle.CurrentAcceptedHoldMinutes,
                    //Status = t.TConnectStatus.Name
                    Status = (t.TConnect.EndWindow < DateTime.UtcNow && t.TConnectStatusId == (int)TConnectStatuses.New) ? "Expired" : t.TConnectStatus.Name
                });

            TConnRequestViewModel model = new TConnRequestViewModel();
            model.RequestRows = tconnectrequests.ToList();

            model.HourListItems = new[]
                   {
                        new SelectListItem() { Text = "1 Hr", Value = "1" },                      
                        new SelectListItem() { Text = "2 Hr", Value = "2" },
                        new SelectListItem() { Text = "3 Hr", Value = "3" },
                        new SelectListItem() { Text = "12 Hr", Value = "12" } ,
                        new SelectListItem() { Text = "1 Day", Value = "24" }
                   };
            return model;
        }
        [HttpPost]
        public ActionResult Index(TConnRequestViewModel mod)
        {


            int hoursToDisp = Convert.ToInt32(mod.SelectedItemId);

            
            TConnRequestViewModel model = LoadModelData(hoursToDisp);
         //set selected hour drop down correctly.
            model.SelectedItemId = mod.SelectedItemId;
            return View(model);
        }
        // GET: /TConnectRequest/Details/5
        /// <summary>
        /// list of tconnect requests for vehicle
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //TConnectRequest tconnectrequest = db.TConnectRequests.Find(id);
            //if (tconnectrequest == null)
            //{
            //    return HttpNotFound();
            //}
            var v = Uow.Repository<TConnectRequest>().Query().Include(t => t.TConnectStatus)
                .Include(t => t.TConnect).Get()
                .OrderByDescending(s => s.RequestedHoldMinutes).Where(t => t.TConnectedVehicleId.Equals((int)id));
            if (v.Count() != 0)
            {
                return View(v.ToList());
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
