/**
 * 
 */
package org.battelle.inczone.inczoneui.ui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.Shader.TileMode;
import android.util.AttributeSet;
import android.view.View;

/**
 *
 */
public class SliderBar extends View {

	//private RadialGradient highlight;

	int mWidth;
	int mHeight;
	double mSliderPosition = 0.5;
	boolean mSliderShown = true;
	
	LinearGradient mBackgroundGradient;
	Paint mBackgroundPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
	Rect mBackgroundRect;

	Paint mSliderPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
	
	/**
	 * @param context
	 */
	public SliderBar(Context context) {
		super(context);
	}

	/**
	 * @param context
	 * @param attrs
	 */
	public SliderBar(Context context, AttributeSet attrs) {
		super(context, attrs);
	}

	/**
	 * @param context
	 * @param attrs
	 * @param defStyle
	 */
	public SliderBar(Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
	}

	@Override
	public void onSizeChanged(int w, int h, int oldw, int oldh) {
		mWidth = w;
		mHeight = h;
		mBackgroundRect = new Rect(0, 0, mWidth, mHeight);
		//mBackgroundGradient = new LinearGradient(0, 0, mWidth, 0, new int[] {Color.BLACK, Color.DKGRAY, Color.BLACK, Color.DKGRAY, Color.BLACK}, new float[] {0.0f, 0.03f, 0.5f, 0.97f, 1.0f}, TileMode.CLAMP);
		mBackgroundGradient = new LinearGradient(0, 0, mWidth, 0, new int[] {Color.DKGRAY, Color.BLACK, Color.DKGRAY}, new float[] {0.0f, 0.5f, 1.0f}, TileMode.CLAMP);
		
	}

	@Override
	public void onDraw(Canvas canvas) {
		super.onDraw(canvas);

		mBackgroundPaint.setShader(mBackgroundGradient);
		//canvas.drawRect(mBackgroundRect, mBackgroundPaint);

		mSliderPaint.setStrokeWidth((float) (mWidth * 0.01));
		
		if (Math.abs(mSliderPosition - 0.5) < 0.1)
		{
			int color = (int) (255 * (Math.abs(mSliderPosition - 0.5) / 0.1));
			mSliderPaint.setColor(Color.rgb((int)(color*0.3), (int)(color*0.3), color));
		}
		int color = Math.min((int) (255 * (Math.abs(mSliderPosition - 0.5) / 0.05)), 255);
		mSliderPaint.setColor(Color.rgb((int)(color*0.0), (int)(color*0.71), (int)(color*0.89)));
		
		float radius = (float)(Math.min(mWidth, mHeight) / 2.5);
		float usableWidth = (float) Math.min((mWidth - 2.0 * radius), mWidth * 0.94);
		if (mSliderShown)
		{
			//canvas.drawCircle((float) ((mWidth - usableWidth)/2.0 + mSliderPosition * usableWidth), (float)(mHeight / 2.0), radius, mSliderPaint);
			canvas.drawLine((float) ((mWidth - usableWidth)/2.0 + mSliderPosition * usableWidth), 0, (float) ((mWidth - usableWidth)/2.0 + mSliderPosition * usableWidth), mHeight, mSliderPaint);
		}
	}
	
	public void setSliderPosition(double position)
	{
		this.mSliderPosition = Math.max(0, Math.min(1, position));
		
		this.invalidate();
	}
	
	public void setSliderShown(boolean show)
	{
		this.mSliderShown = show;
		
		this.invalidate();
	}

}
