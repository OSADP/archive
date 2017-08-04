using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;

using WindowsAzure.Messaging;

using IDTO.Common;
using IDTO.Common.Models;

using GoogleAnalytics.iOS;

namespace IDTO.iPhone
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		public override UIWindow Window {
			get;
			set;
		}

		private SBNotificationHub Hub { get; set; }
		private NSData DeviceToken { get; set; }

		private UIAlertView NotificationAlertView;
		private CLLocationManager LocationManager;
		private DateTime LastSentLocationTime;
		private DateTime FirstSentLocationTime;
		private int DurationToSendLocation_Sec;
		public FavoritesDbManager FavoriteLocations;

		public IGAITracker Tracker;

		#if DEBUG
		public static readonly string TrackingId = "UA-52851928-2";
		#else
		public static readonly string TrackingId = "UA-52851928-1";
		#endif


		//private int backgroundTaskId;

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			FavoriteLocations = new FavoritesDbManager(FavoritesDbManager.DatabaseFilePath);

			UINavigationBar.Appearance.SetBackgroundImage(new UIImage(),UIBarMetrics.Default);
			UINavigationBar.Appearance.ShadowImage = new UIImage ();
			UINavigationBar.Appearance.BackgroundColor = UIColor.Clear;
			UINavigationBar.Appearance.TintColor = UIColor.White;

			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);



			DurationToSendLocation_Sec = 120;

			LocationManager = null;


			//setup Google Analytics

			GAI.SharedInstance.DispatchInterval = 20;
			GAI.SharedInstance.TrackUncaughtExceptions = true;

			Tracker = GAI.SharedInstance.GetTracker (TrackingId);

			return true;

		}
			
		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			// NOTE: Don't call the base implementation on a Model class
			// see http://docs.xamarin.com/guides/ios/application_fundamentals/delegates,_protocols,_and_events 

			Hub = new SBNotificationHub(Constants.ConnectionString, Constants.NotificationHubPath);
			this.DeviceToken = deviceToken;

			try{


			Hub.UnregisterAllAsync (deviceToken, (error) => {
				if (error != null) 
				{
					Console.WriteLine("Error calling Unregister: {0}", error.ToString());
					return;
				} 


				iOSLoginManager loginManager = iOSLoginManager.Instance;

				string username = loginManager.GetUsername();
				string travelerId = loginManager.GetTravelerId().ToString();

				string[] tagsArray = new string[2];
				tagsArray[0] = username;
				tagsArray[1] = travelerId;

				NSSet tags = new NSSet(tagsArray);

				Hub.RegisterNativeAsync(deviceToken, tags, (errorCallback) => {
						if (errorCallback != null){
							GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "notifications", "notifications registration error", null).Build());
							Console.WriteLine("RegisterNativeAsync error: " + errorCallback.ToString());
						}
						else{
							GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "notifications launched", "notifications registration complete", null).Build());
						}
				});
			});
			}catch(Exception ex) {
				Console.WriteLine ("Error" + ex.ToString ());
			}
		}

		public void UnRegisterForRemoteNotifications()
		{
			// NOTE: Don't call the base implementation on a Model class
			// see http://docs.xamarin.com/guides/ios/application_fundamentals/delegates,_protocols,_and_events 

			if (Hub != null && DeviceToken != null) {
				try{



					Hub.UnregisterAllAsync (DeviceToken, (error) => {
						if (error != null) 
						{
							Console.WriteLine("Error calling Unregister: {0}", error.ToString());
							return;
						} 
					});
				}catch(Exception ex) {
					Console.WriteLine ("Error" + ex.ToString ());
				}
			}

		}

		public override void DidReceiveRemoteNotification (UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
		{

			ProcessNotification (userInfo, false, application.ApplicationState);
		}

		/*public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
		{
			ProcessNotification(userInfo, false);
		}*/

		void ProcessNotification(NSDictionary options, bool fromFinishedLaunching, UIApplicationState appState)
		{
			// Check to see if the dictionary has the aps key.  This is the notification payload you would have sent
			if (null != options && options.ContainsKey(new NSString("aps")))
			{
				//Get the aps dictionary
				NSDictionary aps = options.ObjectForKey(new NSString("aps")) as NSDictionary;

				string alert = string.Empty;

				//Extract the alert text
				//NOTE: If you're using the simple alert by just specifying "  aps:{alert:"alert msg here"}  "
				//this will work fine.  But if you're using a complex alert with Localization keys, etc., 
				//your "alert" object from the aps dictionary will be another NSDictionary... Basically the 
				//json gets dumped right into a NSDictionary, so keep that in mind
				if (aps.ContainsKey (new NSString ("alert"))) {
					alert = (aps [new NSString ("alert")] as NSString).ToString ();

					int tripid = -1;
					if (aps.ContainsKey (new NSString ("tripid")))
						tripid = (aps [new NSString ("tripid")] as NSNumber).IntValue;


					//If this came from the ReceivedRemoteNotification while the app was running,
					// we of course need to manually process things like the sound, badge, and alert.
					if (!fromFinishedLaunching) {
						//Manually show an alert
						if (!string.IsNullOrEmpty (alert)) {
							if (NotificationAlertView != null)
								NotificationAlertView.DismissWithClickedButtonIndex (0, false);

							GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "notifications", "alert", null).Build());

							NotificationAlertView = new UIAlertView ("Notification", alert, null, "OK", null);
							NotificationAlertView.Show ();
						}
					}        
				} 

				if (aps.ContainsKey (new NSString ("content-available"))) {
					GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "notifications", "content-available", null).Build());
					//TODO get list of times to send location
					//backgroundTaskId = UIApplication.SharedApplication.BeginBackgroundTask( () => {});
					DurationToSendLocation_Sec = (aps [new NSString ("content-available")] as NSNumber).IntValue;
					//NSTimer locationTimer = NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (60), delegate {
					MonitorLocation(appState);
					//});
				}
			}
		}

		private void MonitorLocation(UIApplicationState appState)
		{
			if (CLLocationManager.LocationServicesEnabled) {

				LastSentLocationTime = new DateTime (0);
				FirstSentLocationTime = new DateTime (0);
				Console.WriteLine ("MONITOR LOCATION");

				GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "location", "monitor location", null).Build());

				LocationManager = new CLLocationManager ();
				LocationManager.DesiredAccuracy = 10;
				LocationManager.PausesLocationUpdatesAutomatically = false;

				LocationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) => {
					CLLocation currentLocation = e.Locations[e.Locations.Length-1];
					DateTime locationDateTime = DateTime.SpecifyKind(currentLocation.Timestamp, DateTimeKind.Utc);

					double timeremaining = UIApplication.SharedApplication.BackgroundTimeRemaining;

					Console.WriteLine ("LOCATION UPDATE - Time Remaining: " + timeremaining.ToString());

					if(LastSentLocationTime.Ticks == 0)
					{

						Console.WriteLine ("FIRST LOCATION SENT");
						sendLocation(currentLocation);
						FirstSentLocationTime = locationDateTime;
						LastSentLocationTime = locationDateTime;
					}
					else
					{
						var diffInSeconds = (locationDateTime - LastSentLocationTime).TotalSeconds;
						if(diffInSeconds >=10.0)
						{
							Console.WriteLine ("LOCATION SENT");
							GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "location", "location sent", null).Build());
							sendLocation(currentLocation);
							LastSentLocationTime = locationDateTime;
						}

						var diffInSecondsFromFirst = (locationDateTime - FirstSentLocationTime).TotalSeconds;
						if(diffInSecondsFromFirst >=120)
						{
							Console.WriteLine ("STOP MONITOR LOCATION");
							GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("app_action", "location", "stop monitor location", null).Build());
							if(LocationManager!=null)
							{
								LocationManager.StopUpdatingLocation();
								LocationManager.StopMonitoringSignificantLocationChanges();
							}

							LocationManager = null;

						}
					}
				};


				if(appState == UIApplicationState.Active)
					LocationManager.StartUpdatingLocation ();
				else
					LocationManager.StartMonitoringSignificantLocationChanges ();
			} else {
				Console.WriteLine ("Location services not enabled");
			}
		}

		async private void sendLocation(CLLocation loc)
		{
			int taskId = -1;
			taskId = UIApplication.SharedApplication.BeginBackgroundTask( () => {
				UIApplication.SharedApplication.EndBackgroundTask(taskId);
			});
			iOSLoginManager loginManager = iOSLoginManager.Instance;

			string userId = loginManager.GetUserId ();
			int travelerId = loginManager.GetTravelerId();

			TravelerLocation travelerLoc = new TravelerLocation ();
			travelerLoc.Latitude = loc.Coordinate.Latitude;
			travelerLoc.Longitude = loc.Coordinate.Longitude;
			travelerLoc.TimeStamp = DateTime.UtcNow; 
				//DateTime.SpecifyKind(loc.Timestamp, DateTimeKind.Utc);
			travelerLoc.UserId = userId;
			travelerLoc.TravelerId = travelerId;

			TripManager tripManager = new TripManager ();
			await tripManager.PostTravelerLocation (travelerLoc);
			UIApplication.SharedApplication.EndBackgroundTask (taskId);
		}


		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
		}
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
			if (LocationManager != null) {
				LocationManager.StopUpdatingLocation ();
				LocationManager.StartMonitoringSignificantLocationChanges ();
			}
		}

		/// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
			if (LocationManager != null) {
				LocationManager.StopMonitoringSignificantLocationChanges ();
				LocationManager.StartUpdatingLocation ();
			}
		}

		/// This method is called when the application is about to terminate. Save data, if needed. 
		public override void WillTerminate (UIApplication application)
		{
		}
	}
}

