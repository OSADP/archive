package org.battelle.inczone.inczoneui.odbii.commands;

import java.util.ArrayList;
import java.util.List;

public class ObdIICommandMaf0110 extends ObdIICommand {

	public ObdIICommandMaf0110(String tag) {
		super("0110", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		final String prefix = "41 10 ";

		for (String str : lines) {
			if (str.startsWith(prefix)) {

				List<Integer> bytes = new ArrayList<Integer>();
				String[] hexBytes = str.substring(prefix.length()).trim()
						.split(" ");

				for (String hexByte : hexBytes) {
					bytes.add(Integer.parseInt(hexByte, 16));
				}

				if (bytes.size() > 1) {
					return ((bytes.get(0) * 256) + bytes.get(1)) / 100.0;
				}
			}

		}

		return null;
	}
}
