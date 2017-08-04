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
using IDTO.Mobile.Manager;
using IDTO.Common.Models;

namespace IDTO.Android
{

	public class SearchPresenter
	{
		private SearchView view;
        private BaseActivity activity;
		private SearchIntent searchIntent;
		private TripSearchResult searchResult;

        public SearchPresenter(BaseActivity activity, Bundle extras)
		{
			this.activity = activity;
			this.view =	new SearchView(activity, this);
			UserTripDataManager dataManager = new UserTripDataManager();
			searchIntent = new SearchIntent (extras);
			SearchAndDisplayResults (searchIntent);
		}

		public async void SearchAndDisplayResults(SearchIntent searchIntent){
			this.view.ShowBusy (true);
            
			try{
			    searchResult = await searchIntent.Search();
			}catch(Exception e){
                Console.WriteLine(e.Message);
				searchResult = null;
			}

            if(searchResult!=null)
                activity.sendGaEvent("ui_action", "search trips", "search results", searchResult.itineraries.Count);
            else
                activity.sendGaEvent("ui_action", "search trips", "search results", -1);
			this.view.ShowSearchResult(searchResult);
			this.view.ShowBusy (false);
		}

		public async void OnResume()
		{
			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);

			if (!await loginManager.IsLoggedIn())
			{
				//Display the login screen
				activity.StartActivity(typeof(LoginActivity));
			}
		}

		public void OnItinerarySelected(Itinerary itinerary)
		{
            activity.sendGaEvent("ui_action", "trip details", "trip details", null);
			ItineraryDetailsIntent.StartTripDetails(activity, itinerary, searchResult);
		}
				
	}
}

