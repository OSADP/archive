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
using EntityFramework.BulkInsert.Extensions;
using System.Transactions;

namespace WebRole1.Controllers
{
    public class MapLinkController : ApiController
    {
        private IncZoneMapContext db = new IncZoneMapContext();

        // GET api/MapLink
        public IQueryable<mapLink> GetmapLinksQueryable()
        {
            return db.mapLinks;
        }

		public List<mapLink> GetmapLinks(Guid id)
		{
			using (var context = new IncZoneMapContext())
			{
				context.ObjectContext().ContextOptions.ProxyCreationEnabled = false;
				var result = context.mapLinks.Where(s => s.mapSetId == id);

				var resultLsit = new List<mapLink>();
				foreach (var mapLink in result)
				{
					var newMapLink = new mapLink
					{
						Id = mapLink.Id,
						endMapNodeId = mapLink.endMapNodeId,
						startMapNodeId = mapLink.startMapNodeId
					};

					resultLsit.Add(newMapLink);
				}

				return resultLsit;
			}
		}

        // GET api/MapLink/5
        [ResponseType(typeof(mapLink))]
		public IHttpActionResult GetmapLink(Guid id)
        {
	        using (var context = new IncZoneMapContext())
	        {
				mapLink maplink = context.mapLinks.Find(id);
		        if (maplink == null)
		        {
			        return NotFound();
		        }
			
				return Ok(maplink);
			}
        }

        // PUT api/MapLink/5
        public IHttpActionResult PutmapLink(Guid id, mapLink maplink)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != maplink.Id)
            {
                return BadRequest();
            }

            db.Entry(maplink).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mapLinkExists(id))
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

		// POST api/MapLink
		[ResponseType(typeof(mapLink))]
		public IHttpActionResult PostmapLink(mapLink maplink)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			using (var context = new IncZoneMapContext())
			{
				context.mapLinks.Add(maplink);
				context.SaveChanges();
			}
			return CreatedAtRoute("DefaultApi", new { id = maplink.Id }, maplink);
		}

		// POST api/MapLink
		[ResponseType(typeof(mapLink))]
		public HttpStatusCode PostmapLinkList(List<mapLink> maplinks)
		{
			if (!ModelState.IsValid)
			{
				return HttpStatusCode.BadRequest;
			}

			using (var context = new IncZoneMapContext())
			{
				using (var transactionScope = new TransactionScope())
				{
					context.BulkInsert(maplinks);

					context.SaveChanges();
					transactionScope.Complete();
				}
			}

			//using (var context = new IncZoneMapContext())
			//{
			//	//context.mapLinks.AddRange(maplinks);

			//	foreach (mapLink maplink in maplinks)
			//	{
			//		context.mapLinks.Add(maplink);
			//	}
			//	context.SaveChanges();
			//}

			//IncZoneMapContext context = null;
			//try
			//{
			//	context = new IncZoneMapContext();
			//	context.Configuration.AutoDetectChangesEnabled = false;

			//	int count = 0;
			//	foreach (var entityToInsert in maplinks)
			//	{
			//		++count;
			//		context = AddToContext(context, entityToInsert, count, 50, true);
			//	}

			//	context.SaveChanges();
			//}
			//finally
			//{
			//	if (context != null)
			//		context.Dispose();
			//}

			return HttpStatusCode.Created;
//			return CreatedAtRoute("DefaultApi", new { id = maplink.Id }, maplink);
		}

		//// POST api/MapNode
		//[ResponseType(typeof(mapNode))]
		//public HttpStatusCode PostmapNodeList(List<mapNode> mapnodes)
		//{
		//	if (!ModelState.IsValid)
		//	{
		//		return HttpStatusCode.BadRequest;
		//	}

		//	foreach (mapNode mapnode in mapnodes)
		//	{
		//		db.mapNodes.Add(mapnode);
		//		db.SaveChanges();
		//	}

		//	//return CreatedAtRoute("DefaultApi", new { id = mapnode.Id }, mapnode);
		//	return HttpStatusCode.Created;
		//}


        // DELETE api/MapLink/5
        [ResponseType(typeof(mapLink))]
		public IHttpActionResult DeletemapLink(Guid id)
        {


			using (var context = new IncZoneMapContext())
			{
				// delete existing records
				context.ObjectContext().ExecuteStoreCommand("DELETE FROM mapLinks WHERE mapSetId = {0}", id);
			}


			//using (var context = new IncZoneMapContext())
			//{
			//	//var result = db.mapNodes.ToList();
			//	var result = context.mapLinks.Where(s => s.mapSetId == id);

			//	context.mapLinks.RemoveRange(result);
			//	context.SaveChanges();
			//}


			///////////////
			//return Ok(maplink);
			//mapLink maplink = db.mapLinks.Find(id);
			//if (maplink == null)
			//{
			//	return NotFound();
			//}

			//db.mapLinks.Remove(maplink);
			//db.SaveChanges();

			return Ok();
		}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

		private bool mapLinkExists(Guid id)
        {
            return db.mapLinks.Count(e => e.Id == id) > 0;
        }

		private IncZoneMapContext AddToContext(IncZoneMapContext context, mapLink entity, int count, int commitCount, bool recreateContext)
		{
			context.Set<mapLink>().Add(entity);

			if (count % commitCount == 0)
			{
				context.SaveChanges();
				if (recreateContext)
				{
					context.Dispose();
					context = new IncZoneMapContext();
					context.Configuration.AutoDetectChangesEnabled = false;
				}
			}

			return context;
		}
    }
}