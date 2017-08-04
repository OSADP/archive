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
	public class ItineraryAdapter: BaseAdapter
	{
		private Itinerary itinerary;
		private LayoutInflater inflater;
		private int selectedPosition=-1;
		private int _count;

		public override int Count {
			get {
				return _count;
			}
		}

		public ItineraryAdapter(LayoutInflater inflater)
		{
			this.inflater = inflater;
			this.itinerary = new Itinerary();
			_count = 0;
		}

		public void Update(Itinerary itinerary)
		{
			this.itinerary = itinerary;
			this._count = this.itinerary.legs.Count;
			NotifyDataSetChanged ();
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return new LI(GetLegAtPosition (position));
		}

		public Leg GetLegAtPosition(int position)
		{
			return itinerary.legs.ElementAt (position);
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
				
			UpdateView (GetLegAtPosition (position), convertView, position);
		
			return convertView;
		}

		public void SelectLegAtPosition(int position)
		{
			this.selectedPosition = position;
			NotifyDataSetChanged();
		}

		private void UpdateView (Leg leg, View convertView, int position)
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

			string mode = leg.mode;
			string durationString = leg.GetDuration_min ().ToString () + " min";
			string stopName = leg.to.name;
			string agencyProvider = leg.agencyId;
			if (agencyProvider != null) {
				try{
					agencyProvider = Providers.IdToString (int.Parse(agencyProvider));
				}catch(Exception e){
					Console.WriteLine (e);
				}
			}
			string agencyAndRoute = agencyProvider + " " + leg.routeShortName;
			string boardingString = "";
			string departString = "";
			string startTimeString = "";
			string endTimeString = "";
			int modeImage;

			startTimeString = leg.GetStartDate ().ToLocalTime ().ToString ("t");
			endTimeString = leg.GetEndDate ().ToLocalTime ().ToString ("t");
			if (mode.ToLower ().Equals ("walk")) {
				modeImage = Resource.Drawable.walking_icon;
				agencyAndRoute = "Walk";
				boardingString = "Walk from " + leg.from.name;
				departString = "to " + leg.to.name;
            }
            else if (mode.ToLower().Equals("rail"))
            {
				modeImage = Resource.Drawable.rail_icon;
				boardingString = leg.agencyId + " " + leg.route + " at " + leg.from.name;
				departString = leg.to.name;
			}
            
            else {
				modeImage = Resource.Drawable.bus_icon;
				boardingString = leg.agencyId + " " + leg.route + " at " + leg.from.name;
				departString = leg.to.name;
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
		private class LI : Java.Lang.Object 
		{
			public Leg leg{ get; set;}

			public LI(Leg leg)
			{
				this.leg = leg;
			}
		}
	}
}
