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
using AttributeRouting.Web.Http;

namespace IDTO.WebAPI.Controllers
{
    [Authorize]
    public class TravelerController : BaseController
    {

        public TravelerController(IDbContext context)
            : base(context)
        {  }

        /// <summary>
        /// Get all Travelers (does not load trips)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TravelerModel> GetTravelers()
        {
            var t = Uow.Repository<Traveler>().Query().Get().ToList();
            return t.Select(trav => new TravelerModel(trav)).ToList();
        }

        /// <summary>
        /// Get Traveler by TravelerID.(does not load trips)
        /// GET api/Traveler/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerModel))]
        public IHttpActionResult GetTraveler(int id)
        {
            Traveler traveler = Uow.Repository<Traveler>().Find(id);
            if (traveler == null)
            {
                return NotFound();
            }

            return Ok(new TravelerModel(traveler));
        }

        /// <summary>
        /// Get Traveler by email.(does not load trips)
        /// GET api/Traveler  load parameter by name 'email'
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerModel))]
        public IHttpActionResult GetTravelerByEmail(string email)
        {
            var trav = Uow.Repository<Traveler>().Query().Filter(t => t.Email.Equals(email)).Get();
            if (!trav.Any())
            {
                return NotFound();
            }

            return Ok(new TravelerModel(trav.First()));
        }

        /// <summary>
        /// Get Traveler by loginId.(does not load trips)
        /// GET api/Traveler  load parameter by name 'loginid'
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerModel))]
        public IHttpActionResult GetTravelerByLoginId(string loginid)
        {
            var trav = Uow.Repository<Traveler>().Query().Filter(t => t.LoginId.Equals(loginid)).Get();
            if (!trav.Any())
            {
                return NotFound();
            }

            return Ok(new TravelerModel(trav.First()));
        }

        /// <summary>
        /// Update a new Traveler in the database.
        /// PUT api/Traveler/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="traveler"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerModel))]
        public IHttpActionResult PutTraveler(int id, [FromBody]TravelerModel traveler)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != traveler.Id)
            {
                return BadRequest();
            }

            //Check for promo code
            if(!String.IsNullOrEmpty(traveler.PromoCode))
            {
                List<PromoCode> promoCodes =  Uow.Repository<PromoCode>().Query().Get().Where(t=> t.Code == traveler.PromoCode).ToList();

                if(promoCodes.Count <= 0)
                {
                    return BadRequest("Invalid Promo Code");
                }
            }

            Traveler travelerEntity = traveler.ToTraveler();

            //Set modified date to now.
            travelerEntity.ModifiedDate = DateTime.UtcNow;

            Uow.Repository<Traveler>().Update(travelerEntity);

            try
            {
                Uow.Save();
                TravelerModel tm = new TravelerModel(travelerEntity);
                return Ok(tm);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TravelerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Update a new Traveler in the database.
        /// PUT api/Traveler/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="testString"></param>
        /// <returns></returns>
        [ResponseType(typeof(String))]
        [PUT("api/Traveler/{id}?testString={testString}"), System.Web.Http.HttpPut]
        public IHttpActionResult PutTraveler(int id, string testString)
        {
                return Ok("Success");
        }


        /// <summary>
        /// Create a new Traveler in the database.
        /// POST api/Traveler
        /// </summary>
        /// <param name="traveler"></param>
        /// <returns></returns>
        [ResponseType(typeof(TravelerModel))]
        public IHttpActionResult PostTraveler(TravelerModel traveler)
        {
            try{
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Check for promo code
            if (!String.IsNullOrEmpty(traveler.PromoCode))
            {
                List<PromoCode> promoCodes = Uow.Repository<PromoCode>().Query().Get().Where(t => t.Code == traveler.PromoCode).ToList();

                if (promoCodes.Count <= 0)
                {
                    return BadRequest("Invalid Promo Code");
                }
            }

            Traveler travelerEntity = traveler.ToTraveler();
           
            //Set creation and modified date to now.
            travelerEntity.CreatedDate = DateTime.UtcNow;
            travelerEntity.ModifiedDate = DateTime.UtcNow;
            
            Uow.Repository<Traveler>().Insert(travelerEntity);
            Uow.Save();
            TravelerModel tm = new TravelerModel(travelerEntity);

            return CreatedAtRoute("DefaultApi", new { id = tm.Id }, tm);
            }
            catch (Exception ex)
            {
                string msg = RecordException(ex, "TravelerController.PostTraveler");
                HttpResponseMessage responseMessage = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(responseMessage);
            }
        }

        // DELETE api/Traveler/5
        [ResponseType(typeof(Traveler))]
        private IHttpActionResult DeleteTraveler(int id)
        {
            Traveler traveler = Uow.Repository<Traveler>().Find(id);
            if (traveler == null)
            {
                return NotFound();
            }

            Uow.Repository<Traveler>().Delete(traveler);
            Uow.Save();

            return Ok(traveler);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Uow.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TravelerExists(int id)
        {
            return Uow.Repository<Traveler>().Find(id) != null;
        }
    }
}