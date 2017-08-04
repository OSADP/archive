package org.battelle.inflo.infloui.alerts;

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

public class QWarnAlert implements Parcelable {

	private final static String PREFIX = "org.battelle.inflo.infloui.QWarnAlert";
	public final static String ACTION_ALERT = PREFIX + ".action_alert";
	public final static String EXTRA_ALERT = PREFIX + ".extra_alert";

	public static Parcelable.Creator<QWarnAlert> CREATOR = new Creator<QWarnAlert>() {

		@Override
		public QWarnAlert[] newArray(int size) {
			return new QWarnAlert[size];
		}

		@Override
		public QWarnAlert createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder.fromJson(source.readString(), QWarnAlert.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	private static StatisticsLog rQWarnLog = new StatisticsLog("qwarn", true,
			"Distance to BOQ", "Distance to FOQ", "Length of Queue",
			"Time to FOQ", "Recommended Action");

	public static void broadcastNewAlert(Context context, double distanceToBOQ,
			double distanceToFOQ, double lengthOfQ, int timeToFOQ,
			String recommendedAction) {

		QWarnAlert alert = new QWarnAlert(distanceToBOQ, distanceToFOQ,
				lengthOfQ, timeToFOQ, recommendedAction);

		rQWarnLog.log(distanceToBOQ, distanceToFOQ, lengthOfQ, timeToFOQ,
				recommendedAction);

		Intent broadcast = new Intent(ACTION_ALERT);
		broadcast.putExtra(EXTRA_ALERT, alert);
		LocalBroadcastManager.getInstance(context).sendBroadcast(broadcast);
	}

	public static QWarnAlert getEmptyAlert() {
		return new QWarnAlert(-1, -1, -1, -1, "");
	}

	private final Date receivedTime;
	private final double distanceToBOQ;
	private final double distanceToFOQ;
	private final double lengthOfQ;
	private final int timeToFOQ;
	private final String recommendedAction;

	private QWarnAlert(double distanceToBOQ, double distanceToFOQ,
			double lengthOfQ, int timeToFOQ, String recommendedAction) {

		receivedTime = DateTime.now().toDate();
		this.distanceToBOQ = distanceToBOQ;
		this.distanceToFOQ = distanceToFOQ;
		this.lengthOfQ = lengthOfQ;
		this.timeToFOQ = timeToFOQ;
		this.recommendedAction = recommendedAction;
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

	/**
	 * @return the receivedTime
	 */
	public DateTime getReceivedTime() {
		return new DateTime(receivedTime);
	}

	/**
	 * @return the distanceToBOQ
	 */
	public double getDistanceToBOQ() {
		return distanceToBOQ;
	}

	/**
	 * @return the distanceToFOQ
	 */
	public double getDistanceToFOQ() {
		return distanceToFOQ;
	}

	/**
	 * @return the lengthOfQ
	 */
	public double getLengthOfQ() {
		return lengthOfQ;
	}

	/**
	 * @return the timeToFOQ
	 */
	public int getTimeToFOQ() {
		return timeToFOQ;
	}

	/**
	 * @return the recommendedAction
	 */
	public String getRecommendedAction() {
		return recommendedAction;
	}

	public boolean isQueueAheadAlert() {
		return distanceToBOQ != -1 && distanceToFOQ == -1;
	}

	public boolean isInQueueAlert() {
		return distanceToBOQ == -1 && distanceToFOQ != -1;
	}

	public boolean isRelatedAlert(QWarnAlert alert) {

		boolean results = true;

		results &= !(this.isInQueueAlert() ^ alert.isInQueueAlert());
		results &= !(this.isQueueAheadAlert() ^ alert.isQueueAheadAlert());
		results &= alert.getRecommendedAction().equals(
				this.getRecommendedAction());

		return results;
	}
}
