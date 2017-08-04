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
	[Activity(Label = "ItineraryMapActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class ItineraryMapActivity : BaseActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			new ItineraryMapPresenter (this, ItineraryMapIntent.itinerary, Intent.Extras);
        }

    }
}