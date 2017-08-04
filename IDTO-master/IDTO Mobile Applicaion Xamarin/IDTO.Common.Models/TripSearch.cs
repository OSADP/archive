namespace IDTO.Common.Models
{
    using System;
    public class TripSearch
    {
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public string StartLocation { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
        public string EndLocation { get; set; }
        public bool SearchByArriveByTime { get; set; }
        public DateTime Time { get; set; }
        public bool NeedWheelchairAccess { get; set; }
        public double MaxWalkMeters { get; set; }


		public string GetStartLocationString(){
			if (String.IsNullOrEmpty (StartLocation))
				return StartLatitude.ToString () + "," + StartLongitude.ToString ();
			else
				return StartLocation;
		}

		public string GetEndLocationString(){
			if (String.IsNullOrEmpty (EndLocation))
				return EndLatitude.ToString () + "," + EndLongitude.ToString ();
			else
				return EndLocation;
		}

    }
}
