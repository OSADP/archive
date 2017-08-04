package org.battelle.inczone.inczoneui.odbii.commands;

import java.io.IOException;
import java.io.OutputStream;

import org.battelle.inczone.inczoneui.bluetooth.AsyncInputStream;

public class ObdIICommand {

	private final int TIMEOUT = 7000;

	private final String strCommandText;
	private final String strTag;
	private String strResponse = null;
	private Object objProcessedResponse = null;

	public ObdIICommand(String commandText, String tag) {
		strCommandText = commandText;
		strTag = tag;
	}

	public boolean run(OutputStream output, AsyncInputStream input) {
		reset();

		StringBuilder response = new StringBuilder();
		try {
			output.write(strCommandText.concat("\r\n").getBytes());

			while (!response.toString().endsWith("\r\r>")) {
				Integer data = input.read(TIMEOUT);
				if (data == null || data == -1)
					throw new IOException("OBD-II Response Timeout");

				response.append((char) data.byteValue());
			}

			// Remove the terminator
			response.delete(response.length() - 3, response.length());

			strResponse = response.toString();
			objProcessedResponse = processResponse(strResponse);

		} catch (IOException e) {
			strResponse = "ERROR: " + e.getMessage();
		}

		return isSuccessful();
	}

	protected void reset() {
		strResponse = null;
		objProcessedResponse = null;
	}

	protected Object processResponse(String response) {

		if (response.equals("NO DATA"))
			return null;

		return strResponse;
	}

	public String getCommandText() {
		return strCommandText;
	}

	public String getTag() {
		return strTag;
	}

	public String getRawResponse() {
		return strResponse;
	}

	public Object getProcessedResponse() {
		return objProcessedResponse;
	}

	public boolean isSuccessful() {
		return objProcessedResponse != null;
	}

}
