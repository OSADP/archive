package org.battelle.idto.mdt.fragments;

import org.battelle.idto.mdt.R;
import org.battelle.idto.mdt.controllers.GatesController;
import org.battelle.idto.mdt.models.Gate;

import android.app.Activity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ListView;
import android.support.v4.app.Fragment;

public class MainMenuFragment extends Fragment implements OnItemClickListener {
	private OnGateSelectedListener mCallback;
	private ListView mRouteListView;
	
	public interface OnGateSelectedListener{
		public void onGateSelected(Gate selectedGate);
	}
	
	GatesController mGatesController;
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container,
			Bundle savedInstanceState) {

		return inflater.inflate(R.layout.main_menu_list, container, false);
	}

	@Override
	public void onStart() {
		super.onStart();
		mGatesController = new GatesController(getActivity());

		mRouteListView = (ListView)this.getActivity().findViewById(R.id.routeListView);

		mRouteListView.setOnItemClickListener(this);
		mRouteListView.setAdapter(mGatesController.getStopTimesArrayAdapter());
	}
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		try {
            mCallback = (OnGateSelectedListener) activity;
        } catch (ClassCastException e) {
            throw new ClassCastException(activity.toString()
                    + " must implement OnHeadlineSelectedListener");
        }
	}

	@Override
	public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

		mGatesController.setSelectedGate((int)id);
		Gate selectedGate = mGatesController.getGates().getGateList().get((int)id);
		mCallback.onGateSelected(selectedGate);

		
	}
	
	public void clearSelection()
	{
		mGatesController.clearSelectedGate();
	}

}
