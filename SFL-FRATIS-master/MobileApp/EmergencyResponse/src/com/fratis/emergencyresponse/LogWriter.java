package com.fratis.emergencyresponse;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;

import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;

import android.content.Context;
import android.os.AsyncTask;
import android.telephony.TelephonyManager;
import android.util.Log;

public class LogWriter 
{
	protected MyApplication myApplication;	// Application Object with global variables
	private final String tmDevice, tmSerial, androidId, tmNumber;
	private String deviceString;
	
	private class logSender extends AsyncTask<String, Void, String> 
	{
		private String tag, type;
	    @Override
	    protected String doInBackground(String... message) 
	    {
	      	String response = "";
	      	
	      	String getUrl = "http://" + myApplication.RemoteHost + "/androidlog.php?device=" + deviceString + "&type=" + type + "&tag=" + tag + "&text=" + message[0];
	      	
	        DefaultHttpClient client = new DefaultHttpClient();
	        HttpGet httpGet = new HttpGet(getUrl);
	        try {
	          client.execute(httpGet);
	        } catch (Exception e) {
	          e.printStackTrace();
	        }
	      return response;
	    }
	}  
	
	public LogWriter(MyApplication myThis) 
	{
	    final TelephonyManager tm = (TelephonyManager) myThis.getSystemService(Context.TELEPHONY_SERVICE);
	    
	    myApplication = myThis;

	    tmNumber = "" + tm.getLine1Number();
	    tmDevice = "" + tm.getDeviceId();
	    tmSerial = "" + tm.getSimSerialNumber();
	    androidId = "" + android.provider.Settings.Secure.getString(myThis.getContentResolver(), android.provider.Settings.Secure.ANDROID_ID);
	    deviceString = tmNumber + "-" + tmDevice + "-" + tmSerial + "-" + androidId;
	    
	    try {
			deviceString = URLEncoder.encode(deviceString, "utf-8");
		} catch (UnsupportedEncodingException e) 
		{
			e.printStackTrace();
		}
	}
	
	public void log(String type, String tag, String text)
	{
		try
		{
			logSender info = new logSender();
			info.tag = URLEncoder.encode(tag, "utf-8");
			info.type = type;
			info.execute(URLEncoder.encode(text, "utf-8"));
		}catch(Exception e)
		{ Log.e("Log Sender", "Couldn't send log"); }
	}
	
	public void i(String tag, String text )
	{
		Log.i(tag, text);
		log("i", tag, text);
	}
	
	public void e(String tag, String text )
	{
		Log.e(tag, text);
		log("e", tag, text);
	}
	
	public void d(String tag, String text )
	{
		Log.d(tag, text);
		log("d", tag, text);
	}
}
