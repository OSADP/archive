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

	[Activity(Label = "C-Ride Home", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class HomeActivity : BaseActivity
	{

		private HomePresenter presenter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.presenter = new HomePresenter(this);
            
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
			base.OnResume();
			presenter.OnResume();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			presenter.OnPause ();
		}

       
    }
}

