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
	class BarView : View
	{

		private Paint paint;

		public BarView(Context context) : base(context)
		{
			init ();
		}

		public BarView(Context context, global::Android.Util.IAttributeSet attrs) : base(context, attrs)
		{
			init ();
		}

		public BarView(Context context, global::Android.Util.IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{		
			init ();
		}

		private void init ()
		{
			paint = new Paint (PaintFlags.AntiAlias);
			paint.StrokeWidth = 8;
			paint.StrokeCap = Paint.Cap.Square;
		

		}

		protected override void OnDraw (Canvas canvas)
		{          
			float width = canvas.Width;
			float height = canvas.Height;
			float innerRadius = width / 6;
			Color orange = Context.Resources.GetColor (Resource.Color.idto_orange);

			PointF circleCenter = new PointF ();
			circleCenter.X = width / 2;
			circleCenter.Y = width / 2;

			Path path = new Path ();
			path.MoveTo (circleCenter.X, circleCenter.Y);
			path.LineTo (circleCenter.X, height);
			paint.SetStyle (Paint.Style.Stroke);
			paint.Color = orange;
			canvas.DrawPath (path, paint);

			paint.SetStyle (Paint.Style.Fill);
			paint.Color = orange;
			canvas.DrawCircle (circleCenter.X, circleCenter.Y, innerRadius * 2, paint);
			paint.Color = Color.White;
			canvas.DrawCircle (circleCenter.X, circleCenter.Y, innerRadius, paint);

		}
	}
}

