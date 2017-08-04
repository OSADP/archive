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

using Android.Graphics;

namespace IDTO.Android
{
	public class BaseView
	{
		protected ProgressBar progressBar;
        protected Typeface RobotoThin;
        protected Typeface RobotoLight;
        protected Typeface RobotoRegular;
        public BaseView(Activity activity)
        {
            RobotoThin = Typeface.CreateFromAsset(activity.Assets, "Roboto-Thin.ttf");
            RobotoLight = Typeface.CreateFromAsset(activity.Assets, "Roboto-Light.ttf");
            RobotoRegular = Typeface.CreateFromAsset(activity.Assets, "Roboto-Regular.ttf");
            
        }
		public virtual void ShowBusy(bool isbusy)
		{
			if (progressBar != null) {
				if(isbusy){					
					progressBar.Visibility = ViewStates.Visible; 
				} else {
					progressBar.Visibility =  ViewStates.Invisible; 
				}
			}

		}
	}
}

