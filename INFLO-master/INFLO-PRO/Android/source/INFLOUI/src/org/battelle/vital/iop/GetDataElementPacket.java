package org.battelle.vital.iop;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.List;

/** IOP packet that retrieves the current value of a data element i.e. rpm, speed, etc. */
public class GetDataElementPacket extends IOPPacket 
{	
	/** Data elements that can be retrieved from the VITAL module. */
	public enum DataElement
	{
		/** Temperature of the air in degrees celcius. */
		AMB_AIR_TEMP_DEG_C((byte) 0x01),
		/** Barometric air pressure in kilopascals. */
		BAROMETRIC_PRESS_KPA((byte) 0x02),
		/** Not presently available? */
		//BRAKE_POSITION
		/** Engine rotations per minute. */
		ENGINE_RPM((byte) 0x05),
		/** Lateral acceleration @TODO: Units TBD? */
		LAT_ACCEL_TBD((byte) 0x13),
		/** Longitudinal acceleration @TODO: Units TBD? */
		LONG_ACCEL_TBD((byte) 0x14),
		/** Mass air flow in grams per second. */
		MAF_GRAMS_PER_SECOND((byte) 0x0F),
		/** Not presently available? */
		//STEERING_ANGLE
		/** Percent engagement of the throttle. */
		THROTTLE_POSITION_PERCENT((byte) 0x11),
		/** Vehicle speed in kilometers per hour. */
		VEHICLE_SPEED_KPH((byte) 0x12);
		/** Not presently available? */
		//VIN
		/** Not presently available? */
		//WIPERS
		
		private byte mRaw;
		
		private DataElement(byte raw) {
			mRaw = raw;
		}
	
		public byte getRaw() {
			return mRaw;
		}
		
		public static DataElement fromByte(byte rawType) {
			if (rawType == AMB_AIR_TEMP_DEG_C.getRaw()) {
				return AMB_AIR_TEMP_DEG_C;
			} else if (rawType == BAROMETRIC_PRESS_KPA.getRaw()) {
				return BAROMETRIC_PRESS_KPA;
			} else if (rawType == ENGINE_RPM.getRaw()) {
				return ENGINE_RPM;
			} else if (rawType == MAF_GRAMS_PER_SECOND.getRaw()) {
				return MAF_GRAMS_PER_SECOND;
			} else if (rawType == LAT_ACCEL_TBD.getRaw()) {
				return LAT_ACCEL_TBD;
			} else if (rawType == LONG_ACCEL_TBD.getRaw()) {
				return LONG_ACCEL_TBD;
			} else if (rawType == THROTTLE_POSITION_PERCENT.getRaw()) {
				return THROTTLE_POSITION_PERCENT;
			} else if (rawType == VEHICLE_SPEED_KPH.getRaw()) {
				return VEHICLE_SPEED_KPH;
			} else {
				throw new RuntimeException("Unsupported data element '" +  String.format("0x%02X ", rawType) + "'!"); // @TODO: Throw something else?
			}
		}
	}
	
	/** IOP command id for the GetDataElement command. */
	public static final byte TYPE = 0x52;
	
	public GetDataElementPacket(DataElement dataElement)
	{
		super(TYPE);
		
		List<Byte> requestData = new ArrayList<Byte>();
		
		// Set the request content data
		requestData.add(dataElement.getRaw());
		setRequestData(requestData);
		
		// Set the packet type
		mDataElement = dataElement;
	}
	
	protected GetDataElementPacket(List<Byte> responseData, String rawResponse, byte type) {
		super(responseData, rawResponse, type);
		
		mDataElement = DataElement.fromByte(responseData.get(0));
	}
	
	public DataElement getDataElement() {
		return mDataElement;
	}
	
	/**
	 * @return The value of the data element
	 */
	public double getValue() {
		List<Byte> responseData         = getResponseData();
		byte[]     valueBytes           = new byte[EXPECTED_RESPONSE_SIZE - 2];

		// If there aren't the correct number of bytes in the response
		if (responseData.size() != EXPECTED_RESPONSE_SIZE) {
			throw new RuntimeException("Malformed response size!"); // @TODO: Throw something else?
		}
		
		// If the data element is inactive
		if (responseData.get(IS_ACTIVE_POS) == 0) {
			throw new RuntimeException("Data element is not active! Check isActive() first!");
		}
		
		// Copy the value bytes into a form that we can convert to a double
		for (int byteIter = 2; byteIter < responseData.size(); byteIter++) {
			valueBytes[valueBytes.length - byteIter + 1] = responseData.get(byteIter);
		}
		
		return ByteBuffer.wrap(valueBytes).getDouble();
	}
	
	/**
	 * @return Whether or not the data element is active.
	 */
	public boolean isActive() {
		List<Byte> responseData = getResponseData();
		
		// If there aren't the correct number of bytes in the response
		if (responseData.size() != EXPECTED_RESPONSE_SIZE) {
			throw new RuntimeException("Malformed response size!"); // @TODO: Throw something else?
		}
		
		return (responseData.get(IS_ACTIVE_POS) == 1);
	}
	
	/** Expected size of the response contents. */
	private static final int EXPECTED_RESPONSE_SIZE = 10;
	
	/** Byte offset of the "isActive?" field. */
	private static final int IS_ACTIVE_POS = 1;

	private DataElement mDataElement;
}
