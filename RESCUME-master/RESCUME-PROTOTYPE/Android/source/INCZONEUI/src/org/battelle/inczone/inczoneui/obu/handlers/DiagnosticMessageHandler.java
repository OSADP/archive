/**
 * @file         inczoneui/obu/handlers/DiagnosticMessageHandler.java
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

package org.battelle.inczone.inczoneui.obu.handlers;

import org.battelle.inczone.inczoneui.ApplicationLog;
import org.battelle.inczone.inczoneui.StatisticsLog;
import org.battelle.inczone.inczoneui.obu.JsonMessageHandler;
import org.battelle.inczone.inczoneui.obu.ObuMessageHandler;
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

	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.handlers.DiagnosticMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_INFO = PREFIX + ".extra_info";

	public final static String TYPE_ID = "DIA";

	private final ApplicationLog rAppLog;
	private final StatisticsLog mGpsHistory = new StatisticsLog("gps-history", true);
	private final long mGpsHistoryStart = System.currentTimeMillis();

	public DiagnosticMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();
		mGpsHistory.setIncludeTimestamp(false);
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		Gson gson = new Gson();
		DiagnosticInformation info = gson.fromJson(object.toString(), DiagnosticInformation.class);

		mGpsHistory.log(String.valueOf((System.currentTimeMillis() - mGpsHistoryStart) / 1000.0), info.getLatitude(),
				info.getLongitude(), info.getHeading(), info.getSpeed());

		Intent intent = new Intent(ACTION_UPDATED);
		intent.putExtra(EXTRA_INFO, info);
		LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);

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
					return builder.fromJson(source.readString(), DiagnosticInformation.class);
				} catch (JsonSyntaxException e) {
					e.printStackTrace();
				}
				return null;
			}
		};

		private static long sCurrentId = 0;

		private final long id;
		private String version;
		private String versiondate;
		private String versionrepo;
		
		private String vehicleid;
		private int vehicleidlock;

		// GPS
		private int gpsfix;
		private double heading;
		private double speed;
		private double latitude;
		private double longitude;
		private double hdop;
		private double vdop;
		private int satcount;

		// Oncoming Data
		private String activetimid;
		private int evacount;
		private double rawlanelocation;

		public DiagnosticInformation() {
			this.id = sCurrentId++;
			this.version = "Unavailable";
			this.versiondate = "";
			this.versionrepo = "";
			this.vehicleid = "Unavailable";
			this.vehicleidlock = 0;
			this.gpsfix = 0;
			this.heading = -1;
			this.speed = -1;
			this.latitude = 0;
			this.longitude = 0;
			this.hdop = -1;
			this.vdop = -1;
			activetimid = "";
			evacount = -1;
			this.rawlanelocation = -1000.0;
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
			return String.format("%s/%s (%s)", version, versionrepo, versiondate);
		}

		public String getVehicleId() {
			return vehicleid;
		}

		public int getVehicleIdLock() {
			return vehicleidlock;
		}

		public int getGpsFix() {
			return gpsfix;
		}

		public double getHeading() {
			return heading;
		}

		public double getSpeed() {
			return speed;
		}

		public double getLatitude() {
			return latitude;
		}

		public double getLongitude() {
			return longitude;
		}

		public double getHdop() {
			return hdop;
		}

		public double getVdop() {
			return vdop;
		}

		public int getSatCount() {
			return satcount;
		}

		public String getActiveTimId() {
			return activetimid;
		}

		public int getEvaCount() {
			return evacount;
		}
		
		public double getRawLaneLocation()
		{
			return rawlanelocation;
		}
	}
}
