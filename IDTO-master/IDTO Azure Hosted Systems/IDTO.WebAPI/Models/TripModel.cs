using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using IDTO.Entity.Models;
namespace IDTO.WebAPI.Models
{
    /// <summary>
    /// Contains fields for loading a Trip to the database.
    /// </summary>
    [DataContract]
    public class TripModel
    {
        /// <summary>
        /// Unique identifier created by the database after trip is added.
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// Foreign Key to Traveler owning this trip.
        /// </summary>
        [DataMember]
        public int TravelerId { get; set; }
        /// <summary>
        /// Place where the trip starts.
        /// </summary>
        [DataMember]
        public string Origination { get; set; }
        /// <summary>
        /// Place where trip ends.
        /// </summary>
        [DataMember]
        public string Destination { get; set; }
        /// <summary>
        /// DateTime of start of trip.
        /// </summary>
        [DataMember]
        public Nullable<DateTime> TripStartDate { get; set; }
        /// <summary>
        /// DateTime of end of trip.
        /// </summary>
        [DataMember]
        public Nullable<DateTime> TripEndDate { get; set; }
        [DataMember]
        public Nullable<bool> MobilityFlag { get; set; }
        [DataMember]
        public Nullable<bool> BicycleFlag { get; set; }
        [DataMember]
        public string PriorityCode { get; set; }
        /// <summary>
        /// Steps comprising the trip
        /// </summary>
        [DataMember]
        public List<StepModel> Steps { get; set; }

        public TripModel()
        {
        }
        public TripModel(Trip t)
        {//could nuget/use AutoMapper.org? to map these
            Id = t.Id;
            TravelerId = t.TravelerId;
            Origination = t.Origination;
            Destination = t.Destination;
            TripStartDate = t.TripStartDate;
            TripEndDate = t.TripEndDate;
            MobilityFlag = t.MobilityFlag;
            BicycleFlag = t.BicycleFlag;
            PriorityCode = t.PriorityCode;
       
        }


    }
}