using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maps.MapControl.WPF;

namespace MapEdit
{
	public class Track
	{
		public string Name { get; set; }		
		public MapPolyline Polyline;
		public List<Pushpin> Pins;
		public Pushpin FirstPushpin;
		public Pushpin LastPushpin;

		public Track()
		{
			Pins = new List<Pushpin>();
			Polyline = new MapPolyline();
 		}
	}
}
