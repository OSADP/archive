namespace IDTO.Common.Models
{
    using System.Collections.Generic;
    using System;
    public class Step
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int ModeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string FromName { get; set; }
        public string FromStopCode { get; set; }
        public int? FromProviderId { get; set; }
        public string ToName { get; set; }
        public string ToStopCode { get; set; }
        public int? ToProviderId { get; set; }
        public decimal Distance { get; set; }
        public string RouteNumber { get; set; }

        public string BlockIdentifier { get; set; }

		public string EncodedMapString { get; set; }

		public List<CoordinateEntity> googlePoints { get; set; }


		public int Duration_sec()
		{
			TimeSpan ts = EndDate - StartDate;
			int ts_min = (int)Math.Round (ts.TotalSeconds);
			return ts_min;
		}

		public int Duration_min()
		{
			TimeSpan ts = EndDate - StartDate;
			int ts_min = (int)Math.Round (ts.TotalMinutes);
			return ts_min;
		}
    }

    public class Trip
    {
        public int Id { get; set; }
        public int TravelerId { get; set; }
        public string Origination { get; set; }
        public string Destination { get; set; }
        public DateTime TripStartDate { get; set; }
        public DateTime TripEndDate { get; set; }
        public bool MobilityFlag { get; set; }
        public bool BicycleFlag { get; set; }
        public string PriorityCode { get; set; }
        public List<Step> Steps { get; set; }

		public int GetNumberOfTransfers()
		{
			int numTransfers = -1;
			foreach (Step leg in Steps) {
				if (leg.ModeId!= (int)ModeType.ModeId.WALK) {
					numTransfers++;
				}

			}

			if (numTransfers < 0)
				numTransfers = 0;

			return numTransfers;
		}

		public int GetWalkTime_min()
		{
			int walkTime_Sec = 0;
			foreach (Step leg in Steps) {
				if (leg.ModeId== (int)ModeType.ModeId.WALK) {
					walkTime_Sec += leg.Duration_sec();
				}
			}

			int ts_min = (int)Math.Round ((double)((double)walkTime_Sec/60.0));
			return ts_min;
		}

		public int Duration_min()
		{
			TimeSpan ts = TripEndDate - TripStartDate;
			int ts_min = (int)Math.Round (ts.TotalMinutes);
			return ts_min;
		}

		public string GetFirstStepString()
		{
			if (Steps !=null && Steps.Count > 0) {
				Step firstStep = Steps [0];
				if (firstStep.ModeId == (int)ModeType.ModeId.WALK) {
					firstStep = Steps [1];
				}

				string stepString = "";
				if(firstStep.FromProviderId!=null)
					stepString = Providers.IdToString(firstStep.FromProviderId.Value) + " " + firstStep.FromName;

				return stepString;
			} else {
				return "";
			}
		}
    }

}
