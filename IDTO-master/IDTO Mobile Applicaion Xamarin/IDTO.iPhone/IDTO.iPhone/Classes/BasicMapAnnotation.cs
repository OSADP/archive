using System;
using MonoTouch.MapKit;
using MonoTouch.CoreLocation;

namespace IDTO.iPhone
{
	class BasicMapAnnotation : MKAnnotation{
		public override CLLocationCoordinate2D Coordinate {get;set;}
		string title, subtitle;
		public override string Title { get{ return title; }}
		public override string Subtitle { get{ return subtitle; }}
		public BasicMapAnnotation (CLLocationCoordinate2D coordinate, string title, string subtitle) {
			this.Coordinate = coordinate;
			this.title = title;
			this.subtitle = subtitle;
		}
	}
}

