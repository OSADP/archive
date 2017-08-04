package org.battelle.idto.ws.otp.data;

import com.google.gson.annotations.SerializedName;

public class Position {

	@SerializedName("Latitude")
    private double mLatitude;
	@SerializedName("Longitude")
    private double mLongitude;
	@SerializedName("Speed")
    private double mSpeed;
	@SerializedName("Heading")
    private double mHeading;
	@SerializedName("TimeStamp")
    private long mTimeStamp;
	@SerializedName("Satellites")
    private int mSatellites;
	@SerializedName("Accuracy")
    private double mAccuracy;
	@SerializedName("Altitude")
    private double mAltitude;
    
	public double getLatitude() {
		return mLatitude;
	}
	public void setLatitude(double latitude) {
		mLatitude = latitude;
	}
	public double getLongitude() {
		return mLongitude;
	}
	public void setLongitude(double longitude) {
		mLongitude = longitude;
	}
	public double getSpeed() {
		return mSpeed;
	}
	public void setSpeed(double speed) {
		mSpeed = speed;
	}
	public double getHeading() {
		return mHeading;
	}
	public void setHeading(double heading) {
		mHeading = heading;
	}
	public long getTimeStamp() {
		return mTimeStamp;
	}
	public void setTimeStamp(long timeStamp) {
		mTimeStamp = timeStamp;
	}
	public int getSatellites() {
		return mSatellites;
	}
	public void setSatellites(int satellites) {
		mSatellites = satellites;
	}
	public double getAccuracy() {
		return mAccuracy;
	}
	public void setAccuracy(double accuracy) {
		mAccuracy = accuracy;
	}
	public double getAltitude() {
		return mAltitude;
	}
	public void setAltitude(double altitude) {
		mAltitude = altitude;
	}

    
}
