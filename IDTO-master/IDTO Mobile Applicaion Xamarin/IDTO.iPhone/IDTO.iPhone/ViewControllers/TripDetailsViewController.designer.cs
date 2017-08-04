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
	[Register ("TripDetailsViewController")]
	partial class TripDetailsViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnSaveCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNumberTransfers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTotalTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTotalWalkTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTravelTime { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblTripDetails { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableViewSteps { get; set; }

		[Action ("saveCancelAction:")]
		partial void saveCancelAction (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (lblTravelTime != null) {
				lblTravelTime.Dispose ();
				lblTravelTime = null;
			}

			if (lblTotalTime != null) {
				lblTotalTime.Dispose ();
				lblTotalTime = null;
			}

			if (lblTotalWalkTime != null) {
				lblTotalWalkTime.Dispose ();
				lblTotalWalkTime = null;
			}

			if (lblNumberTransfers != null) {
				lblNumberTransfers.Dispose ();
				lblNumberTransfers = null;
			}

			if (lblTripDetails != null) {
				lblTripDetails.Dispose ();
				lblTripDetails = null;
			}

			if (tableViewSteps != null) {
				tableViewSteps.Dispose ();
				tableViewSteps = null;
			}

			if (btnSaveCancel != null) {
				btnSaveCancel.Dispose ();
				btnSaveCancel = null;
			}
		}
	}
}
