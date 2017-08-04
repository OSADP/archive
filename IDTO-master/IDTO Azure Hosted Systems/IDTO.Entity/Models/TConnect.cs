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
    public class TConnect : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        // Foreign key; required
        public int TConnectStatusId { get; set; }
        // Navigation properties
        public virtual TConnectStatus TConnectStatus { get; set; }
        /// <summary>
        ///  Foreign key; required.  Identifies the Trip's Step that correlates to the CheckPoint part of the
        ///  TConnectOpportunity. This is the step we are monitoring to see if it is falling behind.
        /// </summary>
        public int InboundStepId { get; set; }
        /// <summary>
        ///  Navigation property for InboundStepId. 
        /// </summary>
        public virtual Step InboundStep { get; set; }
        /// <summary>
        ///  Foreign key; required.Identifies the Trip's Step that correlates to the TConnect part of the
        ///  TConnectOpportunity. This is the step we are keeping track of its estimated departure to decide
        ///  if we should issue a TConnectRequest to ask it to wait.
        /// </summary>
        public int OutboundStepId { get; set; }
        /// <summary>
        ///Navigation property for OutboundStepId.
        /// </summary>
        public virtual Step OutboundStep { get; set; }
        /// <summary>
        /// Updated in the Monitor. Null for CABS and Cota.  Need to know which bus to monitor. Will know what vehicle for CapTrans.
        /// </summary>
        public string InboundVehicle { get; set; }
        /// <summary>
        /// Updated in the Monitor.
        /// </summary>
        //public string VehicleProviderId { get; set; }
        /// <summary>
        /// Start of time slot that this row should be monitored. Updated in the Monitor.
        /// </summary>
        public DateTime? StartWindow { get; set; }
        /// <summary>
        /// End of time slot that this row should be monitored. Updated in the Monitor.
        /// </summary>
        public DateTime? EndWindow { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required]
        public string ModifiedBy { get; set; }

        public DateTime? SurveyDate { get; set; }
        //TConnectRequestID
     
    }
}
