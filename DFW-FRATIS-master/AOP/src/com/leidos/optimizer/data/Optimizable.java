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

import java.util.Date;

/**
 *
 * @author cassadyja
 */
public interface Optimizable {
  public Terminal getToTerminal();
  public Date getCutoffTime();
  public Terminal getFromTerminal();
  public Date getEstimatedIngate();
  public Date getEstimatedLoaded();
  public Date getEstimatedOutgate();
  public Date getEstimatedUnloaded();
  public Date getDateCreated();
  public void setEstimatedIngate(Date estimatedIngate);
  public void setEstimatedLoaded(Date estimatedLoaded);
  public void setEstimatedOutgate(Date estimatedOutgate);
  public void setEstimatedUnloaded(Date estimatedUnloaded);
  public String getId();
  public void setLink(String link);
  public String getLink();
  public Date getAvailabilityDate();
  public int getApptStart();
  public int getApptEnd();
  public String getLegNumber();
  
  public String toCSVString();
//  public MotorCarrier getAssignedCarrier();
//  public void setAssignedCarrier(MotorCarrier assignedCarrier);
//  public void setCutoffTime(Date cutoffTime);
//  public void setFromTerminal(Terminal fromTerminal);
//  public Date getIngate();
//  public void setIngate(Date ingate);
//  public String getLink();

//  public Date getOutgate();
//  public void setOutgate(Date outgate);
//  public MotorCarrier getProposedCarrier();
//  public String getStatus();
//  public void setStatus(String status);
//  public void setToTerminal(Terminal toTerminal);
//  public String getType();
//  public void setType(String type);
//  public Date getUnloaded();
//  public void setUnloaded(Date unloaded);

//  public void setId(String id);

}
