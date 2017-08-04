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

package org.battelle.inczone.situationaldisplay;

import org.battelle.inczone.situationaldisplay.obu.ObuBluetoothState;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.Color;
import android.os.Bundle;
import android.support.v4.content.LocalBroadcastManager;
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

		/* Other */
		((TextView) findViewById(R.id.diag_txtObuVersion))
				.setText("OBU Version: " + rModel.obuDiagnostics.getVersion());

	}
}
