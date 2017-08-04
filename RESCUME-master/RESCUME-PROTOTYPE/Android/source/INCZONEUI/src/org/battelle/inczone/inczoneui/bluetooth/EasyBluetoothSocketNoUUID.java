package org.battelle.inczone.inczoneui.bluetooth;

import java.io.IOException;
import java.lang.reflect.Method;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;

public class EasyBluetoothSocketNoUUID extends EasyBluetoothSocket {

	public EasyBluetoothSocketNoUUID(String macAddress, String bluetoothUuid) {
		super(macAddress, bluetoothUuid);
	}

	@Override
	protected BluetoothSocket openSocket(BluetoothDevice btDevice) {

		BluetoothSocket results = null;

		try {

			Method m = btDevice.getClass().getMethod("createInsecureRfcommSocket", new Class[] { int.class });
			results = (BluetoothSocket) m.invoke(btDevice, 1);

		} catch (Exception e) {
			mAppLog.e(mThreadName, "NoUUDI - openSocket(): Error creating bluetooth socket: " + e.getMessage());
		}

		if (results != null) {

			mAppLog.d(mThreadName, "NoUUDI - openSocket(): Connecting...");
			try {
				results.connect();
				mAppLog.i(mThreadName, "NoUUDI - openSocket(): Connection Established");
			} catch (IOException e) {
				mAppLog.e(mThreadName,
						"NoUUDI - openSocket(): Error connecting to  bluetooth socket: " + e.getMessage());

				closeSocket(results);
				results = null;
			}
		}

		return results;
	}

}
