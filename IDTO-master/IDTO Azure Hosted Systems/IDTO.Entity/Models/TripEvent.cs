using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;

namespace IDTO.Entity.Models
{
    public class TripEvent : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        // Foreign key; required
        public int TripId { get; set; }
        // Navigation properties
        public virtual Trip Trip { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        public string Message { get; set; }

        public TripEvent()
        {
            
        }
        public TripEvent(int tripId, string message)
        {
            this.TripId = tripId;
            this.Message = message;
            this.EventDate = DateTime.UtcNow;
        }
    }
}
