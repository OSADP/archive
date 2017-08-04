using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;

namespace IDTO.Entity.Models
{
    public class Dispatcher : EntityBase
    {
        //primary key
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public int ProviderId { get; set; }
        [Required]//TODO: MVC checks 'Required' too somehow and the user would not enter this, will this mess up UI?
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }

    }
}
