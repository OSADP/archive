using System;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.Serialization;

namespace IDTO.Common.Models
{

    public class StepModel
    {
        /// <summary>
        /// Unique identifier created by the database after step is added.
        /// </summary>

        public int Id { get; set; }
        /// <summary>
        /// Database Id of the trip this step belongs to. Will be set by the back-end for new entries.
        /// </summary>

        public int? TripId { get; set; }
        /// <summary>
        /// Id for mode of transportation used for this step.1 Walk,2 Bus
        /// </summary>

        public int? ModeId { get; set; }
        /// <summary>
        /// DateTime the step starts.
        /// </summary>

        public DateTime? StartDate { get; set; }
        /// <summary>
        /// DateTime the step ends.
        /// </summary>

        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Name of location where the step starts.
        /// </summary>

        public string FromName { get; set; }
        /// <summary>
        /// Optional. Code for provider bus stop where the step starts.
        /// </summary>

        public string FromStopCode { get; set; }
        /// <summary>
        /// Optional.  Database Id for the type of provider invovled in start of the step.
        /// </summary>

        public int? FromProviderId { get; set; }
        /// <summary>
        /// Name of location where the step ends.
        /// </summary>

        public string ToName { get; set; }
       /// <summary>
       /// Optional. Code for provider bus stop where the step ends.
       /// </summary>

        public string ToStopCode { get; set; }
        /// <summary>
        /// Optional.  Database Id for the type of provider invovled in end of the step.
        /// </summary>

        public int? ToProviderId { get; set; }
        /// <summary>
        /// Distance travelled during the step.
        /// </summary>

        public decimal? Distance { get; set; }
        //[DataMember]
        //public int? StepNumber { get; set; }

        public string RouteNumber { get; set; }

        public string BlockIdentifier { get; set; }

        public StepModel() { }

    }
}