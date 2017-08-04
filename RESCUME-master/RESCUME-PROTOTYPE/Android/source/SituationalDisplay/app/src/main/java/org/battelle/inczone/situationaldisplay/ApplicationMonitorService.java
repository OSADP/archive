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

package org.battelle.inczone.situationaldisplay;

import java.lang.Thread.UncaughtExceptionHandler;
import java.util.Locale;

import org.battelle.inczone.situationaldisplay.obu.ObuBluetoothService;
import org.battelle.inczone.situationaldisplay.obu.ObuBluetoothState;
import org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.DiagnosticMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.DiagnosticMessageHandler.DiagnosticInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler.EVASInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler.BSMSInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler.TIMSInformation;

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
import android.os.IBinder;
import android.speech.tts.TextToSpeech;
import android.speech.tts.TextToSpeech.OnInitListener;
import android.support.v4.content.LocalBroadcastManager;
import android.util.Log;
import android.view.Gravity;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;


/**
 * 
 */
public class ApplicationMonitorService extends Service {

	private final static String PREFIX = "org.battelle.inczone.situationaldisplay.ApplicationMonitorService";

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

		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("ApplicationMonitorService", "onCreate()");

		rSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);

		rSettings
				.registerOnSharedPreferenceChangeListener(rSettingsChangeListener);

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
		newNotification("Situational Display Started", 0);
		registerReceivers();

		// Start other services the app depends on
		ObuBluetoothService.startService(this);

	}

	@Override
	public void onDestroy() {

		rSettings
				.unregisterOnSharedPreferenceChangeListener(rSettingsChangeListener);

		ObuBluetoothService.stopService(this);

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
				rUiRequestUpdateReceiver,
				new IntentFilter(ACTION_UI_REQUEST_UPDATE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuBluetoothStateReceiver,
				new IntentFilter(ObuBluetoothService.ACTION_STATE_CHANGE));

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rObuDiagnosticMessageHandlerReceiver,
				new IntentFilter(DiagnosticMessageHandler.ACTION_UPDATED));

        LocalBroadcastManager.getInstance(this).registerReceiver(
                rObuEVASMessageHandlerReceiver,
                new IntentFilter(EVASMessageHandler.ACTION_UPDATED));

        LocalBroadcastManager.getInstance(this).registerReceiver(
                rObuBSMSMessageHandlerReceiver,
                new IntentFilter(BSMSMessageHandler.ACTION_UPDATED));

        LocalBroadcastManager.getInstance(this).registerReceiver(
                rObuTIMSMessageHandlerReceiver,
                new IntentFilter(TIMSMessageHandler.ACTION_UPDATED));
	}

	/**
	 * Unregister all the receivers
	 */
	private void unregisterReceivers() {
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rUiRequestUpdateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuBluetoothStateReceiver);

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rObuDiagnosticMessageHandlerReceiver);
        LocalBroadcastManager.getInstance(this).unregisterReceiver(
                rObuEVASMessageHandlerReceiver);
        LocalBroadcastManager.getInstance(this).unregisterReceiver(
                rObuBSMSMessageHandlerReceiver);
        LocalBroadcastManager.getInstance(this).unregisterReceiver(
                rObuTIMSMessageHandlerReceiver);
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

				rApplicationModel.obuDiagnostics = new DiagnosticInformation();
                rApplicationModel.obuEVAsReceived = new EVASInformation();
                rApplicationModel.obuBSMsReceived = new BSMSInformation();
                rApplicationModel.obuTIMsReceived = new TIMSInformation();

			} else if (ObuBluetoothState.isOkay(newObuBluetoothState)
					&& !ObuBluetoothState
							.isOkay(rApplicationModel.obuBluetoothState)) {

				newNotification("Established Connection to DSRC Radio",
						N_NOTIFICATION_BAR);

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

            invalidate();
        }
    };

    private final BroadcastReceiver rObuEVASMessageHandlerReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {

            rApplicationModel.obuEVAsReceived = (EVASInformation) intent
                    .getParcelableExtra(EVASMessageHandler.EXTRA_INFO);

            invalidate();
        }
    };

    private final BroadcastReceiver rObuBSMSMessageHandlerReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            rApplicationModel.obuBSMsReceived = (BSMSInformation) intent
                    .getParcelableExtra(BSMSMessageHandler.EXTRA_INFO);
            invalidate();
        }
    };

    private final BroadcastReceiver rObuTIMSMessageHandlerReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            rApplicationModel.obuTIMsReceived = (TIMSInformation) intent
                    .getParcelableExtra(TIMSMessageHandler.EXTRA_INFO);
            invalidate();
        }
    };
}
