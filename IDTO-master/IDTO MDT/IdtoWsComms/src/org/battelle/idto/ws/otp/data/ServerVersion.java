
package org.battelle.idto.ws.otp.data;

public class ServerVersion{
   	private String commit;
   	private Number incremental;
   	private Number major;
   	private Number minor;
   	private String qualifier;
   	private Number uid;
   	private String version;

 	public String getCommit(){
		return this.commit;
	}
	public void setCommit(String commit){
		this.commit = commit;
	}
 	public Number getIncremental(){
		return this.incremental;
	}
	public void setIncremental(Number incremental){
		this.incremental = incremental;
	}
 	public Number getMajor(){
		return this.major;
	}
	public void setMajor(Number major){
		this.major = major;
	}
 	public Number getMinor(){
		return this.minor;
	}
	public void setMinor(Number minor){
		this.minor = minor;
	}
 	public String getQualifier(){
		return this.qualifier;
	}
	public void setQualifier(String qualifier){
		this.qualifier = qualifier;
	}
 	public Number getUid(){
		return this.uid;
	}
	public void setUid(Number uid){
		this.uid = uid;
	}
 	public String getVersion(){
		return this.version;
	}
	public void setVersion(String version){
		this.version = version;
	}
}
