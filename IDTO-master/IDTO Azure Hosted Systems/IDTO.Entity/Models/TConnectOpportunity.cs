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
    /// Master list of TConnections. Used to determine if a trip contains a "special criteria"
    /// that makes it subject to generating tConnects.
    /// Eg. if one (bus) step ends at a special stop and the next (bus) step starts at a special
    /// stop, then a TConnect is created to monitor the timing between those stops.
    /// </summary>
    public class TConnectOpportunity : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        /// <summary>
        /// Indicates if the stop where the person is getting off the bus (the bus being monitored for its
        /// expected arrival) is CABS,COTA,etc.
        /// </summary>
        [Required]
        public int CheckpointProviderId { get; set; }
        /// <summary>
        /// Navigation property. Indicates if the stop where the person is getting off the bus (the bus being monitored for its
        /// expected arrival) is CABS,COTA,etc.
        /// </summary>
        public virtual Provider CheckpointProvider { get; set; }
        /// <summary>
        /// Indicates the stop code of the stop where the person is getting off the bus (the bus being monitored for its
        /// expected arrival) 
        /// </summary>
        [Required, MaxLength(20)]
        public string CheckpointStopCode { get; set; }
        /// <summary>
        /// Indicates the route of the stop where the person is getting off the bus (the bus being monitored for its
        /// expected arrival) 
        /// For the checkpoint, Route may have questionable purpose.
        /// </summary>
        [ MaxLength(100)]
        public string CheckpointRoute { get; set; }
        /// <summary>
        /// currently unused in logic.
        /// Indicates if the stop where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close) is CABS,COTA,etc.
        /// </summary>
        [Required]
        public int TConnectProviderId { get; set; }
        /// <summary>
        /// Navigation Property. 
        /// Indicates if the stop where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close) is CABS,COTA,etc.
        /// </summary>
        public virtual Provider TConnectProvider { get; set; }
        /// <summary>
        /// Indicates if stop code where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close) is CABS,COTA,etc.
        /// </summary>
           [Required, MaxLength(20)]
        public string TConnectStopCode { get; set; }
        /// <summary>
        /// Indicates if the route where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close).
        /// The Yellow route may
        /// have buses every 5 minutes so it would be pointless to ask them to wait, thus only the Green and
        /// Red routes would be added to the table.
        /// </summary>
         [MaxLength(100)]
        public string TConnectRoute { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required]
        public string ModifiedBy { get; set; }
       
    }
}
