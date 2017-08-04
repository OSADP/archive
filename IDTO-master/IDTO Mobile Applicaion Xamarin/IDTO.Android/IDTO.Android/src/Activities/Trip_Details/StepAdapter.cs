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
	public class StepAdapter: BaseAdapter
	{
		private Trip trip;
		private LayoutInflater inflater;
		private int selectedPosition=-1;
		private int _count;

		public override int Count {
			get {
				return _count;
			}
		}

		public StepAdapter(LayoutInflater inflater)
		{
			this.inflater = inflater;
			this.trip = new Trip();
			_count = 0;
		}

		public void Update(Trip trip)
		{
			this.trip = trip;
			this._count = this.trip.Steps.Count;
			NotifyDataSetChanged ();
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return new SI(GetStepAtPosition (position));
		}

		public Step GetStepAtPosition(int position)
		{
			return trip.Steps.ElementAt (position);
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (convertView == null) {
				convertView = inflater.Inflate (Resource.Layout.leg_list_item, null);
			}
				
			UpdateView (GetStepAtPosition (position), convertView, position);
		
			return convertView;
		}

		public void SelectStepAtPosition(int position)
		{
			this.selectedPosition = position;
			NotifyDataSetChanged();
		}
		/*public int Id { get; set; }
		public int TripId { get; set; }
		public int ModeId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string FromName { get; set; }
		public string FromStopCode { get; set; }
		public int? FromProviderId { get; set; }
		public string ToName { get; set; }
		public string ToStopCode { get; set; }
		public int? ToProviderId { get; set; }
		public decimal Distance { get; set; }
		public string RouteNumber { get; set; }

		public int Duration_sec()
		{
			TimeSpan ts = EndDate - StartDate;
			int ts_min = (int)Math.Round (ts.TotalSeconds);
			return ts_min;
		}

		public int Duration_min()
		{
			TimeSpan ts = EndDate - StartDate;
			int ts_min = (int)Math.Round (ts.TotalMinutes);-
			return ts_min;
		}*/
		private void UpdateView (Step step, View convertView, int position)
		{
			ImageView ivIcon = convertView.FindViewById<ImageView> (Resource.Id.leg_list_item_iv_icon);
			TextView tvStopName = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_stop_name);
			TextView tvTime = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_time);
			TextView tvDescription = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_description);
			TextView tvDetailsDepart_Time = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_details_depart_time);
			TextView tvDetailsDepart_Description = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_details_depart_description);
			TextView tvDetailsArrive_Time = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_details_arrive_time);
			TextView tvDetailsArrive_Description = convertView.FindViewById<TextView> (Resource.Id.leg_list_item_tv_details_arrive_description);
			LinearLayout llDetails = convertView.FindViewById<LinearLayout> (Resource.Id.leg_list_item_details);

			if (selectedPosition == position) {
				llDetails.Visibility = ViewStates.Visible;
			} else {
				llDetails.Visibility = ViewStates.Gone;
			}
	
			string mode = ModeType.IdToString (step.ModeId);
			string durationString = step.Duration_min ().ToString () + " min";
			string stopName = step.ToName;
			string toProvider = "";
			int? toProviderID = step.ToProviderId;
			if(toProviderID!=null)
				toProvider = Providers.IdToString((int)toProviderID);
			string agencyAndRoute = toProvider + " " + step.RouteNumber;
			string boardingString = "";
			string departString = "";
			string startTimeString = "";
			string endTimeString = "";
			int? fromProviderID = step.FromProviderId;
			string fromProvider = "";
			if(fromProviderID!=null)
				fromProvider = Providers.IdToString((int)fromProviderID);

			int modeImage;

			startTimeString = step.StartDate.ToLocalTime ().ToString ("t");
			endTimeString = step.EndDate.ToLocalTime ().ToString ("t");
			if (mode.ToLower ().Equals ("walk")) {
				modeImage = Resource.Drawable.walking_icon;
				agencyAndRoute = "Walk";
				boardingString = "Walk from " + step.FromName;
				departString = "to " + step.ToName;
            }
            else if (mode.ToLower().Equals("rail"))
            {
				modeImage = Resource.Drawable.rail_icon;
				boardingString = "Board " + fromProvider + " " + step.RouteNumber;
				departString = "Depart at " + step.ToName;
			} 
            else {
				modeImage = Resource.Drawable.bus_icon;
				boardingString = "Board " + fromProvider + " " + step.RouteNumber;
				departString = "Depart at " + step.ToName;
			}

			tvDetailsDepart_Description.Text = boardingString;
			tvDetailsDepart_Time.Text = startTimeString;

			tvDetailsArrive_Description.Text = departString;
			tvDetailsArrive_Time.Text = endTimeString;


			tvStopName.Text = stopName;
			ivIcon.SetImageResource (modeImage);
			tvTime.Text = durationString;
			tvDescription.Text = agencyAndRoute;

		}
		private class SI : Java.Lang.Object 
		{
			public Step step{ get; set;}

			public SI(Step step)
			{
				this.step = step;
			}
		}
	}
}
