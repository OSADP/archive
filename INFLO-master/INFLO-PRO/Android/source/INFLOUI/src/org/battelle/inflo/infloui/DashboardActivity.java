package org.battelle.inflo.infloui;

import java.util.ArrayList;

import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.cloud.TmeCloudEndpointStatistics;
import org.battelle.inflo.infloui.cloud.TmeCloudState;
import org.battelle.inflo.infloui.obu.ObuBluetoothState;
import org.battelle.inflo.infloui.odbii.VehicleDiagnosticsState;
import org.battelle.inflo.infloui.ui.LEDDisplay;
import org.joda.time.DateTime;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.os.Handler;
import android.support.v4.content.LocalBroadcastManager;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;

public class DashboardActivity extends Activity {

	Handler rClockUpdateHandler = new Handler();

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_dashboard);
	}

	@Override
	protected void onResume() {
		super.onResume();
		rClockUpdateHandler.post(updateClock);
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rInvalidationReceiver,
				new IntentFilter(ApplicationMonitorService.ACTION_INVALIDATE));

		ApplicationMonitorService.requestUpdate(this);
	}

	@Override
	protected void onPause() {
		super.onPause();
		rClockUpdateHandler.removeCallbacks(updateClock);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rInvalidationReceiver);
	}

	public void onQWarnAlertClick(View v) {
		// startActivity(new Intent(this, QWarnAlertsActivity.class));
	}

	public void onSpdHarmAlertClick(View v) {

	}

	/**
	 * Causes the activity to redraw itself with the new model data
	 * 
	 * @param rModel
	 */
	private void invalidate(ApplicationModel rModel) {

		if (rModel.alertSpdHarm == null) {
			((LinearLayout) findViewById(R.id.dashboard_layoutRecSpeed))
					.setVisibility(View.GONE);
		} else {
			((LinearLayout) findViewById(R.id.dashboard_layoutRecSpeed))
					.setVisibility(View.VISIBLE);
			((TextView) findViewById(R.id.dashboard_txtRecommendedSpeed))
					.setText(rModel.alertSpdHarm.getSpeed() == -1 ? "" : String
							.valueOf(rModel.alertSpdHarm.getSpeed()));
			((TextView) findViewById(R.id.dashboard_txtSpdHarmText))
					.setText(rModel.alertSpdHarm.getJustificationText());
		}

		QWarnAlert qWarnAlert = rModel.alertQWarn;
		if (qWarnAlert == null || !qWarnAlert.isQueueAheadAlert()) {
			((LinearLayout) findViewById(R.id.dashboard_layoutBoqInfo))
					.setVisibility(View.GONE);
		} else {
			((LinearLayout) findViewById(R.id.dashboard_layoutBoqInfo))
					.setVisibility(View.VISIBLE);

			((TextView) findViewById(R.id.dashboard_txtDistBOQ))
					.setText(rModel.alertQWarn.getDistanceToBOQ() < 0 ? ""
							: String.format("%.1f",
									rModel.alertQWarn.getDistanceToBOQ()));
		}

		if (qWarnAlert == null || !qWarnAlert.isInQueueAlert()) {
			((LinearLayout) findViewById(R.id.dashboard_layoutFoqInfo))
					.setVisibility(View.GONE);
		} else {
			((LinearLayout) findViewById(R.id.dashboard_layoutFoqInfo))
					.setVisibility(View.VISIBLE);

			((TextView) findViewById(R.id.dashboard_txtDistFOQ))
					.setText(rModel.alertQWarn.getDistanceToFOQ() < 0 ? ""
							: String.format("%.1f",
									rModel.alertQWarn.getDistanceToFOQ()));

			((TextView) findViewById(R.id.dashboard_txtTimeToFOQ))
					.setText(rModel.alertQWarn.getTimeToFOQ() < 0 ? "" : String
							.valueOf(rModel.alertQWarn.getTimeToFOQ()));
		}

		if (qWarnAlert == null) {
			((TextView) findViewById(R.id.dashboard_txtQWarnText))
					.setVisibility(View.GONE);
		} else {
			((TextView) findViewById(R.id.dashboard_txtQWarnText))
					.setVisibility(View.VISIBLE);
			((TextView) findViewById(R.id.dashboard_txtQWarnText))
					.setText(qWarnAlert.getRecommendedAction());
		}

		ArrayList<String> warnings = new ArrayList<String>();
		ArrayList<String> errors = new ArrayList<String>();

		for (TmeCloudEndpointStatistics cloudStat : rModel.tmeCloudEndpointStatistics) {
			if (cloudStat.getState() == TmeCloudState.Unavailable) {
				errors.add("TME Unavailable");
				break;
			}
		}

		if (rModel.obuBluetoothState != ObuBluetoothState.Connected) {
			errors.add("OBU Disconnected");
		} else {
			if (!rModel.obuDiagnostics.isGpsFixed())
				errors.add("OBU: No GPS Signal");
			/*
			 * if (rModel.obuDiagnostics.getBatteryLevel() < 25)
			 * errors.add(String.format("OBU: Low Battery: %.0f %%",
			 * rModel.obuDiagnostics.getBatteryLevel()));
			 */
		}
		if (rModel.vehState != VehicleDiagnosticsState.Connected
				&& rModel.vehState != VehicleDiagnosticsState.Disabled) {
			errors.add("Vehicle Disconnected");
		}

		((LEDDisplay) findViewById(R.id.dashboard_ledV2VQueued))
				.setActive(rModel.obuDiagnostics.isQueued()
						&& rModel.obuBluetoothState == ObuBluetoothState.Connected);

		TextView warningFooter = (TextView) findViewById(R.id.dashboard_txtWarningFooter);
		if (warnings.size() > 0) {

			StringBuilder sb = new StringBuilder();
			for (String s : warnings)
				sb.append(s + '\n');

			warningFooter.setText(sb.toString().substring(0, sb.length() - 1));
			warningFooter.setVisibility(View.VISIBLE);
		} else {
			warningFooter.setVisibility(View.GONE);
		}

		TextView errorFooter = (TextView) findViewById(R.id.dashboard_txtErrorFooter);
		if (errors.size() > 0) {

			StringBuilder sb = new StringBuilder();
			for (String s : errors)
				sb.append(s + '\n');

			errorFooter.setText(sb.toString().substring(0, sb.length() - 1));
			errorFooter.setVisibility(View.VISIBLE);
		} else {
			errorFooter.setVisibility(View.GONE);
		}
	}

	Runnable updateClock = new Runnable() {

		@Override
		public void run() {
			TextView time = ((TextView) findViewById(R.id.dashboard_txtTime));
			time.setText(DateTime.now().toString("h:mm"));

			rClockUpdateHandler.postDelayed(this, 5000);
		}
	};

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

}
