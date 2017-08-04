using System;

namespace IDTO.Common.Models
{
    public class TravelerLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime TimeStamp { get; set; }
		public string UserId { get; set; }
		public int TravelerId { get; set; }
    }
}