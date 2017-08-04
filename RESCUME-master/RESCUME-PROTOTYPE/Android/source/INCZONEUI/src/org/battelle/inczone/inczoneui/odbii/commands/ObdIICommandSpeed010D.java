package org.battelle.inczone.inczoneui.odbii.commands;

import java.util.ArrayList;
import java.util.List;

public class ObdIICommandSpeed010D extends ObdIICommand {

	public ObdIICommandSpeed010D(String tag) {
		super("010D", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "41 0D ";

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
