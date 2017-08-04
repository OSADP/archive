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
	[Register ("PlanSearchViewController")]
	partial class PlanSearchViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton btnCurrentLocEnd { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton btnCurrentLocStart { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPlanATrip { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView planFormBkgrndView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISegmentedControl segmentDateType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtEndLocation { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtMaxWalk { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtStartLocation { get; set; }

		[Action ("CurrentLocationEndClicked:")]
		partial void CurrentLocationEndClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("CurrentLocationStartClicked:")]
		partial void CurrentLocationStartClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("DateClicked:")]
		partial void DateClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("DateDidBeginEditing:")]
		partial void DateDidBeginEditing (MonoTouch.Foundation.NSObject sender);

		[Action ("DateDidEndEditing:")]
		partial void DateDidEndEditing (MonoTouch.Foundation.NSObject sender);

		[Action ("FavoriteListEndClicked:")]
		partial void FavoriteListEndClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("FavoriteListStartClicked:")]
		partial void FavoriteListStartClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("MaxWalkClicked:")]
		partial void MaxWalkClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("MaxWalkDidBeginEditing:")]
		partial void MaxWalkDidBeginEditing (MonoTouch.Foundation.NSObject sender);

		[Action ("SearchForTrips:")]
		partial void SearchForTrips (MonoTouch.Foundation.NSObject sender);

		[Action ("StarEndClicked:")]
		partial void StarEndClicked (MonoTouch.Foundation.NSObject sender);

		[Action ("StarStartClicked:")]
		partial void StarStartClicked (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCurrentLocEnd != null) {
				btnCurrentLocEnd.Dispose ();
				btnCurrentLocEnd = null;
			}

			if (btnCurrentLocStart != null) {
				btnCurrentLocStart.Dispose ();
				btnCurrentLocStart = null;
			}

			if (lblPlanATrip != null) {
				lblPlanATrip.Dispose ();
				lblPlanATrip = null;
			}

			if (planFormBkgrndView != null) {
				planFormBkgrndView.Dispose ();
				planFormBkgrndView = null;
			}

			if (segmentDateType != null) {
				segmentDateType.Dispose ();
				segmentDateType = null;
			}

			if (txtDate != null) {
				txtDate.Dispose ();
				txtDate = null;
			}

			if (txtEndLocation != null) {
				txtEndLocation.Dispose ();
				txtEndLocation = null;
			}

			if (txtMaxWalk != null) {
				txtMaxWalk.Dispose ();
				txtMaxWalk = null;
			}

			if (txtStartLocation != null) {
				txtStartLocation.Dispose ();
				txtStartLocation = null;
			}
		}
	}
}
