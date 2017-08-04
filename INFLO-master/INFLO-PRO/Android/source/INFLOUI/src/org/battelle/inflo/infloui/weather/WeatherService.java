/**
 * @file         infloui/obu/weather/WeatherService.java
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

package org.battelle.inflo.infloui.weather;

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.R;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Handler;
import android.os.IBinder;
import android.support.v4.content.LocalBroadcastManager;

/**
 * Android service that polls a list of sensors at a given rate and broadcasts
 * their values for other activities/services to consume.
 * 
 */
public class WeatherService extends Service {

	private final static String PREFIX = "org.battelle.inflo.infloui.weather.WeatherService";

	/**
	 * Broadcast action for new sensor updates
	 */
	public final static String ACTION_UPDATE = PREFIX + ".action_update";

	/*
	 * Extra's of the sensor values
	 */
	public final static String EXTRA_AMBIENT_TEMP = PREFIX + ".extra_ambient_temp";
	public final static String EXTRA_HUMIDITY = PREFIX + ".extra_humidity";
	public final static String EXTRA_PRESSURE = PREFIX + ".extra_pressure";

	/**
	 * Stops the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void stopService(Context context) {

		context.stopService(new Intent(context, WeatherService.class));
	}

	/**
	 * Starts the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void startService(Context context) {

		context.startService(new Intent(context, WeatherService.class));
	}

	/**
	 * List of the sensor types that will be used
	 */
	private static final int[] SENSOR_LIST = { Sensor.TYPE_AMBIENT_TEMPERATURE,
			Sensor.TYPE_PRESSURE, Sensor.TYPE_RELATIVE_HUMIDITY };

	/**
	 * List of keys for placing the sensor values into an extra's bundle. Length
	 * of this MUST match {@code SENSOR_LIST}
	 */
	private static final String[] SENSOR_EXTRA_KEYS = { EXTRA_AMBIENT_TEMP, EXTRA_PRESSURE,
			EXTRA_HUMIDITY };

	/**
	 * Storage array for the sensor values.
	 */
	private final double[] sensorValues = new double[SENSOR_LIST.length];

	/**
	 * Handler that the postDelayed is called on to time the broadcast of sensor
	 * updates
	 */
	private Handler rHandler;
	private SensorManager rSensorManager;
	private ApplicationLog rAppLog;
	private int pollRate;

	public WeatherService() {
	}

	@Override
	public IBinder onBind(Intent intent) {
		return null;
	}

	@Override
	public void onCreate() {
		super.onCreate();
		rAppLog = ApplicationLog.getInstance();
		rAppLog.i("WeatherService", "onCreate()");

		rSensorManager = (SensorManager) this.getSystemService(SENSOR_SERVICE);

		// Register for callbacks on each sensor
		for (int s : SENSOR_LIST) {
			Sensor sensor = rSensorManager.getDefaultSensor(s);
			rSensorManager.registerListener(rSensorListener, sensor,
					SensorManager.SENSOR_DELAY_NORMAL);
		}

		pollRate = Integer.parseInt(getResources().getString(R.string.config_weatherUpdateRate));

		rHandler = new Handler();
		rHandler.postDelayed(rBroadcastTask, 1000);
	}

	@Override
	public void onDestroy() {

		rHandler.removeCallbacksAndMessages(null);
		rHandler = null;

		rSensorManager.unregisterListener(rSensorListener);

		rAppLog.i("WeatherService", "onDestroy()");

		super.onDestroy();
	}

	/**
	 * Callback for when any sensor updates. Searches for the appropriate
	 * storage index to place the value within according to the sensor type.
	 */
	SensorEventListener rSensorListener = new SensorEventListener() {

		@Override
		public void onSensorChanged(SensorEvent event) {

			for (int i = 0; i < SENSOR_LIST.length; i++) {

				if (event.sensor.getType() == SENSOR_LIST[i]) {

					sensorValues[i] = event.values[0];

				}
			}
		}

		@Override
		public void onAccuracyChanged(Sensor sensor, int accuracy) {

		}
	};

	/**
	 * Runnable that broadcasts the current sensor values and then reschedules
	 * itself to run again after {@code pollRate} milliseconds.
	 */
	Runnable rBroadcastTask = new Runnable() {

		@Override
		public void run() {

			rAppLog.i("WeatherService", "Broadcasting Sensor Updates");

			Intent broadcast = new Intent(ACTION_UPDATE);

			for (int i = 0; i < SENSOR_LIST.length; i++) {
				broadcast.putExtra(SENSOR_EXTRA_KEYS[i], sensorValues[i]);
			}

			LocalBroadcastManager.getInstance(WeatherService.this).sendBroadcast(broadcast);

			if (rHandler != null)
				rHandler.postDelayed(rBroadcastTask, pollRate);
		}
	};
}
