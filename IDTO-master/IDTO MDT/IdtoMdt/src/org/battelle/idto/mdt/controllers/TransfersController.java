package org.battelle.idto.mdt.controllers;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Timer;
import java.util.TimerTask;

import org.battelle.idto.mdt.adapters.IdtoTripArrayAdapter;
import org.battelle.idto.mdt.preferences.Constants;
import org.battelle.idto.mdt.utils.IdtoTripComparator;
import org.battelle.idto.ws.comms.IdtoTripType;
import org.battelle.idto.ws.comms.IdtoWsInterface;
import org.battelle.idto.ws.comms.IdtoWsManager;
import org.battelle.idto.ws.comms.IdtoWsResponse;
import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.TConnectStatus;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.analytics.tracking.android.GoogleAnalytics;
import com.google.analytics.tracking.android.MapBuilder;
import com.google.analytics.tracking.android.Tracker;

import android.app.Activity;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.widget.ArrayAdapter;

public class TransfersController {

	private static final Logger mLogger = LoggerFactory.getLogger(TransfersController.class); 
	
	private Tracker mGoogleAnalyticsTracker;
	
	protected ArrayList<IdtoTrip> mTransfers;
	protected IdtoTripArrayAdapter mArrayAdapter;
	private Timer mTimer;
	private Activity mContext;
	private IdtoWsInterface mIdtoWsManager;
	
	public TransfersController(Activity context)
	{
		mContext = context;
		mIdtoWsManager = new IdtoWsManager(Constants.API_URL, context);
		mTransfers = new ArrayList<IdtoTrip>();
		mArrayAdapter = new IdtoTripArrayAdapter(
                context,
                android.R.layout.list_content,
                getTransfers());
		
		mTimer = new Timer();
		mTimer.scheduleAtFixedRate(new TimerTask() {
			  @Override
			  public void run() {
				  mContext.runOnUiThread(new Runnable() {
					     @Override
						public void run() {
					    	 updateArrayData();
					    }
					});
				  
			  }
			}, 1*1000, 10*1000);
		
		
		mGoogleAnalyticsTracker = GoogleAnalytics.getInstance(context).getTracker("UA-43128178-3");
	}
	
	public void refresh()
	{
		updateArrayData();
	}
	
	public ArrayList<IdtoTrip> getTransfers()
	{
		return mTransfers;
	}
	
	public ArrayAdapter<IdtoTrip> getTransfersArrayAdapter()
	{
		return mArrayAdapter;
	}
	
	protected void updateArrayData()
	{
		mGoogleAnalyticsTracker.send(MapBuilder
			    .createEvent("TransferController", "UpdateData", "updateArrayData", null)
			    .build()
			);
		
	    SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(mContext);
	    int userId = prefs.getInt("UserID", -1);
	    
	    
		mIdtoWsManager.getTrips(userId, IdtoTripType.INPROGRESS, mGetInProgressTripResponse);
	}

	private IdtoWsResponse<ArrayList<IdtoTrip>> mGetInProgressTripResponse = new IdtoWsResponse<ArrayList<IdtoTrip>>() {
		
		@Override
		public void onIdtoWsResponse(ArrayList<IdtoTrip> response) {
			
			for(IdtoTrip oldTrip :mTransfers)
			{
				boolean bFound = false;
				for(IdtoTrip newTrip : response)
				{
					if(oldTrip.getId() == newTrip.getId())
						bFound = true;
						
				}
				
				if(!bFound)
					mTransfers.remove(oldTrip);
				else
					mIdtoWsManager.getTConnectStatus(oldTrip.getId(), mGetTConnectStatusResponse);
			}
			
			for(IdtoTrip newTrip :response)
			{
				boolean bFound = false;
				for(IdtoTrip oldTrip : mTransfers)
				{
					if(oldTrip.getId() == newTrip.getId())
						bFound = true;
						
				}
				
				if(!bFound){
					mTransfers.add(newTrip);
					mIdtoWsManager.getTConnectStatus(newTrip.getId(), mGetTConnectStatusResponse);
				}
			}
			
			Collections.sort(mTransfers, new IdtoTripComparator());
			
			mGoogleAnalyticsTracker.send(MapBuilder
				    .createEvent("TransferController", "NewData", "onIdtoWsResponse", null)
				    .build()
				);
			
			mArrayAdapter.notifyDataSetChanged();
		}
		
		@Override
		public void onError(String error) {
			if(error!=null)
				mLogger.error("In Progress Trips Response: " + error);
		}
	};
	
	private IdtoWsResponse<ArrayList<TConnectStatus>> mGetTConnectStatusResponse = new IdtoWsResponse<ArrayList<TConnectStatus>>() {
		@Override
		public void onIdtoWsResponse(ArrayList<TConnectStatus> response) {
		
			int iConn = 0;
			int tripId = 0;
			for(TConnectStatus tc : response)
			{
				tripId = tc.getTripId();
				iConn = tc.getTConnectStatusId();
			}
			
			for(IdtoTrip idtoTrip : mTransfers)
			{
				if(idtoTrip.getId() == tripId)
				{
					idtoTrip.setTConnectStatus(iConn);
					
					continue;
				}
			}
			
			mArrayAdapter.notifyDataSetChanged();
		}

		@Override
		public void onError(String error) {
			if(error!=null)
				mLogger.error("TConnect Status Response: " + error);
		}
	};
}
