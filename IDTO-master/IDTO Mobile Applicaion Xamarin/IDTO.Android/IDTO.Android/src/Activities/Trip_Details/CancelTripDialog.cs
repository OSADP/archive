
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
	public class CancelTripDialog
	{
		public static void Show(Activity activity, TripDetailsPresenter tripDetailsPresenter, Trip trip)
		{
			var builder = new AlertDialog.Builder(activity);
			builder.SetTitle("Cancel Trip");
			builder.SetMessage ("Are you sure?");
			builder.SetPositiveButton("Yes", (EventHandler<DialogClickEventArgs>)null);
			builder.SetNegativeButton("No", (EventHandler<DialogClickEventArgs>)null);
			var dialog = builder.Create();
			dialog.Show();
			var yesBtn = dialog.GetButton((int)DialogButtonType.Positive);
			var noBtn = dialog.GetButton((int)DialogButtonType.Negative);
			yesBtn.Click += (s, e) =>
			{
				CancelTrip(activity, tripDetailsPresenter, trip, dialog);
			};
			noBtn.Click += (s, e) =>
			{
				dialog.Dismiss();
			};
		
		}

		private static async void CancelTrip(Activity activity, TripDetailsPresenter cancelTripListener, Trip trip, AlertDialog dialog)
		{
			int travelerId = AndroidLoginManager.Instance(activity).GetTravelerId();
			bool success = await new UserTripDataManager().CancelTripForUser(travelerId, trip);
			dialog.Dismiss();
			cancelTripListener.OnTripCancelComplete(success);
		}

	}
}

