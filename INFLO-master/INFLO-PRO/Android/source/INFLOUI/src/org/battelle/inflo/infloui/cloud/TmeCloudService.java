/**
 * @file         infloui/cloud/TmeCloudService.java
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

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

import org.apache.http.conn.params.ConnManagerParams;
import org.apache.http.conn.scheme.PlainSocketFactory;
import org.apache.http.conn.scheme.Scheme;
import org.apache.http.conn.scheme.SchemeRegistry;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.impl.conn.tsccm.ThreadSafeClientConnManager;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.params.HttpParams;
import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.StatisticsLog;
import org.joda.time.DateTime;
import org.joda.time.Period;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.IBinder;
import android.os.Parcelable;
import android.support.v4.content.LocalBroadcastManager;

import com.android.volley.Request;
import com.android.volley.Request.Method;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.HttpClientStack;
import com.android.volley.toolbox.Volley;

//TODO: If there are any more statistics, they should be pulled into a parcelable class??

public class TmeCloudService extends Service {

	private final static String PREFIX = "org.battelle.inflo.infloui.cloud.TmeCloudService";

	public final static String ACTION_REQUEST = PREFIX + ".action_request";
	public final static String ACTION_RESPONSE = PREFIX + ".action_response";
	public final static String ACTION_STATISTICS_UPDATE = PREFIX + ".action_statistics_update";

	public final static String EXTRA_REQUEST = PREFIX + ".extra_request";
	public final static String EXTRA_RESPONSE = PREFIX + ".extra_response";

	public final static String EXTRA_STATISTICS = PREFIX + ".extra_statistics";

	public static final int MAX_CONNECTIONS = 2;

	/**
	 * Creates a new intent to post content to a url using TmeCloudService
	 * 
	 * @return
	 */
	public final static void newRequest(Context context, TmeRequest request) {

		Intent results = new Intent(context, TmeCloudService.class);
		results.putExtra(EXTRA_REQUEST, request);
		context.startService(results);
	}

	/**
	 * Stops the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void stopService(Context context) {

		context.stopService(new Intent(context, TmeCloudService.class));
	}

	/**
	 * Volley Request Queue
	 */
	RequestQueue rRequestQueue;
	private ApplicationLog rAppLog;
	private StatisticsLog rStatLog;

	private final Map<String, TmeCloudEndpointStatistics> rStatistics = new HashMap<String, TmeCloudEndpointStatistics>();

	/**
	 * We're not binding, so return null
	 */
	@Override
	public IBinder onBind(Intent intent) {
		return null;
	}

	/**
	 * Called once when service is first started. Initializes
	 * Volley.RequestQueue and HttpClient
	 */
	@Override
	public void onCreate() {

		// Connection Schemes
		SchemeRegistry schemeRegistry = new SchemeRegistry();
		schemeRegistry.register(new Scheme("http", PlainSocketFactory.getSocketFactory(), 80));

		// Default Params
		HttpParams connManagerParams = new BasicHttpParams();
		ConnManagerParams.setMaxTotalConnections(connManagerParams, MAX_CONNECTIONS);

		// Connection Manager
		ThreadSafeClientConnManager cm = new ThreadSafeClientConnManager(new BasicHttpParams(),
				schemeRegistry);

		// Http Client
		DefaultHttpClient client = new DefaultHttpClient(cm, null);
		HttpConnectionParams.setTcpNoDelay(client.getParams(), true);

		// Generate a new Volley Request Queue
		rRequestQueue = Volley.newRequestQueue(this, new HttpClientStack(client));

		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("TmeCloudService", "onCreate()");

		rStatLog = new StatisticsLog("tme-requests", true, "URL", "Response Code",
				"Latency Time (seconds)", "Received Data (or err message)");

		super.onCreate();
	}

	/**
	 * Called when Service is being shutdown for whatever reason. Release
	 * RequestQueue.
	 */
	@Override
	public void onDestroy() {

		rRequestQueue.stop();

		rAppLog.i("TmeCloudService", "onDestroy()");

		super.onDestroy();
	}

	/**
	 * Called whenever a client calls 'startService' with an intent that points
	 * at this service. This takes the intent, generates a Volley Request from
	 * it, and adds it to the queue.
	 */
	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {

		// Service was restarted without an intent, so there is no action to
		// take.
		if (intent != null) {

			final DateTime startTime = DateTime.now();
			final TmeRequest request = (TmeRequest) intent.getExtras().get(EXTRA_REQUEST);

			int method = getHttpMethod(request.getMethod());

			// Generate request and register callbacks
			Request<String> volleyRequest = new TmeCloudVolleyRequest(method, request.getUrl(),
					request.getBody(), new Response.Listener<String>() {

						@Override
						public void onResponse(String response) {

							rAppLog.v("TmeCloudService",
									"onSuccessfulResponse(): " + response.toString());

							// Broadcast Response
							if (request.getResponseTarget() != null) {
								Intent responseIntent = new Intent(request.getResponseTarget());

								responseIntent.putExtra(EXTRA_RESPONSE, new TmeResponse(true,
										response.toString(), null));

								LocalBroadcastManager.getInstance(TmeCloudService.this)
										.sendBroadcast(responseIntent);
							}

							double latency = new Period(startTime, DateTime.now()).getMillis() / 1000.0;
							TmeCloudEndpointStatistics.addSuccess(rStatistics, request.getUrl(),
									latency);

							rStatLog.log(request.getUrl(), 200, latency, response);

							updateStatistics();
						}
					}, new Response.ErrorListener() {

						@Override
						public void onErrorResponse(VolleyError err) {

							rAppLog.e("TmeCloudService", "onErrorResponse(): " + err.getMessage());

							// Broadcast Response
							if (request.getResponseTarget() != null) {
								Intent responseIntent = new Intent(request.getResponseTarget());

								responseIntent.putExtra(EXTRA_RESPONSE, new TmeResponse(false,
										null, err.getMessage()));

								LocalBroadcastManager.getInstance(TmeCloudService.this)
										.sendBroadcast(responseIntent);
							}

							double latency = new Period(startTime, DateTime.now()).getMillis() / 1000.0;
							TmeCloudEndpointStatistics.addError(rStatistics, request.getUrl(),
									latency);

							rStatLog.log(request.getUrl(),
									err.networkResponse != null ? err.networkResponse.statusCode
											: -1, latency, err.getMessage());

							updateStatistics();
						}
					});

			// Log and send
			rAppLog.i("TmeCloudService",
					"onStartCommand() request added to queue: " + request.toString());

			rRequestQueue.add(volleyRequest);

		}

		return Service.START_STICKY;
	}

	/**
	 * Gets the http method from the request intent
	 * 
	 * @param extras
	 *            Extras from intent to find the http method within
	 * @return Volley.Request.Method from extra, otherwise a defaults to 'GET'
	 */
	private int getHttpMethod(TmeRequestMethod method) {

		switch (method) {

		case post:
			return Method.POST;

		case get:
		default:
			return Method.GET;
		}
	}

	/**
	 * Updates the state of the service, and broadcasts updates as needed.
	 * 
	 * @param state
	 *            State to change to
	 */
	private synchronized void updateStatistics() {

		Intent results = new Intent(ACTION_STATISTICS_UPDATE);

		ArrayList<Parcelable> statistics = new ArrayList<Parcelable>();
		for (String key : rStatistics.keySet()) {
			statistics.add(rStatistics.get(key));
		}

		results.putExtra(EXTRA_STATISTICS, statistics.toArray(new Parcelable[statistics.size()]));

		LocalBroadcastManager.getInstance(this).sendBroadcast(results);
	}
}
