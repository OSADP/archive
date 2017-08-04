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
using Android.Util;

namespace IDTO.Android
{

	[Activity (Label = "PlanActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]			
	public class PlanActivity : BaseActivity
	{

		//private String _locationProvider;
		private PlanPresenter presenter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.presenter = new PlanPresenter (this);

		}

        protected override void OnStart()
        {
            base.OnStart();
            presenter.OnStart();
        }

        protected override void OnStop()
        {
            presenter.OnStop();
            base.OnStop();
        }

		protected override void OnResume()
		{
			presenter.OnResume ();
			base.OnResume();

		}

		protected override void OnPause()
		{
			base.OnPause();

		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode.Equals(Result.Ok))
            {
                Finish();
            }
        }

		protected override void OnSaveInstanceState (Bundle outState)
		{
			base.OnSaveInstanceState (outState);
		}

		protected override void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			base.OnRestoreInstanceState (savedInstanceState);
		}
	}
}

