package org.battelle.inczone.inczoneui;

import java.util.ArrayList;

import org.battelle.inczone.inczoneui.ntrip.NTripState;
import org.battelle.inczone.inczoneui.obu.ObuBluetoothState;
import org.battelle.inczone.inczoneui.odbii.VehicleDiagnosticsState;
import org.battelle.inczone.inczoneui.ui.SliderBar;
import org.joda.time.DateTime;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.support.v4.content.LocalBroadcastManager;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

public class DashboardActivity extends Activity {

	Handler rClockUpdateHandler = new Handler();
	private SharedPreferences rSettings;

	boolean continueFlashing = false;
	Handler flashHandler = new Handler();
	boolean flashSpeedAlertText = false;
	boolean flashClosedLaneAlertText = false;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_dashboard);

		rSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);
	}

	@Override
	protected void onResume() {
		super.onResume();
		rClockUpdateHandler.post(updateClock);
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rInvalidationReceiver,
				new IntentFilter(ApplicationMonitorService.ACTION_INVALIDATE));

		flashHandler.post(updateFlashing);
		continueFlashing = true;

		ApplicationMonitorService.requestUpdate(this);
	}

	@Override
	protected void onPause() {
		super.onPause();

		continueFlashing = false;

		rClockUpdateHandler.removeCallbacks(updateClock);
		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rInvalidationReceiver);
	}

	/**
	 * Causes the activity to redraw itself with the new model data
	 * 
	 * @param rModel
	 */
	private void invalidate(ApplicationModel rModel) {

		ArrayList<String> warnings = new ArrayList<String>();
		ArrayList<String> errors = new ArrayList<String>();

		if (rModel.obuBluetoothState != ObuBluetoothState.Connected) {
			errors.add("DSRC Radio Disconnected");
		} else {
			if (rSettings
					.getBoolean(
							getResources().getString(
									R.string.setting_ntripEnabled_key), true)) {
				if (rModel.ntripState == NTripState.Waiting_For_Bluetooth
						|| rModel.ntripState == NTripState.Connecting
						|| rModel.ntripState == NTripState.Disconnected) {
					errors.add("NTRIP "
							+ rModel.ntripState.toString().replace('_', ' '));
				} else if (rModel.ntripState == NTripState.Connected
						|| rModel.ntripState == NTripState.Waiting_For_Data) {
					// warnings.add("NTRIP: Waiting for data");
				}
				if (rModel.obuDiagnostics.getGpsFix() == 1)
					warnings.add("DSRC Radio: No DGPS Fix");
			}

			if (rModel.obuDiagnostics.getGpsFix() == 0)
				errors.add("DSRC Radio: No GPS Fix");
		}
		if (rModel.vehState != VehicleDiagnosticsState.Connected
				&& rModel.vehState != VehicleDiagnosticsState.Disabled) {
			errors.add("Vehicle Disconnected");
		}

		double rawLanePosition = rModel.obuDiagnostics.getRawLaneLocation();

		if (rawLanePosition > -1000.0) {
			int rawLaneNumber = (int) Math.round(rawLanePosition);
			((SliderBar) findViewById(R.id.dashboard_sliderPositionWithinLane))
					.setSliderShown(true);
			((SliderBar) findViewById(R.id.dashboard_sliderPositionWithinLane))
					.setSliderPosition((rawLaneNumber - rawLanePosition) + 0.5);
			((TextView) findViewById(R.id.dashboard_txtLaneNumber))
					.setText(String.valueOf(rawLaneNumber));
		} else {
			((SliderBar) findViewById(R.id.dashboard_sliderPositionWithinLane))
					.setSliderShown(false);
			((SliderBar) findViewById(R.id.dashboard_sliderPositionWithinLane))
					.setSliderPosition(0.5);
			((TextView) findViewById(R.id.dashboard_txtLaneNumber)).setText("");
		}

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

		/*
		 * Closed Lane Alerts
		 */
		ImageView imgStdLaneChange = ((ImageView) findViewById(R.id.dashboard_imgStdLaneChange));
		ImageView imgLrgLaneChange = ((ImageView) findViewById(R.id.dashboard_imgLrgLaneChange));
		switch (rModel.alertInfo.getLaneChangeSignType()) {
		case 0:
			if (rModel.alertInfo.isIsOnLeft()) {
				switch (rModel.alertInfo.getLaneCount()) {
				case 0:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_left_shoulder_closed_ahead);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_left_shoulder_closed_ahead);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				case 1:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_left_lane_closed_ahead);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_left_lane_closed_ahead);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				default:
					break;
				}
			} else {
				switch (rModel.alertInfo.getLaneCount()) {
				case 0:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_right_shoulder_closed_ahead);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_right_shoulder_closed_ahead);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				case 1:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_right_lane_closed_ahead);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_right_lane_closed_ahead);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				default:
					break;
				}
			}
			break;
		case 1:
			if (rModel.alertInfo.isIsOnLeft()) {
				switch (rModel.alertInfo.getLaneCount()) {
				case 0:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_left_shoulder_closed);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_left_shoulder_closed);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				case 1:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_left_lane_closed);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_left_lane_closed);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				default:
					break;
				}
			} else {
				switch (rModel.alertInfo.getLaneCount()) {
				case 0:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_right_shoulder_closed);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_right_shoulder_closed);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				case 1:
					imgStdLaneChange
							.setImageResource(R.drawable.closed_right_lane_closed);
					imgStdLaneChange.setVisibility(View.VISIBLE);
					imgLrgLaneChange
							.setImageResource(R.drawable.closed_right_lane_closed);
					imgLrgLaneChange.setVisibility(View.VISIBLE);
					break;
				default:
					break;
				}
			}
			break;
		case 2:
			if (rModel.alertInfo.isIsOnLeft()) {
				imgStdLaneChange
						.setImageResource(R.drawable.closed_merge_right_symbol);
				imgStdLaneChange.setVisibility(View.VISIBLE);
				imgLrgLaneChange
						.setImageResource(R.drawable.closed_merge_right_symbol);
				imgLrgLaneChange.setVisibility(View.VISIBLE);
			} else {
				imgStdLaneChange
						.setImageResource(R.drawable.closed_merge_left_symbol);
				imgStdLaneChange.setVisibility(View.VISIBLE);
				imgLrgLaneChange
						.setImageResource(R.drawable.closed_merge_left_symbol);
				imgLrgLaneChange.setVisibility(View.VISIBLE);
			}
			break;
		case 3:
			if (rModel.alertInfo.isIsOnLeft()) {
				imgStdLaneChange
						.setImageResource(R.drawable.closed_merge_right_symbol);
				imgStdLaneChange.setVisibility(View.VISIBLE);
				imgLrgLaneChange
						.setImageResource(R.drawable.closed_merge_right_symbol);
				imgLrgLaneChange.setVisibility(View.VISIBLE);
			} else {
				imgStdLaneChange
						.setImageResource(R.drawable.closed_merge_left_symbol);
				imgStdLaneChange.setVisibility(View.VISIBLE);
				imgLrgLaneChange
						.setImageResource(R.drawable.closed_merge_left_symbol);
				imgLrgLaneChange.setVisibility(View.VISIBLE);
			}
			break;
		case 4:
			imgStdLaneChange.setImageResource(R.drawable.stop);
			imgStdLaneChange.setVisibility(View.VISIBLE);
			imgLrgLaneChange.setImageResource(R.drawable.stop);
			imgLrgLaneChange.setVisibility(View.VISIBLE);
			break;
		default:
			imgStdLaneChange.setVisibility(View.GONE);
			imgLrgLaneChange.setVisibility(View.GONE);
			break;
		}

		switch (rModel.alertInfo.getLaneChangeThreatLevel()) {
		case 1:
			((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
					.setText("ALERT!");
			((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
					.setText("ALERT!");
			flashClosedLaneAlertText = true;
			break;
		case 2:
		case 3:
			((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
					.setText("WARNING!");
			((TextView) findViewById(R.id.dashboard_txtLrgLaneChangeAlertText))
					.setText("WARNING!");
			flashClosedLaneAlertText = true;
			break;
		default:
			flashClosedLaneAlertText = false;
			((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
					.setVisibility(View.GONE);
			((TextView) findViewById(R.id.dashboard_txtLrgLaneChangeAlertText))
					.setVisibility(View.GONE);
			break;
		}

		if (rModel.alertInfo.getLaneChangeThreatLevel() >= 2) {
			((LinearLayout) findViewById(R.id.dashboard_layoutStandard))
					.setVisibility(View.GONE);
			((LinearLayout) findViewById(R.id.dashboard_layoutLarge))
					.setVisibility(View.VISIBLE);
		} else {

			((LinearLayout) findViewById(R.id.dashboard_layoutLarge))
					.setVisibility(View.GONE);
			((LinearLayout) findViewById(R.id.dashboard_layoutStandard))
					.setVisibility(View.VISIBLE);
		}

		/*
		 * Speed Alerts
		 */
		ImageView imgStdSpeed = ((ImageView) findViewById(R.id.dashboard_imgStdSpeed));
		ImageView imgLrgSpeed = ((ImageView) findViewById(R.id.dashboard_imgLrgSpeed));
		switch (rModel.alertInfo.getSpeedSignType()) {
		case 0:
			imgStdSpeed
					.setImageResource(rModel.alertInfo.getSpeed() == 25 ? R.drawable.speed_ahead_twentyfive
							: R.drawable.speed_ahead_fortyfive);
			imgStdSpeed.setVisibility(View.VISIBLE);
			imgLrgSpeed
					.setImageResource(rModel.alertInfo.getSpeed() == 25 ? R.drawable.speed_ahead_twentyfive
							: R.drawable.speed_ahead_fortyfive);
			imgLrgSpeed.setVisibility(View.VISIBLE);
			break;
		case 1:
			imgStdSpeed
					.setImageResource(rModel.alertInfo.getSpeed() == 25 ? R.drawable.speed_twentyfive
							: R.drawable.speed_fortyfive);
			imgStdSpeed.setVisibility(View.VISIBLE);
			imgLrgSpeed
					.setImageResource(rModel.alertInfo.getSpeed() == 25 ? R.drawable.speed_twentyfive
							: R.drawable.speed_fortyfive);
			imgLrgSpeed.setVisibility(View.VISIBLE);
			break;
		default:
			imgStdSpeed.setVisibility(View.GONE);
			imgLrgSpeed.setVisibility(View.GONE);
			break;
		}

		switch (rModel.alertInfo.getSpeedThreatLevel()) {
		case 1:
			((TextView) findViewById(R.id.dashboard_txtStdSpeedAlertText))
					.setText("ALERT!");
			flashSpeedAlertText = true;
			break;
		case 2:
			((TextView) findViewById(R.id.dashboard_txtStdSpeedAlertText))
					.setText("WARNING!");
			flashSpeedAlertText = true;
			break;
		default:
			flashSpeedAlertText = false;
			((TextView) findViewById(R.id.dashboard_txtStdSpeedAlertText))
					.setVisibility(View.GONE);
			break;
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

	Runnable updateFlashing = new Runnable() {

		boolean state = false;

		@Override
		public void run() {

			if (!continueFlashing)
				return;

			if (state) {

				if (flashClosedLaneAlertText) {
					((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
							.setVisibility(View.VISIBLE);
					((TextView) findViewById(R.id.dashboard_txtLrgLaneChangeAlertText))
							.setVisibility(View.VISIBLE);
				}

				if (flashSpeedAlertText) {
					((TextView) findViewById(R.id.dashboard_txtStdSpeedAlertText))
							.setVisibility(View.VISIBLE);
				}

				rClockUpdateHandler.postDelayed(this, 500);
			} else {

				((TextView) findViewById(R.id.dashboard_txtStdSpeedAlertText))
						.setVisibility(View.GONE);
				((TextView) findViewById(R.id.dashboard_txtStdLaneChangeAlertText))
						.setVisibility(View.GONE);
				((TextView) findViewById(R.id.dashboard_txtLrgLaneChangeAlertText))
						.setVisibility(View.GONE);

				rClockUpdateHandler.postDelayed(this, 500);
			}

			state = !state;
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
