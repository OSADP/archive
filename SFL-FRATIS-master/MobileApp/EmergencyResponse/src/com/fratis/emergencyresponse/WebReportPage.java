package com.fratis.emergencyresponse;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException; 

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.webkit.JavascriptInterface;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.webkit.WebSettings.RenderPriority;

@SuppressLint({ "SetJavaScriptEnabled", "NewApi", "InlinedApi" })
public class WebReportPage extends Activity 
{
	protected MyApplication myApplication;	// Application Object with global variables
	
	private WebView reportView;				// Report Web View
	private String reportFileName;			// Report File Name		
	private String outputFileName;			// Report JSON Output
	private String	storagePath = Environment.getExternalStorageDirectory().toString();
	
	private static final int photoIntent = 135;
	private Uri photoUri;

	// ----------------Javascript interface	---------------
	public class WebAppInterface 
	{
	    Context mContext;

	    WebAppInterface(Context c) 
	    {
	        mContext = c;
	    }
	    
	    private void storeReport(File reportFile, String data) {
	        try {
	            FileWriter out = new FileWriter(reportFile);
	            out.write(data);
	            out.close();
	        } catch (IOException e) {
	           myApplication.Log.e("storeReport", "Error");
	           e.printStackTrace();
	        }
	    }
	    
	    @JavascriptInterface
	    public String getReconTeam() 
	    {
	    	String rcn = myApplication.settings.getString("reconTeam", "");
	    	myApplication.Log.i("JavascriptInterface", "getReconTeam: " + rcn);
	    	return rcn;
	    }
	    
	    @JavascriptInterface
	    public String getLastLocation()  
	    {
	    	String loc = String.valueOf(myApplication.getLastLocation().getLatitude()) + "," + String.valueOf(myApplication.getLastLocation().getLongitude());
	    	myApplication.Log.i("JavascriptInterface", "getLastLocation: " + loc); 
	    	return loc;
	    }
	     
	    @JavascriptInterface  
	    public void setReconTeam(String reconTeam) 
	    {
	    	myApplication.settings.edit().putString("reconTeam", reconTeam).apply();
	    }

	    @JavascriptInterface
	    public void processReport(String report) 
	    {
	    	File reportOutput = new File(storagePath + "/EmergencyResponse/Reports/" + outputFileName);
	    	reportOutput.getParentFile().mkdirs(); // Create directories if need to
	    	storeReport(reportOutput, report);  
	    	DataUploader reportUploader = new DataUploader(mContext, "reportpage.php?post=true&report=" + reportFileName, true);
	    	reportUploader.execute(reportOutput);
	    	finish();
	    	
	        myApplication.Log.i("JavascriptInterface", "Storing report to: " + reportOutput.toString());
	    }
	    
	    @JavascriptInterface
	    public void windowLoaded()
	    {
	    	myApplication.Log.i("JavascriptInterface", "windowLoaded();");
	    	// Send phone authorization info on UI thread
	    	((Activity) mContext).runOnUiThread(new Runnable() {
	    	    public void run() 
	    	    {
	    	    	phoneAuth();
	    	    }
	    	});  	
	    }
	    
	    @JavascriptInterface
	    public String capturePhoto(String tagId)
	    {
		    Intent intent = new Intent(MediaStore.ACTION_IMAGE_CAPTURE);
		    File photoFile= new File(Environment.getExternalStorageDirectory().toString() + "/EmergencyResponse/Tmp");
		    photoFile.mkdirs();
		    String photoFileName = outputFileName.replace(".json",  "") + "-" + tagId.replace("Photo", "");
		    photoFile = new File (photoFile, photoFileName + ".jpg"); 
		    if (photoFile.exists ()) photoFile.delete (); 	
		    
		    try {
				photoFile.createNewFile();
			} catch (IOException e) {
				e.printStackTrace();
			}
		    
		    photoUri = Uri.fromFile(photoFile);
		    intent.putExtra(MediaStore.EXTRA_OUTPUT, photoUri); 
		    startActivityForResult(intent, photoIntent);
	    	return photoFileName;
	    }
	}
	
	//-------------------------------------------------------------
	private void phoneAuth()
	{
		String businessId = myApplication.settings.getString("businessId", "");
		String businessName = myApplication.settings.getString("businessName", "");
		String loginUser = myApplication.settings.getString("loginUser", "");
		String loginPassword = myApplication.settings.getString("loginPassword", "");
		String loginType = myApplication.settings.getString("loginType", "");
		String command = "javascript:phoneAuth('" + loginUser + "' , '" + loginPassword + "' , '" + loginType + "' , '" + businessId + "' , '" + businessName + "');";
		reportView.loadUrl(command);
		
		myApplication.Log.i("phoneAuth", command);
	}
	
	@Override
	protected void onActivityResult(int request, int result, Intent intent) 
	{
		super.onActivityResult(request, result, intent);
	}
	
	@SuppressWarnings("deprecation")
	@Override
	protected void onCreate(Bundle savedInstanceState) 
	{
		super.onCreate(savedInstanceState);
				
		myApplication = (MyApplication)getApplication();
		reportFileName = getIntent().getStringExtra("report");
		myApplication.Log.i("WebReportPage", "Opening report: " + reportFileName); 
		outputFileName = reportFileName.replace(".xml",  "") + android.text.format.DateFormat.format("yyyy-MM-dd-hh-mm-ss", new java.util.Date()) + ".json";
		
		setContentView(R.layout.web_report);
		
		reportView = (WebView)findViewById(R.id.reportWebView);
		reportView.getSettings().setJavaScriptEnabled(true);
		reportView.getSettings().setAppCacheEnabled( false );
		reportView.addJavascriptInterface(new WebAppInterface(this), "Android"); // Setup javascript interface
		reportView.getSettings().setRenderPriority(RenderPriority.HIGH);
		reportView.getSettings().setLoadWithOverviewMode(true);
		reportView.setWebViewClient(new WebViewClient() {  
		    @Override  
		    public void onPageFinished(WebView view, String url)  
		    {  
		    	// Page Loaded... not much to do yet
		    }  
		});  

		//	Local Page
		String pageUrl = "file:///" + storagePath + "/EmergencyResponse/htmlReports/" + reportFileName.replace(".xml",  "") + ".html";
		
		reportView.loadUrl(pageUrl); 
	}
}