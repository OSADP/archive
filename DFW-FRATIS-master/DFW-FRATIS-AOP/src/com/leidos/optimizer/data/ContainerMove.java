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

public class ContainerMove extends Move implements Serializable {
  private static final long serialVersionUID = 1L;
  private String originRamp;
  private String destRamp;
  private String hazardous;
  private String billOfLading;
  private String lorE;
  private String lastEventLocation;
  private String lastEvent;
  private Date lastEventDate;
  

  public ContainerMove(){
    super();
  }



  @Override
  public String getType(){
    return TYPE_CROSSTOWN;
  }

  public String getBillOfLading() {
    return billOfLading;
  }

  public void setBillOfLading(String BillOfLading) {
    this.billOfLading = BillOfLading;
  }

  public String getLorE() {
    return lorE;
  }

  public void setLorE(String LorE) {
    this.lorE = LorE;
  }

  public String getDestRamp() {
    return destRamp;
  }

  public void setDestRamp(String destRamp) {
    this.destRamp = destRamp;
  }

  public String getHazardous() {
    return hazardous;
  }

  public void setHazardous(String hazardous) {
    this.hazardous = hazardous;
  }

  public String getOriginRamp() {
    return originRamp;
  }

  public void setOriginRamp(String originRamp) {
    this.originRamp = originRamp;
  }

  public String getLastEvent() {
    return lastEvent;
  }

  public void setLastEvent(String lastEvent) {
    this.lastEvent = lastEvent;
  }

  public Date getLastEventDate() {
    return lastEventDate;
  }

  public void setLastEventDate(Date lastEventDate) {
    this.lastEventDate = lastEventDate;
  }

  public String getLastEventLocation() {
    return lastEventLocation;
  }

  public void setLastEventLocation(String lastEventLocation) {
    this.lastEventLocation = lastEventLocation;
  }

  @Override
  public int hashCode() {
    int hash = 0;
    hash += (this.getId() != null ? this.getId().hashCode() : 0);
    return hash;
  }

  @Override
  public boolean equals(Object object) {
    // TODO: Warning - this method won't work in the case the id fields are not set
    if (!(object instanceof ContainerMove)) {
      return false;
    }
    ContainerMove other = (ContainerMove) object;
    if ((this.getId() == null && other.getId() != null) || (this.getId() != null && !this.getId().equals(other.getId()))) {
      return false;
    }
    return true;
  }

  @Override
  public String toString() {
    return "Move: " + getId() + " From: ["
			+ getFromTerminal().getName() + "] TO: ["
			+ getToTerminal().getName() + "] ApptStart: ["+getApptStart()+"] ApptEnd: ["+getApptEnd()+"] \r\n";
  }
  
  public String toCSVString(){
	  StringBuilder sb = new StringBuilder();
	  sb.append(getId());
	  sb.append(",");
	  sb.append(getContainerNumber());
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
	  return sb.toString();
  }



@Override
public Date getDateCreated() {
	// TODO Auto-generated method stub
	return null;
}



}
