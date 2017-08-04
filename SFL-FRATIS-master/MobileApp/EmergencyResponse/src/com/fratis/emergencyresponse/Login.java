package com.fratis.emergencyresponse;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLEncoder;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import android.content.Context;
import android.os.AsyncTask;

public class Login extends AsyncTask <String, String, String> 
{
	protected MyApplication myApplication;
	@SuppressWarnings("unused")
	private Context myContext;
	private ServerCommunicationHelper commHelper = new ServerCommunicationHelper();

	// Generates SHA1 hash
	String sha1Hash( String toHash )
	{
	    String hash = null;
	    try
	    {
	        MessageDigest digest = MessageDigest.getInstance( "SHA-1" );
	        byte[] bytes = toHash.getBytes("UTF-8");
	        digest.update(bytes, 0, bytes.length);
	        bytes = digest.digest();
	        StringBuilder sb = new StringBuilder();
	        for( byte b : bytes )
	        {
	            sb.append( String.format("%02X", b) );
	        }
	        hash = sb.toString();
	        myApplication.Log.i("sha1Hash", "Generated Hash: " + hash);
	    }
	    catch( NoSuchAlgorithmException e )
	    {
	    	myApplication.Log.e("sha1Hash", "No such algorithm: SHA-1");
	        e.printStackTrace();
	    }
	    catch( UnsupportedEncodingException e )
	    {
	    	myApplication.Log.e("sha1Hash", "Unsopported Encoding");
	        e.printStackTrace();
	    }
	    return hash;
	}
	
	public Login(Context c)
	{
		myContext = c;
		commHelper.setContext(c);
		myApplication = (MyApplication)c.getApplicationContext();
	} 
	
	public void Logout()
	{
		myApplication.settings.edit().putString("loginType", "civillian").apply();
		myApplication.settings.edit().putString("loginUser", "none").apply();
		myApplication.settings.edit().putString("loginPassword", "");
		
		myApplication.LoginType = "civillian";
		myApplication.businessName = "";
		myApplication.businessId = "";
	}
	
	@Override
	protected String doInBackground(String... arg0) 
	{
		String response = "invalid";
		myApplication.Log.i("Login", "Checking Connectivity...");
		if(arg0.length == 2)
		{
			if(arg0[0].charAt(arg0[0].length() - 1) == ' ')
				arg0[0] = arg0[0].substring(0, arg0[0].length() - 1);
			
			if(arg0[1].charAt(arg0[1].length() - 1) == ' ')
				arg0[1] = arg0[1].substring(0, arg0[1].length() - 1);
			
			
			if(commHelper.checkConnectivity(myApplication.RemoteHost, null))
			{
				myApplication.Log.i("Login", "Connected to server...");
				
				try {
					URL loginURL = new URL("http://" + myApplication.RemoteHost + "/user_functions.php?func=phone_login&login=" + URLEncoder.encode(arg0[0], "utf-8") + "&password=" +  URLEncoder.encode(arg0[1], "utf-8"));
					myApplication.Log.i("Login", loginURL.toString());
					
					response = commHelper.executeReq(loginURL);
					myApplication.Log.i("Login", "Response:" + response);
					
					String res[] = response.split(",");
					if(res.length >= 5)
					{ 
						response = res[0];
						if(res[1].equals("recon") || res[1].equals("business"));
						{
							myApplication.settings.edit().putString("loginType", res[1]).apply();
							myApplication.settings.edit().putString("loginUser", arg0[0]).apply();
							myApplication.settings.edit().putString("loginPassword", sha1Hash(arg0[1]));
							myApplication.settings.edit().putString("businessId", res[2]).apply();
							myApplication.settings.edit().putString("businessName", res[3]).apply();
							myApplication.settings.edit().putString("businessType", res[4]).apply();
							myApplication.LoginType = res[1];
							myApplication.businessId = res[2];
							myApplication.businessName = res[3];
							myApplication.businessType = res[4];
						}
					}
				} catch (MalformedURLException e) {
					response = "error";
					e.printStackTrace();
				} catch (IOException e) {
					response = "error";
					e.printStackTrace();
				}
			}
			else
				response = "noconnectivity";
		}
		return response;
	}
}