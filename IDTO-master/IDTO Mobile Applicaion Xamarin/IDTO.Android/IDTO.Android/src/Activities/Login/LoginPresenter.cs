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
	class LoginPresenter
	{
		private BaseActivity activity;
		private LoginView view;

		public LoginPresenter(BaseActivity activity)
		{
			this.activity = activity;
			this.view = new LoginView (activity, this);
		}

		public async void OnAttemptLogin(string email, string password)
		{
			try{
				if (string.IsNullOrEmpty (email) || string.IsNullOrEmpty (password)) {
					view.OnLoginError ("You must enter a username and password");
				}else{
					AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
					LoginResult loginResult = await loginManager.Login(email, password);				
				
					view.ShowBusy (false);

                    activity.sendGaEvent("ui_action", "user login", "login result", Convert.ToInt16(loginResult.Success));
					if(loginResult.Success)
					{

                        AccountManager acm = new AccountManager();
                        TravelerModel traveler = await acm.GetTravelerByEmail(email);

                        if(traveler.InformedConsent)
                        {
                            Intent intent = new Intent(activity.ApplicationContext, typeof(HomeActivity));
                            activity.StartActivity(intent);
                            activity.Finish();
                        }
                        else
                        {
                            view.showTerms();
                        }


					} else {
						view.OnLoginError (loginResult.ErrorString);			
					}
				}
			}catch (Exception e) {
				Console.WriteLine (e);
				view.OnLoginError ("Login failed");		
			}

		}

        public async void accpetTerms()
        {
            AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
            string email = loginManager.GetUsername ();

            AccountManager acm = new AccountManager();
            TravelerModel traveler = await acm.GetTravelerByEmail(email);
            traveler.InformedConsent = true;
            traveler.InformedConsentDate = DateTime.UtcNow;
            traveler = await acm.UpdateTraveler(traveler);

            Intent intent = new Intent(activity.ApplicationContext, typeof(HomeActivity));
            activity.StartActivity(intent);
            activity.Finish();
        }

        public void rejectTerms()
        {
            AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);
            loginManager.Logout();

        }

		public void Register ()
		{
			activity.StartActivity(typeof(RegisterActivity)); 
			activity.Finish ();
		}
	}
}

