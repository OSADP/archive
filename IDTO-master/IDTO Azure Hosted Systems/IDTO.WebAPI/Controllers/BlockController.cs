using IDTO.Entity.Models;
using IDTO.WebAPI.Models;
using Repository.Providers.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace IDTO.WebAPI.Controllers
{
    public class BlockController  : BaseController
    {

        public BlockController(IDbContext context)
            : base(context)
        {  }

        /// <summary>
        /// Get all Blocks 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BlockModel> GetBlocks()
        {
            var b = Uow.Repository<Block>().Query().Get().ToList();
            return b.Select(block => new BlockModel(block)).ToList();
        }

        /// <summary>
        /// Get Block by ID.
        /// GET api/Block/5
        /// </summary>
        /// <param name="id">The primary key id of the block</param>
        /// <returns>A BlockModel</returns>
        [ResponseType(typeof(BlockModel))]
        public IHttpActionResult GetBlock(int id)
        {
            Block block = Uow.Repository<Block>().Find(id);
            if (block == null)
            {
                return NotFound();
            }

            return Ok(new BlockModel(block));
        }

        /// <summary>
        /// Get Block by BlockID.
        /// GET api/Block  load parameter by name 'blockID'
        /// </summary>
        /// <param name="blockID">The block id</param>
        /// <returns>BlockModel</returns>
        [ResponseType(typeof(BlockModel))]
        public IHttpActionResult GetBlockById(string blockID)
        {
            var block = Uow.Repository<Block>().Query().Filter(t => t.Id.Equals(blockID)).Get();
            if (!block.Any())
            {
                return NotFound();
            }

            return Ok(new BlockModel(block.First()));
        }
    }
}
