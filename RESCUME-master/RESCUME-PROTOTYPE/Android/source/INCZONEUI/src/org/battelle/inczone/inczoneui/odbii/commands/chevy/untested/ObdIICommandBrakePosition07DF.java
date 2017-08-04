package org.battelle.inczone.inczoneui.odbii.commands.chevy.untested;

import java.util.ArrayList;
import java.util.List;

import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommand;

public class ObdIICommandBrakePosition07DF extends ObdIICommand {

	public ObdIICommandBrakePosition07DF(String tag) {
		super("07DF", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "07 E8 04 62 20 D4 ";

		for (String str : lines) {
			if (str.startsWith(prefix)) {

				List<Integer> bytes = new ArrayList<Integer>();
				String[] hexBytes = str.substring(prefix.length()).trim()
						.split(" ");

				for (String hexByte : hexBytes) {
					bytes.add(Integer.parseInt(hexByte, 16));
				}

				if (bytes.size() > 0) {
					return bytes.get(0);
				}
			}

		}

		return null;
	}
}
