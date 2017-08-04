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


public class Terminal implements Serializable {

  private static final long serialVersionUID = 1L;
  private int loadTime;
  private int unloadTime;
  private int inGateTime;
  private int outGateTime;
  private String city;
  private String state;
  private String zip;
  private String name;
  private Long id;
  
  
  public Terminal() {
  }


  public Long getId() {
    return id;
  }

  public void setId(Long id) {
    this.id = id;
  }

  public int getInGateTime() {
    return inGateTime;
  }

  public void setInGateTime(int val) {
    this.inGateTime = val;
  }

  public int getLoadTime() {
    return loadTime;
  }

  public void setLoadTime(int val) {
    this.loadTime = val;
  }



  public int getOutGateTime() {
    return outGateTime;
  }

  public void setOutGateTime(int val) {
    this.outGateTime = val;
  }

  public int getUnloadTime() {
    return unloadTime;
  }

  public void setUnloadTime(int val) {
    this.unloadTime = val;
  }

  public String getName() {
	    return name;
	  }

  public void setName(String name) {
    this.name = name;
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
    if (!(object instanceof Terminal)) {
      return false;
    }
    Terminal other = (Terminal) object;
    if ((this.getId() == null && other.getId() != null) || (this.getId() != null && !this.getId().equals(other.getId()))) {
      return false;
    }
    return true;
  }

  public String getCity() {
	return city;
}

public void setCity(String city) {
	this.city = city;
}

public String getState() {
	return state;
}

public void setState(String state) {
	this.state = state;
}

public String getZip() {
	return zip;
}

public void setZip(String zip) {
	this.zip = zip;
}

@Override
  public String toString() {
    return this.getClass().getName() + "[id=" + this.getId() + "]";
  }
  
  
  
  
}



