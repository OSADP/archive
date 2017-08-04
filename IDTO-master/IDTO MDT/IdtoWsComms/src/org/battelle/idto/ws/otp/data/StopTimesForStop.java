
package org.battelle.idto.ws.otp.data;

import java.util.List;

public class StopTimesForStop{
   	private List<StopTime> stopTimes;
   	
   	private StopType stopInfo;

 	public List<StopTime> getStopTimes(){
		return this.stopTimes;
	}
	public void setStopTimes(List<StopTime> stopTimes){
		this.stopTimes = stopTimes;
	}
	public StopType getStopInfo() {
		return stopInfo;
	}
	public void setStopInfo(StopType stopInfo) {
		this.stopInfo = stopInfo;
	}
	
	
}
