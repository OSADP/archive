package org.battelle.vital.iop;

import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.List;

/** A packet of data sent to/from the VITAL module using the Garmin IOP protocol. */
public abstract class IOPPacket {
	/** End of packet byte used in IOP packets. */
    public static final byte IOP_ETX_BYTE = 0x03;

    /** Acknowledgment byte used in IOP packets. */
    public static final byte IOP_ACK_BYTE = 0x06;

    /** Data link escape byte used in IOP packets. */
    public static final byte IOP_DLE_BYTE = 0x10;

    /** Negative acknowledgment byte used in IOP packets. */
    public static final byte IOP_NAK_BYTE = 0x15;
    
    /** Garmin protocol's device ID for the VITAL module. */
    public static final byte VITAL_DEVICE_ID = (byte) 0xbd;
    
    protected IOPPacket(byte type) {
    	mIsResponse = false;
    	mType 		= type;
    }
    
    protected IOPPacket(List<Byte> responseData, String rawResponse, byte type) {
    	mIsResponse	  = true;
    	mResponseData = responseData;
    	mRawResponse  = rawResponse;
    	mType         = type;
    }
    
    public String getRawResponse() {
    	return mRawResponse;
    }
    
    /**
     * Send the IOP request over the provided stream.
     *  
     * @param output Output stream to write the request bytes to
     */
	public void send(OutputStream output) throws IOException {
		List<Byte> 	   bytes = new ArrayList<Byte>();
		byte       	   checkSum;
		byte[]		   rawBytes;
		final int      sizePos = 2;
		
		// Add the opening DLE byte
		bytes.add((byte) IOP_DLE_BYTE);
		
		// Add the VITAL device ID
		bytes.add((byte) VITAL_DEVICE_ID);
		
		// Add a temporary size byte, we will come back and update it in a bit
		bytes.add((byte) 0);
		
		// Add the packet type
		bytes.add(mType);
		
		// Add the packet contents
		bytes.addAll(mRequestData);
		
		// Go back and set the packet size
		bytes.set(sizePos, (byte) (bytes.size() - 3));
		
		// Calculate the checksum and add it
		checkSum = calculateChecksum(mRequestData, mType);
		bytes.add(checkSum);
		
		// Add the DLE pads
		for (int iter = 1; iter < bytes.size(); iter++)
		{
			// If this is a DLE character
			if (bytes.get(iter) == IOP_DLE_BYTE)
			{
				// Insert a DLE stuffing
				bytes.add(iter++, (byte) IOP_DLE_BYTE);
			}
		}

		// Add the terminating bytes
		bytes.add((byte) IOP_DLE_BYTE);
		bytes.add((byte) IOP_ETX_BYTE);
		
		// Convert the bytes to the correct form
		rawBytes = new byte[bytes.size()];
		for (int byteIter = 0; byteIter < bytes.size(); byteIter++) {
			rawBytes[byteIter] = bytes.get(byteIter);
		}
		
		// Write out the IOP request
		output.write(rawBytes);
	}
	
	/**
	 * Calculates the 2's complement checksum of the bytes to be iterated over.
	 * 
	 * @param contentData Data that comprises the packet
	 * @param packetType Type of IOP packet
	 * @return checksum
	 */
	protected static byte calculateChecksum(
			List<Byte> contentData,
			byte packetType) {
		byte checksum;
		byte packetSize = (byte) (contentData.size() + 1);
		
		checksum  = VITAL_DEVICE_ID;
		checksum += packetSize;
		checksum += packetType;
		
		for (Byte data : contentData)
		{
			checksum += data;
		}

		return (byte) (0 - checksum);
	}
	
	protected List<Byte> getResponseData() {
		return mResponseData;
	}
	
	public boolean isResponse() {
		return mIsResponse;
	}
	
	protected void setRequestData(List<Byte> requestData) {
		mRequestData = requestData;
	}
	
	/** Whether or not this has response data. */
	private boolean mIsResponse;
	
	/** Raw response received for this packet. Mostly for debug purposes. */
	private String mRawResponse = "";
	
	/** Content data of the packet. */
	private List<Byte> mRequestData = new ArrayList<Byte>();
	
	/** Content data of the response packet. */
	private List<Byte> mResponseData = new ArrayList<Byte>();
	
	/** Type of packet i.e. getDataElement, setTime, etc. */
	private byte mType = 0;
}
