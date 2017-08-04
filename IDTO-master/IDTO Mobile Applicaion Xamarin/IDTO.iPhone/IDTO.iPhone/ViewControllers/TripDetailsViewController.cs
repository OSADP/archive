// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common;
using IDTO.Common.Models;

using IDTO.Mobile.Manager;
using GoogleAnalytics.iOS;

namespace IDTO.iPhone
{
	public partial class TripDetailsViewController : IDTOViewController
	{
		public Itinerary ItineraryToShow{ get; set;}
		public Trip TripToShow{ get; set; }
		private List<Leg> LegsToShowOnMap{ get; set; }
		public TripSearch Criteria { get; set; }

		public TripDetailsViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			UIColor colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);
			UIColor colorIdtoRed = new UIColor ((float)(255.0 / 255.0), (float)(68.0 / 255.0), (float)(68.0 / 255.0), 1.0f);



			base.ViewDidLoad ();
			if (ItineraryToShow != null) {
				lblTravelTime.Text = ItineraryToShow.GetStartDate ().ToLocalTime ().ToString ("g");
				lblNumberTransfers.Text = ItineraryToShow.GetNumberOfTransfers ().ToString ();
				lblTotalWalkTime.Text = ItineraryToShow.GetWalkTime_min ().ToString () + " min";
				lblTotalTime.Text = ItineraryToShow.GetDuration_min ().ToString () + " min";

				TripDetailLegsTableSource tableSource = new TripDetailLegsTableSource (ItineraryToShow.legs);
				tableViewSteps.Source = tableSource;

				btnSaveCancel.SetTitle ("Save", UIControlState.Normal);
				btnSaveCancel.BackgroundColor = colorIdtoOrange;
				btnSaveCancel.Hidden = false;

			} else if(TripToShow!=null){
				lblTravelTime.Text = TripToShow.TripStartDate.ToLocalTime ().ToString ("g");
				lblNumberTransfers.Text = TripToShow.GetNumberOfTransfers ().ToString ();
				lblTotalWalkTime.Text = TripToShow.GetWalkTime_min ().ToString () + " min";
				lblTotalTime.Text = TripToShow.Duration_min ().ToString () + " min";
			
				TripDetailStepsTableSource tableSource = new TripDetailStepsTableSource (TripToShow.Steps);
				tableViewSteps.Source = tableSource;

				if (TripToShow.TripStartDate.ToLocalTime() > DateTime.Now.ToLocalTime()) {
					btnSaveCancel.SetTitle ("Cancel Trip", UIControlState.Normal);
					btnSaveCancel.BackgroundColor = colorIdtoRed;
					btnSaveCancel.Hidden = false;
				} 
				else {
					btnSaveCancel.Hidden = true;
				}
			}
			setupConnectorView ();
		}
			
		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);
			TripDetailsMapViewController vc = (TripDetailsMapViewController)segue.DestinationViewController;
			if (this.ItineraryToShow != null) {
				vc.ItineraryToShow = this.ItineraryToShow;
			} else if (this.TripToShow != null) {
				vc.TripToShow = this.TripToShow;
			}
		}

		private void setupConnectorView ()
		{
			this.AddConnectorView (lblTripDetails);
			this.AddConnectorView (tableViewSteps);
			ExtendToView = tableViewSteps;
		}

		partial void saveCancelAction (NSObject sender2)
		{
			if (ItineraryToShow != null) {
				SaveItinerary();
			}
			else if(TripToShow!=null){

				int buttonClicked = -1;
				UIAlertView alert = new UIAlertView("Cancel", "Are you sure you want to cancel this trip?", null, NSBundle.MainBundle.LocalizedString ("No", "No"),
					NSBundle.MainBundle.LocalizedString ("Yes", "Yes"));

				alert.Clicked += (sender, buttonArgs) =>  { 
					buttonClicked = buttonArgs.ButtonIndex; 

					if(buttonClicked == 1)
					{
						CancelTrip();
					}
				};
				alert.Show ();
			}

		}

		async private void SaveItinerary()
		{

			UserTripDataManager userTripManager = new UserTripDataManager();
			iOSLoginManager loginManager = iOSLoginManager.Instance;

			string origin = Criteria.GetStartLocationString();
			string destination = Criteria.GetEndLocationString();
			string prioritycode = "1";
			bool isWheelchair = false;
			bool isBike = false;

			int travelerId = loginManager.GetTravelerId();
			bool didSave = await userTripManager.SaveTripForUser(travelerId, ItineraryToShow, origin, destination, prioritycode, isWheelchair, isBike);

			GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("ui_action", "save trip", "save trip", didSave).Build());

			if(didSave)
			{
				this.NavigationController.PopToRootViewController (false);
			}
			else
			{
				UIAlertView alert = new UIAlertView ("Not Saved", "Error saving trip.  Please try again.", null, "OK", null);
				alert.Show();
			}
		}

		async private void CancelTrip()
		{

			UserTripDataManager userTripManager = new UserTripDataManager();
			iOSLoginManager loginManager = iOSLoginManager.Instance;

			int travelerId = loginManager.GetTravelerId();
			bool didSave = await userTripManager.CancelTripForUser (travelerId, TripToShow);

			GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateEvent("ui_action", "cancel trip", "cancel trip", didSave).Build());

			if(didSave)
			{
				UIAlertView alert = new UIAlertView ("Canceled", "Trip Canceled", null, "OK", null);
				alert.Show();
				btnSaveCancel.Hidden = true;
			}
			else
			{
				UIAlertView alert = new UIAlertView ("Not Canceled", "Error canceling trip.  Please try again.", null, "OK", null);
				alert.Show();
			}
		}
	}
}