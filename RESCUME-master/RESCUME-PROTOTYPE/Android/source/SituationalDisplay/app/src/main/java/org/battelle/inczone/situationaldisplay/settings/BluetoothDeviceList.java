/**
 * @file         inczoneui/obu/settings/BluetoothDeviceList.java
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

package org.battelle.inczone.situationaldisplay.settings;

import java.util.ArrayList;
import java.util.List;
import java.util.Set;

import org.battelle.inczone.situationaldisplay.R;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Context;
import android.preference.ListPreference;
import android.util.AttributeSet;

/**
 * List Preference that lists all available Bluetooth devices with entries of
 * their MAC addresses
 * 
 * @author branch
 * 
 */
public class BluetoothDeviceList extends ListPreference {

	public BluetoothDeviceList(Context context, AttributeSet attrs) {
		super(context, attrs);

		BluetoothAdapter bta = BluetoothAdapter.getDefaultAdapter();

		if (bta != null) {
			Set<BluetoothDevice> pairedDevices = bta.getBondedDevices();

			String[] additionalBtMacs = context.getResources().getStringArray(
					R.array.config_additionalBtMacs);

			List<CharSequence> entries = new ArrayList<CharSequence>();
			List<CharSequence> entryValues = new ArrayList<CharSequence>();

			for (BluetoothDevice dev : pairedDevices) {
				entries.add(dev.getName());
				entryValues.add(dev.getAddress());
			}

			for (String s : additionalBtMacs) {
				String[] split = s.split(";");
				entries.add(split[0]);
				entryValues.add(split.length == 2 ? split[1] : split[0]);
			}

			setEntries(entries.toArray(new CharSequence[entries.size()]));
			setEntryValues(entryValues
					.toArray(new CharSequence[entries.size()]));
			setDefaultValue(entryValues.get(0));
		} else {
			setEntries(new String[] { "No Paired BT Devices" });
			setEntryValues(new String[] { "" });
			setDefaultValue("");
		}
	}

	public BluetoothDeviceList(Context context) {
		this(context, null);
	}

}
