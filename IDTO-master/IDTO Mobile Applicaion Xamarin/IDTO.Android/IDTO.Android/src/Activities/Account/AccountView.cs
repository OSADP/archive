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
	class AccountView : BaseView
	{
		private AccountPresenter presenter;
		private Button btnLogout;
        private Button btnAddPromoCode;
        private EditText tvPromoCode;
		private TextView tvUsername;
		private TextView tvVersion;
		private View accountUsernameContainer;
        private Activity activity;

        public AccountView(Activity activity, AccountPresenter presenter)
            : base(activity)
        {
            this.activity = activity;

			this.presenter = presenter;
			activity.SetContentView(Resource.Layout.account);

			this.btnLogout = activity.FindViewById<Button>(Resource.Id.account_btn_logout);
			this.btnLogout.Click += btnLogout_Click;

            this.btnAddPromoCode = activity.FindViewById<Button>(Resource.Id.account_btn_addPromoCode);
            this.btnAddPromoCode.Click += btnAddPromoCode_Click;

			this.tvUsername = activity.FindViewById<TextView> (Resource.Id.account_tv_user_name);
            this.tvPromoCode = activity.FindViewById<EditText>(Resource.Id.account_tv_promo_code);
			this.tvVersion = activity.FindViewById<TextView> (Resource.Id.account_tv_version);
			this.progressBar = activity.FindViewById<ProgressBar> (Resource.Id.account_progressbar);
			this.accountUsernameContainer = activity.FindViewById<View> (Resource.Id.account_user_name_container);
			ShowUserInfo ("");
			setAllEnabled (true);
		}	

		public void ShowVersion (string versionName, int versionCode)
		{
			try{
#if DEBUG
                string versionTag = " debug";
#else
			string versionTag = "";
#endif

				tvVersion.Text = versionName+ versionTag;
			}catch(Exception e){
				Console.WriteLine(e);
			}
		}

		public override void ShowBusy (bool isbusy)
		{
			setAllEnabled (false);
			base.ShowBusy (isbusy);
			setAllEnabled (true);
		}
		public void ShowUserInfo(string username)
		{
			tvUsername.Text = username;
		}

        public void ShowPromoCode(string promocode)
        {
            if (!String.IsNullOrEmpty(promocode))
                btnAddPromoCode.Visibility = ViewStates.Gone;
            else
                btnAddPromoCode.Visibility = ViewStates.Visible;

            tvPromoCode.Text = promocode;
        }

		private void btnLogout_Click(object sender, EventArgs e)
		{
			ShowBusy (true);
			presenter.Logout();
		}

        private async void btnAddPromoCode_Click(object sender, EventArgs e)
        {
            ShowBusy(true);
            String result = await presenter.AddPromoCode(tvPromoCode.Text);
            ShowBusy(false);

            AlertDialog.Builder blder = new AlertDialog.Builder(this.activity);
            AlertDialog alertDlg = blder.Create();
            alertDlg.SetTitle("Done");
            alertDlg.SetMessage(result);
            alertDlg.SetButton("OK", (s, ev) =>
                {
                    alertDlg.Dismiss();
                });
            alertDlg.Show();

        }

		private void setAllEnabled (bool enabled)
		{
			btnLogout.Enabled = enabled;
		}
	}
}

