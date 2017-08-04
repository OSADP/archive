// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace IDTO.iPhone
{
	[Register ("TripDetailsMapViewController")]
	partial class TripDetailsMapViewController
	{
		[Outlet]
		IDTO.iPhone.HorizontalScrollView horizScrollView { get; set; }

		[Outlet]
		MonoTouch.MapKit.MKMapView mapView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (horizScrollView != null) {
				horizScrollView.Dispose ();
				horizScrollView = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}
		}
	}
}
