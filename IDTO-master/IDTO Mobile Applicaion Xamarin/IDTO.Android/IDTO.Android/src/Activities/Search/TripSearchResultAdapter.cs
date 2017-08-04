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
	public class TripSearchResultAdapter : BaseAdapter
	{
		private TripSearchResult tripSearchResult;
		private LayoutInflater inflater;

		private int _count;

		public override int Count {
			get {
				return _count;
			}
		}


		public TripSearchResultAdapter(LayoutInflater inflater)
		{
			this.inflater = inflater;
			this.tripSearchResult = new TripSearchResult();
			_count = 0;
		}

		public void Update(TripSearchResult result)
		{
			if (result == null) {
				this.tripSearchResult = new TripSearchResult ();
				this._count = 0;
			} else {
				this.tripSearchResult = result;
				this._count = this.tripSearchResult.itineraries.Count;
			}
			NotifyDataSetChanged ();
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return new JI(GetItineraryAtPosition (position));
		}

		public Itinerary GetItineraryAtPosition(int position)
		{
			return tripSearchResult.itineraries.ElementAt (position);
		}

		public override long GetItemId(int position)
		{
			return position;
		}


		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (convertView == null) {
				convertView = inflater.Inflate (Resource.Layout.search_results_item, null);
			}

			Itinerary itinerary = GetItineraryAtPosition (position);
			DateTime startDate = itinerary.GetStartDate ();
			DateTime now = DateTime.Now;
			CancelView btnDelete = convertView.FindViewById<CancelView> (Resource.Id.search_result_item_btn_cancel);
		

			string dayDisplay = "";
			string timeDisplay = startDate.ToLocalTime().ToString ("h:mm tt");
			string durationDisplay = itinerary.GetDuration_min()+" min";
			string titleDisplay = itinerary.GetFirstAgencyName ();
			string subtitleDisplay = "";

			btnDelete.Visibility = ViewStates.Gone;

			if (itinerary.legs.Count > 0) {
				Leg leg = itinerary.legs.ElementAt (itinerary.legs.Count-1);			
				subtitleDisplay = leg.agencyName;
			}

			if (now.Day == startDate.Day) {
				dayDisplay = "Today";
			} else if (now.Day + 1== startDate.Day ) {
				dayDisplay = "Tomorrow";
			} else {
				dayDisplay = startDate.ToString ("M/dd/yy");
			}
				
			TextView tvDay = convertView.FindViewById<TextView> (Resource.Id.search_result_item_tv_day);
			TextView tvTime = convertView.FindViewById<TextView> (Resource.Id.search_result_item_tv_time);
			TextView tvDuration = convertView.FindViewById<TextView> (Resource.Id.search_result_item_tv_duration);
			TextView tvTitle = convertView.FindViewById<TextView> (Resource.Id.search_result_item_tv_title);
			TextView tvSubtitle = convertView.FindViewById<TextView> (Resource.Id.search_result_item_tv_subtitle);
			tvDay.Text = dayDisplay;
			tvTime.Text = timeDisplay;
			tvDuration.Text = durationDisplay;
			tvTitle.Text = titleDisplay;
			tvSubtitle.Text = subtitleDisplay;

			return convertView;
		}
			
		private class JI : Java.Lang.Object 
		{
			public Itinerary itinerary{ get; set;}

			public JI(Itinerary itinerary)
			{
				this.itinerary = itinerary;
			}
		}
	}
}

