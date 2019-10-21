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


package com.leidos.optimizer.ui;


import javax.swing.JFrame;

public class Optimizer extends JFrame{
	
	public static void main(String[] args){
		new Optimizer().init();
	}
	
	
	public Optimizer(){
		this.setDefaultCloseOperation(EXIT_ON_CLOSE);
//		this.addWindowListener(new WindowAdapter() {
//			@Override
//			public void windowClosing(WindowEvent arg0) {
//				if (dbServer != null) {
//					dbServer.shutDown();
//				}
//			}
//
//		});
		
	}
	
	private void init(){
		this.setContentPane(new DisplayPanel());
		this.setSize(425, 300);
		this.setLocation(75, 75);
		this.setVisible(true);

	}
	
	

}
