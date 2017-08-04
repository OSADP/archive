using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Globalization;
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
namespace IDTO.WebAPI.Controllers
{
    /// <summary>
    /// Add Trips for Traveler to the database; Retrieve Trips for Traveler from the database
    /// </summary>
    public class TripController : BaseController
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="TripController"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public TripController(IDbContext context)
            : base(context)
        { }

        //public TripController(IUnitOfWork specifiedUow)
        //    : base(specifiedUow)
        //{ }

        /// <summary>
        /// Get all trips
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TripModel> GetTrips()
        {
            var t = Uow.Repository<Trip>().Query().Get().ToList();
            return t.Select(trip => new TripModel(trip)).ToList();
        }

        // GET api/Trip/5
        //[ResponseType(typeof(Trip))]
        //public IHttpActionResult GetTrip(int id)
        //{
        //   // Trip trip = Uow.Repository<Trip>().Query().Filter(t => t.Id == id).Get().First();
        //    Trip trip = Uow.Repository<Trip>().Find(id);
        //    if (trip == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(trip);
        //}

        /// <summary>
        /// Get trip by id (and steps)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(TripModel))]
        public TripModel GetTrip(int id)
        {
            //example for logging info messages
            Log(TraceLevel.Info,"TripModel.GetTrip","Get trip by id: "+ id.ToString());
          
            Trip trip = Uow.Repository<Trip>().Find(id);
            if (trip == null)
            {
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.NotFound, "No trip found with id " + id);
                throw new HttpResponseException(responseMessage);
            }

            TripModel tm = new TripModel(trip);
            List<Step> savedSteps = Uow.Repository<Step>().Query().Get().Include(ss=>ss.Block).Where(ss => ss.TripId.Equals(tm.Id)).ToList();

            tm.Steps = savedSteps.Select(s => new StepModel(s)).ToList();
            return tm;
        }

     

        /// <summary>
        /// Gets a list of trips for the traveler based on filter criteria. 
        /// </summary>
        /// <param name="travelerID">Id of the traveler</param>
        /// <param name="type">Enum TripType.        Upcoming=1, InProgress=2,Past=3, All=4</param>
        /// <returns></returns>
        public IEnumerable<TripModel> GetTripsByUser(int travelerID, Nullable<TripType.Type> type)
        {
            try
            {
                //Ensure traveler exists
                if (Uow.Repository<Traveler>().Find(travelerID) == null)
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.NotFound, "No traveler found with id " + travelerID);
                    throw new HttpResponseException(responseMessage);
                }
                //If type of trip was not selected, give back all trips (unfiltered)
                if (type == null)
                {
                    type = TripType.Type.All;
                }
                List<Trip> tripsToSend = new List<Trip>();

                if (type == TripType.Type.Upcoming)
                {
                    tripsToSend = Uow.Repository<Trip>().Query().Filter(t => t.TravelerId == travelerID && t.TripStartDate >= DateTime.UtcNow).Get().OrderBy(o => o.TripStartDate).ToList();
                }
                else if (type == TripType.Type.Past) //sort by descending
                {
                    tripsToSend = Uow.Repository<Trip>().Query().Filter(t => t.TravelerId == travelerID && t.TripEndDate <= DateTime.UtcNow).Get().OrderByDescending(o => o.TripStartDate).ToList();
                }
                else if (type == TripType.Type.InProgress)
                {
                    tripsToSend = Uow.Repository<Trip>().Query().Filter(t => t.TravelerId == travelerID && t.TripStartDate <= DateTime.UtcNow && t.TripEndDate >= DateTime.UtcNow).Get().OrderBy(o => o.TripStartDate).ToList();
                }
                else//All
                {
                    tripsToSend = Uow.Repository<Trip>().Query().Filter(t => t.TravelerId == travelerID).Get().OrderBy(o => o.TripStartDate).ToList();
                }

                var tts = tripsToSend.Select(trip => new TripModel(trip)).ToList();
                foreach(TripModel tm in tts)
                {
                    List<Step> savedSteps = Uow.Repository<Step>().Query().Get().Include(ss=>ss.Block).Where(ss => ss.TripId.Equals(tm.Id)).ToList();
                    tm.Steps = savedSteps.Select(s => new StepModel(s)).ToList();
                }
                return tts;
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "TripController.GetTripsByUser");
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(responseMessage);
            }
        }
        // PUT api/Trip/5
        private IHttpActionResult PutTrip(int id, Trip trip)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != trip.Id)
            {
                return BadRequest();
            }

            Uow.Repository<Trip>().Update(trip);

            try
            {
                Uow.Save();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TripExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Trip
        /// <summary>
        /// Adds a new trip and steps for the specified traveler to the database.
        /// </summary>
        /// <param name="trip">trip data</param>
        /// <returns></returns>
        [ResponseType(typeof(TripModel))]
        public IHttpActionResult PostTrip(TripModel trip)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                   throw new WebFaultException<string>("ModelState not valid", HttpStatusCode.BadRequest);
                }
                //Validate 
                //Ensure traveler exists
                if (Uow.Repository<Traveler>().Find(trip.TravelerId) == null)
                {
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.NotFound, "No traveler found with id " + trip.TravelerId);
                    throw new HttpResponseException(responseMessage);
                }
                //if (trip.TripStartDate < DateTime.UtcNow)
                //{
                //    //Cant add trips in the past
                //    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Trips cannot be created in the past");
                //    throw new HttpResponseException(responseMessage);
                //}
                if (trip.TripEndDate < trip.TripStartDate)
                {
                    //Cant add trips that finish before they start
                    HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid trip stop time");
                    throw new HttpResponseException(responseMessage);
                }


                Trip tripEntity = new Trip();
                tripEntity.TravelerId = trip.TravelerId;
                tripEntity.Origination = trip.Origination;
                tripEntity.Destination = trip.Destination;
                tripEntity.TripStartDate = (DateTime)trip.TripStartDate;
                tripEntity.TripEndDate = (DateTime)trip.TripEndDate;
                tripEntity.MobilityFlag = (bool)trip.MobilityFlag;
                tripEntity.BicycleFlag = (bool)trip.BicycleFlag;
                tripEntity.PriorityCode = trip.PriorityCode;
                //Set creation and modified date to now.
                tripEntity.CreatedDate = tripEntity.ModifiedDate = DateTime.UtcNow;
                List<Step> steps = new List<Step>();
                int stepnumber = 1;
                foreach (StepModel sm in trip.Steps)
                {
                    Step stepEntity = new Step();
                    stepEntity.Distance = (decimal)sm.Distance;
                    stepEntity.EndDate = (DateTime)sm.EndDate;
                    stepEntity.FromName = sm.FromName;
                    if (sm.FromProviderId == 0) sm.FromProviderId = null;
                    stepEntity.FromProviderId = sm.FromProviderId;
                    stepEntity.FromStopCode = sm.FromStopCode;
                    stepEntity.ModeId = (int)sm.ModeId;
                    stepEntity.RouteNumber = sm.RouteNumber;
                    stepEntity.BlockIdentifier = sm.BlockIdentifier;
                    stepEntity.StartDate = (DateTime)sm.StartDate;
                    stepEntity.StepNumber = stepnumber++;
                    stepEntity.ToName = sm.ToName;
                    stepEntity.EncodedMapString = sm.EncodedMapString;
                    if (sm.ToProviderId == 0) sm.ToProviderId = null;
                    stepEntity.ToProviderId = sm.ToProviderId;
                    stepEntity.ToStopCode = sm.ToStopCode;
                    steps.Add(stepEntity);
                }

                string tConnectWindow = ConfigurationManager.AppSettings["TConnectWindowInMinutes"];
                int tConnWindowMin = 0;
                ITripService tripService = int.TryParse(tConnectWindow, out tConnWindowMin) ? new TripService(tConnWindowMin) : new TripService();
                int id = tripService.SaveTrip(tripEntity, steps, Uow);

                Trip savedtrip = Uow.Repository<Trip>().Find(id);
                List<Step> savedSteps = Uow.Repository<Step>().Query().Get().Include(s=>s.Block).Where(ss=> ss.TripId.Equals(id)).ToList();
                TripModel m = new TripModel(savedtrip);
                m.Steps = savedSteps.Select(s => new StepModel(s)).ToList();

                return CreatedAtRoute("DefaultApi", new { id = m.Id }, m);//same as

            }
            catch(HttpResponseException hrex)//rethrow failed validation errors so we don't obliterate the httpstatuscode
            {
                string msg = RecordException(hrex, "TripController.PostTrip");
                throw hrex;
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "TripController.PostTrip");
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(responseMessage);
            }
        }

        /// <summary>
        /// Delete a trip (and steps) and any linked tconnect.
        ///  DELETE api/Trip/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Trip))]
        public IHttpActionResult DeleteTrip(int id)
        {
            try
            {
                Trip trip = Uow.Repository<Trip>().Find(id);
                if (trip == null)
                {
                    return NotFound();
                }
                //find tconnects that reference the steps owned by this trip
                var stepsForTrip  = Uow.Repository<Step>().Query().Get().Include(s=>s.Block).Where(s => s.TripId.Equals(trip.Id)).Select(i=>i.Id).ToList();
                
                List<TConnect> tconn = Uow.Repository<TConnect>().Query().Get().Where(t => stepsForTrip.Any(s=> s.Equals(t.InboundStepId))).ToList();
                foreach (TConnect tc in tconn)
                {
                    //Delete the tconnect
                    // TConnect tonnect = Uow.Repository<Trip>().Find(tconn.id);
                    Uow.Repository<TConnect>().Delete(tc);
                }
                //Delete the trip (cascade delete will do the steps).
                Uow.Repository<Trip>().Delete(trip);
                Uow.Save();

                return Ok(trip);
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "TripController.DeleteTrip");
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(responseMessage);
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

        private bool TripExists(int id)
        {
            return Uow.Repository<Trip>().Find(id) != null;
        }
    }
}