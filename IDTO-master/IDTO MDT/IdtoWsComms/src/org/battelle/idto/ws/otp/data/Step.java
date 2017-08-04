package org.battelle.idto.ws.otp.data;

public class Step {
	public int Id;
    public int TripId;
    public int ModeId;
    public String StartDate;
    public String EndDate;
    public String FromName;
    public String FromStopCode;
    public int FromProviderId;
    public String ToName;
    public String ToStopCode;
    public int ToProviderId;
    public double Distance;
    public String RouteNumber;
    
    public String BlockIdentifier;
    
	public int getId() {
		return Id;
	}
	public void setId(int id) {
		Id = id;
	}
	public int getTripId() {
		return TripId;
	}
	public void setTripId(int tripId) {
		TripId = tripId;
	}
	public int getModeId() {
		return ModeId;
	}
	public void setModeId(int modeId) {
		ModeId = modeId;
	}
	public String getStartDate() {
		return StartDate;
	}
	public void setStartDate(String startDate) {
		StartDate = startDate;
	}
	public String getEndDate() {
		return EndDate;
	}
	public void setEndDate(String endDate) {
		EndDate = endDate;
	}
	public String getFromName() {
		return FromName;
	}
	public void setFromName(String fromName) {
		FromName = fromName;
	}
	public String getFromStopCode() {
		return FromStopCode;
	}
	public void setFromStopCode(String fromStopCode) {
		FromStopCode = fromStopCode;
	}
	public int getFromProviderId() {
		return FromProviderId;
	}
	public void setFromProviderId(int fromProviderId) {
		FromProviderId = fromProviderId;
	}
	public String getToName() {
		return ToName;
	}
	public void setToName(String toName) {
		ToName = toName;
	}
	public String getToStopCode() {
		return ToStopCode;
	}
	public void setToStopCode(String toStopCode) {
		ToStopCode = toStopCode;
	}
	public int getToProviderId() {
		return ToProviderId;
	}
	public void setToProviderId(int toProviderId) {
		ToProviderId = toProviderId;
	}
	public double getDistance() {
		return Distance;
	}
	public void setDistance(double distance) {
		Distance = distance;
	}
	public String getRouteNumber() {
		return RouteNumber;
	}
	public void setRouteNumber(String routeNumber) {
		RouteNumber = routeNumber;
	}
	public String getBlockIdentifier() {
		return BlockIdentifier;
	}
	public void setBlockIdentifier(String blockIdentifier) {
		BlockIdentifier = blockIdentifier;
	}
    
    
}
