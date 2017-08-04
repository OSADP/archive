using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using IDTO.Entity.Models;
namespace IDTO.WebAPI.Models
{
    [DataContract]
    public class StepModel
    {
        /// <summary>
        /// Unique identifier created by the database after step is added.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Database Id of the trip this step belongs to. Will be set by the back-end for new entries.
        /// </summary>
        [DataMember]
        public int? TripId { get; set; }

        /// <summary>
        /// Id for mode of transportation used for this step.1 Walk,2 Bus
        /// </summary>
        [DataMember]
        public int? ModeId { get; set; }

        /// <summary>
        /// DateTime the step starts.
        /// </summary>
        [DataMember]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// DateTime the step ends.
        /// </summary>
        [DataMember]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Name of location where the step starts.
        /// </summary>
        [DataMember]
        public string FromName { get; set; }

        /// <summary>
        /// Optional. Code for provider bus stop where the step starts.
        /// </summary>
        [DataMember]
        public string FromStopCode { get; set; }

        /// <summary>
        /// Optional.  Database Id for the type of provider invovled in start of the step.
        /// </summary>
        [DataMember]
        public int? FromProviderId { get; set; }

        /// <summary>
        /// Name of location where the step ends.
        /// </summary>
        [DataMember]
        public string ToName { get; set; }

        /// <summary>
        /// Optional. Code for provider bus stop where the step ends.
        /// </summary>
        [DataMember]
        public string ToStopCode { get; set; }

        /// <summary>
        /// Optional.  Database Id for the type of provider invovled in end of the step.
        /// </summary>
        [DataMember]
        public int? ToProviderId { get; set; }

        /// <summary>
        /// Distance travelled during the step.
        /// </summary>
        [DataMember]
        public decimal? Distance { get; set; }

        [DataMember]
        public string RouteNumber { get; set; }

        [DataMember]
        public string BlockIdentifier { get; set; }

        [DataMember]
        public BlockModel Block { get; set; }

        [DataMember]
        public string EncodedMapString { get; set; }


        public StepModel() { }

        public StepModel(Step s)
        {
            this.Id = s.Id;
            this.TripId = s.TripId;
            this.ModeId = s.ModeId;
            this.StartDate = s.StartDate;
            this.EndDate = s.EndDate;
            this.FromName = s.FromName;
            this.FromStopCode = s.FromStopCode;
            this.FromProviderId = s.FromProviderId;
            this.ToName = s.ToName;
            this.EncodedMapString = s.EncodedMapString;
            this.ToStopCode = s.ToStopCode;
            this.ToProviderId = s.ToProviderId;
            this.Distance = s.Distance;
            this.RouteNumber = s.RouteNumber;
            this.BlockIdentifier = s.BlockIdentifier;
            if (s.Block != null)
                this.Block = new BlockModel(s.Block);
        }
    }
}