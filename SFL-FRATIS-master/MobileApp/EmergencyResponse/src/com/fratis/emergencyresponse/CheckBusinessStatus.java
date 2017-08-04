//---------------------------------------------------------------------------------------------------------------------
//Class:	 	CheckBusinessStatus
//Author: 	Dmitri Zyuzin - University of Washington TRAC
//Purpose: 	- Fetches business status entries from the SQL database from serverside
//			PHP in the form of a JSON that will be parsed inside report page
//---------------------------------------------------------------------------------------------------------------------

package com.fratis.emergencyresponse;

import java.io.IOException;
import java.net.MalformedURLException;
import java.net.URL;
import org.json.JSONArray;
import org.json.JSONException;
import android.content.Context;
import android.os.AsyncTask;

public class CheckBusinessStatus extends AsyncTask<String, String, JSONArray> 
{
	Context myContext = null;					//	Calling activity context
	protected MyApplication myApplication;		// 	My Application
	ServerCommunicationHelper commHelper = new ServerCommunicationHelper();

	public CheckBusinessStatus(Context c)
	{
		myContext = c;
		myApplication = (MyApplication)c.getApplicationContext();
		commHelper.setContext(c);
	}

	@Override
	protected JSONArray doInBackground(String... reportPage) 
	{
		JSONArray jsonArray = new JSONArray();
		if(!myApplication.businessId.equals(""))
		{
			String json = "";
			try {
				json = commHelper.executeReq(new URL("http://" + myApplication.RemoteHost + "/mapfunctions.php?func=getBusinessStatus&business_id=" + myApplication.businessId));
			} catch (MalformedURLException e1) { 
				e1.printStackTrace(); 
					myApplication.Log.e("CheckBusinessStatus", "Bad URL Request");
				} catch (IOException e1) { 
					myApplication.Log.e("CheckBusinessStatus", "executeReq IOException");
				}
			finally
			{
				if(!json.equals("[]"))
				{
					try {
						jsonArray = new JSONArray(json);
						return jsonArray;
					} catch (JSONException e) {
						myApplication.Log.e("CheckBusinessStatus", "Can't create JSON Array");
					}
				}
			}
		}
		
		return jsonArray;
	}
}
