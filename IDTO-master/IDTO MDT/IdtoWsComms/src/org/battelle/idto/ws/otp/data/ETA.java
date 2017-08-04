package org.battelle.idto.ws.otp.data;

public class ETA {

	public int duration;
	public long startTime;
	public long endTime;
	public int walkTime;
	public int transitTime;
	public int waitingTime;
	public int walkDistance;
	public int getDuration() {
		return duration;
	}
	public void setDuration(int duration) {
		this.duration = duration;
	}
	public long getStartTime() {
		return startTime;
	}
	public void setStartTime(long startTime) {
		this.startTime = startTime;
	}
	public long getEndTime() {
		return endTime;
	}
	public void setEndTime(long endTime) {
		this.endTime = endTime;
	}
	public int getWalkTime() {
		return walkTime;
	}
	public void setWalkTime(int walkTime) {
		this.walkTime = walkTime;
	}
	public int getTransitTime() {
		return transitTime;
	}
	public void setTransitTime(int transitTime) {
		this.transitTime = transitTime;
	}
	public int getWaitingTime() {
		return waitingTime;
	}
	public void setWaitingTime(int waitingTime) {
		this.waitingTime = waitingTime;
	}
	public int getWalkDistance() {
		return walkDistance;
	}
	public void setWalkDistance(int walkDistance) {
		this.walkDistance = walkDistance;
	}
	
	
}
