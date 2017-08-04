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

import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Properties;

import javax.swing.Box;
import javax.swing.BoxLayout;
import javax.swing.JButton;
import javax.swing.JFileChooser;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JTextField;

import com.leidos.optimizer.OptimizerRunner;

public class DisplayPanel extends JPanel {

	private JTextField loadFile = new JTextField(20);
	private JTextField emptyFile = new JTextField(20);
	private JTextField outputFile = new JTextField(20);
	private JTextField routes = new JTextField(20);
	
	private JButton loadButton = new JButton("Load File");
	private JButton emptyButton = new JButton("Empty File");
	private JButton outputButton = new JButton("Opt Plan File");

	private JButton runButton = new JButton("Execute Optimizer");
	private File loads = null;
	private File empties = null;
	private File output = null;
	private int routesNeeded;
	
	JFileChooser chooser = new JFileChooser(System.getProperty("user.dir"));
	SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd");
	Properties p = new Properties();
	
	private class MyActionListener implements ActionListener{

		@Override
		public void actionPerformed(ActionEvent evt) {
			if(evt.getSource().equals(loadButton)){
				int result = chooser.showOpenDialog(DisplayPanel.this);
				if(result == JFileChooser.APPROVE_OPTION){
					loads = chooser.getSelectedFile();
					loadFile.setText(loads.getAbsolutePath());
					setOutputFile(loads);
					storeProps();
				}
			}else if(evt.getSource().equals(emptyButton)){
				int result = chooser.showOpenDialog(DisplayPanel.this);
				if(result == JFileChooser.APPROVE_OPTION){
					empties = chooser.getSelectedFile();
					emptyFile.setText(empties.getAbsolutePath());
					setOutputFile(empties);
				}				
			}else if(evt.getSource().equals(outputButton)){
				int result = chooser.showOpenDialog(DisplayPanel.this);
				if(result == JFileChooser.APPROVE_OPTION){
					output = chooser.getSelectedFile();
					outputFile.setText(output.getAbsolutePath());
				}					
			}else if(evt.getSource().equals(runButton)){
				if (routes.getText() != null && !"".equals(routes.getText())){
					routesNeeded = Integer.parseInt(routes.getText());
				}
				OptimizerRunner runner = new OptimizerRunner(loadFile.getText(),emptyFile.getText(), outputFile.getText(),routesNeeded);
				try {
					runner.run();
					JOptionPane.showMessageDialog(DisplayPanel.this, "Optimization Complete");
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				
			}
			
		}
		
		private void setOutputFile(File f){
			if(outputFile.getText().length() == 0){
				outputFile.setText(f.getParent()+"\\saic_opt_output_"+sdf.format(new Date())+".csv");
			}
		}
		
	}
	
	
	public DisplayPanel(){
		loadProps();
		
		init();
	}
	
	
	private void storeProps(){
		p.setProperty("FileDIR", loads.getParent());
		try {
			File f = new File("optimizer.properties");
			if(!f.exists()){
				f.createNewFile();
			}
			p.store(new FileWriter(f), "");
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	private void loadProps(){
		File f = new File("optimizer.properties");
		if(f.exists()){
			try {
				p.load(new FileReader(f));
				String dir = p.getProperty("FileDIR");
				chooser.setCurrentDirectory(new File(dir));
			} catch (FileNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
	
	
	private void init(){
		this.setLayout(new BoxLayout(this, BoxLayout.Y_AXIS));
		ActionListener al = new MyActionListener();
		
		loadButton.addActionListener(al);
		emptyButton.addActionListener(al);
		outputButton.addActionListener(al);
		runButton.addActionListener(al);

		JLabel loadLbl =   new JLabel("Loads:      ");
		JLabel emptyLbl =  new JLabel("Empties:   ");
		JLabel outputLbl = new JLabel("Opt. Plan: ");
		JLabel routeLbl =  new JLabel("Routes:     ");
		
		loadFile.setPreferredSize(new Dimension(200, 25));
		outputFile.setPreferredSize(new Dimension(200, 25));
		emptyFile.setPreferredSize(new Dimension(200, 25));
		routes.setPreferredSize(new Dimension(200, 25));
		
		loadFile.setMaximumSize(new Dimension(200, 25));
		outputFile.setMaximumSize(new Dimension(200, 25));
		emptyFile.setMaximumSize(new Dimension(200, 25));
		routes.setMaximumSize(new Dimension(200, 25));
		routes.setText("40");
		
		
		
		Box loadBox = Box.createHorizontalBox();
		loadBox.add(loadLbl);
		loadBox.add(loadFile);
		loadBox.add(Box.createHorizontalGlue());
		loadBox.add(loadButton);
		loadBox.add(Box.createHorizontalGlue());
		
		Box emptyBox = Box.createHorizontalBox();
		emptyBox.add(emptyLbl);
		emptyBox.add(emptyFile);
		emptyBox.add(Box.createHorizontalGlue());
		emptyBox.add(emptyButton);
		emptyBox.add(Box.createHorizontalGlue());
		
		Box outputBox = Box.createHorizontalBox();
		outputBox.add(outputLbl);
		outputBox.add(outputFile);
		outputBox.add(Box.createHorizontalGlue());
		outputBox.add(outputButton);
		outputBox.add(Box.createHorizontalGlue());
		
		Box routeBox = Box.createHorizontalBox();
		routeBox.add(routeLbl);
		routeBox.add(routes);
		routeBox.add(Box.createHorizontalGlue());		
		
		Box runBox = Box.createHorizontalBox();
		runBox.add(Box.createHorizontalGlue());
		runBox.add(runButton);
		runBox.add(Box.createHorizontalGlue());
		
		this.add(loadBox);
		this.add(Box.createVerticalStrut(5));
		this.add(emptyBox);
		this.add(Box.createVerticalStrut(5));
		this.add(outputBox);
		this.add(Box.createVerticalStrut(5));
		this.add(routeBox);
		this.add(Box.createVerticalGlue());
		this.add(runBox);
		this.add(Box.createVerticalGlue());
		
	}
	
}
