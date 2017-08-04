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
	class HomeView :BaseView
	{
		private Activity activity;
		private Button btnAccount;
		private Button btnPlanTrip;
		private Button btnScheduledTrips;
		private Button btnTripHistory;
		private TextView tvNextDuration;
		private TextView tvNextSubtitle;

		private TextView[] tvUpcomingDay = new TextView[3];
		private TextView[] tvUpcomingTime = new TextView[3];
		private TextView[] tvUpcomingTitle = new TextView[3];
		private UpcomingTripView[] upcomingTripViews = new UpcomingTripView[3];
		private HomePresenter presenter;
		private View upcomingTripsContainer;
		private View noTripsContainer;
		private ImageView ivWeatherIcon;
		private TextView tvTemperature;

        public HomeView(Activity activity, HomePresenter presenter)
            : base(activity)
        {
			this.presenter = presenter;
			this.activity = activity;
			activity.SetContentView (Resource.Layout.home_view);
			SetupButtons ();
			SetupLabelsAndIcons ();
			SetupLayouts ();
			SetupProgressBar ();
			SetWeatherVisible (false);
		}
			
		private void SetupButtons ()
		{
			this.btnAccount = activity.FindViewById<Button> (Resource.Id.home_view_btn_account);
			this.btnPlanTrip = activity.FindViewById<Button> (Resource.Id.home_view_btn_plan);
			this.btnScheduledTrips = activity.FindViewById<Button> (Resource.Id.home_view_btn_scheduled_trips);
			this.btnTripHistory = activity.FindViewById<Button> (Resource.Id.home_view_btn_history);
			this.btnAccount.Click += btnAccount_Click;
			this.btnPlanTrip.Click += btnPlanTrip_Click;
			this.btnScheduledTrips.Click += btnScheduledTrips_Click;
			this.btnTripHistory.Click += btnTripHistory_Click;
		}

		private void SetupLabelsAndIcons ()
		{
			this.ivWeatherIcon = activity.FindViewById<ImageView> (Resource.Id.home_view_iv_weather_conditions);
			this.tvTemperature = activity.FindViewById<TextView> (Resource.Id.home_view_tv_temperature);
			this.tvUpcomingDay [0] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_next_day);
			this.tvUpcomingTime [0] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_next_time);
			this.tvUpcomingTitle [0] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_next_title);
			this.tvNextSubtitle = activity.FindViewById<TextView> (Resource.Id.home_view_tv_next_subtitle);
			this.tvNextDuration = activity.FindViewById<TextView> (Resource.Id.home_view_tv_next_duration);
			this.tvUpcomingDay [1] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_day_upcoming1);
			this.tvUpcomingTime [1] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_time_upcoming1);
			this.tvUpcomingTitle [1] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_title_upcoming1);
			this.tvUpcomingDay [2] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_day_upcoming2);
			this.tvUpcomingTime [2] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_time_upcoming2);
			this.tvUpcomingTitle [2] = activity.FindViewById<TextView> (Resource.Id.home_view_tv_title_upcoming2);
		}

		private void SetupLayouts ()
		{
			this.noTripsContainer = activity.FindViewById<View> (Resource.Id.home_view_ll_no_upcoming_trips_container);
			this.upcomingTripViews [0] = new UpcomingTripView (this, null, activity.FindViewById<View> (Resource.Id.home_view_ll_next_trip), true, tvUpcomingDay [0], tvUpcomingTime [0], tvUpcomingTitle [0]);
			this.upcomingTripViews [1] = new UpcomingTripView (this, null, activity.FindViewById<View> (Resource.Id.home_view_upcoming_item1), false, tvUpcomingDay [1], tvUpcomingTime [1], tvUpcomingTitle [1]);
			this.upcomingTripViews [2] = new UpcomingTripView (this, null, activity.FindViewById<View> (Resource.Id.home_view_upcoming_item2), false, tvUpcomingDay [2], tvUpcomingTime [2], tvUpcomingTitle [2]);
			this.upcomingTripsContainer = activity.FindViewById<View> (Resource.Id.home_view_upcomingTrips);
		}

		private void SetupProgressBar()
		{
			progressBar = activity.FindViewById<ProgressBar> (Resource.Id.home_view_progressbar);
			ShowBusy (false);
		}

		public void ShowUpcomingTrips(List<Trip> upcomingTrips)
		{
			upcomingTripViews [0].Refresh (null);
			upcomingTripViews [1].Refresh (null);
			upcomingTripViews [2].Refresh (null);

			upcomingTripsContainer.Visibility = upcomingTrips.Count > 1 ? ViewStates.Visible : ViewStates.Gone;
			noTripsContainer.Visibility = upcomingTrips.Count == 0 ? ViewStates.Visible : ViewStates.Gone;
			btnScheduledTrips.Visibility = upcomingTrips.Count >3 ? ViewStates.Visible : ViewStates.Gone;

			for (int i = 0; i < upcomingTrips.Count; i++) {
				if (i < upcomingTripViews.Count ()) {
					upcomingTripViews [i].Refresh (upcomingTrips.ElementAt (i));
				}
			}

		}

		private class UpcomingTripView : Java.Lang.Object, View.IOnClickListener
		{
			private HomeView homeView;
			private Trip trip;
			private View view;
			private bool nextTrip;
			private TextView tvDay;
			private TextView tvTime;
			private TextView tvTitle;

			public UpcomingTripView(HomeView homeView, Trip trip, View view, bool nextTrip, TextView tvDay, TextView tvTime, TextView tvTitle)
			{
				this.homeView =homeView;
				this.trip = trip;
				this.view = view;
				this.nextTrip = nextTrip;
				this.tvDay = tvDay;
				this.tvTime = tvTime;
				this.tvTitle = tvTitle;
				this.view.SetOnClickListener(this);
				Refresh(trip);
			}

			public void Refresh (Trip trip)
			{
				this.trip = trip;
				if (trip == null) {
					view.Visibility = ViewStates.Gone;
					view.Enabled = false;
				} else {
					view.Visibility = ViewStates.Visible;
					view.Enabled = true;
					ShowTrip (trip, tvDay, tvTime, tvTitle);
					if (nextTrip) {
						homeView.ShowNextTrip (trip);
					}
				}
			}

			public void  OnClick(View view)
			{
				homeView.OnClickTrip (trip);
			}

			private void ShowTrip(Trip trip, TextView day, TextView time, TextView title)
			{
				day.Text = "";
				time.Text = "";
				title.Text = "";

				try{
					DateTime startDate = trip.TripStartDate;
					DateTime now = DateTime.Now;
					string dayDisplay = "";		
					string timeDisplay = startDate.ToLocalTime().ToString ("h:mm tt");
					string titleDisplay = trip.Destination;
					if (now.Day == startDate.Day) {
						dayDisplay = "Today";
					} else if (now.Day + 1== startDate.Day ) {
						dayDisplay = "Tomorrow";
					} else {
						dayDisplay = startDate.ToString ("M/dd/yy");
					}

					day.Text = dayDisplay;
					time.Text = timeDisplay;
					title.Text = titleDisplay;
				}catch{
				}
			}
		}
	
		private void ShowNextTrip (Trip trip)
		{
			tvNextDuration.Text = "";
			tvNextSubtitle.Text = "";
			try{
				TimeSpan span = trip.TripEndDate.Subtract(trip.TripStartDate);
				tvNextDuration.Text = span.Minutes + "Min";		
			}catch{
			}
			try{
					tvNextSubtitle.Text = trip.GetFirstStepString ();
			}catch{
			}
		}
			


		public void OnClickTrip (Trip trip)
		{
			presenter.OnClickUpcomingTrip (trip);
		}

		public void OnWeatherUpdate (WeatherInfo weather)
		{	
			//LogWeather (weather);
			if (weather == null) {
				SetWeatherVisible (false);

			} else {
				int weatherDrawableResourceID = GetWeatherIconResID (activity, weather);
			
				SetTemperatureF ((int)weather.TemperatureDegF);
				SetConditionsIcon (weatherDrawableResourceID);
				SetWeatherVisible (true);
			}
		}

		private void SetWeatherVisible (bool visible)
		{
			ViewStates visibility = visible ? ViewStates.Visible : ViewStates.Invisible;
			ivWeatherIcon.Visibility = visibility;
			tvTemperature.Visibility = visibility;
		}

		private int GetWeatherIconResID (Context context, WeatherInfo weather)
		{
			string iconName = weather.IconName;
			if (!String.IsNullOrEmpty(iconName) && iconName.Contains (".")) {
				string[] split = iconName.Split ('.');
				if (split.Length > 0)
					iconName = split [0];
			}
			iconName = "w" + iconName;
			return context.Resources.GetIdentifier (iconName, "drawable", activity.PackageName);
		}

		private void LogWeather (WeatherInfo weather)
		{
			if (weather == null) {
				Console.WriteLine ("WEATHER: null weather");
			} else {
				string weatherMSG = weather.IconName + weather.TemperatureDegF + " " + weather.Conditions;
				Console.WriteLine ("WEATHER: " + weatherMSG);
			}
		}

		private void SetTemperatureF(int tempF)
		{

			tvTemperature.Text = tempF.ToString() + "°F";
		}


		private void SetConditionsIcon(int iconId)
		{
			ivWeatherIcon.SetImageResource (iconId);
		}

		private void alert (string title, string message)
		{
			AlertDialog alert = new AlertDialog.Builder (activity).Create ();
			alert.SetTitle (title);
			alert.SetMessage (message);
			alert.Show ();
		}

		private void btnAccount_Click(object sender, EventArgs e)
		{
			this.presenter.OnAccountClick();
		}

		private void btnPlanTrip_Click(object sender, EventArgs e)
		{
			this.presenter.OnPlanTripClick();
		}

		private void btnScheduledTrips_Click(object sender, EventArgs e)
		{
			this.presenter.OnScheduledTripsClick();
		}

		private void btnTripHistory_Click(object sender, EventArgs e)
		{
			this.presenter.OnbtnTripHistoryClick();
		}
	}
}

