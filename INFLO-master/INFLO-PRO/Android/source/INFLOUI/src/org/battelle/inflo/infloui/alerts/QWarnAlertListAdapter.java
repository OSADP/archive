package org.battelle.inflo.infloui.alerts;

import org.battelle.inflo.infloui.R;

import android.app.Activity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class QWarnAlertListAdapter extends ArrayAdapter<QWarnAlert> {

	Activity mContext;
	int layoutResourceId;
	QWarnAlert data[] = null;

	public QWarnAlertListAdapter(Activity context, QWarnAlert[] objects) {
		super(context, R.layout.listitem_qwarn_alert, objects);

		this.layoutResourceId = R.layout.listitem_qwarn_alert;
		this.mContext = context;
		this.data = objects;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent) {

		if (convertView == null) {
			LayoutInflater inflater = mContext.getLayoutInflater();
			convertView = inflater.inflate(layoutResourceId, parent, false);
		}

		QWarnAlert alert = data[position];

		TextView txtDist = (TextView) convertView.findViewById(R.id.listitem_qwarn_alert_txtDist);
		txtDist.setText(String.format("%.1f", alert.getDistanceToBOQ()));
		txtDist.setTextColor(AlertColorUtilities.getColor(alert));

		TextView txtTime = (TextView) convertView.findViewById(R.id.listitem_txtTimestamp);
		txtTime.setText(alert.getReceivedTime().toLocalDateTime().toString("d/m/yyyy h:mm:ss a"));

		TextView txtLength = (TextView) convertView
				.findViewById(R.id.listitem_qwarn_alert_txtLength);
		txtLength.setText(String.format("%.1f mi (%d min)", alert.getLengthOfQ(),
				alert.getTimeToFOQ()));

		return convertView;
	}
}
