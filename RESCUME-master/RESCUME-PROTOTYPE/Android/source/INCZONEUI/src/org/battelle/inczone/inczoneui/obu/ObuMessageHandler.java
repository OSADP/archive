/**
 * @file         inczoneui/obu/ObuBluetoothHandler.java
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

package org.battelle.inczone.inczoneui.obu;

import org.battelle.inczone.inczoneui.ApplicationLog;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;

/**
 * Abstract class for any message handlers that will be processing messages
 * going to or coming from the OBU device.
 * 
 * Provides a callback function for when data is received from the OBU, and a
 * send message function for when sending data to the OBU.
 * 
 * @author branch
 * 
 */
public abstract class ObuMessageHandler implements JsonMessageHandler {

	protected final String typeid;
	private final JsonMessageHandler rSendHandler;
	protected final Context rContext;
	private final ApplicationLog rAppLog;

	protected ObuMessageHandler(Context context, String typeid,
			JsonMessageHandler sendHandler) {
		this.typeid = typeid;
		this.rSendHandler = sendHandler;
		this.rContext = context;
		rAppLog = ApplicationLog.getInstance();
	}

	protected void sendMessage(JSONObject object) {
		this.rSendHandler.handleMessage(object);
	}

	@Override
	public boolean handleMessage(JSONObject object) {

		try {
			if (object.getString("typeid").equals(typeid)) {
				receiveMessageCallback(object);
				return true;
			}
		} catch (JSONException e) {
			rAppLog.e("ObuMessageHandler",
					"Error handling message: " + e.getMessage());
		}

		return false;
	}

	abstract public void unregister();

	abstract protected void receiveMessageCallback(JSONObject object);
}
