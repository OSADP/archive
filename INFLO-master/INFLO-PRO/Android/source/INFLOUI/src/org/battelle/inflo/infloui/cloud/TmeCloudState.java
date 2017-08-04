/**
 * @file         infloui/cloud/TmeCloudState.java
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

package org.battelle.inflo.infloui.cloud;

/**
 * State of the {@code TmeCloudService}.
 * 
 * @author branch
 * 
 */
public enum TmeCloudState {
	Unknown, Unavailable, Available;

	/**
	 * Utility to check if a given {@code TmeCloudState} is an okay state.
	 * 
	 * @param state
	 * @return
	 */
	public static boolean isOkay(TmeCloudState state) {
		return state.equals(Available);
	}
};
