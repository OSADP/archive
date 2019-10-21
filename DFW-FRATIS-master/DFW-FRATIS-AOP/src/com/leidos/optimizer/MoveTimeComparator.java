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

import java.util.Comparator;

import com.leidos.optimizer.data.Optimizable;

public class MoveTimeComparator implements Comparator<Optimizable> {

	@Override
	public int compare(Optimizable o1, Optimizable o2) {
		int o1Window = o1.getApptEnd()-o1.getApptStart();
		int o2Window = o2.getApptEnd() - o2.getApptStart();
		
		if(o1Window == 0 && o2Window == 0){
			if(o1.getApptStart() < o2.getApptStart()){
				return -1;
			}else if (o1.getApptStart() > o2.getApptStart()){
				return 1;
			}else{
				return 0;
			}
		}
		
		if( o1Window == 0 && o2Window > 0){
			return -1;
		}
		
		if(o1Window > 0 && o2Window == 0){
			return 1;
		}
		
		if(o1Window > 0 && o2Window > 0){
			//neither hard appt.  Do Normal compar
			if(o1.getApptStart() < o2.getApptStart()){
				return -1;
			}else if (o1.getApptStart() > o2.getApptStart()){
				return 1;
			}else{
				if(o1.getApptEnd() < o2.getApptEnd()){
					return -1;
				}else if(o1.getApptEnd() == o2.getApptEnd()){
					return 0;
				}else{
					return 1;
				}
				
			}
		}
		
		
		System.out.println("Didn't hit any valid case?");
		return 0;
//		if(o1.getApptStart() < o2.getApptStart()){
//			return -1;
//		}else if (o1.getApptStart() > o2.getApptStart()){
//			return 1;
//		}else{
//			if(o1.getApptEnd() < o2.getApptEnd()){
//				return -1;
//			}else if(o1.getApptEnd() == o2.getApptEnd()){
//				return 0;
//			}else{
//				return 1;
//			}
//			
//		}
	}
	

}
