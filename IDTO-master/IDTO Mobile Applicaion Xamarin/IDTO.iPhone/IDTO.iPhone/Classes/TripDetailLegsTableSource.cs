using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common.Models;
using IDTO.Mobile.Manager;

namespace IDTO.iPhone
{
	public class TripDetailLegsTableSource :UITableViewSource
	{
		public List<Leg> mLegs;

		private NSIndexPath mSelectedIndex;

		private string mCellIdentifier = "TableCell";

		public TripDetailLegsTableSource (List<Leg> legs)
		{
			mLegs = legs;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return (mLegs.Count *2) -1;
		}

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Row % 2 == 1) {
				return 22;
			}

			if (mSelectedIndex!=null && indexPath.Row == mSelectedIndex.Row) {
				return 166;
			}

			return 66;
		}
			
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			if (mSelectedIndex != null && mSelectedIndex.Row == indexPath.Row) {
				//Deselect
				mSelectedIndex = null;
			} else {
				mSelectedIndex = indexPath;
			}
			tableView.DeselectRow (indexPath, true);
	
			tableView.BeginUpdates ();

			NSIndexPath[] pathArray = new NSIndexPath [1];
			pathArray [0] = indexPath;

			tableView.ReloadRows (pathArray, UITableViewRowAnimation.Fade);
			tableView.EndUpdates ();

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
				TripDetailLegsTableCell cell = tableView.DequeueReusableCell (mCellIdentifier) as TripDetailLegsTableCell;
				if (cell == null)
					cell = new TripDetailLegsTableCell (mCellIdentifier);


				int index = indexPath.Row / 2;

				Leg leg = mLegs [index];

				cell.UpdateCell (leg);
				return cell;
			}

		}
	}
}

