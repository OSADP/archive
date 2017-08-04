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
using IDTO.Mobile.Manager;
using IDTO.Common.Models;

namespace IDTO.Android
{

	public class HistoryPresenter
	{
		private HistoryView view;
		private Activity activity;

		public HistoryPresenter(Activity activity)
		{
			this.activity = activity;
			this.view =	new HistoryView(activity, this);
			UserTripDataManager dataManager = new UserTripDataManager();

		}

		public async void SearchAndDisplayResults(){

			UserTripDataManager dataManager = new UserTripDataManager ();
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
			int travelerId = loginManager.GetTravelerId ();
			List<Trip> tripsInHistory = await dataManager.GetPastTrips (travelerId, 100);
			this.view.ShowTrips (tripsInHistory);
			this.view.ShowBusy (false);
		}

		public async void OnResume()
		{
			this.view.ShowBusy (true);
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);

			if (await loginManager.IsLoggedIn ()) {
				SearchAndDisplayResults ();
			} else {
				//Display the login screen
				activity.StartActivity (typeof(LoginActivity));
			}
		}

		public void OnTripSelected(Trip trip)
		{
			TripDetailsActivity.trip = trip;
			Intent intent = new Intent (activity.ApplicationContext, typeof(TripDetailsActivity));
			intent.PutExtra (TripDetailsActivity.KEY_CANCELABLE, false);
			activity.StartActivity (intent);
		}



				
	}
}

