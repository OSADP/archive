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
	[Activity(Label = "RegisterActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class RegisterActivity : BaseActivity
    {
		private RegisterPresenter presenter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			this.presenter = new RegisterPresenter (this);
        }


		public override void OnBackPressed ()
		{
			presenter.GoToLogin ();
		}
    }
}