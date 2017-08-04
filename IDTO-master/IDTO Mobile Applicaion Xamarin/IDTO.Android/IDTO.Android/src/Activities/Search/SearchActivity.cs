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
	[Activity (Label = "SearchActivity")]			
	public class SearchActivity : BaseActivity
	{
		private SearchPresenter presenter;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			this.presenter = new SearchPresenter(this, Intent.Extras);

	
		}

		protected override void OnResume()
		{
			base.OnResume();
			presenter.OnResume();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			if (resultCode.Equals (Result.Ok)) {
                SetResult(Result.Ok);
				Finish ();
			} 
		}
	}
}

