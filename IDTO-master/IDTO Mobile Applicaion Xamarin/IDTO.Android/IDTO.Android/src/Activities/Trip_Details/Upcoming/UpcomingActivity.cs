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
	[Activity (Label = "UpcomingActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]			
	public class UpcomingActivity : BaseActivity
	{
		private UpcomingPresenter presenter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.presenter = new UpcomingPresenter(this);	
		}

		protected override void OnResume()
		{
			base.OnResume();
			presenter.OnResume();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (resultCode.Equals (Result.Ok)) {
				Finish ();
			} 
		}
	}
}

