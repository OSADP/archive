/**
 * 
 */
package org.battelle.inczone.situationaldisplay.ui;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.LinearGradient;
import android.graphics.Paint;
import android.graphics.RadialGradient;
import android.graphics.RectF;
import android.graphics.Shader;
import android.util.AttributeSet;
import android.view.View;

/**
 *
 */
public class LEDDisplay extends View {

	private boolean active = false;

	private int activeColor = Color.RED;
	private int inactiveColor = Color.BLACK;
	private int currentColor = Color.RED;

	private LinearGradient bezelGradient1;
	private LinearGradient bezelGradient2;
	private LinearGradient bezelGradient3;
	private RadialGradient highlight;

	private RectF bezelRect1;
	private RectF bezelRect2;
	private RectF bezelRect3;
	private RectF ledRect;
	private RectF highlightRect;

	int Width = this.getWidth();
	int Height = this.getHeight();

	private Paint curPaint = new Paint(Paint.ANTI_ALIAS_FLAG);

	/**
	 * @param context
	 */
	public LEDDisplay(Context context) {
		super(context);
	}

	/**
	 * @param context
	 * @param attrs
	 */
	public LEDDisplay(Context context, AttributeSet attrs) {
		super(context, attrs);
	}

	/**
	 * @param context
	 * @param attrs
	 * @param defStyle
	 */
	public LEDDisplay(Context context, AttributeSet attrs, int defStyle) {
		super(context, attrs, defStyle);
	}

	private void setGradients() {

		if (active) {
			currentColor = getActiveColor();
		} else {
			currentColor = getInactiveColor();
		}
		bezelRect1 = new RectF(0, 0, Width, Height);
		bezelGradient1 = new LinearGradient(0, 0, Width, Height, Color.GRAY,
				Color.DKGRAY, Shader.TileMode.CLAMP);
		bezelRect2 = new RectF(2, 2, Width - 2, Height - 2);
		bezelGradient2 = new LinearGradient(0, 0, Width, Height, Color.WHITE,
				Color.GRAY, Shader.TileMode.CLAMP);
		bezelRect3 = new RectF(4, 4, Width - 4, Height - 4);
		bezelGradient3 = new LinearGradient(0, 0, Width, Height, Color.DKGRAY,
				Color.GRAY, Shader.TileMode.CLAMP);
		ledRect = new RectF(6, 6, Width - 6, Height - 6);
		float x1 = Width / 8;
		if (x1 < 7)
			x1 = 7;
		float y1 = Height / 8;
		if (y1 < 7)
			y1 = 7;
		float x2 = (Width - 6) / 4 * 3;
		float y2 = (Height - 6) / 4 * 3;
		highlightRect = new RectF(x1, y1, x2, y2);
		float rad = (x2 - x1) / 2;
		if (rad <= 0)
			rad = .1f;
		highlight = new RadialGradient(x1 + ((x2 - x1) / 2), y1
				+ ((y2 - y1) / 2), rad, Color.WHITE, currentColor,
				Shader.TileMode.CLAMP);
		this.invalidate();
	}

	@Override
	public void onSizeChanged(int w, int h, int oldw, int oldh) {
		Width = w;
		Height = h;

		setGradients();
	}

	@Override
	public void onDraw(Canvas canvas) {
		super.onDraw(canvas);

		curPaint.setColor(currentColor);

		curPaint.setShader(bezelGradient1);
		canvas.drawOval(bezelRect1, curPaint);

		curPaint.setShader(bezelGradient2);
		canvas.drawOval(bezelRect2, curPaint);

		curPaint.setShader(bezelGradient3);
		canvas.drawOval(bezelRect3, curPaint);

		curPaint.setShader(null);
		canvas.drawOval(ledRect, curPaint);

		curPaint.setShader(highlight);
		canvas.drawOval(highlightRect, curPaint);

	}

	public int getActiveColor() {
		return activeColor;
	}

	public void setActiveColor(int activeColor) {
		this.activeColor = activeColor;
		setGradients();
	}

	public int getInactiveColor() {
		return inactiveColor;
	}

	public void setInactiveColor(int inactiveColor) {
		this.inactiveColor = inactiveColor;
		setGradients();
	}

	public int getCurrentColor() {
		return currentColor;
	}

	public void setCurrentColor(int currentColor) {
		this.currentColor = currentColor;
		setGradients();
	}

	public boolean isActive() {
		return active;
	}

	public void setActive(boolean active) {
		this.active = active;
		setGradients();
	}
}
