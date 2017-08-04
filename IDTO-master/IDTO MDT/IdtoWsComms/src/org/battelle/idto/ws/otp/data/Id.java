
package org.battelle.idto.ws.otp.data;

import java.util.List;

public class Id{
   	private String agencyId;
   	private String id;

   	public Id(String agency, String id)
   	{
   		this.agencyId = agency;
   		this.id = id;
   	}
   	
 	public String getAgency(){
		return this.agencyId;
	}
	public void setAgency(String agency){
		this.agencyId = agency;
	}
 	public String getId(){
		return this.id;
	}
	public void setId(String id){
		this.id = id;
	}
}
