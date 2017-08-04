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
	class HistoryView : BaseView
	{
		private HistoryPresenter presenter;
		private ListView listViewResults;
		private TripAdapter tripHistoryAdapter;

        public HistoryView(Activity activity, HistoryPresenter presenter)
            : base(activity)
		{
			this.presenter = presenter;
			activity.SetContentView (Resource.Layout.trip_history);
			this.tripHistoryAdapter = new TripAdapter (activity.LayoutInflater);
			this.progressBar = activity.FindViewById<ProgressBar> (Resource.Id.trip_history_progressbar);
			this.listViewResults = activity.FindViewById<ListView>(Resource.Id.trip_history_list_view);

			this.listViewResults.Adapter = this.tripHistoryAdapter;
			this.listViewResults.ItemClick += (object sender, AdapterView.ItemClickEventArgs args) 
				=> OnTripSelected(sender, args);

			ShowBusy (false);
		}

		public void ShowTrips(List<Trip> tripsInHistory)
		{
			tripHistoryAdapter.Update (tripsInHistory);
		}

		private void OnTripSelected(object sender, AdapterView.ItemClickEventArgs e)
		{
			Trip trip = (Trip)tripHistoryAdapter.GetTripAtPosition (e.Position);
			presenter.OnTripSelected (trip);
		}

	
	}
}

