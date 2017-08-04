using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using IDTO.Common.Models;
using IDTO.Mobile.Manager;

namespace IDTO.Android
{
	public class TripDetailsPresenter 
	{
		private TripDetailsView view;
        private BaseActivity activity;
        private Trip trip;
		public TripDetailsPresenter(BaseActivity activity, Trip trip, Bundle extras)
		{
			var cancelable = isCancelable (extras);
            this.activity = activity;
            this.trip = trip;

			this.view = new TripDetailsView (activity, this, cancelable);
			this.view.DisplayTrip (trip);
		}

		private bool isCancelable (Bundle extras)
		{
			bool cancelable = true;
			if (extras != null) {
				cancelable = extras.GetBoolean (TripDetailsActivity.KEY_CANCELABLE);
			}
			return cancelable;
		}

        public void Cancel()
        {
			CancelTripDialog.Show (activity, this, trip);

			//int travelerId = AndroidLoginManager.Instance(activity).GetTravelerId();

			//bool success = await new UserTripDataManager().CancelTripForUser(travelerId, trip);

			//OnTripCancelComplete(success);
        }

		public void OnTripCancelComplete(bool success)
        {
            activity.sendGaEvent("ui_action", "cancel trip", "cancel trip", Convert.ToInt16(success));
            if (success)
            {
                view.OnCancelComplete();
                activity.SetResult(Result.Ok);
                activity.Finish();
            }
            else
            {
                view.OnCancelError();
                activity.SetResult(Result.Canceled);
            }
        }
	}
}

