/**
 * @file         infloui/obu/handlers/DiagnosticMessageHandler.java
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */

package org.battelle.inflo.infloui.obu.handlers;

import java.util.ArrayList;
import java.util.List;

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.StatisticsLog;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.content.Intent;
import android.os.Parcel;
import android.os.Parcelable;
import android.support.v4.content.LocalBroadcastManager;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

/**
 * {@code ObuMessageHandler} that receives diagnostic information from the OBU
 * and broadcasts them for other services to consume.
 * 
 * Uses typeid = 'DIA'
 * 
 */
public final class DiagnosticMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inflo.infloui.obu.handlers.DiagnosticMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_INFO = PREFIX + ".extra_info";

	public final static String TYPE_ID = "DIA";

	private final ApplicationLog rAppLog;
	private final StatisticsLog rRadioSignalLog = new StatisticsLog(
			"radio-signal", true, "RSSI", "DISTANCE", "BSM COUNT", "BSM RATE",
			"MILE MARKER", "HEADING", "SPEED");

	public DiagnosticMessageHandler(Context context,
			JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();
	}

	List<Long> rvBsmReceivedCounts = new ArrayList<Long>();
	List<Long> rvBsmReceivedTimes = new ArrayList<Long>();

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {
		try {

			double batteryLevel = object.has("bat") ? object.getDouble("bat")
					: 0.0;
			boolean gpsFixed = object.has("gps") ? object.getBoolean("gps")
					: false;
			boolean rsuConnected = object.has("rsu") ? object.getBoolean("rsu")
					: false;
			boolean queued = object.has("qState") ? object.getBoolean("qState")
					: false;
			String roadway = object.has("road") ? object.getString("road")
					: "Not Available";
			double mmarker = object.has("mmarker") ? object
					.getDouble("mmarker") : 0.0;
			long rvBsmsReceived = object.has("rvbsm") ? object.getLong("rvbsm")
					: 0;
			double rssi = object.has("rssi") ? object.getDouble("rssi") : 0.0;
			double heading = object.has("heading") ? object
					.getDouble("heading") : 0.0;
			double speed = object.has("speed") ? object.getDouble("speed")
					: 0.0;
			double rvdist = object.has("rvdist") ? object.getDouble("rvdist")
					: -1.0;
			int rvcount = object.has("rvcount") ? object.getInt("rvcount") : 0;
			long timRelayCount = object.has("timrelay") ? object
					.getLong("timrelay") : 0;
			String version = object.has("version") ? object
					.getString("version") : "Unavailable";
			long rsurssi = object.has("rsurssi") ? object.getLong("rsurssi")
					: 0;

			/*
			 * Statistics logging
			 */
			long currentMillis = System.currentTimeMillis();
			rvBsmReceivedCounts.add(rvBsmsReceived);
			rvBsmReceivedTimes.add(currentMillis);
			double rate = 0;
			if (rvBsmReceivedCounts.size() > 2) {
				rate = (rvBsmReceivedCounts.get(rvBsmReceivedCounts.size() - 1) - rvBsmReceivedCounts
						.get(0))
						* 1000.0
						/ (rvBsmReceivedTimes
								.get(rvBsmReceivedTimes.size() - 1) - rvBsmReceivedTimes
								.get(0));
			}
			if (rate == 0) {
				rssi = 0;
				rvdist = 0;
			}
			if (rvBsmReceivedCounts.size() > 5) {
				rvBsmReceivedCounts.remove(0);
				rvBsmReceivedTimes.remove(0);
			}
			rRadioSignalLog.log(rssi, rvdist, rvBsmsReceived, rate, mmarker,
					heading, speed);

			DiagnosticInformation info = new DiagnosticInformation(version,
					batteryLevel, gpsFixed, rsuConnected, queued, roadway,
					mmarker, rvBsmsReceived, rssi, heading, speed, rvdist,
					rvcount, timRelayCount, rsurssi);

			Intent intent = new Intent(ACTION_UPDATED);
			intent.putExtra(EXTRA_INFO, info);
			LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);

		} catch (JSONException e) {
			rAppLog.e("DiagnosticMessageHandler",
					"Error parsing Diagnostic Information", e);
		}

	}

	@Override
	public void unregister() {
	}

	public static class DiagnosticInformation implements Parcelable {

		/**
		 * Required for creating {@code DiagnosticInformation} from
		 * {@code Parcelable}.
		 */
		public static Parcelable.Creator<DiagnosticInformation> CREATOR = new Creator<DiagnosticInformation>() {

			@Override
			public DiagnosticInformation[] newArray(int size) {
				return new DiagnosticInformation[size];
			}

			@Override
			public DiagnosticInformation createFromParcel(Parcel source) {

				Gson builder = new Gson();

				try {
					return builder.fromJson(source.readString(),
							DiagnosticInformation.class);
				} catch (JsonSyntaxException e) {
					e.printStackTrace();
				}
				return null;
			}
		};

		private static long sCurrentId = 0;

		private final long id;
		private final String version;
		private final double batteryLevel;
		private final boolean gpsFixed;
		private final boolean rsuConnected;
		private final boolean queued;
		private final String roadway;
		private final double mmarker;
		private final long rvBsmsReceived;
		private final double rssi;
		private final double heading;
		private final double speed;
		private final double rvdist;
		private final int rvcount;
		private final long timRelayCount;
		private final long rsuRssi;

		public DiagnosticInformation(String version, double batteryLevel,
				boolean gpsFixed, boolean rsuConnected, boolean queued,
				String roadway, double mmarker, long rvBsmsReceived,
				double rssi, double heading, double speed, double rvdist,
				int rvcount, long timRelayCount, long rsuRssi) {
			this.id = sCurrentId++;
			this.version = version;
			this.batteryLevel = batteryLevel;
			this.gpsFixed = gpsFixed;
			this.rsuConnected = rsuConnected;
			this.queued = queued;
			this.roadway = roadway;
			this.mmarker = mmarker;
			this.rvBsmsReceived = rvBsmsReceived;
			this.rssi = rssi;
			this.heading = heading;
			this.speed = speed;
			this.rvdist = rvdist;
			this.rvcount = rvcount;
			this.timRelayCount = timRelayCount;
			this.rsuRssi = rsuRssi;
		}

		public DiagnosticInformation() {
			this.id = sCurrentId++;
			this.version = "Unavailable";
			this.batteryLevel = -1;
			this.gpsFixed = false;
			this.rsuConnected = false;
			this.queued = false;
			this.roadway = "Unavailable";
			this.mmarker = -1;
			this.rvBsmsReceived = -1;
			this.rssi = -1;
			this.heading = -1;
			this.speed = -1;
			this.rvdist = -1;
			this.rvcount = -1;
			this.timRelayCount = -1;
			this.rsuRssi = 0;
		}

		@Override
		public int describeContents() {
			return 0;
		}

		@Override
		public void writeToParcel(Parcel dest, int flags) {
			Gson builder = new Gson();
			dest.writeString(builder.toJson(this));
		}

		public long getId() {
			return id;
		}

		public String getVersion() {
			return version;
		}

		public double getBatteryLevel() {
			return batteryLevel;
		}

		public boolean isGpsFixed() {
			return gpsFixed;
		}

		public boolean isRsuConnected() {
			return rsuConnected;
		}

		public boolean isQueued() {
			return queued;
		}

		public String getRoadway() {
			return roadway;
		}

		public double getMmarker() {
			return mmarker;
		}

		public long getRvBsmsReceived() {
			return rvBsmsReceived;
		}

		public double getRssi() {
			return rssi;
		}

		public double getHeading() {
			return heading;
		}

		public double getSpeed() {
			return speed;
		}

		public double getRvdist() {
			return rvdist;
		}

		public int getRvcount() {
			return rvcount;
		}

		public long getTimRelayCount() {
			return timRelayCount;
		}

		public long getRsuRssi() {
			return rsuRssi;
		}

	}
}
