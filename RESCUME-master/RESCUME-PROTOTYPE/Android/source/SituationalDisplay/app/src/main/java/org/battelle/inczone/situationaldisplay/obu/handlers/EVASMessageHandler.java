/**
 * @file         inczoneui/obu/handlers/EVASMessageHandler.java
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

package org.battelle.inczone.situationaldisplay.obu.handlers;

import android.content.Context;
import android.content.Intent;
import android.os.Parcel;
import android.os.Parcelable;
import android.support.v4.content.LocalBroadcastManager;
import android.util.Log;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

import org.battelle.inczone.situationaldisplay.ApplicationLog;
import org.battelle.inczone.situationaldisplay.obu.JsonMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.ObuMessageHandler;
import org.json.JSONObject;

import java.util.ArrayList;

/**
 * {@code ObuMessageHandler} that receives extracts of EVA messages from the OBU.
 * 
 * Uses typeid = 'EVAS'
 * 
 */
public final class EVASMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_INFO = PREFIX + ".extra_info";

	public final static String TYPE_ID = "EVAS";

	private final ApplicationLog rAppLog;

	public EVASMessageHandler(Context context,
                              JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		Gson gson = new Gson();
        try {
            EVASInformation info = gson.fromJson(object.toString(),
                    EVASInformation.class);
            Intent intent = new Intent(ACTION_UPDATED);
            intent.putExtra(EXTRA_INFO, info);
            LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);
        }
        catch(Exception e)
        {
            Log.e("EVA Received", e.toString());
        }

	}

	@Override
	public void unregister() {

	}

    public static class evaExtract {
        private String vehID;
        private long typeEvent;

        // GPS
        private double lat;
        private double lon;

        public evaExtract() {
            this.vehID = "";
            this.typeEvent = -1;
            this.lat = 0;
            this.lon = 0;
        }

        public String getId() {
            return vehID;
        }

        public double getLatitude() {
            return lat;
        }

        public double getLongitude() {
            return lon;
        }

        public long getTypeEvent() { return typeEvent; }
    };



	public static class EVASInformation implements Parcelable {

		/**
		 * Required for creating {@code EVASInformation} from
		 * {@code Parcelable}.
		 */
		public static Creator<EVASInformation> CREATOR = new Creator<EVASInformation>() {

			@Override
			public EVASInformation[] newArray(int size) {
				return new EVASInformation[size];
			}

			@Override
			public EVASInformation createFromParcel(Parcel source) {

				Gson builder = new Gson();

				try {
                    EVASInformation evasInformation = builder.fromJson(source.readString(), EVASInformation.class);
                    return evasInformation;
				} catch (JsonSyntaxException e) {
					e.printStackTrace();
				}
				return null;
			}
		};

        private ArrayList<evaExtract> eva;

		public EVASInformation() {
            eva = new ArrayList<evaExtract>();
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

        public ArrayList<evaExtract> getEva() { return eva; }
	}
}
