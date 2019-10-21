package com.leidos.optimizer;

import org.junit.Test;

import com.leidos.optimizer.data.ContainerMove;

import junit.framework.TestCase;

public class TestTimeGapChecker extends TestCase{

	TimeGapChecker gapChecker = new TimeGapChecker();
	Truck t1 = new Truck();
	Truck t2 = new Truck();
	Truck t3 = new Truck();
	ContainerMove empty = new ContainerMove();
	
	@Override
	public void setUp(){
		empty = new ContainerMove();
		empty.setApptStart(0);
		empty.setApptEnd(0);
		
		ContainerMove cm = new ContainerMove();
		cm.setApptStart(500);
		cm.setApptEnd(500);
		t1.addMove(cm);
		t2.addMove(cm);
		t3.addMove(cm);
		
		
		cm = new ContainerMove();
		cm.setApptStart(700);
		cm.setApptEnd(700);
		
		t2.addMove(cm);
		t2.addMove(empty);
		
		cm = new ContainerMove();
		cm.setApptStart(1000);
		cm.setApptEnd(1000);
		
		t2.addMove(cm);
		t3.addMove(empty);
		
		cm = new ContainerMove();
		cm.setApptStart(900);
		cm.setApptEnd(1200);
		
		t3.addMove(cm);
		t3.addMove(empty);
		
		
	}
	
	@Test
	public void testOneMovePlusNewHardApptGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(700);
		move.setApptEnd(700);
		assertTrue(gapChecker.checkTimeDifference(t1, move));
		
	
		
		
	}

	@Test
	public void testOneMovePlusNewHardApptBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(600);
		move.setApptEnd(600);

		assertFalse(gapChecker.checkTimeDifference(t1, move));
	}
	
	
	@Test
	public void testOneMovePlusNewWindowGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(700);
		move.setApptEnd(1200);
		assertTrue(gapChecker.checkTimeDifference(t1, move));
	}	

	@Test
	public void testOneMovePlusNewWindowBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(530);
		move.setApptEnd(600);
		assertFalse(gapChecker.checkTimeDifference(t1, move));
	}	
	
	
	@Test
	public void testMultiMovePlusNewHardApptGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1130);
		move.setApptEnd(1130);

		assertTrue(gapChecker.checkTimeDifference(t2, move));
	}
	
	@Test
	public void testMultiMovePlusNewHardApptBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1100);
		move.setApptEnd(1100);

		assertFalse(gapChecker.checkTimeDifference(t2, move));
	}	
	
	@Test
	public void testMultiMovePlusNewWindowGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1130);
		move.setApptEnd(1300);

		assertTrue(gapChecker.checkTimeDifference(t2, move));
		
		move.setApptStart(830);
		move.setApptEnd(1300);
		assertTrue(gapChecker.checkTimeDifference(t2, move));
	}	

	
	@Test
	public void testMultiMovePlusNewWindowBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1100);
		move.setApptEnd(1100);

		assertFalse(gapChecker.checkTimeDifference(t2, move));
	}	
	
	
	@Test
	public void testMultiMoveWithEmptyAndWindowPlusNewHardApptGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1300);
		move.setApptEnd(1300);

		assertTrue(gapChecker.checkTimeDifference(t3, move));
	}	
	

	@Test
	public void testMultiMoveWithEmptyAndWindowPlusNewHardApptBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(1030);
		move.setApptEnd(1030);

		assertFalse(gapChecker.checkTimeDifference(t3, move));
	}	
	
	@Test
	public void testMultiMoveWithEmptyAndWindowPlusNewWindowGood(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(800);
		move.setApptEnd(1500);

		assertTrue(gapChecker.checkTimeDifference(t3, move));
	}	

	
	@Test
	public void testMultiMoveWithEmptyAndWindowPlusNewWindowBad(){
		ContainerMove move = new ContainerMove();
		move.setApptStart(800);
		move.setApptEnd(1030);

		assertFalse(gapChecker.checkTimeDifference(t3, move));
	}	
	
	
	@Test
	public void testOneWindowOneEmptyAddOneWindow(){
		Truck truck = new Truck();
		ContainerMove cm = new ContainerMove();
		cm.setApptStart(800);
		cm.setApptEnd(1500);
		truck.addMove(cm);
		truck.addMove(empty);
		cm = new ContainerMove();
		cm.setApptStart(800);
		cm.setApptEnd(1700);
		
		assertTrue(gapChecker.checkTimeDifference(truck, cm));
	}
	
	@Test
	public void testOneWindowOneEmptyAddOneWindowBad(){
		Truck truck = new Truck();
		ContainerMove cm = new ContainerMove();
		cm.setApptStart(800);
		cm.setApptEnd(1100);
		truck.addMove(cm);
		truck.addMove(empty);
		cm = new ContainerMove();
		cm.setApptStart(800);
		cm.setApptEnd(1200);
		
		assertFalse(gapChecker.checkTimeDifference(truck, cm));
	}
	
	
	

}
