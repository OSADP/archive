/**
 * @file         infloui/obu/handlers/BsmMessageHandler.java
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
import org.battelle.inflo.infloui.StatisticsLog;
import org.battelle.inflo.infloui.cloud.TmeCloudService;
import org.battelle.inflo.infloui.cloud.TmeRequest;
import org.battelle.inflo.infloui.cloud.TmeRequestMethod;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Handler;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives bsm data from the OBU, bundles them
 * together, and broadcast's the bundle to be posted to TME.
 * 
 * Uses typeid = 'BSM'
 */
public final class BsmMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inflo.infloui.obu.handlers.BsmMessageHandler";

	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_RECEIVED_COUNT = PREFIX + ".extra_received_count";
	public final static String EXTRA_POSTED_COUNT = PREFIX + ".extra_posted_count";

	public final static String TYPE_ID = "BSM";

	/*
	 * Some constants loaded from configuration
	 */
	private final int bsmBundleMaxSize;
	private final int bsmTimeoutPeriod;

	private final SharedPreferences rSettings;
	private final Handler rHandler;
	private final StatisticsLog rBsmLog = new StatisticsLog("received-bsms", true, "BSM Json");
	private JSONArray rBsmArray = new JSONArray();
	private final ApplicationLog rAppLog;
	/*
	 * Statistics
	 */
	private int statsReceivedCount = 0;
	private int statsPostedCount = 0;

	public BsmMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);

		rSettings = context.getSharedPreferences(
				context.getResources().getString(R.string.setting_file_name),
				Context.MODE_MULTI_PROCESS);

		bsmBundleMaxSize = Integer.parseInt(context.getResources().getString(
				R.string.config_bsmBundleMaxSize));
		bsmTimeoutPeriod = Integer.parseInt(context.getResources().getString(
				R.string.config_bsmBundleTimeout));

		rHandler = new Handler();

		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		rBsmLog.log(object);

		// Put BSM into bundle
		rBsmArray.put(object);

		// Stats and logging
		rAppLog.v("BsmMessageHandler", "Received message");
		statsReceivedCount++;

		// Send bundle or post a runnable to send the bundle if there is a
		// timeout.
		if (rBsmArray.length() == bsmBundleMaxSize)
			postBundle();
		else
			rHandler.postDelayed(new Runnable() {
				@Override
				public void run() {
					postBundle();
				}
			}, bsmTimeoutPeriod);

		broadcastStatistics();
	}

	private void broadcastStatistics() {
		Intent updateIntent = new Intent(ACTION_UPDATED);
		updateIntent.putExtra(EXTRA_RECEIVED_COUNT, statsReceivedCount);
		updateIntent.putExtra(EXTRA_POSTED_COUNT, statsPostedCount);

		LocalBroadcastManager.getInstance(rContext).sendBroadcast(updateIntent);
	}

	private synchronized void postBundle() {

		// We're sending one, so stop the timeout event.
		rHandler.removeCallbacksAndMessages(null);

		// Format the BSM
		JSONObject bsmBundle = new JSONObject();
		try {
			bsmBundle.put("typeid", "BSB");
			bsmBundle.put("payload", rBsmArray);
		} catch (JSONException e) {
			e.printStackTrace();
		}

		// Reset the BSM array for the next collection.
		rBsmArray = new JSONArray();

		String url = rSettings.getString(
				rContext.getResources().getString(R.string.setting_tmeBsmWebUrl_key), "");

		if (url.isEmpty()) {
			rAppLog.w("BsmMessageHandler", "Url is empty. Can not post BsmBundle");
			return;
		}

		// Stats and logging
		statsPostedCount++;
		rAppLog.i("BsmMessageHandler", "Posting message");

		// Post the bsmBundle to TME
		TmeRequest postRequest = new TmeRequest(TmeRequestMethod.post, url);
		postRequest.setBody(bsmBundle.toString());

		TmeCloudService.newRequest(this.rContext, postRequest);
	}

	@Override
	public void unregister() {
		rHandler.removeCallbacksAndMessages(null);
	}
}
