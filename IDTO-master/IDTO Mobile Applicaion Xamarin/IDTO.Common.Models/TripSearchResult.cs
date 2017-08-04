namespace IDTO.Common.Models
{
    using System.Collections.Generic;
    using System;

	using IDTO.Common;
    
    public class StopId
    {
        public string agencyId { get; set; }
        public string id { get; set; }
    }

    public class From
    {
        public string name { get; set; }
        public StopId stopId { get; set; }
        public string stopCode { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
		public string arrival { get; set; }
		public string departure { get; set; }
        public string orig { get; set; }
		public string zoneId { get; set; }
        public int? stopIndex { get; set; }
    }

    public class StopId2
    {
        public string agencyId { get; set; }
        public string id { get; set; }
    }

    public class To
    {
        public string name { get; set; }
        public StopId2 stopId { get; set; }
        public string stopCode { get; set; }
        public double lon { get; set; }
        public double lat { get; set; }
		public string arrival { get; set; }
		public string departure { get; set; }
        public string orig { get; set; }
		public string zoneId { get; set; }
        public int stopIndex { get; set; }
    }

    public class Leg
    {
        public long startTime { get; set; }
        public long endTime { get; set; }
        public double distance { get; set; }
        public string mode { get; set; }
        public string route { get; set; }
        public string agencyName { get; set; }
        public string routeColor { get; set; }
        public int routeType { get; set; }
        public string routeId { get; set; }
        public string routeTextColor { get; set; }
        public string tripBlockId { get; set; }
        public string headsign { get; set; }
        public string agencyId { get; set; }
        public string tripId { get; set; }
		public From from { get; set; }
		public To to { get; set; }
        public string routeShortName { get; set; }
        public string routeLongName { get; set; }
        public int duration { get; set; }
        public EncodedPolylineBean legGeometry { get; set; }
        public List<CoordinateEntity> googlePoints { get; set; }
		public int GetDuration_min()
		{
			float dur_min = (float)(duration) / 60000.0f;
			int iduration_min = (int)Math.Round (dur_min);

			return iduration_min;
		}

		public DateTime GetStartDate()
		{
			return startTime.ToDateTimeUTC();
		}

		public DateTime GetEndDate()
		{
			return endTime.ToDateTimeUTC();
		}
    }

    public class Itinerary
    {
        public int duration { get; set; }
		public long startTime { get; set; }
        public long endTime { get; set; }
        public int walkTime { get; set; }
        public int transitTime { get; set; }
        public int waitingTime { get; set; }
        public int walkDistance { get; set; }
		public List<Leg> legs { get; set; }

		public string GetFirstAgencyName()
		{
			string agencyName = "";

			foreach (Leg leg in legs) {
				string agency = leg.agencyId;
				if (!string.IsNullOrEmpty (agency)) {
					if (string.IsNullOrEmpty (agencyName))
						agencyName += agency + " " + leg.routeShortName;
				}

			}

			return agencyName;
		}

		public int GetNumberOfTransfers()
		{
			int numTransfers = -1;
			foreach (Leg leg in legs) {
				if (!leg.mode.ToLower ().Equals ("walk")) {
					numTransfers++;
				}

			}

			if (numTransfers < 0)
				numTransfers = 0;

			return numTransfers;
		}

		public DateTime GetStartDate()
		{
			DateTime dtStart = startTime.ToDateTimeUTC ();
			return dtStart;
		}

		public int GetWalkTime_min()
		{
			float time_min = (float)(walkTime) / 60.0f;
			int itime_min = (int)Math.Round (time_min);

			return itime_min;
		}

		public int GetDuration_min()
		{
			float dur_min = (float)(duration) / 60000.0f;
			int iduration_min = (int)Math.Round (dur_min);

			return iduration_min;
		}
    }

    public class TripSearchResult
    {
        public long date { get; set; }
		public List<Itinerary> itineraries { get; set; }
		public TripSearch searchCriteria { get; set; }
    }
}
