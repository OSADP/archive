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


package com.leidos.optimizer.data;

import java.util.List;

import com.leidos.optimizer.Truck;



public class TruckLoad {
	
	private int routeCounter;
	private int loadsAssigned;
	private int emptiesAssigned;
	private List<Optimizable> unassignedOrders;
	private List<Truck> assignedTruckMoves;
	
	public int getRouteCounter() {
		return routeCounter;
	}
	public void setRouteCounter(int routeCounter) {
		this.routeCounter = routeCounter;
	}
	public int getLoadsAssigned() {
		return loadsAssigned;
	}
	public void setLoadsAssigned(int loadsAssigned) {
		this.loadsAssigned = loadsAssigned;
	}
	public int getEmptiesAssigned() {
		return emptiesAssigned;
	}
	public void setEmptiesAssigned(int emptiesAssigned) {
		this.emptiesAssigned = emptiesAssigned;
	}
	public List<Optimizable> getUnassignedOrders() {
		return unassignedOrders;
	}
	public void setUnassignedOrders(List<Optimizable> unassignedOrders) {
		this.unassignedOrders = unassignedOrders;
	}
	public List<Truck> getAssignedTruckMoves() {
		return assignedTruckMoves;
	}
	public void setAssignedTruckMoves(List<Truck> assignedTruckMoves) {
		this.assignedTruckMoves = assignedTruckMoves;
	}
	
}
