package org.battelle.idto.mdt.models;

import java.util.ArrayList;
import java.util.Collections;

import org.battelle.idto.ws.otp.data.StopTime;

import com.google.gson.annotations.SerializedName;

public class Gate {

	@SerializedName ("name")
	private String mGateName;
	
	@SerializedName ("code")
	private String mGateCode;
	
	@SerializedName("latitude")
	private double mLatitude;
	
	@SerializedName("longitude")
	private double mLongitude;
	
	private ArrayList<GateStopTime> mStopTimeList;
	
	public Gate()
	{
		mStopTimeList = new ArrayList<GateStopTime>();
	}
	
	public double getLatitude() {
		return mLatitude;
	}

	public double getLongitude() {
		return mLongitude;
	}

	public void setGateName(String gateName) {
		mGateName = gateName;
	}

	public void setStopTimeList(ArrayList<GateStopTime> stopTimeList) {
		mStopTimeList = stopTimeList;
		Collections.sort(mStopTimeList);
	}

	public String getGateName() {
		return mGateName;
	}

	public ArrayList<GateStopTime> getStopTimeList() {
		return mStopTimeList;
	}
	
	public void addStopTime(GateStopTime stopTime)
	{
		mStopTimeList.add(stopTime);
		Collections.sort(mStopTimeList);
	}

	@Override
	public String toString() {
		return mGateName;
	}

	public void clearStopTimes() {
		mStopTimeList.clear();
	}

	public String getGateCode() {
		return mGateCode;
	}

	public void setGateCode(String gateCode) {
		mGateCode = gateCode;
	}

}
