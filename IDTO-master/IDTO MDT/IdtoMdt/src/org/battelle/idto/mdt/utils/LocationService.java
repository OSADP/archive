package org.battelle.idto.mdt.utils;

import java.util.ArrayList;

import org.battelle.idto.mdt.preferences.Constants;
import org.battelle.idto.ws.comms.IdtoWsInterface;
import org.battelle.idto.ws.comms.IdtoWsManager;
import org.battelle.idto.ws.comms.IdtoWsResponse;
import org.battelle.idto.ws.otp.data.Position;
import org.battelle.idto.ws.otp.data.ProbeMessage;
import org.battelle.idto.ws.otp.data.ProbeResponse;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesClient;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.location.LocationClient;
import com.google.android.gms.location.LocationListener;
import com.google.android.gms.location.LocationRequest;

import android.app.Service;
import android.content.Intent;
import android.content.SharedPreferences;
import android.location.Location;
import android.os.Binder;
import android.os.Bundle;
import android.os.IBinder;
import android.preference.PreferenceManager;
import android.widget.Toast;

public class LocationService extends Service implements
GooglePlayServicesClient.ConnectionCallbacks,
GooglePlayServicesClient.OnConnectionFailedListener, LocationListener{

	private static final Logger mLogger = LoggerFactory.getLogger(LocationService.class); 
	
	private LocationClient mLocationClient;
	private LocationRequest mLocationRequest;
	private IdtoWsInterface mIdtoWsManager;
	
	public class LocalBinder extends Binder {
		LocationService getService() {
			return LocationService.this;
		}
	}
	
	@Override
	public void onCreate() {
		super.onCreate();
		
		mIdtoWsManager = new IdtoWsManager(Constants.API_URL, this);

		int resp = GooglePlayServicesUtil.isGooglePlayServicesAvailable(this);
	    if (resp == ConnectionResult.SUCCESS) {
	    	mLocationRequest = LocationRequest.create();
	    	mLocationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);
	    	mLocationRequest.setInterval(1 * 1000);
	    	mLocationRequest.setFastestInterval(1 * 1000);
	   	 	
	    	mLocationClient = new LocationClient(this, this, this);

	    	mLocationClient.connect();
	    	 
	    	 

	    } else {
	        Toast.makeText(this, "Please install Google Play Service.",
	                Toast.LENGTH_SHORT).show();
	    }
    	 
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		return super.onStartCommand(intent, flags, startId);
	}


	@Override
	public void onDestroy() {
		super.onDestroy();

		if(mLocationClient.isConnected())
			mLocationClient.removeLocationUpdates(this);
	}

	@Override
	public void onLocationChanged(Location location) {
				//String msg = "Updated Location: " +
		        //        Double.toString(location.getLatitude()) + "," +
		        //        Double.toString(location.getLongitude());
		        //Toast.makeText(this, msg, Toast.LENGTH_SHORT).show();
		        
		SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getApplicationContext());
    	String vehicleName = prefs.getString("VehicleName", "Tablet1");
    	
    	SharedPreferences.Editor editor = getSharedPreferences("LOCATION", 0).edit();
    	editor.putString("LastLatitude", Double.toString(location.getLatitude()));
    	editor.putString("LastLongitude", Double.toString(location.getLongitude()));
    	editor.commit();
    	
		Position pos = new Position();
		
		pos.setAccuracy(location.getAccuracy());
		pos.setAltitude(location.getAltitude());
		pos.setHeading(location.getBearing());
		pos.setLatitude(location.getLatitude());
		pos.setLongitude(location.getLongitude());
		pos.setSatellites(0);
		pos.setSpeed(location.getSpeed());
		pos.setTimeStamp(location.getTime());
		
        ProbeMessage probeMsg = new ProbeMessage();
        ArrayList<Position> posArray = new ArrayList<Position>();
        posArray.add(pos);
        
        probeMsg.setPositions(posArray);
        probeMsg.setInboundVehicle(vehicleName);
        probeMsg.setWave("None");
        
        mIdtoWsManager.postProbe(probeMsg, mPostProbeResponse);
	}
	
	private IdtoWsResponse<ProbeResponse> mPostProbeResponse = new IdtoWsResponse<ProbeResponse>() {
		
		@Override
		public void onIdtoWsResponse(ProbeResponse response) {

		}
		
		@Override
		public void onError(String error) {
			if(error!=null)
				mLogger.error(error);
		}
	};

	@Override
	public void onConnectionFailed(ConnectionResult result) {
		mLogger.error(result.toString());
	}

	@Override
	public void onConnected(Bundle connectionHint) {
		mLocationClient.requestLocationUpdates(mLocationRequest, this);
	}

	@Override
	public void onDisconnected() {
	}

	//private final IBinder mBinder = new LocalBinder();
	@Override
	public IBinder onBind(Intent intent) {
		// TODO Auto-generated method stub
		throw new UnsupportedOperationException("Not yet implemented");
//		return mBinder;
	}
}
