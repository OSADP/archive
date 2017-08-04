using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDTO.Entity.Models
{
    public class Step : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        // Foreign key; required
        public int TripId { get; set; }
        // Navigation property
        public virtual Trip Trip { get; set; }
        // Foreign key; required
        public int ModeId { get; set; }
        // Navigation properties
        public virtual Mode Mode { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        [Required, MaxLength(50)]
        public string FromName { get; set; }

        public string FromStopCode { get; set; }
        //Foreign Key
        public int? FromProviderId { get; set; }
        // Navigation property
        public virtual Provider FromProvider { get; set; }

        [Required, MaxLength(50)]
        public string ToName { get; set; }

        public string ToStopCode { get; set; }
        //Foreign Key
        public int? ToProviderId { get; set; }
        // Navigation property
        public virtual Provider ToProvider { get; set; }
        [Required]
        public decimal Distance { get; set; }
        [Required]
        public int StepNumber { get; set; }
        [MaxLength(100)]
        public string RouteNumber { get; set; }

        //[MaxLength(50)]
        
        public string BlockIdentifier { get; set; }

        [ForeignKey("BlockIdentifier")]
        public virtual Block Block { get; set; }

        public string EncodedMapString { get; set; }

    }
}
