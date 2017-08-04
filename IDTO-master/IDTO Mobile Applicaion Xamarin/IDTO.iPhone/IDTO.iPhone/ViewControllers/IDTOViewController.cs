using System;
using System.Collections.Generic;

using BigTed;

using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;

using GoogleAnalytics.iOS;

namespace IDTO.iPhone
{
	public class IDTOViewController :UIViewController
	{
		List<UIView> viewList = new List<UIView> ();
		List<CircleView>circleViews = new List<CircleView>();
		CAShapeLayer lineLayer;

		protected UIView ExtendToView { get; set; }
		public IDTOViewController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidAppear (bool animated)
		{
			String className = this.GetType ().Name;
			GAI.SharedInstance.DefaultTracker.Set (GAIConstants.ScreenName, className);
			GAI.SharedInstance.DefaultTracker.Send (GAIDictionaryBuilder.CreateAppView ().Build ());

			base.ViewDidAppear (animated);
		}

		public override void ViewDidLoad ()
		{
			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, false);
			base.ViewDidLoad ();

			this.NavigationController.NavigationBar.Translucent = true;

			UIImage backgroundImage = new UIImage ();

			bool retina = (UIScreen.MainScreen.Scale > 1.0);
			if (retina) {
				if (UIScreen.MainScreen.Bounds.Size.Height > 480.0f) {
					backgroundImage = new UIImage ("background-568h@2x.png");
				} else {
					backgroundImage = new UIImage ("background@2x.png");
				}
			} else {
				backgroundImage = new UIImage ("backgroundx.png");
			}

			UIImageView backgroundImageView = new UIImageView (backgroundImage);

			this.View.Add (backgroundImageView);
			this.View.SendSubviewToBack (backgroundImageView);
		}

		protected void showLoading()
		{
			//BTProgressHUD.Show("Loading");
			BTProgressHUD.Show ("Loading", -1, ProgressHUD.MaskType.Gradient);
		}

		protected void dismissLoading()
		{
			BTProgressHUD.Dismiss();
		}

		public override void ViewDidLayoutSubviews ()
		{
			base.ViewDidLayoutSubviews ();

			if (lineLayer != null)
				lineLayer.RemoveFromSuperLayer ();

			lineLayer = null;

			foreach(CircleView circle in circleViews)
			{
				circle.RemoveFromSuperview ();
			}

			circleViews.Clear ();

			drawConnectorView (viewList);
			this.View.LayoutSubviews ();
		}

		protected void AddConnectorView(UIView viewToConnect)
		{
			viewList.Add (viewToConnect);
		}

		private void drawConnectorView(List<UIView> uiViewList)
		{
			float startX = 10;
			float circleRadius = 8;
			float startY = 100;

			float endY = this.View.Frame.Height;
			if (ExtendToView != null) {
				endY = ExtendToView.Frame.Y + ExtendToView.Frame.Size.Height;
			}

			foreach (UIView view in uiViewList)
			{
				float tempY = view.Frame.Y;
				float tempHeight = view.Frame.Size.Height;

				float center = tempY + (tempHeight / 2.0f);

				if (center < startY)
					startY = center;

				if (center > endY)
					endY = center;
			}

			UIBezierPath path = new UIBezierPath ();
			path.MoveTo (new System.Drawing.PointF (startX, startY));
			path.AddLineTo (new System.Drawing.PointF (startX, endY));

			lineLayer = new CAShapeLayer ();
			lineLayer.Path = path.CGPath;
			UIColor colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);

			lineLayer.StrokeColor = colorIdtoOrange.CGColor;
			lineLayer.LineWidth = 5;
			lineLayer.FillColor = UIColor.Clear.CGColor;

			this.View.Layer.AddSublayer (lineLayer);

			foreach (UIView view in uiViewList)
			{
				float tempY = view.Frame.Y;
				float tempHeight = view.Frame.Size.Height;

				float center = tempY + (tempHeight / 2.0f);

				CircleView newCircle = new CircleView ();
				if (view is UITableView || view is MKMapView) {
					center = tempY + circleRadius;
					newCircle.Frame = new System.Drawing.RectangleF (startX - circleRadius, center - circleRadius, 2 * circleRadius, 2 * circleRadius);
				} else {
					newCircle.Frame = new System.Drawing.RectangleF (startX - circleRadius, center - circleRadius, 2 * circleRadius, 2 * circleRadius);
				}

				circleViews.Add (newCircle);

				this.View.AddSubview (newCircle);
			}
		}
	}
}

