using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.DispatcherPortal.Models
{
    public class TConnVehicleViewModel
    {
        //VehicleID
        public int Id { get; set; }
        /// <summary>
        /// The time this bus was supposed to leave.
        /// </summary>
  
        public DateTime OriginallyScheduledDeparture { get; set; }


        /// <summary>
        /// Indicates if stop code where the person is trying to catch the next bus (the bus that might be asked
        /// to be held if it is close) is CABS,COTA,etc.
        /// </summary>
 
        public string TConnectStopCode { get; set; }

        public string TConnectRoute { get; set; }

        /// <summary>
        /// Minutes that the bus has agreed to wait past the originally scheduled departure.
        /// </summary>

        public int CurrentAcceptedHoldMinutes { get; set; }

        public int NumberRequests { get; set; }

    }
}