package org.battelle.inczone.inczoneui.odbii.commands;

import java.util.ArrayList;
import java.util.List;

public class ObdIICommandAmbAirTemp0146 extends ObdIICommand {

	public ObdIICommandAmbAirTemp0146(String tag) {
		super("0146", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "41 46 ";

		for (String str : lines) {
			if (str.startsWith(prefix)) {

				List<Integer> bytes = new ArrayList<Integer>();
				String[] hexBytes = str.substring(prefix.length()).trim()
						.split(" ");

				for (String hexByte : hexBytes) {
					bytes.add(Integer.parseInt(hexByte, 16));
				}

				if (bytes.size() > 0) {
					return bytes.get(0) - 40;
				}
			}

		}

		return null;
	}
}
