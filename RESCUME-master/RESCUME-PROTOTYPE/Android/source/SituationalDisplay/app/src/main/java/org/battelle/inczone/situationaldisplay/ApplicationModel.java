/**
 * @file         inczoneui/ApplicationModel.java
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */

package org.battelle.inczone.situationaldisplay;

import org.battelle.inczone.situationaldisplay.obu.ObuBluetoothState;
import org.battelle.inczone.situationaldisplay.obu.handlers.BSMSMessageHandler.BSMSInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.DiagnosticMessageHandler.DiagnosticInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.EVASMessageHandler.EVASInformation;
import org.battelle.inczone.situationaldisplay.obu.handlers.TIMSMessageHandler.TIMSInformation;

import android.os.Parcel;
import android.os.Parcelable;

import com.google.gson.Gson;
import com.google.gson.JsonSyntaxException;

/**
 * Parcelable Model class containing fields that keep the current model of the
 * entire application.
 * 
 * The idea is that UI's can then use the invalidation broadcast from
 * {@code ApplicationMonitorService} to update their views from the model's
 * field values.
 * 
 */
public class ApplicationModel implements Parcelable {

	/**
	 * Required for creating {@code ApplicationModel} from {@code Parcelable}.
	 */
	public static Parcelable.Creator<ApplicationModel> CREATOR = new Creator<ApplicationModel>() {

		@Override
		public ApplicationModel[] newArray(int size) {
			return new ApplicationModel[size];
		}

		@Override
		public ApplicationModel createFromParcel(Parcel source) {

			Gson builder = new Gson();

			try {
				return builder.fromJson(source.readString(),
						ApplicationModel.class);
			} catch (JsonSyntaxException e) {
				e.printStackTrace();
			}
			return null;
		}
	};

	public ApplicationModel() {
	}

	/*
	 * Parcelable methods
	 */
	@Override
	public int describeContents() {
		return 0;
	}

	@Override
	public void writeToParcel(Parcel arg0, int arg1) {

		Gson builder = new Gson();
		arg0.writeString(builder.toJson(this));
	}

	/*
	 * OBU BT Model
	 */
	public ObuBluetoothState obuBluetoothState = ObuBluetoothState.Unknown;
	public DiagnosticInformation obuDiagnostics = new DiagnosticInformation();
    public EVASInformation obuEVAsReceived = new EVASInformation();
    public BSMSInformation obuBSMsReceived = new BSMSInformation();
    public TIMSInformation obuTIMsReceived = new TIMSInformation();

	@Override
	public String toString() {

		Gson builder = new Gson();
		return builder.toJson(this);
	}
}
