using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using IDTO.Common;

namespace IDTO.iPhone
{
	public class TripTableCellHomeScreen: TripTableCell
	{
		public TripTableCellHomeScreen (string cellId): base(cellId)
		{
			mDurationLabel.RemoveFromSuperview ();

			mDateLabel.Font = UIFont.FromName ("HelveticaNeue-Light", 12);
			mTimeLabel.Font = UIFont.FromName("HelveticaNeue-Bold",24);
			mAmPmLabel.Font = UIFont.FromName("HelveticaNeue-Bold",12);
			mTitleLabel.Font = UIFont.FromName("HelveticaNeue-Light",16);
		}

		public void UpdateCell (DateTime dateTime, string titleString)
		{
			base.UpdateCell (dateTime, titleString, "");
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			mDateLabel.Frame = new System.Drawing.RectangleF (5, 2, 50, 15);
			mTimeLabel.Frame = new System.Drawing.RectangleF (5, 19, 65, 20);
			mAmPmLabel.Frame = new System.Drawing.RectangleF (70, 26, 25, 15);
			mTitleLabel.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 157, 7, 157, 21);
			mLineView.Frame = new System.Drawing.RectangleF (0, Frame.Size.Height - 1, Frame.Size.Width, 1);
		}
	}
}

