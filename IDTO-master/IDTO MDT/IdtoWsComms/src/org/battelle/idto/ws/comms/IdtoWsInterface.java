package org.battelle.idto.ws.comms;

import java.util.ArrayList;

import org.battelle.idto.ws.comms.IdtoWsResponse;
import org.battelle.idto.ws.otp.data.ETA;
import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.ProbeResponse;
import org.battelle.idto.ws.otp.data.ProbeMessage;
import org.battelle.idto.ws.otp.data.StopTimesForStop;
import org.battelle.idto.ws.otp.data.StopType;
import org.battelle.idto.ws.otp.data.StopsNearPoint;
import org.battelle.idto.ws.otp.data.TConnectStatus;

import android.location.Location;


public interface IdtoWsInterface {
	
	public void getStopsNearPoint(Location point, final IdtoWsResponse<StopsNearPoint> idtoWsResponse);
	public void getStopTimesForStop(final StopType stopType, long startTime_ms, long endTime_ms, final IdtoWsResponse<StopTimesForStop> idtoWsResponse);
	
	public void getTrips(int userId, IdtoTripType tripType, final IdtoWsResponse<ArrayList<IdtoTrip>> idtoWsResponse);
	
	public void getTConnectStatus(int tripId, final IdtoWsResponse<ArrayList<TConnectStatus>> idtoWsResponse);
	
	public void postIdtoTrip(IdtoTrip trip,final IdtoWsResponse<IdtoTrip> idtoWsResponse);
	
	public void postProbe(ProbeMessage probeMsg, final IdtoWsResponse<ProbeResponse> idtoWsResponse);
	
	public void getETA(Location startLocation, Location endLocation, final IdtoWsResponse<ETA> idtoWsResponse);
}
