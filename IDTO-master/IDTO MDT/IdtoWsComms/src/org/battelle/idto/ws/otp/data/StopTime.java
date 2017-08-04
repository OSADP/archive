
package org.battelle.idto.ws.otp.data;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.List;

public class StopTime{
   	private String phase;
   	private Number time;
   	private RouteTrip trip;

 	public String getPhase(){
		return this.phase;
	}
	public void setPhase(String phase){
		this.phase = phase;
	}
 	public Number getTime(){
		return this.time;
	}
	public void setTime(Number time){
		this.time = time;
	}
 	public RouteTrip getTrip(){
		return this.trip;
	}
	public void setTrip(RouteTrip trip){
		this.trip = trip;
	}
	
	@Override
	public String toString() {
		Date date = new Date(getTime().longValue()*1000l);
		
		SimpleDateFormat formatter = new SimpleDateFormat("hh:mm:ss a");
		return formatter.format(date) + " - " + getTrip().getTripHeadsign();
	}
}
