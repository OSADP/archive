/**
 * @file         infloui/cloud/TmeCloudVolleyRequest.java
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

package org.battelle.inflo.infloui.cloud;

import com.android.volley.AuthFailureError;
import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.Response;
import com.android.volley.Response.ErrorListener;
import com.android.volley.Response.Listener;

/**
 * A Volley {@code Request} for working with JSON REST services.
 * 
 * Note: This differs from the default Volley JSON Request by not requiring that
 * the json messages be {@code JSONObject}s
 * 
 * @author branch
 * 
 */
public class TmeCloudVolleyRequest extends Request<String> {

	String strBody;
	/**
	 * Listener for calling back on the response.
	 */
	Listener<String> rListener;

	public TmeCloudVolleyRequest(int method, String url, String body, Listener<String> listener,
			ErrorListener errListener) {
		super(method, url, errListener);

		this.strBody = body;
		this.rListener = listener;
	}

	@Override
	public byte[] getBody() throws AuthFailureError {
		return strBody.getBytes();
	}

	@Override
	public String getBodyContentType() {
		return "application/json";
	}

	@Override
	protected void deliverResponse(String arg0) {
		rListener.onResponse(arg0);
	}

	@Override
	protected Response<String> parseNetworkResponse(NetworkResponse arg0) {
		return Response.success(new String(arg0.data), null);
	}

	@Override
	public String toString() {
		return super.toString() + " PostBody: " + strBody;
	}

}
