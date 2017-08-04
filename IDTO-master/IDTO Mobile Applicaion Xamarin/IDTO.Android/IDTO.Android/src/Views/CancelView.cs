using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace IDTO.Android
{
	class CancelView : View
	{
		private Paint paint;
		private Color reddish;

		public CancelView(Context context) : base(context)
		{
			init ();
		}

		public CancelView(Context context, global::Android.Util.IAttributeSet attrs) : base(context, attrs)
		{
			init ();
		}

		public CancelView(Context context, global::Android.Util.IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{		
			init ();
		}

		private void init ()
		{
			paint = new Paint (PaintFlags.AntiAlias);
			reddish = Context.Resources.GetColor (Resource.Color.reddish);
		}

		protected override void OnDraw (Canvas canvas)
		{
			//calculate
			float width = canvas.Width;
			float height = canvas.Height;
			float cx = width / 2f;
			float cy = height / 2f;
			//Calc circle
			float smallest = cx < cy ? cx : cy;
			float radius = smallest-2f;
			radius = radius < 0 ? 0 : radius;
			//Calc rect
			float rectWidth = width * .667f;
			float rectHeight = height * .167f;
			float left = cx - (rectWidth / 2f);
			float top = cy - (rectHeight / 2f);
			float right = left + rectWidth;
			float bottom = top + rectHeight;
			RectF rect = new RectF (left, top, right, bottom);
			//draw Circle	
			paint.Color = reddish;
			canvas.DrawCircle (cx, cy, radius, paint);
			//draw bar
			paint.Color = Color.White;
			canvas.DrawRect (rect, paint);
		}

	}
}

