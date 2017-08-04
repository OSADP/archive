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
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONObject;

import android.content.Context;
import android.content.SharedPreferences;

/**
 * {@code ObuMessageHandler} that receives diagnostic information from the OBU
 * and broadcasts them for other services to consume.
 * 
 * Uses typeid = 'TRQ'
 * 
 */
public final class TimMessageHandler extends ObuMessageHandler {

	// private final static String PREFIX =
	// "org.battelle.inflo.infloui.obu.handlers.TimMessageHandler";

	public final static String TYPE_ID = "TIM";

	private final SharedPreferences rSettings;
	private final ApplicationLog rAppLog;

	public TimMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
		rAppLog = ApplicationLog.getInstance();

		rSettings = context.getSharedPreferences(context.getResources()
				.getString(R.string.setting_file_name),
				Context.MODE_MULTI_PROCESS);
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		String url = rSettings.getString(
				rContext.getResources().getString(
						R.string.setting_tmeTimWebUrl_key), "");

		if (url.isEmpty()) {
			rAppLog.w("TimMessageHandler",
					"Url is empty. Can not post Tim Message");
			return;
		}

		TmeRequest postRequest = new TmeRequest(TmeRequestMethod.post, url);
		postRequest.setBody(object.toString());
		TmeCloudService.newRequest(this.rContext, postRequest);

		rAppLog.d("TimMessageHandler", "Posted TIM Message.");
	}

	@Override
	public void unregister() {
	}
}
