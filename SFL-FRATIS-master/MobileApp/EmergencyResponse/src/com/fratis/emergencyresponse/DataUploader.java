package com.fratis.emergencyresponse;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.io.StringWriter;
import java.io.Writer;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.io.BufferedReader;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.params.HttpParams;
import org.apache.http.protocol.BasicHttpContext;
import org.apache.http.protocol.HttpContext;
import org.apache.http.util.EntityUtils;
import org.apache.http.entity.mime.content.ByteArrayBody;
import org.apache.http.entity.mime.content.FileBody;
import org.apache.http.entity.mime.content.StringBody;
import org.json.JSONArray;
import org.json.JSONObject;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;


import android.content.Context;
import android.content.SharedPreferences;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.AsyncTask;
import android.os.Environment;
//import android.util.Log;
import android.widget.Toast;

@SuppressWarnings("unused")
public class DataUploader extends AsyncTask<File, Integer, String> 
{
	protected MyApplication myApplication;		// Application Object with global variables
	
	String uploadScript = "";
	Context myContext = null;					//	Calling activity context
	NotificationHelper notify;					//	Notification
	
	public boolean htmlReport = false;			// 	TODO remove this when reports swapped
	private String htmlReportString = "";		//	HTML Report contents
	
	ArrayList<String> reptPhotos;
	
	int UploadAttempt = 0;						//	Attempt Counter
	int MaxAttempts = 5;						//	How many attempts before storing local
	int SleepLength = 5000;	 					//	How much to timeout before next attempt
	public boolean storeLocal = false;			//	Failed to connect, store locally 
	
	boolean deleteAfterUpload = true;			// Delete temporary file after upload?
	
	long totalSize = 0;							//	Total size of the Multipart Entity
	
	ServerCommunicationHelper commHelper = new ServerCommunicationHelper();
	
	private DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
	private DocumentBuilder documentBuilder = null;
	private Document xmlDocument = null;
	private Element reportElement;
	public boolean isGpsUpload = false;
	
	// Grab XML String from Document
	public void loadReportFile(File reportFile)
	{
		myApplication.Log.i("loadReportFile", "Loading File: " + reportFile.getPath());
		if(reportFile.exists())
		{
			myApplication.Log.i("loadReportFile", "Found File ");
			if(!htmlReport)
			{
				myApplication.Log.i("loadReportFile", "Loading XML report: " + reportFile.getPath());
				try 
				{
			        xmlDocument = documentBuilder.parse(reportFile);
		            myApplication.Log.i("loadReportFile", "XML Report Loaded.");
				} catch (SAXException e) {
					e.printStackTrace();
				} catch (IOException e) {
					e.printStackTrace();
				}
			}
			else
			{
				myApplication.Log.i("loadReportFile", "Loading JSON report: " + reportFile.getPath());
		        try {
		        	htmlReportString = ""; // Just in case
		        	
		            BufferedReader in = new BufferedReader(new FileReader(reportFile));
		            String tmpString;
		            while((tmpString = in.readLine()) != null) 
		            	htmlReportString += tmpString;
		            
		            in.close();
		            myApplication.Log.i("loadReportFile", "JSON Report Loaded.");

		        } catch (IOException e) {
		           myApplication.Log.e("loadReportFile", "Error reading file");
		           e.printStackTrace();
		        }
			}
		}
		else
		{
			myApplication.Log.e("loadReportFile", "Critical error. File does not exist: " + reportFile.getPath());
		}
	}

	public DataUploader(Context c, String uploadScript, boolean htmlReport)
	{
		this.htmlReport = htmlReport;
		this.uploadScript = uploadScript;
		myContext = c;
		commHelper.setContext(c);
		
		myApplication = (MyApplication)c.getApplicationContext();
		
		if(!htmlReport)
		{
			try {
				documentBuilder = documentBuilderFactory.newDocumentBuilder();
			} catch (ParserConfigurationException e) {
				e.printStackTrace();
			}
		}
	}
	

	@Override
	protected void onPreExecute()
	{	
		super.onPreExecute();
		notify = new NotificationHelper(myContext, 1);
		notify.createNotification("Data Upload", android.R.drawable.stat_sys_upload);	
		notify.textUpdate("Checking Connection");
	}
	
	@Override
	protected String doInBackground(File... arg0) 
	{
		myApplication.Log.i("DataUploader", "Starting upload for: " + arg0[0].getName());
		
		if(!isGpsUpload)
			loadReportFile(arg0[0]);
		
		while(!commHelper.checkConnectivity(myApplication.RemoteHost, notify))
		{
			if(UploadAttempt < MaxAttempts)
			{
			   try {
				      Thread.sleep(SleepLength);
				    } catch (Exception e) {
				      UploadAttempt = 10;
				      notify.textUpdate("Upload Failed! Storing Local.");
						myApplication.Log.i("DataUploader", "Upload Failed! Storing Local.");
				      storeLocal = true;
				      return "failed";
				    }
			   UploadAttempt++;
			}
			else
			{
				notify.textUpdate("Upload Failed! Storing Local.");
				myApplication.Log.i("DataUploader", "Upload Failed! Storing Local.");
			    storeLocal = true;
			    return "failed";
			}
		}
		
		notify.setIcon(android.R.drawable.stat_sys_upload);
		
		myApplication.Log.i("DataUploader", "Uploading...");
		
		HttpParams httpParameters = new BasicHttpParams();
        HttpConnectionParams.setConnectionTimeout(httpParameters, 100000);
        HttpConnectionParams.setSoTimeout(httpParameters, 100000);
        
		HttpClient httpClient = new DefaultHttpClient(httpParameters);
		HttpContext httpContext = new BasicHttpContext();
		
		HttpPost httpPost = new HttpPost("http://" + myApplication.RemoteHost + "/" + uploadScript);
		
		try
		{
			notify.textUpdate("Uploading...");
			CustomMultiPartEntity.ProgressListener pr = new CustomMultiPartEntity.ProgressListener()
			{
				public void transferred(long num)
				{
					publishProgress((int) ((num / (float) totalSize) * 100));
				}
			};		
			
			//Create Multipart Entity
			myApplication.Log.i("DataUploader", "Creating Multipart Entity...");
			CustomMultiPartEntity multipartContent = new CustomMultiPartEntity(pr);

			if(!htmlReport)
			{
				multipartContent.addPart("Report", new FileBody(arg0[0]));
				
				if(!isGpsUpload)
				{
					//Add photo files
					NodeList xmlButtons = xmlDocument.getElementsByTagName("button");
					for(int n = 0; n < xmlButtons.getLength(); n++)
					{
						Element xmlButton = (Element)xmlButtons.item(n);
						
						if(xmlButton.getAttribute("action").equals("photo"))
						{
							if(xmlButton.hasAttribute("photoFileName"))
							{
								myApplication.Log.i("Data Uploader", "Found Photo Element: " +  xmlButton.getAttribute("photoFileName"));
								//File photoFile= new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp");
								String photoFileName = xmlButton.getAttribute("photoFileName");
								File photoFile = new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp/" + photoFileName + ".jpg");
								if(photoFile.exists())
								{
									myApplication.Log.i("Data Uploader", "Adding Photo: " + photoFile.getName());
									multipartContent.addPart(photoFileName, new FileBody(photoFile));
								}
							}
								
						}
					}
				}
			} 
			else 
			{
				multipartContent.addPart("reportType", new StringBody("business"));
				multipartContent.addPart("jsonReport", new StringBody(htmlReportString));
				
				JSONArray jArray = new JSONArray(htmlReportString);
				
				reptPhotos = new ArrayList<String>();
				 
				
				for(int e = 0; e < jArray.length(); e++)
				{
					JSONObject entry = (JSONObject) jArray.get(e);
					if(entry.has("damagePhoto"))
					{
						if(!entry.getString("damagePhoto").equals("") && !reptPhotos.contains(entry.getString("damagePhoto")))
						{
							myApplication.Log.i("DataUploader", "Attempting to attach photo to Multipart Entity: " + entry.getString("damagePhoto"));
							reptPhotos.add(entry.getString("damagePhoto"));
							File reptPhoto = new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp/" + entry.getString("damagePhoto") + ".jpg");
							
							if(reptPhoto.exists()) 
							{
								myApplication.Log.i("DataUploader", "Attached Photo: " + entry.getString("damagePhoto"));
								multipartContent.addPart(entry.getString("damagePhoto"), new FileBody(reptPhoto));
							}
						}  
					}
				}
			}
			
			totalSize = multipartContent.getContentLength();  
			
			// Send it 
			httpPost.setEntity(multipartContent);

			myApplication.Log.i("DataUploader", "Sending Data: " + totalSize); 

			HttpResponse response = httpClient.execute(httpPost, httpContext);
			String serverResponse = EntityUtils.toString(response.getEntity());
			
			myApplication.Log.i("DataUploader", "Server Response: " + serverResponse);
			

			if(deleteAfterUpload && !storeLocal)
			{
				arg0[0].delete();
				
				if(!isGpsUpload)
				{
					for(int p = 0; p < reptPhotos.size(); p++)
					{
						File reptPhoto = new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp/" + reptPhotos.get(p));
						if(reptPhoto.exists())
							reptPhoto.delete();
	
					}
				}
			}
			return "ok";
		}
		catch (Exception e)
		{
			e.printStackTrace();
			storeLocal = true;
		}
			
		return null;
	}

	@Override
	protected void onProgressUpdate(Integer... progress)
	{
		super.onProgressUpdate(progress);
		notify.progressUpdate((Integer) (progress[0]));
	}
	
	@Override
	protected void onPostExecute(String result)
	{
		if(storeLocal)
			try {Thread.sleep(10000);} catch (InterruptedException e) { e.printStackTrace(); }

		notify.completed();
		Toast.makeText(myContext, "Upload Complete!", Toast.LENGTH_LONG).show();
		myApplication.Log.i("DataUploader", "Upload Complete!");
	}
}