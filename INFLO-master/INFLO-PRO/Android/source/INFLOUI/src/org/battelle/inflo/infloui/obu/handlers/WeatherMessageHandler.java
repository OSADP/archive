/**
 * @file         infloui/obu/handlers/WeatherMessageHandler.java
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

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.battelle.inflo.infloui.weather.WeatherService;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives weather update broadcasts, formats
 * the messages, and transmits them to the OBU.
 * 
 * Uses typeid = 'WTR'
 * 
 */
public class WeatherMessageHandler extends ObuMessageHandler {

	public final static String TYPE_ID = "WTR";

	public final ApplicationLog rAppLog;

	public WeatherMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, "", sendHandler);

		LocalBroadcastManager.getInstance(context).registerReceiver(
				rWeatherReceiver,
				new IntentFilter(WeatherService.ACTION_UPDATE));

		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	public void unregister() {
		LocalBroadcastManager.getInstance(rContext).unregisterReceiver(
				rWeatherReceiver);
	}

	@Override
	protected void receiveMessageCallback(JSONObject object) {
		// This should never be called
	}

	BroadcastReceiver rWeatherReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {

			// Create JSON object from intent and send message
			JSONObject object = new JSONObject();

			try {
				object.put("typeid", TYPE_ID);
				object.put(
						"temp",
						intent.getExtras().getDouble(
								WeatherService.EXTRA_AMBIENT_TEMP, -1000));
				object.put(
						"pres",
						intent.getExtras().getDouble(
								WeatherService.EXTRA_PRESSURE, -1));
				object.put(
						"hum",
						intent.getExtras().getDouble(
								WeatherService.EXTRA_HUMIDITY, -1));

				rAppLog.v("WeatherMessageHandler",
						"weatherReceiver.onReceive() Sending Weather Data to OBU: "
								+ object.toString());

				sendMessage(object);

			} catch (JSONException e) {
				rAppLog.e(
						"WeatherMessageHandler",
						"weatherReceiver.onReceive() Error creating JSON Object",
						e);
			}
		}
	};
}
