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
	public class RegisterView : BaseView
	{
		private TextView tvTerms;
		private Button btnAgree;
		private Button btnCancel;
		private Button btnRegister;
		private EditText etUsername;
		private EditText etPassword;
        private EditText etFirstName;
        private EditText etLastName;
		private EditText etVerifyPassword;
		private TextView tvLogin;
		private RegisterPresenter presenter;
		private Activity activity;
		private bool needsToAcceptTerms = true;
        Dialog dialog;

        public RegisterView(Activity activity, RegisterPresenter presenter)
            : base(activity)
        {
			this.presenter = presenter;
			this.activity =	activity;
			Init ();
		
		}

		private void Init ()
		{
			if (needsToAcceptTerms) {
                showTerms();
			} else {
				activity.SetContentView (Resource.Layout.register);
				btnRegister = activity.FindViewById<Button> (Resource.Id.register_btn_register);
				etUsername = activity.FindViewById<EditText> (Resource.Id.register_et_email);
				etPassword = activity.FindViewById<EditText> (Resource.Id.register_et_password);
				etFirstName = activity.FindViewById<EditText> (Resource.Id.register_et_firstname);
				etLastName = activity.FindViewById<EditText> (Resource.Id.register_et_lastname);
				etVerifyPassword = activity.FindViewById<EditText> (Resource.Id.register_et_password_verify);
				tvLogin = activity.FindViewById<TextView> (Resource.Id.register_tv_login);
				progressBar = activity.FindViewById<ProgressBar> (Resource.Id.register_progressbar);
				btnRegister.Click += btnRegisterUser_Click;
				tvLogin.Click += tvLogin_Click;	
				ShowBusy (false);
			}
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
            needsToAcceptTerms = true;
            presenter.GoToLogin();
        }

        private void btnAgree_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
            needsToAcceptTerms = false;
            Init();
        }

		private void tvLogin_Click(object sender, EventArgs e){
			presenter.GoToLogin ();
		}

		public void btnRegisterUser_Click(object sender, EventArgs e){
			ShowBusy (true);
			this.presenter.AttemptToRegister (etUsername.Text, etPassword.Text, etVerifyPassword.Text, 

				etFirstName.Text, etLastName.Text);

		}

		public void onPasswordsDoNotMatchError(){
			alert ("Error", "Passwords Don't Match");
			ShowBusy (false);
		}

		public override void ShowBusy(bool isbusy)
		{
			base.ShowBusy (isbusy);
			SetAllEnabled (!isbusy);
			Console.WriteLine("show busy:" + isbusy);
		}

		public void onInvalidNameError ()
		{
			alert ("Missing Required Fields", "You must give a valid first and last name.");
			ShowBusy (false);
		}

		public void OnRequiredFieldsMissing ()
		{
			alert ("Missing Required Fields", "You must fill out all required fields.");
			ShowBusy (false);
		}

		public void OnRegistrationError (string message)
		{
			alert ("Registration Error", message);
			ShowBusy (false);
		}

		private void alert (string title, string message)
		{
			AlertDialog alert = new AlertDialog.Builder (activity).Create ();
			alert.SetTitle (title);
			alert.SetMessage (message);
			alert.Show ();
		}

		private void SetAllEnabled (bool enabled)
		{
			this.btnRegister.Enabled = enabled;
			this.etUsername.Enabled = enabled;
			this.etPassword.Enabled = enabled;
			this.etFirstName.Enabled = enabled;
			this.etLastName.Enabled = enabled;
			this.etVerifyPassword.Enabled = enabled;
			this.tvLogin.Enabled = enabled;
			this.btnRegister.Enabled = enabled;
			this.tvLogin.Enabled = enabled;
		}

	}
}

