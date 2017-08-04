package org.battelle.idto.mdt.fragments;

import org.battelle.idto.mdt.controllers.TransfersController;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import android.support.v4.app.ListFragment;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.os.Bundle;


public class MainConnectionsFragment extends ListFragment {
	
	private TransfersController mTransferController;
	
	private static final Logger mLogger = LoggerFactory.getLogger(MainConnectionsFragment.class); 
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		
		mTransferController = new TransfersController(this.getActivity());
		setListAdapter(mTransferController.getTransfersArrayAdapter());
		
		super.onCreate(savedInstanceState);
	}

	@Override
	public void onResume() {
		ColorDrawable crd = new ColorDrawable(android.R.color.transparent);
		
		getListView().setDivider(crd);
		getListView().setDividerHeight(20);
		super.onResume();
	}
}
 