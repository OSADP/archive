package org.battelle.vital.iop;

import java.nio.ByteBuffer;
import java.util.List;

/** IOP packet that retrieves the hardware and software versions of the VITAL module. */
public class GetVersionPacket extends IOPPacket {
	/** Packet type for the GetVersion request. */
	public static final byte TYPE = (byte) 0x13;
	
	public GetVersionPacket() {
		super(TYPE);
	}
	
	protected GetVersionPacket(List<Byte> responseData, String rawResponse, byte type) {
		super(responseData, rawResponse, type);
	}
	
	public byte getHardwareVersion() {
		List<Byte> responseData = getResponseData();
		
		return responseData.get(0).byteValue();
	}
	
	public short getSoftwareMajorVersion() {
		final int  bytesPerShort        = 2;
		final int  offset               = 1;
		List<Byte> responseData         = getResponseData();
		List<Byte> softwareVersion;
		byte[]     softwareVersionBytes = new byte[bytesPerShort];

		// If there aren't the correct number of bytes in the response
		if (responseData.size() != EXPECTED_RESPONSE_SIZE) {
			throw new RuntimeException("Malformed response size!");
		}
		
		// Extract the software version bytes
		softwareVersion = responseData.subList(offset, offset + bytesPerShort);
		
		// Pack the hardware version bytes into a form we can convert to an integer
		softwareVersionBytes[0] = softwareVersion.get(1);
		softwareVersionBytes[1] = softwareVersion.get(0);
		
		return ByteBuffer.wrap(softwareVersionBytes).getShort();
	}
	
	public short getSoftwareMinorVersion() {
		final int  bytesPerShort        = 2;
		final int  offset               = 3;
		List<Byte> responseData         = getResponseData();
		List<Byte> softwareVersion;
		byte[]     softwareVersionBytes = new byte[bytesPerShort];

		// If there aren't the correct number of bytes in the response
		if (responseData.size() != EXPECTED_RESPONSE_SIZE) {
			throw new RuntimeException("Malformed response size!");
		}
		
		// Extract the software version bytes
		softwareVersion = responseData.subList(offset, offset + bytesPerShort);
		
		// Pack the hardware version bytes into a form we can convert to an integer
		softwareVersionBytes[0] = softwareVersion.get(1);
		softwareVersionBytes[1] = softwareVersion.get(0);
		
		return ByteBuffer.wrap(softwareVersionBytes).getShort();
	}
	
	/** Expected size of the response contents. */
	private static int EXPECTED_RESPONSE_SIZE = 5;
}
