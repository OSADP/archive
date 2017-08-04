using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ByteSmith.WindowsAzure.Messaging;
using PushSharp.Client;
using IDTO.Common.Models;
using IDTO.Common;

namespace IDTO.Android
{
	[Service] 
	public  class PushHandlerService : PushHandlerServiceBase
	{
		public static string RegistrationID { get; private set; }
		private NotificationHub Hub { get; set; }
		private static int _messageId;
		private const string DEFAULT_NOTIFICATION_TITLE = "Ride notification";
		public const string PREFERENCES_KEY = "IDTO.Android.PREFERENCES_KEY";
		public const string KEY_REGISTERED = "IDTO.Android.KEY_REGISTERED";
		public const string WAKE_LOCK_TAG_IDTOPushHandlerService = "WAKE_LOCK_TAG_IDTOPushHandlerService";
		private const string JSON_KEY_CONTENT_AVAILABLE = "content-available";
		private const string JSON_KEY_MESSAGE = "message";
		private Handler handler = new Handler ();
		static PowerManager.WakeLock _wakeLock;
		static readonly object Lock = new object();
		public static Registration NativeRegistration { get; set; }

		public PushHandlerService() : base(Constants.SenderID) 
		{
			Log.Info("IDTO", "PushHandlerService() constructor"); 
		}

		private ISharedPreferences GetPreferences(Context context)
		{
			return context.GetSharedPreferences (PREFERENCES_KEY, FileCreationMode.Private);
		}

		public static void AcquireWakeLock(Context context)
		{
			lock (Lock)
			{
				if (_wakeLock == null)
				{
					// This is called from BroadcastReceiver, there is no init.
					var pm = PowerManager.FromContext(context);
					_wakeLock = pm.NewWakeLock(
						WakeLockFlags.Partial, WAKE_LOCK_TAG_IDTOPushHandlerService);
				}
			}
			_wakeLock.Acquire();
		}

		public static void RunIntentInService(Context context, Intent intent)
		{				
			AcquireWakeLock (context);
			intent.SetClass(context, typeof(PushHandlerService));
			context.StartService(intent);
		}
			
		protected override async void OnRegistered(Context context, string registrationId)
		{
			Log.Info("IDTO", "GCM Registered: " + registrationId);
			RegistrationID = registrationId;
			Hub = new NotificationHub (Constants.NotificationHubPath, Constants.ConnectionString);
			try {
				await Hub.UnregisterAllAsync (registrationId);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}


			AndroidLoginManager loginMngr = AndroidLoginManager.Instance (context);
			if (!await loginMngr.IsLoggedIn ()) {
				Log.Info ("IDTO", "HandleRegistration Error: Not logged in");
				return;
			}
			int travelerID = loginMngr.GetTravelerId ();
			TravelerModel traveler = await new AccountManager ().GetTravelerById (travelerID);
			string email = "";
			if (traveler != null)
				email = traveler.Email;
			var tags = new List<string> () {
				email,
				travelerID.ToString ()
			};
			try {
				NativeRegistration = await Hub.RegisterNativeAsync (registrationId, tags);
				ISharedPreferencesEditor editor = GetPreferences (context).Edit ();
				editor.PutBoolean (KEY_REGISTERED, true);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		protected override async void OnUnRegistered(Context context, string registrationId)
		{

			Log.Info ("IDTO", "OnUnRegistered");
		
			if (NativeRegistration != null && Hub!=null) {
				await Hub.UnregisterAsync (NativeRegistration);
				await Hub.UnregisterAllAsync (registrationId);
				Log.Info ("IDTO", "Unregistered");
			}
			else {
				Log.Info ("IDTO", "Unregister error: NativeRegistration was null");
			}
			ISharedPreferencesEditor editor =  GetPreferences (context).Edit ();
            if(editor!=null)
    			editor.PutBoolean (KEY_REGISTERED, false);
		}
		protected override void OnMessage(Context context, Intent intent)
		{
			Log.Info ("IDTO","OnMessage!!!");
			string duration = intent.GetStringExtra (JSON_KEY_CONTENT_AVAILABLE);
			if(!string.IsNullOrEmpty (duration))
			{
				try {
					double reportTimeInSeconds =  double.Parse (duration);
					NotifyLocationReporting (reportTimeInSeconds);
				}catch(Exception e) {
					Log.Error ("IDTO", "Failed to [NotifyLocationReporting] due to exception");
					Log.Error ("IDTO", e.ToString ());
				}
			}
			//
			string title = "";
			string message = "";
			_messageId++;
			bool notifyUser = !string.IsNullOrEmpty(intent.GetStringExtra(JSON_KEY_MESSAGE));
			if (notifyUser) {
				title = intent.GetStringExtra("message");
				message = intent.GetStringExtra ("tripid");
				title = title == null ? DEFAULT_NOTIFICATION_TITLE : title;
				message = message == null ? "" : message;
				CreateUINotification (title, message);
			} else {
				//handle other messages
			}				
		}

		private void CreateUINotification (string title, string message)
		{
			var nMgr = (NotificationManager)GetSystemService (NotificationService);
			var pendingIntent = PendingIntent.GetActivity (this, 0, new Intent (this, typeof(HomeActivity)), 0);
			Notification.Builder builder = new Notification.Builder (this).SetAutoCancel (true).SetContentIntent (pendingIntent).SetContentTitle (title).SetSmallIcon (Resource.Drawable.icon).SetContentText (message).SetTicker (title);
            var notification = builder.Notification;
			nMgr.Notify (_messageId, notification);
		}

		private void NotifyLocationReporting (double reportTimeInSeconds)
		{
			try{
				handler.Post(delegate() {
					new LocationReporterManager (ApplicationContext).Track (reportTimeInSeconds);
				});
			}catch(Exception e){
				Log.Error ("IDTO",e.ToString());
			}
		}
	

		protected override void OnError(Context context, string registrationId)
		{
		}


	}
}