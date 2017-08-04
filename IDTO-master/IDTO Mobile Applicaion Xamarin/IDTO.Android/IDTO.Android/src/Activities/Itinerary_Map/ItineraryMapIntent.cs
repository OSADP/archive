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
namespace IDTO.Android
{
	public class ItineraryMapIntent
	{

		public static Itinerary itinerary { get; set; }

		public static void StartMapIntent(Activity activity, Itinerary itinerary)
		{
			ItineraryMapIntent.itinerary = itinerary;
			Intent intent =  new Intent (activity.ApplicationContext, typeof(ItineraryMapActivity));
            activity.StartActivityForResult (intent, 1);
		}

	}
}

