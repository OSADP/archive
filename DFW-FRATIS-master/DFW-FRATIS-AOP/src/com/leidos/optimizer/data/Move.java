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



public abstract class Move implements Serializable, Optimizable {
  private static final long serialVersionUID = 1L;
  
  // Move types
  public static final String TYPE_CROSSTOWN = "Crosstown";
  public static final String TYPE_INDUSTRY = "Industry";
  public static final String TYPE_CHASSIS = "Chassis";
  // Statuses
  public static final String STATUS_UNASSIGNED = "Unassigned";
  public static final String STATUS_ASSIGNED = "Assigned";
  public static final String STATUS_ACCEPTED = "Accepted";
  public static final String STATUS_REJECTED = "Rejected";
  public static final String STATUS_ENROUTE = "Enroute";
  public static final String STATUS_COMPLETED = "Completed";

  private String id;
  private String type;
  private String status;
  private Terminal toTerminal;
  private Terminal fromTerminal;
  private Date unloaded;
  private Date cutoffTime;
  private Date outgate;
  private Date ingate;
  private Date availabilityDate;
  private int apptStart;
  private int apptEnd;
  private String legNumber; 
  private String containerNumber;
  
  
  public String getLegNumber() {
	return legNumber;
}

public void setLegNumber(String legNumber) {
	this.legNumber = legNumber;
}

private Date estimatedUnloaded;
  private Date estimatedOutgate;
  private Date estimatedIngate;
  private Date estimatedLoaded;

  private String link;

  public Move() {
  }



  public String getId() {
    return id;
  }

  public void setId(String id) {
    this.id = id;
  }



  public Date getCutoffTime() {
    return cutoffTime;
  }

  public void setCutoffTime(Date cutoffTime) {
    this.cutoffTime = cutoffTime;
  }

  public Terminal getFromTerminal() {
    return fromTerminal;
  }

  public void setFromTerminal(Terminal fromTerminal) {
    this.fromTerminal = fromTerminal;
  }

  public Date getIngate() {
    return ingate;
  }

  public void setIngate(Date ingate) {
    this.ingate = ingate;
  }

  public String getLink() {
    return link;
  }

  public void setLink(String link) {
    this.link = link;
  }

  public Date getOutgate() {
    return outgate;
  }

  public void setOutgate(Date outgate) {
    this.outgate = outgate;
  }



  public String getStatus() {
    return status;
  }

  public void setStatus(String status) {
    this.status = status;
  }

  public Terminal getToTerminal() {
    return toTerminal;
  }

  public void setToTerminal(Terminal toTerminal) {
    this.toTerminal = toTerminal;
  }

  public String getType() {
    return type;
  }

  public Date getUnloaded() {
    return unloaded;
  }

  public void setUnloaded(Date unloaded) {
    this.unloaded = unloaded;
  }

    public Date getEstimatedIngate() {
        return estimatedIngate;
    }

    public void setEstimatedIngate(Date estimatedIngate) {
        this.estimatedIngate = estimatedIngate;
    }

    public Date getEstimatedLoaded() {
        return estimatedLoaded;
    }

    public void setEstimatedLoaded(Date estimatedLoaded) {
        this.estimatedLoaded = estimatedLoaded;
    }

    public Date getEstimatedOutgate() {
        return estimatedOutgate;
    }

    public void setEstimatedOutgate(Date estimatedOutgate) {
        this.estimatedOutgate = estimatedOutgate;
    }

    public Date getEstimatedUnloaded() {
        return estimatedUnloaded;
    }

    public void setEstimatedUnloaded(Date estimatedUnloaded) {
        this.estimatedUnloaded = estimatedUnloaded;
    }

    public Date getAvailabilityDate() {
        return availabilityDate;
    }

    public void setAvailabilityDate(Date availabilityDate) {
        this.availabilityDate = availabilityDate;
    }

    

  

  public int getApptStart() {
		return apptStart;
	}

	public void setApptStart(int apptStart) {
		this.apptStart = apptStart;
	}

	public int getApptEnd() {
		return apptEnd;
	}

	public void setApptEnd(int apptEnd) {
		this.apptEnd = apptEnd;
	}
	
	

public String getContainerNumber() {
		return containerNumber;
	}

	public void setContainerNumber(String containerNumber) {
		this.containerNumber = containerNumber;
	}

@Override
  public int hashCode() {
    int hash = 0;
    hash += (id != null ? id.hashCode() : 0);
    return hash;
  }

  @Override
  public boolean equals(Object object) {
    // TODO: Warning - this method won't work in the case the id fields are not set
    if (!(object instanceof Move)) {
      return false;
    }
    Move other = (Move) object;
    if ((this.id == null && other.id != null) || (this.id != null && !this.id.equals(other.id))) {
      return false;
    }
    return true;
  }

//  @Override
//  public String toString() {
//    return "com.saic.ctip.imex.data.Move[id=" + id + "]";
//  }

}
