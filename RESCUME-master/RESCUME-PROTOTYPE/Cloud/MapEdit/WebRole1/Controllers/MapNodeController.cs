using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MapEdit.Data.Models;
using System.Data.Entity.Core.Objects;
using EntityFramework.BulkInsert.Extensions;
using System.Transactions;

namespace WebRole1.Controllers
{
    public class MapNodeController : ApiController
    {
        private IncZoneMapContext db = new IncZoneMapContext();

        //GET api/MapNode
		public IQueryable<mapNode> GetmapNodesQueriable(Guid id)
		{
			using (var context = new IncZoneMapContext())
			{
				//context.ContextOptions.ProxyCreationEnabled = false;

				context.ObjectContext().ContextOptions.ProxyCreationEnabled = false;

				return context.mapNodes.Where(s => s.mapSetId == id);
			}
		}
		public List<mapNode> GetmapNodes(Guid id)
		{
			using (var context = new IncZoneMapContext())
			{
				//var result = db.mapNodes.ToList();
                //var result = context.mapNodes.Where(s => s.mapSetId == id).ToList();
				var result = db.mapNodes.Where(s => s.mapSetId == id).ToList();
				return result;
				//	//context.ContextOptions.ProxyCreationEnabled = false;

				//	context.ObjectContext().ContextOptions.ProxyCreationEnabled = false;

				//	return context.mapNodes.Where(s => s.mapSetId == id);
			}
		}
		// GET api/MapNode/5
        [ResponseType(typeof(mapNode))]
		public IHttpActionResult GetmapNode(Guid id)
        {
            mapNode mapnode = db.mapNodes.Find(id);
            if (mapnode == null)
            {
                return NotFound();
            }

            return Ok(mapnode);
        }

        // PUT api/MapNode/5
		public IHttpActionResult PutmapNode(Guid id, mapNode mapnode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mapnode.Id)
            {
                return BadRequest();
            }

            db.Entry(mapnode).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!mapNodeExists(id))
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

		// POST api/MapNode
		[ResponseType(typeof(mapNode))]
		public IHttpActionResult PostmapNode(mapNode mapnode)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			using (var context = new IncZoneMapContext())
			{
				context.mapNodes.Add(mapnode);
				context.SaveChanges();
			}
			return CreatedAtRoute("DefaultApi", new { id = mapnode.Id }, mapnode);
		}

		// POST api/MapNode
		[ResponseType(typeof(mapNode))]
		public HttpStatusCode PostmapNodeList(List<mapNode> mapnodes)
		{
			if (!ModelState.IsValid)
			{
				return HttpStatusCode.BadRequest;
			}


			using (var context = new IncZoneMapContext())
			{
				using (var transactionScope = new TransactionScope())
				{
					context.BulkInsert(mapnodes);

					context.SaveChanges();
					transactionScope.Complete();
				}
			}

			//using (var context = new IncZoneMapContext())
			//{
			//	//context.mapNodes.AddRange(mapnodes);
			//	context.Configuration.AutoDetectChangesEnabled = false;
			//	context.Configuration.ValidateOnSaveEnabled= false;
			//	foreach (mapNode mapnode in mapnodes)
			//	{
			//		context.mapNodes.Add(mapnode);
			//		context.SaveChanges();
			//	}
			//}

			//IncZoneMapContext context = null;
			//try
			//{
			//	context = new IncZoneMapContext();
			//	context.Configuration.AutoDetectChangesEnabled = false;

			//	int count = 0;
			//	foreach (var entityToInsert in mapnodes)
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
		}

        // DELETE api/MapNode/5
        [ResponseType(typeof(mapNode))]
		public IHttpActionResult DeletemapNode(Guid id)
        {
			using (var context = new IncZoneMapContext())
			{
				// delete existing records
				context.ObjectContext().ExecuteStoreCommand("DELETE FROM mapNodes WHERE mapSetId = {0}", id);
			}

			//using (var context = new IncZoneMapContext())
			//{
			//	//var result = db.mapNodes.ToList();
			//	var result = context.mapNodes.Where(s => s.mapSetId == id).ToList();

			//	context.mapNodes.RemoveRange(result);
			//	context.SaveChanges();
			//}

			//mapNode mapnode = db.mapNodes.Find(id);
			//if (mapnode == null)
			//{
			//	return NotFound();
			//}

			//db.mapNodes.Remove(mapnode);
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

		private bool mapNodeExists(Guid id)
        {
            return db.mapNodes.Count(e => e.Id == id) > 0;
        }

		private IncZoneMapContext AddToContext(IncZoneMapContext context,
		mapNode entity, int count, int commitCount, bool recreateContext)
		{
			context.Set<mapNode>().Add(entity);

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