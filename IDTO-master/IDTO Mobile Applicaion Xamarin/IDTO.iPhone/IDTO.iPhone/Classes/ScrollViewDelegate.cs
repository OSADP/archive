using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;


namespace IDTO.iPhone
{

	public class ScrollViewDelegate : UIScrollViewDelegate
	{
		public delegate void ScrollingViewDelegate(UIScrollView scrollView);

		public ScrollViewDelegate ()
		{

		}

		public ScrollingViewDelegate ScolledDelegate { get; set;}

		/*public override void Scrolled (UIScrollView scrollView)
		{
			if (ScolledDelegate != null)
				ScolledDelegate.Invoke (scrollView);
		}
*/
		public override void DecelerationEnded (UIScrollView scrollView)
		{
			if (ScolledDelegate != null)
				ScolledDelegate.Invoke (scrollView);
		}
	}
}

