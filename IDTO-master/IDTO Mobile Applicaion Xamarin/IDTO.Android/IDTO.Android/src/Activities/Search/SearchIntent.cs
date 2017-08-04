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
using System.Threading.Tasks;


namespace IDTO.Android
{
	public class SearchIntent 
	{

		const string KEY_startLocation = "startLocation";
		const string KEY_endLocation = "endLocation";
		const string KEY_year = "year";
		const string KEY_month = "month";
		const string KEY_day = "day";
		const string KEY_hour = "hour";
		const string KEY_minute = "minute";
		const string KEY_isDeparture = "isDeparture";
		const string KEY_maxWalkDistance = "KEY_maxWalkDistance";
        const string KEY_cityString = "KEY_cityString";
        const string KEY_stateString = "KEY_stateString";
 
		public readonly Intent intent;
		private readonly DateTime _date;
		private readonly string _startLocation;
		private readonly string _endLocation;
		private readonly bool _isDeparture;
		private readonly bool _isSearchable;
		private readonly double _maxWalkDistance;
        private readonly String _cityString;
        private readonly String _stateString;

        //public SearchIntent(Context context, SearchIntent searchIntent, Type nextActivity) 
        //    : this(context, searchIntent._startLocation, 
        //        searchIntent._endLocation, searchIntent._date, searchIntent._isDeparture, 

        //        nextActivity, new Distance(searchIntent._maxWalkDistance, Distance.UnitsOfDistance.METERS))
        //{}

		public SearchIntent(Context context, string startLocation, string endLocation, 
			DateTime date, bool isDeparture, Type nextActivity, Distance maxWalkDistance, String cityString, String stateString)
		{
			//maxWalkDistance.ConvertTo (Distance.UnitsOfDistance.METERS);

			_startLocation = startLocation;
			_endLocation = endLocation;
			_isDeparture = isDeparture;
			_date = date;
			_isSearchable = false;
			_maxWalkDistance = maxWalkDistance.GetDistanceValue();
            _cityString = cityString;
            _stateString = stateString;

			this.intent = new Intent (context, nextActivity);
			intent.PutExtra(KEY_startLocation, startLocation);
			intent.PutExtra(KEY_endLocation, endLocation);
			intent.PutExtra(KEY_year, date.Year);
			intent.PutExtra(KEY_month, date.Month);
			intent.PutExtra(KEY_day, date.Day);
			intent.PutExtra(KEY_hour, date.Hour);
			intent.PutExtra(KEY_minute, date.Minute);
			intent.PutExtra(KEY_isDeparture, isDeparture);
			intent.PutExtra (KEY_maxWalkDistance, _maxWalkDistance);
            intent.PutExtra(KEY_cityString, _cityString);
            intent.PutExtra(KEY_stateString, _stateString);

		}

		public SearchIntent(Bundle extras)
		{
			try {
				_startLocation = extras.GetString(KEY_startLocation);
				_endLocation = extras.GetString(KEY_endLocation);
				_date = GetDate(extras);
				_isDeparture = extras.GetBoolean(KEY_isDeparture);
				_maxWalkDistance = extras.GetDouble(KEY_maxWalkDistance);
                _cityString = extras.GetString(KEY_cityString);
                _stateString = extras.GetString(KEY_stateString);

				_isSearchable = true;
			} catch (Exception ex) {
				Console.WriteLine (ex);
				_isSearchable = false;
			}

		}

		public async Task<TripSearchResult> Search()
		{
			TripSearchResult searchResult = new TripSearchResult ();

			if (_isSearchable) {
				UserTripDataManager dataManager = new UserTripDataManager ();

                searchResult = await dataManager.SearchForTrips(_startLocation, _endLocation, 
                    _maxWalkDistance, _date, _isDeparture, _cityString, _stateString);

			} 
			return searchResult;
		}
			
		private DateTime GetDate(Bundle extras){
			int year = extras.GetInt(KEY_year);
			int month = extras.GetInt(KEY_month);
			int day = extras.GetInt(KEY_day);
			int hour = extras.GetInt(KEY_hour);
			int minute  = extras.GetInt(KEY_minute);
			return new DateTime(year, month, day, hour, minute, 0);
		}
	}
}

