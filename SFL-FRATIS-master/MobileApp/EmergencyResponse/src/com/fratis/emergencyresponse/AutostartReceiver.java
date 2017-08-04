package com.fratis.emergencyresponse;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.util.Log;

public class AutostartReceiver extends BroadcastReceiver 
{
	public SharedPreferences settings;
	public static final String PREFS_NAME = "EmergencyResponse";

	@Override
	public void onReceive(Context arg0, Intent arg1) 
	{
		settings = arg0.getSharedPreferences(PREFS_NAME, 0);
		
		if(settings.getBoolean("serviceEnable", true))
		{
	        Intent intent = new Intent(arg0,EmergencyResponseService.class);
	        arg0.startService(intent);
	        Log.i("EmergencyResponseService", "started");
		}
	}
}
