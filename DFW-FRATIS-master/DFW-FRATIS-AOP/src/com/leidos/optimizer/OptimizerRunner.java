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

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import com.leidos.optimizer.data.LegMove;
import com.leidos.optimizer.data.Move;
import com.leidos.optimizer.data.Optimizable;
import com.leidos.optimizer.data.TruckLoad;

public class OptimizerRunner {

	public String fileLocation = "C:\\test\\saic optimization_20141124_140008_evt.csv";
	public String emptyLocation = "C:\\test\\cos cty fratis_20141124_140002_evt.csv";
	public String outputFile = "Move_Optimization.csv";
	private int routesNeeded = 40;
	private OptimizerFileReader fileReader = new OptimizerFileReader();
	
	public static void main(String[] args) throws IOException {
		new OptimizerRunner().run();

	}
	
	public OptimizerRunner(){
		
	}
	
	public OptimizerRunner(String loads, String empties, String output, int routesNeeded){
		this.fileLocation = loads;
		this.emptyLocation = empties;
		this.outputFile = output;
		this.routesNeeded = routesNeeded;
	}
	
	
	
	public void run() throws IOException {
		List<Optimizable> moves = new ArrayList<Optimizable>();
		List<Move> empties = new ArrayList<Move>();
		moves.addAll(fileReader.readFile(fileLocation));

		empties.addAll(fileReader.readEmptyFile(emptyLocation));
		System.out.println("Working with [" + moves.size() + "] moves");
		System.out.println("get line 14 :" + " " + moves.size());

		Collections.sort(moves, new MoveIDComparator());
		// send both legMove and moves to the method moveCapture so they can be
		// iterated throgh ad get the list items out and brake then down etc

		List<Optimizable> moveList = doLegMoveCreation(moves);

		// MoveSorter(moves, empties);

		Collections.sort(moveList, new MoveTimeComparator());
		MoveOptimizerImpl opt = new MoveOptimizerImpl(routesNeeded);
		TruckLoad trucks = opt.optimizeMovesTruck(moveList, empties);

		writeFile(trucks);
	}

	private void printEmptyMovesList(List<Move> empties) {
		for (Optimizable o : empties) {
			System.out.println(o.toString());
		}
	}

	private void printMovesList(List<Optimizable> moves) {
		for (Optimizable o : moves) {
			System.out.println(o.toString());
		}
	}

	
	protected List<Optimizable> doLegMoveCreation(List<Optimizable> moves2) {

		List<Optimizable> moves = new ArrayList<Optimizable>();
		boolean inleg = false;
		Optimizable lastmove = null;
		List<Move> legmove = new ArrayList<Move>();
		for (Optimizable m : moves2) {
			String mId = m.getId();

			if (lastmove != null) {
				String moveId = lastmove.getId();
				if (moveId.equalsIgnoreCase(mId)) {
					legmove.add((Move) lastmove);
					inleg = true;

				} else {
					if (inleg) {
						legmove.add((Move) lastmove);
						LegMove placeHolder = new LegMove();
						placeHolder.setId(legmove.get(0).getId());
						placeHolder.setFromTerminal(legmove.get(0)
								.getFromTerminal());
						placeHolder.setToTerminal(legmove.get(
								legmove.size() - 1).getToTerminal());
						placeHolder.setLegMove(legmove);
						placeHolder.setApptStart(getStartTime(legmove));
						placeHolder.setApptEnd(getEndTime(legmove));

						moves.add(placeHolder);
						legmove = new ArrayList<Move>();
						inleg = false;
					} else {
						moves.add((Move) lastmove);
					}
				}
			}
			lastmove = m;
		}

		return moves;

	}

	private int getStartTime(List<Move> move) {

		List<Move> legMove = new ArrayList<Move>();
		Move startMoves = new LegMove();
		int timeCatch;
		int timeCatchHolder = 0;

		for (int i = 0; i < move.size(); i++) {
			startMoves = move.get(i);
			timeCatch = move.get(i).getApptStart();

			if (timeCatch > timeCatchHolder) {
				timeCatchHolder = timeCatch;
			}
		}

		return timeCatchHolder;
	}

	private int getEndTime(List<Move> move) {
		List<Move> legMove = new ArrayList<Move>();
		Move endMoves = new LegMove();
		int timeCatch;
		int timeCatchHolder = 0;

		for (int i = 0; i < move.size(); i++) {
			endMoves = move.get(i);
			timeCatch = move.get(i).getApptEnd();

			if (timeCatch > timeCatchHolder) {
				timeCatchHolder = timeCatch;
			}
		}

		return timeCatchHolder;
	}

	private void writeFile(TruckLoad truck) throws IOException {
		int i = 0;

		File file = new File(outputFile);
		file.createNewFile();
		try {
			PrintWriter wr = new PrintWriter(new FileWriter(file));
			
			wr.println("Load File: "+fileLocation.substring(fileLocation.lastIndexOf('\\')+1, fileLocation.length()));
			wr.println("Empty File: "+emptyLocation.substring(emptyLocation.lastIndexOf('\\')+1, emptyLocation.length()));
			wr.println("\nThe Number of matched moves from COS file = "
					+ truck.getEmptiesAssigned());
			wr.println("The Number of matched moves from Optimization file  = "
					+ truck.getLoadsAssigned() );
			wr.println();
			wr.println("List of unassigned orders: " + truck.getUnassignedOrders().size());


			wr.println("Count,ID,ContainerNumber,From,From City,To,To City,Start,End");
			for(Optimizable o:truck.getUnassignedOrders()){
				wr.println(i+","+o.toCSVString());
				i++;
			}

			wr.println();
			wr.println();
			
			
			wr.write("List of assigned orders: \n\n");

			int j = 0;

			wr.println("Count,ID,ContainerNumber,From,From City,To,To City,Start,End");
			for (Truck t : truck.getAssignedTruckMoves()) {
				i=0;
				wr.println("Route #"+j);
				for (Optimizable o : t.getMoves()) {
					wr.println(i+","+o.toCSVString());
					i++;
				}
				j++;
				wr.println();
			}
			
			wr.close();

		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

	}



}
