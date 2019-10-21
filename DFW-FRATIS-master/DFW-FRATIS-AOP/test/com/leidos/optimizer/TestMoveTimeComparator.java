package com.leidos.optimizer;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import com.leidos.optimizer.data.Optimizable;

public class TestMoveTimeComparator {

	
	
	public static void main(String[] args) throws IOException{
		OptimizerRunner runner = new OptimizerRunner();
		OptimizerFileReader fileReader = new OptimizerFileReader();
		List<Optimizable> moves = new ArrayList<Optimizable>(); 
		moves.addAll(fileReader.readFile("C:\\test\\saic optimization_20140818_140116_evt.csv"));
		MoveTimeComparator mtc = new MoveTimeComparator();
		Collections.sort(moves, new MoveIDComparator());
		
		moves = runner.doLegMoveCreation(moves);
		
		Collections.sort(moves, mtc);
		
		for(Optimizable o: moves){
			System.out.println(o.toString());
		}
		System.out.println("Done");
		
		
		
	}
	
}
