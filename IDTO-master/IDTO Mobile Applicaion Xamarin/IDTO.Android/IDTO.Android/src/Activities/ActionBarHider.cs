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
	class ActionBarHider
	{
		public ActionBarHider(Activity activity)
		{
			try{
				ActionBar ab = activity.ActionBar;
				if(ab != null)
				{
					ab.Hide();
				}
			}catch(Exception e){
				Console.WriteLine (e.ToString ());
			}
		}

	}
}

