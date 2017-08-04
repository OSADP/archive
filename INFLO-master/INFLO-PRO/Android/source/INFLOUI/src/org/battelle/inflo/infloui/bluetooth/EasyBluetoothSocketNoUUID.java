package org.battelle.inflo.infloui.bluetooth;

import java.io.IOException;
import java.lang.reflect.Method;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.os.Handler;

public class EasyBluetoothSocketNoUUID extends EasyBluetoothSocket {

	public EasyBluetoothSocketNoUUID(String macAddress, String bluetoothUuid) {
		super(macAddress, bluetoothUuid);
	}

	public EasyBluetoothSocketNoUUID(String macAddress, String bluetoothUuid,
			Handler callbackHandler) {
		super(macAddress, bluetoothUuid, callbackHandler);
	}

	@Override
	protected BluetoothSocket openSocket(BluetoothDevice btDevice) {

		BluetoothSocket results = null;

		try {

			Method m = btDevice.getClass().getMethod(
					"createInsecureRfcommSocket", new Class[] { int.class });
			results = (BluetoothSocket) m.invoke(btDevice, 1);

		} catch (Exception e) {
			rAppLog.e(strThreadName,
					"NoUUDI - openSocket(): Error creating bluetooth socket: "
							+ e.getMessage());
		}

		if (results != null) {

			rAppLog.d(strThreadName, "NoUUDI - openSocket(): Connecting...");
			try {
				results.connect();
				rAppLog.i(strThreadName,
						"NoUUDI - openSocket(): Connection Established");
			} catch (IOException e) {
				rAppLog.e(strThreadName,
						"NoUUDI - openSocket(): Error connecting to  bluetooth socket: "
								+ e.getMessage());

				closeSocket(results);
				results = null;
			}
		}

		return results;
	}

}
