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

import org.battelle.inczone.inczoneui.obu.JsonMessageHandler;
import org.battelle.inczone.inczoneui.obu.ObuMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.content.Intent;
import android.support.v4.content.LocalBroadcastManager;

/**
 * {@code ObuMessageHandler} that receives diagnostic information from the OBU
 * and broadcasts them for other services to consume.
 * 
 * Uses typeid = 'TRQ'
 * 
 */
public final class ConMessageHandler extends ObuMessageHandler {

	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.handlers.ConMessageHandler";

	public final static String ACTION_MESSAGE = PREFIX + ".action_message";

	public final static String EXTRA_MSG = PREFIX + ".extra_msg";

	public final static String TYPE_ID = "CON";

	public ConMessageHandler(Context context, JsonMessageHandler sendHandler) {
		super(context, TYPE_ID, sendHandler);
	}

	@Override
	protected synchronized void receiveMessageCallback(JSONObject object) {

		if (object.has("msg")) {
			Intent intent = new Intent(ACTION_MESSAGE);
			try {
				intent.putExtra(EXTRA_MSG, object.getString("msg"));
				LocalBroadcastManager.getInstance(rContext).sendBroadcastSync(
						intent);

			} catch (JSONException e) {
				e.printStackTrace();
			}
		}
	}

	@Override
	public void unregister() {
	}

}
