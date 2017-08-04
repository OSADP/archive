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

using Android.Locations;
namespace IDTO.Android
{
	public delegate void FavoriteSelectedDelegate(string favorite);

	public class PlanPresenter : LocationPresenter
	{
		private const string DATE_KEY = "DATE_KEY";
		private const string START_LOC_KEY = "START_LOC_KEY";
		private const string END_LOC_KEY = "END_LOC_KEY";
		private const string ARRIVE_DEPART_KEY = "ARRIVE_DEPART_KEY";
		public const string CURRENT_LOCATION_LABEL = "Current Location";

		private PlanActivity activity;
		private PlanView view;
		private	global::Android.Locations.Location _currentLocation;
		private bool isDeparture = true;

		private Action<String> FavClickedAction;

		private FavoritesRepository favoriesRepository;

		public PlanPresenter(PlanActivity activity):base(activity)
		{

			this.activity = activity;
			this.view = new PlanView (activity, this);
			this.view.ShowTimeAsDeparture (isDeparture);
			this.view.SetCurrentLocationEnabled (false);
		}

		public async void OnResume()
		{

			AndroidLoginManager loginManager = AndroidLoginManager.Instance(activity.ApplicationContext);

			if (!await loginManager.IsLoggedIn())
			{
				//Display the login screen
				activity.StartActivity(typeof(LoginActivity));
			}

			view.Resume ();

		}

        public override void UpdateLocation(global::Android.Locations.Location loc)
		{
			view.SetCurrentLocationEnabled (true);
			this._currentLocation = loc;
		}

		public void SetIsDeparture(bool isDeparture)
		{
			this.isDeparture = isDeparture;
		}

		public void PlanTrip (string startLocation, string endLocation, 
			DateTime date, Distance maxWalkDistance)
		{
			view.ShowBusy (true);
			if (startLocation.Equals (CURRENT_LOCATION_LABEL)) {
				if (_currentLocation == null) {
					return;
				} else {
					startLocation = _currentLocation.Latitude + "," + _currentLocation.Longitude;
				}
			}
			if (endLocation.Equals (CURRENT_LOCATION_LABEL)) {
				if (_currentLocation == null) {
					return;
				} else {
					endLocation = _currentLocation.Latitude + "," + _currentLocation.Longitude;
				}
			}

			if (string.IsNullOrEmpty (startLocation) || string.IsNullOrEmpty (endLocation)) {
				view.OnEmptyLocationError ();
				return;
			}

			Search (startLocation, endLocation, date, isDeparture, maxWalkDistance);
			view.ShowBusy (false);
		}

		private async void Search (string startLocation, string endLocation, DateTime date, bool isDeparture, Distance maxWalkDistance)
		{
            string city = "";
			string state = "";
			try{
				IList<Address> addressesNearMyLocation = await new Geocoder(activity).GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 1);
				Address addressNearMyLocation = addressesNearMyLocation.First();
				if (addressNearMyLocation != null) 
				{
                    try{
                        city = addressNearMyLocation.Locality;
                        state = addressNearMyLocation.AdminArea;
				
			        }catch{
			        }
				}
			}catch{
			}
			SearchIntent searchIntent = new SearchIntent (activity.ApplicationContext, 
				startLocation, endLocation, date, isDeparture, typeof(SearchActivity), maxWalkDistance, city, state);
			//activity.StartActivity (searchIntent.intent);
            activity.StartActivityForResult(searchIntent.intent,1);
		}

		public void OnSaveInstanceState (Bundle outState)
		{
			outState.PutLong (DATE_KEY, view.GetDate());
			outState.PutString (START_LOC_KEY, view.GetStartLocation());
			outState.PutString (END_LOC_KEY, view.GetEndLocation());
			outState.PutBoolean (ARRIVE_DEPART_KEY, this.isDeparture);
		}

		public void OnRestoreInstanceState (Bundle savedInstanceState)
		{
			view.SetDate (savedInstanceState.GetLong (DATE_KEY));
			view.SetStartLocation (savedInstanceState.GetString (START_LOC_KEY));
			view.SetEndLocation (savedInstanceState.GetString (END_LOC_KEY));
			view.ShowTimeAsDeparture (savedInstanceState.GetBoolean (ARRIVE_DEPART_KEY));
		} 

		public void OnSaveFavorite(String favoriteLocation)
		{
			FavoriteLocation favLoc = new FavoriteLocation ();
			favLoc.Location = favoriteLocation;
			FavoritesRepository.SaveFavoriteLocation (favLoc);

			var builder = new AlertDialog.Builder (activity);
			builder.SetTitle ("Favorites");
			builder.SetMessage ("Saved to favorites");
			builder.SetNegativeButton ("OK", (object sender, DialogClickEventArgs e) => {
				(sender as Dialog).Cancel();
			});
			builder.Create().Show();
		}

		public void ShowFavoritesList(Action<String> stringAction)
		{
			FavClickedAction = stringAction;

			List<FavoriteLocation> favorites = FavoritesRepository.GetFavoriteLocations () as List<FavoriteLocation>;

			List<String> favStringArray = new List<string> ();

			foreach (FavoriteLocation fav in favorites) {
				favStringArray.Add (fav.Location);
			}

			var builder = new AlertDialog.Builder (activity);
			builder.SetTitle ("Favorites");
			builder.SetItems (favStringArray.ToArray(), HandleFavClickedStart);
			builder.SetNegativeButton ("Cancel", (object sender, DialogClickEventArgs e) => {
				(sender as Dialog).Cancel();
			});
			builder.Create().Show();
		}

		private void HandleFavClickedStart (object sender, DialogClickEventArgs e)
		{
			List<FavoriteLocation> favorites = FavoritesRepository.GetFavoriteLocations () as List<FavoriteLocation>;

			if (FavClickedAction != null)
				FavClickedAction (favorites [e.Which].Location);
				
		}
	}
}

