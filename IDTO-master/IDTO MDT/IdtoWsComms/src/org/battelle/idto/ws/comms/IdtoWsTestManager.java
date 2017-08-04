package org.battelle.idto.ws.comms;

import java.util.ArrayList;
import java.util.Date;

import org.battelle.idto.ws.otp.data.ETA;
import org.battelle.idto.ws.otp.data.Id;
import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.ProbeMessage;
import org.battelle.idto.ws.otp.data.ProbeResponse;
import org.battelle.idto.ws.otp.data.StopTime;

import org.battelle.idto.ws.otp.data.StopTimesForStop;
import org.battelle.idto.ws.otp.data.StopType;

import org.battelle.idto.ws.otp.data.StopsNearPoint;
import org.battelle.idto.ws.otp.data.RouteTrip;
import org.battelle.idto.ws.otp.data.TConnectStatus;

import android.location.Location;

public class IdtoWsTestManager implements IdtoWsInterface {

	@Override
	public void getStopsNearPoint(Location point, IdtoWsResponse<StopsNearPoint> idtoWsResponse) {
		//39.977592&lon=-82.913055 //West Gate

		//39.975148&lon=-82.894379//MainGate
		
		StopsNearPoint response = new StopsNearPoint();
		ArrayList<StopType> stops = new ArrayList<StopType>();
		
		
		if(point.getLatitude() == 39.975148) // Main Gate
		{
			stops.add(generateStopType("E BROAD ST & BEECHTREE RD", 
					39.973919, -82.894701, "BROBEETE", "COTA", "2502"));
			stops.add(generateStopType("E BROAD ST & BEECHTREE RD", 
					39.974148, -82.89389, "BROBEETW", "COTA", "2522"));

		}
		else if(point.getLatitude() == 39.977592)//West Gate
		{
			stops.add(generateStopType("RUHL AVE & N JAMES RD", 
					39.977599, -82.913471, "RUHJAME", "COTA", "5701"));
			stops.add(generateStopType("RUHL AVE & N KELLNER RD", 
					39.977718, -82.914155, "RUHKELW", "COTA", "7274"));
		}
		
		response.setStops(stops);
		idtoWsResponse.onIdtoWsResponse(response);
	}

	private StopType generateStopType(String name, double lat, double lon, String code, String agency, String id)
	{
		StopType stopType = new StopType();
		stopType.setStopName(name);
		stopType.setStopLat(lat);
		stopType.setStopLon(lon);
		stopType.setStopCode(code);
		
		Id stopid = new Id(agency, id);
		
		stopType.setId(stopid);
		
		return stopType;
	}
	
	@Override
	public void getStopTimesForStop(final StopType stopType, long startTime_ms, long endTime_ms, final IdtoWsResponse<StopTimesForStop> idtoWsResponse) {
		StopTimesForStop stopTimesForStop = new StopTimesForStop();
		ArrayList<StopTime> stopTimeList = new ArrayList<StopTime>();
		
		int stopId = Integer.parseInt(stopType.getId().getId());
		
		if(stopId==2502)//E Broad St and Beechtree Rd East
		{
			stopTimeList.add(genearteStopTime("10R EAST BROAD TO HAMILTON AND MAIN", 7, "80002"));
			stopTimeList.add(genearteStopTime("10E EAST BROAD TO MC NAUGHTEN AND MOUNT CARMEL HOSPITAL", 12, "80004"));
			
		}
		else if(stopId==2522)//E Broad St and Beechtree Rd West
		{
			stopTimeList.add(genearteStopTime("10G WEST BROAD TO DOCTORS HOSPITAL AND GALLOWAY ROAD", 3, "80001"));
			stopTimeList.add(genearteStopTime("10K WEST BROAD TO WILSON ROAD", 12, "80003"));
		}
		else if(stopId==5701)//Ruhl Ave. and N James Rd
		{
			stopTimeList.add(genearteStopTime("6 SULLIVANT TO GEORGESVILLE ROAD", 5, "465741"));
			stopTimeList.add(genearteStopTime("92 JAMES-STELZER TO VA HOSPITAL AND EASTLAND MALL", 6, "469938"));
		}
		else if(stopId==7274)//Rulh Ave. and N Kellner Rd
		{
			stopTimeList.add(genearteStopTime("6 MOUNT VERNON TO JAMES ROAD AND VA HOSPITAL", 8, "465791"));
			stopTimeList.add(genearteStopTime("92 JAMES-STELZER TO VA HOSPITAL AND EASTON", 9, "469988"));
		}
		
		
		stopTimesForStop.setStopTimes(stopTimeList);
		
		idtoWsResponse.onIdtoWsResponse(stopTimesForStop);
		
	}
	
	private StopTime genearteStopTime(String headsign, int minOffset, String id){
		StopTime stopTime = new StopTime();
		
		RouteTrip trip = new RouteTrip();
		trip.setId(new Id("COTA", id));
		trip.setTripHeadsign(headsign);
		
		stopTime.setTrip(trip);
		stopTime.setPhase("departure");
		Date now = new Date();
		long time = now.getTime() + (1000 * 60 * minOffset);
		stopTime.setTime(time);

		return stopTime;
	}
	
	public void getTrips(int userId, IdtoTripType tripType, final IdtoWsResponse<ArrayList<IdtoTrip>> idtoWsResponse)
	{
		idtoWsResponse.onIdtoWsResponse(new ArrayList<IdtoTrip>());
	}
	
	public void postIdtoTrip(IdtoTrip trip,final IdtoWsResponse<IdtoTrip> idtoWsResponse)
	{
		idtoWsResponse.onIdtoWsResponse(trip);
	}
	
	public void postProbe(ProbeMessage probeMsg, final IdtoWsResponse<ProbeResponse> idtoWsResponse)
	{
		idtoWsResponse.onIdtoWsResponse(new ProbeResponse());
	}

	@Override
	public void getTConnectStatus(int tripId, final IdtoWsResponse<ArrayList<TConnectStatus>> idtoWsResponse){
		
	}
	
	public void getETA(Location startLocation, Location endLocation, final IdtoWsResponse<ETA> idtoWsResponse)
	{

	}
}
