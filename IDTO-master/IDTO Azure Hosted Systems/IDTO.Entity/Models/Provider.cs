using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;

namespace IDTO.Entity.Models
{
    public class Provider : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        // Foreign key; required
        public int ProviderTypeId { get; set; }
        // Navigation properties
        public virtual ProviderType ProviderType { get; set; }
    }
}
