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

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import com.leidos.optimizer.data.ContainerMove;
import com.leidos.optimizer.data.Move;
import com.leidos.optimizer.data.Terminal;

public class OptimizerFileReader {
	private String[] excludeList = new String[]{"CRAFTMADE INTL"};
	
	public List<Move> readFile(String fileName) throws IOException{
		List<Move> legMoves = new ArrayList<Move>();
		File f = new File(fileName);
		BufferedReader br = new BufferedReader(new FileReader(f));
		String s = br.readLine();
		s = br.readLine();
		while (s != null) {
			s = s.replaceAll("\"", "");
			String[] line = s.split(",");
			// Move m = new ContainerMove();
			if(!isExcluded(line[3]) && !isExcluded(line[8])){
				ContainerMove l = new ContainerMove();
				l.setId(line[0]);
				if(line.length > 29){
					l.setContainerNumber(line[29]);
				}
				Terminal ft = new Terminal();
				ft.setName(line[3]);
				ft.setCity(line[5]);
				ft.setState(line[6]);
				ft.setZip(line[7]);
				l.setFromTerminal(ft);
				Terminal t = new Terminal();
				t.setName(line[8]);
				t.setCity(line[10]);
				t.setState(line[11]);
				t.setZip(line[12]);
				l.setToTerminal(t);
				
	
				l.setLegNumber(line[14]);
	
				if (line[19] != null && !"".equals(line[19])
						&& line[19].indexOf(':') > 0) {
					l.setApptStart(Integer.parseInt(line[19].replaceAll(":", "")));
				}
				if (line[20] != null && !"".equals(line[20])) {
					l.setApptEnd(Integer.parseInt(line[20].replaceAll(":", "")));
				}
				legMoves.add(l);
			}

			s = br.readLine();
		}
		return legMoves;
		
	}
	
	
	public List<Move> readEmptyFile(String fileName) throws IOException{
		List<Move> legMoves = new ArrayList<Move>();
		File f = new File(fileName);
		BufferedReader br = new BufferedReader(new FileReader(f));
		String s = br.readLine();
		s = br.readLine();
		while (s != null) {
			s = s.replaceAll("\"", "");
			String[] line = s.split(",");
			// Move m = new ContainerMove();
			if(!isExcluded(line[3]) && !isExcluded(line[8])){
				ContainerMove l = new ContainerMove();
				l.setId(line[0]);
				if(line.length > 30){
					l.setContainerNumber(line[30]);
				}
				Terminal ft = new Terminal();
				ft.setName(line[3]);
				ft.setCity(line[5]);
				ft.setState(line[6]);
				ft.setZip(line[7]);
				l.setFromTerminal(ft);
				Terminal t = new Terminal();
				t.setName(line[8]);
				t.setCity(line[10]);
				t.setState(line[11]);
				t.setZip(line[12]);
				l.setToTerminal(t);
				
	
				l.setLegNumber(line[14]);
	
				if (line[19] != null && !"".equals(line[19])
						&& line[19].indexOf(':') > 0) {
					l.setApptStart(Integer.parseInt(line[19].replaceAll(":", "")));
				}
				if (line[20] != null && !"".equals(line[20])) {
					l.setApptEnd(Integer.parseInt(line[20].replaceAll(":", "")));
				}
				legMoves.add(l);
			}

			s = br.readLine();
		}
		return legMoves;
		
	}	
	
	private boolean isExcluded(String name){
		for(String s:excludeList){
			if(s.equalsIgnoreCase(name)){
				return true;
			}
		}
		
		return false;
	}
}
