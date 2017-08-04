// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;

using IDTO.Common.Models;
using IDTO.Mobile.Manager;
using System.Threading.Tasks;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace IDTO.iPhone
{
	public partial class PastTripsViewController : IDTOViewController
	{
		private UserTripDataManager mUserTripManager;
		private Trip mTripSelected;

		public PastTripsViewController (IntPtr handle) : base (handle)
		{

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			mUserTripManager = new UserTripDataManager ();
			setupConnectorView ();
		}

		private void setupConnectorView ()
		{
			this.AddConnectorView (lblPastTrips);
			ExtendToView = pastTripsTableView;
		}

		async public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			showLoading ();
			await loadData ();

			dismissLoading();
		}

		async private Task loadData()
		{
			iOSLoginManager loginManager = iOSLoginManager.Instance;

			int travelerId = loginManager.GetTravelerId ();

			List<Trip> trips = await mUserTripManager.GetPastTrips (travelerId, 25);

			UpcomingTripsTableSource tableSource = new UpcomingTripsTableSource (trips, new TripTableCell(""));

			tableSource.TripSelected += (Trip trip) => {
				mTripSelected = trip;
				PerformSegue("TripDetailsSegue", this);
			};


			pastTripsTableView.Source = tableSource;
			pastTripsTableView.ReloadData ();

		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier == "TripDetailsSegue") {
				TripDetailsViewController vc = (TripDetailsViewController)segue.DestinationViewController;
				vc.TripToShow = mTripSelected;
			}
		}
	}
}
