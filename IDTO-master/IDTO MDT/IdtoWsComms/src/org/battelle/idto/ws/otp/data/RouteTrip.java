
package org.battelle.idto.ws.otp.data;

import java.util.List;

public class RouteTrip{
   	private String blockId;
   	private String directionId;
   	private Id id;
   	private String route;
   	private String routeId;
   	private String serviceId;
   	private String shapeId;
   	private String tripBikesAllowed;
   	private String tripHeadsign;
   	private String tripShortName;
   	private String wheelchairAccessible;

 	public String getBlockId(){
		return this.blockId;
	}
	public void setBlockId(String blockId){
		this.blockId = blockId;
	}
 	public String getDirectionId(){
		return this.directionId;
	}
	public void setDirectionId(String directionId){
		this.directionId = directionId;
	}
 	public Id getId(){
		return this.id;
	}
	public void setId(Id id){
		this.id = id;
	}
 	public String getRoute(){
		return this.route;
	}
	public void setRoute(String route){
		this.route = route;
	}
 	public String getRouteId(){
		return this.routeId;
	}
	public void setRouteId(String routeId){
		this.routeId = routeId;
	}
 	public String getServiceId(){
		return this.serviceId;
	}
	public void setServiceId(String serviceId){
		this.serviceId = serviceId;
	}
 	public String getShapeId(){
		return this.shapeId;
	}
	public void setShapeId(String shapeId){
		this.shapeId = shapeId;
	}
 	public String getTripBikesAllowed(){
		return this.tripBikesAllowed;
	}
	public void setTripBikesAllowed(String tripBikesAllowed){
		this.tripBikesAllowed = tripBikesAllowed;
	}
 	public String getTripHeadsign(){
		return this.tripHeadsign;
	}
	public void setTripHeadsign(String tripHeadsign){
		this.tripHeadsign = tripHeadsign;
	}
 	public String getTripShortName(){
		return this.tripShortName;
	}
	public void setTripShortName(String tripShortName){
		this.tripShortName = tripShortName;
	}
 	public String getWheelchairAccessible(){
		return this.wheelchairAccessible;
	}
	public void setWheelchairAccessible(String wheelchairAccessible){
		this.wheelchairAccessible = wheelchairAccessible;
	}
}
