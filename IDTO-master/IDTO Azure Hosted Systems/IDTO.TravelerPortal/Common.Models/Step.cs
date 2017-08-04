using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using IDTO.TravelerPortal.Models;

namespace IDTO.TravelerPortal.Common.Models
{

    public class Step
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

        public DateTime StartDate { get; set; }
        /// <summary>
        /// DateTime the step ends.
        /// </summary>

        public DateTime EndDate { get; set; }
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

        public string EncodedMapString { get; set; }

        public List<CoordinateEntity> googlePoints { get; set; }

        public Step() { }

        public string GetFromName()
        {
            if (FromName.Length < 4)
                return FromName;

            if (FromName.Substring(0, 4).Equals("way "))
            {
                return "Unnamed Road";
            }
            else
            {
                return FromName;
            }
        }

        public string GetToName()
        {
            if (ToName.Length < 4)
                return ToName;

            if (ToName.Substring(0, 4).Equals("way "))
            {
                return "Unnamed Road";
            }
            else
            {
                return ToName;
            }
        }

        public int Duration_sec()
        {
            TimeSpan ts = EndDate - StartDate;
            int ts_min = (int)Math.Round(ts.TotalSeconds);
            return ts_min;
        }

        public int Duration_min()
        {
            TimeSpan ts = EndDate - StartDate;
            int ts_min = (int)Math.Round(ts.TotalMinutes);
            return ts_min;
        }


    }
}