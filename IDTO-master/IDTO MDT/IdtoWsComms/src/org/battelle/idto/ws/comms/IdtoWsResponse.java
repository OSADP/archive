package org.battelle.idto.ws.comms;

public interface IdtoWsResponse<T>{
	public void onIdtoWsResponse(T response);
	public void onError(String error);
}

