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

import android.content.Context;
import android.graphics.Color;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

public class IdtoTripArrayAdapter extends ArrayAdapter<IdtoTrip> {

	private ArrayList<IdtoTrip> mArrayList;

	private Context mContext;
	
	public IdtoTripArrayAdapter(Context context, int textViewResourceId, ArrayList<IdtoTrip> objects) {
		super(context, textViewResourceId, objects);
		mArrayList = objects;
		mContext = context;
	}
	
	@Override
	public View getView(int position, View convertView, ViewGroup parent){

		SimpleDateFormat inputFormatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH);
		inputFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));
		
		
		
		// assign the view we are converting to a local variable
		View v = convertView;

		// first check to see if the view is null. if so, we have to inflate it.
		// to inflate it basically means to render, or show, the view.
		if (v == null) {
			LayoutInflater inflater = (LayoutInflater) getContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			v = inflater.inflate(R.layout.list_idto_trip, null);
		}

		/*
		 * Recall that the variable position is sent in as an argument to this method.
		 * The variable simply refers to the position of the current object in the list. (The ArrayAdapter
		 * iterates through the list we sent it)
		 * 
		 * Therefore, i refers to the current Item object.
		 */
		IdtoTrip i = mArrayList.get(position);

		
		
		if (i != null) {

			Step lastStep = i.getSteps().get(i.getSteps().size()-1);
			
			// This is how you obtain a reference to the TextViews.
			// These TextViews are created in the XML files we defined.

			//LinearLayout cardBG = (LinearLayout)v.findViewById(R.id.cardBackgroundLinearLayout);
			TextView textTransferTime = (TextView) v.findViewById(R.id.textTransferTime);
			TextView textTransferHeadsign = (TextView) v.findViewById(R.id.textTransferHeadsign);

			ImageView tripStatusImageView = (ImageView)v.findViewById(R.id.idto_trip_view_status_image);
			
			String tripStatusString = "Unknown";
			
			
			//textTransferTime.setBackgroundColor(Color.WHITE);
			//textTransferHeadsign.setBackgroundColor(Color.WHITE);
			
			if(i.getTConnectStatus() == 0){ //New
				tripStatusString = "New";
			}
			else if(i.getTConnectStatus() == 1){ //In Progress
				tripStatusString = "In Progress";
			}
			else if(i.getTConnectStatus()==2){ //Monitored
				tripStatusString = "Monitored";
			}
			else if(i.getTConnectStatus() ==3){ //Requested
				tripStatusImageView.setImageResource(R.drawable.status_requested);
				tripStatusString = "Requested";
			}
			else if(i.getTConnectStatus()==4){ //Auto Rejected
				tripStatusImageView.setImageResource(R.drawable.status_decline);
				tripStatusString = "Auto Rejected";
			}
			else if(i.getTConnectStatus()==5){ //Rejected
				tripStatusImageView.setImageResource(R.drawable.status_decline);
				tripStatusString = "Rejected";
			}
			else if(i.getTConnectStatus()==6){ //Accepted
				tripStatusImageView.setImageResource(R.drawable.status_accepted);
				tripStatusString = "Accepted";
			}
			else if(i.getTConnectStatus()==7){ //Completed
				tripStatusString = "Completed";
			}
			else if(i.getTConnectStatus()==8){ //Deleted
				tripStatusString = "Deleted";
			}
			else{
				tripStatusString = "Unknown";
			}
			
			// check to see if each individual textview is null.
			// if not, assign some text!
			if (textTransferTime != null){
				Date date;
				
				try {
					date = inputFormatter.parse(lastStep.getStartDate());
					SimpleDateFormat formatter = new SimpleDateFormat("h:mm a");
					formatter.setTimeZone(TimeZone.getDefault());
					
					textTransferTime.setText(formatter.format(date));
				} catch (ParseException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				
				
			}
			if (textTransferHeadsign != null){
				textTransferHeadsign.setText(i.getDestination());
			}
		}

		// the view must be returned to our activity
		return v;

	}
}
