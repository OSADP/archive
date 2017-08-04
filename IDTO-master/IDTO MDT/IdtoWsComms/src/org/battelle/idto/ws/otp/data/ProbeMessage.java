package org.battelle.idto.ws.otp.data;

import java.util.ArrayList;

import com.google.gson.annotations.SerializedName;

public class ProbeMessage {
	@SerializedName("Positions")
    private ArrayList<Position> mPositions;
	@SerializedName("InboundVehicle")
    private String mInboundVehicle;
	@SerializedName("Wave")
    private String mWave;
    
	public ArrayList<Position> getPositions() {
		return mPositions;
	}
	public void setPositions(ArrayList<Position> positions) {
		this.mPositions = positions;
	}
	public String getInboundVehicle() {
		return mInboundVehicle;
	}
	public void setInboundVehicle(String inboundVehicle) {
		this.mInboundVehicle = inboundVehicle;
	}
	public String getWave() {
		return mWave;
	}
	public void setWave(String wave) {
		this.mWave = wave;
	}
    
    
}
