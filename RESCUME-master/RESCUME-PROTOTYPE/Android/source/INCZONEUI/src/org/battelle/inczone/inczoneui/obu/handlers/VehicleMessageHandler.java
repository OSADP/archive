/**
 * @file         inczoneui/obu/handlers/VehicleMessageHandler.java
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
import org.battelle.inczone.inczoneui.obu.JsonMessageHandler;
import org.battelle.inczone.inczoneui.obu.ObuMessageHandler;
import org.battelle.inczone.inczoneui.odbii.VehicleDiagnosticsService;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives vehicle update broadcasts, formats
 * the messages, and transmits them to the OBU.
 * 
 * Uses typeid = 'VEH'
 * 
 */
public class VehicleMessageHandler extends ObuMessageHandler {

	public final static String TYPE_ID = "VEH";

	public final ApplicationLog rAppLog;

	public VehicleMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, "", sendHandler);

		LocalBroadcastManager.getInstance(context).registerReceiver(
				rVehicleDiagnosticsReceiver,
				new IntentFilter(VehicleDiagnosticsService.ACTION_DATA_UPDATE));

		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	public void unregister() {
		LocalBroadcastManager.getInstance(rContext).unregisterReceiver(
				rVehicleDiagnosticsReceiver);
	}

	@Override
	protected void receiveMessageCallback(JSONObject object) {
		// This should never be called
	}

	BroadcastReceiver rVehicleDiagnosticsReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {

			try {
				JSONObject object = new JSONObject(
						intent.getStringExtra(VehicleDiagnosticsService.EXTRA_DATA));
				object.put("typeid", TYPE_ID);

				sendMessage(object);

				rAppLog.v("VehicleMessageHandler",
						"rVehicleDiagnosticsReceiver.onReceive() Sending Vehicle Data to OBU: "
								+ object.toString());

			} catch (JSONException e) {
				rAppLog.e(
						"VehicleMessageHandler",
						"rVehicleDiagnosticsReceiver.onReceive() Error creating JSON Object",
						e);
			}
		}
	};
}
