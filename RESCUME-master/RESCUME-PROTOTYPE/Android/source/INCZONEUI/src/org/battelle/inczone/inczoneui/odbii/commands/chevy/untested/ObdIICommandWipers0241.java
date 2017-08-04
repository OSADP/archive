package org.battelle.inczone.inczoneui.odbii.commands.chevy.untested;

import java.util.ArrayList;
import java.util.List;

import org.battelle.inczone.inczoneui.odbii.commands.ObdIICommand;

public class ObdIICommandWipers0241 extends ObdIICommand {

	public ObdIICommandWipers0241(String tag) {
		super("0241", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "06 41 04 62 90 29 ";

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
