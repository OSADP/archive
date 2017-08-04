/**
 * @file         inczoneui/obu/handlers/TIMSMessageHandler.java
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
 * {@code ObuMessageHandler} that receives extracts of TIM messages from the OBU.
 *
 * Uses typeid = 'TIMS'
 * 
 */
public final class TIMSMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_INFO = PREFIX + ".extra_info";

	public final static String TYPE_ID = "TIMS";

	private final ApplicationLog rAppLog;

	public TIMSMessageHandler(Context context,
                              JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		Gson gson = new Gson();
		TIMSInformation info = gson.fromJson(object.toString(),
				TIMSInformation.class);
		Intent intent = new Intent(ACTION_UPDATED);
		intent.putExtra(EXTRA_INFO, info);
		LocalBroadcastManager.getInstance(rContext).sendBroadcast(intent);

	}

	@Override
	public void unregister() {

	}

	public static class TIMSInformation implements Parcelable {

		/**
		 * Required for creating {@code TIMSInformation} from
		 * {@code Parcelable}.
		 */
		public static Creator<TIMSInformation> CREATOR = new Creator<TIMSInformation>() {

			@Override
			public TIMSInformation[] newArray(int size) {
				return new TIMSInformation[size];
			}

			@Override
			public TIMSInformation createFromParcel(Parcel source) {

				Gson builder = new Gson();

				try {
					return builder.fromJson(source.readString(),
							TIMSInformation.class);
				} catch (JsonSyntaxException e) {
					e.printStackTrace();
				}
				return null;
			}
		};

        public static class Node {
            private double nLat;
            private double nLon;

            public Node() {
                this.nLat = 0;
                this.nLon = 0;
            }

            public double getNodeLatitude() {
                return nLat;
            }

            public double getNodeLongitude() {
                return nLon;
            }
        }

        public static class Region {

            private double aLat;
            private double aLon;
            private long lnW;
            private long dir;
            private ArrayList<Node> regionNodes;

            public Region() {
                this.aLat = 0;
                this.aLon = 0;
                this.lnW = -1;
                this.dir = -1;
                this.regionNodes = new ArrayList<Node>();
            }

            public long getLaneWidth() { return lnW; }

            public double getAnchorLatitude() {
                return aLat;
            }

            public double getAnchorLongitude() {
                return aLon;
            }

            public ArrayList<Node> getRegionNodes() { return regionNodes; }

            public long getDirectionality() { return dir; }

        }

        public static class DataFrame {

            private long lnT;
            private ArrayList<Region> region;

            public DataFrame() {
                this.lnT = -1;
                region = new ArrayList<Region>();
            }

            public long getLaneType() { return lnT; }

            public ArrayList<Region> getRegion() { return region; }
        };

        public static class timExtract {
            private ArrayList<DataFrame> dataFrame;

            public timExtract() {
                this.dataFrame = new ArrayList<DataFrame>();
            }

            public ArrayList<DataFrame> getDataFrame() { return dataFrame; }
        }

        private ArrayList<timExtract> tim;

        public TIMSInformation() {
            tim = new ArrayList<timExtract>();
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

        public ArrayList<timExtract> getTim() { return tim; }
	}
}
