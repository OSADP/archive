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

namespace IDTO.Android
{
	[Activity(Label = "C-Ride", MainLauncher = true, ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class SplashActivity : BaseActivity
	{
		bool clicked =false;
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			clicked = false;
			SetContentView (Resource.Layout.splash);

			new Handler ().PostDelayed (Click, 3000);
		}

		private void Click ()
		{
			if (clicked)
				return;
			clicked = true;
			GoToHome ();
			return;
		}

		public override bool OnTouchEvent (MotionEvent e)
		{
			Click ();
			return false;
		}	

		private void GoToHome ()
		{
			Intent intent = new Intent (ApplicationContext, typeof(HomeActivity));
			StartActivity (intent);
			Finish ();
		}
	}
}

