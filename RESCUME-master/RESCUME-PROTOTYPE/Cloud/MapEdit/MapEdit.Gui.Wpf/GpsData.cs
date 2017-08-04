using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEdit
{
	public class GpsData
	{
		public string Time { get; set; }
		public string Date { get; set; }
		public int FixQuality { get; set; }
		public double Hdop;
		public double Heading { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double Speed { get; set; }
		public double Pdop { get; set; }
		public double Elevation { get; set; }
		public long ReportedDate { get; set; }
		public int NumberOfSatellites { get; set; }
		public bool Valid { get; set; }
		public int Quality { get; set; }
		public int SatCount { get; set; }
		public double Altitude { get; set; }
		public string LastDgps { get; set; }
		public string DgpsStationId { get; set; }
		public int LaneOrder { get; set; }
		public int PostedSpeed { get; set; }
		public string LaneDirection { get; set; }
		public string LaneType { get; set; }

		public GpsData()
		{
			
		}

		public GpsData(GpsData data)
		{
			Time = data.Time;
			Date = data.Date;
			FixQuality = data.FixQuality;
			Hdop = data.Hdop;
			Heading = data.Heading;
			Latitude = data.Latitude;
			Longitude = data.Longitude;
			Speed = data.Speed;
			Pdop = data.Pdop;
			Elevation = data.Elevation;
			ReportedDate = data.ReportedDate;
			NumberOfSatellites = data.NumberOfSatellites;
			Valid = data.Valid;
			Quality = data.Quality;
			SatCount = data.SatCount;
			Altitude = data.Altitude;
			LastDgps = data.LastDgps;
			DgpsStationId = data.DgpsStationId;
			LaneOrder = data.LaneOrder;
			PostedSpeed = data.PostedSpeed;
			LaneDirection = data.LaneDirection;
			LaneType = data.LaneType;
		}

		public void CopyGpsData(GpsData source, ref GpsData destination)
		{
			destination.Time = source.Time;
			destination.Date = source.Date;
			destination.FixQuality = source.FixQuality;
			destination.Hdop = source.Hdop;
			destination.Heading = source.Heading;
			destination.Latitude = source.Latitude;
			destination.Longitude = source.Longitude;
			destination.Speed = source.Speed;
			destination.Pdop = source.Pdop;
			destination.Elevation = source.Elevation;
			destination.ReportedDate = source.ReportedDate;
			destination.NumberOfSatellites = source.NumberOfSatellites;
			destination.Valid = source.Valid;
			destination.Quality = source.Quality;
			destination.SatCount = source.SatCount;
			destination.Altitude = source.Altitude;
			destination.LastDgps = source.LastDgps;
			destination.DgpsStationId = source.DgpsStationId;
			destination.LaneOrder = source.LaneOrder;
			destination.PostedSpeed = source.PostedSpeed;
			destination.LaneDirection = source.LaneDirection;
			destination.LaneType = source.LaneType;
		}
	}
}
