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
using Android.Webkit;
using IDTO.Common.Models;

namespace IDTO.Android
{
	class LoginView : BaseView
	{
		const string ERROR_MSG = "Error";

		private Button btnLogin;
		private Button btnRegister;
		private EditText etEmail;
		private EditText etPassword;
		private LoginPresenter presenter;
		private Activity activity;
        Dialog dialog;

		public LoginView(Activity activity, LoginPresenter presenter):base(activity)
		{
			this.activity = activity;
			this.presenter = presenter;
			this.activity.SetContentView(Resource.Layout.login);

			this.btnRegister = activity.FindViewById<Button>(Resource.Id.btnRegister);

			this.btnRegister.Click += delegate {
				presenter.Register();
			};
			this.etEmail = activity.FindViewById<EditText>(Resource.Id.txtLoginEmailAddress);
			this.etPassword = activity.FindViewById<EditText>(Resource.Id.txtLoginPassword);
			this.btnLogin = activity.FindViewById<Button>(Resource.Id.btnLogin);
            
			this.btnLogin.Click += btnLogin_Click;

			//btnLogin.PerformClick ();
			this.progressBar = activity.FindViewById<ProgressBar> (Resource.Id.login_view_progressbar);
			ShowBusy (false);


            
		}

        public void showTerms()
        {
            dialog = new Dialog(activity);
            dialog.SetContentView(Resource.Layout.terms);
            dialog.SetTitle("C-Ride Terms and Conditions");

            WebView webView = dialog.FindViewById<WebView>(Resource.Id.terms_web_view);
            webView.LoadUrl("http://idtotravelerportal.azurewebsites.net/Content/Terms/Terms.html");

            Button cancelButton = dialog.FindViewById<Button>(Resource.Id.terms_btn_cancel);
            Button acceptButton = dialog.FindViewById<Button>(Resource.Id.terms_btn_agree);

            cancelButton.Click += btnCancel_Click;
            acceptButton.Click += btnAgree_Click;

            dialog.Show();
        }

		public override void ShowBusy(bool isbusy)
		{
			base.ShowBusy (isbusy);
			SetAllEnabled (!isbusy);
		}

		private void SetAllEnabled (bool enabled)
		{
			this.btnRegister.Enabled = enabled;
			this.etEmail.Enabled = enabled;
			this.etPassword.Enabled = enabled;
			this.btnLogin.Enabled = enabled;
		}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
            presenter.rejectTerms();
        }

        private void btnAgree_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
            presenter.accpetTerms();
        }

		private void btnLogin_Click(object sender, EventArgs e)
		{
			btnLogin.Enabled = false;
			btnRegister.Enabled = false;
			ShowBusy (true);
			presenter.OnAttemptLogin (etEmail.Text, etPassword.Text);
		}

		private void reset ()
		{
			btnLogin.Enabled = true;
			btnRegister.Enabled = true;
			etEmail.Text = "";
			etPassword.Text = "";
			ShowBusy (false);
		}

		public void OnLoginError(string errorString)
		{
			AlertDialog alert = new AlertDialog.Builder(activity).Create();
			alert.SetTitle (ERROR_MSG);
			alert.SetMessage(errorString);
			alert.Show();
			reset ();
		}
	}
}

