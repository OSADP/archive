package org.battelle.idto.mdt.controllers;

import org.battelle.idto.mdt.R;
import org.battelle.idto.mdt.adapters.GatesArrayAdapter;
import org.battelle.idto.mdt.models.Gate;
import org.battelle.idto.mdt.models.Gates;
import org.battelle.idto.mdt.utils.AssetFileManager;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import android.content.Context;
import android.widget.ArrayAdapter;

public class GatesController{

	private static final Logger mLogger = LoggerFactory.getLogger(GatesController.class); 
	
	protected Gates mGates;
	private GatesArrayAdapter mArrayAdapter;
	
	
	public GatesController(Context context)
	{
		mLogger.debug("Loading gates from gates.json");
		mGates = AssetFileManager.getConfigurationFileContents("gates.json", Gates.class);

		
		mArrayAdapter = new GatesArrayAdapter(context, android.R.layout.list_content, mGates.getGateList());
		
		mArrayAdapter.notifyDataSetChanged();
	}

	public Gates getGates()
	{
		return mGates;
	}
	
	public ArrayAdapter<Gate> getStopTimesArrayAdapter()
	{
		return mArrayAdapter;
	}
	
	public void setSelectedGate(int pos)
	{
		mArrayAdapter.setSelectedIndex(pos);
	}
	
	public void clearSelectedGate()
	{
		mArrayAdapter.setSelectedIndex(-1);
	}
	
}
