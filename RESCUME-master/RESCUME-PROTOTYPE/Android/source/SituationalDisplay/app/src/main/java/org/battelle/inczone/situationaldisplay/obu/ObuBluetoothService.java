/**
 * @file         inczoneui/obu/ObuBluetoothService.java
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

package org.battelle.inczone.situationaldisplay.obu;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import org.battelle.inczone.situationaldisplay.ApplicationLog;
import org.battelle.inczone.situationaldisplay.R;
import org.battelle.inczone.situationaldisplay.bluetooth.EasyBluetoothSocket;
import org.battelle.inczone.situationaldisplay.bluetooth.EasyBluetoothState;
import org.battelle.inczone.situationaldisplay.bluetooth.EasySocketListener;
import org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.DiagnosticMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.HandlerThread;
import android.os.IBinder;
import android.support.v4.content.LocalBroadcastManager;
import android.util.Log;
import android.widget.Toast;

public class ObuBluetoothService extends Service implements JsonMessageHandler {

	private final static String PREFIX = "org.battelle.inczone.situationaldisplay.obu.ObuBluetoothService";

	public final static String ACTION_STATE_CHANGE = PREFIX + ".action_state_change";
	public final static String EXTRA_STATE = PREFIX + ".extra_state";

	/**
	 * Stops the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void stopService(Context context) {

		context.stopService(new Intent(context, ObuBluetoothService.class));
	}

	/**
	 * Starts the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void startService(Context context) {

		context.startService(new Intent(context, ObuBluetoothService.class));
	}

	/**
	 * The {@code ManagedBluetoothSocket} that the service will use to connect
	 * to the OBU
	 */
	private EasyBluetoothSocket rManagedBluetoothSocket;
	/**
	 * List of message handlers that the service can look towards to process
	 * received messages
	 */
	private List<ObuMessageHandler> rReceiveHandlers;

	private ApplicationLog rAppLog;
	private SharedPreferences rSettings;
	private String OBU_UUID;
	private HandlerThread rProcessThread = new HandlerThread("ObuBluetoothService");

	/*
	 * Statistics
	 */
	private ObuBluetoothState currentState = ObuBluetoothState.Unknown;

	public ObuBluetoothService() {
	}

	@Override
	public IBinder onBind(Intent intent) {
		return null;
	}

	@Override
	public void onCreate() {
		super.onCreate();

		// Get (or create) the logger
		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("ObuBluetoothService", "onCreate()");

		// Determine if we have bluetooth. Kill ourselves if we don't
		if (!EasyBluetoothSocket.isBluetoothAvailable()) {
			rAppLog.e("ObuBluetoothService", "Device does not support Bluetooth!  Killing Service (stopSelf())");

			updateState(ObuBluetoothState.Bluetooth_Unavailable);

			stopSelf();
			return;
		}

		if (!EasyBluetoothSocket.isBluetoothEnabled()) {
			rAppLog.w("ObuBluetoothService", "Bluetooth not enabled!");

			// Ask to enable bluetooth
			Intent btIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			btIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
			this.startActivity(btIntent);
		}

		// Load the settings and register for changes
		rSettings = getSharedPreferences(getResources().getString(R.string.setting_file_name), MODE_MULTI_PROCESS);
		rSettings.registerOnSharedPreferenceChangeListener(settingChangedListener);

		OBU_UUID = this.getResources().getString(R.string.config_obuBluetoothUUID);

		// Initialize handlers that will receive messages
		rReceiveHandlers = new ArrayList<ObuMessageHandler>();
		rReceiveHandlers.add(new DiagnosticMessageHandler(this, this));
        rReceiveHandlers.add(new EVASMessageHandler(this, this));
        rReceiveHandlers.add(new BSMSMessageHandler(this, this));
        rReceiveHandlers.add(new TIMSMessageHandler(this, this));

		rProcessThread.start();

		// Initialize the socket
		initBluetoothSocket();
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		return Service.START_STICKY;
	}

	@Override
	public void onDestroy() {

		// Log that we're dying
		rAppLog.i("ObuBluetoothService", "onDestroy()");

		// Unregister from any setting changes
		rSettings.unregisterOnSharedPreferenceChangeListener(settingChangedListener);

		// Close the socket
		if (rManagedBluetoothSocket != null)
			rManagedBluetoothSocket.close();

		// Let the receive handler unregister from any broadcasts they might
		// have registered to.
		for (ObuMessageHandler h : rReceiveHandlers) {
			h.unregister();
		}

		rProcessThread.quit();

		super.onDestroy();
	}

	/**
	 * Closes any currently open bluetooth socket, creates a new socket and
	 * starts the connection process
	 */
	private void initBluetoothSocket() {

		// Get the current mac address
		String obuMacAddress = rSettings.getString(getResources().getString(R.string.setting_obuMacAddress_key), "");

		// Close any old sockets
		if (rManagedBluetoothSocket != null) {

			updateState(ObuBluetoothState.Disconnected);
			rManagedBluetoothSocket.setListener(null);
			rManagedBluetoothSocket.close();
		}

		// Create new socket and connect
		rManagedBluetoothSocket = new EasyBluetoothSocket(obuMacAddress, OBU_UUID);
		rManagedBluetoothSocket.setListener(receiveListener);
		rManagedBluetoothSocket.setConnectionRetryDelay(Integer.parseInt(getResources().getString(
				R.string.config_btConnectionRetryDelay)));

		rManagedBluetoothSocket.connect();
	}

	@Override
	public boolean handleMessage(JSONObject object) {

		byte[] dataBytes = object.toString().getBytes();
		// msg format: [0x02, 3 letter message id, message, 0x03]
		byte[] transmitBytes = new byte[dataBytes.length + 2];

		// Start of text
		transmitBytes[0] = 0x02;

		// Message
		for (int i = 0; i < dataBytes.length; i++) {
			transmitBytes[i + 1] = dataBytes[i];
		}

		// End of text
		transmitBytes[transmitBytes.length - 1] = 0x03;

		// Send the data and tell the caller that we handled the message
		rManagedBluetoothSocket.write(transmitBytes);
		return true;
	}

	/**
	 * Searches through all of the message handlers looking for one that will
	 * handle the object.
	 * 
	 * @param object
	 *            Object to find a message handler for
	 */
	private void findMessageHandler(final JSONObject object) {
		new Runnable() {
			// new Handler(this.getMainLooper()).post(new Runnable() {

			@Override
			public void run() {

				for (JsonMessageHandler h : rReceiveHandlers) {
					if (h.handleMessage(object))
						return;
				}
				rAppLog.w("ObuBluetoothService", "findMessageHandler() No handler found for: " + object.toString());
			}
		}.run();
		/*
		 * new Thread(new Runnable() { // new
		 * Handler(this.getMainLooper()).post(new Runnable() {
		 * 
		 * @Override public void run() {
		 * 
		 * for (JsonMessageHandler h : rReceiveHandlers) { if
		 * (h.handleMessage(object)) return; } rAppLog.w("ObuBluetoothService",
		 * "findMessageHandler() No handler found for: " + object.toString()); }
		 * }).start();
		 */
	}

	/**
	 * Updates the state of the service, and broadcasts updates as needed.
	 * 
	 * @param state
	 *            State to change to
	 */
	private synchronized void updateState(ObuBluetoothState state) {
		if (this.currentState != state) {
			this.currentState = state;

			this.rAppLog.v("ObuBlutoothService", "updateState() " + state.toString());

			Intent results = new Intent(ACTION_STATE_CHANGE);
			results.putExtra(EXTRA_STATE, state);

			LocalBroadcastManager.getInstance(this).sendBroadcast(results);
		}
	}

	/**
	 * Fires whenever a settings is changed. Used to reinitialize the bluetooth
	 * socket if a different MAC address is selected.
	 */
	OnSharedPreferenceChangeListener settingChangedListener = new OnSharedPreferenceChangeListener() {

		@Override
		public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {

			// If obu mac address changed, reinitialize the socket.
			if (key.equals(getResources().getString(R.string.setting_obuMacAddress_key))) {

				rAppLog.v("ObuBluetoothService", "prefChangedListener.onSharedPreferenceChanged: Setting changed ("
						+ key + "). Restarting BluetoothSocket.");

				initBluetoothSocket();
			}
		}
	};

	private final EasySocketListener receiveListener = new EasySocketListener() {

		byte[] data = new byte[EasyBluetoothSocket.MAX_BUFFER_SIZE];
		int dataIndex = 0;

		@Override
		public void onDataReceived(int count) {

			try {
				dataIndex += rManagedBluetoothSocket.getInputStream().readAsync(data, dataIndex);
			} catch (IOException e) {
				rAppLog.e("ObuBluetoothService", "receiveListener.onDataReceived() ", e);
			}

			if (dataIndex == data.length)
				dataIndex = 0;

			int consumed = 0;
			while ((consumed = process()) > 0) {
				// Return the length of data that we consumed.
				for (int i = consumed; i < dataIndex; i++) {
					data[i - consumed] = data[i];
				}
				dataIndex -= consumed;
			}
		}

		private int process() {
			// Start and end position of the message
			int startPosition = -1;
			int endPosition = -1;

			// Search for the start and end
			for (int i = 0; i < dataIndex; i++) {
				if (data[i] == 0x02) {
					startPosition = i;
				} else if (data[i] == 0x03) {
					endPosition = i;
					break;
				}
			}

			if (endPosition > -1) {
				if (endPosition < startPosition) {
					Toast.makeText(ObuBluetoothService.this,
							"FRAMING ERROR. End position is in front of start position", Toast.LENGTH_LONG).show();
				}
			} else if (startPosition > 0)
				Toast.makeText(ObuBluetoothService.this, "FRAMING ERROR. Start position != 0", Toast.LENGTH_LONG)
						.show();

			// Little log to tell us what the search found.
			// rAppLog.v("ObuBluetoothService",
			// "receiveListener.process(): Start Position = " + startPosition
			// + "  End Position = " + endPosition + "  Length: " + dataIndex);

			/*
			 * If the end if before or at the beginning, then return the index
			 * of the beginning. This will tell the caller to just consume all
			 * of the bytes prior to the message start byte.
			 */
			if (endPosition <= startPosition)
				return startPosition;

			/*
			 * If we have a start position (and from above we know the end
			 * position follows the start), we can parse everything
			 */
			if (startPosition != -1) {

				// Generate a message string from the data, and find the first
				// '{' (the beginning of the JSON)
				String dataString = new String(data, startPosition + 1, endPosition - startPosition - 1);
				int jsonStartIndex = dataString.indexOf('{');
				if (jsonStartIndex > 0)
					dataString = dataString.substring(dataString.indexOf('{'));

				rAppLog.i("ObuBluetoothService", "receiveListener.onDataReceived(): Recieved data: " + dataString);

				try {
					// Try to parse and handle the message
					JSONObject jsonData = new JSONObject(dataString);
                    Log.d("Received", jsonData.toString());
					findMessageHandler(jsonData);

				} catch (JSONException e) {
					rAppLog.e("ObuBluetoothService", "receiveListener.onDataReceived(): Could not parse JSON Data - "
							+ dataString + " : " + e.getMessage());
				}
			}

			return endPosition + 1;
		}

		@Override
		public synchronized void onStateUpdated(EasyBluetoothState state) {

			switch (state) {
			case Bluetooth_Off:
				updateState(ObuBluetoothState.Bluetooth_Off);
				break;

			case Bluetooth_Unavailable:
				updateState(ObuBluetoothState.Bluetooth_Unavailable);
				break;

			case Connected:
				updateState(ObuBluetoothState.Connected);
				break;

			case Connecting:
				updateState(ObuBluetoothState.Connecting);
				break;

			case Disconnected:
				updateState(ObuBluetoothState.Disconnected);
				break;

			default:
				updateState(ObuBluetoothState.Unknown);
				break;
			}
		}
	};

}
