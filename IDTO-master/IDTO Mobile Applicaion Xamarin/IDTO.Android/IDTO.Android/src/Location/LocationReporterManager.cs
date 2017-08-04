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
using Android.Util;
using IDTO.Common;

namespace IDTO.Android
{
	public class LocationReporterManager
	{
		public const double DEFAULT_REPORT_TIME_SEC = 120;
		private static LocationReporter locationReporter;
		private int travelerId;
		public LocationReporterManager(Context context)
		{
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(context);
			this.travelerId = loginManager.GetTravelerId ();		
			if (LocationReporterManager.locationReporter == null) {
				LocationReporterManager.locationReporter = new LocationReporter (context, travelerId);
			}
		}

		public void Track(double reportTimeInSeconds)
		{
			reportTimeInSeconds = reportTimeInSeconds <= 0 ? DEFAULT_REPORT_TIME_SEC : reportTimeInSeconds;
			if (travelerId == -1) {
				Log.Error ("", "UpdateTripTimes [travelerId == -1]");
			} else {		
				locationReporter.StartNow (reportTimeInSeconds);
			}
		}
	}
}

