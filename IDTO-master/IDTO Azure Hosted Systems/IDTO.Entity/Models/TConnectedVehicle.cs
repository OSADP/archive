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
    /// <summary>
    /// Entry created as a result of a 
    /// </summary>
    public class TConnectedVehicle : EntityBase
    {
        //Primary Key
        public int Id { get; set; }


        /// <summary>
        /// The time this bus was supposed to leave.
        /// </summary>
        [Required]
        public DateTime OriginallyScheduledDeparture { get; set; }

        /// <summary>
        /// Minutes that the bus has agreed to wait past the originally scheduled departure.
        /// </summary>
        [Required]
        public int CurrentAcceptedHoldMinutes { get; set; }

        /// <summary>
        /// Indicates if stop code where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close) is CABS,COTA,etc.
        /// </summary>
        [Required, MaxLength(20)]
        public string TConnectStopCode { get; set; }
        [MaxLength(100)]
        public string TConnectRoute { get; set; }
        [Required, MaxLength(50)]
        public string TConnectFromName { get; set; }
        [MaxLength(50)]
        public string TConnectBlockIdentifier { get; set; }
        [ForeignKey("TConnectBlockIdentifier")]
        public virtual Block Block { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required, MaxLength(20)]
        public string ModifiedBy { get; set; }

        //TConnectRequest
        List<TConnectRequest> TConnectRequests { get; set; }

    }
}
