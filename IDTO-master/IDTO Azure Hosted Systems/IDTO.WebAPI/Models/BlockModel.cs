using IDTO.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace IDTO.WebAPI.Models
{
    [DataContract]
    public class BlockModel
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Agency { get; set; }
        [DataMember]
        public string ServiceGroupId { get; set; }
        [DataMember]
        public string BlockName { get; set; }

        public BlockModel()
        {

        }

        public BlockModel(Block block)
        {
            this.Id = block.Id;
            this.Agency = block.Agency;
            this.ServiceGroupId = block.ServiceGroupId;
            this.BlockName = block.BlockName;
        }
    }
}