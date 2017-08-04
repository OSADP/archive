using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common.Models;
using IDTO.Mobile.Manager;
namespace IDTO.iPhone
{
	public delegate void UpcomingTripSelected(Trip trip);

	public class UpcomingTripsTableSource :UITableViewSource
	{
		public event UpcomingTripSelected TripSelected;

		private List<Trip> mTrips;
		private string mCellIdentifier = "TableCell";
		private UITableViewCell mTableViewCell;

		public UpcomingTripsTableSource (List<Trip> trips, UITableViewCell viewCell)
		{
			mTrips = trips;
			mTableViewCell = viewCell;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			if (mTrips != null)
				return (mTrips.Count * 2) - 1;
			else
				return 0;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);

			if (TripSelected != null) {
				if (indexPath.Row % 2 == 0) {

					int index = indexPath.Row / 2;
					Trip trip = mTrips[index];

					TripSelected (trip);
				}
			}
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (mTableViewCell is TripTableCellHomeScreen) {
				if (indexPath.Row % 2 == 1) {

					return 0;
				}
				return 44;
			}
			else{
				if (indexPath.Row % 2 == 1) {

					return 10;
				}
				return 66;
			}

		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Row % 2 == 1) {
				//invisible row
				UITableViewCell cell = tableView.DequeueReusableCell ("blankcell");
				if (cell == null)
					cell = new UITableViewCell (UITableViewCellStyle.Default, "blankcell");

				cell.BackgroundColor = UIColor.Clear;
				cell.UserInteractionEnabled = false;

				return cell;

			} else {

				TripTableCell cell;
				if (mTableViewCell is TripTableCellHomeScreen) {
					cell = tableView.DequeueReusableCell (mCellIdentifier) as TripTableCellHomeScreen;
					if (cell == null)
						cell = new TripTableCellHomeScreen (mCellIdentifier);
				}
				else{
					cell = tableView.DequeueReusableCell (mCellIdentifier) as TripTableCell;
					if (cell == null)
						cell = new TripTableCell (mCellIdentifier);
				}

				int index = indexPath.Row / 2;

				Trip trip = mTrips [index];


				string destinationString = trip.Destination;
				string durationString = trip.Duration_min().ToString() + " min";
				DateTime startDate = trip.TripStartDate;

				cell.UpdateCell (startDate.ToLocalTime (), destinationString, durationString);
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				return cell;
			}

		}
	}
}

