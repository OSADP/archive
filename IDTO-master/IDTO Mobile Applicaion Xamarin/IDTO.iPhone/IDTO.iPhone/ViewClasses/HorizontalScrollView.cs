using System;
using System.Collections.Generic;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace IDTO.iPhone
{
	[Register("HorizontalScrollView")]

	public class HorizontalScrollView :UIView
	{
		public delegate void HorizontalScrollViewPageChangeDelegate(int pageNumber);

		public HorizontalScrollViewPageChangeDelegate PageChanged { get; set;}

		UIScrollView _scrollView;

		List<UILabel> _buttons;

		UIImageView ltChevronImageView;
		UIImageView rtChevronImageView;

		float h;
		float w;
		float padding;

		public HorizontalScrollView() {


		}

		public HorizontalScrollView(IntPtr handle) :base (handle){
			_buttons = new List<UILabel> ();

			int chevronWidth = 20;

			h = this.Frame.Height-10;
			w = this.Frame.Width - (2*chevronWidth);
			padding = 0;

			_scrollView = new UIScrollView {
				Frame = new RectangleF (chevronWidth, 0, w,
					h + 2 * padding),
				ContentSize = new SizeF ((w + padding) * _buttons.Count, h),
				BackgroundColor = UIColor.Clear,
				PagingEnabled = true,
				ShowsVerticalScrollIndicator = false,
				ShowsHorizontalScrollIndicator = false,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth
			};

			ltChevronImageView = new UIImageView(new RectangleF(0,0,chevronWidth,h));
			ltChevronImageView.Image = new UIImage ("chevron_lt.png");
			ltChevronImageView.BackgroundColor = UIColor.Clear;
			ltChevronImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			ltChevronImageView.Hidden = true;

			UITapGestureRecognizer ltTapGesture = new UITapGestureRecognizer ();
			ltTapGesture.AddTarget (() => {
				//Go back
				float offset = _scrollView.ContentOffset.X;

				int i = (int)(offset / w);

				i=i-1;

				offset = i * w;

				PointF newCO = new PointF();
				newCO.Y = _scrollView.ContentOffset.Y;
				newCO.X = offset;

				_scrollView.ContentOffset = newCO;

				showPage(i);

			});
			ltTapGesture.NumberOfTapsRequired = 1;

			ltChevronImageView.UserInteractionEnabled = true;
			ltChevronImageView.AddGestureRecognizer (ltTapGesture);

			this.Add (ltChevronImageView);

			rtChevronImageView = new UIImageView (new RectangleF (this.Frame.Width - chevronWidth, 0, chevronWidth, h));
			rtChevronImageView.Image = new UIImage ("chevron_rt.png");
			rtChevronImageView.BackgroundColor = UIColor.Clear;
			rtChevronImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			rtChevronImageView.Hidden = false;

			UITapGestureRecognizer rtTapGesture = new UITapGestureRecognizer ();
			rtTapGesture.AddTarget (() => {
				//Go back
				float offset = _scrollView.ContentOffset.X;

				int i = (int)(offset / w);

				i=i+1;

				offset = i * w;

				PointF newCO = new PointF();
				newCO.Y = _scrollView.ContentOffset.Y;
				newCO.X = offset;

				_scrollView.ContentOffset = newCO;

				showPage(i);

			});
			rtTapGesture.NumberOfTapsRequired = 1;

			rtChevronImageView.UserInteractionEnabled = true;
			rtChevronImageView.AddGestureRecognizer (rtTapGesture);


			this.Add (rtChevronImageView);

			var scrollViewDelegate = new ScrollViewDelegate ();

			_scrollView.Delegate = scrollViewDelegate;

			scrollViewDelegate.ScolledDelegate = (UIScrollView scrollView) => {
				float offset = scrollView.ContentOffset.X;

				int i = (int)(offset / w);

				showPage(i);

			};

			this.Add (_scrollView);

			List<String> items = new List<string> ();
			items.Add ("Overview Map");
			items.Add ("Step 1");

			//AddButtons (items);
		}

		public void showPage(int i)
		{
			if(i ==0)
				ltChevronImageView.Hidden = true;
			else
				ltChevronImageView.Hidden = false;

			if(i == _buttons.Count-1)
				rtChevronImageView.Hidden = true;
			else
				rtChevronImageView.Hidden = false;

			if(PageChanged!=null)
				PageChanged(i);
		}

		public void AddItems(List<String> items)
		{
			AddButtons (items);
			this.SetNeedsDisplay ();
		}

		private void AddButtons (List<string> itemsList)
		{
			for(int i =0; i<itemsList.Count;i++)
			{
				string item = itemsList [i];
				var label = new UILabel ();

				label.Text = item;
				label.Frame = new RectangleF (padding * (i + 1) + (i * w),
					padding, w, h);
				label.BackgroundColor = UIColor.Clear;
				label.TextColor = UIColor.White;
				label.Font = UIFont.FromName ("HelveticaNeue-Thin", 24);
				label.TextAlignment = UITextAlignment.Center;
				_scrollView.AddSubview (label);
				_buttons.Add (label);
			}

			_scrollView.ContentSize = new SizeF ((w + padding) * _buttons.Count, h);
		}

		public HorizontalScrollView (List<String> itemsList)
		{
			AddButtons (itemsList);
		}

	}
}

