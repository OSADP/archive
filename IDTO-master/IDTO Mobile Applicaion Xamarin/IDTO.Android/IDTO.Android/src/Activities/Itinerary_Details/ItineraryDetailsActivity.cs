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

using IDTO.Common;

namespace IDTO.Android
{
	[Activity(Label = "ItineraryDetailsActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class ItineraryDetailsActivity : BaseActivity
    {
		public const string KEY_startLocation = "KEY_startLocation";
		public const string KEY_endLocation = "KEY_endLocation";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			new ItineraryDetailsPresenter (this, ItineraryDetailsIntent.itinerary, Intent.Extras);
        }

    }
}