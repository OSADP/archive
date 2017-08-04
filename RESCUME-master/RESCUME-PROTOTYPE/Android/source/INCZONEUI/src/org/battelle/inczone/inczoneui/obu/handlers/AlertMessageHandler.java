/**
 * @file         inczoneui/obu/handlers/AlertMessageHandler.java
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

public final class AlertMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.handlers.AlertMessageHandler";

	public final static String ACTION_NEW_ALERT = PREFIX + ".action_new_alert";
	public final static String EXTRA_INFO = PREFIX + ".extra_info";

	public final static String TYPE_ID = "ALRT";

	private final ApplicationLog mAppLog;
	private final StatisticsLog mAlertLogs = new StatisticsLog("alerts", true, "Lane Change Threat Level", "Speed Threat Level", "Lane Change Sign Type", "Speed Sign Type", "Reduced Speed Limit", "Incident Is On Left", "Lane Count");

	public AlertMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		mAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		Gson gson = new Gson();
		AlertInformation info = gson.fromJson(object.toString(), AlertInformation.class);
		
		mAlertLogs.setIncludeTimestamp(true);
		mAlertLogs.log(info.lanechangethreatlevel, info.speedthreatlevel, info.lanechangesigntype, info.speedsigntype, info.speed, info.isonleft, info.lanecount);
		
		Intent intent = new Intent(ACTION_NEW_ALERT);
		intent.putExtra(EXTRA_INFO, info);
		LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);

	}

	@Override
	public void unregister() {

	}

	public static class AlertInformation implements Parcelable {

		/**
		 * Required for creating {@code AlertInformation} from
		 * {@code Parcelable}.
		 */
		public static Parcelable.Creator<AlertInformation> CREATOR = new Creator<AlertInformation>() {

			@Override
			public AlertInformation[] newArray(int size) {
				return new AlertInformation[size];
			}

			@Override
			public AlertInformation createFromParcel(Parcel source) {

				Gson builder = new Gson();

				try {
					return builder.fromJson(source.readString(), AlertInformation.class);
				} catch (JsonSyntaxException e) {
					e.printStackTrace();
				}
				return null;
			}
		};

		private static long sCurrentId = 0;
		private final long id;
		private int speedthreatlevel;
		private int speedsigntype;
		private int speed;
		private int lanechangethreatlevel;
		private int lanechangesigntype;
		private int lanecount;
		private boolean isonleft;

		public AlertInformation() {
			this.id = sCurrentId++;
			speedthreatlevel = -1;
			speedsigntype = -1;
			speed = 45;
			lanechangethreatlevel = -1;
			lanechangesigntype = -1;
			lanecount = 0;
			isonleft = false;
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

		public int getSpeedThreatLevel() {
			return speedthreatlevel;
		}

		public int getSpeedSignType() {
			return speedsigntype;
		}
		
		public int getSpeed() {
			return speed;
		}

		public int getLaneChangeThreatLevel() {
			return lanechangethreatlevel;
		}

		public int getLaneChangeSignType() {
			return lanechangesigntype;
		}

		public int getLaneCount() {
			return lanecount;
		}

		public boolean isIsOnLeft() {
			return isonleft;
		}
	}
}
