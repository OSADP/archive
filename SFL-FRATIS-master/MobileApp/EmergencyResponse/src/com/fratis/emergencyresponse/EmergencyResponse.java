package com.fratis.emergencyresponse;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList; 
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;
import com.fratis.emergencyresponse.EmergencyResponseService.LocalBinder;
import com.fratis.emergencyresponse.R;
import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesClient.ConnectionCallbacks;
import com.google.android.gms.common.GooglePlayServicesClient.OnConnectionFailedListener;
import com.google.android.gms.location.LocationListener;
import com.google.android.gms.location.LocationRequest;
import com.google.android.gms.location.LocationClient;
import com.google.android.gms.maps.GoogleMap.OnMyLocationButtonClickListener;
import android.net.Uri;
import android.os.AsyncTask;
import android.annotation.SuppressLint;
import android.app.ActivityManager;
import android.app.ActivityManager.RunningServiceInfo;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.ServiceConnection; 
import android.content.pm.PackageManager.NameNotFoundException;
import android.location.Location;
import android.location.LocationManager;
import android.os.Bundle;
import android.os.Environment;
import android.os.IBinder;
import android.os.PowerManager;
import android.support.v4.app.FragmentActivity;
import android.text.InputType;
import android.view.Gravity;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.WindowManager; 
import android.webkit.CookieManager;
import android.webkit.JavascriptInterface;
import android.webkit.WebSettings;
import android.webkit.WebSettings.RenderPriority;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;  
import android.widget.LinearLayout; 
import android.widget.ProgressBar; 
import android.widget.TextView;
import android.graphics.Color;
import android.graphics.PorterDuff;

@SuppressLint("SetJavaScriptEnabled")
public class EmergencyResponse extends FragmentActivity implements
		ConnectionCallbacks, OnConnectionFailedListener, LocationListener,
		OnMyLocationButtonClickListener {
	
	protected MyApplication myApplication;	// Application Object with global variables
	private static FragmentActivity	myContext;
    EmergencyResponseService mService;
    boolean mBound = false;
	
	private LocationClient mLocationClient;
    private boolean trackLocation = false;

    //App Parameters
    private ArrayList<Location> locations = new ArrayList<Location>();
    private boolean isDriving = false;
 
	//Options Menu items
    private MenuItem	mItem_ClearCache;
    private MenuItem	mItem_StartService;
    private MenuItem	mItem_MapType;
    
    //Screen Buttons
    private Button		bLogin;
    private Intent serviceIntent;
    
    private Login phoneLogin;
    
    private ReportUpdater reportUpdater = null;
    private ProgressDialog mProgressDialog;
    
    AlertDialog.Builder GPSbuilder = null;
  
	private WebView mapWebView;	
	private static final LocationRequest REQUEST = new LocationRequest()
			.setInterval(2500) // grab location every 2.5 seconds
			.setFastestInterval(16) // fastest
			.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);

	private boolean isMyServiceRunning() {
	    ActivityManager manager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
	    for (RunningServiceInfo service : manager.getRunningServices(Integer.MAX_VALUE)) {
	        if (EmergencyResponseService.class.getName().equals(service.service.getClassName())) {
	            return true;
	        }
	    }
	    return false;
	}
	
	
	public class WebAppInterface 
	{
	    Context mContext;

	    WebAppInterface(Context c) 
	    {
	        mContext = c;
	    }
	    
	    @JavascriptInterface
	    public void startLocationUpdate()
	    {
			trackLocation = true;
	    }
	    
	    @JavascriptInterface
	    public void stopLocationUpdate()
	    {
			trackLocation = false;
	    }
	    
	    @JavascriptInterface
	    public void locationMapPanned()
	    {
	    	myApplication.Log.i("JavascriptInterface", "Map panned...");
	    	trackLocation = false;
	    }
	    
	    @JavascriptInterface
	    public void toggleService()
	    {
            if(!myApplication.settings.getBoolean("serviceEnable", true))
            {
            	myApplication.settings.edit().putBoolean("serviceEnable", true).apply();
            	
    	    	((Activity) mContext).runOnUiThread(new Runnable() {
    	    	    public void run() 
    	    	    {
    	            	mItem_StartService.setTitle("Tracking Service: [ON]");
    	            	mapWebView.loadUrl("javascript:toggleService(true);");
    	            	if(!isMyServiceRunning())
    	            		startService(serviceIntent);
    		            myApplication.Log.i("EmergencyResponseService", "started");
    	    	    }
    	    	});
            }
            else
            {
            	myApplication.settings.edit().putBoolean("serviceEnable", false).apply();
    	    	((Activity) mContext).runOnUiThread(new Runnable() {
    	    	    public void run() 
    	    	    {
    	            	if(isMyServiceRunning())
    	            		stopService(serviceIntent);
    	            	
    	            	mItem_StartService.setTitle("Tracking Service: [OFF]");
    	            	mapWebView.loadUrl("javascript:toggleService(true);");
    		            myApplication.Log.i("EmergencyResponseService", "stopped");  
    	    	    }
    	    	});
            }
	    }
	    
	    @JavascriptInterface
	    public void windowLoaded()
	    {
	    	myApplication.Log.i("JavascriptInterface", "windowLoaded();");
	    	
	    	// Send phone authorization info on UI thread
	    	((Activity) mContext).runOnUiThread(new Runnable() {
	    	    public void run() 
	    	    {
	    	    	if(myApplication.settings.getBoolean("serviceEnable", true))
	    	    		mapWebView.loadUrl("javascript:toggleService(true);");
	    	    	else
	    	    		mapWebView.loadUrl("javascript:toggleService(false);");
	    	    	phoneAuth();
	    	    } 
	    	}); 	
	    }
	    
	    @JavascriptInterface
	    public void reconReport()
	    {
	    	if(checkGPS()) return;
	    	((Activity) mContext).runOnUiThread(new Runnable() {
	    	    public void run() 
	    	    {
	    	    	Intent intent = new Intent(mContext, WebReportPage.class);
	    	    	intent.putExtra("report", "recon.xml"); //intent.put Extra("file", "recon.xml");
	    	    	startActivity(intent);
	    	    }
	    	});
	    }
	    
	    @JavascriptInterface
	    public void citizenReport()
	    {
	    	if(checkGPS()) return;
	    	((Activity) mContext).runOnUiThread(new Runnable() {
	    	    public void run() 
	    	    {
	    	    	Intent intent = new Intent(mContext, WebReportPage.class);
	    	    	intent.putExtra("report", "citizen.xml"); //intent.putExtra("file", "recon.xml");
	    	    	startActivity(intent);
	    	    }
	    	});
	    }
	    
	    @JavascriptInterface
	    public void businessReport()
	    {
	    	if(checkGPS()) return;
	    	((Activity) mContext).runOnUiThread(new Runnable() {
	    	    public void run() 
	    	    {
	    	    	Intent intent = new Intent(mContext, WebReportPage.class);
	    	    	
	    	    	//Special reports for Depot, Terminal and Port
	    	    	if(myApplication.businessType.equals("depot"))
	    	    		intent.putExtra("report", "depot.xml");
	    	    	else
	    	    		intent.putExtra("report", "business.xml"); 
	    	    	startActivity(intent);
	    	    }
	    	});
	    }
	}
	
	
	private boolean checkGPS()
	{
		final LocationManager manager = (LocationManager) getSystemService( Context.LOCATION_SERVICE );

	    if ( !manager.isProviderEnabled( LocationManager.GPS_PROVIDER ) ) {
	    	if(GPSbuilder == null)
	    	{
			    GPSbuilder = new AlertDialog.Builder(this);
			    GPSbuilder.setMessage("GPS Location Provider disabled. Do you want to enable it?")
			           .setCancelable(false)
			           .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
			               public void onClick(final DialogInterface dialog, final int id) {
			                   startActivity(new Intent(android.provider.Settings.ACTION_LOCATION_SOURCE_SETTINGS));
			                   GPSbuilder = null;
			                   dialog.cancel();
			               }
			           })
			           .setNegativeButton("No", new DialogInterface.OnClickListener() {
			               public void onClick(final DialogInterface dialog, final int id) {
			                    dialog.cancel();
			                    GPSbuilder = null;
			               }
			           });
			    GPSbuilder.create().show();
	    	}
		    return true;
	    }
	    return false;
	}
	
	private void phoneAuth()
	{
		String businessId = myApplication.settings.getString("businessId", "");
		String businessName = myApplication.settings.getString("businessName", "");
		String loginUser = myApplication.settings.getString("loginUser", "");
		String loginPassword = myApplication.settings.getString("loginPassword", "");
		String loginType = myApplication.settings.getString("loginType", "");
		
		//phoneAuth(loginUser, loginPassword, loginType, businessId, businessName)
		String command = "javascript:phoneAuth('" + loginUser + "' , '" + loginPassword + "' , '" + loginType + "' , '" + businessId + "' , '" + businessName + "');";
		mapWebView.loadUrl(command);
		myApplication.Log.i("phoneAuth", command);
	}
	
	private class DownloadTask extends AsyncTask<String, Integer, String> {

	    private Context context;
	    private PowerManager.WakeLock mWakeLock;
	    private File update;

	    public DownloadTask(Context context) { 
	        this.context = context;
	    }

	    @SuppressWarnings("resource")
		@Override
	    protected String doInBackground(String... sUrl) {
	        InputStream input = null;
	        OutputStream output = null;
	        HttpURLConnection connection = null;
	        try {
	            URL url = new URL(sUrl[0]);
	            connection = (HttpURLConnection) url.openConnection();
	            connection.connect();

	            if (connection.getResponseCode() != HttpURLConnection.HTTP_OK) {
	                return "Server returned HTTP " + connection.getResponseCode()
	                        + " " + connection.getResponseMessage();
	            }

	            int fileLength = connection.getContentLength();

	            // download the file
	            input = connection.getInputStream();
	            update = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/update/");
	            update.mkdirs();
	            update = new File(update, "EmergencyResponse.apk");
	            output = new FileOutputStream(update);

	            byte data[] = new byte[4096];
	            long total = 0;
	            int count;
	            while ((count = input.read(data)) != -1) {
	                // allow canceling with back button
	                if (isCancelled()) {
	                    input.close();
	                    output.close();
	                    return null;
	                }
	                total += count;
	                // publishing the progress....
	                if (fileLength > 0) // only if total length is known
	                    publishProgress((int) (total * 100 / fileLength));
	                output.write(data, 0, count);
	            }
	        } catch (Exception e) {
	            return e.toString();
	        } finally {
	            try {
	                if (output != null)
	                    output.close();
	                if (input != null)
	                    input.close();
	            } catch (IOException ignored) {
	            }
	            if (connection != null)
	                connection.disconnect();
	        }
	        return null;
	    }
	    
	    @Override
	    protected void onPreExecute() {
	        super.onPreExecute();
	        
	        PowerManager pm = (PowerManager) context.getSystemService(Context.POWER_SERVICE);
	        mWakeLock = pm.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK,
	             getClass().getName());
	        mWakeLock.acquire();
	        mProgressDialog.show();
	    }

	    @Override
	    protected void onProgressUpdate(Integer... progress) {
	        super.onProgressUpdate(progress);
	        // if we get here, length is known, now set indeterminate to false
	        mProgressDialog.setIndeterminate(false);
	        mProgressDialog.setMax(100);
	        mProgressDialog.setProgress(progress[0]);
	    }

	    @Override
	    protected void onPostExecute(String result) {
	        mWakeLock.release();
	        mProgressDialog.dismiss();
	        if (result != null)
	        	myApplication.Log.i("AppUpdater", "Download error:" + result);
	            //Toast.makeText(context,"Download error: "+result, Toast.LENGTH_LONG).show();
	        else
	        {
	        	myApplication.Log.i("AppUpdater", "Update Downloaded!");
	        	
	        	Intent intent = new Intent(Intent.ACTION_VIEW);
	        	intent.setDataAndType(Uri.fromFile(update), "application/vnd.android.package-archive");
	        	intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
	        	startActivity(intent);
	        	finish();
	        	    
	        }
	    }
	} 
	
	public void DownloadUpdate()
	{
		mProgressDialog = new ProgressDialog(EmergencyResponse.this);
		mProgressDialog.setMessage("Updating app. \nPlease wait...");
		mProgressDialog.setIndeterminate(true);
		mProgressDialog.setProgressStyle(ProgressDialog.STYLE_HORIZONTAL);
		mProgressDialog.setCancelable(true);

		// execute this when the downloader must be fired
		final DownloadTask downloadTask = new DownloadTask(EmergencyResponse.this);
		downloadTask.execute("http://" + myApplication.RemoteHost + "/App/EmergencyResponse.apk");

		mProgressDialog.setOnCancelListener(new DialogInterface.OnCancelListener() {
		    @Override
		    public void onCancel(DialogInterface dialog) {
		        downloadTask.cancel(true);
		    }
		});
	}

    private ServiceConnection mConnection = new ServiceConnection() {

        @Override
        public void onServiceConnected(ComponentName className,
                IBinder service) {
            // We've bound to LocalService, cast the IBinder and get LocalService instance
            LocalBinder binder = (LocalBinder) service;
            mService = binder.getService();
            mService.stopLocationUpdates();
            mBound = true;
            setUpLocationClientIfNeeded();
            
    		if(mLocationClient != null)
    		{
    			myApplication.Log.i("LocationClient", "Resumed, tracking location");
    			mLocationClient.connect();
    			if(mLocationClient.isConnecting())
    				myApplication.Log.i("LocationClient", "Connecting...");
    		}
        }

        @Override
        public void onServiceDisconnected(ComponentName arg0) 
        {
        	mService.startLocationUpdates();
            mBound = false;
        }
    };
	
	
	@SuppressWarnings("deprecation")
	@SuppressLint("InlinedApi")
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		myContext = this;
		myApplication = (MyApplication)getApplication();
        serviceIntent = new Intent(this,EmergencyResponseService.class);
        
        //serviceInte
		if(myApplication.settings.getBoolean("serviceEnable", false) && !isMyServiceRunning())
		{
	    	 myApplication.Log.i("EmergencyResponseService", "Starting Service");
			 startService(serviceIntent);
		}
		
		setContentView(R.layout.activity_emergency_response_web);
		getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
		
		mapWebView = (WebView)findViewById(R.id.webMapView);
		
		mapWebView.getSettings().setJavaScriptEnabled(true);
		mapWebView.addJavascriptInterface(new WebAppInterface(this), "Android");
		mapWebView.getSettings().setRenderPriority(RenderPriority.HIGH);
		mapWebView.getSettings().setLoadWithOverviewMode(true);
		mapWebView.getSettings().setAppCacheMaxSize( 50 * 1024 * 1024);
		mapWebView.getSettings().setAppCachePath(getApplicationContext().getCacheDir().getAbsolutePath());
		mapWebView.getSettings().setAllowFileAccess( true );
		mapWebView.getSettings().setAppCacheEnabled( true );
		mapWebView.getSettings().setCacheMode( WebSettings.LOAD_DEFAULT ); 

		mapWebView.loadUrl("http://" + myApplication.RemoteHost + "/?phone=1");
		mapWebView.setWebViewClient(new WebViewClient());
		CookieManager.getInstance().setAcceptCookie(true);

		bLogin = (Button)findViewById(R.id.btnLogin);
		bLogin.getBackground().setColorFilter(0xFF00FFFF, PorterDuff.Mode.MULTIPLY);
		
		if(!myApplication.settings.getString("loginUser", "none").equals("none")) 
			bLogin.setText("Logout");
			
		if(savedInstanceState == null)         
		{
	        if(reportUpdater == null)
	        {
		        //Update Reports
				reportUpdater = new ReportUpdater(this);
				reportUpdater.execute();
	        }	
		}

		if(isMyServiceRunning())
			bindService(serviceIntent, mConnection, 0);

		setUpLocationClientIfNeeded();
	}

    @Override
	protected void onDestroy() 
    {
		super.onDestroy();
		if(mBound)
    	{
    		mService.startLocationUpdates();
    		unbindService(mConnection);
    	}
	}

	@Override
	protected void onResume() 
	{
		super.onResume();
		
		if(mLocationClient != null)
		{
			myApplication.Log.i("LocationClient", "Resumed, tracking location");
			mLocationClient.connect();
			if(mLocationClient.isConnecting())
				myApplication.Log.i("LocationClient", "Connecting...");
		}

	}

	@Override
	public void onPause() {
		super.onPause(); 
		if (mLocationClient != null) {
			mLocationClient.disconnect();
		}
	} 
	
	@Override
	public void onSaveInstanceState(Bundle savedInstanceState) {
	  super.onSaveInstanceState(savedInstanceState);
	  savedInstanceState.putInt("last_orientation", getResources().getConfiguration().orientation);
	}
	
	@Override
	public void onRestoreInstanceState(Bundle savedInstanceState) {
	  super.onRestoreInstanceState(savedInstanceState);
	}

	private void setUpLocationClientIfNeeded() {
		if (mLocationClient == null) {
			mLocationClient = new LocationClient(EmergencyResponse.myContext, this, this); 
		}
	}


	@Override
	public void onLocationChanged(Location location) 
	{
		if(location.hasSpeed())
		{
			if(locations.size() < 10)
				locations.add(location);
			else
			{
				locations.remove(0);
				locations.add(location);
				float avgSpeed = 0;
				for(Location loc: locations)
					avgSpeed += (loc.getSpeed() * 2.2369); 
				
				avgSpeed = avgSpeed/10;
				
				if(avgSpeed > 7 && !isDriving)
				{
					isDriving = true;
					mapWebView.loadUrl("javascript:drivingStarted();");
				}
				
				if(avgSpeed < 7 && isDriving)
				{
					isDriving = false;
					mapWebView.loadUrl("javascript:drivingEnded();");
				}
			}
		}

		myApplication.setLastLocation(location);
		
    	if(mBound)
    		mService.onLocationChanged(location);

		if(trackLocation)
		{		
			myApplication.Log.i("Location", "Location updated");
			mapWebView.loadUrl("javascript:repositionMap(" + String.valueOf(location.getLatitude()) + "," + String.valueOf(location.getLongitude()) + ")");
		}
	}

	@Override
	public void onConnected(Bundle connectionHint) 
	{
		myApplication.Log.i("LocationClient", "Location Client Connected!");
		mLocationClient.requestLocationUpdates(REQUEST, this);
	}

	@Override
	public void onDisconnected() {

	}

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

	        
    @SuppressWarnings("deprecation")
	public void phoneLogin(View v) 
    {
    	phoneLogin = new Login(this);
    	
		if(!myApplication.settings.getString("loginUser", "none").equals("none"))
		{
			phoneLogin.Logout();
			((Button)v).setText("Login");
			return;
		}

    	// Create container for control
		final LinearLayout controlContainer = new LinearLayout(this);
    	
    	LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT);
    	controlContainer.setLayoutParams(lp);
    	controlContainer.setOrientation(LinearLayout.VERTICAL);
    	
       	final ImageView loginIcon = new ImageView(this);
       	loginIcon.setBackgroundDrawable(this.getResources().getDrawable(R.drawable.login_icon));
       	lp.gravity = Gravity.CENTER_HORIZONTAL;
       	loginIcon.setLayoutParams(lp);
       
       	final ProgressBar loginProgress = new ProgressBar(this);
       	loginProgress.setLayoutParams(lp);
       	loginProgress.setVisibility(ProgressBar.GONE);
       	
       	final TextView loginLabel = new TextView(this);
       	loginLabel.setText("Login:");
       	
       	final EditText login = new EditText(this);
       	login.setInputType(InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS | InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
       	
       	final TextView passwordLabel = new TextView(this);
       	passwordLabel.setText("Password:");
       	
       	final EditText password = new EditText(this);
       	password.setInputType(InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS | InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
       	
    	controlContainer.addView(loginProgress);   
    	controlContainer.addView(loginIcon);   
    	controlContainer.addView(loginLabel);    	
    	controlContainer.addView(login);
    	controlContainer.addView(passwordLabel);
    	controlContainer.addView(password);

       	final TextView loginStatus = new TextView(this);
       	loginStatus.setVisibility(View.GONE);
       	loginStatus.setGravity(Gravity.CENTER);
       	
    	controlContainer.addView(loginStatus);

    	final AlertDialog loginDialog = new AlertDialog.Builder(this)
        .setView(controlContainer)
        .setPositiveButton("Ok", new DialogInterface.OnClickListener() 
        {
            public void onClick(DialogInterface dialog, int whichButton) 
            {
            }
        }).setNegativeButton("Cancel", new DialogInterface.OnClickListener() 
        { public void onClick(DialogInterface dialog, int whichButton) {}}).create();
    	
    	loginDialog.show(); 
    	
    	loginDialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(new View.OnClickListener()
        {
            public void onClick(View v) 
            {    			
            	if(login.getText().toString().matches("") || password.getText().toString().matches(""))
            	{
            		loginStatus.setText("Error: You must enter a login and password!");
	            	
	            	loginStatus.setTextColor(Color.rgb(150, 0, 0));
	            	
	            	loginStatus.setVisibility(View.VISIBLE);
            	}
            	else
            	{
	            	if(phoneLogin.getStatus() != AsyncTask.Status.RUNNING || phoneLogin.getStatus() != AsyncTask.Status.FINISHED)
	            	{
		            	loginIcon.setVisibility(ImageView.GONE);
		            	loginProgress.setVisibility(ProgressBar.VISIBLE);
		            	
		            	phoneLogin.execute(login.getText().toString(), password.getText().toString());
		            	
		            	try 
		            	{
							String result = phoneLogin.get(10, TimeUnit.SECONDS);
							
							//
							String[] results = result.split(",");
							
							if(results[0].equals("valid"))
							{
								myApplication.Log.i("phoneLogin", "Valid Login!");
								
								loginIcon.setVisibility(ImageView.VISIBLE);
				            	loginProgress.setVisibility(ProgressBar.GONE);
				            	loginLabel.setVisibility(ProgressBar.GONE);
				            	login.setVisibility(ProgressBar.GONE);
				            	passwordLabel.setVisibility(ProgressBar.GONE);
				            	password.setVisibility(ProgressBar.GONE);
	
				            	loginDialog.getButton(AlertDialog.BUTTON_NEGATIVE).setVisibility(View.GONE);
				            	loginDialog.getButton(AlertDialog.BUTTON_POSITIVE).setVisibility(View.GONE);
				            	
				            	loginStatus.setTextColor(Color.rgb(0, 150, 0));
				            	loginStatus.setText("Login OK!");
				            	
				            	loginStatus.setVisibility(View.VISIBLE);
				            	
				            	final Timer myTimer = new Timer();
				                myTimer.schedule(new TimerTask() {          
				                    @Override
				                    public void run() {
				                    	runOnUiThread(new Runnable() {
					                    	    public void run() 
					                    	    {
					                    	    	loginDialog.dismiss();
				                    	    		myTimer.cancel();
					                    	    }
				                    	    });
				                    }
	
				                }, 2000, 2000);
				            	
				                mapWebView.reload();
				            	
								bLogin.setText("Logout");
								return;
							}
							else
							{
								myApplication.Log.e("phoneLogin", "Invalid Login!");
								loginIcon.setVisibility(ImageView.VISIBLE);
				            	loginProgress.setVisibility(ProgressBar.GONE);
				            	
				            	if(results[0].equals("noconnectivity"))
				            		loginStatus.setText("Error: can't reach server.");
				            	else if(results[0].equals("invalid"))
				            		loginStatus.setText("Error: invalid login.");
				            	else
				            		loginStatus.setText("Error: something went wrong...");
				            	
				            	loginStatus.setTextColor(Color.rgb(150, 0, 0));
				            	loginStatus.setVisibility(View.VISIBLE);
				            	phoneLogin = new Login(v.getContext());
							}
							
							
						} catch (InterruptedException e){
						} catch (ExecutionException e) {
						} catch (TimeoutException e) {
						}
	            	}
            	}
            }
        });
    }
    
    //-----------------------Options Menu
	@Override
	public boolean onCreateOptionsMenu(Menu menu)
	{
        mItem_ClearCache		= menu.add("Clear Cache [Development]");
        
        if(myApplication.settings.getBoolean("serviceEnable", true))
        	mItem_StartService			= menu.add("Tracking Service: [ON]");
        else
        	mItem_StartService			= menu.add("Tracking Service: [OFF]");
        
		return true;
	}
	
    @Override
    public boolean onOptionsItemSelected(MenuItem item)
    {
    	if (item == mItem_MapType)
    	{
    		myApplication.mapWeb = !myApplication.mapWeb;
    		myApplication.settings.edit().putBoolean("mapWeb", myApplication.mapWeb).apply();
    		
    		if(myApplication.mapWeb)
    			item.setTitle("Map Type: [Web]");
    		else
    			item.setTitle("Map Type: [Android]");
    		
    	    Intent intent = getIntent();
    	    finish();
    	    startActivity(intent);
    	}
    	
    	if (item == mItem_StartService)
    	{
            if(!myApplication.settings.getBoolean("serviceEnable", true))
            {
            	myApplication.settings.edit().putBoolean("serviceEnable", true).apply();
            	mItem_StartService.setTitle("Tracking Service: [ON]");
	           
            	if(!isMyServiceRunning())
            		startService(serviceIntent);
            	
	            myApplication.Log.i("EmergencyResponseService", "started");
            }
            else
            {
            	myApplication.settings.edit().putBoolean("serviceEnable", false).apply();
            	mItem_StartService.setTitle("Tracking Service: [OFF]");
            	
            	if(isMyServiceRunning())
            		stopService(serviceIntent);
            	
	            myApplication.Log.i("EmergencyResponseService", "stopped");
            }
            
            myApplication.Log.i("EmergencyResponseService", "serviceEnable: " + myApplication.settings.getBoolean("serviceEnable", true));
    	}
    	
        if (item == mItem_ClearCache)
        {
    		mapWebView.clearCache(true); 
    		mapWebView.reload();
        }
        
    	return true;
    }

	@Override
	public boolean onMyLocationButtonClick() 
	{
		return false;
	}
}