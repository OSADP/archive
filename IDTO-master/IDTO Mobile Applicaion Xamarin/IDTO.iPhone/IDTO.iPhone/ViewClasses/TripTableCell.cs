using System;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

using IDTO.Common;

namespace IDTO.iPhone
{
	public class TripTableCell : UITableViewCell
	{
		protected UILabel mTimeLabel;
		protected UILabel mAmPmLabel;
		protected UILabel mDateLabel;
		protected UILabel mTitleLabel;
		protected UILabel mDurationLabel;
		protected UIView mLineView;
		protected UIColor colorIdtoOrange;

		public TripTableCell (string cellId): base(UITableViewCellStyle.Default, cellId)
		{
			colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);

			this.BackgroundColor = UIColor.White;

			mLineView = new UIView();
			mLineView.BackgroundColor = colorIdtoOrange;

			mDateLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Light",16),
				TextColor = UIColor.LightGray,
				BackgroundColor = UIColor.Clear
			};

			mTimeLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Bold",28),
				TextColor = colorIdtoOrange,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Center
			};

			mAmPmLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Bold",16),
				TextColor = colorIdtoOrange,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Left
			};

			mTitleLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Light",20),
				TextColor = UIColor.DarkGray,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Right
			};
			mDurationLabel = new UILabel () {
				Font = UIFont.FromName("HelveticaNeue-Light",18),
				TextColor = UIColor.LightGray,
				BackgroundColor = UIColor.Clear,
				TextAlignment = UITextAlignment.Right
			};


			ContentView.Add(mLineView);

			ContentView.Add (mDateLabel);
			ContentView.Add (mTimeLabel);
			ContentView.Add (mAmPmLabel);
			ContentView.Add (mTitleLabel);
			ContentView.Add (mDurationLabel);
		}

		public void UpdateCell(DateTime dateTime, string titleString, string durationString)
		{
			mTimeLabel.Text = dateTime.GetTimeString();
			mAmPmLabel.Text = dateTime.GetTimeAmPm();
			mDateLabel.Text = dateTime.GetTodayTomorrowString();
			mTitleLabel.Text = titleString;
			mDurationLabel.Text = durationString;
		}
			
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			mDateLabel.Frame = new System.Drawing.RectangleF (5, 5, 50, 20);
			mTimeLabel.Frame = new System.Drawing.RectangleF (2, 31, 75, 30);
			mAmPmLabel.Frame = new System.Drawing.RectangleF (77, 41, 30, 20);
			mTitleLabel.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 175, 2, 175, 30);
			mDurationLabel.Frame = new System.Drawing.RectangleF (ContentView.Bounds.Width - 150, 36, 150, 25);
			mLineView.Frame = new System.Drawing.RectangleF (0, Frame.Size.Height - 5, Frame.Size.Width, 5);

		}
	}
}

