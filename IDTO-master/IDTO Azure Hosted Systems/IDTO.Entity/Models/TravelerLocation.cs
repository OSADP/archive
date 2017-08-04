using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;

namespace IDTO.Entity.Models
{
    public class TravelerLocation : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        [Required]
        // Foreign key; required
        public int TravelerId { get; set; }
        // Navigation properties
        public virtual Traveler Traveler { get; set; }
        [Required]
        public DateTime PositionTimestamp { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}
