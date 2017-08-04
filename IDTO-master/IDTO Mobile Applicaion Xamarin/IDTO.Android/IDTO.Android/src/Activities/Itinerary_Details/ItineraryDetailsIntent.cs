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
	public class ItineraryDetailsIntent
	{

		public static Itinerary itinerary { get; set; }

		public static void StartTripDetails(Activity activity, Itinerary itinerary, TripSearchResult searchResult)
		{
			ItineraryDetailsIntent.itinerary = itinerary;
			Intent intent =  new Intent (activity.ApplicationContext, typeof(ItineraryDetailsActivity));
			intent.PutExtra(ItineraryDetailsActivity.KEY_startLocation, searchResult.searchCriteria.GetStartLocationString ());
			intent.PutExtra(ItineraryDetailsActivity.KEY_endLocation, searchResult.searchCriteria.GetEndLocationString ());
			activity.StartActivityForResult (intent, 1);
		}

	}
}

