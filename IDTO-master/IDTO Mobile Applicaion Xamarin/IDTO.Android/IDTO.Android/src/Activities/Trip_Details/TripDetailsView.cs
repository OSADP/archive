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
	class TripDetailsView
	{
		private TripDetailsPresenter presenter;
		private Activity activity;
		private TextView tvNumberOfTransfers;
		private TextView tvTotalWalk;
		private TextView tvTravel;
		private TextView tvTotalTime;
		private ListView listViewLegs;
		private StepAdapter stepAdapter;
		private Button btnCancel;

		public TripDetailsView(Activity activity, TripDetailsPresenter presenter, bool cancelable)
		{
			this.presenter = presenter;
			this.activity =	activity;
			this.activity.SetContentView (Resource.Layout.trip_details);
			this.tvNumberOfTransfers = activity.FindViewById<TextView>(Resource.Id.trip_details_number_of_transfers);
			this.tvTotalWalk = activity.FindViewById<TextView>(Resource.Id.trip_details_total_walk);
			this.tvTotalTime = activity.FindViewById<TextView>(Resource.Id.trip_details_total_time);
			this.tvTravel = activity.FindViewById<TextView>(Resource.Id.trip_details_travel);
			this.listViewLegs = activity.FindViewById<ListView> (Resource.Id.trip_details_number_listview_legs);
			this.btnCancel = activity.FindViewById<Button> (Resource.Id.trip_details_bottom_btn);
			this.stepAdapter = new StepAdapter (activity.LayoutInflater);
			this.listViewLegs.Adapter = this.stepAdapter;
			this.listViewLegs.ItemClick += OnStepSelected;
			this.btnCancel.Visibility = cancelable ? ViewStates.Visible : ViewStates.Gone;
			this.btnCancel.Click += btnCancel_Click;
		}

		public void DisplayTrip(Trip trip)
		{
			this.stepAdapter.Update (trip);
			this.tvNumberOfTransfers.Text = trip.GetNumberOfTransfers ().ToString();
			this.tvTotalWalk.Text = trip.GetWalkTime_min ().ToString () + " min";
			this.tvTotalTime.Text = trip.Duration_min ().ToString () + " min";
			this.tvTravel.Text = trip.TripStartDate.ToLocalTime().ToString ("h:mm:ss tt, M/dd/yy");
		}

		private void OnStepSelected(object sender, AdapterView.ItemClickEventArgs e)
		{
			stepAdapter.SelectStepAtPosition (e.Position);
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
            this.presenter.Cancel();
			Console.WriteLine ("<<<CANCEL>>>");
		}

        public void OnCancelComplete()
        {
            Toast.MakeText(activity, "Cancel Complete", ToastLength.Long).Show();
        }

        public void OnCancelError()
        {
            Toast.MakeText(activity, "There was a problem canceling your trip", ToastLength.Long).Show();
        }
	}
}

