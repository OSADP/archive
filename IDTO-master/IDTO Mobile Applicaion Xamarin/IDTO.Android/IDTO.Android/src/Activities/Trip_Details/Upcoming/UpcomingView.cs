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

namespace IDTO.Android
{
	public class UpcomingView : BaseView, ITripSelector
	{
		private UpcomingPresenter presenter;
		private ListView listViewResults;
		private UpcomingTripAdapter tripAdapter;

		public UpcomingView(Activity activity, UpcomingPresenter presenter):base(activity)
		{
			this.presenter = presenter;
			activity.SetContentView (Resource.Layout.upcoming_trips);
			this.tripAdapter = new UpcomingTripAdapter (activity.LayoutInflater, this);

			this.listViewResults = activity.FindViewById<ListView>(Resource.Id.upcoming_trips_list_view);

			this.listViewResults.Adapter = this.tripAdapter;

			ShowBusy (false);
		}

		public void ShowTrips(List<Trip> tripsInHistory)
		{
			tripAdapter.Update (tripsInHistory);
		}

		public void OnCancelTrip(Trip trip)
		{
			Console.WriteLine ("::::::OnCancelTrip::::::");
		}

		public void OnSelectTrip(Trip trip)
		{
			presenter.OnTripSelected (trip);
		}


	
	}
}

