package org.battelle.idto.mdt.models;

import java.util.ArrayList;

import com.google.gson.annotations.SerializedName;

public class Gates {
	@SerializedName("Gates")
	public ArrayList<Gate> mGateList;

	public Gates()
	{
		mGateList = new ArrayList<Gate>();
	}
	
	public ArrayList<Gate> getGateList() {
		return mGateList;
	}

	public void setGates(ArrayList<Gate> gates) {
		mGateList = gates;
	}
	
	public void addGate(Gate gates){
		
		for(int i=0;i<mGateList.size();i++)
		{
			if(gates.getGateName().equals(mGateList.get(i).getGateName()))
			{
				mGateList.set(i, gates);
				return;
			}
		}
		
		mGateList.add(gates);
	}
}
