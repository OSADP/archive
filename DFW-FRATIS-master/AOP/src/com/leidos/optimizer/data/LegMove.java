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

import java.io.Serializable;
import java.util.Date;
import java.util.List;





public class LegMove extends Move implements Serializable {

	
	/**
	 * 
	 */
	private static final long serialVersionUID = -5577070282081313154L;
	private List<Move> legMove;
	
	
	public List<Move> getLegMove() {
		return legMove;
	}
	public void setLegMove(List<Move> legMove) {
		this.legMove = legMove;
	} 
	
	public String toString(){

		String s = "LegMove: " + getId()+ " ApptStart: ["+getApptStart()+"] ApptEnd: ["+getApptEnd()+"]\r\n";
		for (Move o : legMove) {
			s+=("\t" + o.toString() + "\r\n");
		}
		
		return s;
	}
	@Override
	  public String toCSVString(){
		  StringBuilder sb = new StringBuilder();
		  sb.append(getId());
		  sb.append(",");
		  sb.append(",");
		  sb.append(getFromTerminal().getName());
		  sb.append(",");
		  sb.append(getFromTerminal().getCity());
		  sb.append(",");
		  sb.append(getToTerminal().getName());
		  sb.append(",");
		  sb.append(getToTerminal().getCity());
		  sb.append(",");
		  sb.append(getApptStart());
		  sb.append(",");
		  sb.append(getApptEnd());
		  sb.append("\r\n");
		  for(Move m:getLegMove()){
			  sb.append(",");
			  sb.append(",");
			  sb.append(m.getContainerNumber());
			  sb.append(",");
			  sb.append(m.getFromTerminal().getName());
			  sb.append(",");
			  sb.append(m.getFromTerminal().getCity());
			  sb.append(",");
			  sb.append(m.getToTerminal().getName());
			  sb.append(",");
			  sb.append(m.getToTerminal().getCity());
			  sb.append(",");
			  sb.append(m.getApptStart());
			  sb.append(",");
			  sb.append(m.getApptEnd());
			  sb.append("\r\n");
		  }
		  return sb.toString();
	  }
	@Override
	public Date getDateCreated() {
		// TODO Auto-generated method stub
		return null;
	}


	
}



