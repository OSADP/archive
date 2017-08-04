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

namespace IDTO.DispatcherPortal.Controllers
{
    public class TConnectedVehicleController : Controller
    {
        readonly IUnitOfWork Uow;
        // private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string connectionString = String.Empty;

        public TConnectedVehicleController()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DataConnectionString"].ConnectionString;
            IDTOContext idto = new IDTOContext(connectionString);
            this.Uow = new UnitOfWork(idto);
        }
        /// <summary>
        /// for unit testing
        /// </summary>
        /// <param name="unitOfWork"></param>
        public TConnectedVehicleController(IUnitOfWork unitOfWork)
        {
            this.Uow = unitOfWork;
        }
        // GET: /TConnectedVehicle/
        public ActionResult Index()
        {
            return View(Uow.Repository<TConnectedVehicle>().Query().Get().OrderByDescending(s => s.OriginallyScheduledDeparture).ToList());
        }

        public ActionResult Pending()
        {
            var vehicles = GetVehiclesWithPendingRequests();

            return View(vehicles);
        }

        public  List<TConnVehicleViewModel> GetVehiclesWithPendingRequests()
        {
            //get vehicles that have requests with status of New
           // List<int> vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get()
           //     .Where(r => r.TConnectStatusId <= (int)TConnectStatuses.New).Select(r => r.TConnectedVehicleId).Distinct().ToList();
            var vehicleidsWithNew = Uow.Repository<TConnectRequest>().Query().Get().Include(r=>r.TConnect)
                .Where(r => r.TConnectStatusId.Equals((int)TConnectStatuses.New)
                && r.TConnect.TConnectStatusId.Equals((int)TConnectStatuses.Monitored))
                .Select(r => r.TConnectedVehicleId).Distinct();

            //select those vehicles, and load into TConnVehicleViewModel
            var vehicles = Uow.Repository<TConnectedVehicle>().Query().Get()
                .Where(q => vehicleidsWithNew.Any(v => v.Equals(q.Id)))
                .Select(s => new TConnVehicleViewModel
                {
                    Id = s.Id,
                    CurrentAcceptedHoldMinutes = s.CurrentAcceptedHoldMinutes,
                    TConnectStopCode = s.TConnectStopCode,
                    TConnectRoute = s.TConnectRoute,
                    OriginallyScheduledDeparture = s.OriginallyScheduledDeparture,
                    //NumberRequests =  Uow.Repository<TConnectRequest>().Query().Get().Where(r=>r.TConnectedVehicleId.Equals(s.Id)).Count()
                }).OrderByDescending(s => s.OriginallyScheduledDeparture).ToList();

            //Can't figure out how to embed this up above. Runtime error results.
            foreach (var v in vehicles)
            {
                v.NumberRequests = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(v.Id)).Count();
            }
            // int mustNotBeInQueryLine = tvehAll[0].Id;
            //int test = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(mustNotBeInQueryLine)).Count();
            return vehicles;
        }
        /*
                // GET: /TConnectedVehicle/Details/5
                public ActionResult Details(int? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    TConnectedVehicle tconnectedvehicle = db.TConnectedVehicles.Find(id);
                    if (tconnectedvehicle == null)
                    {
                        return HttpNotFound();
                    }
                    return View(tconnectedvehicle);
                }

                // GET: /TConnectedVehicle/Create
                public ActionResult Create()
                {
                    return View();
                }

                // POST: /TConnectedVehicle/Create
                // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
                // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Create([Bind(Include = "Id,OriginallyScheduledDeparture,CurrentAcceptedHoldMinutes,TConnectStopCode,ModifiedDate,ModifiedBy")] TConnectedVehicle tconnectedvehicle)
                {
                    if (ModelState.IsValid)
                    {
                        db.TConnectedVehicles.Add(tconnectedvehicle);
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }

                    return View(tconnectedvehicle);
                }

                // GET: /TConnectedVehicle/Edit/5
                public ActionResult Edit(int? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    TConnectedVehicle tconnectedvehicle = db.TConnectedVehicles.Find(id);
                    if (tconnectedvehicle == null)
                    {
                        return HttpNotFound();
                    }
                    return View(tconnectedvehicle);
                }

                // POST: /TConnectedVehicle/Edit/5
                // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
                // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
                [HttpPost]
                [ValidateAntiForgeryToken]
                public ActionResult Edit([Bind(Include = "Id,OriginallyScheduledDeparture,CurrentAcceptedHoldMinutes,TConnectStopCode,ModifiedDate,ModifiedBy")] TConnectedVehicle tconnectedvehicle)
                {
                    if (ModelState.IsValid)
                    {
                        db.Entry(tconnectedvehicle).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    return View(tconnectedvehicle);
                }

                // GET: /TConnectedVehicle/Delete/5
                public ActionResult Delete(int? id)
                {
                    if (id == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    TConnectedVehicle tconnectedvehicle = db.TConnectedVehicles.Find(id);
                    if (tconnectedvehicle == null)
                    {
                        return HttpNotFound();
                    }
                    return View(tconnectedvehicle);
                }

                // POST: /TConnectedVehicle/Delete/5
                [HttpPost, ActionName("Delete")]
                [ValidateAntiForgeryToken]
                public ActionResult DeleteConfirmed(int id)
                {
                    TConnectedVehicle tconnectedvehicle = db.TConnectedVehicles.Find(id);
                    db.TConnectedVehicles.Remove(tconnectedvehicle);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                */

        [HttpPost]

        public ActionResult Pending
       (string btnSubmit, FormCollection collection, int? id)
        {
            if (id == null)
            {
                //todo error
                throw new Exception("TConnectedVehicle ID must be specified.");
            }
            switch (btnSubmit)
            {
                case "Accept":
                    //int delayMinutesToAccept = 5;
                    //AcceptWaitForVehicle((int)id, delayMinutesToAccept);
                    AcceptAllWaitsForVehicle_WaitMaxWindow((int)id);
                    break;
                case "Reject":
                    RejectAllWaitsForVehicle((int)id);
                    break;
            }
            return RedirectToAction("Pending");
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
        private void AcceptAllWaitsForVehicle_WaitMaxWindow(int id)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Where(r => r.TConnectedVehicleId.Equals(id))
                .Include(r => r.TConnect.OutboundStep)
                .OrderByDescending(r=>r.RequestedHoldMinutes).ToList();
            foreach (TConnectRequest req in reqs)
            {
                req.TConnectStatusId = (int)TConnectStatuses.Accepted;
                Uow.Repository<TConnectRequest>().Update(req);
                Uow.Repository<TripEvent>().Insert(new TripEvent(req.TConnect.OutboundStep.TripId, "T-Connect Request accepted by dispatcher"));
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
        private void RejectAllWaitsForVehicle(int id)
        {
            var reqs = Uow.Repository<TConnectRequest>().Query().Get().Include(r => r.TConnect).Include(r => r.TConnect.OutboundStep).Where(r => r.TConnectedVehicleId.Equals(id)).ToList();
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
            }


            TConnectedVehicle vehicle = Uow.Repository<TConnectedVehicle>().Find(id);
            vehicle.CurrentAcceptedHoldMinutes = 0;
            Uow.Repository<TConnectedVehicle>().Update(vehicle);
            Uow.Save();
        }


        //[HttpParamAction]
        //public ActionResult Save(TConnVehicleViewModel model)
        //{
        //    return RedirectToAction("purple");
        //}

        //[HttpParamAction]
        //public ActionResult Publish(TConnVehicleViewModel model)
        //{
        //    return RedirectToAction("red");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
    //public class HttpParamActionAttribute : ActionNameSelectorAttribute
    //{
    //    public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
    //    {
    //        if (actionName.Equals(methodInfo.Name, StringComparison.InvariantCultureIgnoreCase))
    //            return true;

    //        var request = controllerContext.RequestContext.HttpContext.Request;
    //        return request[methodInfo.Name] != null;
    //    }
    //}
}
