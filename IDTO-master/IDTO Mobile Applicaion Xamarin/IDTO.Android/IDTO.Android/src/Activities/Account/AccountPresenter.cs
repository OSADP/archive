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
using Android.Content.PM;
using System.Threading.Tasks;

namespace IDTO.Android
{
	class AccountPresenter
	{
	
		private AccountView view;
		private Activity activity;
		private  string username = "";

		public AccountPresenter(Activity activity)
		{
			this.activity = activity;
			this.view = new AccountView(activity, this);
			PackageInfo packageInfo = activity.PackageManager.GetPackageInfo (activity.PackageName, 0);
			this.view.ShowVersion(packageInfo.VersionName, packageInfo.VersionCode);

		}

		public async void OnResume()
		{
			view.ShowBusy (true);
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);

			if (!await loginManager.IsLoggedIn ()) {			
				GoToLoginActivity ();
			} else {
				username = loginManager.GetUsername ();
				view.ShowUserInfo (username);

                AccountManager acm = new AccountManager();
                TravelerModel traveler = await acm.GetTravelerByEmail(username);

                view.ShowPromoCode(traveler.PromoCode);
			}
			view.ShowBusy (false);
		}

		public void Logout()
		{		
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
			loginManager.Logout();
			GoToLoginActivity();
		}

		public void OnMobilitySettingsChanged(MobilitySettings settings)
		{

		}

		private void GoToLoginActivity()
		{
			activity.StartActivity(typeof(LoginActivity));
			activity.Finish ();
		}

        public async Task<String> AddPromoCode(string code)
        {
            AccountManager acm = new AccountManager();
            TravelerModel traveler = await acm.GetTravelerByEmail(username);

            traveler.PromoCode = code;
            try
            {
                traveler = await acm.UpdateTraveler(traveler);
            }catch(Exception ex)
            {
                return ex.Message;
            }

            return "Set Promo Code";
        }

	}
}

