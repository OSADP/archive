/**
Copyright 2015 Leidos Corp

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

package com.leidos.optimizer;

import com.leidos.optimizer.data.Optimizable;

public class TimeGapChecker {

	
	
	public boolean checkTimeDifference(Truck t, Optimizable potentialMove){
		int lastApptWindow = 0;
		int lastApptStart = 0;
		int lastApptEnd = 0;
		int emptiesAfterLastLoad = 0;
		int firstApptStart = t.getMoves()[0].getApptStart();
		int firstApptEnd = t.getMoves()[0].getApptEnd();
		
		if(potentialMove.getApptStart() == 0 && potentialMove.getApptEnd() == 0){
			return true;
		}
		
		
		
		for(Optimizable o:t.getMoves()){
			if(o.getApptStart() == 0 && o.getApptEnd() == 0){
				emptiesAfterLastLoad++;
			}else{
				if(o.getApptStart() > lastApptStart){
					lastApptStart = o.getApptStart(); 
				}
				if(o.getApptEnd() > lastApptEnd){
					lastApptEnd = o.getApptEnd(); 
				}
				if (o.getApptStart() < firstApptStart) {
					firstApptStart = o.getApptStart();
				}
				if (o.getApptEnd() < firstApptEnd) {
					firstApptEnd = o.getApptEnd();
				}
				
				lastApptWindow = o.getApptEnd()-o.getApptStart();
				emptiesAfterLastLoad = 0;
			}
		}
		
		
		
		int timeGap = 0;
		if(emptiesAfterLastLoad > 0){
			if(emptiesAfterLastLoad %2 == 0){
				timeGap = (emptiesAfterLastLoad*150)+130;
			}else{
				timeGap = ((emptiesAfterLastLoad+1)*150);
			}
		}else{
			timeGap = 130;
		}
		
		if(potentialMove.getApptStart() >= lastApptStart+timeGap || potentialMove.getApptEnd() >= lastApptEnd+timeGap){
			return true;
		}else{
			//int potentialAppWindow = potentialMove.getApptEnd()-potentialMove.getApptStart();
			if(lastApptWindow > 0){
				int valToAdjust = 0;
				if(timeGap == 130){
					valToAdjust = 170;
				}else{
					valToAdjust = timeGap;
				}
				if(lastApptEnd - valToAdjust > lastApptStart){
					lastApptEnd = lastApptEnd - valToAdjust;
					if(potentialMove.getApptStart() >= lastApptStart+timeGap || potentialMove.getApptEnd() >= lastApptEnd+timeGap){
						return true;
					}
				}
			}
			
		}
		
		
		
		
		
//		if(emptiesAfterLastLoad == 0){
//			if(lastApptWindow == 0){
//				if(isHardAppt(potentialMove)){
//					return potentialMove.getApptStart() - lastApptStart >= 130;
//				}else{
//					if(potentialMove.getApptStart() - lastApptStart >= 130 || potentialMove.getApptEnd() - lastApptStart >=130){
//						return true;
//					}
//				}
////			}else{
//
//			}
////		}else{
////			//use number of empties to figure out how much time we need between moves
//			
//		}
//		//last appt wasn't a hard appt...
//		int gap = 0;
//		if(t.getMoves().length % 2 == 0){
//			gap = ((t.getMoves().length)*150);
//		}else{
//			gap = ((t.getMoves().length-1)*150)+130;
//		}
//		
//		if(potentialMove.getApptStart() >= firstApptStart+gap || potentialMove.getApptEnd() >= firstApptEnd+gap){
//			return true;
//		}
		
		
		
		return false;
	}
	
	private boolean isHardAppt(Optimizable o){
		return o.getApptStart() == o.getApptEnd();
	}
	
}
