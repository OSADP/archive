package org.battelle.inflo.infloui;

import org.battelle.inflo.infloui.alerts.AlertColorUtilities;
import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.alerts.QWarnAlertListAdapter;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.support.v4.content.LocalBroadcastManager;
import android.view.View;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ListView;
import android.widget.TextView;

public class QWarnAlertsActivity extends Activity {

	ListView qWarnList;
	QWarnAlert[] alerts;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
				WindowManager.LayoutParams.FLAG_FULLSCREEN);

		setContentView(R.layout.activity_qwarn_alerts);

		qWarnList = (ListView) findViewById(R.id.qw_alerts_listAlerts);
		qWarnList.setOnItemClickListener(new OnItemClickListener() {

			@Override
			public void onItemClick(AdapterView<?> parent, View view,
					int position, long id) {
				displayAlert(alerts[position]);
			}
		});
	}

	@Override
	protected void onResume() {
		super.onResume();

		alerts = new QWarnAlert[0];

		QWarnAlertListAdapter listAdapter = new QWarnAlertListAdapter(this,
				alerts);
		qWarnList.setAdapter(listAdapter);

		displayAlert(alerts.length > 0 ? alerts[0] : null);

		LocalBroadcastManager.getInstance(this).registerReceiver(
				rModelReceiver,
				new IntentFilter(ApplicationMonitorService.ACTION_INVALIDATE));

		ApplicationMonitorService.requestUpdate(this);
	}

	@Override
	protected void onPause() {
		super.onPause();

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rModelReceiver);
	}

	private void displayAlert(QWarnAlert alert) {

		if (alert == null) {
			((TextView) findViewById(R.id.qw_alerts_txtDistToBOQ)).setText("");
			((TextView) findViewById(R.id.qw_alerts_txtTimeReceived))
					.setText("");
			((TextView) findViewById(R.id.qw_alerts_txtLength)).setText("");
			((TextView) findViewById(R.id.qw_alerts_txtTimeToFOQ)).setText("");
			((TextView) findViewById(R.id.qw_alerts_txtRecommendedAction))
					.setText("");
		} else {
			((TextView) findViewById(R.id.qw_alerts_txtDistToBOQ))
					.setText(String.format("%.1f", alert.getDistanceToBOQ()));
			((TextView) findViewById(R.id.qw_alerts_txtDistToBOQ))
					.setTextColor(AlertColorUtilities.getColor(alert));

			((TextView) findViewById(R.id.qw_alerts_txtTimeReceived))
					.setText(alert.getReceivedTime().toLocalDateTime()
							.toString("d/M/yyyy h:mm:ss a"));
			((TextView) findViewById(R.id.qw_alerts_txtLength)).setText(String
					.format("%.1f mi", alert.getLengthOfQ()));
			((TextView) findViewById(R.id.qw_alerts_txtTimeToFOQ))
					.setText(String.format("%d min", alert.getTimeToFOQ()));

			((TextView) findViewById(R.id.qw_alerts_txtRecommendedAction))
					.setText(alert.getRecommendedAction());
		}
	}

	BroadcastReceiver rModelReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {

			alerts = ((ApplicationModel) intent.getExtras().getParcelable(
					ApplicationMonitorService.EXTRA_MODEL)).alertQWarnAlertList
					.toArray(alerts);

			QWarnAlertListAdapter listAdapter = new QWarnAlertListAdapter(
					QWarnAlertsActivity.this, alerts);
			qWarnList.setAdapter(listAdapter);
		}
	};
}
