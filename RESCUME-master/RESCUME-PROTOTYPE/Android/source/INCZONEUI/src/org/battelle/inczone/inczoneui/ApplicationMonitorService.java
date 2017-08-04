/**
 * @file         inczoneui/ApplicationMonitorService.java
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

package org.battelle.inczone.inczoneui;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.lang.Thread.UncaughtExceptionHandler;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Locale;

import org.battelle.inczone.inczoneui.kml.Kml;
import org.battelle.inczone.inczoneui.kml.LatLngPoint;
import org.battelle.inczone.inczoneui.kml.LineString;
import org.battelle.inczone.inczoneui.ntrip.NTripService;
import org.battelle.inczone.inczoneui.ntrip.NTripState;
import org.battelle.inczone.inczoneui.obu.ObuBluetoothService;
import org.battelle.inczone.inczoneui.obu.ObuBluetoothState;
import org.battelle.inczone.inczoneui.obu.handlers.AlertMessageHandler;
import org.battelle.inczone.inczoneui.obu.handlers.AlertMessageHandler.AlertInformation;
import org.battelle.inczone.inczoneui.obu.handlers.DiagnosticMessageHandler;
import org.battelle.inczone.inczoneui.obu.handlers.DiagnosticMessageHandler.DiagnosticInformation;
import org.battelle.inczone.inczoneui.odbii.VehicleDiagnosticsService;
import org.battelle.inczone.inczoneui.odbii.VehicleDiagnosticsState;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.Notification;
import android.app.PendingIntent;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.graphics.Color;
import android.media.RingtoneManager;
import android.os.Environment;
import android.os.Handler;
import android.os.IBinder;
import android.speech.tts.TextToSpeech;
import android.speech.tts.TextToSpeech.OnInitListener;
import android.support.v4.content.LocalBroadcastManager;
import android.view.Gravity;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

/**
 * 
 */
public class ApplicationMonitorService extends Service {

	private final static String PREFIX = "org.battelle.inczone.inczoneui.ApplicationMonitorService";

	public final static String ACTION_INVALIDATE = PREFIX
			+ ".action_invalidate";
	private final static String ACTION_UI_REQUEST_UPDATE = PREFIX
			+ ".action_ui_request_update";
	public final static String EXTRA_MODEL = PREFIX + ".extra_model";

	final static int FOREGROUND_ID = 24601;

	/*
	 * Notification Option Flags
	 */
	public static final int N_VIBRATE = 0x01;
	public static final int N_BEEP = 0x02;
	public static final int N_TEXT_TO_SPEECH = 0x04;
	public static final int N_POP_UP = 0x08;
	public static final int N_NOTIFICATION_BAR = 0x10;
	public static final int N_TEXT_TO_SPEECH_CLEAR = 0x20;

	/**
	 * Utility method a UI can call to force the
	 * {@code ApplicationMonitorService} to send a new invalidation broadcast
	 * with the current model. Often useful for when an {@code Activity} resumes
	 * and needs the most recent application model.
	 * 
	 * @param context
	 */
	public static void requestUpdate(Context context) {
		LocalBroadcastManager.getInstance(context).sendBroadcast(
				new Intent(ACTION_UI_REQUEST_UPDATE));
	}

	private ApplicationLog mAppLog;
	private SharedPreferences mSettings;
	private TextToSpeech mTextToSpeach;
	private final ApplicationModel rApplicationModel = new ApplicationModel();

	private final SimpleDateFormat rDateFormatter = new SimpleDateFormat(
			"yyyy-MM-dd_HH-mm-ss-SSSS", Locale.US);

	Kml rKml = new Kml("inczonemap_"
			+ rDateFormatter.format(Calendar.getInstance().getTime()));
	LineString rCurrentMapPath = null;

	public ApplicationMonitorService() {
	}

	@Override
	public IBinder onBind(Intent intent) {
		return null; // Not binding
	}

	@Override
	public void onCreate() {
		super.onCreate();

		mAppLog = ApplicationLog.getInstance();
		mAppLog.i("ApplicationMonitorService", "onCreate()");

		mSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);

		mSettings
				.registerOnSharedPreferenceChangeListener(rSettingsChangeListener);

		// Start logger and set default uncaught exceptions
		Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {

			@Override
			public void uncaughtException(Thread arg0, Throwable arg1) {
				ApplicationLog.getInstance().e(arg0.getName(), "UncaughtError",
						arg1);
			}
		});

		this.mTextToSpeach = new TextToSpeech(this, new OnInitListener() {
			@Override
			public void onInit(int status) {
			}
		});
		mTextToSpeach.setLanguage(Locale.US);

		/*
		 * Setup the notification bar and move to foreground
		 */
		newNotification("INC-ZONE Application Started", 0);
		registerReceivers();

		// Start other services the app depends on
		ObuBluetoothService.startService(this);
		VehicleDiagnosticsService.startService(this);

	}

	@Override
	public void onDestroy() {

		if (Environment.getExternalStorageState().equals(
				Environment.MEDIA_MOUNTED)) {
			File file = new File(Environment.getExternalStorageDirectory()
					+ "/inczonelogs/" + rKml.getName());
			try {
				file.createNewFile();
			} catch (Exception e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

			FileWriter fw;
			try {
				fw = new FileWriter(file);
				fw.write(rKml.getXml());
				fw.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

		}

		mSettings
				.unregisterOnSharedPreferenceChangeListener(rSettingsChangeListener);

		ObuBluetoothService.stopService(this);
		VehicleDiagnosticsService.stopService(this);

		// Stop all services
		new Handler().postDelayed(new Runnable() {

			@Override
			public void run() {
				NTripService.stopService(ApplicationMonitorService.this);
			}
		}, 5000);

		unregisterReceivers();

		// Remove from notification bar
		stopForeground(true);
		mTextToSpeach.shutdown();

		mAppLog.i("ApplicationMonitorService", "onDestroy()");
		ApplicationLog.resetInstance();
		super.onDestroy();
	}

	OnSharedPreferenceChangeListener rSettingsChangeListener = new OnSharedPreferenceChangeListener() {

		@Override
		public void onSharedPreferenceChanged(
				SharedPreferences sharedPreferences, String key) {

		}
	};

	/**
	 * Creates and displays a new notification
	 * 
	 * @param strNotification
	 *            Notification Text
	 * @param flags
	 *            Notification option flags
	 */
	private void newNotification(String strNotification, int flags) {

		/*
		 * Generic parts of the notification
		 */
		Intent intent = new Intent(this, MainActivity.class);
		intent.setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP
				| Intent.FLAG_ACTIVITY_SINGLE_TOP);

		PendingIntent pendIntent = PendingIntent
				.getActivity(this, 0, intent, 0);
		Notification.Builder notice = new Notification.Builder(this)
				.setContentTitle("INC-ZONE")
				.setContentText(
						"INC-ZONE UI is currently active.  Click to open.")
				.setSmallIcon(R.drawable.icon).setAutoCancel(false)
				.setContentIntent(pendIntent).setAutoCancel(true);

		/*
		 * Flag options of the notification.
		 */
		if ((flags & N_NOTIFICATION_BAR) > 0) {
			notice.setTicker(strNotification);
		}
		if ((flags & N_VIBRATE) > 0) {
			notice.setVibrate(new long[] { 0, 250, 250, 750 });
		}
		if ((flags & N_BEEP) > 0) {
			notice.setSound(RingtoneManager
					.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION));
		}
		if ((flags & N_TEXT_TO_SPEECH_CLEAR) > 0) {
			mTextToSpeach.stop();
		}
		if ((flags & N_TEXT_TO_SPEECH) > 0
				&& mSettings.getBoolean(
						getResources()
								.getString(R.string.setting_enableTTS_key),
						true)) {
			mTextToSpeach.speak(strNotification, TextToSpeech.QUEUE_ADD, null);
		}
		if ((flags & N_POP_UP) > 0) {

			// TODO: Stop using toast...
			Toast toast = Toast.makeText(this, strNotification,
					Toast.LENGTH_LONG);
			toast.setGravity(Gravity.CENTER_HORIZONTAL
					| Gravity.CENTER_VERTICAL, 0, 0);

			/*
			 * This is all to change Toast's text view so that the font is
			 * larger and the text is centered
			 */
			LinearLayout toastLayout = (LinearLayout) toast.getView();
			TextView toastTextView = (TextView) toastLayout.getChildAt(0);
			toastTextView.setTextSize(35);
			toastTextView.setGravity(Gravity.CENTER_HORIZONTAL
					| Gravity.CENTER_VERTICAL);

			toast.show();
		}

		startForeground(FOREGROUND_ID, notice.build());

		mAppLog.v("ApplicationMonitorService",
				"newNotification() Displayed a new notification: "
						+ strNotification + " with flags: " + flags);
	}

	/**
	 * Sends an invalidation broadcast with a copy of the application model
	 */
	private synchronized void invalidate() {

		Intent broadcast = new Intent(ACTION_INVALIDATE);
		broadcast.putExtra(EXTRA_MODEL, rApplicationModel);
		LocalBroadcastManager.getInstance(this).sendBroadcast(broadcast);

	}

	/**
	 * Register all the receivers
	 */
	private void registerReceivers() {
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rNtripStateReceiver,
				new IntentFilter(NTripService.ACTION_UPDATED));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rUiRequestUpdateReceiver,
				new IntentFilter(ACTION_UI_REQUEST_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuBluetoothStateReceiver,
				new IntentFilter(ObuBluetoothService.ACTION_STATE_CHANGE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuDiagnosticMessageHandlerReceiver,
				new IntentFilter(DiagnosticMessageHandler.ACTION_UPDATED));

		LocalBroadcastManager.getInstance(this)
				.registerReceiver(
						rVehicleStateReceiver,
						new IntentFilter(
								VehicleDiagnosticsService.ACTION_STATE_CHANGE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rVehicleDataReceiver,
				new IntentFilter(VehicleDiagnosticsService.ACTION_DATA_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rAlertMessageHandlerReceiver,
				new IntentFilter(AlertMessageHandler.ACTION_NEW_ALERT));
	}

	/**
	 * Unregister all the receivers
	 */
	private void unregisterReceivers() {
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rNtripStateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rUiRequestUpdateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuBluetoothStateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuDiagnosticMessageHandlerReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rVehicleStateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rVehicleDataReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rAlertMessageHandlerReceiver);
	}

	/*
	 * GENERAL
	 */
	private final BroadcastReceiver rUiRequestUpdateReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {
			invalidate();
		}
	};

	/*
	 * NTRIP SERVICE
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rNtripStateReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			if (intent.hasExtra(NTripService.EXTRA_STATE))
				rApplicationModel.ntripState = (NTripState) intent.getExtras()
						.get(NTripService.EXTRA_STATE);

			if (intent.hasExtra(NTripService.EXTRA_BYTES_RECEIVED))
				rApplicationModel.ntripBytesReceived = intent.getExtras()
						.getLong(NTripService.EXTRA_BYTES_RECEIVED);

			invalidate();
		}
	};

	/*
	 * OBU BLUETOOTH SERVICE
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rObuBluetoothStateReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {
			ObuBluetoothState newObuBluetoothState = (ObuBluetoothState) intent
					.getExtras().get(ObuBluetoothService.EXTRA_STATE);

			// Compare old vs new state to trigger notifications.
			if (!ObuBluetoothState.isOkay(newObuBluetoothState)
					&& ObuBluetoothState
							.isOkay(rApplicationModel.obuBluetoothState)) {

				newNotification("Lost Connection to DSRC Radio!", N_VIBRATE
						| N_TEXT_TO_SPEECH | N_POP_UP);
				rApplicationModel.alertInfo = new AlertInformation();
				NTripService.stopService(ApplicationMonitorService.this);

				rApplicationModel.obuDiagnostics = new DiagnosticInformation();

			} else if (ObuBluetoothState.isOkay(newObuBluetoothState)
					&& !ObuBluetoothState
							.isOkay(rApplicationModel.obuBluetoothState)) {

				newNotification("Established Connection to DSRC Radio",
						N_NOTIFICATION_BAR);
				NTripService.startService(ApplicationMonitorService.this);

			}

			rApplicationModel.obuBluetoothState = newObuBluetoothState;
			invalidate();
		}
	};

	private final BroadcastReceiver rObuDiagnosticMessageHandlerReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			rApplicationModel.obuDiagnostics = (DiagnosticInformation) intent
					.getParcelableExtra(DiagnosticMessageHandler.EXTRA_INFO);

			if (rCurrentMapPath == null
					&& rApplicationModel.obuDiagnostics.getGpsFix() > 0) {
				int color = rApplicationModel.obuDiagnostics.getGpsFix() == 1 ? Color.YELLOW
						: Color.GREEN;
				rCurrentMapPath = new LineString(rDateFormatter.format(Calendar
						.getInstance().getTime()), color);
				rKml.addLineString(rCurrentMapPath);
			} else if (rCurrentMapPath != null
					&& rApplicationModel.obuDiagnostics.getGpsFix() == 0)
				rCurrentMapPath = null;

			if (rCurrentMapPath != null) {
				rCurrentMapPath.addPoint(new LatLngPoint(
						rApplicationModel.obuDiagnostics.getLatitude(),
						rApplicationModel.obuDiagnostics.getLongitude()));
			}

			invalidate();
		}
	};

	/*
	 * Vehicle Diagnostics
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rVehicleStateReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			VehicleDiagnosticsState newVehicleState = (VehicleDiagnosticsState) intent
					.getExtras().get(VehicleDiagnosticsService.EXTRA_STATE);

			// Compare old vs new state to trigger notifications.
			if (!VehicleDiagnosticsState.isOkay(newVehicleState)
					&& VehicleDiagnosticsState
							.isOkay(rApplicationModel.vehState)) {

				newNotification("Lost Connection to Vehicle!", N_VIBRATE
						| N_TEXT_TO_SPEECH | N_POP_UP);

			} else if (VehicleDiagnosticsState.isOkay(newVehicleState)
					&& !VehicleDiagnosticsState
							.isOkay(rApplicationModel.vehState)) {

				newNotification("Established Connection to Vehicle",
						N_NOTIFICATION_BAR);

			}

			rApplicationModel.vehState = newVehicleState;
			invalidate();
		}
	};

	private final BroadcastReceiver rVehicleDataReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			JSONObject vehData;
			try {
				vehData = new JSONObject(intent.getExtras().getString(
						VehicleDiagnosticsService.EXTRA_DATA));

				JSONArray vehKeys = vehData.names();

				rApplicationModel.vehData.clear();
				if (vehKeys != null) {
					for (int i = 0; i < vehKeys.length(); i++) {

						try {
							String key = vehKeys.getString(i);
							Object value = vehData.get(key);
							rApplicationModel.vehData.put(key, value);
						} catch (JSONException e1) {
							e1.printStackTrace();
						}

					}
				}

			} catch (JSONException e) {
				e.printStackTrace();
			}

			invalidate();
		}
	};

	/*
	 * Alerts
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rAlertMessageHandlerReceiver = new BroadcastReceiver() {
		@Override
		public synchronized void onReceive(Context context, Intent intent) {

			AlertInformation newAlert = (AlertInformation) intent
					.getParcelableExtra(AlertMessageHandler.EXTRA_INFO);

			if (newAlert.getLaneChangeThreatLevel() > rApplicationModel.alertInfo
					.getLaneChangeThreatLevel()) {
				String laneDescription = "";
				String mergeDirection = newAlert.isIsOnLeft() ? "Right"
						: "Left";

				switch (newAlert.getLaneCount()) {
				case 0:
					laneDescription = String.format("%s shoulder",
							newAlert.isIsOnLeft() ? "Left" : "Right");
					break;
				case 1:
					laneDescription = String.format("%s lane",
							newAlert.isIsOnLeft() ? "Left" : "Right");
					break;
				}

				switch (newAlert.getLaneChangeThreatLevel()) {
				case 0:
					newNotification(String.format("%s closed ahead.  .",
							laneDescription), N_TEXT_TO_SPEECH);

					break;
				case 1:
					if (newAlert.getSpeedThreatLevel() >= 2) {
						newNotification(
								String.format(
										"ALERT: %s closed ahead. Slow down and merge %s.  .",
										laneDescription, mergeDirection),
								N_TEXT_TO_SPEECH | N_TEXT_TO_SPEECH_CLEAR);
					} else {
						newNotification(String.format(
								"ALERT: %s closed ahead. Merge %s.  .",
								laneDescription, mergeDirection),
								N_TEXT_TO_SPEECH | N_TEXT_TO_SPEECH_CLEAR);
					}

					break;
				case 2:
					if (newAlert.getSpeedThreatLevel() >= 2) {
						newNotification(
								String.format(
										"WARNING: Slow down and merge %s immediately.  .",
										mergeDirection), N_TEXT_TO_SPEECH
										| N_TEXT_TO_SPEECH_CLEAR);
					} else {
						newNotification(String.format(
								"WARNING: Merge %s immediately.  .",
								mergeDirection), N_TEXT_TO_SPEECH
								| N_TEXT_TO_SPEECH_CLEAR);
					}

					break;
				case 3:
					newNotification(
							String.format("STOP: Imminent collision Detected!"),
							N_TEXT_TO_SPEECH | N_TEXT_TO_SPEECH_CLEAR);
					break;
				}
			}

			if (newAlert.getSpeedThreatLevel() > rApplicationModel.alertInfo
					.getSpeedThreatLevel()
					&& newAlert.getLaneChangeThreatLevel() < 2) {

				switch (newAlert.getSpeedThreatLevel()) {
				case 0:
					newNotification(String.format("Reduced Speed Ahead.  ."),
							N_TEXT_TO_SPEECH);
					break;
				case 1:
					newNotification(
							String.format("ALERT: Reduced Speed Zone. Slow Down.  ."),
							N_TEXT_TO_SPEECH | N_TEXT_TO_SPEECH_CLEAR);
					break;
				case 2:
					newNotification(
							String.format("WARNING: Slow Down. Reduced Speed Zone.  ."),
							N_TEXT_TO_SPEECH | N_TEXT_TO_SPEECH_CLEAR);
					break;
				}
			}

			rApplicationModel.alertInfo = newAlert;
			invalidate();
		}
	};

}
