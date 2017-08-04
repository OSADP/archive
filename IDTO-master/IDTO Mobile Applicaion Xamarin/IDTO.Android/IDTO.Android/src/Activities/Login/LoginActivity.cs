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
	[Activity(Label = "LoginActivity", ScreenOrientation=global::Android.Content.PM.ScreenOrientation.Portrait)]
	public class LoginActivity : BaseActivity
    {
	
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			new LoginPresenter (this);
		

        }
   
		protected override void OnResume ()
		{
			base.OnResume ();
			HomePresenter.UnRegisterGCM (this);
		}
    }
}