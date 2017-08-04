using Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDTO.Entity.Models
{
    public class Block : EntityBase
    {
        // Primary Key
        public string Id { get; set; }

        [Required]
        public string Agency { get; set; }

        public string ServiceGroupId { get; set; }

        [Required]
        public string BlockName { get; set; }
    }
}
