package org.battelle.inczone.inczoneui.ntrip;

import java.io.IOException;
import java.io.InputStream;
import java.net.Socket;
import java.net.UnknownHostException;

import org.apache.http.util.ByteArrayBuffer;
import org.battelle.inczone.inczoneui.ApplicationLog;
import org.battelle.inczone.inczoneui.R;
import org.battelle.inczone.inczoneui.bluetooth.EasyBluetoothSocket;
import org.battelle.inczone.inczoneui.bluetooth.EasyBluetoothState;
import org.battelle.inczone.inczoneui.bluetooth.EasySocketListener;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.IBinder;
import android.support.v4.content.LocalBroadcastManager;
import android.util.Base64;

public class NTripService extends Service {

	private final static String UUID = "6f408520-f4ee-11e3-a3ac-0800200c9a66";
	private final static int BLUETOOTH_CONNECTION_RETRY_DELAY = 3000;
	private final static int NTRIP_CONNECTION_RETRY_DELAY = 3000;

	private final static String PREFIX = "org.battelle.inczone.inczoneui.obu.handlers.NTripService";

	public final static String ACTION_RECEIVED_RTCM = PREFIX
			+ ".action_received_rtcm";
	public final static String ACTION_UPDATED = PREFIX + ".action_updated";
	public final static String EXTRA_STATE = PREFIX + ".extra_state";
	public final static String EXTRA_BYTES_RECEIVED = PREFIX
			+ ".extra_bytes_received";
	public final static String EXTRA_RTCM2_3 = PREFIX + ".extra_rtcm2_3";
	public final static String EXTRA_GGA = PREFIX + ".extra_gga";

	/**
	 * Stops the service
	 * 
	 * @param context
	 *            Context that the intent will be created with. This is often
	 *            the calling service or activity
	 */
	public final static void stopService(Context context) {

		context.stopService(new Intent(context, NTripService.class));
	}

	public final static void startService(Context context) {

		context.startService(new Intent(context, NTripService.class));
	}

	String mEndpoint = null; // "156.63.133.118";
	int mPort = 0; // 2101;
	String mMountPoint = null; // "ODOT_RTCM23";

	String mUserName = null;
	String mPassword = null;

	private SharedPreferences mSettings;
	private Socket mSocket;
	private Handler mHandler;
	private HandlerThread mHandlerThread;
	private boolean mActive = false;
	private boolean mRestartFlag = false;
	private final ApplicationLog mAppLog = ApplicationLog.getInstance();
	private NTripState mState = NTripState.Disconnected;
	private long mBytesReceived = 0;
	private EasyBluetoothSocket mBluetoothSocket = null;

	@Override
	public IBinder onBind(Intent arg0) {
		return null;
	}

	@Override
	public void onCreate() {
		super.onCreate();

		mAppLog.i("NTripService", "onCreate()");

		mSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);
		mSettings
				.registerOnSharedPreferenceChangeListener(mSettingChangedListener);

		mBluetoothSocket = new EasyBluetoothSocket(mSettings.getString(
				getResources().getString(R.string.setting_obuMacAddress_key),
				""), UUID);
		mBluetoothSocket
				.setConnectionRetryDelay(BLUETOOTH_CONNECTION_RETRY_DELAY);
		mBluetoothSocket.setListener(mBluetoothListener);
		mBluetoothSocket.connect();

		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources().getString(R.string.setting_ntripEnabled_key));
		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources().getString(R.string.setting_ntripEndpoint_key));
		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources()
						.getString(R.string.setting_ntripEndpointPort_key));
		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources().getString(R.string.setting_ntripUserName_key));
		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources().getString(R.string.setting_ntripPassword_key));
		mSettingChangedListener.onSharedPreferenceChanged(mSettings,
				getResources().getString(R.string.setting_ntripMountPoint_key));

		mRestartFlag = false;

		mHandlerThread = new HandlerThread("NTripService");
		mHandlerThread.start();
		mHandler = new Handler(mHandlerThread.getLooper());
		mHandler.post(new ManageSocketTask());
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {

		return Service.START_STICKY;
	}

	@Override
	public void onDestroy() {
		mAppLog.i("NTripService", "onDestroy()");

		mSettings
				.unregisterOnSharedPreferenceChangeListener(mSettingChangedListener);

		mActive = false;

		mHandlerThread.quit();
		mHandlerThread = null;

		updateState(NTripState.Disconnected);
		updateSocket(null);

		mBluetoothSocket.close();
		mBluetoothSocket.setListener(null);

		super.onDestroy();
	}

	/**
	 * Fires whenever a settings is changed. Used to reinitialize the bluetooth
	 * socket if a different MAC address is selected.
	 */
	OnSharedPreferenceChangeListener mSettingChangedListener = new OnSharedPreferenceChangeListener() {

		@Override
		public void onSharedPreferenceChanged(
				SharedPreferences sharedPreferences, String key) {

			// If obu mac address changed, reinitialize the socket.
			if (key.equals(getResources().getString(
					R.string.setting_ntripEnabled_key))) {

				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");
				mActive = mSettings.getBoolean(key, true);
			} else if (key.equals(getResources().getString(
					R.string.setting_ntripEndpoint_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");
				mEndpoint = mSettings.getString(key, null);
			} else if (key.equals(getResources().getString(
					R.string.setting_ntripEndpointPort_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");
				try {
					mPort = Integer.parseInt(mSettings.getString(key, "0"));
				} catch (NumberFormatException e) {
					mPort = 0;
				}
				mRestartFlag = true;
			} else if (key.equals(getResources().getString(
					R.string.setting_ntripUserName_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");

				mUserName = mSettings.getString(key, null);
				mRestartFlag = true;
			} else if (key.equals(getResources().getString(
					R.string.setting_ntripPassword_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");

				mPassword = mSettings.getString(key, null);
				mRestartFlag = true;
			} else if (key.equals(getResources().getString(
					R.string.setting_ntripMountPoint_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");

				mMountPoint = mSettings.getString(key, null);
				mRestartFlag = true;
			} else if (key.equals(getResources().getString(
					R.string.setting_obuMacAddress_key))) {
				mAppLog.i("NTripService",
						"prefChangedListener.onSharedPreferenceChanged: Setting changed ("
								+ key + ").");

				mBluetoothSocket.close();
				mBluetoothSocket.setListener(null);

				mBluetoothSocket = new EasyBluetoothSocket(mSettings.getString(
						getResources().getString(
								R.string.setting_obuMacAddress_key), ""), UUID);
				mBluetoothSocket
						.setConnectionRetryDelay(BLUETOOTH_CONNECTION_RETRY_DELAY);
				mBluetoothSocket.setListener(mBluetoothListener);
				mBluetoothSocket.connect();
			}
		}
	};

	private EasySocketListener mBluetoothListener = new EasySocketListener() {

		@Override
		public void onStateUpdated(EasyBluetoothState state) {

			if (state != EasyBluetoothState.Connected)
				updateSocket(null);

		}

		@Override
		public void onDataReceived(int count) {

			mAppLog.i("NTripService",
					String.format("Receieved %d bytes from radio", count));

			ByteArrayBuffer data = new ByteArrayBuffer(200);
			int dataLength = 0;
			try {
				dataLength = mBluetoothSocket.getInputStream().readAsync(
						data.buffer(), 0);
				mAppLog.i("NTripService",
						String.format("Read %d bytes from radio", dataLength));
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			if (dataLength == 0)
				return;

			data.setLength(dataLength);
			new SendToNTRIPTask(data.toByteArray()).run();
		}
	};

	protected synchronized void updateState(NTripState state) {
		if (state != mState) {
			mAppLog.i("NTripService", String.format(
					"State changed from [%s] to [%s]", mState.toString()
							.replace('_', ' '),
					state.toString().replace('_', ' ')));
			mState = state;
			broadcastState();
		}
	}

	protected void broadcastState() {
		Intent intent = new Intent(ACTION_UPDATED);
		intent.putExtra(EXTRA_STATE, mState);
		intent.putExtra(EXTRA_BYTES_RECEIVED, mBytesReceived);

		LocalBroadcastManager.getInstance(this).sendBroadcast(intent);
	}

	private synchronized void updateSocket(Socket s) {
		if (this.mSocket != null)
			try {
				mSocket.close();
				updateState(NTripState.Disconnected);
			} catch (IOException e) {
				e.printStackTrace();
			}
		mSocket = s;
	}

	private class SendToNTRIPTask implements Runnable {

		private final byte[] data;

		public SendToNTRIPTask(byte[] data) {
			this.data = data;
		}

		@Override
		public void run() {

			if (mSocket != null) {
				try {
					mSocket.getOutputStream().write(data);
				} catch (IOException e) {
					updateSocket(null);
				}
			}
		}
	}

	private class ManageSocketTask implements Runnable {

		@Override
		public void run() {
			int delay = 0;

			if (mRestartFlag) {
				updateSocket(null);
				mRestartFlag = false;
			} else if (!mActive) {
				updateSocket(null);
				delay = NTRIP_CONNECTION_RETRY_DELAY;
			} else if (mSocket == null) {
				// Socket is null, so try to open a new socket.
				if (mBluetoothSocket.getCurrentState() == EasyBluetoothState.Connected) {
					updateState(NTripState.Connecting);
					Socket newSocket = null;

					try {
						if (mEndpoint == null || mPort == 0
								|| mMountPoint == null || mUserName == null
								|| mPassword == null) {
							// TODO: Log...
							delay = NTRIP_CONNECTION_RETRY_DELAY;
							return;
						}

						newSocket = new Socket(mEndpoint, mPort);

						StringBuilder sb = new StringBuilder();
						sb.append("GET /" + mMountPoint + " HTTP/1.0\r\n");

						newSocket.getOutputStream().write(
								("GET /" + mMountPoint + " HTTP/1.0\r\n")
										.getBytes());
						// newSocket.getOutputStream().write("Accept: */*\r\n".getBytes());
						// newSocket.getOutputStream().write("Connection: close\r\n".getBytes());
						if (!mUserName.isEmpty() || !mPassword.isEmpty()) {
							sb.append("Authorization: Basic "
									+ Base64.encodeToString(
											(mUserName + ":" + mPassword)
													.getBytes(), Base64.NO_WRAP)
									+ "\r\n\r\n");
						}

						mAppLog.v("NTripService",
								"Sending message: " + sb.toString());

						newSocket.getOutputStream().write(
								sb.toString().getBytes());

					} catch (UnknownHostException e) {
						// e.printStackTrace();
						newSocket = null;
					} catch (IOException e) {
						// e.printStackTrace();
						newSocket = null;
					}

					if (newSocket != null) {
						updateState(NTripState.Connected);
						updateSocket(newSocket);

						mHandler.post(this);
					} else { // Connection failed, so retry after a delay.
						// TODO: Log...
						delay = NTRIP_CONNECTION_RETRY_DELAY;
					}
				} else {
					updateState(NTripState.Waiting_For_Bluetooth);
					delay = NTRIP_CONNECTION_RETRY_DELAY;
				}

			} else {
				if (!mSocket.isConnected()) {
					updateSocket(null);
				} else {

					try {
						InputStream in = mSocket.getInputStream();

						if (in.available() > 0) {
							ByteArrayBuffer data = new ByteArrayBuffer(2000);
							int length = in.read(data.buffer());

							data.setLength(length);

							mAppLog.v("NTripService", String.format(
									"RECEIVED %d BYTES: "
											+ new String(data.toByteArray()),
									length));

							mBytesReceived += length;
							broadcastState();

							if (mState == NTripState.Receiving_Data
									|| mState == NTripState.Waiting_For_Data) {
								updateState(NTripState.Receiving_Data);

								mBluetoothSocket.write(data.toByteArray());
							}

							if (new String(data.toByteArray())
									.endsWith("\r\n\r\n"))
								updateState(NTripState.Waiting_For_Data);
						}

					} catch (IOException e) {
						updateSocket(null);
						// TODO: Log...
						// e.printStackTrace();
					}
					delay = 100;
				}
			}

			mHandler.postDelayed(this, delay);
		}
	}
}
