using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// Probe Vehicle Data
    /// </summary>
    [DataContract]
    public class ProbeVehicleData
    {
        /// <summary>
        /// Positions
        /// </summary>
        [DataMember]
        public List<PositionSnapshot> Positions = new List<PositionSnapshot>();
        /// <summary>
        /// Inbound Vehicle string
        /// For CapTrans, this will be the name of the MDT (like MDT1, MDT2 etc).
        /// For Cabs, this will be like 1203,1204, etc.
        /// </summary>
        [DataMember]
        public string InboundVehicle;
        ///<summary>
        /// Wave
        /// </summary>
        [DataMember(IsRequired = false)]
        public String Wave;
    }
}