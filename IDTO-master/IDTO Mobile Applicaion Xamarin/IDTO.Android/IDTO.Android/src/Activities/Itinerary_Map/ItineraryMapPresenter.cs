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
using IDTO.Mobile.Manager;

namespace IDTO.Android
{
	public class ItineraryMapPresenter
	{
		private Itinerary itinerary;
		private ItineraryMapView view;
		private Activity activity;

        public ItineraryMapPresenter(Activity activity, Itinerary itinerary, Bundle extras)
        {
			this.activity = activity;
			this.itinerary = itinerary;	
			this.view = new ItineraryMapView (activity, this, this.itinerary);
		}
	}
}

