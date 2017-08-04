package org.battelle.inflo.infloui.alerts;

import org.battelle.inflo.infloui.R;

import android.app.Activity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class SpdHarmAlertListAdapter extends ArrayAdapter<SpdHarmAlert> {

	Activity mContext;
	int layoutResourceId;
	SpdHarmAlert data[] = null;

	public SpdHarmAlertListAdapter(Activity context, SpdHarmAlert[] objects) {
		super(context, R.layout.listitem_spdharm_alert, objects);

		this.layoutResourceId = R.layout.listitem_spdharm_alert;
		this.mContext = context;
		this.data = objects;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent) {

		if (convertView == null) {
			LayoutInflater inflater = mContext.getLayoutInflater();
			convertView = inflater.inflate(layoutResourceId, parent, false);
		}

		SpdHarmAlert alert = data[position];

		TextView txtSpd = (TextView) convertView.findViewById(R.id.listitem_spdharm_alert_txtSpd);
		txtSpd.setText(String.format("%.1f", alert.getSpeed()));
		txtSpd.setTextColor(AlertColorUtilities.getColor(alert));

		TextView txtTime = (TextView) convertView.findViewById(R.id.listitem_txtTimestamp);
		txtTime.setText(alert.getReceivedTime().toLocalDateTime().toString("d/m/yyyy h:mm:ss a"));

		return convertView;
	}
}
