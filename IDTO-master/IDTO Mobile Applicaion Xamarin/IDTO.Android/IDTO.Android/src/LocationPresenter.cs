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

using Android.Gms.Common;
using Android.Gms.Location;

namespace IDTO.Android
{
    public class LocationPresenter : Java.Lang.Object,
        IGooglePlayServicesClientConnectionCallbacks,
        IGooglePlayServicesClientOnConnectionFailedListener,
        ILocationListener
    {
        private LocationClient mLocationClient;
        private LocationRequest mLocationRequest;

        public LocationPresenter(Activity activity)
		{
            mLocationClient = new LocationClient(activity, this, this);

            mLocationRequest = LocationRequest.Create();
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            mLocationRequest.SetInterval(10 * 1000);
            mLocationRequest.SetFastestInterval(1000);
		}

        public void OnStart()
        {
            mLocationClient.Connect();
        }

        public void OnStop()
        {
            StopTrackingLocation();
        }

        protected void StopTrackingLocation()
        {
            if (mLocationClient.IsConnected)
            {
                mLocationClient.RemoveLocationUpdates(this);
            }
            mLocationClient.Disconnect();
        }

        public void OnConnected(Bundle p0)
        {
            mLocationClient.RequestLocationUpdates(mLocationRequest, this);
        }

        public void OnDisconnected()
        {
            Console.WriteLine("Home Activity - Google Play Service Connection Failed");
        }

        public void OnConnectionFailed(ConnectionResult p0)
        {
            Console.WriteLine("Home Activity - Google Play Service Connection Failed");
        }

        public void OnLocationChanged(global::Android.Locations.Location p0)
        {
            UpdateLocation(p0);
            
        }

        public virtual void UpdateLocation(global::Android.Locations.Location loc){}
    }
}