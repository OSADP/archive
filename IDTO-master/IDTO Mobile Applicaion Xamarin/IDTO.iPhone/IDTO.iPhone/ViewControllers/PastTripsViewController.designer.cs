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
	[Register ("PastTripsViewController")]
	partial class PastTripsViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblPastTrips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView pastTripsTableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblPastTrips != null) {
				lblPastTrips.Dispose ();
				lblPastTrips = null;
			}

			if (pastTripsTableView != null) {
				pastTripsTableView.Dispose ();
				pastTripsTableView = null;
			}
		}
	}
}
