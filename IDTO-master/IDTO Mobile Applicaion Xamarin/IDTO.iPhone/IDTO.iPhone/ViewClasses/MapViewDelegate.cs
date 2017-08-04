using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;

namespace IDTO.iPhone
{
	public class MapViewDelegate : MKMapViewDelegate
	{
		public MapViewDelegate ()
		{
		}

		string pId = "PinAnnotation";
		static string annotationId = "ConferenceAnnotation";

		public override MKAnnotationView GetViewForAnnotation (MKMapView mapView, NSObject annotation)
		{
			if (annotation is MKUserLocation)
				return null; 
				
			if (annotation is MKStartPointAnnotation) {
				if (((MKStartPointAnnotation)annotation).Mode.ToLower ().Equals ("none")) {
					MKPinAnnotationView pinView = (MKPinAnnotationView)mapView.DequeueReusableAnnotation (pId);
					if (pinView == null)
						pinView = new MKPinAnnotationView (annotation, pId);
					pinView.PinColor = MKPinAnnotationColor.Green;

					pinView.CanShowCallout = true;

					return pinView;
				} else {
					MKAnnotationView annotationView = mapView.DequeueReusableAnnotation (annotationId);

					if (annotationView == null)
						annotationView = new MKAnnotationView (annotation, annotationId);

					if (((MKStartPointAnnotation)annotation).Mode.ToLower ().Equals ("walk"))
						annotationView.Image = new UIImage ("Walking_Icon_Small.png");
					else if(((MKStartPointAnnotation)annotation).Mode.ToLower ().Equals ("rail"))
						annotationView.Image = new UIImage ("Rail_Icon_Small.png");
					else
						annotationView.Image = new UIImage ("Bus_Icon_Small.png");

					annotationView.CanShowCallout = true;

					return annotationView;
				}

			} else if (annotation is MKEndPointAnnotation) {

				MKPinAnnotationView pinView = (MKPinAnnotationView)mapView.DequeueReusableAnnotation (pId);
				if (pinView == null)
					pinView = new MKPinAnnotationView (annotation, pId);
				pinView.PinColor = MKPinAnnotationColor.Red;

				pinView.CanShowCallout = true;

				return pinView;

			} else {
				MKPinAnnotationView pinView = (MKPinAnnotationView)mapView.DequeueReusableAnnotation (pId);
				if (pinView == null)
					pinView = new MKPinAnnotationView (annotation, pId);
				pinView.PinColor = MKPinAnnotationColor.Purple;

				pinView.CanShowCallout = true;

				return pinView;
			}


		}
			
		[Obsolete ("Since iOS 7 it is recommnended that you use GetRendererForOverlay")]
		public override MKOverlayView GetViewForOverlay (MKMapView mapView, NSObject overlay)
		{
			UIColor colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);
			UIColor colorIdtoRed = new UIColor ((float)(255.0 / 255.0), (float)(68.0 / 255.0), (float)(68.0 / 255.0), 1.0f);



			var ov = overlay as MKPolyline;
			var ovView = new MKPolylineView (ov);
			ovView.FillColor = colorIdtoRed;
			ovView.StrokeColor = colorIdtoRed;
			ovView.LineWidth = 5;
			return ovView;
		}
	}
}

