package org.battelle.inczone.situationaldisplay;

import java.util.ArrayList;

import org.battelle.inczone.situationaldisplay.obu.ObuBluetoothState;
import org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler.TIMSInformation;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;

import com.google.android.gms.maps.CameraUpdate;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;

import android.graphics.Color;
import android.os.Bundle;
import android.support.v4.content.LocalBroadcastManager;
import android.text.format.Time;
import android.util.Log;
import android.view.View;
import android.widget.TextView;

import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.MapFragment;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.maps.model.Polyline;
import com.google.android.gms.maps.model.PolylineOptions;

public class DashboardActivity extends Activity {

	private SharedPreferences mSettings;
    private GoogleMap mMap;
    private ArrayList<Marker> mMarkers;
    private ArrayList<Polyline> mLaneLines;
    private Time mTimLaneTime;

    private boolean CameraStatus = false;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_dashboard);

		mSettings = getSharedPreferences(
				getResources().getString(R.string.setting_file_name),
				MODE_MULTI_PROCESS);

        mMap = ((MapFragment) getFragmentManager().findFragmentById(R.id.map)).getMap();
        mMap.setMapType(GoogleMap.MAP_TYPE_SATELLITE);
        mMarkers = new ArrayList<Marker>();
        mLaneLines = new ArrayList<Polyline>();
        mTimLaneTime = new Time();
	}

	@Override
	protected void onResume() {
		super.onResume();
		LocalBroadcastManager.getInstance(this).registerReceiver(
				rInvalidationReceiver,
				new IntentFilter(ApplicationMonitorService.ACTION_INVALIDATE));

		ApplicationMonitorService.requestUpdate(this);
	}

	@Override
	protected void onPause() {
		super.onPause();

		LocalBroadcastManager.getInstance(this).unregisterReceiver(
				rInvalidationReceiver);
	}

	/**
	 * Causes the activity to redraw itself with the new model data
	 * 
	 * @param rModel
	 */
	private void invalidate(ApplicationModel rModel) {
        ArrayList<String> warnings = new ArrayList<String>();
        ArrayList<String> errors = new ArrayList<String>();
        boolean connectedState = true;

        if (rModel.obuBluetoothState != ObuBluetoothState.Connected) {
            errors.add("DSRC Radio Disconnected");
            connectedState = false;
        } else {
            if (rModel.obuDiagnostics.getGpsFix() == 0)
                errors.add("DSRC Radio: No GPS Fix");
        }

        ArrayList<BSMSMessageHandler.bsmExtract> bsmList = rModel.obuBSMsReceived.getBsm();
       for (int i = 0; i < bsmList.size(); i++) {
           boolean bsmMarkerExists = false;
           BSMSMessageHandler.bsmExtract bsm = bsmList.get(i);
           LatLng pos = new LatLng(bsm.getLatitude(), bsm.getLongitude());
           if (!CameraStatus) {
               CameraUpdate zoomedLocation = CameraUpdateFactory.newLatLngZoom(pos, 15);
               mMap.animateCamera(zoomedLocation);
               CameraStatus = !CameraStatus;
           }
           for(int k = 0; k < mMarkers.size(); k++)
           {
               Marker marker = mMarkers.get(k);
               if (bsm.getId().startsWith(marker.getTitle().substring(0, 5))) {
                   bsmMarkerExists = true;
                   marker.setTitle(bsm.getId());
                   marker.setPosition(pos);
                   break;
               }

           }
           if(!bsmMarkerExists) {
               Marker marker = mMap.addMarker(new MarkerOptions()
                       .position(pos)
                       .title(bsm.getId())
                       .snippet(String.valueOf(String.valueOf(bsm.getLatitude() + "," + bsm.getLongitude())))
                       .icon(BitmapDescriptorFactory.fromResource(R.drawable.indicatordotgreen)));
               mMarkers.add(marker);
           }
       }

        ArrayList<EVASMessageHandler.evaExtract> evaList = rModel.obuEVAsReceived.getEva();
        for (int i = 0; i < evaList.size(); i++) {
            boolean evaMarkerExists = false;
            EVASMessageHandler.evaExtract eva = evaList.get(i);
            LatLng pos = new LatLng(eva.getLatitude(), eva.getLongitude());
            if (!CameraStatus) {
                CameraUpdate zoomedLocation = CameraUpdateFactory.newLatLngZoom(pos, 15);
                mMap.animateCamera(zoomedLocation);
                CameraStatus = !CameraStatus;
            }
            for(int k = 0; k < mMarkers.size(); k++)
            {
                Marker marker = mMarkers.get(k);
                if(marker.getTitle().compareTo(eva.getId()) == 0)  {
                    evaMarkerExists = true;
                    marker.setPosition(pos);
                    break;
                }
            }
            if(!evaMarkerExists) {
                Marker marker = mMap.addMarker(new MarkerOptions()
                        .position(pos)
                        .title(eva.getId())
                        .snippet(String.valueOf(String.valueOf(eva.getLatitude() + "," + eva.getLongitude())))
                        .icon(BitmapDescriptorFactory.fromResource(R.drawable.indicatordotdarkblue)));
                mMarkers.add(marker);
            }
        }

        ArrayList<TIMSMessageHandler.TIMSInformation.timExtract> timList = rModel.obuTIMsReceived.getTim();
        boolean incidentStatus = false;
        ArrayList<PolylineOptions> incidentLineOptionsList = new ArrayList<PolylineOptions>();
        if(timList.size() > 0) {
            while(mLaneLines.size() > 0){
                Polyline removingLine = mLaneLines.remove(0);
                removingLine.setVisible(false);
                removingLine.remove();
            }
            for (int i = 0; i < timList.size(); i++) {
                TIMSInformation.timExtract tim = timList.get(i);
                ArrayList<TIMSInformation.DataFrame> dataFrameList = tim.getDataFrame();
                for (int k = 0; k < dataFrameList.size(); k++) {
                    TIMSInformation.DataFrame dataFrame = dataFrameList.get(k);
                    ArrayList<TIMSInformation.Region> regionList = dataFrame.getRegion();
                    for (int j = 0; j < regionList.size(); j++) {
                        TIMSInformation.Region region = regionList.get(j);
                        LatLng aPos = new LatLng(region.getAnchorLatitude(), region.getAnchorLongitude());
                        if (!CameraStatus) {
                            CameraUpdate zoomedLocation = CameraUpdateFactory.newLatLngZoom(aPos, 15);
                            mMap.animateCamera(zoomedLocation);
                            CameraStatus = !CameraStatus;
                        }

                      /* Marker marker = mMap.addMarker(new MarkerOptions()
                                .position(aPos)
                                .title("Region Anchor")
                                .snippet(String.valueOf(String.valueOf(region.getAnchorLatitude() + "," + region.getAnchorLongitude())))
                                .icon(BitmapDescriptorFactory.fromResource(R.drawable.indicatordotred)));
                      */
                        ArrayList<TIMSInformation.Node> nodeList = region.getRegionNodes();
                        ArrayList<LatLng> points = new ArrayList<LatLng>();
                        for (int n = 0; n < nodeList.size(); n++) {
                            points.add(new LatLng(nodeList.get(n).getNodeLatitude(), nodeList.get(n).getNodeLongitude()));
                        }
                        PolylineOptions lineOptions = new PolylineOptions();

                        if (dataFrame.getLaneType() == 0) {// Lane will be closed
                            lineOptions.addAll(points);
                            lineOptions.color(Color.RED);
                            Polyline line = mMap.addPolyline(lineOptions);
                            mLaneLines.add(line);
                        } else if (dataFrame.getLaneType() == 1) { // Lane will be speed restricted
                            lineOptions.addAll(points);
                            lineOptions.color(Color.YELLOW);
                            Polyline line = mMap.addPolyline(lineOptions);
                            mLaneLines.add(line);
                        } else if (dataFrame.getLaneType() == 2) {
                            PolylineOptions incidentLineOptions = new PolylineOptions();
                            incidentLineOptions.addAll(points);
                            incidentLineOptions.color(Color.BLACK); // Region has incident
                            incidentLineOptions.width(20);
                            incidentLineOptionsList.add(incidentLineOptions);
                            incidentStatus = true;
                        } else {                              // Lane type is other
                            lineOptions.addAll(points);
                            lineOptions.color(Color.GREEN);
                            Polyline line = mMap.addPolyline(lineOptions);
                            mLaneLines.add(line);
                        }

                    }
                    if (incidentStatus) {
                        for (int m = 0; m < incidentLineOptionsList.size(); m++) {
                            PolylineOptions lineOptions = incidentLineOptionsList.get(m);
                            Polyline line = mMap.addPolyline(lineOptions);
                            mLaneLines.add(line);
                        }
                    }
                }
                mTimLaneTime.setToNow();
            }
        }

        TextView warningFooter = (TextView) findViewById(R.id.dashboard_txtWarningFooter);
        if (warnings.size() > 0) {

            StringBuilder sb = new StringBuilder();
            for (String s : warnings)
                sb.append(s + '\n');

            warningFooter.setText(sb.toString().substring(0, sb.length() - 1));
            warningFooter.setVisibility(View.VISIBLE);
        } else {
            warningFooter.setVisibility(View.GONE);
        }

        TextView errorFooter = (TextView) findViewById(R.id.dashboard_txtErrorFooter);
        if (errors.size() > 0) {

            StringBuilder sb = new StringBuilder();
            for (String s : errors)
                sb.append(s + '\n');

            errorFooter.setText(sb.toString().substring(0, sb.length() - 1));
            errorFooter.setVisibility(View.VISIBLE);
        } else {
            errorFooter.setVisibility(View.GONE);
        }

        if(!connectedState) {
            while(mMarkers.size() > 0){
                Marker marker = mMarkers.remove(0);
                marker.remove();
            }
        }
        if (mMarkers.size() > 0) {
            int k = 0;
            while(k < mMarkers.size()) {

                boolean markerExists = false;
                    Marker marker = mMarkers.get(k);
                    String id = marker.getTitle();

                    for (int i = 0; i < evaList.size(); i++) {
                        EVASMessageHandler.evaExtract eva = evaList.get(i);
                        if (eva.getId().compareTo(id) == 0) {
                            markerExists = true;
                            break;
                        }
                    }

                    if (!markerExists) {
                        for (int i = 0; i < bsmList.size(); i++) {
                            BSMSMessageHandler.bsmExtract bsm = bsmList.get(i);
                            if (bsm.getId().compareTo(id) == 0) {
                                markerExists = true;
                                break;
                            }
                        }
                    }

                    if (!markerExists) {
                        marker.setVisible(false);
                        mMarkers.remove(k);
                        marker.remove();
                        Log.d("MARKER REMOVED", id);
                    } else {
                        k++;
                    }
            }
        }

        Time currentTime = new Time();
        currentTime.setToNow();
        if(!connectedState || (currentTime.toMillis(false) - mTimLaneTime.toMillis(false))  > 10000) {
            while(mLaneLines.size() > 0){
                Polyline removingLine = mLaneLines.remove(0);
                removingLine.setVisible(false);
                removingLine.remove();
            }
            mTimLaneTime.setToNow();
        }

    }

	/**
	 * Application Monitor will send this broadcast when the model changes
	 */
	BroadcastReceiver rInvalidationReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			invalidate((ApplicationModel) intent.getExtras().getParcelable(
					ApplicationMonitorService.EXTRA_MODEL));
		}
	};

}
