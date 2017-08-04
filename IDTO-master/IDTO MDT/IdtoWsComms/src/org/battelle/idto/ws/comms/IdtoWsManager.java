package org.battelle.idto.ws.comms;

import java.util.ArrayList;
import java.util.Arrays;

import org.battelle.idto.ws.otp.data.ETA;
import org.battelle.idto.ws.otp.data.IdtoTrip;
import org.battelle.idto.ws.otp.data.ProbeMessage;
import org.battelle.idto.ws.otp.data.ProbeResponse;
import org.battelle.idto.ws.otp.data.StopTimesForStop;
import org.battelle.idto.ws.otp.data.StopType;
import org.battelle.idto.ws.otp.data.StopsNearPoint;
import org.battelle.idto.ws.otp.data.TConnectStatus;
import org.json.JSONArray;
import org.json.JSONObject;

import android.content.Context;
import android.location.Location;
import android.util.Log;

import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonArrayRequest;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.JsonStringRequest;
import com.android.volley.toolbox.Volley;

import com.google.gson.*;

public class IdtoWsManager implements IdtoWsInterface{

	String mDomain;
	Context mContext;
	Gson gson;
	RequestQueue mRequestQueue;
	String mUsername;
	String mPassword;

	public IdtoWsManager(String domain, Context context)
	{
		mDomain = domain;
		mContext = context;
		mUsername = "tadpole";
		mPassword = "tadpole";
		gson = new Gson();
		
		mRequestQueue = Volley.newRequestQueue(mContext);
	}
	
	public void getStopsNearPoint(Location point, final IdtoWsResponse<StopsNearPoint> idtoWsResponse)
	{

		String url = mDomain + "/api/BusStop?latitude=" +
				Double.toString(point.getLatitude())+
				"&longitude="+
				Double.toString(point.getLongitude())+
				"&radius=200";
		
		JsonObjectRequest jsObjRequest = new JsonObjectRequest(Request.Method.GET, url, null, new Response.Listener<JSONObject>() {

			
			@Override
			public void onResponse(JSONObject response) {
				
				String responseString = response.toString();
				
				StopsNearPoint stopsNearPoint = gson.fromJson(responseString, StopsNearPoint.class);
				
				idtoWsResponse.onIdtoWsResponse(stopsNearPoint);
			}
		}, new Response.ErrorListener() {

			@Override
			public void onErrorResponse(VolleyError error) {
				idtoWsResponse.onError(error.getMessage());
			}
		}, mUsername, mPassword);
		
	
		
		mRequestQueue.add(jsObjRequest);
		
	}
	
	public void getTConnectStatus(int tripId, final IdtoWsResponse<ArrayList<TConnectStatus>> idtoWsResponse)
	{

		String url = mDomain + "/api/tconnect?tripId=" +
				Integer.toString(tripId);
		
		JsonArrayRequest jsObjRequest = new JsonArrayRequest(url,new Response.Listener<JSONArray>() {

			@Override
			public void onResponse(JSONArray response) {
				String responseString = response.toString();
				try{
					TConnectStatus[] itdoTrips = gson.fromJson(responseString, TConnectStatus[].class);
				
					ArrayList<TConnectStatus> idtoTripsArrayList = new ArrayList<TConnectStatus>(Arrays.asList(itdoTrips));
					
					idtoWsResponse.onIdtoWsResponse(idtoTripsArrayList);
				}catch(Exception ex)
				{
					idtoWsResponse.onError(ex.getMessage());
				}
			}
		}, 
		new Response.ErrorListener() {
			
			@Override
			public void onErrorResponse(VolleyError error) {
				idtoWsResponse.onError(error.getMessage());
			}
		}, mUsername, mPassword);

		mRequestQueue.add(jsObjRequest);
	}

	@Override
	public void getStopTimesForStop(final StopType stopType, long startTime_ms, long endTime_ms, final IdtoWsResponse<StopTimesForStop> idtoWsResponse) {

		String url = mDomain + "/api/BusStop?" +
				"stopId="+stopType.getId().getId() +
				"&agency="+stopType.getId().getAgency() +
				"&startTime="+Long.toString(startTime_ms) +
				"&endTime="+Long.toString(endTime_ms);
		
		Log.d("IDTOWS", "getStopTimesForStop requestString: " + url);
		
		JsonObjectRequest jsObjRequest = new JsonObjectRequest(Request.Method.GET, url, null, new Response.Listener<JSONObject>() {

			@Override
			public void onResponse(JSONObject response) {
				
				String responseString = response.toString();
				Log.d("IDTOWS", "getStopTimesForStop onResponse jsonString: " + responseString);
				try{
					StopTimesForStop stopsNearPoint = gson.fromJson(responseString, StopTimesForStop.class);
				
					stopsNearPoint.setStopInfo(stopType);
					idtoWsResponse.onIdtoWsResponse(stopsNearPoint);
				}catch(Exception ex)
				{
					idtoWsResponse.onError(ex.getMessage());
				}
			}
		}, new Response.ErrorListener() {

			@Override
			public void onErrorResponse(VolleyError error) {
				idtoWsResponse.onError(error.getMessage());
			}
		}, mUsername, mPassword);
		
	
		
		mRequestQueue.add(jsObjRequest);
	}
	
	public void getTrips(int userId, IdtoTripType tripType, final IdtoWsResponse<ArrayList<IdtoTrip>> idtoWsResponse){
		try{

			String url = mDomain + "/api/Trip?" +
					"travelerID="+Integer.toString(userId) +
					"&type="+ Integer.toString(tripType.getCode());

			JsonArrayRequest jsObjRequest = new JsonArrayRequest(url,new Response.Listener<JSONArray>() {

				@Override
				public void onResponse(JSONArray response) {
					
					String responseString = response.toString();
					try{
						IdtoTrip[] itdoTrips = gson.fromJson(responseString, IdtoTrip[].class);
					
						ArrayList<IdtoTrip> idtoTripsArrayList = new ArrayList<IdtoTrip>(Arrays.asList(itdoTrips));
						
						idtoWsResponse.onIdtoWsResponse(idtoTripsArrayList);
					}catch(Exception ex)
					{
						idtoWsResponse.onError(ex.getMessage());
					}
				}
			}, new Response.ErrorListener() {

				@Override
				public void onErrorResponse(VolleyError error) {
					idtoWsResponse.onError(error.getMessage());
				}
			}, mUsername, mPassword);
			
		
			
			mRequestQueue.add(jsObjRequest);
		}catch(Exception ex)
		{
			Log.e("IDTO", ex.getMessage());
		}
	}
	
	public void postIdtoTrip(IdtoTrip trip,final IdtoWsResponse<IdtoTrip> idtoWsResponse)
	{
		try{
			String url = mDomain + "/api/Trip";
			
			String jsonString = gson.toJson(trip);
			
			JSONObject jsonObject = new JSONObject(jsonString);
			Log.d("IDTO WS MANAGER", "POST TRIP JSON: " + jsonString);
			
			JsonObjectRequest jsObjRequest = new JsonObjectRequest(Request.Method.POST, url, jsonObject, new Response.Listener<JSONObject>() {
				@Override
				public void onResponse(JSONObject response) {
					
					String responseString = response.toString();
	
					IdtoTrip idtoTrip = gson.fromJson(responseString, IdtoTrip.class);
					
					idtoWsResponse.onIdtoWsResponse(idtoTrip);
					
				}
			}, new Response.ErrorListener() {
	
				@Override
				public void onErrorResponse(VolleyError error) {
					idtoWsResponse.onError(error.getMessage());
				}
			}, mUsername, mPassword);

			mRequestQueue.add(jsObjRequest);
		}catch(Exception ex)
		{
			Log.e("IDTO", ex.getMessage());
		}
	}
	
	public void postProbe(ProbeMessage probeMsg, final IdtoWsResponse<ProbeResponse> idtoWsResponse)
	{
		try{
			String url = mDomain + "/api/Probe";
			
			String jsonString = gson.toJson(probeMsg);
			
			JSONObject jsonObject = new JSONObject(jsonString);
			
			JsonStringRequest jsStrRequest = new JsonStringRequest(Request.Method.POST, url, jsonObject, new Response.Listener<String>() {
				@Override
				public void onResponse(String response) {
					
					ProbeResponse response1 = new ProbeResponse();
					
					idtoWsResponse.onIdtoWsResponse(response1);
					
				}
			}, new Response.ErrorListener() {
	
				@Override
				public void onErrorResponse(VolleyError error) {
					idtoWsResponse.onError(error.getMessage());
				}
			}, mUsername, mPassword);

			mRequestQueue.add(jsStrRequest);
		}catch(Exception ex)
		{
			Log.e("IDTO", ex.getMessage());
		}
	}
	
	public void getETA(Location startLocation, Location endLocation, final IdtoWsResponse<ETA> idtoWsResponse)
	{
		try{

			String url = mDomain + "/api/ETA?" +
					"startLatitude="+Double.toString(startLocation.getLatitude()) +
					"&startLongitude="+Double.toString(startLocation.getLongitude()) +
					"&endLatitude="+Double.toString(endLocation.getLatitude()) +
					"&endLongitude="+Double.toString(endLocation.getLongitude());

			JsonObjectRequest jsObjRequest = new JsonObjectRequest(Request.Method.GET, url, null, new Response.Listener<JSONObject>() {

				@Override
				public void onResponse(JSONObject response) {
					
					String responseString = response.toString();
					try{
						ETA stopsNearPoint = gson.fromJson(responseString, ETA.class);
					
						idtoWsResponse.onIdtoWsResponse(stopsNearPoint);
					}catch(Exception ex)
					{
						idtoWsResponse.onError(ex.getMessage());
					}
				}
			}, new Response.ErrorListener() {

				@Override
				public void onErrorResponse(VolleyError error) {
					idtoWsResponse.onError(error.getMessage());
				}
			}, mUsername, mPassword);
			
		
			
			mRequestQueue.add(jsObjRequest);
		}catch(Exception ex)
		{
			Log.e("IDTO", ex.getMessage());
		}
	}
}
