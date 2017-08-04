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

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.R;
import org.battelle.inflo.infloui.cloud.TmeCloudService;
import org.battelle.inflo.infloui.cloud.TmeRequest;
import org.battelle.inflo.infloui.cloud.TmeRequestMethod;
import org.battelle.inflo.infloui.cloud.TmeResponse;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives diagnostic information from the OBU
 * and broadcasts them for other services to consume.
 * 
 * Uses typeid = 'TRQ'
 * 
 */
public final class TimRequestMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inflo.infloui.obu.handlers.TimRequestMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	private final static String ACTION_TIM_CALLBACK = PREFIX
			+ ".action_tim_callback";

	public final static String EXTRA_TIM_REQUEST_COUNT = PREFIX
			+ ".extra_tim_request_count";
	public final static String EXTRA_TIM_RESPONSE_COUNT = PREFIX
			+ ".extra_tim_response_count";

	public final static String TYPE_ID = "TRQ";

	private final SharedPreferences rSettings;
	private final ApplicationLog rAppLog;
	/*
	 * Statistics
	 */
	private int statsTimRequestCount = 0;
	private int statsTimResponseCount = 0;

	public TimRequestMessageHandler(Context context,
			JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();

		rSettings = context.getSharedPreferences(context.getResources()
				.getString(R.string.setting_file_name),
				Context.MODE_MULTI_PROCESS);

		LocalBroadcastManager.getInstance(rContext).registerReceiver(
				rTimCallbackReceiver, new IntentFilter(ACTION_TIM_CALLBACK));

		String url = rSettings.getString(
				rContext.getResources().getString(
						R.string.setting_tmeTimWebUrl_key), "");
		if (!url.isEmpty()) {
			String roadwayid = "1";
			double mm = 0.1;
			url = url + String.format("?roadwayid=%s&mm=%f", roadwayid, mm);

			TmeRequest getRequest = new TmeRequest(TmeRequestMethod.get, url);
			TmeCloudService.newRequest(this.rContext, getRequest);
		}

	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		String url = rSettings.getString(
				rContext.getResources().getString(
						R.string.setting_tmeTimWebUrl_key), "");
		if (url.isEmpty()) {
			rAppLog.w("TimRequestMessageHandler",
					"Url is empty. Can not get Tim Messages");
			return;
		}

		try {
			String roadwayid = object.getString("roadwayid");
			double mm = object.getDouble("mm");
			url = url + String.format("?roadwayid=%s&mm=%f", roadwayid, mm);

			TmeRequest getRequest = new TmeRequest(TmeRequestMethod.get, url);
			getRequest.setResponseTarget(ACTION_TIM_CALLBACK);
			TmeCloudService.newRequest(this.rContext, getRequest);

		} catch (JSONException e) {
			rAppLog.e(
					"TimRequestMessageHandler",
					"Error Requesting TIM Message.  RoadwayID or MM unavailable.",
					e);
		}

		statsTimRequestCount++;
		broadcastStatistics();
	}

	@Override
	public void unregister() {
		LocalBroadcastManager.getInstance(rContext).unregisterReceiver(
				rTimCallbackReceiver);
	}

	private void broadcastStatistics() {
		Intent updateIntent = new Intent(ACTION_UPDATED);
		updateIntent.putExtra(EXTRA_TIM_REQUEST_COUNT, statsTimRequestCount);
		updateIntent.putExtra(EXTRA_TIM_RESPONSE_COUNT, statsTimResponseCount);

		LocalBroadcastManager.getInstance(rContext).sendBroadcast(updateIntent);
	}

	BroadcastReceiver rTimCallbackReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {

			TmeResponse response = (TmeResponse) intent
					.getParcelableExtra(TmeCloudService.EXTRA_RESPONSE);

			rAppLog.v("TimRequestMessageHandler", "Received TIM Message: "
					+ response.getBody());

			if (response.isSuccessful()) {
				try {
					JSONObject object = new JSONObject(response.getBody());
					sendMessage(object);
					rAppLog.v("TimRequestMessageHandler",
							"TIM Message sent to OBU");

					statsTimResponseCount++;
					broadcastStatistics();

				} catch (JSONException e) {
					rAppLog.e("TimRequestMessageHandler",
							"Error parsing TIM Message response to JSON", e);
				}
			}

		}
	};
}
