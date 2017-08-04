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
	public class ItineraryDetailsView
	{

		private ItineraryDetailsPresenter presenter;
		private Activity activity;
		private TextView tvNumberOfTransfers;
		private TextView tvTotalWalk;
		private TextView tvTravel;
		private TextView tvTotalTime;
		private Button btnSave;
        private ImageButton btnMap;
		private ListView listViewLegs;
		private ItineraryAdapter itineraryAdapter;

		public ItineraryDetailsView(Activity activity, ItineraryDetailsPresenter presenter){
			this.presenter = presenter;
			this.activity =	activity;
			this.activity.SetContentView (Resource.Layout.itinerary_details);
			this.tvNumberOfTransfers = activity.FindViewById<TextView>(Resource.Id.itinerary_details_number_of_transfers);
			this.tvTotalWalk = activity.FindViewById<TextView>(Resource.Id.itinerary_details_total_walk);
			this.tvTotalTime = activity.FindViewById<TextView>(Resource.Id.itinerary_details_total_time);
			this.tvTravel = activity.FindViewById<TextView>(Resource.Id.itinerary_details_travel);
			this.btnSave = activity.FindViewById<Button>(Resource.Id.itinerary_details_btn_save);
            this.btnMap = activity.FindViewById<ImageButton>(Resource.Id.itinerary_details_btn_map);
			this.listViewLegs = activity.FindViewById<ListView> (Resource.Id.itinerary_details_number_listview_legs);

			this.btnSave.Click += btnSave_Click;

            this.btnMap.Click += btnMap_Click;

			this.itineraryAdapter = new ItineraryAdapter (activity.LayoutInflater);
			this.listViewLegs.Adapter = this.itineraryAdapter;
			this.listViewLegs.ItemClick += OnLegSelected;
		}

		public void DisplayItinerary(Itinerary itinerary)
		{
			this.itineraryAdapter.Update (itinerary);
			this.tvNumberOfTransfers.Text = itinerary.GetNumberOfTransfers ().ToString();
			this.tvTotalWalk.Text = itinerary.GetWalkTime_min ().ToString() + " min";
			this.tvTotalTime.Text = itinerary.GetDuration_min ().ToString() + " min";
			this.tvTravel.Text = itinerary.GetStartDate ().ToLocalTime().ToString ("h:mm:ss tt, M/dd/yy");
		}

		private void OnLegSelected(object sender, AdapterView.ItemClickEventArgs e)
		{
			itineraryAdapter.SelectLegAtPosition (e.Position);
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			presenter.Save ();
		}

        private void btnMap_Click(object sender, EventArgs e)
        {
            presenter.ShowMap();
        }

		public void OnSaveComplete ()
		{
			Toast.MakeText (activity, "Save Complete", ToastLength.Long).Show ();
		}

		public void OnSaveError ()
		{
			Toast.MakeText (activity, "There was a problem saving your trip", ToastLength.Long).Show ();
		}
	}
}

