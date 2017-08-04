
package org.battelle.idto.ws.otp.data;

public class ServerInfo{
   	private String cpuName;
   	private Number nCores;
   	private ServerVersion serverVersion;

 	public String getCpuName(){
		return this.cpuName;
	}
	public void setCpuName(String cpuName){
		this.cpuName = cpuName;
	}
 	public Number getNCores(){
		return this.nCores;
	}
	public void setNCores(Number nCores){
		this.nCores = nCores;
	}
 	public ServerVersion getServerVersion(){
		return this.serverVersion;
	}
	public void setServerVersion(ServerVersion serverVersion){
		this.serverVersion = serverVersion;
	}
}
