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

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import com.leidos.optimizer.data.Move;
import com.leidos.optimizer.data.Optimizable;
import com.leidos.optimizer.data.TruckLoad;

/**
 * This class is responsible for assigning proposed carriers to all the moves
 * provided. The carrier for the move is determined by their goal move
 * percentage and the capacity the carrier has for the day. If all carriers are
 * utilized to capacity and their are moves remaining, those moves will not have
 * a proposed carrier.
 * 
 * @author cassadyja
 */
public class MoveOptimizerImpl {
	private int loadCounter= 0;
	private int emptyCounter= 0;
	private final int MAX_MOVE_LENGTH = 4;
	private int routesNeeded;
	
	private TimeGapChecker timeChecker = new TimeGapChecker();
	
	public MoveOptimizerImpl(int routesNeeded) {
		this.routesNeeded = routesNeeded;
	}


	public TruckLoad optimizeMovesTruck(List<Optimizable> moves,
			List<Move> empties) {
		loadCounter ++;
		List<Truck> retMoves = new ArrayList <Truck>();
		Optimizable move = moves.get(0);
		int j = 0, k = 0, l = 0;
		moves.remove(0);
		// List<Move> route = new ArrayList<Move>();
		Truck t = new Truck();
		t.addMove(move);
		Optimizable toTerminal = move;
		boolean matchFound = false;

		while (l < moves.size()) {

			while (j < moves.size() && !matchFound) {

				Optimizable fromTerminal = moves.get(j);
				if (toTerminal
						.getToTerminal()
						.getName()
						.equalsIgnoreCase(
								fromTerminal.getFromTerminal().getName())
						&& (timeChecker.checkTimeDifference(t, fromTerminal))) {
					if(t.getMoves().length > 1){
						loadCounter = loadCounter + 1;
					}else{
						loadCounter += 2;
					}

					matchFound = true;
					moves.remove(j);
					t.addMove(fromTerminal);
					toTerminal = fromTerminal;
					j = 0;
				} else {
					j++;
				}
			}

			j = 0;

			if(!matchFound){
				while (k < empties.size()) {
	
					Optimizable fromTerminalEmpties = empties.get(k);
	
					if (toTerminal
							.getToTerminal()
							.getName()
							.equalsIgnoreCase(
									fromTerminalEmpties.getFromTerminal().getName())) {
						emptyCounter++;
						matchFound = true;
						empties.remove(k);
						t.addMove(fromTerminalEmpties);
						toTerminal = fromTerminalEmpties;
						break;
	
					} else {
						k++;
					}
				}
			}
			k = 0;

			if(!matchFound){
				Optimizable sortCatcher = findZipCodeMatch(t, toTerminal, moves,
						empties);
	
				if (sortCatcher != null) {
					toTerminal = sortCatcher;
					matchFound = true;
					t.addMove(sortCatcher);
				}
			}
			
			

			if(!matchFound){
				Optimizable cityCatcher = findCityMatch(t, toTerminal, moves, empties);
				if (cityCatcher != null) {
					toTerminal = cityCatcher;
					matchFound = true;
					t.addMove(cityCatcher);
				}
			}
			
			
			if (!matchFound) {
				if (t.getMoves().length > 1) {
					retMoves.add(t);
					
				} else {
					moves.add(0, toTerminal);
					l++;
				}
				Collections.sort(moves, new MoveTimeComparator());
				toTerminal = moves.get(l);
				moves.remove(l);
				t = new Truck();
				t.addMove(toTerminal);
			}  else {
				if(t.getMoves().length == MAX_MOVE_LENGTH){
					retMoves.add(t);
					t = new Truck();
					Collections.sort(moves, new MoveTimeComparator());
					toTerminal = moves.get(0);
					moves.remove(0);
					t.addMove(toTerminal);
					
				}
				matchFound = false;

			}

		}
		
		List<Truck> otherHardAppts = checkUnassignedHardAppts(moves);
		retMoves.addAll(otherHardAppts);

		List<Truck> routeFiller = fillNeededRoutes(moves, retMoves.size());
		retMoves.addAll(routeFiller);
		
		
		
		TruckLoad  truckLoad= new TruckLoad();
		truckLoad.setAssignedTruckMoves(retMoves);
		truckLoad.setEmptiesAssigned(emptyCounter);
		truckLoad.setLoadsAssigned(loadCounter);
		truckLoad.setUnassignedOrders(moves);

		return truckLoad;
	}
	
	private List<Truck> fillNeededRoutes(List<Optimizable> moves, int currentRouteCount){
		List<Truck> ret = new ArrayList<Truck>();
		List<Optimizable> toRemove = new ArrayList<Optimizable>();
		int neededRoutes = routesNeeded - currentRouteCount;
		if(moves.size() < neededRoutes){
			neededRoutes = moves.size();
		}
		for(int i=0;i<neededRoutes;i++){
			Truck t = new Truck();
			t.addMove(moves.get(i));
			toRemove.add(moves.get(i));
			ret.add(t);
		}
		moves.removeAll(toRemove);
		return ret;
	}

	private List<Truck> checkUnassignedHardAppts(List<Optimizable> moves) {
		List<Truck> ret = new ArrayList<Truck>();
		List<Optimizable> toRemove = new ArrayList<Optimizable>();
		for(Optimizable o:moves){
			if(o.getApptEnd() - o.getApptStart() == 0){
				Truck t = new Truck();
				t.addMove(o);
				toRemove.add(o);
				ret.add(t);
			}
		}
		moves.removeAll(toRemove);
		return ret;
	}


	public Optimizable findZipCodeMatch(Truck t, Optimizable toTerminal,
			List<Optimizable> moves, List<Move> empties) {

		String mHolder = toTerminal.getToTerminal().getZip();

		for (int m = 0; m < moves.size(); m++) {
			Optimizable zipHolder = moves.get(m);
			String moveLocation = moves.get(m).getFromTerminal().getZip();

			if (moveLocation.equalsIgnoreCase(mHolder)
					&& timeChecker.checkTimeDifference(t, zipHolder)) {
				moves.remove(m);
				loadCounter = loadCounter + 1;
				return zipHolder;
			}

		}

		for (int n = 0; n < empties.size(); n++) {
			Optimizable emptyHolder = empties.get(n);
			String emptyLocation = empties.get(n).getFromTerminal().getZip();
			if (mHolder.equalsIgnoreCase(emptyLocation)
					&& timeChecker.checkTimeDifference(t, emptyHolder)) {
				empties.remove(n);
				emptyCounter = emptyCounter + 1;
				return emptyHolder;
			}

		}
		return null;
	}

	public Optimizable findCityMatch(Truck t, Optimizable toTerminal,
			List<Optimizable> moves, List<Move> empties) {

		String mHolder = toTerminal.getToTerminal().getCity();

		for (int m = 0; m < moves.size(); m++) {
			Optimizable cityHolder = moves.get(m);
			String moveLocation = moves.get(m).getFromTerminal().getCity();

			if (moveLocation.equalsIgnoreCase(mHolder)
					&& timeChecker.checkTimeDifference(t, cityHolder)) {
				moves.remove(m);
				loadCounter = loadCounter + 1;
				return cityHolder;
			}

		}

		for (int n = 0; n < empties.size(); n++) {
			Optimizable emptyHolder = empties.get(n);
			String emptyLocation = empties.get(n).getFromTerminal().getCity();
			if (mHolder.equalsIgnoreCase(emptyLocation)
					&& timeChecker.checkTimeDifference(t, emptyHolder)) {
				empties.remove(n);
				emptyCounter = emptyCounter + 1;
				return emptyHolder;
			}
		}
		return null;
	}

}
