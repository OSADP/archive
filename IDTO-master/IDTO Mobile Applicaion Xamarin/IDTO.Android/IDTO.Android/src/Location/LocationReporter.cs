using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
//using Android.Locations;
using Android.Util;
using IDTO.Common;
using IDTO.Common.Models;

using Android.Gms.Common;
using Android.Gms.Location;

namespace IDTO.Android
{
    class LocationReporter : Java.Lang.Object, IGooglePlayServicesClientConnectionCallbacks,
        IGooglePlayServicesClientOnConnectionFailedListener, ILocationListener
	{
        private LocationClient mLocationClient;
        private LocationRequest mLocationRequest;

		private double updatePeriodMilliseconds = 10000;
		private DateTime lastTime;
		//private LocationTracker locationTracker;
  
		private global::Android.Locations.Location location;
		private int locationErrorCount = 0;
		private TravelerLocation travelerLocation;
		private bool isReporting = false;
		private DateTime startTime;
		private DateTime endTime; 
		private Context context;
		private const int locationErrorCountMax = 4;


		public LocationReporter(Context context, int userId)
		{
            mLocationClient = new LocationClient(context, this, this);

            mLocationRequest = LocationRequest.Create();
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetInterval(10 * 1000);
            mLocationRequest.SetFastestInterval(1000);

			this.travelerLocation = new TravelerLocation ();
			this.travelerLocation.UserId = userId.ToString();
			this.context = context;
		}	

		public void StartNow(double secondsToRun)
		{
			if (isReporting) {
				Log.Info ("IDTO","Continue location report secondsToRun="+secondsToRun);
				endTime = DateTime.Now.AddSeconds (secondsToRun);
			} else {
				Log.Info ("IDTO","StartNow location report");
                
				Start (DateTime.Now, DateTime.Now.AddSeconds (secondsToRun));
			}
		}

		public void Start(DateTime startTime, DateTime endTime)
		{	
			isReporting = true;
			locationErrorCount = 0;
			this.startTime = startTime;
			this.endTime = endTime;
			lastTime = DateTime.Now;
            mLocationClient.Connect();	
		}

		public void Stop()
		{
			this.isReporting = false;
            if (mLocationClient.IsConnected)
            {
                mLocationClient.RemoveLocationUpdates(this);
            }
            mLocationClient.Disconnect();
		}

		private void OnLocationEvent()
		{
			Log.Info ("IDTO","_|*|_ On location event _|*|_");
			if (!isReporting || DateTime.Now > endTime) {
				Stop ();
				return;
			} else if(DateTime.Now >= lastTime.AddMilliseconds(updatePeriodMilliseconds)) {
				lastTime = DateTime.Now;
				if (location == null) {
					locationErrorCount++;
					Log.Info ("IDTO","locationErrorCount: "+locationErrorCount);
					if (locationErrorCount > locationErrorCountMax) {
						//OnErrorFindingLocation ();
					}
				} else {
					locationErrorCount = 0;	
					ReportLocation (CreateTravelerLocation ());
				}
			}
		}

		private TravelerLocation CreateTravelerLocation()
		{
			AndroidLoginManager loginMngr = AndroidLoginManager.Instance (context);
			TravelerLocation travelerLocation = new TravelerLocation ();
			travelerLocation.Latitude = location.Latitude;
			travelerLocation.Longitude = location.Longitude;
			travelerLocation.TimeStamp = DateTime.Now;
			travelerLocation.UserId = loginMngr.GetUserId();
			travelerLocation.TravelerId = loginMngr.GetTravelerId ();
			return travelerLocation;
		}

		private async void ReportLocation(TravelerLocation travelerLocation)
		{
			Log.Info ("IDTO","### location report ###");
			TripManager tripManager = new TripManager ();
			await tripManager.PostTravelerLocation (travelerLocation);
		}

        public void OnConnected(Bundle p0)
        {
            mLocationClient.RequestLocationUpdates(mLocationRequest, this);
        }

        public void OnDisconnected()
        {
            Console.WriteLine("Google Play Service Connection Failed");
        }

        public void OnConnectionFailed(ConnectionResult p0)
        {
            Console.WriteLine("Google Play Service Connection Failed");
        }

        public void OnLocationChanged(global::Android.Locations.Location p0)
        {
            this.location = p0;
            if (isReporting)
            {
                OnLocationEvent();
            }

        }
	}
}

