package org.battelle.idto.mdt.adapters;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.TimeZone;

import org.battelle.idto.mdt.R;
import org.battelle.idto.mdt.models.GateStopTime;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class GateStopTimesArrayAdapter extends ArrayAdapter<GateStopTime> {

	private ArrayList<GateStopTime> mArrayList;

	private Context mContext;
	
	public GateStopTimesArrayAdapter(Context context, int textViewResourceId, ArrayList<GateStopTime> objects) {
		super(context, textViewResourceId, objects);
		mArrayList = objects;
		mContext = context;
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent){
		// assign the view we are converting to a local variable
		View v = convertView;

		// first check to see if the view is null. if so, we have to inflate it.
		// to inflate it basically means to render, or show, the view.
		if (v == null) {
			LayoutInflater inflater = (LayoutInflater) getContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			v = inflater.inflate(R.layout.list_idto_stop_times, null);
		}

		/*
		 * Recall that the variable position is sent in as an argument to this method.
		 * The variable simply refers to the position of the current object in the list. (The ArrayAdapter
		 * iterates through the list we sent it)
		 * 
		 * Therefore, i refers to the current Item object.
		 */
		GateStopTime i = mArrayList.get(position);

		
		
		if (i != null) {

			TextView textTransferTime = (TextView) v.findViewById(R.id.textStopTime);
			TextView textTransferHeadsign = (TextView) v.findViewById(R.id.textStopTimeHeadsign);
			
			if (textTransferTime != null){

				Date date = new Date(i.getStopTime().getTime().longValue()*1000l);
				
				SimpleDateFormat formatter = new SimpleDateFormat("hh:mm a");
				
				formatter.setTimeZone(TimeZone.getDefault());
				
				textTransferTime.setText(formatter.format(date));

			}
			if (textTransferHeadsign != null){
				textTransferHeadsign.setText(i.getStopTime().getTrip().getTripHeadsign());
			}
		}

		// the view must be returned to our activity
		return v;

	}
}
