package com.fratis.emergencyresponse;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.io.StringWriter;
import java.io.Writer;
import java.net.HttpURLConnection;
import java.net.URL;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.util.Log;

public class ServerCommunicationHelper 
{
	Context myContext = null;	
	
	//	MUST SPECIFY CONTEXT!!!!!
	public void setContext(Context c)
	{
		myContext = c;
	}
	
	//	Utility function for reading server response from String Stream
    public String convertStreamToString(InputStream is)
            throws IOException {
        if (is != null) {
            Writer writer = new StringWriter();
 
            char[] buffer = new char[1024];
            try {
                Reader reader = new BufferedReader(
                        new InputStreamReader(is, "UTF-8"));
                int n;
                while ((n = reader.read(buffer)) != -1) {
                    writer.write(buffer, 0, n);
                }
            } finally {
                is.close();
            }
            return writer.toString();
        } else {        
            return "";
        }
    }
	
    //	Executes http request and reads response. Only used to check connectivity
    public String executeReq(URL urlObject) throws IOException
    {
	    HttpURLConnection conn = null;

	    try {
			conn = (HttpURLConnection) urlObject.openConnection();
    	    conn.setReadTimeout(5000);//milliseconds
    	    conn.setConnectTimeout(5000);//milliseconds
    	    conn.setRequestMethod("GET");
    	    conn.setDoInput(true);
    	    conn.connect();
    	    return convertStreamToString(conn.getInputStream());

		} catch (IOException e) {
			e.printStackTrace();
		}
	    return "error";
	}

	public boolean checkConnectivity(String RemoteHost, NotificationHelper notify)
	{
	    ConnectivityManager cm = (ConnectivityManager) myContext.getSystemService(Context.CONNECTIVITY_SERVICE);
	    NetworkInfo ni = cm.getActiveNetworkInfo();
	    
	    if(ni != null) 
	    {
	    	if(notify != null)
	    		notify.textUpdate("Checking Connectivity");
	    	
	    	if(ni.isConnected())
	    	{
				Log.i("checkConnectivity", "Network Active");
		    	if(notify != null)
		    		notify.textUpdate("Checking Server...");
		    	
		    	try{
		    	    URL url = new URL("http://" + RemoteHost);
					Log.i("checkConnectivity", "Checking Server: " + url.toString());
		    	    if(!executeReq(url).contains("Emergency"))
		    	    {
		    	    	if(notify != null)
		    	    		notify.textUpdate("Server Error!");
						Log.i("checkConnectivity", "Server Error!");
				    	if(notify != null)
				    		notify.setIcon(android.R.drawable.stat_notify_error);
		    	    	return false;
		    	    }
		    	    
		    	    return true;
		    	    }catch(Exception e){
		    	    	if(notify != null)
		    	    		notify.textUpdate("Server Error!");
						Log.i("checkConnectivity", "Server Error!");
				    	if(notify != null)
				    		notify.setIcon(android.R.drawable.stat_notify_error);
		    	    	e.printStackTrace();
		    	    	return false;
		    	    }
	    	}
	    }
	    else
	    {
			Log.i("checkConnectivity", "Network Error!");
	    	if(notify != null)
	    		notify.setIcon(android.R.drawable.stat_notify_error);
	    	return false;
	    }
	    
	    return (null != ni);
	}
}