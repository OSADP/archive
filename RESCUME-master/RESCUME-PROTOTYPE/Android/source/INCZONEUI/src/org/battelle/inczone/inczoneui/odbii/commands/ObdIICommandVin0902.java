package org.battelle.inczone.inczoneui.odbii.commands;

import java.util.ArrayList;
import java.util.List;

public class ObdIICommandVin0902 extends ObdIICommand {

	public ObdIICommandVin0902(String tag) {
		super("0902", tag);
	}

	@Override
	protected Object processResponse(String response) {

		if (super.processResponse(response) == null)
			return null;

		String[] lines = response.split("\r");

		List<Byte> vinBytes = new ArrayList<Byte>();

		for (String str : lines) {
			if (str.matches("^(\\d)+:.*$")) {

				String[] hexBytes = str
						.substring(str.indexOf(':') + 1, str.length()).trim()
						.split(" ");

				for (String hexByte : hexBytes) {
					vinBytes.add((byte) Integer.parseInt(hexByte, 16));
				}
			}

		}

		if (vinBytes.size() > 0) {
			vinBytes.remove(0);
			vinBytes.remove(0);
			vinBytes.remove(0);

			StringBuilder results = new StringBuilder();

			for (int i = 0; i < vinBytes.size(); i++) {
				results.append((char) (byte) vinBytes.get(i));
			}

			return results.toString();
		}
		return null;
	}
}
