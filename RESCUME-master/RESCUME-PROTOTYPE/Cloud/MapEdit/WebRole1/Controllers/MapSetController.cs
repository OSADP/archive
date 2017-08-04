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
using MapEdit.Data.Models;

namespace WebRole1.Controllers
{
    public class MapSetController : ApiController
    {
        private IncZoneMapContext db = new IncZoneMapContext();

        // GET api/MapSet
        public IQueryable<mapSet> GetmapSetsQueryable()
        {
            return db.mapSets;
        }
		public List<mapSet> GetmapSets()
		{
			using (var context = new IncZoneMapContext())
			{
				//var result = db.mapNodes.ToList();
				var result = db.mapSets.ToList();
				return result;
				//	//context.ContextOptions.ProxyCreationEnabled = false;

				//	context.ObjectContext().ContextOptions.ProxyCreationEnabled = false;

				//	return context.mapNodes.Where(s => s.mapSetId == id);
			}
		}

        // GET api/MapSet/5
        [ResponseType(typeof(mapSet))]
		public IHttpActionResult GetmapSet(Guid id)
        {
            mapSet mapset = db.mapSets.Find(id);
            if (mapset == null)
            {
                return NotFound();
            }

            return Ok(mapset);
        }

        // PUT api/MapSet/5
		public IHttpActionResult PutmapSet(Guid id, mapSet mapset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mapset.Id)
            {
                return BadRequest();
            }

            db.Entry(mapset).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mapSetExists(id))
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

        // POST api/MapSet
        [ResponseType(typeof(mapSet))]
        public IHttpActionResult PostmapSet(mapSet mapset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.mapSets.Add(mapset);
            db.SaveChanges();

			//return CreatedAtRoute("DefaultApi", new { id = mapset.Id }, mapset);
			var stuff = CreatedAtRoute("DefaultApi", new { id = mapset.Id }, mapset);

			return Ok(mapset);
		}

        // DELETE api/MapSet/5
        [ResponseType(typeof(mapSet))]
		public IHttpActionResult DeletemapSet(Guid id)
        {
            mapSet mapset = db.mapSets.Find(id);
            if (mapset == null)
            {
                return NotFound();
            }

            db.mapSets.Remove(mapset);
            db.SaveChanges();

            return Ok(mapset);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

		private bool mapSetExists(Guid id)
        {
            return db.mapSets.Count(e => e.Id == id) > 0;
        }
    }
}