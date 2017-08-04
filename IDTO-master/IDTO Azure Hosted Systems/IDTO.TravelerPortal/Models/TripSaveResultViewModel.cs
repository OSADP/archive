using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDTO.TravelerPortal.Common.Models;
using IDTO.TravelerPortal.Common.ExtensionMethods;
using IDTO.TravelerPortal.Common;

namespace IDTO.TravelerPortal.Models
{
    public class TripSaveResultViewModel
    {
        public int TravelerId { get; set; }

        public string Origination { get; set; }

        public string Destination { get; set; }

        public DateTime StartTime { get; set; }

        public int Duration { get; set; }

        public int Transfers { get; set; }

        public string StartFirstNonWalk { get; set; }

        public string StartFrom { get; set; }

        public string EndAt { get; set; }

        public Nullable<DateTime> TripStartDate { get; set; }

        public Nullable<DateTime> TripEndDate { get; set; }

        public Nullable<bool> MobilityFlag { get; set; }

        public Nullable<bool> BicycleFlag { get; set; }

        public string PriorityCode { get; set; }

        public TripSaveResultViewModel()
        {
        }

        public TripSaveResultViewModel(Trip trip)
        {
             this.TravelerId = trip.TravelerId;

             this.Origination = trip.Origination;

             this.Destination = trip.Destination;

             this.TripStartDate = trip.TripStartDate;

             this.TripEndDate = trip.TripEndDate;

             this.MobilityFlag = trip.MobilityFlag;

             this.BicycleFlag = trip.BicycleFlag;

             this.PriorityCode = trip.PriorityCode;

             this.StartTime = TimeZoneInfo.ConvertTimeFromUtc(trip.TripStartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
             this.Duration = trip.Duration_min();
             this.StartFirstNonWalk = string.Empty;
             this.StartFrom = string.Empty;
             this.EndAt = string.Empty;

             var steps = trip.Steps.OrderBy(l => l.StartDate);

             // Count how many back-to-back bus legs there are and that is how 
             // many transfers there are.
             int numTransfers = 0;
             bool prevIsBus = false;
             foreach (var step in steps)
             {
                 if (string.Compare("bus", ModeType.IdToString((int)step.ModeId), true) == 0)
                 {
                     if (prevIsBus)
                     {
                         numTransfers++;
                     }
                     else
                     {
                         prevIsBus = true;
                     }
                 }
                 else
                 {
                     prevIsBus = false;
                 }
             }
             
            this.Transfers = numTransfers;

             if (steps.Count() > 0)
             {
                 this.StartFrom = steps.First().FromName;
                 this.EndAt = steps.Last().ToName;
                 foreach (var step in steps)
                 {
                     if (string.Compare("walk", ModeType.IdToString((int)step.ModeId), true) != 0)
                     {
                         this.StartFirstNonWalk = step.FromName;
                         break;
                     }
                 }
             }


        }


    }
}