/**
 * @file         infloui/obu/ObdIIBluetoothState.java
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

package org.battelle.inflo.infloui.odbii;

public enum VehicleDiagnosticsState {
	Unknown, Bluetooth_Unavailable, Bluetooth_Off, Disconnected, Connecting, Connected, Disabled;

	public static boolean isOkay(VehicleDiagnosticsState state) {
		return state.equals(Connected);
	}
}
