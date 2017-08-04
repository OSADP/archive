using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using IDTO.Entity.Models;
using IDTO.Data;
using Repository;
using Repository.Providers.EntityFramework;
using IDTO.WebAPI.Models;
using IDTO.Service;
using System.ServiceModel.Web;
using System.Web.Http.Tracing;
using System.Runtime.Serialization;
using IDTO.Common;
namespace IDTO.WebAPI.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public class TConnectController : BaseController
    {
        public TConnectController(IDbContext context)
            : base(context)
        { }

        /// <summary>
        /// Gets a status for the trip.
        /// </summary>
        /// <param name="tripid">id for trip </param>
        /// <returns>List of TConnect Statuses associated with trip. Though rare, a trip can have more than one TConnect.</returns>
        [ResponseType(typeof(IEnumerable<TConnectStatusModel>))]
        public IEnumerable<TConnectStatusModel> Get(int tripId)
        {
            //Get the Tconnects that are related to this trip.  Could have more than one, technically.
            List<TConnectStatusModel> statuses = new List<TConnectStatusModel>();

            Trip trip = Uow.Repository<Trip>().Find(tripId);
            if (trip == null)
            {
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "No trip found with id " + tripId);
                throw new HttpResponseException(responseMessage);
            }

            //If the trip is over or not yet started, we don't really involve any tconnect logic,
            //just return that much.
            if (trip.TripStartDate > DateTime.UtcNow)
            {
                //Trip not started
                TConnectStatusModel sm = new TConnectStatusModel
                {
                    TripId = tripId,
                    TConnectStatusId = (int)TConnectStatusModel.Status.Saved
                };
                statuses.Add(sm);
                return statuses;
            }
            if (trip.TripEndDate < DateTime.UtcNow)
            {
                //Trip over
                TConnectStatusModel sm = new TConnectStatusModel
                {
                    TripId = tripId,
                    TConnectStatusId = (int)TConnectStatusModel.Status.Completed
                };
                statuses.Add(sm);
                return statuses;
            }




            var tconnectsForTrip = Uow.Repository<TConnect>().Query().Get()
                  .Where(t => t.InboundStep.TripId.Equals(tripId)).ToList();

            if (tconnectsForTrip.Count < 1)
            {
                //This trip has no tconnects, just say it is inProgress.
                TConnectStatusModel sm = new TConnectStatusModel
                {
                    TripId = tripId,
                    TConnectStatusId = (int)TConnectStatusModel.Status.InProgress
                };
                statuses.Add(sm);
                return statuses;
            }

            foreach (TConnect tc in tconnectsForTrip)
            {
                //The status that the MDT cares about is a combination between the 
                //TConnect status (used by the monitor to determine whether to Monitor) 
                //and the TConnectRequest status (used by the Dispatcher and viewers)
                int externalStatus = (int)DeduceExternalStatusForTConnect(tc);

                TConnectStatusModel sm = new TConnectStatusModel
                    {
                        TripId = tripId,
                        TConnectStatusId = externalStatus
                    };
                statuses.Add(sm);
            }

            return statuses;
        }

        /// <summary>
        /// The MDT wants to see status of request.
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        public TConnectStatusModel.Status DeduceExternalStatusForTConnect(TConnect tc)
        {
            TConnectStatusModel.Status externalStatus = TConnectStatusModel.Status.Monitored;

            var trlist = Uow.Repository<TConnectRequest>().Query().Get()
                .Where(t => t.TConnectId.Equals(tc.Id)).ToList();

            if (trlist.Count > 0)//Has a Request
            {
                var tr = trlist.First();
                if (tc.TConnectStatusId == (int)TConnectStatuses.Done &&
                    tr.TConnectStatusId == (int)TConnectStatuses.New)
                {
                    //This request expired with no response, same as rejected.
                    externalStatus = TConnectStatusModel.Status.AutoRejected;
                }
                else
                {
                    if (tr.TConnectStatusId == (int)TConnectStatuses.Accepted)
                    {
                        externalStatus = TConnectStatusModel.Status.Accepted;
                    }
                    else if (tr.TConnectStatusId == (int)TConnectStatuses.Rejected)
                    {
                        externalStatus = TConnectStatusModel.Status.Rejected;
                    }
                    else//New
                    {
                        externalStatus = TConnectStatusModel.Status.Requested;
                    }

                }
            }
            //else //TConnect has no requests
            //{
            //    if (tc.TConnectStatusId == (int)TConnectStatuses.New
            //        || tc.TConnectStatusId == (int)TConnectStatuses.Monitored)
            //    {
            //        //'New' for a tconnect is a very tiny window before the monitor picks it
            //        //up, it is essentially the same as Monitored.
            //        externalStatus = (int)TConnectStatuses.Monitored;
            //    }
            //    else //Done
            //    {
            //        externalStatus = (int)TConnectStatuses.Done;
            //    }
            //}

            return externalStatus;
        }

        [ResponseType(typeof(IEnumerable<TConnectRequest>))]
        public IHttpActionResult GetTConnectRequests()
        {
            var tripTConRequests = Uow.Repository<TConnectRequest>().Query().Get().ToList();
            return Ok(tripTConRequests);
        }

        [ResponseType(typeof(IEnumerable<TConnectRequest>))]
        public IHttpActionResult GetTConnectRequestsWithinPeriod(TimeSpan timespan)
        {
            var tripTConRequests = Uow.Repository<TConnectRequest>().Query().Get().Where(tr=>tr.EstimatedTimeArrival >= DateTime.Now.Subtract(timespan)).ToList();
            return Ok(tripTConRequests);
        }
    }
    [DataContract]
    public class TConnectStatusModel
    {
        /// <summary>
        /// Unique identifier created for this trip.
        /// </summary>
        [DataMember]
        public int TripId { get; set; }
        /// <summary>
        /// Enum integer for TConnectStatus from table in database.
        /// </summary>
        [DataMember]
        public int TConnectStatusId { get; set; }

        public enum Status
        {
            Saved = 0,//(a trip that has been created but not started)
            InProgress = 1,//(a trip that has started, but no TConnetions exist for this trip)
            Monitored = 2,// (a trip that has a TConnection)
            Requested = 3,// (a trip that has a TConnection request created and waiting for dispatcher action)
            AutoRejected = 4,//a trip that had  TConnection request generated but was never accepted or rejected by the dispatcher during its window.)
            Rejected = 5,//(a trip that had a TConnection request rejected by the dispatcher)
            Accepted = 6,//(a trip that had a TConnection request accepted by the dispatcher)
            Completed = 7,//(a trip which whose end time has passed)
            Deleted = 8// (a trip deleted by the user, for future use)


        }
    }
}
