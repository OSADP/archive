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
import org.battelle.inflo.infloui.alerts.SpdHarmAlert;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;

/**
 * {@code SpdAlertMessageHandler} that receives Spd Harm Alerts from the OBU,
 * and sends them to be processed and displayed
 * 
 * Uses typeid = 'SHA'
 * 
 */
public final class SpdAlertMessageHandler extends ObuMessageHandler {

	public final static String TYPE_ID = "SHA";

	private final ApplicationLog rAppLog;

	public SpdAlertMessageHandler(Context context,
			JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);

		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected void receiveMessageCallback(JSONObject object) {

		try {
			String text = object.has("text") ? object.getString("text") : "";
			int speed = object.has("speed") ? object.getInt("speed") : -1;

			SpdHarmAlert.broadcastNewAlert(rContext, speed, text);

		} catch (JSONException e) {
			rAppLog.e("SpdAlertMessageHandler", "Error parsing SpdHarm Alert",
					e);
		}
	}

	@Override
	public void unregister() {
	}
}
