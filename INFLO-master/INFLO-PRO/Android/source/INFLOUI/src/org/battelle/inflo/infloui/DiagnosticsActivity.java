/**
 * @file         infloui/DiagnosticsActivity.java
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

import java.text.DecimalFormat;
import java.util.Random;

import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.alerts.SpdHarmAlert;
import org.battelle.inflo.infloui.cloud.TmeCloudEndpointStatistics;
import org.battelle.inflo.infloui.cloud.TmeCloudState;
import org.battelle.inflo.infloui.obu.ObuBluetoothState;
import org.battelle.inflo.infloui.odbii.VehicleDiagnosticsState;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Color;
import android.os.Bundle;
import android.support.v4.content.LocalBroadcastManager;
import android.view.View;
import android.widget.GridLayout;
import android.widget.LinearLayout;
import android.widget.TextView;

public class DiagnosticsActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_diagnostics);

		((TextView) findViewById(R.id.diag_txtUiVersion))
				.setText("UI Version: "
						+ getResources().getString(R.string.config_version));
	}

	@Override
	protected void onResume() {
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rInvalidationReceiver,
				new IntentFilter(ApplicationMonitorService.ACTION_INVALIDATE));

		ApplicationMonitorService.requestUpdate(this);

		super.onResume();
	}

	@Override
	protected void onPause() {

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rInvalidationReceiver);
		super.onPause();
	}

	/**
	 * Application Monitor will send this broadcast when the model changes
	 */
	BroadcastReceiver rInvalidationReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			invalidate((ApplicationModel) intent.getExtras().getParcelable(
					ApplicationMonitorService.EXTRA_MODEL));
		}
	};

	long obuLastRvBsmCount = 0;
	long obuLastTimRelayCount = 0;
	long obuLastId = -1;

	/**
	 * Causes the activity to redraw itself with the new model data
	 * 
	 * @param rModel
	 */
	private void invalidate(ApplicationModel rModel) {

		ApplicationLog.getInstance().v("DiagnosticsActivity",
				"invalidate() Invalidated from broadcasted model");

		DecimalFormat df = new DecimalFormat("#.###");

		/*
		 * Alerts
		 */

		if (rModel.alertSpdHarm == null) {
			((TextView) findViewById(R.id.diag_txtSpdHarmAlert))
					.setText("No Active Alert");
		} else {
			((TextView) findViewById(R.id.diag_txtSpdHarmAlert)).setText(String
					.format("Speed: %d MPH\nMessage: \"%s\"",
							rModel.alertSpdHarm.getSpeed(),
							rModel.alertSpdHarm.getJustificationText()));
		}

		if (rModel.alertQWarn == null) {
			((TextView) findViewById(R.id.diag_txtQWarnAlert))
					.setText("No Active Alert");
		} else {
			if (rModel.alertQWarn.isInQueueAlert()) {
				((TextView) findViewById(R.id.diag_txtQWarnAlert))
						.setText(String
								.format("Length: %.2f miles\nTime Remaining: %d minutes\nMessage: \"%s\"",
										rModel.alertQWarn.getDistanceToFOQ(),
										rModel.alertQWarn.getTimeToFOQ(),
										rModel.alertQWarn
												.getRecommendedAction()));
			} else if (rModel.alertQWarn.isQueueAheadAlert()) {

				((TextView) findViewById(R.id.diag_txtQWarnAlert))
						.setText(String
								.format("Distance To Queue: %.2f miles\nMessage: \"%s\"",
										rModel.alertQWarn.getDistanceToBOQ(),
										rModel.alertQWarn
												.getRecommendedAction()));
			} else {
				((TextView) findViewById(R.id.diag_txtQWarnAlert))
						.setText("Malformed Q-Warn Alert");
			}
		}

		/*
		 * Cloud
		 */
		TmeCloudState concatinatedState = rModel.tmeCloudEndpointStatistics.length == 0 ? TmeCloudState.Unknown
				: TmeCloudState.Available;
		for (TmeCloudEndpointStatistics i : rModel.tmeCloudEndpointStatistics) {
			if (i.getState() == TmeCloudState.Unavailable) {
				concatinatedState = TmeCloudState.Unavailable;
				break;
			}
		}

		TextView txtCloudState = ((TextView) findViewById(R.id.diag_txtCloudState));
		txtCloudState.setText(concatinatedState.toString().replace('_', ' '));
		txtCloudState
				.setTextColor(TmeCloudState.isOkay(concatinatedState) ? Color.GREEN
						: Color.RED);

		LinearLayout layoutCloudStat = (LinearLayout) findViewById(R.id.diag_layoutCloudStats);

		layoutCloudStat.removeAllViews();
		for (TmeCloudEndpointStatistics i : rModel.tmeCloudEndpointStatistics) {

			LinearLayout layout = new LinearLayout(this);
			layout.setOrientation(LinearLayout.HORIZONTAL);
			layoutCloudStat.addView(layout);

			TextView txtUrl = new TextView(this);
			txtUrl.setText(i.getUrl());
			txtUrl.setPadding(30, 0, 0, 0);
			txtUrl.setTextColor(TmeCloudState.isOkay(i.getState()) ? Color.GREEN
					: Color.RED);
			layout.addView(txtUrl);

			TextView txtStatus = new TextView(this);
			txtStatus.setText(i.getState().toString().replace('_', ' '));
			txtStatus.setPadding(60, 0, 0, 0);
			txtStatus
					.setTextColor(TmeCloudState.isOkay(i.getState()) ? Color.GREEN
							: Color.RED);
			// layout.addView(txtStatus);
		}

		/*
		 * OBU Bluetooth
		 */
		TextView txtObuBtState = ((TextView) findViewById(R.id.diag_txtObuBtState));
		txtObuBtState.setText(rModel.obuBluetoothState.toString().replace('_',
				' '));
		txtObuBtState.setTextColor(ObuBluetoothState
				.isOkay(rModel.obuBluetoothState) ? Color.GREEN : Color.RED);

		/*
		 * ((LinearLayout) findViewById(R.id.diag_layoutObuInfo))
		 * .setVisibility(ObuBluetoothState .isOkay(rModel.obuBluetoothState) ?
		 * View.VISIBLE : View.GONE);
		 */
		if (rModel.obuDiagnostics.getId() != obuLastId) {

			((TextView) findViewById(R.id.diag_txtObuVersion))
					.setText("OBU Version: "
							+ rModel.obuDiagnostics.getVersion());

			((TextView) findViewById(R.id.diag_txtRsuInRange))
					.setTextColor(rModel.obuDiagnostics.isRsuConnected() ? Color.GREEN
							: Color.DKGRAY);
			((TextView) findViewById(R.id.diag_txtReceivingBsms))
					.setTextColor(rModel.obuDiagnostics.getRvBsmsReceived() != obuLastRvBsmCount
							&& rModel.obuDiagnostics.getRvBsmsReceived() != -1 ? Color.GREEN
							: Color.DKGRAY);
			((TextView) findViewById(R.id.diag_txtRelayingTims))
					.setTextColor(rModel.obuDiagnostics.getTimRelayCount() != obuLastTimRelayCount
							&& rModel.obuDiagnostics.getTimRelayCount() != -1 ? Color.GREEN
							: Color.DKGRAY);
			((TextView) findViewById(R.id.diag_txtQueued))
					.setTextColor(rModel.obuDiagnostics.isQueued() ? Color.RED
							: Color.DKGRAY);

			((TextView) findViewById(R.id.diag_txtRsuRssi))
					.setText(String.format("RSU RSSI: %d",
							rModel.obuDiagnostics.getRsuRssi()));

			TextView obuBatteryPerc = ((TextView) findViewById(R.id.diag_txtObuBatteryPerc));
			if (rModel.obuDiagnostics.getBatteryLevel() != -1) {
				obuBatteryPerc.setText(String
						.valueOf((int) rModel.obuDiagnostics.getBatteryLevel()
								+ " %"));
				if (rModel.obuDiagnostics.getBatteryLevel() < 25)
					obuBatteryPerc.setTextColor(Color.RED);
				else if (rModel.obuDiagnostics.getBatteryLevel() < 50)
					obuBatteryPerc.setTextColor(Color.YELLOW);
				else
					obuBatteryPerc.setTextColor(Color.WHITE);
			} else {
				obuBatteryPerc.setText("Unavailable");
				obuBatteryPerc.setTextColor(Color.WHITE);
			}

			TextView obuGpsFix = ((TextView) findViewById(R.id.diag_txtObuGpsStatus));
			if (!rModel.obuDiagnostics.isGpsFixed()) {
				obuGpsFix.setText("No GPS Fix");
				obuGpsFix.setTextColor(Color.RED);
			} else {
				obuGpsFix.setText(String.format("%.0f\u00B0 @ %2.1f m/s",
						rModel.obuDiagnostics.getHeading(),
						rModel.obuDiagnostics.getSpeed()));
				obuGpsFix.setTextColor(Color.WHITE);
			}

			if (rModel.obuDiagnostics.getMmarker() != -1) {
				((TextView) findViewById(R.id.diag_txtRoadway)).setText(String
						.format("%s @mm %.2f",
								rModel.obuDiagnostics.getRoadway(),
								rModel.obuDiagnostics.getMmarker()));
			} else {
				((TextView) findViewById(R.id.diag_txtRoadway))
						.setText("Unavailable");
			}

			((TextView) findViewById(R.id.diag_txtRemoteVehicles))
					.setText(rModel.obuDiagnostics.getRvcount() != -1 ? String
							.valueOf(rModel.obuDiagnostics.getRvcount())
							: "Unavailable");
			((TextView) findViewById(R.id.diag_txtRvDist))
					.setText(rModel.obuDiagnostics.getRvdist() != -1 ? df
							.format(rModel.obuDiagnostics.getRvdist())
							+ " meters" : "Unavailable");
			((TextView) findViewById(R.id.diag_txtRssi))
					.setText(rModel.obuDiagnostics.getRssi() != -1 ? df
							.format(rModel.obuDiagnostics.getRssi())
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtBsmReceivedCount))
					.setText(String.valueOf(rModel.obuBsmReceivedCount));
			((TextView) findViewById(R.id.diag_txtBsmPostedCount))
					.setText(String.valueOf(rModel.obuBsmPostedCount));

			((TextView) findViewById(R.id.diag_txtTimRequestCount))
					.setText(String.valueOf(rModel.obuTimRequestCount));
			((TextView) findViewById(R.id.diag_txtTimResponseCount))
					.setText(String.valueOf(rModel.obuTimResponseCount));
		}

		/*
		 * Vehicle
		 */
		TextView txtVehState = ((TextView) findViewById(R.id.diag_txtVehState));
		txtVehState.setText(rModel.vehState.toString().replace('_', ' '));
		txtVehState.setTextColor(VehicleDiagnosticsState
				.isOkay(rModel.vehState) ? Color.GREEN : Color.RED);

		if (rModel.vehState == VehicleDiagnosticsState.Disabled) {
			((GridLayout) findViewById(R.id.diag_layoutVehData))
					.setVisibility(View.GONE);
		} else {

			((GridLayout) findViewById(R.id.diag_layoutVehData))
					.setVisibility(View.VISIBLE);

			((TextView) findViewById(R.id.diag_txtVehVin))
					.setText(rModel.vehData.containsKey("vin") ? rModel.vehData
							.get("vin").toString() : "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehSpeed))
					.setText(rModel.vehData.containsKey("spd") ? rModel.vehData
							.get("spd").toString() + " kmph" : "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehRpm))
					.setText(rModel.vehData.containsKey("rpm") ? rModel.vehData
							.get("rpm").toString() + " rpm" : "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehThrottle))
					.setText(rModel.vehData.containsKey("throttle") ? rModel.vehData
							.get("throttle").toString() + " %"
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehMaf))
					.setText(rModel.vehData.containsKey("maf") ? rModel.vehData
							.get("maf").toString() + " g/sec" : "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehAirTemp))
					.setText(rModel.vehData.containsKey("airtemp") ? rModel.vehData
							.get("airtemp").toString() + " \u2103"
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtVehPres))
					.setText(rModel.vehData.containsKey("pres") ? rModel.vehData
							.get("pres").toString() + " kPa"
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtBrakePosition))
					.setText(rModel.vehData.containsKey("brakeposition") ? rModel.vehData
							.get("brakeposition").toString() + " %"
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtSteeringAngle))
					.setText(rModel.vehData.containsKey("steerangle") ? rModel.vehData
							.get("steerangle").toString() + " degrees"
							: "Unavailable");

			if (!rModel.vehData.containsKey("wipers")) {
				((TextView) findViewById(R.id.diag_txtWipers))
						.setText("Unavailable");
			} else {

				String text = "Unknown";
				switch (Integer.parseInt(rModel.vehData.get("wipers")
						.toString())) {
				case 0:
					text = "Off";
					break;
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
					text = "Delay";
					break;
				case 6:
					text = "Low/Single Wipe";
					break;
				case 7:
					text = "High";
					break;
				}

				((TextView) findViewById(R.id.diag_txtWipers)).setText(text);
			}

			((TextView) findViewById(R.id.diag_txtLongAccel))
					.setText(rModel.vehData.containsKey("longaccel") ? rModel.vehData
							.get("longaccel").toString() + " m/s^2"
							: "Unavailable");

			((TextView) findViewById(R.id.diag_txtLatAccel))
					.setText(rModel.vehData.containsKey("lataccel") ? rModel.vehData
							.get("lataccel").toString() + " m/s^2"
							: "Unavailable");
		}

		/*
		 * Weather
		 */
		((TextView) findViewById(R.id.diag_txtWeatherTemp)).setText(df
				.format(rModel.weatherTemp) + " \u2103");
		((TextView) findViewById(R.id.diag_txtWeatherPres)).setText(df
				.format(rModel.weatherPressure) + " hPa");
		((TextView) findViewById(R.id.diag_txtWeatherHum)).setText(df
				.format(rModel.weatherHumidity) + " %");

		obuLastRvBsmCount = rModel.obuDiagnostics.getRvBsmsReceived();
		obuLastTimRelayCount = rModel.obuDiagnostics.getTimRelayCount();
		obuLastId = rModel.obuDiagnostics.getId();
	}

	public void onGenerateQWarnAlert(View v) {

		if (new Random().nextDouble() > 0.5) {
			QWarnAlert.broadcastNewAlert(this, -1,
					new Random().nextDouble() * 2,
					new Random().nextDouble() * 3, new Random().nextInt(20),
					"Test Q-Warn Message");
		} else {

			QWarnAlert.broadcastNewAlert(this, new Random().nextDouble() * 2,
					-1, new Random().nextDouble() * 3,
					new Random().nextInt(20), "Test Q-Warn Message");
		}
	}

	public void onGenerateSpdHarmAlert(View v) {
		SpdHarmAlert.broadcastNewAlert(this,
				(new Random().nextInt(10) + 4) * 5, "Test Spd-Harm Message");
	}
}
