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
using Android.Graphics;
using IDTO.Common.Models;

namespace IDTO.Android
{
	public class TripAdapter : BaseAdapter
	{

		protected LayoutInflater inflater;
		protected List<Trip> trips;
		protected int _count;
		protected bool cancelable = false;
		public override int Count {
			get {
				return _count;
			}
		}
		public TripAdapter(LayoutInflater inflater) : this(inflater, false)
		{		}

		public TripAdapter(LayoutInflater inflater, bool cancelable)
		{
			this.cancelable = cancelable;
			this.inflater = inflater;
			this.trips = new List<Trip> ();
			_count = 0;
		}

		public void Update(List<Trip> trips)
		{
			this.trips = trips;
			this._count = trips.Count;
			NotifyDataSetChanged ();
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return new TI(GetTripAtPosition (position));
		}

		public Trip GetTripAtPosition(int position)
		{
			return trips.ElementAt (position);
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

			Trip trip = GetTripAtPosition (position);
			DateTime startDate = trip.TripStartDate;
			DateTime now = DateTime.Now;
			string dayDisplay = "";
			string timeDisplay = startDate.ToLocalTime().ToString ("h:mm tt");
			string durationDisplay = trip.Duration_min()+" min";
			string titleDisplay = trip.Destination;
			string subtitleDisplay = "";
			try {
				subtitleDisplay = trip.GetFirstStepString();
			}catch{
			}
			/*int walkMode = 1;
			if (trip.Steps.Count > 0) {
				Step step = trip.Steps.ElementAt (0);			
				if (step.ModeId==walkMode) {
					subtitleDisplay = "Walk";
				} else {
					subtitleDisplay = step.FromProviderId + " " + step.FromName;
				}
			}*/

			if (now.Day == startDate.Day) {
				dayDisplay = "Today";
			} else if (now.Day + 1== startDate.Day ) {
				dayDisplay = "Tomorrow";
			} else {
				dayDisplay = startDate.ToString ("M/dd/yy");
			}
			CancelView btnCancel =  convertView.FindViewById<CancelView> (Resource.Id.search_result_item_btn_cancel);
			//btnCancel.Visibility = cancelable  ? ViewStates.Visible : ViewStates.Gone;
			btnCancel.Visibility = ViewStates.Gone;
			btnCancel.Enabled = false;
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



		protected class TI : Java.Lang.Object 
		{
			public Trip trip{ get; set;}

			public TI(Trip trip)
			{
				this.trip = trip;
			}
		}
	}
}

