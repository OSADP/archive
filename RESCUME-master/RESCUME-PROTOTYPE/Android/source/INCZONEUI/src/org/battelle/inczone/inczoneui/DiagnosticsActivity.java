/**
 * @file         inczoneui/DiagnosticsActivity.java
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

import org.battelle.inczone.inczoneui.ntrip.NTripState;
import org.battelle.inczone.inczoneui.obu.ObuBluetoothState;
import org.battelle.inczone.inczoneui.odbii.VehicleDiagnosticsState;

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

	/**
	 * Causes the activity to redraw itself with the new model data
	 * 
	 * @param rModel
	 */
	private void invalidate(ApplicationModel rModel) {

		/*
		 * NTRIP
		 */
		TextView txtNtripState = ((TextView) findViewById(R.id.diag_txtNtripState));
		txtNtripState.setText(rModel.ntripState.toString().replace('_', ' '));
		txtNtripState
				.setTextColor(rModel.ntripState == NTripState.Waiting_For_Bluetooth
						|| rModel.ntripState == NTripState.Connecting
						|| rModel.ntripState == NTripState.Disconnected ? Color.RED
						: Color.GREEN);

		if (rModel.ntripBytesReceived >= 10000) {
			((TextView) findViewById(R.id.diag_txtNtripReceived))
					.setText(String.format("%,.2f KB",
							rModel.ntripBytesReceived / 1024.0));
		} else if (rModel.ntripBytesReceived >= 1000 * 1024) {
			((TextView) findViewById(R.id.diag_txtNtripReceived))
					.setText(String.format("%,.2f MB",
							rModel.ntripBytesReceived / 1048576.0));
		} else {
			((TextView) findViewById(R.id.diag_txtNtripReceived))
					.setText(String.format("%,d bytes",
							rModel.ntripBytesReceived));
		}
		/*
		 * OBU Bluetooth
		 */
		TextView txtObuBtState = ((TextView) findViewById(R.id.diag_txtDsrcBluetoothState));
		txtObuBtState.setText(rModel.obuBluetoothState.toString().replace('_',
				' '));
		txtObuBtState.setTextColor(ObuBluetoothState
				.isOkay(rModel.obuBluetoothState) ? Color.GREEN : Color.RED);

		// ((TextView)
		// findViewById(R.id.diag_txtReceivingBsms)).setTextColor(Color.DKGRAY);

		/* GPS */
		TextView obuGpsFixStatus = ((TextView) findViewById(R.id.diag_txtObuGpsFixStatus));
		switch (rModel.obuDiagnostics.getGpsFix()) {
		case 0:
			obuGpsFixStatus.setText("No Fix");
			obuGpsFixStatus.setTextColor(Color.RED);
			break;
		case 1:
			obuGpsFixStatus.setText("Satellite Only Fix");
			obuGpsFixStatus.setTextColor(Color.YELLOW);
			break;
		case 2:
			obuGpsFixStatus.setText("DGPS Fix");
			obuGpsFixStatus.setTextColor(Color.WHITE);
			break;
		default:
			obuGpsFixStatus.setText("Unknown");
			obuGpsFixStatus.setTextColor(Color.RED);
			break;
		}

		if (rModel.obuDiagnostics.getHeading() < 0
				|| rModel.obuDiagnostics.getSpeed() < 0)
			((TextView) findViewById(R.id.diag_txtObuGpsHeadingSpeed))
					.setText("Unavailable");
		else
			((TextView) findViewById(R.id.diag_txtObuGpsHeadingSpeed))
					.setText(String.format("%.1f mph @ %.0f \u00B0",
							rModel.obuDiagnostics.getSpeed() * 2.2369,
							rModel.obuDiagnostics.getHeading()));

		((TextView) findViewById(R.id.diag_txtObuGpsActiveSatellites))
				.setText(String.format("%d active",
						rModel.obuDiagnostics.getSatCount()));

		/* DSRC Radio Application */
		{
			StringBuilder sb = new StringBuilder();
			sb.append(rModel.obuDiagnostics.getVehicleId());
			switch (rModel.obuDiagnostics.getVehicleIdLock()) {
			case 1:
				sb.append(" (Temporary Lock)");
				break;
			case 2:
				sb.append(" (Permanent Lock)");
				break;
			}
			((TextView) findViewById(R.id.diag_txtObuAppVehicleId)).setText(sb
					.toString());

		}

		/* Oncoming Data */

		((TextView) findViewById(R.id.diag_txtObuOncomingActiveTimId))
				.setText(rModel.obuDiagnostics.getActiveTimId().equals("") ? "Unavailable"
						: rModel.obuDiagnostics.getActiveTimId());
		((TextView) findViewById(R.id.diag_txtObuOncomingEvaCount))
				.setText(String.valueOf(rModel.obuDiagnostics.getEvaCount()));
		((TextView) findViewById(R.id.diag_txtObuOncomingRawLaneLocation))
				.setText(rModel.obuDiagnostics.getRawLaneLocation() != -1000.0 ? String
						.valueOf(rModel.obuDiagnostics.getRawLaneLocation())
						: "Not in a Lane");

		/* Other */
		((TextView) findViewById(R.id.diag_txtObuVersion))
				.setText("OBU Version: " + rModel.obuDiagnostics.getVersion());

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
		}
	}
}
