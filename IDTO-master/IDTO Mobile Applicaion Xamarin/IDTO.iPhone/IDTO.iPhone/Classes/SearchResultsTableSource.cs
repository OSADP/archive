using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common.Models;
using IDTO.Mobile.Manager;
namespace IDTO.iPhone
{
	public delegate void SearchResultSelected(Itinerary itinerary);

	public class SearchResultsTableSource :UITableViewSource
	{
		public event SearchResultSelected ItinerarySelected;

		private TripSearchResult mSearchResult;
		private string mCellIdentifier = "TableCell";
		public SearchResultsTableSource (TripSearchResult searchResult)
		{
			mSearchResult = searchResult;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			if (mSearchResult != null && mSearchResult.itineraries!=null)
				return (mSearchResult.itineraries.Count * 2) - 1;
			else
				return 0;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);

			if (ItinerarySelected != null) {
				if (indexPath.Row % 2 == 0) {

					int index = indexPath.Row / 2;
					Itinerary itinerary = mSearchResult.itineraries [index];

					ItinerarySelected (itinerary);
				}
			}
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Row % 2 == 1) {
				return 22;
			}
			return 66;
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
				TripTableCell cell = tableView.DequeueReusableCell (mCellIdentifier) as TripTableCell;
				if (cell == null)
					cell = new TripTableCell (mCellIdentifier);


				int index = indexPath.Row / 2;

				Itinerary itinerary = mSearchResult.itineraries [index];

				string durationString = itinerary.GetDuration_min().ToString() + " min";

				string stepString = itinerary.GetFirstAgencyName();

				cell.UpdateCell (itinerary.GetStartDate().ToLocalTime(), stepString, durationString);
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				return cell;
			}

		}
	}
}

