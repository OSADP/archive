/**
 * @file         infloui/obdii/VehicleDiagnosticsService.java
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

package org.battelle.inflo.infloui.odbii;

import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.R;
import org.battelle.inflo.infloui.bluetooth.AsyncInputStream;
import org.battelle.inflo.infloui.bluetooth.EasyBluetoothSocket;
import org.battelle.inflo.infloui.bluetooth.EasyBluetoothState;
import org.battelle.inflo.infloui.bluetooth.EasySocketListener;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommand;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandAmbAirTemp0146;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandBarPres0133;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandBrakePosition07DF;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandLatAccel0243;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandLongAccel0243;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandMaf0110;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandRpm010C;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandSpeed010D;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandSteeringAngle0243;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandThrottle0111;
import org.battelle.inflo.infloui.odbii.commands.ObdIICommandWipers0241;
import org.battelle.inflo.infloui.weather.WeatherService;
import org.battelle.vital.iop.GetDataElementPacket;
import org.battelle.vital.iop.GetDataElementPacket.DataElement;
import org.battelle.vital.iop.GetVersionPacket;
import org.battelle.vital.iop.IOPPacket;
import org.battelle.vital.iop.IOPPacketFactory;
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

	private final static String PREFIX = "org.battelle.inflo.infloui.obu.VehicleDiagnosticsService";

	public final static String ACTION_DATA_UPDATE = PREFIX
			+ ".action_data_update";
	public final static String EXTRA_DATA = PREFIX + ".extra_data";

	public final static String ACTION_STATE_CHANGE = PREFIX
			+ ".action_state_change";
	public final static String EXTRA_STATE = PREFIX + ".extra_state";
	
	public final static String LOG_TAG = "VehicleDiagnosticsService";
	
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

		context.startService(new Intent(context,
				VehicleDiagnosticsService.class));
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
	
	/** Interface for constructing IOP packets from raw byte streams. */
	private IOPPacketFactory iopFactory = new IOPPacketFactory();
	
	/** Whether or not the OBD2 command loop is running. */
	private boolean isOdb2Active = false;
	
	/** Whether or not the IOP command loop is running. */
	private boolean isIopActive = false;
	
	/** IOP getDataElement requests to be issued with each command loop. */
	private Map<String, GetDataElementPacket> iopPackets = new LinkedHashMap<String, GetDataElementPacket>();

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

		pollRate = Integer.parseInt(getResources().getString(
				R.string.config_vehicleUpdateRate));

		// Get (or create) the logger
		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("VehicleDiagnosticsService", "onCreate()");

		// Determine if we have bluetooth. Kill ourselves if we don't
		if (!EasyBluetoothSocket.isBluetoothAvailable()) {
			rAppLog.e("VehicleDiagnosticsService",
					"Device does not support Bluetooth!  Killing Service (stopSelf())");

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
		rSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);
		rSettings
				.registerOnSharedPreferenceChangeListener(settingChangedListener);

		OBDII_UUID = this.getResources().getString(
				R.string.config_obdIIBlueoothUUID);

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
		
		// Add the desired getDataElement packets
		iopPackets.put("spd", new GetDataElementPacket(DataElement.VEHICLE_SPEED_KPH));
		iopPackets.put("rpm", new GetDataElementPacket(DataElement.ENGINE_RPM));
		iopPackets.put("throttle", new GetDataElementPacket(DataElement.THROTTLE_POSITION_PERCENT));
		iopPackets.put("maf", new GetDataElementPacket(DataElement.MAF_GRAMS_PER_SECOND));
		iopPackets.put("airtemp", new GetDataElementPacket(DataElement.AMB_AIR_TEMP_DEG_C));
		iopPackets.put("pres", new GetDataElementPacket(DataElement.BAROMETRIC_PRESS_KPA));
		iopPackets.put("longaccel", new GetDataElementPacket(DataElement.LONG_ACCEL_TBD));
		iopPackets.put("lataccel", new GetDataElementPacket(DataElement.LAT_ACCEL_TBD));
		
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
		rSettings
				.unregisterOnSharedPreferenceChangeListener(settingChangedListener);

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
			rEasyBluetoothSocket.removeListener(null);
			rEasyBluetoothSocket.close();
		}

		boolean enabled = rSettings.getBoolean(
				getResources().getString(R.string.setting_enableObdII_key),
				false);

		if (!enabled) {
			updateState(VehicleDiagnosticsState.Disabled);
			return;
		}

		// Get the current mac address
		String obuMacAddress = rSettings.getString(
				getResources().getString(R.string.setting_obdIIMacAddress_key),
				"");

		// Create new socket and connect
		rEasyBluetoothSocket = new EasyBluetoothSocket(obuMacAddress,
				OBDII_UUID);
		rEasyBluetoothSocket.addListener(receiveListener);
		rEasyBluetoothSocket.setConnectionRetryDelay(Integer
				.parseInt(getResources().getString(
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

		if (state == VehicleDiagnosticsState.Connected) {
			WeatherService.stopService(VehicleDiagnosticsService.this);
		} else {
			WeatherService.startService(VehicleDiagnosticsService.this);
		}

		if (this.currentState != state) {
			this.currentState = state;

			this.rAppLog.v("VehicleDiagnosticsService", "updateState() "
					+ state.toString());

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
		public void onSharedPreferenceChanged(
				SharedPreferences sharedPreferences, String key) {

			// If obu mac address changed, reinitialize the socket.
			if (key.equals(getResources().getString(
					R.string.setting_obdIIMacAddress_key))) {

				rAppLog.v("VehicleDiagnosticsService",
						"settingChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + "). Restarting BluetoothSocket.");

				initBluetoothSocket();
			} else if (key.equals(getResources().getString(
					R.string.setting_enableObdII_key))) {

				rAppLog.v("VehicleDiagnosticsService",
						"settingChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + "). Restarting BluetoothSocket.");

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
				int       iopAttempts = 3;
				boolean   isIOP = false;
				
				rLoopThread = new HandlerThread(
						"VehicleDiagnosticsService.rLoopThread");
				rLoopThread.start();
				
				// Wait for the current command stream to die off
				try {
					rLoopHandler = null;
					
					if (isIopActive) {
						synchronized (runIOPCommands) {
							runIOPCommands.wait(10000);
						}
					}
					
					if (isOdb2Active) {
						synchronized (rRunCommands) {
							rRunCommands.wait(10000);
						}
					}
					
					rLoopHandler = new Handler(rLoopThread.getLooper());
				} catch (InterruptedException ex) {
					rAppLog.w(LOG_TAG, "Interrupted while wait for command stream to die down!");
				}
				
				// Determine if the attached module is running the IOP protocol
				while ((iopAttempts-- > 0) && !isIOP) {
					try {
						AsyncInputStream bluetoothIn  = rEasyBluetoothSocket.getInputStream();
						OutputStream     bluetoothOut = rEasyBluetoothSocket.getOutputStream();
						GetVersionPacket helloPacket  = new GetVersionPacket();
						IOPPacket        responsePacket;
						
						// Send the hello packet
						helloPacket.send(bluetoothOut);
						
						// Read in the response
						responsePacket = iopFactory.create(bluetoothIn);
						if (responsePacket instanceof GetVersionPacket) {
							helloPacket = (GetVersionPacket) responsePacket;
						
							// The module is using the IOP protocol and responded to our request
							rAppLog.i(LOG_TAG, "Attached VITAL module is running the IOP protocol.\n"
									+ "Hardware Version: " + helloPacket.getHardwareVersion() + "\n" 
									+ "Software Version: " + helloPacket.getSoftwareMajorVersion() + "."
									+ helloPacket.getSoftwareMinorVersion());
						} else {
							// We received a valid IOP packet, it just wasn't what we expected
							rAppLog.i(LOG_TAG, "Attached VITAL module is running the IOP protocol. "
									+ "(Got unexpected packet, but still a valid IOP packet!)\n");
						}
						
						isIOP = true;
						
						// Run the IOP getDataElement commands
						rLoopHandler.post(runIOPCommands);
					} catch (Exception ex) {
						rAppLog.w(LOG_TAG, "Failed to receive IOP response! Reported error = " + ex.getMessage());
					}
				}
				
				if (!isIOP) {
					// The module is not using the IOP protocol
					rAppLog.w(LOG_TAG, "Attached VITAL module is not running the IOP protocol!");
					
					// Use the old VITAL commands
					rLoopHandler.post(rRunConfigCommands);
					rLoopHandler.post(rRunCommands);
				}
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
				LocalBroadcastManager.getInstance(
						VehicleDiagnosticsService.this).sendBroadcast(intent);
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

	/** IOP getDataElement command runner loop. */
	Runnable runIOPCommands = new Runnable() {
		@Override
		public void run() {
			AsyncInputStream bluetoothIn  = rEasyBluetoothSocket.getInputStream();
			OutputStream     bluetoothOut = rEasyBluetoothSocket.getOutputStream();
			JSONObject       broadcastResults;
			Intent 	         intent;
			Map<String, GetDataElementPacket> results = new HashMap<String, GetDataElementPacket>();
			
			isIopActive = true;

			// For each registered getDataElement command
			for (Map.Entry<String, GetDataElementPacket> entry : iopPackets.entrySet()) {
				try {
					GetDataElementPacket command = entry.getValue();
					DataElement 		 dataElement;
					boolean     	     isActive;
					IOPPacket            responsePacket;
					double				 value = 0;
					
					// Run the IOP command	
					command.send(bluetoothOut);
					
					// Read in the IOP response
					do {
						responsePacket = iopFactory.create(bluetoothIn);
						
						if (responsePacket instanceof GetDataElementPacket) {
							GetDataElementPacket result = (GetDataElementPacket) responsePacket;
							results.put(entry.getKey(), result);
							
							// If the data element is active, extract its value
							dataElement = result.getDataElement();
							isActive    = result.isActive();
							if (isActive) {
								value 	= result.getValue();
							}
							
							// Log the results
							rAppLog.v(
									"VehicleDiagnosticsService",
									"RunIOPCommands: Success - TAG: " + entry.getKey()
											+ " Data Element: " + dataElement 
											+ " IsActive: " + isActive 
											+ " Value: " + value
											+ " Raw Bytes: " + result.getRawResponse()
								);
						} else {
							rAppLog.w(LOG_TAG, "Received unknown packet type!");
						}
					} while (!(responsePacket instanceof GetDataElementPacket));
					
				} catch (Exception ex) {
					// Log the error
					rAppLog.e(
							"VehicleDiagnosticsService",
							"RunIOPCommands: Error - TAG: " + entry.getKey()
									+ " Error: " + ex.getMessage() 
									+ " Raw Response: " + entry.getValue().getRawResponse()
						);
					
					try {
						final int oneSecond = 1000;
						Integer   throwAway;
						
						// Drain the input stream of any lingering bytes
						do {
							throwAway = bluetoothIn.read(oneSecond);
							
							if (throwAway != null) {
								rAppLog.e(
										"VehicleDiagnosticsService",
										"Dropped Byte: " + String.format("0x%02X ", throwAway.byteValue())
									);
							}
						} while ((throwAway != null) && (throwAway != -1));
					} catch (IOException ex2) {
						rAppLog.e(
								"VehicleDiagnosticsService",
								"Failed to clean up input stream after bad packet!"
							);
					}
				}
			}
			
			// Pack the results of this latest round of polling into a broadcast
			broadcastResults = new JSONObject();
			for (Map.Entry<String, GetDataElementPacket> entry : results.entrySet()) {
				GetDataElementPacket command = entry.getValue();
				
				// If the command was successful and the data element is active
				if (command.isResponse() && command.isActive()) {
					try {
						// Add the data element to the broadcast
						broadcastResults.put(
								entry.getKey(),
								command.getValue()
							);
					} catch (JSONException e) {
						e.printStackTrace();
					}
				}
			}

			// Broadcast the results
			intent = new Intent(ACTION_DATA_UPDATE);
			intent.putExtra(EXTRA_DATA, broadcastResults.toString());
			LocalBroadcastManager.getInstance(VehicleDiagnosticsService.this)
					.sendBroadcast(intent);
			
			// Keep polling the data elements
			if (rLoopHandler != null) {
				rAppLog.i(LOG_TAG, "IOP command stream is restarting...");
				rLoopHandler.postDelayed(runIOPCommands, pollRate);
			} else {
				rAppLog.i(LOG_TAG, "IOP command stream is dying off...");
				
				synchronized (this) {
					isIopActive = false;
					notifyAll();
				}
			}
		}
	};
	
	Runnable rRunConfigCommands = new Runnable() {

		@Override
		public void run() {
			for (ObdIICommand cmd : rConfigCommands) {
				if (!cmd.run(rEasyBluetoothSocket.getOutputStream(), rEasyBluetoothSocket.getInputStream())) {
					rAppLog.e("VehicleDiagnosticsService",
							"RunConfigCommands: Error - TAG: " + cmd.getTag()
									+ " RESPONSE: " + cmd.getRawResponse());

				} else {
					rAppLog.v("VehicleDiagnosticsService",
							"RunConfigCommands: Success - TAG: " + cmd.getTag()
									+ " RESPONSE: " + cmd.getRawResponse());
				}

			}
		}
	};

	Runnable rRunCommands = new Runnable() {

		@Override
		public void run() {
			isOdb2Active = true;
			
			for (ObdIICommand cmd : rCommands) {
				if (!cmd.run(rEasyBluetoothSocket.getOutputStream(),
						rEasyBluetoothSocket.getInputStream())) {
					rAppLog.e("VehicleDiagnosticsService",
							"RunCommands: Error - TAG: " + cmd.getTag()
									+ " RESPONSE: " + cmd.getRawResponse());

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
						broadcastResults.put(cmd.getTag(),
								cmd.getProcessedResponse());
					} catch (JSONException e) {
						e.printStackTrace();
					}
				}
			}

			Intent intent = new Intent(ACTION_DATA_UPDATE);
			intent.putExtra(EXTRA_DATA, broadcastResults.toString());

			LocalBroadcastManager.getInstance(VehicleDiagnosticsService.this)
					.sendBroadcast(intent);

			if (rLoopHandler != null) {
				rAppLog.i(LOG_TAG, "ODB2 command stream is restarting...");
				rLoopHandler.postDelayed(rRunCommands, pollRate);
			} else {
				rAppLog.i(LOG_TAG, "ODB2 command stream is dying off...");
				synchronized (this) {
					isOdb2Active = false;
					notifyAll();
				}
			}
		}
	};

}
