namespace IDTO.TravelerPortal.Common.Models
{
    using System.Collections.Generic;
    using System;
    
    /// <summary>
    /// Contains fields for loading a Trip to the database.
    /// </summary>

    public class Trip
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
        
        public List<Step> Steps { get; set; }

        public Trip()
        {
        }

        public int GetWalkTime_min()
        {
            int walkTime_Sec = 0;
            foreach (Step leg in Steps)
            {
                if (leg.ModeId == (int)ModeType.ModeId.WALK)
                {
                    walkTime_Sec += leg.Duration_sec();
                }
            }

            int ts_min = (int)Math.Round((double)((double)walkTime_Sec / 60.0));
            return ts_min;
        }

        public int Duration_min()
        {
            if (!TripEndDate.HasValue || !TripStartDate.HasValue)
                return 0;

            TimeSpan ts = (DateTime)TripEndDate - (DateTime)TripStartDate;
            int ts_min = (int)Math.Round(ts.TotalMinutes);
            return ts_min;
        }

        public int GetNumberOfTransfers()
        {
            if (Steps == null)
                return 0;

            int numTransfers = -1;
            foreach (Step leg in Steps)
            {
                if (leg.ModeId != (int)ModeType.ModeId.WALK)
                {
                    numTransfers++;
                }

            }

            if (numTransfers < 0)
                numTransfers = 0;

            return numTransfers;
        }

        public string GetFirstStepString()
        {
            if (Steps == null)
                return string.Empty;

            if (Steps.Count > 0)
            {
                Step firstStep = Steps[0];
                if ((firstStep.ModeId == (int)ModeType.ModeId.WALK) && (Steps.Count > 1))
                {
                    firstStep = Steps[1];
                }

                string stepString;
                if (firstStep.FromProviderId.HasValue)
                {
                    stepString = Providers.IdToString((int)firstStep.FromProviderId) + " " + firstStep.RouteNumber + " " + firstStep.FromName;

                }
                else
                {
                    stepString = firstStep.RouteNumber + " " + firstStep.FromName;
                }
                return stepString;
            }
            else
            {
                return "";
            }
        }
    }
}