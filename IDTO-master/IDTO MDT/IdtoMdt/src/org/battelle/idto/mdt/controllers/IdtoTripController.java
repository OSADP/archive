package org.battelle.idto.mdt.controllers;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.TimeZone;

import org.battelle.idto.mdt.models.GateStopTime;
import org.battelle.idto.mdt.preferences.Constants;
import org.battelle.idto.ws.comms.IdtoWsInterface;
import org.battelle.idto.ws.comms.IdtoWsManager;
import org.battelle.idto.ws.comms.IdtoWsResponse;
import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.Step;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import android.content.Context;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;

public class IdtoTripController {

	private static final Logger mLogger = LoggerFactory.getLogger(GateStopTimesController.class); 
	
	private IdtoWsInterface mIdtoWsManager;
	private Context mContext;
	public IdtoTripController(Context context)
	{
		mContext = context;
		mIdtoWsManager = new IdtoWsManager(Constants.API_URL, context);
	}
	
	public void createTrip(String fromGateName, String fromGateCode, GateStopTime gateStopTime)
	{
		long cotaStartDate_ms = gateStopTime.getStopTime().getTime().longValue() * 1000l;
		
		String blockId = gateStopTime.getStopTime().getTrip().getBlockId();

		Date tripStartDate = new Date();
		tripStartDate = new Date(tripStartDate.getTime() + (0 * 60 * 1000));
		Date capTranEndDate = new Date(tripStartDate.getTime() + (3 * 60 * 1000));
		
		Date walkStartDate = new Date(capTranEndDate.getTime());
		Date walkEndDate = new Date(walkStartDate.getTime() + (2*60*1000));
		
		Date cotaStartDate = new Date(cotaStartDate_ms);
		Date cotaEndDate = new Date(cotaStartDate_ms + (3 * 60 * 1000));		
		SimpleDateFormat dateFrmt = new SimpleDateFormat("MM/dd/yyyy HH:mm:ss");
		dateFrmt.setTimeZone(TimeZone.getTimeZone("UTC"));
	    SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(mContext);
	    int userId = prefs.getInt("UserID", -1);
	    
		
		IdtoTrip idtoTrip = new IdtoTrip();
		
		idtoTrip.setBicycleFlag(Boolean.parseBoolean(gateStopTime.getStopTime().getTrip().getTripBikesAllowed()));
		idtoTrip.setMobilityFlag(Boolean.parseBoolean(gateStopTime.getStopTime().getTrip().getWheelchairAccessible()));
		idtoTrip.setDestination(gateStopTime.getStopTime().getTrip().getTripHeadsign());
		idtoTrip.setOrigination("DCSC");
		idtoTrip.setTravelerId(userId);
		idtoTrip.setPriorityCode("1");
		
		ArrayList<Step> stepList = new ArrayList<Step>();
		
		Step capTranBusSetp = new Step();
		capTranBusSetp.setModeId(2);
		capTranBusSetp.setFromProviderId(4);
		capTranBusSetp.setStartDate(dateFrmt.format(tripStartDate));
		capTranBusSetp.setEndDate(dateFrmt.format(capTranEndDate));
		capTranBusSetp.setFromName("DSCS Campus");
		capTranBusSetp.setToName(fromGateName);
		capTranBusSetp.setToStopCode(fromGateCode);
		capTranBusSetp.setToProviderId(4);
		capTranBusSetp.setDistance(1);
		stepList.add(capTranBusSetp);
		
		Step walkStep = new Step();
		walkStep.setModeId(1);
		walkStep.setDistance(0.1);
		walkStep.setToProviderId(1);
		walkStep.setStartDate(dateFrmt.format(walkStartDate));
		walkStep.setEndDate(dateFrmt.format(walkEndDate));
		walkStep.setFromName(fromGateName);
		walkStep.setFromStopCode(fromGateCode);
		walkStep.setFromProviderId(4);
		walkStep.setToName(gateStopTime.getStopType().getStopName());
		walkStep.setToStopCode(gateStopTime.getStopType().getStopCode());
		stepList.add(walkStep);
		
		Step cotaStep = new Step();

		cotaStep.setStartDate(dateFrmt.format(cotaStartDate));
		cotaStep.setFromName(gateStopTime.getStopType().getStopName());
		cotaStep.setFromProviderId(1);//COTA
		cotaStep.setDistance(1);
		cotaStep.setEndDate(dateFrmt.format(cotaEndDate));
		cotaStep.setFromStopCode(gateStopTime.getStopType().getStopCode());
		cotaStep.setModeId(2);
		cotaStep.setBlockIdentifier(blockId);
		
		if(gateStopTime.getStopTime().getTrip().getRouteId()==null)
		{
			cotaStep.setRouteNumber("");
		}
		else
		{
			cotaStep.setRouteNumber(gateStopTime.getStopTime().getTrip().getRouteId());
		}
		
		cotaStep.setRouteNumber(gateStopTime.getStopTime().getTrip().getRouteId());
		cotaStep.setToName("Unknown");
		cotaStep.setToStopCode("UNK");

		stepList.add(cotaStep);
		
		idtoTrip.setSteps(stepList);
		idtoTrip.setTripEndDate(dateFrmt.format(cotaEndDate));
		idtoTrip.setTripStartDate(dateFrmt.format(tripStartDate));
		
		mIdtoWsManager.postIdtoTrip(idtoTrip, mPostTripResponse);
	}

	private IdtoWsResponse<IdtoTrip> mPostTripResponse = new IdtoWsResponse<IdtoTrip>() {
		
		@Override
		public void onIdtoWsResponse(IdtoTrip response) {
			
			mLogger.debug("Trip Response ID" + Integer.toString(response.Id));
		}
		
		@Override
		public void onError(String error) {
			if(error!=null)
				mLogger.error("Trip Response: " + error);
		}
	};
}
