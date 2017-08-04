using System;
using System.Collections.Generic;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

using IDTO.Common.Models;
using MonoTouch.ObjCRuntime;

namespace IDTO.iPhone
{
	public class TripDetailLegsTableCell : UITableViewCell
	{
		UIImageView mModeImageView;
		UILabel mDuration;
		UILabel mAgencyAndRoute;
		UILabel mStopName;
		UIView mLineView;

		UILabel mBoardLabel;
		UILabel mDepartLabel;

		UILabel mFromTime;
		UILabel mToTime;

		//MKMapView mMapview;

		public TripDetailLegsTableCell (string cellId): base(UITableViewCellStyle.Default, cellId)
		{
			UIColor colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);
			//UIColor colorIdtoClear = new UIColor ((float)(255.0/ 255.0), (float)(255.0 / 255.0), (float)(255.0 / 255.0), 0.0f);

			this.BackgroundColor = UIColor.White;

			mLineView = new UIView();
			mLineView.BackgroundColor = colorIdtoOrange;

			mModeImageView = new UIImageView (new System.Drawing.RectangleF (5, 5, 40, 50));
			mModeImageView.ContentMode = UIViewContentMode.Center;
			mModeImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

			mDuration = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue",14),
				TextColor = UIColor.Gray,
				BackgroundColor = UIColor.Clear
			};

			mAgencyAndRoute = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue",14),
				TextColor = colorIdtoOrange,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Center
			};

			mStopName = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Light",14),
				TextColor = colorIdtoOrange,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Left
			};

			mFromTime = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Medium",14),
				TextColor = UIColor.Gray,
				BackgroundColor = UIColor.Clear
			};

			mToTime = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Medium",14),
				TextColor = UIColor.Gray,
				BackgroundColor = UIColor.Clear
			};

			mBoardLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue",14),
				TextColor = UIColor.Gray,
				BackgroundColor = UIColor.Clear,
				LineBreakMode = UILineBreakMode.WordWrap,
				Lines = 2,

				
			};

			mDepartLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue",14),
				TextColor = UIColor.Gray,
				BackgroundColor = UIColor.Clear,
				LineBreakMode = UILineBreakMode.WordWrap,
				Lines = 2
			};

			ContentView.Add(mLineView);

			ContentView.Add (mModeImageView);
			ContentView.Add (mDuration);
			ContentView.Add (mAgencyAndRoute);
			ContentView.Add (mStopName);


		}

		/*void NewMethod (Leg leg)
		{
			//TODO Finish Map stuff.  Need to label start and end.  Auto zoom
			mMapview = new MKMapView ();
			mMapview.MapType = MKMapType.Standard;
			mMapview.ShowsUserLocation = false;
			mMapview.ZoomEnabled = false;
			mMapview.ScrollEnabled = false;
			mMapview.PitchEnabled = false;
			mMapview.ShowsBuildings = false;
			MapDelegate mapDelegate = new MapDelegate ();
			mMapview.Delegate = mapDelegate;
			ContentView.Add (mMapview);
			var originPlaceMark = new MKPlacemark (new CLLocationCoordinate2D (leg.from.lat, leg.from.lon), null);
			var sourceItem = new MKMapItem (originPlaceMark);
			var sourceAnnotation = new BasicMapAnnotation (new CLLocationCoordinate2D (leg.from.lat, leg.from.lon), "Start", "");
			mMapview.AddAnnotation (sourceAnnotation);
			PerformSelector (new Selector ("SelectAnnotation"), sourceAnnotation, 0.3f);
			var destPlaceMark = new MKPlacemark (new CLLocationCoordinate2D (leg.to.lat, leg.to.lon), null);
			var destItem = new MKMapItem (destPlaceMark);
			var destAnnotation = new BasicMapAnnotation (new CLLocationCoordinate2D (leg.to.lat, leg.to.lon), "End", "");
			mMapview.AddAnnotation (destAnnotation);
			PerformSelector (new Selector ("SelectAnnotation"), destAnnotation, 0.3f);
			var request = new MKDirectionsRequest {
				Source = sourceItem,
				Destination = destItem
			};
			var directions = new MKDirections (request);
			directions.CalculateDirections ((response, error) =>  {
				if (error != null) {
				}
				else {
					mMapview.AddOverlay (response.Routes [0].Polyline);
				}
			});
			mMapview.SetRegion (RegionFromLocations (new CLLocationCoordinate2D (leg.from.lat, leg.from.lon), new CLLocationCoordinate2D (leg.to.lat, leg.to.lon)), false);
		}
*/
		public void UpdateCell(Leg leg)
		{
			string mode = leg.mode;
			string durationString = leg.GetDuration_min().ToString() +" min";
			string stopName = leg.to.name;
			string agencyAndRoute = leg.agencyId + " " + leg.routeShortName;

			string boardingString = "";
			string departString = "";
			string startTimeString = "";
			string endTimeString = "";
			UIImage modeImage;
			if (mode.ToLower ().Equals ("walk")) {
				mAgencyAndRoute.Text = "WALK";
				modeImage = new UIImage ("Walking_Icon.png");

				boardingString = "Walk from " + leg.from.name;
				departString = "to " + leg.to.name;

				startTimeString = leg.GetStartDate ().ToLocalTime(). ToString ("t");
				endTimeString = leg.GetEndDate ().ToLocalTime().ToString ("t");

			} 
			else if (mode.ToLower ().Equals ("rail")){
				//mMapview = null;
				mAgencyAndRoute.Text = agencyAndRoute;
				modeImage = new UIImage ("Rail_Icon.png");

				boardingString = leg.agencyId + " " + leg.route + " at " + leg.from.name;
				departString = leg.to.name;

				startTimeString = leg.GetStartDate ().ToLocalTime(). ToString ("t");
				endTimeString = leg.GetEndDate ().ToLocalTime().ToString ("t");

			}
			else {
				//mMapview = null;
				mAgencyAndRoute.Text = agencyAndRoute;
				modeImage = new UIImage ("Bus_Icon.png");

				boardingString = leg.agencyId + " " + leg.route + " at " + leg.from.name;
				departString = leg.to.name;

				startTimeString = leg.GetStartDate ().ToLocalTime(). ToString ("t");
				endTimeString = leg.GetEndDate ().ToLocalTime().ToString ("t");

			}

			mBoardLabel.Text = boardingString;
			mDepartLabel.Text = departString;

			mFromTime.Text = startTimeString;
			mToTime.Text = endTimeString;

			mDuration.Text = durationString;

			mStopName.Text = stopName;
			mModeImageView.Image = modeImage;
		}

		public void UpdateCell(Step step)
		{
			string durationString = step.Duration_min().ToString() +" min";
			string stopName = step.ToName;
			string agencyAndRoute = "";

			string boardingString = "";
			string departString = "";
			string startTimeString = "";
			string endTimeString = "";
			UIImage modeImage;
			if (step.ModeId == (int)ModeType.ModeId.WALK) {
				mAgencyAndRoute.Text = "WALK";
				modeImage = new UIImage ("Walking_Icon.png");

				boardingString = "Walk from " + step.FromName;
				departString = "to " + step.ToName;

				startTimeString = step.StartDate.ToLocalTime(). ToString ("t");
				endTimeString = step.EndDate.ToLocalTime().ToString ("t");

			} 
			else if (step.ModeId == (int)ModeType.ModeId.RAIL){
				//mMapview = null;
				agencyAndRoute = Providers.IdToString(step.FromProviderId.Value) + " " + step.RouteNumber;
				mAgencyAndRoute.Text = agencyAndRoute;
				modeImage = new UIImage ("Rail_Icon.png");

				boardingString = Providers.IdToString(step.FromProviderId.Value) + " " + step.RouteNumber + " at " + step.FromName;
				departString = step.ToName;

				startTimeString = step.StartDate.ToLocalTime(). ToString ("t");
				endTimeString = step.EndDate.ToLocalTime().ToString ("t");
			}
			else {
				//mMapview = null;
				agencyAndRoute = Providers.IdToString(step.FromProviderId.Value) + " " + step.RouteNumber;
				mAgencyAndRoute.Text = agencyAndRoute;
				modeImage = new UIImage ("Bus_Icon.png");

				boardingString = Providers.IdToString(step.FromProviderId.Value) + " " + step.RouteNumber + " at " + step.FromName;
				departString = step.ToName;

				startTimeString = step.StartDate.ToLocalTime(). ToString ("t");
				endTimeString = step.EndDate.ToLocalTime().ToString ("t");

			}

			mBoardLabel.Text = boardingString;
			mDepartLabel.Text = departString;

			mFromTime.Text = startTimeString;
			mToTime.Text = endTimeString;

			mDuration.Text = durationString;

			mStopName.Text = stopName;
			mModeImageView.Image = modeImage;
		}
			
		/*[Export("SelectAnnotation")]
		public void SelectAnnotation(MKAnnotation annotation)
		{
			mMapview.SelectAnnotation (annotation, false);
		}
*/
		public override void LayoutSubviews ()
		{

			base.LayoutSubviews ();

			mDuration.Frame = new System.Drawing.RectangleF (50, 5, 50, 20);
			mAgencyAndRoute.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 150, 5, 150, 20);
			mStopName.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 200, 30, 200, 25);
			mLineView.Frame = new System.Drawing.RectangleF (0, Frame.Size.Height - 5, Frame.Size.Width, 5);

			if (ContentView.Bounds.Size.Height > 66) {
				//Display extra content

				ContentView.Add (mFromTime);
				ContentView.Add (mToTime);
				ContentView.Add (mBoardLabel);
				ContentView.Add (mDepartLabel);

				mFromTime.Frame = new System.Drawing.RectangleF (5, 75, 75, 40);
				mToTime.Frame = new System.Drawing.RectangleF (5, 125, 75, 40);

				mBoardLabel.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 200, 75, 200, 40); 
				mDepartLabel.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 200, 125, 200, 40); 

				//if (mMapview != null)
				//	mMapview.Frame = new System.Drawing.RectangleF (25, 65, ContentView.Bounds.Width - 50, 90);

			} else {
				mFromTime.RemoveFromSuperview ();
				mToTime.RemoveFromSuperview ();
				mBoardLabel.RemoveFromSuperview ();
				mDepartLabel.RemoveFromSuperview ();
				//if (mMapview != null)
				//	mMapview.RemoveFromSuperview ();
			}
		}

		/*private MKCoordinateRegion RegionFromLocations(CLLocationCoordinate2D startLoc, CLLocationCoordinate2D endLoc)
		{
			double uplat = 0;
			double uplon = 0;

			double lowlat = 0;
			double lowlon = 0;

			if (startLoc.Latitude > endLoc.Latitude)
				uplat = startLoc.Latitude;
			else
				uplat = endLoc.Latitude;

			if (startLoc.Longitude > endLoc.Longitude)
				uplon = startLoc.Longitude;
			else
				uplon = endLoc.Longitude;

			if (startLoc.Latitude < endLoc.Latitude)
				lowlat = startLoc.Latitude;
			else
				lowlat = endLoc.Latitude;

			if (startLoc.Longitude < endLoc.Longitude)
				lowlon = startLoc.Longitude;
			else
				lowlon = endLoc.Longitude;


			MKCoordinateSpan span;
			span.LatitudeDelta = uplat - lowlat +0.0009;
			span.LongitudeDelta = uplon - lowlon +0.0009;

			CLLocationCoordinate2D center;
			center.Latitude = (uplat + lowlat) / 2.0;
			center.Longitude = (uplon + lowlon) / 2.0;

			MKCoordinateRegion region = new MKCoordinateRegion (center, span);
			return region;
		}
		*/
	}
}

