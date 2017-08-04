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
using Android.Locations;

namespace IDTO.Android
{
	class LocationTracker 
	{

		private LocationManager _locationManager;
		private String _locationProvider;
		private Context context;
		private ILocationListener locationListener;
		private int errorCount = 0;
		private const int maxErrorCount = 3;

		public LocationTracker(Context context, ILocationListener locationListener)
		{
			this.context = context;
			this.locationListener = locationListener;
		}

		public void InitializeLocation(Accuracy accuracy)
		{
			try {
                _locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
                Criteria criteriaForLocationService = new Criteria
                {
                    Accuracy = accuracy
                };
                IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

                if (acceptableLocationProviders.Any())
                {
                    _locationProvider = acceptableLocationProviders.First();
                }
                else
                {
                    _locationProvider = String.Empty;
                }

                _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, locationListener);
			} catch (Exception e) {
				Console.WriteLine (e);
				OnErrorFindingLocation ();
			}
		}

		public void RemoveUpdates()
		{
			_locationManager.RemoveUpdates (locationListener);
		}

		public void OnErrorFindingLocation()
		{
			errorCount++;
			if (errorCount > maxErrorCount) {
				errorCount = 0;
			} else {
				InitializeLocation (Accuracy.High);			
			}
		}

	}
}

