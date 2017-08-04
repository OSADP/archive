// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace IDTO.iPhone
{
	[Register ("FavoritesViewController")]
	partial class FavoritesViewController
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblFavorites { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableView { get; set; }

		[Action ("editClicked:")]
		partial void editClicked (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (lblFavorites != null) {
				lblFavorites.Dispose ();
				lblFavorites = null;
			}

			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}
		}
	}
}
