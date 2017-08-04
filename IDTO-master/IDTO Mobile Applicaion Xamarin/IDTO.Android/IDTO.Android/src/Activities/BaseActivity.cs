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

using Android.Gms.Analytics;
using IDTO.Android;

namespace IDTO.Android
{
	[Activity (Label = "BaseActivity")]			
	public class BaseActivity : Activity
	{

        private Tracker mGATracker;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			new ActionBarHider (this);
		}

        protected override void OnResume()
        {
            mGATracker = ((CRideApp)this.Application).getTracker();
            mGATracker.SetScreenName(this.GetType().Name);

            var build = new HitBuilders.AppViewBuilder().Build();

            mGATracker.Send(build.ToDictionary());

            base.OnResume();
        }

        public void sendGaEvent(String action, String category, String label, long? value)
        {
            var gaEvent = new HitBuilders.EventBuilder();
            gaEvent.SetAction(action);
            gaEvent.SetCategory(category);
            gaEvent.SetLabel(label);

            if(value.HasValue)
                gaEvent.SetValue(value.Value);

            var build = gaEvent.Build();

            mGATracker.Send(build.ToDictionary());
        }
	}
}

