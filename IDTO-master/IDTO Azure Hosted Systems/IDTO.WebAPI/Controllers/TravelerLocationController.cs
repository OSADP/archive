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
namespace IDTO.WebAPI.Controllers
{

    public class TravelerLocationController : BaseController
    {

        public TravelerLocationController(IDbContext context)
            : base(context)
        { }


        /// <summary>
        /// Create a new TravelerLocation entry in the database.
        /// POST api/TravelerLocation
        /// </summary>
        /// <param name="traveler"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerLocationModel))]
        public IHttpActionResult PostTraveler(TravelerLocationModel tloc)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                TravelerLocation tLocEntity = new TravelerLocation();
                tLocEntity.Latitude = tloc.Latitude;
                tLocEntity.Longitude = tloc.Longitude;
                tLocEntity.PositionTimestamp = tloc.TimeStamp;

                string loginid = tloc.UserId;
                //Lookup traveler Id
                var trav = Uow.Repository<Traveler>().Query().Filter(t => t.LoginId.Equals(loginid)).Get();
                if (!trav.Any())
                {
                    return NotFound();
                }

                tLocEntity.TravelerId = trav.First().Id;

                Uow.Repository<TravelerLocation>().Insert(tLocEntity);
                Uow.Save();
                TravelerLocationModel tm = new TravelerLocationModel(tLocEntity, tloc.UserId);

                return CreatedAtRoute("DefaultApi", new { id = tm.Id }, tm);
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "TravelerLocationController.PostTravelerLocation");
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


    }
}