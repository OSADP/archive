package com.fratis.emergencyresponse;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileWriter; 
import java.io.IOException;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesClient.ConnectionCallbacks;
import com.google.android.gms.common.GooglePlayServicesClient.OnConnectionFailedListener;
import com.google.android.gms.location.LocationListener;
import com.google.android.gms.location.LocationRequest;
import com.google.android.gms.location.LocationClient;


import android.app.Service;
import android.content.Intent;
import android.content.pm.PackageManager.NameNotFoundException;
import android.location.Location;
import android.os.Binder;
import android.os.Bundle;
import android.os.Environment;
import android.os.IBinder;

public class EmergencyResponseService extends Service implements LocationListener, ConnectionCallbacks, OnConnectionFailedListener
{
	// Fixed timeout scheduler
	protected MyApplication myApplication;	// Application Object with global variables
	private LocationClient mLocationClient;
	private String	storagePath = Environment.getExternalStorageDirectory().toString();
	private File	gpsFile;
	private int gpsCount = 0;
	NotificationHelper notify;
	Location lastLocation = null;
	
	// Slow location request
	private static final LocationRequest REQUEST = LocationRequest.create()
			.setInterval(15000) // grab location every 10 seconds
			.setFastestInterval(15000) // fastest
			.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY)
			.setSmallestDisplacement(25);
	
    public class LocalBinder extends Binder 
    {
    	EmergencyResponseService getService() 
    	{
            return EmergencyResponseService.this;
        }
    }
			
    private final IBinder mBinder = new LocalBinder();
	
	@Override
	public IBinder onBind(Intent arg0) 
	{
		return mBinder;
	}
	
    @Override
    public void onDestroy() 
    {
		notify.completed(); 
		mLocationClient.disconnect();
		
    	super.onDestroy();
    }
    
    public void newFile()
    {
    	SimpleDateFormat formatter = new SimpleDateFormat("yyyy_MM_dd_HH_mm_ss", Locale.US);
    	Date now = new Date();
    	String fileDate = formatter.format(now);
        gpsFile = new File(storagePath + "/EmergencyResponse/Gps/gps_" + fileDate + ".log");

        myApplication.Log.i("Emergency Service", "New File: " + gpsFile.getName());
        
        if (!gpsFile.exists())
        {
        	try
	        {
	        	gpsFile.createNewFile();
	        } 
	        catch (IOException e)
	        {
	           e.printStackTrace();
	        }
        }
    }
    
    @Override 
    public void onStart(Intent intent, int startid)
    {
    	myApplication = (MyApplication)getApplication();  
    	myApplication.Log.i("EmergencyResponseService", "onStart");

		notify = new NotificationHelper(this, 14);
		notify.createNotification("Emergency Service", android.R.drawable.stat_notify_sync);	
		notify.setIcon(android.R.drawable.stat_notify_sync);
		notify.textUpdate("Started");
        
        myApplication.Log.i("EmergencyResponseService", "Service Started!");

        gpsFile = new File(storagePath + "/EmergencyResponse/Gps");
        gpsFile.mkdirs();
        newFile();
        startLocationUpdates();
    }
    
    public void startLocationUpdates()
    {
        setUpLocationClientIfNeeded();
        mLocationClient.connect();
    }
    
    public void stopLocationUpdates()
    {
    	if(mLocationClient.isConnected())
    	{
    		mLocationClient.disconnect();
    		mLocationClient = null;
    	}
    }
    
    @Override
    public int onStartCommand(Intent intent, int flags, int startId) 
    {
        return super.onStartCommand(intent, flags, startId);
    }
    
	private void setUpLocationClientIfNeeded() {
        myApplication.Log.i("EmergencyResponseService", "Configuring Location Services");
		if (mLocationClient == null) {
			mLocationClient = new LocationClient(getApplicationContext(), this, this); 
		}
	}
	
	public void logGps(Location location)
	{

		String gpsString = 	  String.valueOf(location.getLatitude()) + "," //Latitude
							+ String.valueOf(location.getLongitude()) + "," //Longitude
							+ String.valueOf(location.getSpeed()) + ","   //Speed in MPH
							+ String.valueOf(location.getBearing()) + "," //Direction
							+ String.valueOf(location.getAccuracy()) + "," //Accuracy
							+ android.text.format.DateFormat.format("yyyy-MM-dd-hh-mm-ss", new java.util.Date()); // DateTime
		
        myApplication.Log.i("EmergencyResponseService", "Logging GPS: " + gpsString);
		
		try {
		      //BufferedWriter for performance, true to set append to file flag
		      BufferedWriter buf = new BufferedWriter(new FileWriter(gpsFile, true)); 
		      buf.append(gpsString + "\r\n");
		      //buf.newLine();   
		      buf.close();
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
	
	@Override
	public void onLocationChanged(Location location) 
	{

		myApplication.setLastLocation(location);
		myApplication.Log.i("EmergencyResponseService", "GPS Update...");
		
		if(gpsCount < 6) //ten minutes (hopefully)
		{
			if(location.hasSpeed() && location.getAccuracy() <= 100.0)
			{
				notify.textUpdate("Speed: " + (int)(location.getSpeed() * 2.2369) + " " + gpsCount + "/6");
				logGps(location);
				lastLocation = location;
				gpsCount++;
			}
		}
		else
		{
			DataUploader gpsUpload = new DataUploader(this, "gpsupload.php", false);
			gpsUpload.isGpsUpload = true;
			gpsUpload.execute(this.gpsFile);
			
			newFile();
			gpsCount = 0;
		}
	}
	
	@Override
	public void onConnected(Bundle connectionHint) {
        myApplication.Log.i("EmergencyResponseService", "Connected, requesting updates");
		notify.textUpdate("Connected");
		mLocationClient.requestLocationUpdates(REQUEST, this);
	}

	@Override
	public void onDisconnected() {}

	@Override
	public void onConnectionFailed(ConnectionResult result) 
	{
		int v = 0;
		try {
			v = getPackageManager().getPackageInfo("com.google.android.gms", 0 ).versionCode;
		} catch (NameNotFoundException e) 
		{
			myApplication.Log.d("LocationClient", "Can't get google play version!");
		}
		myApplication.Log.e("LocationClient", "onConnectionFailed: " + result.getErrorCode() + " GooglePlayServices:" + v);
	}	
}
