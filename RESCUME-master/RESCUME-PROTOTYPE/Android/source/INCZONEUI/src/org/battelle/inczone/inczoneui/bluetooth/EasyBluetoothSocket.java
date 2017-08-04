/**
 * @file         inczoneui/bluetooth/ManagedBluetoothSocket.java
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

package org.battelle.inczone.inczoneui.bluetooth;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.Field;
import java.util.UUID;
import java.util.concurrent.ArrayBlockingQueue;

import org.apache.http.util.ByteArrayBuffer;
import org.battelle.inczone.inczoneui.ApplicationLog;

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

	protected final UUID mUuid;
	protected final String mMacAddress;
	protected final String mThreadName;

	protected Handler mThreadHandler = new Handler();
	protected Handler mCallbackHandler = new Handler();

	private HandlerThread mHandlerThread;

	private final ArrayBlockingQueue<Byte> mReceivedBytes = new ArrayBlockingQueue<Byte>(MAX_BUFFER_SIZE);

	/**
	 * Listener that receives callback when new data arrives
	 */
	private EasySocketListener mListener = null;
	/**
	 * Current active bt socket
	 */
	private BluetoothSocket mSocket = null;

	protected final ApplicationLog mAppLog;
	private volatile boolean mActive = false;
	private int mConnectionRetryDelay = 10000;
	private EasyBluetoothState mCurrentState = EasyBluetoothState.Unknown;

	public EasyBluetoothSocket(String macAddress, String bluetoothUuid) {

		this.mMacAddress = macAddress;
		this.mUuid = UUID.fromString(bluetoothUuid);

		this.mAppLog = ApplicationLog.getInstance();

		this.mThreadName = "EasyBluetoothSocket|" + macAddress;
	}

	public EasyBluetoothState getCurrentState() {
		return mCurrentState;
	}

	/**
	 * Adds a new callback listener for data received, connection state updated,
	 * etc
	 * 
	 * @param listener
	 *            New listener. {@code null} is not allowed.
	 */
	public void setListener(EasySocketListener listener) {
		this.mListener = listener;
	}

	/**
	 * Sets the time between failed connection attempts
	 * 
	 * @param connectionRetryDelay
	 */
	public void setConnectionRetryDelay(int connectionRetryDelay) {
		this.mConnectionRetryDelay = connectionRetryDelay;
	}

	/**
	 * Starts persistent connection attempt.
	 * 
	 * Connection attempt with only stop upon a configuration error: *Wrong MAC
	 * address format *Wrong UUID format *Bluetooth not supported
	 */
	public void connect() {
		// Create new thread and handler.
		this.mHandlerThread = new HandlerThread(mThreadName);
		this.mHandlerThread.start();
		this.mThreadHandler = new Handler(mHandlerThread.getLooper());

		// Set active flag and start managing socket
		this.mActive = true;
		this.mThreadHandler.post(new ManageSocketTask());
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
		this.mActive = false;
		setSocket(null);
	}

	/**
	 * Sends data if socket is available. Currently runs on calling thread.
	 * 
	 * @param data
	 *            Data to be sent
	 */
	public void write(byte[] data) {

		new Thread(new WriteDataTask(data)).start();
		// if (rHandlerThread != null)
		// rThreadHandler.post(new WriteDataTask(data));
		// new WriteDataTask(data).run();
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
			results = btDevice.createInsecureRfcommSocketToServiceRecord(mUuid);
		} catch (IOException e) {
			mAppLog.e(mThreadName, "openSocket(): Error creating bluetooth socket: " + e.getMessage());
		}

		if (results != null) {

			mAppLog.d(mThreadName, "openSocket(): Connecting...");
			try {
				results.connect();
				mAppLog.i(mThreadName, "openSocket(): Connection Established");
			} catch (IOException e) {
				mAppLog.e(mThreadName, "openSocket(): Error connecting to  bluetooth socket: " + e.getMessage());

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
		mCallbackHandler.post(new Runnable() {

			@Override
			public void run() {
				if (mListener != null)
					mListener.onDataReceived(mReceivedBytes.size());
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
		if (mSocket != null) {
			closeSocket(mSocket);
			updateState(EasyBluetoothState.Disconnected);
		}

		mSocket = socket;
	}

	/**
	 * Properly closes a socket. Uses reflection to get around an error where
	 * file descriptors don't get closed.
	 * 
	 * @param socket
	 *            Socket to close.
	 */
	protected void closeSocket(BluetoothSocket socket) {

		mAppLog.d(mThreadName, "closeSocket() closing socket");

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

		mCallbackHandler.post(new Runnable() {

			@Override
			public void run() {
				if (newState != mCurrentState) {

					mCurrentState = newState;

					if (mListener != null)
						mListener.onStateUpdated(mCurrentState);

					mAppLog.v(mThreadName, "updateState(): " + newState.toString());
				}
			}
		});
	}

	/**
	 * Releases all resources used in management of the socket.
	 */
	private void releaseResources() {

		setSocket(null);
		mAppLog.i(mThreadName, "releaseResources() Killing handler thread.");

		mHandlerThread.quit();

		mHandlerThread = null;
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

			if (!mActive) {
				// No longer active, so release everything
				setSocket(null);
				releaseResources();

			} else if (!isBluetoothAvailable()) {
				// Bluetooth isn't available so there isn't anything to do, so
				// just release.
				mActive = false;
				setSocket(null);
				releaseResources();

			} else if (!isBluetoothEnabled()) {
				// Bluetooth isn't enabled, so retry after a delay.
				updateState(EasyBluetoothState.Bluetooth_Off);
				mThreadHandler.postDelayed(this, mConnectionRetryDelay);

			} else if (mSocket == null) {
				// Socket is null, so try to open a new socket.
				updateState(EasyBluetoothState.Connecting);

				BluetoothAdapter btAdapter = BluetoothAdapter.getDefaultAdapter();
				BluetoothDevice btDevice = btAdapter.getRemoteDevice(mMacAddress);
				BluetoothSocket newSocket = openSocket(btDevice);

				if (newSocket != null) {
					setSocket(newSocket);
					mThreadHandler.post(this);

				} else {
					// Connection failed, so retry after a delay.
					mThreadHandler.postDelayed(this, mConnectionRetryDelay);
				}

			} else {
				// Bluetooth
				updateState(EasyBluetoothState.Connected);

				try {
					InputStream is = mSocket.getInputStream();
					BufferedInputStream bis = new BufferedInputStream(is);

					while (mSocket != null) {

						ByteArrayBuffer data = new ByteArrayBuffer(500);
						int length = bis.read(data.buffer());

						data.setLength(length);

						mAppLog.v(mThreadName,
								String.format("Received %d bytes: " + new String(data.toByteArray()), length));
						for (int i = 0; i < length; i++) {
							if (data.byteAt(i) == 0)
								mAppLog.e(mThreadName, String.format("RECEIVED a NULL CHARACTER"));
							if (!mReceivedBytes.offer((byte) data.byteAt(i))) {
								mAppLog.w(mThreadName, "Input Queue Full!!");
								break;
							}
						}

						onBytesReceived();
					}

				} catch (IOException e) {
					setSocket(null);
					// TODO: Log...
					// e.printStackTrace();
				}

				mThreadHandler.post(this);
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
			if (!mActive)
				return;

			if (mSocket == null) {
				mAppLog.w(mThreadName, "SendDataRunnable.run() btSocket is null.  Discarding data.");

			} else if (!mSocket.isConnected()) {
				mAppLog.w(mThreadName, "SendDataRunnable.run() btSocket is disconnected.  Discarding data.");

			} else {

				try {
					OutputStream os = mSocket.getOutputStream();
					BufferedOutputStream bos = new BufferedOutputStream(os);

					bos.write(data);
					bos.flush();
					// for (int i = 0; i < data.length; i += 100) {
					// os.write(data, i, Math.min(100, data.length - i));
					// }

					mAppLog.v(mThreadName, "SendDataRunnable.run() Sent " + data.length + " bytes: " + new String(data));

				} catch (IOException e) {
					mAppLog.e(mThreadName, "SendDataRunnable.run() bSocket.write error", e);
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

			if (mReceivedBytes.size() > 0) {
				results = Integer.valueOf(mReceivedBytes.remove());
			}
			return results;
		}

		public int readAsync(byte[] buffer, int byteOffset) throws IOException {
			int results = 0;

			if (mReceivedBytes.size() > 0) {
				while (!mReceivedBytes.isEmpty() && byteOffset < buffer.length) {
					buffer[byteOffset] = mReceivedBytes.remove();
					byteOffset++;
					results++;
				}
			}
			return results;
		};
	};

	OutputStream rWriteStream = new OutputStream() {

		@Override
		public void write(int oneByte) throws IOException {
			if (mSocket != null)
				mSocket.getOutputStream().write(oneByte);
		}
	};

}
