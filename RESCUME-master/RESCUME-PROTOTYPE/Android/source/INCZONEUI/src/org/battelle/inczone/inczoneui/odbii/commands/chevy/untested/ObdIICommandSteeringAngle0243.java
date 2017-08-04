package org.battelle.inczone.inczoneui.odbii.commands.chevy.untested;

import java.util.ArrayList;
import java.util.List;

import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommand;

public class ObdIICommandSteeringAngle0243 extends ObdIICommand {

	public ObdIICommandSteeringAngle0243(String tag) {
		super("0243", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "06 43 05 62 40 31 ";

		for (String str : lines) {
			if (str.startsWith(prefix)) {

				List<Integer> bytes = new ArrayList<Integer>();
				String[] hexBytes = str.substring(prefix.length()).trim()
						.split(" ");

				for (String hexByte : hexBytes) {
					bytes.add(Integer.parseInt(hexByte, 16));
				}

				if (bytes.size() > 1) {
					return ((bytes.get(0) * 256) + bytes.get(1)) / 4.0;
				}
			}

		}

		return null;
	}
}
