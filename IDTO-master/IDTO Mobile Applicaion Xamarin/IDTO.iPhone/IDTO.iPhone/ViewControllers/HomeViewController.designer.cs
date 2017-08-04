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
	[Register ("HomeViewController")]
	partial class HomeViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnAccount { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnMoreTrips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnPlanTrip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnTripHistory { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView imgWeather { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNextTrip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblUpcomingTrips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableUpcomingTrips { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtDateHeader { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripAmPm { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripDuration { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripSubTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtNextTripTitle { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel txtOutsideTemp { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnAccount != null) {
				btnAccount.Dispose ();
				btnAccount = null;
			}

			if (btnPlanTrip != null) {
				btnPlanTrip.Dispose ();
				btnPlanTrip = null;
			}

			if (btnTripHistory != null) {
				btnTripHistory.Dispose ();
				btnTripHistory = null;
			}

			if (imgWeather != null) {
				imgWeather.Dispose ();
				imgWeather = null;
			}

			if (lblNextTrip != null) {
				lblNextTrip.Dispose ();
				lblNextTrip = null;
			}

			if (lblUpcomingTrips != null) {
				lblUpcomingTrips.Dispose ();
				lblUpcomingTrips = null;
			}

			if (tableUpcomingTrips != null) {
				tableUpcomingTrips.Dispose ();
				tableUpcomingTrips = null;
			}

			if (txtDateHeader != null) {
				txtDateHeader.Dispose ();
				txtDateHeader = null;
			}

			if (txtNextTripAmPm != null) {
				txtNextTripAmPm.Dispose ();
				txtNextTripAmPm = null;
			}

			if (txtNextTripDate != null) {
				txtNextTripDate.Dispose ();
				txtNextTripDate = null;
			}

			if (txtNextTripDuration != null) {
				txtNextTripDuration.Dispose ();
				txtNextTripDuration = null;
			}

			if (txtNextTripSubTitle != null) {
				txtNextTripSubTitle.Dispose ();
				txtNextTripSubTitle = null;
			}

			if (txtNextTripTime != null) {
				txtNextTripTime.Dispose ();
				txtNextTripTime = null;
			}

			if (txtNextTripTitle != null) {
				txtNextTripTitle.Dispose ();
				txtNextTripTitle = null;
			}

			if (txtOutsideTemp != null) {
				txtOutsideTemp.Dispose ();
				txtOutsideTemp = null;
			}

			if (btnMoreTrips != null) {
				btnMoreTrips.Dispose ();
				btnMoreTrips = null;
			}
		}
	}
}
