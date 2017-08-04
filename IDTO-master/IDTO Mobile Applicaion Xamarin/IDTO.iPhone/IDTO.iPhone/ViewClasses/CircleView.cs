using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace IDTO.iPhone
{
	public class CircleView : UIView
	{
		public CircleView ()
		{
			this.BackgroundColor = UIColor.Clear;
		}

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);

			float lineWidth = 2;

			RectangleF borderRect = rect.Inset (2, 2);

			UIColor colorIdtoOrange = new UIColor ((float)(250.0 / 255.0), (float)(175.0 / 255.0), (float)(64.0 / 255.0), 1.0f);
			UIColor colorIdtoWhite = new UIColor ((float)(255.0 / 255.0), (float)(255.0 / 255.0), (float)(255.0 / 255.0), 1.0f);



			using(CGContext g = UIGraphics.GetCurrentContext ()){

				g.SetFillColor (colorIdtoWhite.CGColor);
				g.SetStrokeColor (colorIdtoOrange.CGColor);
				g.SetLineWidth (lineWidth);
				g.FillEllipseInRect (borderRect);
				g.StrokeEllipseInRect (borderRect);
				g.FillPath ();
			}
		}
	}
}

