using Android.App;
using Android.Content;
using ByteSmith.WindowsAzure.Messaging;
using PushSharp.Client;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is only needed for android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace IDTO.Android
{
	[BroadcastReceiver(Permission= "com.google.android.c2dm.permission.SEND")]

	[IntentFilter(new[] { "com.google.android.c2dm.intent.UNREGISTER" }, Categories = new[] {"notificationHubsIDTO" })]
	[IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new[] {"notificationHubsIDTO" })]
	[IntentFilter(new[] { "com.google.android.c2dm.intent.REGISTRATION" }, Categories = new[] {"notificationHubsIDTO" })]
	[IntentFilter(new[] { "com.google.android.gcm.intent.RETRY" }, Categories = new[] { "notificationHubsIDTO"})]
	public class MyGCMBroadcastReceiver : PushHandlerBroadcastReceiverBase<PushHandlerService>
	{
		const string TAG = "PushHandlerBroadcastReceiver";
		public override void OnReceive(Context context, Intent intent)
		{
			//PushHandlerService.AcquireWakeLock (context);
			PushHandlerService.RunIntentInService(context, intent);
			//
			SetResult(Result.Ok, null, null);
		}
	}
}