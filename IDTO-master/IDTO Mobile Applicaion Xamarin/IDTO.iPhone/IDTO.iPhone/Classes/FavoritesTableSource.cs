using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common;
using IDTO.Common.Models;
using IDTO.Mobile.Manager;
namespace IDTO.iPhone
{
	public delegate void FavoriteSelectedDelegate(String favorite);

	public class FavoritesTableSource :UITableViewSource
	{
		public event FavoriteSelectedDelegate FavoriteSelected;

		private List<FavoriteLocation> mFavorites;
		private string mCellIdentifier = "Cell";
		public FavoritesTableSource ()
		{
			AppDelegate appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			FavoritesDbManager favorites = appDelegate.FavoriteLocations;

			mFavorites = favorites.GetFavoriteLocations()as List<FavoriteLocation>;
		}

		public override int RowsInSection (UITableView tableview, int section)
		{
			if (mFavorites != null)
				return mFavorites.Count;
			else
				return 0;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			tableView.DeselectRow (indexPath, true);

			if (FavoriteSelected != null) {

					int index = indexPath.Row;

					FavoriteLocation fav = mFavorites [index];

					FavoriteSelected (fav.Location);

			}
		}

		/*public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return 33;
		}*/

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{

			UITableViewCell cell = tableView.DequeueReusableCell (mCellIdentifier);
			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Default, mCellIdentifier);


			int index = indexPath.Row;

			FavoriteLocation fav = mFavorites [index];

			cell.TextLabel.Text = fav.Location;

			return cell;


		}

		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			AppDelegate appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			FavoritesDbManager favorites = appDelegate.FavoriteLocations;

			switch (editingStyle) {
			case UITableViewCellEditingStyle.Delete:
				// remove the item from the underlying data source
				FavoriteLocation fav = mFavorites [indexPath.Row];
				favorites.DeleteFavoriteLocation (fav);
				// delete the row from the table
				mFavorites.RemoveAt (indexPath.Row);
				tableView.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
				break;
			case UITableViewCellEditingStyle.None:
				Console.WriteLine ("CommitEditingStyle:None called");
				break;
			}
		}
		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true; // return false if you wish to disable editing for a specific indexPath or for all rows
		}

	}
}

