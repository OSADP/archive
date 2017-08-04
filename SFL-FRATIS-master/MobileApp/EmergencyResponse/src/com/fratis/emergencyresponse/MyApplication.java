
package com.fratis.emergencyresponse;

import android.app.Application;
import android.content.SharedPreferences;
import android.location.Location;


public class MyApplication extends Application 
{
	public static final String PREFS_NAME = "EmergencyResponse";
	private static Location lastLocation;
    public String RemoteHost 	= "140.142.198.59";
    public String LoginType		= "civillian";
    public String businessId	= "";
    public String businessName	= "";
    public String businessType	= "";
	public SharedPreferences settings;
	public LogWriter Log;
	public boolean mapWeb = true;
	
	@Override
	public void onCreate()
	{
		super.onCreate();
		
		Log = new LogWriter(this);
		settings = getSharedPreferences(PREFS_NAME, 0);
		LoginType = settings.getString("loginType", "civillian");
		businessId = settings.getString("businessId", "");
		businessName = settings.getString("businessName", "");
		Log.i("myApplication", "businessName: " + businessName);
		Log.i("myApplication", "businessId: " + businessId);
		mapWeb = settings.getBoolean("mapWeb", true);
		lastLocation = new Location("dummy");
	}
	 
	@Override
	public void onTerminate() 
	{
		super.onTerminate();
		
		settings.edit().putString("loginType", LoginType).apply();
		settings.edit().putBoolean("mapWeb", mapWeb).apply();
	}

	public Location getLastLocation()
	{
		return lastLocation;
	}
	public void setLastLocation(Location lastLocation)
	{
		MyApplication.lastLocation = lastLocation;
	}
}