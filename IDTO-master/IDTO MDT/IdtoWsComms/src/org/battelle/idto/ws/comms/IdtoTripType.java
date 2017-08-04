package org.battelle.idto.ws.comms;

public enum IdtoTripType {
	UPCOMING(1),
    INPROGRESS(2),
    PAST(3),
    ALL(4);

	IdtoTripType(int code){
         this.code=code;
    }
    protected int code;
    public int getCode() {
          return this.code;
    }
}
