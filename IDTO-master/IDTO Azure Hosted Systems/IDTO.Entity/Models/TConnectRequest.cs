using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;


namespace IDTO.Entity.Models
{
    /// <summary>
    /// Entry created as a result of a trip that contains connections that matched an entry in the 
    /// TConnectOpportunity table.  Trips that have TConnects will be monitored.
    /// </summary>
    public class TConnectRequest : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        // Foreign key; required
        public int TConnectStatusId { get; set; }
        // Navigation properties
        public virtual TConnectStatus TConnectStatus { get; set; }
        /// <summary>
        ///  Foreign key; required.  Identifies the TConnect for this request.
        /// </summary>
        public int TConnectId { get; set; }
        /// <summary>
        ///  Navigation property for TConnect. 
        /// </summary>
        public  TConnect TConnect { get; set; }
        /// <summary>
        ///  Foreign key; required.Identifies the outbound tconnect vehicle linked to this request.
        /// </summary>
        public int TConnectedVehicleId { get; set; }
        /// <summary>
        ///Navigation property for TConnectedVehicle.
        /// </summary>
        public virtual TConnectedVehicle TConnectedVehicle { get; set; }
        /// <summary>
        /// Estimated time of arrival of the inbound vehicle.
         /// </summary>
        [Required]
        public DateTime EstimatedTimeArrival { get; set; }
        /// <summary>
        /// Number of minutes we are asking the departing bus to wait so that we'll make the bus.
        /// </summary>
        [Required]
        public int RequestedHoldMinutes { get; set; }



        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required]
        public string ModifiedBy { get; set; }

        //TConnectRequestID
     
    }
}
