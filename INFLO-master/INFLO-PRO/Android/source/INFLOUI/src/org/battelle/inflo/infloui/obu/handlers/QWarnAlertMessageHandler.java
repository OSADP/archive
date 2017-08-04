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
import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.obu.JsonMessageHandler;
import org.battelle.inflo.infloui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;

/**
 * {@code ObuMessageHandler} that receives bsm data from the OBU, bundles them
 * together, and broadcast's the bundle to be posted to TME.
 * 
 * Uses typeid = 'QWA'
 * 
 */
public final class QWarnAlertMessageHandler extends ObuMessageHandler {

	public final static String TYPE_ID = "QWA";

	private final ApplicationLog rAppLog;

	public QWarnAlertMessageHandler(Context context,
			JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);

		rAppLog = ApplicationLog.getInstance();
	}

	@Override
	protected void receiveMessageCallback(JSONObject object) {

		try {
			String text = object.has("text") ? object.getString("text") : "";
			double distboq = object.has("distboq") ? object
					.getDouble("distboq") : -1;
			double distfoq = object.has("distfoq") ? object
					.getDouble("distfoq") : -1;
			double length = object.has("length") ? object.getDouble("length")
					: -1;
			int time = object.has("time") ? object.getInt("time") : -1;

			QWarnAlert.broadcastNewAlert(rContext, distboq, distfoq, length,
					time, text);

		} catch (JSONException e) {
			rAppLog.e("QWarnAlertMessageHandler", "Error parsing QWarn Alert",
					e);
		}
	}

	@Override
	public void unregister() {
	}
}
