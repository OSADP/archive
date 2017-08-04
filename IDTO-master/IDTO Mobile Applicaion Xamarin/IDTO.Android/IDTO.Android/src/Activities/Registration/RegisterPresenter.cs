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
using IDTO.Common.Models;
using System.Threading.Tasks;

namespace IDTO.Android
{
	public class RegisterPresenter
	{


		private RegisterView view;
		private BaseActivity activity;

		public RegisterPresenter(BaseActivity activity){
			this.activity = activity;
			this.view = new RegisterView (activity, this);
		}

		public async void AttemptToRegister(string username, string password, string password_verify, string firstname, string lastname)
		{
			try{
				await Register (username, password, password_verify, firstname, lastname);
			}catch(Exception e) {
				Console.WriteLine (e);
				view.OnRegistrationError ("There was a problem with your registraion.");
				GoToLogin ();
			}
		}

		private async Task Register (string username, string password, string password_verify, string firstname, string lastname)
		{
			AndroidLoginManager loginManager = AndroidLoginManager.Instance (activity.ApplicationContext);
			//LoginResult loginResult = await loginManager.Register (username, password, firstname, lastname);
			LoginResult loginResult = await new RegistrationApplicant (username, password, password_verify, firstname, lastname).Register (loginManager);

            activity.sendGaEvent("ui_action", "register user", "register result", Convert.ToInt16(loginResult.Success));

            if (loginResult.Success) {
				LoginAndGoHome (username, password);
			} else {
				view.OnRegistrationError (loginResult.ErrorString);
			}
		}

		private async  void LoginAndGoHome (string username, string password)
		{
			AndroidLoginManager loginManager = AndroidLoginManager.Instance (activity.ApplicationContext);
			if (await loginManager.IsLoggedIn ()) {
				GoToHome ();
			}
			else {
				LoginResult lr = await loginManager.Login (username, password);
				if (lr.Success)
					GoToHome ();
				else
					GoToLogin ();
			}
		}

		private void GoToHome ()
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(HomeActivity));
			activity.StartActivity (intent);
			activity.Finish ();
		}

		public void GoToLogin ()
		{
			Intent intent = new Intent (activity.ApplicationContext, typeof(LoginActivity));
			activity.StartActivity (intent);
			activity.Finish ();
		}
	}
}

