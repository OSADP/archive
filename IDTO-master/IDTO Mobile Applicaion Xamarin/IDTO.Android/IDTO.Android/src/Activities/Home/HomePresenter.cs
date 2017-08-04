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
using IDTO.Mobile.Manager;
using IDTO.Common.Models;

using IDTO.Common;

namespace IDTO.Android
{
	public class HomePresenter :LocationPresenter 
	{
		private const string KEY_LATITUDE = "IDTO.Android.KEY_LATITUDE";
		private const string KEY_LONGITUDE = "IDTO.Android.KEY_LONGITUDE";
		private HomeView view;
		private Activity activity;
		private AndroidLoginManager loginManager;
		private double latitude = -1;
		private double longitude = -1;
		private WeatherInfo weather;

		public HomePresenter(Activity activity):base(activity)
		{
			this.activity = activity;
			this.view = new HomeView (activity, this);
		}

		private static ISharedPreferences GetPreferences (Context context)
		{
			return context.GetSharedPreferences (PushHandlerService.PREFERENCES_KEY, FileCreationMode.Private);
		}

		public void OnAccountClick() 
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(AccountActivity));
			activity.StartActivity (intent);
		}

		public void OnPlanTripClick() 
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(PlanActivity));
			activity.StartActivity (intent);
		}

		public void OnScheduledTripsClick()
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(UpcomingActivity));
			activity.StartActivity (intent);
		}

		public void OnbtnTripHistoryClick()
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(HistoryActivity));
			activity.StartActivity (intent);
		}

		public async void OnResume()
		{
			loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
			if (await loginManager.IsLoggedIn ()) {

                TravelerModel traveler = await new AccountManager().GetTravelerById(loginManager.GetTravelerId());

                if (traveler.InformedConsent == false)
                {
                    loginManager.Logout();
                    activity.StartActivity(typeof(LoginActivity));
                    activity.Finish();
                }
                else
                {
                    LoadTrips();
                    RegisterGCM(activity);
                }
			} else {
				//Display the login screen
				activity.StartActivity(typeof(LoginActivity));
				activity.Finish ();
			}		
		}

		public void OnPause()
		{
			//UnRegisterGCM (activity);

		}

		public void OnClickUpcomingTrip (Trip trip)
		{
			if (trip == null)
				return;
			TripDetailsActivity.trip = trip;
			Intent intent = new Intent (activity.ApplicationContext, typeof(TripDetailsActivity));
			intent.PutExtra (TripDetailsActivity.KEY_CANCELABLE, true);
			activity.StartActivity (intent);
		}



		public static void RegisterGCM(Context context)
		{
		
		
			if (HomePresenter.GetPreferences (context).GetBoolean (PushHandlerService.KEY_REGISTERED, false)) {
				Console.WriteLine ("RegisterGCM ERROR: Cannot register multiple times");
				return;
			} else {
				try{
					const string senders = Constants.SenderID;
					var intent = new Intent ("com.google.android.c2dm.intent.REGISTER");
					intent.SetPackage ("com.google.android.gsf");
					intent.PutExtra ("app", PendingIntent.GetBroadcast (context, 0, new Intent (), 0));
					intent.PutExtra ("sender", senders);
					context.StartService (intent);				
				}catch(Exception e) {
					Console.WriteLine (e);

				}
			}
		}
		private ISharedPreferences GetPreferences()
		{
			return activity.GetPreferences (FileCreationMode.Private);
		}

		private void SaveLocationAsPreference ()
		{
			if (latitude != -1 && longitude != -1) {
				ISharedPreferencesEditor editor = GetPreferences ().Edit ();
				editor.PutString (KEY_LATITUDE, latitude.ToString());
				editor.PutString (KEY_LONGITUDE, longitude.ToString());
			}
		}

		private void LoadSavedLocation ()
		{
			try{
				ISharedPreferences p = GetPreferences ();
				latitude = Double.Parse(p.GetString(KEY_LATITUDE, "-1").ToString());
				longitude = Double.Parse(p.GetString(KEY_LONGITUDE, "-1").ToString());
				UpdateWeather();
			}catch(Exception e){
				Console.WriteLine (e);
			}
		}

        public override void UpdateLocation(global::Android.Locations.Location loc)
        {
            if (loc != null)
            {
                this.latitude = loc.Latitude;
                this.longitude = loc.Longitude;
                if (latitude != -1 && longitude != -1)
                {
                    SaveLocationAsPreference();
                    UpdateWeather();
                    StopTrackingLocation();
                }
            }
        }

		private void UpdateWeather()
		{	
			if (latitude != -1 && longitude != -1) {
				this.weather = new HomeDataManager ().GetWeather (latitude, longitude);
				view.OnWeatherUpdate (weather);
			}
		}
		public static void UnRegisterGCM(Context context)
		{
			var intent = new Intent("com.google.android.c2dm.intent.UNREGISTER");
			intent.PutExtra("app", PendingIntent.GetBroadcast(context, 0, new Intent(), 0));
			context.StartService(intent);
		}
		private async void  LoadTrips()
		{
			view.ShowBusy (true);

            

			List<Trip> upcomingTrips = await new UserTripDataManager ().GetUpcomingTrips (loginManager.GetTravelerId (), 4);
			view.ShowUpcomingTrips (upcomingTrips);
			view.ShowBusy (false);
		}

       
	}
}

