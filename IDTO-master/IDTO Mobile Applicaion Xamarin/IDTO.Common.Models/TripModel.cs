namespace IDTO.Common.Models
{
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Contains fields for loading a Trip to the database.
    /// </summary>

    public class TripModel
    {
        /// <summary>
        /// Unique identifier created by the database after trip is added.
        /// </summary>
        
        public int Id { get; set; }
        /// <summary>
        /// Foreign Key to Traveler owning this trip.
        /// </summary>
        
        public int TravelerId { get; set; }
        /// <summary>
        /// Place where the trip starts.
        /// </summary>
        
        public string Origination { get; set; }
        /// <summary>
        /// Place where trip ends.
        /// </summary>
        
        public string Destination { get; set; }
        /// <summary>
        /// DateTime of start of trip.
        /// </summary>
        
        public Nullable<DateTime> TripStartDate { get; set; }
        /// <summary>
        /// DateTime of end of trip.
        /// </summary>
        
        public Nullable<DateTime> TripEndDate { get; set; }
        
        public Nullable<bool> MobilityFlag { get; set; }
        
        public Nullable<bool> BicycleFlag { get; set; }
        
        public string PriorityCode { get; set; }
        /// <summary>
        /// Steps comprising the trip
        /// </summary>
        
        public List<StepModel> Steps { get; set; }

        public TripModel()
        {
        }
    }
}