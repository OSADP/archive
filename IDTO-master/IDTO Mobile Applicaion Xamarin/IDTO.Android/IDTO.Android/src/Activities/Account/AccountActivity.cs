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
	[Activity (Label = "AccountActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]			
	public class AccountActivity : BaseActivity
	{
		private AccountPresenter presenter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.presenter = new AccountPresenter (this);
		}


		protected  override void OnResume()
		{
			base.OnResume();
			presenter.OnResume();
		}

		public  override void OnBackPressed()
		{
			Finish ();
		}
	}
}

