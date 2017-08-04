using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common.Models;
using IDTO.Mobile.Manager;

namespace IDTO.iPhone
{
	public class TripDetailStepsTableSource :UITableViewSource
	{
		public List<Step> mSteps;

		private NSIndexPath mSelectedIndex;

		private string mCellIdentifier = "TableCell";

		public TripDetailStepsTableSource (List<Step> steps)
		{
			mSteps = steps;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			return (mSteps.Count *2) -1;
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

				Step step = mSteps [index];

				cell.UpdateCell (step);
				return cell;
			}

		}
	}
}

