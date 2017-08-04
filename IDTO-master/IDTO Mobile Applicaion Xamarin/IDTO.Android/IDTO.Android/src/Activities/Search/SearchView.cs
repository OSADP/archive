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
	class SearchView : BaseView
	{
		private SearchPresenter presenter;
		private ListView listViewResults;
		private TripSearchResultAdapter tripSearchAdapter;
		private TextView tvNoResultsFound;

        public SearchView(Activity activity, SearchPresenter presenter)
            : base(activity)
		{
			this.presenter = presenter;
			activity.SetContentView (Resource.Layout.search_results);
			this.tvNoResultsFound = activity.FindViewById<TextView> (Resource.Id.search_result_tv_no_results);
			this.tvNoResultsFound.Visibility = ViewStates.Gone;
			this.progressBar = activity.FindViewById<ProgressBar> (Resource.Id.search_result_progressbar);
			this.tripSearchAdapter = new TripSearchResultAdapter (activity.LayoutInflater);

			this.listViewResults = activity.FindViewById<ListView>(Resource.Id.search_results_list_view);

			this.listViewResults.Adapter = this.tripSearchAdapter;
			this.listViewResults.ItemClick += (object sender, AdapterView.ItemClickEventArgs args) 
				=> OnItinerarySelected(sender, args);

			ShowBusy (true);
		}

		public void ShowSearchResult(TripSearchResult result){
			tripSearchAdapter.Update (result);
			if (tripSearchAdapter.Count == 0) {
				this.tvNoResultsFound.Visibility = ViewStates.Visible;
			}
			ShowBusy (false);
		}

		private void OnItinerarySelected(object sender, AdapterView.ItemClickEventArgs e)
		{
			Itinerary itinerary = (Itinerary)tripSearchAdapter.GetItineraryAtPosition (e.Position);
			presenter.OnItinerarySelected(itinerary);
		}

	
	}
}

