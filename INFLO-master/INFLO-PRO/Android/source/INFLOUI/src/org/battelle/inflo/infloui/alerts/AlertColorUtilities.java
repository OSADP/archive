package org.battelle.inflo.infloui.alerts;

import org.battelle.inflo.infloui.ApplicationModel;

import android.graphics.Color;

public class AlertColorUtilities {
	private AlertColorUtilities() {
	}

	@SuppressWarnings("unused")
	private final static int L_GREEN = 0x00;
	private final static int L_YELLOW = 0x01;
	private final static int L_RED = 0x02;

	private final static double THRESHOLD_QWARN_YELLOW = 1.5;
	private final static double THRESHOLD_QWARN_RED = 0.5;

	private final static int THRESHOLD_SPDHARM_YELLOW = 50;
	private final static int THRESHOLD_SPDHARM_RED = 30;

	public static int getColor(QWarnAlert alert) {
		return getColorFromMask(getAlertMask(alert));
	}

	public static int getColor(SpdHarmAlert alert) {
		return getColorFromMask(getAlertMask(alert));
	}

	public static int getColor(ApplicationModel model) {
		return getColorFromMask(getAlertMask(model.alertQWarn) | getAlertMask(model.alertSpdHarm));
	}

	private static int getColorFromMask(int mask) {
		if ((mask & L_RED) > 0)
			return Color.parseColor("#ff4343");
		if ((mask & L_YELLOW) > 0)
			return Color.parseColor("#ff8700");

		return Color.parseColor("#98cb00");
	}

	private static int getAlertMask(QWarnAlert alert) {
		int alertMask = 0;

		if (alert.getDistanceToBOQ() < 0)
			return 0;

		if (alert.getDistanceToBOQ() < THRESHOLD_QWARN_RED)
			alertMask |= L_RED;
		else if (alert.getDistanceToBOQ() < THRESHOLD_QWARN_YELLOW)
			alertMask |= L_YELLOW;

		return alertMask;
	}

	private static int getAlertMask(SpdHarmAlert alert) {
		int alertMask = 0;

		if (alert.getSpeed() < 0)
			return 0;

		if (alert.getSpeed() < THRESHOLD_SPDHARM_RED)
			alertMask |= L_RED;
		else if (alert.getSpeed() < THRESHOLD_SPDHARM_YELLOW)
			alertMask |= L_YELLOW;

		return alertMask;
	}
}
