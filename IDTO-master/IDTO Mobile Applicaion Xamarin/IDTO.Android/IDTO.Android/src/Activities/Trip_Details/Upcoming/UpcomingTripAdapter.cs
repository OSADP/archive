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
	public class UpcomingTripAdapter : TripAdapter
	{
		private ITripSelector tripSelector;

		public UpcomingTripAdapter(LayoutInflater layoutInflater, ITripSelector tripSelector) : base(layoutInflater, true)
		{
			this.tripSelector = tripSelector;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			convertView = base.GetView (position, convertView, parent);
			Trip trip = GetTripAtPosition (position);

			DateTime now = DateTime.Now;
			DateTime startDate = trip.TripStartDate;
			View item = convertView.FindViewById<View> (Resource.Id.search_result_item_ll_details);
			CancelView btnCancel =  convertView.FindViewById<CancelView> (Resource.Id.search_result_item_btn_cancel);
			//btnCancel.Visibility = cancelable  ? ViewStates.Visible : ViewStates.Gone;
			//btnCancel.Enabled = now.Day != startDate.Day;
			//if (now.Day == startDate.Day) {
				//	btnCancel.SetBackgroundColor (Color.DarkGray);
			//}


			//btnCancel.SetOnClickListener (new OnCancelClickListener (trip, tripSelector));
			item.SetOnClickListener (new OnSelectClickListener (trip, tripSelector));

			return convertView;
		}

		public class OnCancelClickListener : Java.Lang.Object, View.IOnClickListener 
		{
			private Trip trip;
			private ITripSelector tripSelector;

			public OnCancelClickListener(Trip trip, ITripSelector tripSelector)
			{
				this.trip = trip;
				this.tripSelector = tripSelector;
			}

			public void OnClick(View v)
			{
				tripSelector.OnCancelTrip (trip);
			}
		

		}

		private class OnSelectClickListener : Java.Lang.Object, View.IOnClickListener
		{
			private Trip trip;
			private ITripSelector tripSelector;

			public OnSelectClickListener(Trip trip, ITripSelector tripSelector)
			{
				this.trip = trip;
				this.tripSelector = tripSelector;
			}	

			public void OnClick(View v)
			{
				tripSelector.OnSelectTrip (trip);
			}
		}
	

	}
}

