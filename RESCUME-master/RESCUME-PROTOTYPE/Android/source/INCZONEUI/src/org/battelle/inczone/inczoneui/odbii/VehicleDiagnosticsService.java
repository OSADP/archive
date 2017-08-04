/**
 * @file         inczoneui/obdii/VehicleDiagnosticsService.java
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

package org.battelle.inczone.inczoneui.odbii;

import java.util.ArrayList;
import java.util.List;

import org.battelle.inczone.inczoneui.ApplicationLog;
import org.battelle.inczone.inczoneui.R;
import org.battelle.inczone.inczoneui.bluetooth.EasyBluetoothSocket;
import org.battelle.inczone.inczoneui.bluetooth.EasyBluetoothState;
import org.battelle.inczone.inczoneui.bluetooth.EasySocketListener;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommand;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandAmbAirTemp0146;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandBarPres0133;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandMaf0110;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandRpm010C;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandSpeed010D;
import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommandThrottle0111;
import org.battelle.inczone.inczoneui.odbii.commands.chevy.untested.ObdIICommandBrakePosition07DF;
import org.battelle.inczone.inczoneui.odbii.commands.chevy.untested.ObdIICommandLatAccel0243;
import org.battelle.inczone.inczoneui.odbii.commands.chevy.untested.ObdIICommandLongAccel0243;
import org.battelle.inczone.inczoneui.odbii.commands.chevy.untested.ObdIICommandSteeringAngle0243;
import org.battelle.inczone.inczoneui.odbii.commands.chevy.untested.ObdIICommandWipers0241;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Service;
import android.bluetooth.BluetoothAdapter;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.IBinder;
import android.support.v4.content.LocalBroadcastManager;

public class VehicleDiagnosticsService extends Service {

	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.VehicleDiagnosticsService";

	public final static String ACTION_DATA_UPDATE = PREFIX + ".action_data_update";
	public final static String EXTRA_DATA = PREFIX + ".extra_data";

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

		context.stopService(new Intent(context, VehicleDiagnosticsService.class));
	}

	/**
	 * Starts the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void startService(Context context) {

		context.startService(new Intent(context, VehicleDiagnosticsService.class));
	}

	/**
	 * The {@code ManagedBluetoothSocket} that the service will use to connect
	 * to the OBU
	 */
	private EasyBluetoothSocket rEasyBluetoothSocket;
	private ApplicationLog rAppLog;
	private SharedPreferences rSettings;
	private String OBDII_UUID;
	private HandlerThread rLoopThread = null;
	private Handler rLoopHandler = null;
	private int pollRate;
	private List<ObdIICommand> rConfigCommands = new ArrayList<ObdIICommand>();
	private List<ObdIICommand> rCommands = new ArrayList<ObdIICommand>();

	/*
	 * Statistics
	 */
	private VehicleDiagnosticsState currentState = VehicleDiagnosticsState.Unknown;

	public VehicleDiagnosticsService() {
	}

	@Override
	public IBinder onBind(Intent intent) {
		return null;
	}

	@Override
	public void onCreate() {
		super.onCreate();

		pollRate = Integer.parseInt(getResources().getString(R.string.config_vehicleUpdateRate));

		// Get (or create) the logger
		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("VehicleDiagnosticsService", "onCreate()");

		// Determine if we have bluetooth. Kill ourselves if we don't
		if (!EasyBluetoothSocket.isBluetoothAvailable()) {
			rAppLog.e("VehicleDiagnosticsService", "Device does not support Bluetooth!  Killing Service (stopSelf())");

			updateState(VehicleDiagnosticsState.Bluetooth_Unavailable);

			stopSelf();
			return;
		}

		if (!EasyBluetoothSocket.isBluetoothEnabled()) {
			rAppLog.w("VehicleDiagnosticsService", "Bluetooth not enabled!");

			// Ask to enable bluetooth
			Intent btIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
			btIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
			this.startActivity(btIntent);
		}

		// Load the settings and register for changes
		rSettings = getSharedPreferences(getResources().getString(R.string.setting_file_name), MODE_MULTI_PROCESS);
		rSettings.registerOnSharedPreferenceChangeListener(settingChangedListener);

		OBDII_UUID = this.getResources().getString(R.string.config_obdIIBlueoothUUID);

		rConfigCommands.add(new ObdIICommand("ATE0", "ECHO OFF"));
		rConfigCommands.add(new ObdIICommand("ATM1", "MEMORY ON"));
		rConfigCommands.add(new ObdIICommand("ATL0", "LINEFEEDS OFF"));
		rConfigCommands.add(new ObdIICommand("ATS1", "SPACES ON"));
		rConfigCommands.add(new ObdIICommand("ATSP0", "SET PROTOCOL AUTO"));

		// rCommands.add(new ObdIICommandVin0902("vin"));
		rCommands.add(new ObdIICommandSpeed010D("spd"));
		rCommands.add(new ObdIICommandRpm010C("rpm"));
		rCommands.add(new ObdIICommandThrottle0111("throttle"));
		rCommands.add(new ObdIICommandMaf0110("maf"));
		rCommands.add(new ObdIICommandAmbAirTemp0146("airtemp"));
		rCommands.add(new ObdIICommandBarPres0133("pres"));
		rCommands.add(new ObdIICommandBrakePosition07DF("brakeposition"));
		rCommands.add(new ObdIICommandSteeringAngle0243("steerangle"));
		rCommands.add(new ObdIICommandWipers0241("wipers"));
		rCommands.add(new ObdIICommandLongAccel0243("longaccel"));
		rCommands.add(new ObdIICommandLatAccel0243("lataccel"));

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
		rAppLog.i("VehicleDiagnosticsService", "onDestroy()");

		// Unregister from any setting changes
		rSettings.unregisterOnSharedPreferenceChangeListener(settingChangedListener);

		// Close the socket
		if (rEasyBluetoothSocket != null)
			rEasyBluetoothSocket.close();

		if (rLoopHandler != null) {
			rLoopHandler.removeCallbacksAndMessages(null);
			rLoopHandler = null;
		}
		if (rLoopThread != null) {
			rLoopThread.quit();
			rLoopThread = null;
		}

		super.onDestroy();
	}

	/**
	 * Closes any currently open bluetooth socket, creates a new socket and
	 * starts the connection process
	 */
	private void initBluetoothSocket() {

		// Close any old sockets
		if (rEasyBluetoothSocket != null) {

			updateState(VehicleDiagnosticsState.Disconnected);
			rEasyBluetoothSocket.setListener(null);
			rEasyBluetoothSocket.close();
		}

		boolean enabled = rSettings.getBoolean(getResources().getString(R.string.setting_enableObdII_key), false);

		if (!enabled) {
			updateState(VehicleDiagnosticsState.Disabled);
			return;
		}

		// Get the current mac address
		String obuMacAddress = rSettings.getString(getResources().getString(R.string.setting_obdIIMacAddress_key), "");

		// Create new socket and connect
		rEasyBluetoothSocket = new EasyBluetoothSocket(obuMacAddress, OBDII_UUID);
		rEasyBluetoothSocket.setListener(receiveListener);
		rEasyBluetoothSocket.setConnectionRetryDelay(Integer.parseInt(getResources().getString(
				R.string.config_btConnectionRetryDelay)));

		rEasyBluetoothSocket.connect();
	}

	/**
	 * Updates the state of the service, and broadcasts updates as needed.
	 * 
	 * @param state
	 *            State to change to
	 */
	private synchronized void updateState(VehicleDiagnosticsState state) {
		if (this.currentState != state) {
			this.currentState = state;

			this.rAppLog.v("VehicleDiagnosticsService", "updateState() " + state.toString());

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
			if (key.equals(getResources().getString(R.string.setting_obdIIMacAddress_key))) {

				rAppLog.v("VehicleDiagnosticsService",
						"settingChangedListener.onSharedPreferenceChanged: Setting changed (" + key
								+ "). Restarting BluetoothSocket.");

				initBluetoothSocket();
			} else if (key.equals(getResources().getString(R.string.setting_enableObdII_key))) {

				rAppLog.v("VehicleDiagnosticsService",
						"settingChangedListener.onSharedPreferenceChanged: Setting changed (" + key
								+ "). Restarting BluetoothSocket.");

				initBluetoothSocket();
			}
		}
	};

	private final EasySocketListener receiveListener = new EasySocketListener() {

		@Override
		public void onDataReceived(int count) {
		}

		@Override
		public synchronized void onStateUpdated(EasyBluetoothState state) {

			if (state == EasyBluetoothState.Connected) {
				rLoopThread = new HandlerThread("VehicleDiagnosticsService.rLoopThread");
				rLoopThread.start();
				rLoopHandler = new Handler(rLoopThread.getLooper());
				rLoopHandler.post(rRunConfigCommands);
				rLoopHandler.post(rRunCommands);
			} else {
				if (rLoopHandler != null) {
					rLoopHandler.removeCallbacksAndMessages(null);
					rLoopHandler = null;
				}
				if (rLoopThread != null) {
					rLoopThread.quit();
					rLoopThread = null;
				}

				JSONObject broadcastResults = new JSONObject();
				Intent intent = new Intent(ACTION_DATA_UPDATE);
				intent.putExtra(EXTRA_DATA, broadcastResults.toString());
				LocalBroadcastManager.getInstance(VehicleDiagnosticsService.this).sendBroadcast(intent);
			}

			switch (state) {
			case Bluetooth_Off:
				updateState(VehicleDiagnosticsState.Bluetooth_Off);
				break;

			case Bluetooth_Unavailable:
				updateState(VehicleDiagnosticsState.Bluetooth_Unavailable);
				break;

			case Connected:
				updateState(VehicleDiagnosticsState.Connected);
				break;

			case Connecting:
				updateState(VehicleDiagnosticsState.Connecting);
				break;

			case Disconnected:
				updateState(VehicleDiagnosticsState.Disconnected);
				break;

			default:
				updateState(VehicleDiagnosticsState.Unknown);
				break;
			}
		}
	};

	Runnable rRunConfigCommands = new Runnable() {

		@Override
		public void run() {
			for (ObdIICommand cmd : rConfigCommands) {
				if (!cmd.run(rEasyBluetoothSocket.getOutputStream(), rEasyBluetoothSocket.getInputStream())) {
					rAppLog.e("VehicleDiagnosticsService", "RunConfigCommands: Error - TAG: " + cmd.getTag()
							+ " RESPONSE: " + cmd.getRawResponse());

				} else {
					rAppLog.v("VehicleDiagnosticsService", "RunConfigCommands: Success - TAG: " + cmd.getTag()
							+ " RESPONSE: " + cmd.getRawResponse());
				}

			}
		}
	};

	Runnable rRunCommands = new Runnable() {

		@Override
		public void run() {
			for (ObdIICommand cmd : rCommands) {
				if (!cmd.run(rEasyBluetoothSocket.getOutputStream(), rEasyBluetoothSocket.getInputStream())) {
					rAppLog.e("VehicleDiagnosticsService", "RunCommands: Error - TAG: " + cmd.getTag() + " RESPONSE: "
							+ cmd.getRawResponse());

					if (rEasyBluetoothSocket.getCurrentState() != EasyBluetoothState.Connected)
						return;

				} else {
					/*
					 * rAppLog.v("VehicleDiagnosticsService",
					 * "RunCommands: Success - TAG: " + cmd.getTag() +
					 * " RESPONSE: " + cmd.getProcessedResponse().toString());
					 */
				}

			}

			JSONObject broadcastResults = new JSONObject();
			for (ObdIICommand cmd : rCommands) {
				if (cmd.isSuccessful()) {
					try {
						broadcastResults.put(cmd.getTag(), cmd.getProcessedResponse());
					} catch (JSONException e) {
						e.printStackTrace();
					}
				}
			}

			Intent intent = new Intent(ACTION_DATA_UPDATE);
			intent.putExtra(EXTRA_DATA, broadcastResults.toString());

			LocalBroadcastManager.getInstance(VehicleDiagnosticsService.this).sendBroadcast(intent);

			if (rLoopHandler != null) {
				rLoopHandler.postDelayed(rRunCommands, pollRate);
			}
		}
	};

}
