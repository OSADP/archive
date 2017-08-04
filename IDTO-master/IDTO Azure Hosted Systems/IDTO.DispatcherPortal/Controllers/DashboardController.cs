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
using IDTO.Common;
using IDTO.DispatcherPortal.Models;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace IDTO.DispatcherPortal.Controllers
{
    public class DashboardController : AsyncController
    {
        readonly IUnitOfWork Uow;
        // private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string connectionString = String.Empty;
        private PushNotificationManager notificationManager;

        public DashboardController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
            IDTOContext idto = new IDTOContext(connectionString);
            this.Uow = new UnitOfWork(idto);

            String EndPointConnection = ConfigurationManager.AppSettings["EndPointConnection"];
            String HubName =ConfigurationManager.AppSettings["HubName"];

            notificationManager = new PushNotificationManager(EndPointConnection,HubName);
        }
        /// <summary>
        /// for unit testing
        /// </summary>
        /// <param name="unitOfWork"></param>
        public DashboardController(IUnitOfWork unitOfWork)
        {
            this.Uow = unitOfWork;
        }
        // GET: /TConnectedVehicle/
        //public ActionResult Index()
        //{
        //    return View(Uow.Repository<TConnectedVehicle>().Query().Get().OrderByDescending(s => s.OriginallyScheduledDeparture).ToList());
        //}

        public ActionResult Index()
        {
           // var vehicles = GetVehiclesWithPendingRequests();

          //  return View(vehicles);

            //Response.AddHeader("REFRESH", "10;URL=" + Request.Url.ToString());
        
            return View();
        }
        public ActionResult NewTConnect()
        {
            var vehicles = GetVehiclesWithPendingRequests();

            List<int> CurrentIdsSeen;
            if (TempData.ContainsKey("CurrentIdsSeen"))
            {
                CurrentIdsSeen = (List<int>)TempData["CurrentIdsSeen"];
            }
            else
            {
                CurrentIdsSeen = new List<int>();
            }

            foreach (var v in vehicles)
            {
                //Flag an audio reminder if a new ID comes in
                if (!CurrentIdsSeen.Contains(v.Id))
                {
                    CurrentIdsSeen.Add(v.Id);
                    v.SoundReminder = true;
                }
                else
                {
                    v.SoundReminder = false;
                }
            }

            //Clean up any stale IDs in the static list
            for (int i = 0; i < CurrentIdsSeen.Count; i++)
            {                
               if (vehicles.Find(x => x.Id == CurrentIdsSeen[i]) == null)
               {
                   CurrentIdsSeen.RemoveAt(i);
                   i--;
               }
            }
            
            TempData["CurrentIdsSeen"] = CurrentIdsSeen;
            TempData.Keep("CurrentIdsSeen");
            return PartialView(vehicles);
        }
        public ActionResult Active()
        {

            var vehicles = GetVehiclesWithActiveRequests();

            return PartialView(vehicles);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Image(int id)
        {
        //row id starts at 1.
            int row = id;
            string blue = "../../Images/Bus_Blue_160.png";
            string green = "../../Images/Bus_Green_160.png";
            string red = "../../Images/Bus_Red_160.png";
            string img = blue;

            List<string> imagesToRotate = new List<string>();
            imagesToRotate.Add(red);    
            imagesToRotate.Add(blue);
            imagesToRotate.Add(green);

            if (row > 2)
            {
                row = row % 3;
                img = imagesToRotate[row];
            }
            else
            {
                img = imagesToRotate[row];
            }

            FileStream fs = new FileStream(Server.MapPath(img), FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fs, "image/jpeg");

        }

        private List<DashboardViewModel> GetVehiclesWithActiveRequests()
        {
            //get vehicles that have requests with status of New
    //        var vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get().OrderByDescending(o=>o.EstimatedTimeArrival).Include(r => r.TConnect)
    //.Where(r => r.TConnectStatusId.Equals((int)TConnectStatuses.Accepted))
    //.Select(r => r.TConnectedVehicleId).Distinct();

            string email = User.Identity.GetUserName();

            var dispatchers = Uow.Repository<Dispatcher>().Query().Get().Where(d => d.Email == email);

            var dispatcher = dispatchers.FirstOrDefault<Dispatcher>();

            int providerId = dispatcher.ProviderId;

            var vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect.OutboundStep)
                .Where(r => r.TConnect.TConnectStatusId.Equals((int)TConnectStatuses.Monitored)
                && r.TConnectStatusId.Equals((int)TConnectStatuses.Accepted) && r.TConnect.OutboundStep.FromProviderId==providerId)
                .Select(r => r.TConnectedVehicleId).Distinct();

            var vehicles = Uow.Repository<TConnectedVehicle>().Query().Get()
                .Include("Block")
                .Where(q => vehicleidsWithNew.Any(v => v.Equals(q.Id)))
                .Select(s => new DashboardViewModel
                {
                    Id = s.Id,
                    CurrentAcceptedHoldMinutes = s.CurrentAcceptedHoldMinutes,
                    TConnectStopCode = s.TConnectStopCode,
                    TConnectFromName = s.TConnectFromName,
                    TConnectRoute = s.TConnectRoute,
                    Block = s.Block,
                    OriginallyScheduledDeparture = s.OriginallyScheduledDeparture
                }).OrderByDescending(s => s.OriginallyScheduledDeparture).ToList();

            //Can't figure out how to embed this up above. Runtime error results.
            foreach (var v in vehicles)
            {
                v.RequestedHoldMinutes = Uow.Repository<TConnectRequest>().Query().Get()
                    .Where(r => r.TConnectedVehicleId.Equals(v.Id)).OrderByDescending(r => r.RequestedHoldMinutes)
                    .First().RequestedHoldMinutes;
                v.DepartureTime = v.OriginallyScheduledDeparture.ToString();
            }

            return vehicles;
        }
        public List<DashboardViewModel> GetVehiclesWithPendingRequests()
        {

            //get vehicles that have requests with status of New
            //var vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect)             
            //    .Select(r => r.TConnectedVehicleId).Distinct();

            //get vehicles that have requests with status of New
            string email = User.Identity.GetUserName();

            var dispatchers = Uow.Repository<Dispatcher>().Query().Get().Where(d => d.Email == email);

            var dispatcher = dispatchers.FirstOrDefault<Dispatcher>();

            int providerId = dispatcher.ProviderId;


            var vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect.OutboundStep)
                .Where(r => r.TConnectStatusId.Equals((int)TConnectStatuses.New)
                && r.TConnect.TConnectStatusId.Equals((int)TConnectStatuses.Monitored) && r.TConnect.OutboundStep.FromProviderId == providerId)
                .Select(r => r.TConnectedVehicleId).Distinct();

            //select those vehicles, and load into DashboardViewModel
            var vehicles = Uow.Repository<TConnectedVehicle>().Query().Get()
                .Include("Block")
                .Where(q => vehicleidsWithNew.Any(v => v.Equals(q.Id)))
                .Select(s => new DashboardViewModel
                {
                    Id = s.Id,
                    CurrentAcceptedHoldMinutes = s.CurrentAcceptedHoldMinutes,
                    TConnectStopCode = s.TConnectStopCode,
                    TConnectFromName = s.TConnectFromName,
                    TConnectRoute = s.TConnectRoute,
                    Block = s.Block,
                    OriginallyScheduledDeparture = s.OriginallyScheduledDeparture
                }).OrderByDescending(s => s.OriginallyScheduledDeparture).ToList();

            //Can't figure out how to embed this up above. Runtime error results.
            foreach (var v in vehicles)
            {
                v.RequestedHoldMinutes = Uow.Repository<TConnectRequest>().Query().Get()
                    .Where(r => r.TConnectedVehicleId.Equals(v.Id)).OrderByDescending(r => r.RequestedHoldMinutes)
                    .First().RequestedHoldMinutes;
                v.DepartureTime = v.OriginallyScheduledDeparture.ToUniversalTime().ToString();
                var t = v.OriginallyScheduledDeparture.ToString();
            }
            // int mustNotBeInQueryLine = tvehAll[0].Id;
            //int test = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(mustNotBeInQueryLine)).Count();

            return vehicles;
        }


        [HttpPost]

       public async Task<ActionResult> PendingReject
       (FormCollection collection, int? id)
        {
            if (id == null)
            {
                //todo error
                throw new Exception("TConnectedVehicle ID must be specified.");
            }



            await RejectAllWaitsForVehicle((int)id);

            return RedirectToAction("Index");
        }
        [HttpPost]

        public async Task<ActionResult> PendingAccept
       (FormCollection collection, int? id)
        {
            if (id == null)
            {
                //todo error
                throw new Exception("TConnectedVehicle ID must be specified.");
            }

            //int delayMinutesToAccept = 5;
            //AcceptWaitForVehicle((int)id, delayMinutesToAccept);
            await AcceptAllWaitsForVehicle_WaitMaxWindow((int)id);

            return RedirectToAction("Index");
        }
        /// <summary>
        /// Accept requested wait times that are less than the accepted amount.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="delayMinutesToAccept"></param>
        private void AcceptWaitForVehicle(int id, int delayMinutesToAccept)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.RequestedHoldMinutes <= delayMinutesToAccept && r.TConnectedVehicleId.Equals(id)).ToList();
            foreach (TConnectRequest req in reqs)
            {
                req.TConnectStatusId = (int)TConnectStatuses.Accepted;
                Uow.Repository<TConnectRequest>().Update(req);
            }


            TConnectedVehicle vehicle = Uow.Repository<TConnectedVehicle>().Find(id);
            vehicle.CurrentAcceptedHoldMinutes = delayMinutesToAccept;
            Uow.Repository<TConnectedVehicle>().Update(vehicle);
            Uow.Save();
        }
        private async Task AcceptAllWaitsForVehicle_WaitMaxWindow(int id)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(id))
                .Include(r => r.TConnect.OutboundStep).Include(r=>r.TConnect.InboundStep.Trip)
                .OrderByDescending(r => r.RequestedHoldMinutes).ToList();
            foreach (TConnectRequest req in reqs)
            {
                req.TConnectStatusId = (int)TConnectStatuses.Accepted;
                Uow.Repository<TConnectRequest>().Update(req);
                Uow.Repository<TripEvent>().Insert(new TripEvent(req.TConnect.OutboundStep.TripId, "T-Connect Request accepted by dispatcher"));
                bool result = await notificationManager.SendAcceptNotificationsAsync(req.TConnect.InboundStep.Trip.TravelerId.ToString());
            }
            TimeSpan maxwait = (TimeSpan)(reqs.First().TConnect.EndWindow - reqs.First().TConnect.OutboundStep.StartDate);

            TConnectedVehicle vehicle = Uow.Repository<TConnectedVehicle>().Find(id);
            if (reqs.Count > 0)
            {
                //Since we are accepting all waits (on the assumption that excessive wait times should not be permitted
                //via the window calculation in SaveTrip), then we can use the greated requestedHoldMinutes as the current accepted hold.
                vehicle.CurrentAcceptedHoldMinutes = maxwait.Minutes;
            }
            Uow.Repository<TConnectedVehicle>().Update(vehicle);
            Uow.Save();
        }
        private void AcceptAllWaitsForVehicle_WaitMaxRequest(int id)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(id)).OrderByDescending(r => r.RequestedHoldMinutes).ToList();
            foreach (TConnectRequest req in reqs)
            {
                req.TConnectStatusId = (int)TConnectStatuses.Accepted;
                Uow.Repository<TConnectRequest>().Update(req);
            }


            TConnectedVehicle vehicle = Uow.Repository<TConnectedVehicle>().Find(id);
            if (reqs.Count > 0)
            {
                //Since we are accepting all waits (on the assumption that excessive wait times should not be permitted
                //via the window calculation in SaveTrip), then we can use the greated requestedHoldMinutes as the current accepted hold.
                vehicle.CurrentAcceptedHoldMinutes = reqs.First().RequestedHoldMinutes;
            }
            Uow.Repository<TConnectedVehicle>().Update(vehicle);
            Uow.Save();
        }
        private async Task RejectAllWaitsForVehicle(int id)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect).Include(r => r.TConnect.OutboundStep).Include(r => r.TConnect.InboundStep.Trip).Where(r => r.TConnectedVehicleId.Equals(id)).ToList();
            foreach (TConnectRequest req in reqs)
            {
                req.TConnectStatusId = (int)TConnectStatuses.Rejected;
                Uow.Repository<TConnectRequest>().Update(req);

                //When TConnectRequests are rejected, we also stop monitoring them,
                //because if they made up time later, we don't want them to pop up a second time
                //to be rejected again.
                req.TConnect.TConnectStatusId = (int)TConnectStatuses.Done;
                Uow.Repository<TConnect>().Update(req.TConnect);
                Uow.Repository<TripEvent>().Insert(new TripEvent(req.TConnect.OutboundStep.TripId, "T-Connect Request rejected by dispatcher"));

                await notificationManager.SendRejectNotificationsAsync(req.TConnect.InboundStep.Trip.TravelerId.ToString());
            }


            TConnectedVehicle vehicle = Uow.Repository<TConnectedVehicle>().Find(id);
            vehicle.CurrentAcceptedHoldMinutes = 0;
            Uow.Repository<TConnectedVehicle>().Update(vehicle);
            Uow.Save();
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
