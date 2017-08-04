package org.battelle.inczone.inczoneui.ntrip;

public enum NTripState {
	Disconnected, Waiting_For_Bluetooth, Connecting, Connected, Waiting_For_Data, Receiving_Data;

	public static boolean isOkayState(NTripState state) {
		return state == NTripState.Receiving_Data;
	}
};
