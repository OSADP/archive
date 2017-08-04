package org.battelle.idto.ws.otp.data;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

public class IdtoTrip implements Comparable<IdtoTrip> {
	public int Id;
    public int TravelerId;
    public String Origination;
    public String Destination;
    public String TripStartDate;
    public String TripEndDate;
    public boolean MobilityFlag;
    public boolean BicycleFlag;
    public String PriorityCode;
    public ArrayList<Step> Steps;
    
    public int TConnectStatus;
	
    public int getId() {
		return Id;
	}
	public void setId(int id) {
		Id = id;
	}
	public int getTravelerId() {
		return TravelerId;
	}
	public void setTravelerId(int travelerId) {
		TravelerId = travelerId;
	}
	public String getOrigination() {
		return Origination;
	}
	public void setOrigination(String origination) {
		Origination = origination;
	}
	public String getDestination() {
		return Destination;
	}
	public void setDestination(String destination) {
		Destination = destination;
	}
	public String getTripStartDate() {
		return TripStartDate;
	}
	public void setTripStartDate(String tripStartDate) {
		TripStartDate = tripStartDate;
	}
	public String getTripEndDate() {
		return TripEndDate;
	}
	public void setTripEndDate(String tripEndDate) {
		TripEndDate = tripEndDate;
	}
	public boolean isMobilityFlag() {
		return MobilityFlag;
	}
	public void setMobilityFlag(boolean mobilityFlag) {
		MobilityFlag = mobilityFlag;
	}
	public boolean isBicycleFlag() {
		return BicycleFlag;
	}
	public void setBicycleFlag(boolean bicycleFlag) {
		BicycleFlag = bicycleFlag;
	}
	public String getPriorityCode() {
		return PriorityCode;
	}
	public void setPriorityCode(String priorityCode) {
		PriorityCode = priorityCode;
	}
	public ArrayList<Step> getSteps() {
		return Steps;
	}
	public void setSteps(ArrayList<Step> steps) {
		Steps = steps;
	}
	
	
    
	public int getTConnectStatus() {
		return TConnectStatus;
	}
	public void setTConnectStatus(int tConnectStatus) {
		TConnectStatus = tConnectStatus;
	}
	@Override
	public String toString() {
		
		try {
			SimpleDateFormat inputFormatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH);
			inputFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));
			
			Date date;
			
			date = inputFormatter.parse(TripStartDate);
			
			SimpleDateFormat formatter = new SimpleDateFormat("HH:mm:ss");
			formatter.setTimeZone(TimeZone.getDefault());
			
			
			
			return formatter.format(date) + " - " + getDestination();
		} catch (ParseException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		return "";
	}
	@Override
	public int compareTo(IdtoTrip another) {
		try {
		SimpleDateFormat inputFormatter = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH);
		inputFormatter.setTimeZone(TimeZone.getTimeZone("UTC"));
		
		Date myStartDate;

			myStartDate = inputFormatter.parse(TripStartDate);
		
		
		Date anotherStartDate = inputFormatter.parse(another.getTripStartDate());
		
			return myStartDate.compareTo(anotherStartDate);
		} catch (ParseException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		return 0;
	}
}
