package org.battelle.vital.iop;

import java.util.List;

/**
 * IOP packet instantiated by the packet factory when we receive an unknown, but well-formed, IOP packet.
 * 
 * @author R. Matt McCann @ Battelle Memorial Institute
 */
public class UnknownIOPPacket extends IOPPacket {
	
	protected UnknownIOPPacket(List<Byte> responseData, String rawResponse, byte type) {
		super(responseData, rawResponse, type);
	}
	
}
