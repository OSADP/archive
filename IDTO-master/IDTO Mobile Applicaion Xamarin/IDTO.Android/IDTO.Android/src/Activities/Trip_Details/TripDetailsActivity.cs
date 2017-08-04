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
using IDTO.Common.Models;

namespace IDTO.Android
{
	[Activity(Label = "TripDetailsActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class TripDetailsActivity : BaseActivity
    {
		public static Trip trip { get; set; }
		public  const string KEY_CANCELABLE = "KEY_CANCELABLE";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			new TripDetailsPresenter (this, TripDetailsActivity.trip, Intent.Extras);
        }
		 
    }
}