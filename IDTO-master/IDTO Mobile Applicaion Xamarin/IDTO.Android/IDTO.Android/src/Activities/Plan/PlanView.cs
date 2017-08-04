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
using Android.Views.InputMethods;

using IDTO.Common.Models;

namespace IDTO.Android
{

	public class PlanView : BaseView
	{

		private PlanPresenter presenter;

		private Button btnPlan;
		private Button btnDeparture;
		//private Button btnTimeAndDateOK;
		private Button btnArrival;
		private EditText etStartLocation;
		private EditText etEndLocation;
		private EditText etTime;
		private EditText etDate;
		private LinearLayout llTime;
		private ImageButton btnStartLocationUseCurrent;
		private ImageButton btnEndLocationUseCurrent;

		private ImageButton btnShowFavoritesStart;
		private ImageButton btnShowFavoritesEnd;

		private ImageButton btnSaveFavStart;
		private ImageButton btnSaveFavEnd;

		private Color orange;
		private Color white;
		private CalendarView datePicker;

		//private DateTimePicker timePicker;
		DateTime dtNowPlus5;
		private Activity activity;

		private LinearLayout llStartAndEnd;

		private DistanceSpinnerAdapter maxWalkDistanceSpinnerAdapter;
		private Spinner maxWalkDistanceSpinner;
		private Distance maxWalkDistance;

        public PlanView(Activity activity, PlanPresenter presenter)
            : base(activity)
		{
			this.activity = activity;
			this.presenter = presenter;
			this.activity.SetContentView (Resource.Layout.plan);
			this.orange = this.activity.Resources.GetColor (Resource.Color.idto_orange);
			this.white = Color.White;
			this.maxWalkDistanceSpinnerAdapter = new DistanceSpinnerAdapter (activity.LayoutInflater, activity);

			maxWalkDistance = Distance.GetPredefinedDefault ();



			maxWalkDistanceSpinner = activity.FindViewById<Spinner>(Resource.Id.plan_spinner_max_walk_distance);
			maxWalkDistanceSpinner.Adapter = this.maxWalkDistanceSpinnerAdapter;

			maxWalkDistanceSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
				maxWalkDistance = (Distance)(maxWalkDistanceSpinnerAdapter.GetDistanceAtPosition(e.Position));

			};

			btnPlan = activity.FindViewById<Button>(Resource.Id.plan_btn_search);
			//btnTimeAndDateOK = activity.FindViewById<Button>(Resource.Id.plan_btn_time_and_date_ok);
			btnStartLocationUseCurrent = activity.FindViewById<ImageButton>(Resource.Id.plan_ib_use_current_loation_start);
			btnEndLocationUseCurrent = activity.FindViewById<ImageButton>(Resource.Id.plan_ib_use_current_loation_end);

			btnShowFavoritesStart = activity.FindViewById<ImageButton> (Resource.Id.plan_ib_show_favorites_start);
			btnShowFavoritesEnd = activity.FindViewById<ImageButton> (Resource.Id.plan_ib_show_favorites_end);

			btnSaveFavStart = activity.FindViewById<ImageButton> (Resource.Id.plan_ib_star_favorite_start);
			btnSaveFavEnd = activity.FindViewById<ImageButton> (Resource.Id.plan_ib_star_favorite_end);

			etStartLocation= activity.FindViewById<EditText>(Resource.Id.plan_et_start);
			etEndLocation= activity.FindViewById<EditText>(Resource.Id.plan_et_end);
			etTime= activity.FindViewById<EditText>(Resource.Id.plan_et_time);
			etDate = activity.FindViewById<EditText> (Resource.Id.plan_et_date);
			llTime= activity.FindViewById<LinearLayout>(Resource.Id.plan_ll_time);

			llStartAndEnd = activity.FindViewById<LinearLayout> (Resource.Id.plan_ll_start_and_end);

			//Time
			dtNowPlus5 = NowPlus5Minutes();

			btnDeparture =  activity.FindViewById<Button>(Resource.Id.plan_toggle_btn_departure);
			btnArrival = activity.FindViewById<Button>(Resource.Id.plan_toggle_btn_arrival);
			btnDeparture.Click += btnDeparture_Click;
			btnArrival.Click += btnArrival_Click;

			etTime.Focusable = true;
			etTime.FocusChange += OnTime_FocusChanged;
			etDate.Focusable = true;

			etDate.FocusChange += OnDate_FocusChanged;

			btnStartLocationUseCurrent.Click += btnStartLocationUseCurrent_Click;
			btnEndLocationUseCurrent.Click += btnEndLocationUseCurrent_Click;

			btnSaveFavStart.Click += (object sender, EventArgs e) => {
				presenter.OnSaveFavorite(etStartLocation.Text);
			};

			btnSaveFavEnd.Click += (object sender, EventArgs e) => {
				presenter.OnSaveFavorite(etEndLocation.Text);
			};

			btnShowFavoritesStart.Click += (object sender, EventArgs e) => {
				presenter.ShowFavoritesList(SetFavStart);
			};

			btnShowFavoritesEnd.Click += (object sender, EventArgs e) => {
				presenter.ShowFavoritesList(SetFavEnd);
			};


			btnPlan.Click += btnPlan_Click;
			Enable ();
			ShowBusy (false);

			setDateAndTimeField (dtNowPlus5);
		}

		public void Resume()
		{
			Enable ();
		}

		private void Enable()
		{
			SetEnabledValues(true);
		}
		private void Disable()
		{
			SetEnabledValues(false);
		}

		private void SetEnabledValues(bool enabled)
		{
			btnPlan.Enabled = enabled;
			btnArrival.Enabled = enabled;
			btnDeparture.Enabled = enabled;
			maxWalkDistanceSpinner.Enabled = enabled;
			btnStartLocationUseCurrent.Enabled = enabled;
			btnEndLocationUseCurrent.Enabled = enabled;
		}

		public void OnErrorFindingLocation ()
		{
			Toast.MakeText (activity, "Error in finding current location", ToastLength.Long).Show ();
			Toast.MakeText (activity, "Attempting to find location by other methods", ToastLength.Long).Show ();
			etStartLocation.RequestFocus ();
			Enable ();
		}

		private DateTime NowPlus5Minutes()
		{
			DateTime dtNow = DateTime.Now;
			int minutes = dtNow.Minute;

			int modVal = minutes % 5;

			modVal = 5 - modVal;

			if (modVal < 5)
				modVal = modVal + 5;

			DateTime dt = DateTime.Now.AddMinutes (modVal);

			return dt;
		}

		private void OnTime_FocusChanged(object sender, View.FocusChangeEventArgs e)
		{
			if (e.HasFocus) {
				DateTime dt = DateTime.Parse (etDate.Text + " " + etTime.Text);
				TimePickerDialogIntervals dlg = new TimePickerDialogIntervals (this.activity, HandleTimeSet, dt.Hour, dt.Minute, false);
				dlg.Show ();
			}
		}
			
		void HandleTimeSet (object sender, TimePickerDialog.TimeSetEventArgs e)
		{
			DateTime dt = DateTime.Parse (etDate.Text + " " + e.HourOfDay.ToString() + ":" + e.Minute);

			setDateAndTimeField (dt);
			etTime.ClearFocus ();
		}

		private void OnDate_FocusChanged(object sender, View.FocusChangeEventArgs e)
		{
			if (e.HasFocus) {
				DateTime dt = DateTime.Parse (etDate.Text + " " + etTime.Text);
				DatePickerDialog dlg = new DatePickerDialog (this.activity, HandleDateSet, dt.Year,
					                      dt.Month - 1, dt.Day);
				dlg.Show ();
			}
		}

		void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			DateTime dtOld = DateTime.Parse (etDate.Text + " " + etTime.Text);
			DateTime dt = new DateTime(e.Date.Year, e.Date.Month, e.Date.Day, 
				dtOld.Hour, dtOld.Minute, dtOld.Second, DateTimeKind.Local);
				
			setDateAndTimeField (dt);
			etDate.ClearFocus ();
		}

		private void setDateAndTimeField(DateTime dtNew)
		{
			DateTime dtTimeToSet = dtNew;
			if (dtNew.CompareTo (dtNowPlus5) < 0) {
				dtTimeToSet = dtNowPlus5;
			}

			etTime.Text = dtTimeToSet.ToString ("h:mm tt");
			etDate.Text = dtTimeToSet.ToString ("M/d/yyyy");
		}

		private void btnDeparture_Click(object sender, EventArgs e)
		{
			presenter.SetIsDeparture (true);
			ShowTimeAsDeparture (true);
		}

		private void btnArrival_Click(object sender, EventArgs e)
		{
			presenter.SetIsDeparture (false);
			ShowTimeAsDeparture (false);
		}

		public void ShowTimeAsDeparture(bool isDeparture)
		{
			if (isDeparture) {
				btnDeparture.SetBackgroundColor (orange);
				btnDeparture.SetTextColor (white);
				btnArrival.SetBackgroundColor (white);
				btnArrival.SetTextColor (orange);
			} else {
				btnDeparture.SetBackgroundColor (white);
				btnDeparture.SetTextColor (orange);
				btnArrival.SetBackgroundColor (orange);
				btnArrival.SetTextColor (white);
			}
		}

		private void SetStartAsCurrentLocation ()
		{
			SetStartAsCurrentLocation (true);
		}

		private void SetEndAsCurrentLocation ()
		{
			SetEndAsCurrentLocation (true);
		}

		private void SetStartAsCurrentLocation (bool mutuallyExclusive)
		{
			etStartLocation.Text = PlanPresenter.CURRENT_LOCATION_LABEL;
			if (mutuallyExclusive && etEndLocation.Text.Equals (PlanPresenter.CURRENT_LOCATION_LABEL)) {
				etEndLocation.Text = "";
			}
		}

		private void SetEndAsCurrentLocation (bool mutuallyExclusive)
		{
			etEndLocation.Text = PlanPresenter.CURRENT_LOCATION_LABEL;
			if (mutuallyExclusive && etStartLocation.Text.Equals (PlanPresenter.CURRENT_LOCATION_LABEL)) {
				etStartLocation.Text = "";
			}
		}

		private void btnStartLocationUseCurrent_Click(object sender, EventArgs e)
		{
			SetStartAsCurrentLocation ();
		}

		private void btnEndLocationUseCurrent_Click(object sender, EventArgs e)
		{
			SetEndAsCurrentLocation ();
		}

		/*private DateTime GetDateAndTime ()
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime date = epoch.AddMilliseconds (datePicker.Date);
			DateTime time = dtNowPlus5
			date = new DateTime (date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
			return date;
		}*/

		private void btnPlan_Click(object sender, EventArgs e)
		{
			Disable ();
			DateTime date = DateTime.Parse (etDate.Text + " " + etTime.Text);

			this.presenter.PlanTrip (etStartLocation.Text, etEndLocation.Text, 
				date, maxWalkDistance); 
		}
	
		public void OnEmptyLocationError ()
		{
			Toast.MakeText (activity, "Start and end location are required", ToastLength.Long).Show ();
			Enable ();
		}

		public void SetCurrentLocationEnabled(bool isEnabled)
		{
			if (isEnabled) {
				btnStartLocationUseCurrent.Visibility = ViewStates.Visible;
				btnEndLocationUseCurrent.Visibility = ViewStates.Visible;
			} else {
				btnStartLocationUseCurrent.Visibility = ViewStates.Invisible;
				btnEndLocationUseCurrent.Visibility = ViewStates.Invisible;
			}
		}

		public long GetDate(){
			return datePicker.Date;
		}

		public string GetStartLocation(){
			return etStartLocation.Text;
		}

		public string GetEndLocation(){
			return etEndLocation.Text;
		}

		public void SetDate(long dateL)
		{
		}

		public void SetStartLocation(String value){
			etStartLocation.Text = value;
		}

		public void SetEndLocation(String value){
			etEndLocation.Text = value;
		}

		public void SetFavStart(String fav)
		{
			etStartLocation.Text = fav;
		}

		public void SetFavEnd(String fav)
		{
			etEndLocation.Text = fav;
		}
	}
}

