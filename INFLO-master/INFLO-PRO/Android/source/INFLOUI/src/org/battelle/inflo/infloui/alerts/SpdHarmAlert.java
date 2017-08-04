package org.battelle.inflo.infloui.alerts;

import java.util.ArrayList;
import java.util.Date;

import org.battelle.inflo.infloui.StatisticsLog;
import org.joda.time.DateTime;

import android.content.Context;
import android.content.Intent;
import android.os.Parcel;
import android.os.Parcelable;
import android.support.v4.content.LocalBroadcastManager;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

public class SpdHarmAlert implements Parcelable {

	private final static String PREFIX = "org.battelle.inflo.infloui.SpdHarmAlert";
	public final static String ACTION_ALERT = PREFIX + ".action_alert";
	public final static String EXTRA_ALERT = PREFIX + ".extra_alert";

	public static Parcelable.Creator<SpdHarmAlert> CREATOR = new Creator<SpdHarmAlert>() {

		@Override
		public SpdHarmAlert[] newArray(int size) {
			return new SpdHarmAlert[size];
		}

		@Override
		public SpdHarmAlert createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder
						.fromJson(source.readString(), SpdHarmAlert.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	private static StatisticsLog rSpdHarmLog = new StatisticsLog("spdharm",
			true, "Speed", "Justification");
	private static ArrayList<SpdHarmAlert> rAlertList = new ArrayList<SpdHarmAlert>();

	public static void broadcastNewAlert(Context context, int speed,
			String justificationText) {

		SpdHarmAlert alert = new SpdHarmAlert(speed, justificationText);

		rSpdHarmLog.log(speed, justificationText);
		rAlertList.add(0, alert);

		Intent broadcast = new Intent(ACTION_ALERT);
		broadcast.putExtra(EXTRA_ALERT, alert);
		LocalBroadcastManager.getInstance(context).sendBroadcast(broadcast);
	}

	public static SpdHarmAlert[] getAlerts() {
		return rAlertList.toArray(new SpdHarmAlert[rAlertList.size()]);
	}

	public static SpdHarmAlert getEmptyAlert() {
		return new SpdHarmAlert(-1, "");
	}

	private final Date receivedTime;
	private final int speed;
	private final String justificationText;

	private SpdHarmAlert(int speed, String justificationText) {

		receivedTime = DateTime.now().toDate();
		this.speed = speed;
		this.justificationText = justificationText;
	}

	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel arg0, int arg1) {

		Gson builder = new Gson();
		arg0.writeString(builder.toJson(this));
	}

	public DateTime getReceivedTime() {
		return new DateTime(receivedTime);
	}

	public int getSpeed() {
		return speed;
	}

	public String getJustificationText() {
		return justificationText;
	}

	public boolean isRelatedAlert(SpdHarmAlert alert) {

		boolean results = true;

		results &= this.getSpeed() == alert.getSpeed();
		results &= this.getJustificationText().equals(
				alert.getJustificationText());

		return results;
	}
}
