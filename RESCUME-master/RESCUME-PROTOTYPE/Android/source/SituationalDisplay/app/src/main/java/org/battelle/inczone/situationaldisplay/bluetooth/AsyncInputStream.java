package org.battelle.inczone.situationaldisplay.bluetooth;

import java.io.IOException;
import java.io.InputStream;

public abstract class AsyncInputStream extends InputStream {

	/**
	 * 
	 * @return NULL If data is unavailable, -1 if end of stream, byte data
	 *         otherwise
	 * 
	 * @throws IOException
	 *             If the socket is dead
	 */
	public abstract Integer readAsync() throws IOException;

	@Override
	public int read() throws IOException {
		return read(-1);
	}

	public Integer read(int timeout) throws IOException {
		long startTime = System.currentTimeMillis();

		Integer results = null;
		do {
			results = readAsync();
		} while (results == null
				&& (((System.currentTimeMillis() - startTime) <= timeout) || timeout == -1));

		return results;
	}

	public int read(byte[] buffer, int byteOffset, int timeout)
			throws IOException {

		int index = byteOffset;
		Integer value = 0;

		while (index < buffer.length && (value = read(timeout)) != null
				&& value >= 0) {
			buffer[index] = value.byteValue();
			index++;
		}

		return index - byteOffset;
	}

	public int readAsync(byte[] buffer, int byteOffset) throws IOException {

		return read(buffer, byteOffset, 0);
	}
}
