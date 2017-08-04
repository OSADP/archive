package com.fratis.emergencyresponse;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.MalformedURLException;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

import android.net.Uri;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.pm.ApplicationInfo;
import android.content.res.AssetManager;
import android.media.MediaScannerConnection;
import android.os.AsyncTask;
import android.os.Environment;
import android.app.Activity;

public class ReportUpdater extends AsyncTask<String, Integer, String> 
{
	protected MyApplication myApplication;	// Application Object with global variables
	String scriptFile = "reportList.php";
	Context myContext = null;					//	Calling activity context
	NotificationHelper notify;					//	Notification
	AlertDialog.Builder Updatebuilder = null;
	ServerCommunicationHelper commHelper = new ServerCommunicationHelper();
	File sdCard = Environment.getExternalStorageDirectory();
	
	public ReportUpdater(Context c) 
	{
		myContext = c;
		myApplication = (MyApplication) c.getApplicationContext();
		commHelper.setContext(c);
	}
	
	private void copyFileOrDir(String path) {
	    AssetManager assetManager = myContext.getAssets();
	    String assets[] = null;
	    try {
	        assets = assetManager.list(path);
	        if(assets.length > 0)
	        	myApplication.Log.i("copyFileOrDir", "Assets: " + assets[0]);
	        
	        if (assets.length == 0) { 
	            copyFile(path);
	        } else {
	            File dir = new File (Environment.getExternalStorageDirectory() + "/EmergencyResponse/htmlReports/" + path);
	            if (!dir.exists())
	                dir.mkdir();
 
	            for (int i = 0; i < assets.length; ++i)
	                copyFileOrDir(path + "/" + assets[i]);
	        }
	    } catch (IOException ex) {
	        System.out.println("Exception in copyFileOrDir"+ex);
	    }
	}

	private void copyFile(String filename) 
	{
	    AssetManager assetManager = myContext.getAssets();
	    InputStream in = null;
	    OutputStream out = null;
	    try {
	        in = assetManager.open(filename);
	        String newFileName = Environment.getExternalStorageDirectory() + "/EmergencyResponse/htmlReports/" + filename;
	        out = new FileOutputStream(newFileName);
	        byte[] buffer = new byte[1024];
	        int read;
	        
	        while ((read = in.read(buffer)) != -1)
	            out.write(buffer, 0, read);
	        
	        in.close();
	        in = null;
	        out.flush();
	        out.close();
	        out = null;
	    } catch (Exception e) {
	        System.out.println("Exception in copyFile"+e);
	    }

	}
	
	void DeleteRecursive(File fileOrDirectory) {
	    if (fileOrDirectory.isDirectory())
	        for (File child : fileOrDirectory.listFiles())
	            DeleteRecursive(child);

	    fileOrDirectory.delete();
	}
	 
	
	public void checkAppUpdates()
	{ 
		  try{
			     ApplicationInfo ai = myContext.getPackageManager().getApplicationInfo(myContext.getPackageName(), 0);
			     ZipFile zf = new ZipFile(ai.sourceDir);
			     ZipEntry ze = zf.getEntry("classes.dex");
			     Long time = ze.getTime();
			     
			     SimpleDateFormat  format = new SimpleDateFormat("dd/MM/yyyy kk:mm:ss", Locale.US); 
			     String current = (String) android.text.format.DateFormat.format("dd/MM/yyyy kk:mm:ss",new java.util.Date(time));
			     
			     myApplication.Log.i("AppUpdate", "Current app version: " + current);  
			     
			     Date currentDate = format.parse(current);     
			     String latest = commHelper.executeReq(new URL("http://" + myApplication.RemoteHost + "/app"));
			     Date latestDate = format.parse(latest);     
			     zf.close();
			     
			     myApplication.Log.i("AppUpdate", "Latest app version: " + latest);
			     
			     if(currentDate.before(latestDate))
			     {
				     myApplication.Log.i("AppUpdate", "Update Required!");
				     DeleteRecursive(new File (Environment.getExternalStorageDirectory() + "/EmergencyResponse")); 

				     if(Updatebuilder == null)
		    		 {
					     ((Activity) myContext).runOnUiThread(new Runnable() {
					    	  public void run()
					    	  {  
							    	Updatebuilder = new AlertDialog.Builder(myContext); 
							    	Updatebuilder.setMessage("App update available.\nDownload and install?").setPositiveButton("Yes",  new DialogInterface.OnClickListener() 
							    	{
			                            public void onClick(DialogInterface dialog, int which) 
			                            {  
			                            	((EmergencyResponse)myContext).DownloadUpdate();
			                            }
							    	}).setNegativeButton("No", null).show(); 
					    	  }
					    	});
		    		 }
		     	}
			  }catch(Exception e)
			  {
				  e.printStackTrace();
			  }
	}

	public void UpdateReport(File report, boolean html)
	{
		try {
			URL url;
			if(html)
			{
				url = new URL("http://" + myApplication.RemoteHost + "/reportpage.php?report=" + report.getName() + "&phone=1");
				myApplication.Log.i("UpdateReport", "Report file: " + report.getName());
			} 
			else
				url = new URL("http://" + myApplication.RemoteHost + "/reports/" + report.getName());
			
			myApplication.Log.i("Report Update", "Updating Report: " + url.toString());

			String reportData = commHelper.executeReq(url);
			
			if(html)
				report = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/htmlReports/" + report.getName().replace(".xml", ".html"));
			
			PrintWriter writer = new PrintWriter(report);
			writer.print(reportData); 
			writer.close();
			
			// Inform Media Scanner that a new file has been added... it's so weird that you have to do this on android...
	       	MediaScannerConnection.scanFile(myContext,
	       	          new String[] { report.toString() }, null,
	       	          new MediaScannerConnection.OnScanCompletedListener() {
	       	      public void onScanCompleted(String path, Uri uri) {
	
	       	      }
	       	 });
			myApplication.Log.i("Report Update", "Updated Report: " + report);
		}
		catch(Exception e)
		{
			e.printStackTrace();
			myApplication.Log.e("Report Update", "Failed to update report: " + report);
		}
	} 

	@Override
	protected String doInBackground(String... scriptPath)  
	{
		notify = new NotificationHelper(myContext, 2);
		notify.createNotification("Checking for updates...", android.R.drawable.stat_sys_download);	
				
		//Check for App update
		checkAppUpdates();
		
		File htmlReportFile = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/htmlReports/");
		File reportFile = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/");
		reportFile.mkdirs();
		htmlReportFile.mkdirs();
		
		copyFileOrDir("css");
		copyFileOrDir("js");
		copyFileOrDir("images"); 
	
		myApplication.Log.i("getFileList", "Checking Connectivity: " + myApplication.RemoteHost);
		
		if(commHelper.checkConnectivity(myApplication.RemoteHost, notify))
		{
			notify.textUpdate("Checking for app updates...");
			checkAppUpdates();
			notify.textUpdate("Checking for updated reports...");
			
    	    URL url;
			try {
				url = new URL("http://" + myApplication.RemoteHost + "/" + scriptFile);
				myApplication.Log.i("getFileList", "Executing: " + url.toString());
				
				String list = commHelper.executeReq(url);
				String[] reports = list.split("\n");
				String report = "";
				String size = "";
				String[] rp;
				
				for(String r: reports)
				{
					try {
						rp = r.split(",");
						report = rp[0];
						size = rp[1];
						
						reportFile = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/" + report);
						htmlReportFile = new File(Environment.getExternalStorageDirectory() + "/EmergencyResponse/htmlReports/" + report);
						if(!reportFile.exists() || !String.valueOf(reportFile.length()).contains(size))
						{
							// Download new file
							myApplication.Log.i("Report Update", "Updating Report: " + report);
							UpdateReport(reportFile, false);
							UpdateReport(htmlReportFile, true);
						}
					}
					catch(Exception e)
					{
						myApplication.Log.e("Report Update", "Wrong report list format from server!");
					}
				}
				
			} catch (MalformedURLException e) {
				e.printStackTrace(); 
			} catch (IOException e) {
				e.printStackTrace();
			}
    	    
		}
		else
		{
			myApplication.Log.e("getFileList", "Failed To Connect: " + myApplication.RemoteHost);
		}
		notify.completed();	
		return null;
	}
}