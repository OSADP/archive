/**
 * @file         infloui/ApplicationMonitorService.java
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

package org.battelle.inflo.infloui;

import java.lang.Thread.UncaughtExceptionHandler;
import java.util.Locale;

import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.alerts.SpdHarmAlert;
import org.battelle.inflo.infloui.cloud.TmeCloudEndpointStatistics;
import org.battelle.inflo.infloui.cloud.TmeCloudService;
import org.battelle.inflo.infloui.obu.ObuBluetoothService;
import org.battelle.inflo.infloui.obu.ObuBluetoothState;
import org.battelle.inflo.infloui.obu.handlers.BsmMessageHandler;
import org.battelle.inflo.infloui.obu.handlers.DiagnosticMessageHandler;
import org.battelle.inflo.infloui.obu.handlers.DiagnosticMessageHandler.DiagnosticInformation;
import org.battelle.inflo.infloui.obu.handlers.TimRequestMessageHandler;
import org.battelle.inflo.infloui.odbii.VehicleDiagnosticsService;
import org.battelle.inflo.infloui.odbii.VehicleDiagnosticsState;
import org.battelle.inflo.infloui.weather.WeatherService;
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
import android.media.RingtoneManager;
import android.os.Handler;
import android.os.IBinder;
import android.os.Parcelable;
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

	private final static String PREFIX = "org.battelle.inflo.infloui.ApplicationMonitorService";

	public final static String ACTION_INVALIDATE = PREFIX
			+ ".action_invalidate";
	private final static String ACTION_UI_REQUEST_UPDATE = PREFIX
			+ ".action_ui_request_update";
	public final static String EXTRA_MODEL = PREFIX + ".extra_model";

	final static int FOREGROUND_ID = 24601;

	private int alertLifeTimeout = 10;

	/*
	 * Notification Option Flags
	 */
	public static final int N_VIBRATE = 0x01;
	public static final int N_BEEP = 0x02;
	public static final int N_TEXT_TO_SPEECH = 0x04;
	public static final int N_POP_UP = 0x08;
	public static final int N_NOTIFICATION_BAR = 0x10;

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

	private ApplicationLog rAppLog;
	private SharedPreferences rSettings;
	private TextToSpeech rTextToSpeach;
	private Handler rHandler;
	private final ApplicationModel rApplicationModel = new ApplicationModel();

	public ApplicationMonitorService() {
	}

	@Override
	public IBinder onBind(Intent intent) {
		return null; // Not binding
	}

	@Override
	public void onCreate() {
		super.onCreate();

		rHandler = new Handler();

		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("ApplicationMonitorService", "onCreate()");

		rSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);

		rSettings
				.registerOnSharedPreferenceChangeListener(rSettingsChangeListener);

		try {
			alertLifeTimeout = 1000 * Integer.parseInt(rSettings
					.getString(
							getResources().getString(
									R.string.setting_alertTimeout_key), "10"));
		} catch (NumberFormatException e) {

		}

		// Start logger and set default uncaught exceptions
		Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {

			@Override
			public void uncaughtException(Thread arg0, Throwable arg1) {
				ApplicationLog.getInstance().e(arg0.getName(), "UncaughtError",
						arg1);
			}
		});

		this.rTextToSpeach = new TextToSpeech(this, new OnInitListener() {
			@Override
			public void onInit(int status) {
			}
		});
		rTextToSpeach.setLanguage(Locale.US);

		/*
		 * Setup the notification bar and move to foreground
		 */
		newNotification("INFLO Application Started", 0);
		registerReceivers();

		// Start other services the app depends on
		ObuBluetoothService.startService(this);
		VehicleDiagnosticsService.startService(this);
		WeatherService.startService(this);

	}

	@Override
	public void onDestroy() {

		rSettings
				.unregisterOnSharedPreferenceChangeListener(rSettingsChangeListener);

		ObuBluetoothService.stopService(this);
		VehicleDiagnosticsService.stopService(this);

		// Stop all services
		new Handler().postDelayed(new Runnable() {

			@Override
			public void run() {
				WeatherService.stopService(ApplicationMonitorService.this);
				TmeCloudService.stopService(ApplicationMonitorService.this);
			}
		}, 2000);

		unregisterReceivers();

		// Remove from notification bar
		stopForeground(true);
		rTextToSpeach.shutdown();

		rAppLog.i("ApplicationMonitorService", "onDestroy()");
		ApplicationLog.resetInstance();
		super.onDestroy();
	}

	OnSharedPreferenceChangeListener rSettingsChangeListener = new OnSharedPreferenceChangeListener() {

		@Override
		public void onSharedPreferenceChanged(
				SharedPreferences sharedPreferences, String key) {

			try {
				alertLifeTimeout = 1000 * Integer.parseInt(rSettings.getString(
						getResources().getString(
								R.string.setting_alertTimeout_key), "10"));
			} catch (NumberFormatException e) {

			}

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
				.setContentTitle("INFLO")
				.setContentText("INFLO UI is currently active.  Click to open.")
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
		if ((flags & N_TEXT_TO_SPEECH) > 0
				&& rSettings.getBoolean(
						getResources()
								.getString(R.string.setting_enableTTS_key),
						true)) {
			rTextToSpeach.speak(strNotification, TextToSpeech.QUEUE_ADD, null);
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

		rAppLog.v("ApplicationMonitorService",
				"newNotification() Displayed a new notification: "
						+ strNotification + " with flags: " + flags);
	}

	/**
	 * Sends an invalidation broadcast with a copy of the application model
	 */
	private void invalidate() {
		Intent broadcast = new Intent(ACTION_INVALIDATE);
		broadcast.putExtra(EXTRA_MODEL, rApplicationModel);
		LocalBroadcastManager.getInstance(this).sendBroadcast(broadcast);
	}

	/**
	 * Register all the receivers
	 */
	private void registerReceivers() {
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rUiRequestUpdateReceiver,
				new IntentFilter(ACTION_UI_REQUEST_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rCloudStateReceiver,
				new IntentFilter(TmeCloudService.ACTION_STATISTICS_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuBluetoothStateReceiver,
				new IntentFilter(ObuBluetoothService.ACTION_STATE_CHANGE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rBsmMessageHandlerReceiver,
				new IntentFilter(BsmMessageHandler.ACTION_UPDATED));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rTimRequestMessageHandlerReceiver,
				new IntentFilter(TimRequestMessageHandler.ACTION_UPDATED));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuDiagnosticMessageHandlerReceiver,
				new IntentFilter(DiagnosticMessageHandler.ACTION_UPDATED));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rWeatherReceiver,
				new IntentFilter(WeatherService.ACTION_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rAlertQWarnReceiver, new IntentFilter(QWarnAlert.ACTION_ALERT));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rAlertSpdHarmReceiver,
				new IntentFilter(SpdHarmAlert.ACTION_ALERT));

		LocalBroadcastManager.getInstance(this)
				.registerReceiver(
						rVehicleStateReceiver,
						new IntentFilter(
								VehicleDiagnosticsService.ACTION_STATE_CHANGE));
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rVehicleDataReceiver,
				new IntentFilter(VehicleDiagnosticsService.ACTION_DATA_UPDATE));
	}

	/**
	 * Unregister all the receivers
	 */
	private void unregisterReceivers() {
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rUiRequestUpdateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rCloudStateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuBluetoothStateReceiver);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rBsmMessageHandlerReceiver);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rTimRequestMessageHandlerReceiver);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuDiagnosticMessageHandlerReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rWeatherReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rAlertQWarnReceiver);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rAlertSpdHarmReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rVehicleStateReceiver);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rVehicleDataReceiver);
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
	 * TME CLOUD SERVICE
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rCloudStateReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			Parcelable[] parcelCloudStatistics = intent
					.getParcelableArrayExtra(TmeCloudService.EXTRA_STATISTICS);

			TmeCloudEndpointStatistics[] cloudStatistics = new TmeCloudEndpointStatistics[parcelCloudStatistics.length];
			for (int i = 0; i < parcelCloudStatistics.length; i++) {
				cloudStatistics[i] = (TmeCloudEndpointStatistics) parcelCloudStatistics[i];
			}

			rApplicationModel.tmeCloudEndpointStatistics = cloudStatistics;

			/*
			 * TmeCloudState newCloudState = (TmeCloudState)
			 * intent.getExtras().get( TmeCloudService.EXTRA_STATISTICS);
			 * 
			 * // Compare old vs new state to trigger notifications. if
			 * (!TmeCloudState.isOkay(newCloudState) &&
			 * TmeCloudState.isOkay(rApplicationModel.tmeCloudState)) {
			 * 
			 * newNotification("Lost Connection to TME!", N_TEXT_TO_SPEECH |
			 * N_POP_UP);
			 * 
			 * } else if (TmeCloudState.isOkay(newCloudState) &&
			 * !TmeCloudState.isOkay(rApplicationModel.tmeCloudState)) {
			 * 
			 * newNotification("Established Connection to TME",
			 * N_NOTIFICATION_BAR);
			 * 
			 * }
			 * 
			 * rApplicationModel.tmeCloudState = newCloudState;
			 * rApplicationModel.tmeCloudAverageLatency = intent.getDoubleExtra(
			 * TmeCloudService.EXTRA_AVERAGE_LATENCY, 0);
			 * rApplicationModel.tmeCloudCurrentLatency = intent.getDoubleExtra(
			 * TmeCloudService.EXTRA_CURRENT_LATENCY, 0);
			 * rApplicationModel.tmeCloudErrorCount = intent.getIntExtra(
			 * TmeCloudService.EXTRA_ERROR_COUNT, 0);
			 * rApplicationModel.tmeCloudSuccessCount = intent.getIntExtra(
			 * TmeCloudService.EXTRA_SUCCESS_COUNT, 0);
			 */
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

				newNotification("Lost Connection to OBU!", N_VIBRATE
						| N_TEXT_TO_SPEECH | N_POP_UP);

				rApplicationModel.obuDiagnostics = new DiagnosticInformation();

			} else if (ObuBluetoothState.isOkay(newObuBluetoothState)
					&& !ObuBluetoothState
							.isOkay(rApplicationModel.obuBluetoothState)) {

				newNotification("Established Connection to OBU",
						N_NOTIFICATION_BAR);

			}

			rApplicationModel.obuBluetoothState = newObuBluetoothState;
			invalidate();
		}
	};

	private final BroadcastReceiver rBsmMessageHandlerReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			rApplicationModel.obuBsmPostedCount = intent.getIntExtra(
					BsmMessageHandler.EXTRA_POSTED_COUNT, 0);
			rApplicationModel.obuBsmReceivedCount = intent.getIntExtra(
					BsmMessageHandler.EXTRA_RECEIVED_COUNT, 0);

			invalidate();
		}
	};

	private final BroadcastReceiver rTimRequestMessageHandlerReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			rApplicationModel.obuTimRequestCount = intent.getIntExtra(
					TimRequestMessageHandler.EXTRA_TIM_REQUEST_COUNT, 0);
			rApplicationModel.obuTimResponseCount = intent.getIntExtra(
					TimRequestMessageHandler.EXTRA_TIM_RESPONSE_COUNT, 0);

			invalidate();
		}
	};

	private final BroadcastReceiver rObuDiagnosticMessageHandlerReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			rApplicationModel.obuDiagnostics = (DiagnosticInformation) intent
					.getParcelableExtra(DiagnosticMessageHandler.EXTRA_INFO);

			synchronized (rApplicationModel) {
				if (!rApplicationModel.obuDiagnostics.isGpsFixed()) {
					rApplicationModel.alertQWarn = null;
					rApplicationModel.alertSpdHarm = null;
				}
			}

			invalidate();
		}
	};

	/*
	 * WEATHER SERVICE
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rWeatherReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			rApplicationModel.weatherTemp = intent.getDoubleExtra(
					WeatherService.EXTRA_AMBIENT_TEMP, 0);
			rApplicationModel.weatherPressure = intent.getDoubleExtra(
					WeatherService.EXTRA_PRESSURE, 0);
			rApplicationModel.weatherHumidity = intent.getDoubleExtra(
					WeatherService.EXTRA_HUMIDITY, 0);

			invalidate();
		}
	};

	/*
	 * ALERTS
	 * ________________________________________________________________________________________________
	 */
	private final BroadcastReceiver rAlertQWarnReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			synchronized (rApplicationModel) {
				if (!rApplicationModel.obuDiagnostics.isGpsFixed())
					return;
			}

			QWarnAlert alert = (QWarnAlert) intent.getExtras().get(
					QWarnAlert.EXTRA_ALERT);

			if (rApplicationModel.setQWarnAlert(alert)
					&& !alert.getRecommendedAction().equals("")) {
				newNotification(alert.getRecommendedAction(), N_TEXT_TO_SPEECH);
			}

			rHandler.removeCallbacks(rAlertQWarnResetRunnable);
			rHandler.postDelayed(rAlertQWarnResetRunnable, alertLifeTimeout);

			invalidate();
		}
	};

	private final Runnable rAlertQWarnResetRunnable = new Runnable() {

		@Override
		public void run() {
			rApplicationModel.alertQWarn = null;
			invalidate();
		}
	};

	private final BroadcastReceiver rAlertSpdHarmReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {

			synchronized (rApplicationModel) {
				if (!rApplicationModel.obuDiagnostics.isGpsFixed())
					return;
			}

			SpdHarmAlert alert = (SpdHarmAlert) intent.getExtras().get(
					SpdHarmAlert.EXTRA_ALERT);

			if (rApplicationModel.setSpdHarmAlert(alert)
					&& !alert.getJustificationText().equals("")) {
				newNotification(String.format(
						"Speed Harm. %d Miles Per Hour. %s", alert.getSpeed(),
						alert.getJustificationText()), N_TEXT_TO_SPEECH);
			}

			rHandler.removeCallbacks(rAlertSpdHarmResetRunnable);
			rHandler.postDelayed(rAlertSpdHarmResetRunnable, alertLifeTimeout);

			invalidate();
		}
	};

	private final Runnable rAlertSpdHarmResetRunnable = new Runnable() {

		@Override
		public void run() {
			rApplicationModel.alertSpdHarm = null;
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

}
