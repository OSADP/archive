using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;

namespace IDTO.Entity.Models
{
    public class Trip : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        // Foreign key; required
        public int TravelerId { get; set; }
        // Navigation properties
        public virtual Traveler Traveler { get; set; }
        [Required]
        public string Origination { get; set; }
         [Required]
        public string Destination { get; set; }
         [Required]
        public DateTime TripStartDate { get; set; }
         [Required]
         public DateTime TripEndDate { get; set; }

         [Required]
        public bool MobilityFlag { get; set; }
         [Required]
        public bool BicycleFlag { get; set; }
         [Required]
        public string PriorityCode { get; set; }
         public bool TripStartNotificationSent { get; set; }
         [Required]
        public DateTime CreatedDate { get; set; }
         [Required]
        public DateTime ModifiedDate { get; set; }
       
        public string ModifiedBy { get; set; }
    }
}
