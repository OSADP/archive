using System;

using MonoTouch.UIKit;

using MonoTouch.MapKit;

namespace IDTO.iPhone
{
	public class MapDelegate : MKMapViewDelegate
	{
		public MapDelegate ()
		{

			
		}

		public override MKOverlayRenderer OverlayRenderer (MKMapView mapView, IMKOverlay overlay)
		{
			if (overlay is MKPolyline) {
				var route = (MKPolyline)overlay;
				var renderer = new MKPolylineRenderer (route);
				renderer.StrokeColor = UIColor.Blue;
				renderer.LineWidth = 1.5f;

				return renderer;
			}

			return null;
		}
	}
}

