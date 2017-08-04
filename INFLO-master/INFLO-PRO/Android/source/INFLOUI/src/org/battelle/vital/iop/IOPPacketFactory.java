package org.battelle.vital.iop;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import org.battelle.inflo.infloui.ApplicationLog;
import org.battelle.inflo.infloui.bluetooth.AsyncInputStream;

/**
 * Handles constructing packet from input data streams.
 *
 * @author R. Matt McCann, Battelle Memorial Institute
 * @patterns Factory, StateMachine
 */
public class IOPPacketFactory {
	
	/**
	 * Consumes to provided data byte.
	 *  
	 * @param dataByte Byte of data to consume
	 * @returns true If dataByte was the final byte in the packet
	 * 			false Packet is not yet finished
	 */
	private boolean consumeByte(byte dataByte)
	{
		// If this is a DLE byte
		if (dataByte == IOPPacket.IOP_DLE_BYTE) 
		{
			// If we are not expecting unpacked DLE bytes
			if ((mParsingState != State.OPENING_DLE) && (mParsingState != State.TERMINATORS)) 
			{
				// If we have already thrown away a DLE packing byte
				if (mThrewAwayPackingDLE) 
				{
					// Don't throw away this DLE because it's actual data, not packing
					mThrewAwayPackingDLE = false;
				} 
				else // If we have not thrown away a DLE packing byte yet
				{ 
					// Do nothing with this byte because it is a DLE packing byte
					mLog.e(mLogTag, "Threw away " + String.format("0x%02X ", dataByte));
					mThrewAwayPackingDLE = true;
					
					return false;
				}
			}
		}
		
		switch (mParsingState) 
		{
		case CHECKSUM:
			byte checksum;
			
			// Calculate the checksum for the packet we have received
			checksum = IOPPacket.calculateChecksum(mResponseData, mType);

			// If the checksums match as expected
			if (checksum == dataByte) 
			{
				// Move on to parse the closing packet terminators
				mParsingState = State.TERMINATORS;
			}
			else // If the checksums do not match
			{
				throw new RuntimeException("Packet CRC mismatch!"); // @TODO: Throw something else?
			}
			
			return false;
			
		case CONTENT:
			// Add the byte to the content
			mResponseData.add(dataByte);
			
			// If we have consumed all of the content bytes
			if (mResponseData.size() == mResponseSize)
			{
				// Move on to parsing the checksum
				mParsingState = State.CHECKSUM;
			}
			
			return false;
			
		case CONTENT_SIZE:
			// Save the content size and move on to parse the packet type
			mResponseSize = (byte) (dataByte - 1); // -1 Accounts for the packet type byte
			mParsingState = State.PACKET_TYPE;
			mResponseData.clear();

			return false;
			
		case DEVICE_ID:
			// If the device ID matches the expected VITAL id
			if (dataByte == IOPPacket.VITAL_DEVICE_ID)
			{
				// Move onto parsing the content size
				mParsingState = State.CONTENT_SIZE;
			}
			else // If this is not a VITAL module
			{
				throw new RuntimeException("Unsupported IOP device!"); // @TODO: Throw something else?
			}
			
			return false;
			
		case OPENING_DLE:
			// If the first byte is the expected opening DLE
			if (dataByte == IOPPacket.IOP_DLE_BYTE) 
			{
				// Move to the next processing state
				mParsingState = State.DEVICE_ID;
			} 
			else // If we received an unexpected byte
			{ 
				throw new RuntimeException("Malformed packet!"); // @TODO: Throw something else?
			}
			
			return false;
			
		case PACKET_TYPE:
			mType = dataByte;
			mParsingState = State.CONTENT;
			
			return false;
			
		case TERMINATORS:
			// If this is the the DLE terminator byte
			if (dataByte == IOPPacket.IOP_DLE_BYTE)
			{
				return false;
			}
			else if (dataByte == IOPPacket.IOP_ETX_BYTE) // If this is the last packet byte
			{
				return true;
			}
			else // If this is an unexpected byte
			{
				throw new RuntimeException("Malformed packet!"); // @TODO: Throw something else?
			}
			
		default:
			throw new RuntimeException("Illegal packet parsing state!"); // @TODO: Throw something else?
		}
	}
	
	/**
	 * Creates an IOP packet using data from the provided stream.
	 * 
	 * @param input Stream of data to build the packet from
	 * @return Constructed IOP packet
	 * @throws IOException Failed to read in the packet
	 */
	public IOPPacket create(AsyncInputStream input) throws IOException {
		boolean isPacketFinished = false;
		long    startTime        = System.currentTimeMillis();
		
		// Reset the internal state of response parsing
		mParsingState		 = State.OPENING_DLE;
		mRawResponse		 = "";
		mResponseData		 = new ArrayList<Byte>();
		mThrewAwayPackingDLE = false;
		
		mLog.e(mLogTag, "=======================================================");
		
		// Keep parsing bytes from the response until the packet is complete
		while (!isPacketFinished)
		{
			final int threeSeconds = 3000;
			
			// Read in the next byte
			Integer nextByte = input.read(threeSeconds);
			
			// If we have reached the end of the stream
			/*if (nextByte == -1) {
				// If we are out of time/patience
				if (System.currentTimeMillis() - startTime > threeSeconds) {
					throw new RuntimeException("Reached end of stream before completing packet!"); // @TODO: Throw something else?
				}
				// If we still have time to keep trying
				else {
					mLog.e(mLogTag, "End of stream reached early!");
					continue;
				}
			}*/
			
			// If we didn't get the next byte in time
			if (nextByte == null) {
				throw new RuntimeException("Reading next byte timed out!"); // @TODO: Throw something else
			}
			
			mLog.e(mLogTag, String.format("0x%02X ", nextByte.byteValue()));
			
			// Consume the byte
			mRawResponse     += String.format("0x%02X ", nextByte.byteValue());
			isPacketFinished  = consumeByte(nextByte.byteValue());
		}
		
		// Instantiate the packet now that we know all the details of its composition
		return instantiatePacket();
	}
	
	private IOPPacket instantiatePacket() {
		switch (mType) {
		case GetDataElementPacket.TYPE:
			return new GetDataElementPacket(mResponseData, mRawResponse, mType);
		case GetVersionPacket.TYPE:
			return new GetVersionPacket(mResponseData, mRawResponse, mType);
		default:
			return new UnknownIOPPacket(mResponseData, mRawResponse, mType);
		}
	}
	
	/** Current data processing state of the packet. */
	private enum State {
		/** Checksum of the packet. */
		CHECKSUM,
		/** Content of the packet. */
		CONTENT,
		/** Size of the packet contents. */
		CONTENT_SIZE,
		/** Device ID of the packet. */
		DEVICE_ID,
		/** First byte of the packet - DLE. */
		OPENING_DLE,
		/** Content type of the packet. */
		PACKET_TYPE,
		/** Terminating bytes of the packet. */
		TERMINATORS
	};
	
    /** Logging interface for the class. */
    private static ApplicationLog mLog = ApplicationLog.getInstance();

    /** Logging identifier used by the class. */
    private static String mLogTag = "IOPPacketFactory";

	/** Current state of the parsing state machine. */
	private State mParsingState = State.OPENING_DLE;
    
	/** Raw response received for this packet. Mostly for debug purposes. */
	private String mRawResponse = "";

	/** Content data of the response packet. */
	private List<Byte> mResponseData = new ArrayList<Byte>();
	
	/** Size of the response data. */
	private int mResponseSize = 0;
	
	/** Whether or not the last byte parsed was a thrown away DLE. */
	private boolean mThrewAwayPackingDLE = false;
	
	/** Type of packet i.e. getDataElement, setTime, etc. being constructed. */
	private byte mType = 0;
}
