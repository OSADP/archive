/**
 * @file         infloui/bluetooth/ManagedBluetoothSocket.java
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */

package org.battelle.inflo.infloui.bluetooth;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ArrayBlockingQueue;

import org.battelle.inflo.infloui.ApplicationLog;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.ParcelFileDescriptor;

public class EasyBluetoothSocket {

	public static int MAX_BUFFER_SIZE = 3000;

	/**
	 * 
	 * @return True if there is a bluetooth interface available
	 */
	public static boolean isBluetoothAvailable() {
		BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();
		return btAdapter != null;
	}

	/**
	 * 
	 * @return True if the default bluetooth adapter is enabled.
	 */
	public static boolean isBluetoothEnabled() {
		BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();
		return btAdapter != null && btAdapter.isEnabled();
	}

	/**
	 * UUID of the service we are connecting to
	 */
	protected final UUID rUuid;
	/**
	 * MAC address of the remote bluetooth device the service will be connecting
	 * too
	 */
	protected final String strMacAddress;
	/**
	 * Name of the thread that this {@code ManagedBluetoothSocket} runs on.
	 * Contains MAC and UUID numbers
	 */
	protected final String strThreadName;
	/**
	 * Handler that all time intensive (connecting, reading) operations will be
	 * performed on
	 */
	protected Handler rThreadHandler;
	/**
	 * Spawned thread that connecting and reading is performed on.
	 * {@code handler} uses this thread's looper.
	 */
	private HandlerThread rHandlerThread;
	/**
	 * All bytes the socket has received but not yet consumed
	 */
	private final ArrayBlockingQueue<Byte> rReceivedBytes = new ArrayBlockingQueue<Byte>(
			MAX_BUFFER_SIZE);

	/**
	 * Listener that receives callback when new data arrives
	 */
	private List<EasySocketListener> rListeners = new ArrayList<EasySocketListener>();
	/**
	 * Current active bt socket
	 */
	private BluetoothSocket rBtSocket = null;

	protected final Handler rCallerHandler;
	protected final ApplicationLog rAppLog;
	private volatile boolean active = false;
	private int connectionRetryDelay = 10000;
	private EasyBluetoothState currentState = EasyBluetoothState.Unknown;

	public EasyBluetoothSocket(String macAddress, String bluetoothUuid) {

		this.rCallerHandler = new Handler();

		this.strMacAddress = macAddress;
		this.rUuid = UUID.fromString(bluetoothUuid);

		this.rAppLog = ApplicationLog.getInstance();

		this.strThreadName = "EasyBluetoothSocket|" + macAddress;
	}

	public EasyBluetoothSocket(String macAddress, String bluetoothUuid,
			Handler callbackHandler) {

		this.rCallerHandler = callbackHandler;

		this.strMacAddress = macAddress;
		this.rUuid = UUID.fromString(bluetoothUuid);

		this.rAppLog = ApplicationLog.getInstance();

		this.strThreadName = "EasyBluetoothSocket|" + macAddress;
	}

	public EasyBluetoothState getCurrentState() {
		return currentState;
	}

	/**
	 * Adds a new callback listener for data received, connection state updated,
	 * etc
	 * 
	 * @param listener
	 *            New listener. {@code null} is not allowed.
	 */
	public void addListener(EasySocketListener listener) {
		this.rListeners.add(listener);
	}

	/**
	 * Removes a new callback listener for data received, connection state
	 * updated, etc. null parameter removes all listeners.
	 * 
	 * @param listener
	 *            New listener. {@code null} is allowed.
	 */
	public void removeListener(EasySocketListener listener) {
		if (listener == null)
			this.rListeners.clear();
		else
			this.rListeners.remove(listener);

	}

	/**
	 * Sets the time between failed connection attempts
	 * 
	 * @param connectionRetryDelay
	 */
	public void setConnectionRetryDelay(int connectionRetryDelay) {
		this.connectionRetryDelay = connectionRetryDelay;
	}

	/**
	 * Starts persistent connection attempt.
	 * 
	 * Connection attempt with only stop upon a configuration error: *Wrong MAC
	 * address format *Wrong UUID format *Bluetooth not supported
	 */
	public void connect() {
		// Create new thread and handler.
		this.rHandlerThread = new HandlerThread(strThreadName);
		this.rHandlerThread.start();
		this.rThreadHandler = new Handler(rHandlerThread.getLooper());

		// Set active flag and start managing socket
		this.active = true;
		this.rThreadHandler.post(new ManageSocketTask());
	}

	/**
	 * Closes any open sockets and schedules a CloseTask which shuts down
	 * background thread
	 */
	public void close() {

		/*
		 * Reset active flag and close the current socket. Any blocking
		 * operations will throw exceptions and the ManageSocketTask will
		 * release resources when ready.
		 */
		this.active = false;
		setSocket(null);
	}

	/**
	 * Sends data if socket is available. Currently runs on calling thread.
	 * 
	 * @param data
	 *            Data to be sent
	 */
	public void write(byte[] data) {
		new WriteDataTask(data).run();
	}

	/**
	 * Tries to open a socket for the given {@code BluetoothDevice}
	 * 
	 * @param btDevice
	 *            Device to try to open socket too
	 * @return Connected {@code BluetoothSocket} if connection was successful,
	 *         otherwise null
	 */
	protected BluetoothSocket openSocket(BluetoothDevice btDevice) {

		BluetoothSocket results = null;

		try {
			results = btDevice.createInsecureRfcommSocketToServiceRecord(rUuid);
		} catch (IOException e) {
			rAppLog.e(
					strThreadName,
					"openSocket(): Error creating bluetooth socket: "
							+ e.getMessage());
		}

		if (results != null) {

			rAppLog.d(strThreadName, "openSocket(): Connecting...");
			try {
				results.connect();
				rAppLog.i(strThreadName, "openSocket(): Connection Established");
			} catch (IOException e) {
				rAppLog.e(strThreadName,
						"openSocket(): Error connecting to  bluetooth socket: "
								+ e.getMessage());

				closeSocket(results);
				results = null;
			}
		}

		return results;
	}

	/**
	 * Called when new data is received
	 */
	protected void onBytesReceived() {

		final int receivedCount;
		synchronized (rReceivedBytes) {
			receivedCount = rReceivedBytes.size();
		}

		// Give it to the listener. They return how much they've consumed
		rCallerHandler.post(new Runnable() {

			@Override
			public void run() {
				for (EasySocketListener i : rListeners)
					i.onDataReceived(receivedCount);

			}
		});

	}

	/**
	 * Updates the current socket with the new one. Closes the current one if
	 * open. A {@code null} socket parameter will just close the current socket.
	 * 
	 * @param socket
	 *            {@code BluetoothSocket} to set as the current socket.
	 *            {@code null} will just close the current socket.
	 */
	private void setSocket(BluetoothSocket socket) {

		if (rBtSocket != null) {
			closeSocket(rBtSocket);
		}

		rBtSocket = socket;
	}

	/**
	 * Properly closes a socket. Uses reflection to get around an error where
	 * file descriptors don't get closed.
	 * 
	 * @param socket
	 *            Socket to close.
	 */
	protected void closeSocket(BluetoothSocket socket) {

		rAppLog.d(strThreadName, "closeSocket() closing socket");

		Field mPfdField = null;
		try {
			mPfdField = BluetoothSocket.class.getDeclaredField("mPfd");
		} catch (NoSuchFieldException e) {
			e.printStackTrace();
		}

		ParcelFileDescriptor mPfd = null;
		if (mPfdField != null) {
			mPfdField.setAccessible(true);
			try {
				mPfd = (ParcelFileDescriptor) mPfdField.get(socket);
				mPfdField.set(socket, null);
			} catch (IllegalAccessException e) {
				e.printStackTrace();
			} catch (IllegalArgumentException e) {
				e.printStackTrace();
			}
		}

		try {
			socket.close();
		} catch (IOException e) {
			e.printStackTrace();
		}

		if (mPfd != null) {
			try {
				mPfd.close();
			} catch (IOException e) {
				e.printStackTrace();
			}
		}
	}

	/**
	 * Updates the state of the socket, firing any appropriate callbacks.
	 * 
	 * @param newState
	 */
	private void updateState(final EasyBluetoothState newState) {
		// Only trigger callbacks on a change.
		if (newState != this.currentState) {

			rCallerHandler.post(new Runnable() {

				@Override
				public void run() {
					for (EasySocketListener i : rListeners)
						i.onStateUpdated(newState);

				}
			});

			rAppLog.v(strThreadName, "updateState(): " + newState.toString());
		}

		this.currentState = newState;
	}

	/**
	 * Attempts to read from remote socket.
	 * 
	 * @return True if read was successful, false if there was an error.
	 */
	private boolean executeRead() {

		try {
			InputStream input = rBtSocket.getInputStream();

			byte[] inBytes = new byte[MAX_BUFFER_SIZE];
			// This line blocks if there are no bytes to be read.
			int bytesReceived = input.read(inBytes);

			/*
			 * rAppLog.v(strThreadName, "ReadRunnable.run(): Received " +
			 * bytesReceived + " bytes");
			 */
			synchronized (rReceivedBytes) {
				for (int i = 0; i < bytesReceived; i++) {

					if (rReceivedBytes.remainingCapacity() == 0) {
						rReceivedBytes.clear();
						rAppLog.w(strThreadName,
								"ReadRunnable.run(): No room left in Received Byte's queue!!");
					}

					rReceivedBytes.offer(inBytes[i]);
				}
			}

			// Allow processing of received bytes
			onBytesReceived();
			return true;

		} catch (IOException e) {
			rAppLog.w(strThreadName,
					"ReadRunnable.run(): Error reading from socket input stream: "
							+ e.getMessage());
		}

		return false;
	}

	/**
	 * Releases all resources used in management of the socket.
	 */
	private void releaseResources() {

		setSocket(null);
		rAppLog.i(strThreadName, "releaseResources() Killing handler thread.");

		rThreadHandler.removeCallbacksAndMessages(null);
		rHandlerThread.quit();

		rHandlerThread = null;
		rThreadHandler = null;
	}

	/**
	 * Task that handles the management of the different actions that must take
	 * place for different states of the socket
	 * 
	 * @author branch
	 * 
	 */
	private class ManageSocketTask implements Runnable {

		@Override
		public void run() {

			if (!active) {
				// No longer active, so release everything
				updateState(EasyBluetoothState.Disconnected);
				releaseResources();

			} else if (!isBluetoothAvailable()) {
				// Bluetooth isn't available so there isn't anything to do, so
				// just release.
				active = false;
				updateState(EasyBluetoothState.Bluetooth_Unavailable);
				releaseResources();

			} else if (!isBluetoothEnabled()) {
				// Bluetooth isn't enabled, so retry after a delay.
				updateState(EasyBluetoothState.Bluetooth_Off);
				rThreadHandler.postDelayed(this, connectionRetryDelay);

			} else if (rBtSocket == null) {
				// Socket is null, so try to open a new socket.
				updateState(EasyBluetoothState.Connecting);

				BluetoothAdapter btAdapter = BluetoothAdapter
						.getDefaultAdapter();
				BluetoothDevice btDevice = btAdapter
						.getRemoteDevice(strMacAddress);
				BluetoothSocket newSocket = openSocket(btDevice);

				if (newSocket != null) {
					setSocket(newSocket);
					rThreadHandler.post(this);

				} else {
					// Connection failed, so retry after a delay.
					rThreadHandler.postDelayed(this, connectionRetryDelay);
				}

			} else {
				// Bluetooth
				updateState(EasyBluetoothState.Connected);
				if (!executeRead())
					setSocket(null);

				rThreadHandler.post(this);
			}
		}

	}

	private class WriteDataTask implements Runnable {

		byte[] data;

		public WriteDataTask(byte[] data) {
			this.data = data;
		}

		@Override
		public void run() {
			if (!active)
				return;

			if (rBtSocket == null) {
				rAppLog.w(strThreadName,
						"SendDataRunnable.run() btSocket is null.  Discarding data.");

			} else if (!rBtSocket.isConnected()) {
				rAppLog.w(strThreadName,
						"SendDataRunnable.run() btSocket is disconnected.  Discarding data.");

			} else {

				try {

					rBtSocket.getOutputStream().write(data);

					rAppLog.v(strThreadName, "SendDataRunnable.run() Sent "
							+ data.length + " bytes.");

				} catch (IOException e) {
					rAppLog.e(strThreadName,
							"SendDataRunnable.run() bSocket.write error", e);
				}
			}
		}
	}

	public AsyncInputStream getInputStream() {
		return rReadStream;
	}

	public OutputStream getOutputStream() {
		return rWriteStream;
	}

	AsyncInputStream rReadStream = new AsyncInputStream() {

		@Override
		public Integer readAsync() throws IOException {
			Integer results = null;

			synchronized (rReceivedBytes) {
				if (rReceivedBytes.size() > 0) {
					results = Integer.valueOf(rReceivedBytes.remove());
				}
			}
			return results;
		}
	};

	OutputStream rWriteStream = new OutputStream() {

		@Override
		public void write(int oneByte) throws IOException {
			if (rBtSocket != null)
				rBtSocket.getOutputStream().write(oneByte);
		}
	};

}
