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
	[Register ("UpcomingTripsViewController")]
	partial class UpcomingTripsViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblUpcomingTrips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableViewUpcomingTrips { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblUpcomingTrips != null) {
				lblUpcomingTrips.Dispose ();
				lblUpcomingTrips = null;
			}

			if (tableViewUpcomingTrips != null) {
				tableViewUpcomingTrips.Dispose ();
				tableViewUpcomingTrips = null;
			}
		}
	}
}
