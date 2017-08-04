package org.battelle.idto.mdt.fragments;

import org.battelle.idto.mdt.adapters.GateStopTimesArrayAdapter;
import org.battelle.idto.mdt.controllers.GateStopTimesController;
import org.battelle.idto.mdt.controllers.IdtoTripController;
import org.battelle.idto.mdt.controllers.GateStopTimesController.GatePopulationListener;
import org.battelle.idto.mdt.models.GateStopTime;
import org.battelle.idto.mdt.models.Gate;

import com.google.gson.Gson;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.support.v4.app.ListFragment;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;

public class StopTimesFragment extends ListFragment implements DialogInterface.OnCancelListener {
	public static final String FRAGMENT_NAME = "StopTimesFragment";
	public static final String ARG_OBJ_JSON = "obj_json";
	
	private OnRouteSelectedListener mCallback;
	
	public interface OnRouteSelectedListener{
		public void onRouteSelected();
	}
	
	private GateStopTimesController mGateController;
	//private ArrayAdapter<GateStopTime> mArrayAdapter;
	private GateStopTimesArrayAdapter mGateStopTimesArrayAdapter;
	private ProgressDialog mProgressDialog;
	@Override
	public void onCreate(Bundle savedInstanceState) {
		// TODO Auto-generated method stub
		super.onCreate(savedInstanceState);
		Bundle args = getArguments();
		String jsonString = args.getString(StopTimesFragment.ARG_OBJ_JSON);
		Gson gson = new Gson();
		Gate selectedGate = gson.fromJson(jsonString, Gate.class);
		
		
		
		mGateController = new GateStopTimesController(selectedGate, mGatePopulationListener, this.getActivity());
		
		mGateStopTimesArrayAdapter = new GateStopTimesArrayAdapter(getActivity(), android.R.layout.list_content, mGateController.getGateStops().getStopTimeList());
		
		setListAdapter(mGateStopTimesArrayAdapter);
		
		mProgressDialog = new ProgressDialog(this.getActivity());
		mProgressDialog.setCancelable(true);
		mProgressDialog.setIndeterminate(true);
		mProgressDialog.setCanceledOnTouchOutside(false);
		mProgressDialog.setMessage("Retrieving Data");
		mProgressDialog.setOnCancelListener(this);
		mProgressDialog.show();
	}

	@Override
	public void onResume() {
		ColorDrawable crd = new ColorDrawable(android.R.color.transparent);
		
		getListView().setDivider(crd);
		getListView().setDividerHeight(20);
		super.onResume();
	}


	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		try {
            mCallback = (OnRouteSelectedListener) activity;
        } catch (ClassCastException e) {
            throw new ClassCastException(activity.toString()
                    + " must implement OnHeadlineSelectedListener");
        }
	}
	
	private GatePopulationListener mGatePopulationListener = new GatePopulationListener() {
		
		@Override
		public void populated(Gate gateStops) {
			mGateStopTimesArrayAdapter.notifyDataSetChanged();
			mProgressDialog.dismiss();
		}
		
		@Override
		public void error(String errorString) {
			mProgressDialog.dismiss();
		}
	};

	@Override
	public void onCancel(DialogInterface dialog) {
		mProgressDialog.dismiss();
	}
	
	@Override
	public void onListItemClick(ListView l, View v, int position, long id) {
		// TODO Auto-generated method stub
		super.onListItemClick(l, v, position, id);
		
		Gate selectedGateStops = mGateController.getGateStops();
		
//		mGoogleAnalyticsTracker.send(MapBuilder
//			    .createEvent("UX", "touch", "createTrip", null)
//			    .build()
//			);
//
//		mGoogleAnalyticsTracker.send(MapBuilder
//			    .createEvent("Trip", "create", obj.toString(), null)
//			    .build()
//			);
		
		IdtoTripController newTripController = new IdtoTripController(this.getActivity());
		newTripController.createTrip(selectedGateStops.getGateName(), selectedGateStops.getGateCode(), selectedGateStops.getStopTimeList().get(position));
		
		mCallback.onRouteSelected();
	}
	
}
