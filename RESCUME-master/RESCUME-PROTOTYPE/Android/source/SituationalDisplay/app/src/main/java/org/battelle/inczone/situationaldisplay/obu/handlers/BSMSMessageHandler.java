/**
 * @file         inczoneui/obu/handlers/BSMSMessageHandler.java
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
 * {@code ObuMessageHandler} that receives extracts of bsm messages from the OBU.
 *
 * Uses typeid = 'BSMS'
 *
 */
public final class BSMSMessageHandler extends ObuMessageHandler {

    private final static String PREFIX = "org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler";

    public final static String ACTION_UPDATED = PREFIX + ".action_updated";
    public final static String EXTRA_INFO = PREFIX + ".extra_info";

    public final static String TYPE_ID = "BSMS";

    private final ApplicationLog rAppLog;

    public BSMSMessageHandler(Context context,
                              JsonMessageHandler sendHandler) {
        super(context, TYPE_ID, sendHandler);
        rAppLog = ApplicationLog.getInstance();
    }

    @Override
    protected synchronized void receiveMessageCallback(JSONObject object) {

        Gson gson = new Gson();
        try {
            BSMSInformation info = gson.fromJson(object.toString(),
                    BSMSInformation.class);
            Intent intent = new Intent(ACTION_UPDATED);
            intent.putExtra(EXTRA_INFO, info);
            LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);
        }
        catch(Exception e)
        {
            Log.e("Bsm Received", e.toString());
        }

    }

    @Override
    public void unregister() {

    }

    public static class bsmExtract {
        private String vehID;

        // GPS
        private double lat;
        private double lon;

        public bsmExtract() {
            this.vehID = "";
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

    };



    public static class BSMSInformation implements Parcelable {

        /**
         * Required for creating {@code bsmSInformation} from
         * {@code Parcelable}.
         */
        public static Creator<BSMSInformation> CREATOR = new Creator<BSMSInformation>() {

            @Override
            public BSMSInformation[] newArray(int size) {
                return new BSMSInformation[size];
            }

            @Override
            public BSMSInformation createFromParcel(Parcel source) {

                Gson builder = new Gson();

                try {
                    BSMSInformation BSMSInformation = builder.fromJson(source.readString(), BSMSInformation.class);
                    return BSMSInformation;
                } catch (JsonSyntaxException e) {
                    e.printStackTrace();
                }
                return null;
            }
        };

        private ArrayList<bsmExtract> bsm;

        public BSMSInformation() {
            bsm = new ArrayList<bsmExtract>();
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

        public ArrayList<bsmExtract> getBsm() { return bsm; }
    }
}
