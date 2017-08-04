
package org.battelle.idto.ws.otp.data;

import java.util.List;

public class StopType{
   	private String direction;
   	private Id id;
   	private String locationType;
   	private String parentStation;
   	private List<Routes> routes;
   	private String stopCode;
   	private String stopDesc;
   	private Number stopLat;
   	private Number stopLon;
   	private String stopName;
   	private String stopUrl;
   	private String wheelchairBoarding;
   	private String zoneId;

 	public String getDirection(){
		return this.direction;
	}
	public void setDirection(String direction){
		this.direction = direction;
	}
 	public Id getId(){
		return this.id;
	}
	public void setId(Id id){
		this.id = id;
	}
 	public String getLocationType(){
		return this.locationType;
	}
	public void setLocationType(String locationType){
		this.locationType = locationType;
	}
 	public String getParentStation(){
		return this.parentStation;
	}
	public void setParentStation(String parentStation){
		this.parentStation = parentStation;
	}
 	public List<Routes> getRoutes(){
		return this.routes;
	}
	public void setRoutes(List<Routes> routes){
		this.routes = routes;
	}
 	public String getStopCode(){
		return this.stopCode;
	}
	public void setStopCode(String stopCode){
		this.stopCode = stopCode;
	}
 	public String getStopDesc(){
		return this.stopDesc;
	}
	public void setStopDesc(String stopDesc){
		this.stopDesc = stopDesc;
	}
 	public Number getStopLat(){
		return this.stopLat;
	}
	public void setStopLat(Number stopLat){
		this.stopLat = stopLat;
	}
 	public Number getStopLon(){
		return this.stopLon;
	}
	public void setStopLon(Number stopLon){
		this.stopLon = stopLon;
	}
 	public String getStopName(){
		return this.stopName;
	}
	public void setStopName(String stopName){
		this.stopName = stopName;
	}
 	public String getStopUrl(){
		return this.stopUrl;
	}
	public void setStopUrl(String stopUrl){
		this.stopUrl = stopUrl;
	}
 	public String getWheelchairBoarding(){
		return this.wheelchairBoarding;
	}
	public void setWheelchairBoarding(String wheelchairBoarding){
		this.wheelchairBoarding = wheelchairBoarding;
	}
 	public String getZoneId(){
		return this.zoneId;
	}
	public void setZoneId(String zoneId){
		this.zoneId = zoneId;
	}
}
