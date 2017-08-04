/**
 * @file         infloui/ApplicationModel.java
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

package org.battelle.inflo.infloui;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

import org.battelle.inflo.infloui.alerts.QWarnAlert;
import org.battelle.inflo.infloui.alerts.SpdHarmAlert;
import org.battelle.inflo.infloui.cloud.TmeCloudEndpointStatistics;
import org.battelle.inflo.infloui.obu.ObuBluetoothState;
import org.battelle.inflo.infloui.obu.handlers.DiagnosticMessageHandler.DiagnosticInformation;
import org.battelle.inflo.infloui.odbii.VehicleDiagnosticsState;

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
	 * CLOUD Model
	 */
	TmeCloudEndpointStatistics[] tmeCloudEndpointStatistics = new TmeCloudEndpointStatistics[0];

	/*
	 * OBU BT Model
	 */
	public ObuBluetoothState obuBluetoothState = ObuBluetoothState.Unknown;
	public DiagnosticInformation obuDiagnostics = new DiagnosticInformation();
	public int obuBsmReceivedCount = 0;
	public int obuBsmPostedCount = 0;
	public int obuTimRequestCount = 0;
	public int obuTimResponseCount = 0;

	/*
	 * WEATHER Model
	 */
	public double weatherTemp = 0;
	public double weatherPressure = 0;
	public double weatherHumidity = 0;

	/*
	 * ALERTS Model
	 */
	public ArrayList<SpdHarmAlert> alertSpdHarmAlertList = new ArrayList<SpdHarmAlert>();
	public SpdHarmAlert alertSpdHarm = null;

	public boolean setSpdHarmAlert(SpdHarmAlert alert) {
		boolean results = false;

		if (alert.getSpeed() == -1) {
			alert = null;
		} else if (this.alertSpdHarm == null
				|| !this.alertSpdHarm.isRelatedAlert(alert)) {
			this.alertSpdHarmAlertList.add(0, alert);
			results = true;
		} else {
			this.alertSpdHarmAlertList.set(0, alert);
			results = false;
		}

		alertSpdHarm = alert;
		return results;
	}

	public ArrayList<QWarnAlert> alertQWarnAlertList = new ArrayList<QWarnAlert>();
	public QWarnAlert alertQWarn = null;

	public boolean setQWarnAlert(QWarnAlert alert) {
		boolean results = false;

		if (alert.getDistanceToBOQ() == -1 && alert.getDistanceToFOQ() == -1) {
			alert = null;
		} else if (this.alertQWarn == null
				|| !this.alertQWarn.isRelatedAlert(alert)) {
			this.alertQWarnAlertList.add(0, alert);
			results = true;
		} else {
			this.alertQWarnAlertList.set(0, alert);
			results = false;
		}

		alertQWarn = alert;
		return results;
	}

	/*
	 * Vehicle Diagnostics
	 */
	public VehicleDiagnosticsState vehState = VehicleDiagnosticsState.Unknown;
	public Map<String, Object> vehData = new HashMap<String, Object>();

	@Override
	public String toString() {

		Gson builder = new Gson();
		return builder.toJson(this);
	}

}
