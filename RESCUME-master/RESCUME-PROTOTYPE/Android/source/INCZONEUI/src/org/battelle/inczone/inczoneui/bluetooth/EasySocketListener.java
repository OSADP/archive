/**
 * @file         inczoneui/bluetooth/ManagedSocketListener.java
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

package org.battelle.inczone.inczoneui.bluetooth;

public interface EasySocketListener {
	/**
	 * Called when new data is received. All unconsumed data is provided as an
	 * argument and the function returns how much data was consumed.
	 * 
	 * @param data
	 *            All data that hasn't been consumed yet
	 * @return The amount of data that was consumed by this function call.
	 */
	void onDataReceived(int count);

	/**
	 * Called when the state of the socket has changed.
	 * 
	 * @param state
	 *            The new state of the socket
	 */
	void onStateUpdated(EasyBluetoothState state);
}
