package org.battelle.idto.mdt.models;

import java.util.Date;

import org.battelle.idto.ws.otp.data.StopTime;
import org.battelle.idto.ws.otp.data.StopType;

public class GateStopTime implements Comparable<GateStopTime>{

	private StopTime mStopTime;
	private StopType mStopType;
	
	public GateStopTime(StopTime stopTime, StopType stopType)
	{
		mStopTime = stopTime;
		mStopType = stopType;
	}
	
	public StopTime getStopTime() {
		return mStopTime;
	}
	public void setStopTime(StopTime stopTime) {
		mStopTime = stopTime;
	}
	public StopType getStopType() {
		return mStopType;
	}
	public void setStopType(StopType stopType) {
		mStopType = stopType;
	}
	
	@Override
	public String toString() {
		return this.mStopTime.toString();
	}

	@Override
	public int compareTo(GateStopTime another) {
		Date date = new Date(mStopTime.getTime().longValue()*1000l);
		Date anotherDate = new Date(another.getStopTime().getTime().longValue()*1000l);
		
		return date.compareTo(anotherDate);
	}
}
