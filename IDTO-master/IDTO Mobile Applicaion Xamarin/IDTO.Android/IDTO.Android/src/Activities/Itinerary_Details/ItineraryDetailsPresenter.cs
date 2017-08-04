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
using IDTO.Mobile.Manager;
using IDTO.Android.src.Location;

namespace IDTO.Android
{
	public class ItineraryDetailsPresenter
	{
		private Itinerary itinerary;
		private ItineraryDetailsView view;
        private BaseActivity activity;
		private string startLocation;
		private string endLocation;
		private bool isTripValid = false;
        private AlarmManager mAlarmManager;
		public ItineraryDetailsPresenter(BaseActivity activity, Itinerary itinerary, Bundle extras){
			this.activity = activity;
			this.itinerary = itinerary;	
			this.view = new ItineraryDetailsView (activity, this);
			this.view.DisplayItinerary (this.itinerary);

            this.mAlarmManager = (AlarmManager)activity.GetSystemService(Context.AlarmService);


			try{
				startLocation = extras.GetString(ItineraryDetailsActivity.KEY_startLocation);
				endLocation = extras.GetString(ItineraryDetailsActivity.KEY_endLocation);
				isTripValid = true;
			}catch(Exception e)
			{
				Console.WriteLine(e);
				isTripValid = false;
			}

		}

        public void ShowMap()
        {
            ItineraryMapIntent.StartMapIntent(this.activity, this.itinerary);
        }

		public async void Save()
		{
			try{
			if (isTripValid) {			
				int travelerId = AndroidLoginManager.Instance (activity).GetTravelerId ();
				string prioritycode = "1";
				bool isWheelchariNeeded = false;
				bool isBikeRackNeeded = false;
				bool success = await new UserTripDataManager ().SaveTripForUser (travelerId, itinerary, 
					              startLocation, endLocation, prioritycode, isWheelchariNeeded, isBikeRackNeeded);
                

				OnTripSaveComplete (success);
			}else{
				OnTripSaveComplete (false);
			}
			}catch(Exception e) {
				Console.WriteLine (e);
				OnTripSaveComplete (false);
			}
		}
	
		private void OnTripSaveComplete(bool success)
		{
            activity.sendGaEvent("ui_action", "save trip", "save trip", Convert.ToInt16(success));

			if (success) {

                Intent alarmIntent = new Intent(activity,typeof(LocationAlarmReceiver));
                PendingIntent pi = PendingIntent.GetBroadcast(activity.ApplicationContext, 0, alarmIntent, 0);

                DateTime dtNow = DateTime.Now.ToLocalTime();
                DateTime dtStart = this.itinerary.GetStartDate().ToLocalTime();

                TimeSpan diffTS = dtStart - dtNow;

                long ms = (long)diffTS.TotalMilliseconds;
                
                if (((int)Build.VERSION.SdkInt) >= 19)
                {
                    mAlarmManager.SetExact(AlarmType.ElapsedRealtimeWakeup, ms, pi);
                }
                else
                {
                    mAlarmManager.Set(AlarmType.ElapsedRealtimeWakeup, ms, pi);
                }
				view.OnSaveComplete ();
				activity.SetResult (Result.Ok);
				activity.Finish ();
			} else {
				view.OnSaveError ();
				activity.SetResult (Result.Canceled);
			}
		}
	}
}

