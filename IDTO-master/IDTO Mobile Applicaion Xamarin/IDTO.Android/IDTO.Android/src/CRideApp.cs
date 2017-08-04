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

using Android.Gms.Analytics;
namespace IDTO.Android
{
    class CRideApp : Application
    {
#if DEBUG
        private static String PROPERTY_ID = "UA-52851928-2";
#else
        private static String PROPERTY_ID = "UA-52851928-1";
#endif
        
        public CRideApp(IntPtr handle, JniHandleOwnership transfer):base(handle, transfer)
        {

        }

        Tracker mTracker;

        public Tracker getTracker() 
        {
            if (mTracker==null)
            {
                GoogleAnalytics analytics = GoogleAnalytics.GetInstance(this);

                mTracker = analytics.NewTracker(PROPERTY_ID);

            }
            return mTracker;
        }
    }
}