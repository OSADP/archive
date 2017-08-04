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

namespace IDTO.Android.src.Location
{
    public class LocationAlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            new LocationReporterManager(context).Track(LocationReporterManager.DEFAULT_REPORT_TIME_SEC);
        }
    }
}