using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using IDTO.Common;

namespace IDTO.iPhone
{
	public class FavoritesTableCell : UITableViewCell
	{
		protected UILabel mTitleLabel;
		protected UIView mLineView;
		protected UIColor colorIdtoOrange;

		public FavoritesTableCell (string cellId): base(UITableViewCellStyle.Default, cellId)
		{
			colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);

			this.BackgroundColor = UIColor.White;

			mLineView = new UIView();
			mLineView.BackgroundColor = colorIdtoOrange;

			mTitleLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Light",20),
				TextColor = UIColor.DarkGray,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Right
			};

			ContentView.Add(mLineView);

			ContentView.Add (mTitleLabel);
		}

		public void UpdateCell(string titleString)
		{
			mTitleLabel.Text = titleString;
		}
			
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			mTitleLabel.Frame = new System.Drawing.RectangleF (10, 5, 250, 30);
			mLineView.Frame = new System.Drawing.RectangleF (0, Frame.Size.Height - 5, Frame.Size.Width, 5);

		}
	}
}

