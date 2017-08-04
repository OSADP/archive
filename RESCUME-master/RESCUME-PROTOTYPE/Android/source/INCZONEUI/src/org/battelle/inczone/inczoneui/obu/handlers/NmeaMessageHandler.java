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
import org.battelle.inczone.inczoneui.ntrip.NTripService;
import org.battelle.inczone.inczoneui.obu.JsonMessageHandler;
import org.battelle.inczone.inczoneui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives diagnostic information from the OBU
 * and broadcasts them for other services to consume.
 * 
 * Uses typeid = 'DIA'
 * 
 */
public final class NmeaMessageHandler extends ObuMessageHandler {

	@SuppressWarnings("unused")
	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.handlers.NmeaMessageHandler";

	public final static String TYPE_ID = "NMEA";

	private final ApplicationLog rAppLog;
	private final Context rContext;

	public NmeaMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);

		rAppLog = ApplicationLog.getInstance();
		rContext = context;

		LocalBroadcastManager.getInstance(rContext).registerReceiver(rRtcmReceiver,
				new IntentFilter(NTripService.ACTION_RECEIVED_RTCM));
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {
		try {

			String GgaString = object.has("gga") ? object.getString("gga") : null;

			//Intent intent = new Intent(rContext, NTripService.class);
			//intent.putExtra(NTripService.EXTRA_GGA, GgaString);

			//rContext.startService(intent);

		} catch (JSONException e) {
			rAppLog.e("NmeaMessageHandler", "Error parsing Diagnostic Information", e);
		}

	}

	@Override
	public void unregister() {
		LocalBroadcastManager.getInstance(rContext).unregisterReceiver(rRtcmReceiver);
	}

	BroadcastReceiver rRtcmReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			if (intent.hasExtra(NTripService.EXTRA_RTCM2_3)) {
				byte data[] = intent.getByteArrayExtra(NTripService.EXTRA_RTCM2_3);

				StringBuilder sb = new StringBuilder();

				for (byte b : data)
					sb.append(String.format("%02x", b));

				JSONObject object = new JSONObject();
				try {
					object.put("typeid", "RTCM");
					object.put("2.3", sb.toString());
					sendMessage(object);
				} catch (JSONException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}
	};
}
