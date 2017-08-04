package org.battelle.idto.mdt.controllers;

import java.util.Date;

import org.battelle.idto.mdt.models.GateStopTime;
import org.battelle.idto.mdt.models.Gate;
import org.battelle.idto.mdt.preferences.Constants;
import org.battelle.idto.ws.comms.IdtoWsInterface;
import org.battelle.idto.ws.comms.IdtoWsManager;
import org.battelle.idto.ws.comms.IdtoWsResponse;
import org.battelle.idto.ws.otp.data.StopTime;
import org.battelle.idto.ws.otp.data.ETA;
import org.battelle.idto.ws.otp.data.StopTimesForStop;
import org.battelle.idto.ws.otp.data.StopType;
import org.battelle.idto.ws.otp.data.StopsNearPoint;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import android.content.Context;
import android.content.SharedPreferences;
import android.location.Location;

public class GateStopTimesController {

	public interface GatePopulationListener{
		public void populated(Gate gateStops);
		public void error(String errorString);
	}
	
	private Gate mGateStops;
	private IdtoWsInterface mIdtoWsManager;
	private GatePopulationListener mPopulationListener;
	private int mETA =0;
	private static final Logger mLogger = LoggerFactory.getLogger(GateStopTimesController.class); 
	
	public GateStopTimesController(Gate gate, GatePopulationListener populationListener, Context context)
	{
		SharedPreferences prefs = context.getSharedPreferences("LOCATION", 0);
		
		double latitude = Double.parseDouble(prefs.getString("LastLatitude","0"));
		double longitude = Double.parseDouble(prefs.getString("LastLongitude","0"));
		
		Location currentLocation = new Location("");
		currentLocation.setLatitude(latitude);
		currentLocation.setLongitude(longitude);

		mGateStops = gate;
		mPopulationListener = populationListener;
		
		mIdtoWsManager = new IdtoWsManager(Constants.API_URL, context);
		
		Location loc = new Location("");
		loc.setLatitude(mGateStops.getLatitude());
		loc.setLongitude(mGateStops.getLongitude());
		mIdtoWsManager.getETA(currentLocation, loc, mETAResponse);
		
	}

	public Gate getGateStops() {
		return mGateStops;
	}

	private IdtoWsResponse<ETA> mETAResponse = new IdtoWsResponse<ETA>() {

		@Override
		public void onIdtoWsResponse(ETA response) {
			// TODO Auto-generated method stub
			Location loc = new Location("");
			loc.setLatitude(mGateStops.getLatitude());
			loc.setLongitude(mGateStops.getLongitude());
			
			double dETA = ((double)response.duration/1000.0);
			dETA = dETA /60.0;
			
			mETA = (int) Math.round(dETA);
			
			mIdtoWsManager.getStopsNearPoint(loc, mStopsNearPointResponse);
		}

		@Override
		public void onError(String error) {
			// TODO Auto-generated method stub
			Location loc = new Location("");
			loc.setLatitude(mGateStops.getLatitude());
			loc.setLongitude(mGateStops.getLongitude());
			mIdtoWsManager.getStopsNearPoint(loc, mStopsNearPointResponse);
		}
		
	};
	
	private IdtoWsResponse<StopsNearPoint> mStopsNearPointResponse = new IdtoWsResponse<StopsNearPoint>() {
		
		@Override
		public void onIdtoWsResponse(StopsNearPoint response) {
			// TODO Auto-generated method stub
			mGateStops.clearStopTimes();
			for(StopType stop:response.getStops())
			{
				Date now = new Date();
				
				int offset = mETA + 3; //add 3 min walk time to ETA
				
				long startTime_ms = now.getTime() + (1000 * 60 * offset);
				
				long endTime_ms = startTime_ms + (1000 * 60 * 30);// add 30 min in ms
				
				mIdtoWsManager.getStopTimesForStop(stop, startTime_ms, endTime_ms, mStopTimesNearStopResponse);
			}
		}
		
		@Override
		public void onError(String error) {
			mPopulationListener.equals(error);
		}
	};
	

	private IdtoWsResponse<StopTimesForStop> mStopTimesNearStopResponse = new IdtoWsResponse<StopTimesForStop>() {
		
		@Override
		public void onIdtoWsResponse(StopTimesForStop response) {
			
			for(StopTime stopTime:response.getStopTimes()){
				mGateStops.addStopTime(new GateStopTime(stopTime, response.getStopInfo()));
			}
			
			mPopulationListener.populated(mGateStops);
			
		}
		
		@Override
		public void onError(String error) {
			if(error==null)
				error="";
			
			mPopulationListener.error(error);
			mLogger.error("StopTimesForStopResponse: " + error);
		}
	};
}
