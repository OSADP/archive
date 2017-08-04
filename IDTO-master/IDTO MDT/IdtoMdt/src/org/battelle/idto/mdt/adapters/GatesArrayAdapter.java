package org.battelle.idto.mdt.adapters;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.TimeZone;

import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.Step;
import org.battelle.idto.mdt.R;
import org.battelle.idto.mdt.models.Gate;

import android.content.Context;
import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;

public class GatesArrayAdapter extends ArrayAdapter<Gate> {

	private ArrayList<Gate> mArrayList;

	private Context mContext;
	
	private int mSelectedIndex;
	
	public GatesArrayAdapter(Context context, int textViewResourceId, ArrayList<Gate> objects) {
		super(context, textViewResourceId, objects);
		mArrayList = objects;
		mContext = context;
		mSelectedIndex = -1;
	}
	
	public void setSelectedIndex(int i)
	{
		mSelectedIndex = i;
		notifyDataSetChanged();
	}

	
	@Override
	public View getView(int position, View convertView, ViewGroup parent){
		// assign the view we are converting to a local variable
		View v = convertView;

		// first check to see if the view is null. if so, we have to inflate it.
		// to inflate it basically means to render, or show, the view.
		if (v == null) {
			LayoutInflater inflater = (LayoutInflater) getContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			v = inflater.inflate(R.layout.gate_list, null);
		}

		/*
		 * Recall that the variable position is sent in as an argument to this method.
		 * The variable simply refers to the position of the current object in the list. (The ArrayAdapter
		 * iterates through the list we sent it)
		 * 
		 * Therefore, i refers to the current Item object.
		 */
		Gate i = mArrayList.get(position);

		LinearLayout linearLayoutGateView = (LinearLayout)v.findViewById(R.id.linearLayoutGateView);
		TextView txtGateName = (TextView)v.findViewById(R.id.txtGateName);
		//Button btnGateName = (Button)v.findViewById(R.id.btnGateName);
		
		if (i != null) {
			if(position == mSelectedIndex)
			{
				//highlight
				//linearLayoutGateView.setBackgroundColor(mContext.getResources().getColor(R.color.idto_dkpurple));
				linearLayoutGateView.setBackground(mContext.getResources().getDrawable(R.drawable.gradient_row_selected));
			}
			else
			{
				linearLayoutGateView.setBackgroundColor(mContext.getResources().getColor(android.R.color.transparent));
			}
			txtGateName.setText(i.getGateName());
			//String firstChar = i.getGateName().substring(0, 1);
			//btnGateName.setText(firstChar);
		}

		// the view must be returned to our activity
		return v;

	}
}
