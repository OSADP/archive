/**
 * @file         inczoneui/ApplicationLog.java
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

package org.battelle.inczone.inczoneui;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Locale;

import org.joda.time.DateTime;

import android.os.Environment;
import android.os.Handler;
import android.util.Log;

public class StatisticsLog {

	private final Handler rCloseHandler = new Handler();
	private File rLogFile;
	private PrintWriter rPrintWriter;
	private FileWriter rFileWriter;
	private final SimpleDateFormat rDateFormatter = new SimpleDateFormat("yyyy-MM-dd_HH-mm-ss-SSSS", Locale.US);
	private boolean includeTimestamp = true;

	public StatisticsLog(String filename, boolean addTimeToFileName, String... headers) {

		if (Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED)) {
			File baseDirectory = new File(Environment.getExternalStorageDirectory() + "/inczonelogs");

			// Make directory for file
			if (!baseDirectory.exists())
				baseDirectory.mkdirs();

			// Create file
			rLogFile = new File(baseDirectory + "/"
					+ (addTimeToFileName ? rDateFormatter.format(Calendar.getInstance().getTime()) + "_" : "")
					+ filename + ".txt");

			if (!rLogFile.exists()) {
				try {
					rLogFile.createNewFile();
					writeHeaders(headers);
				} catch (IOException e) {

					Log.e("Logger", "Error creating log file", e);
					rLogFile = null;
				}
			}
		}
	}

	public boolean isIncludeTimestamp() {
		return includeTimestamp;
	}

	public void setIncludeTimestamp(boolean includeTimestamp) {
		this.includeTimestamp = includeTimestamp;
	}

	public synchronized boolean isOpen() {
		return rFileWriter != null;
	}

	public synchronized void open() throws Exception {

		// Return fw already exists
		if (this.rFileWriter != null)
			return;
		if (this.rLogFile == null)
			throw new Exception("Base Directory not initalized (is external SD card present?)");

		Log.i("Logger", "Openning file at: " + rLogFile.toString());

		// Open
		rFileWriter = new FileWriter(rLogFile, true);
		rPrintWriter = new PrintWriter(rFileWriter);
	}

	public synchronized void close() {

		if (rFileWriter == null)
			return;

		try {
			rFileWriter.close();
		} catch (IOException e) {
			e.printStackTrace();
		}

		rFileWriter = null;
		rPrintWriter = null;
	}

	public synchronized void log(Object... values) {
		if (!this.isOpen()) {
			try {
				this.open();
			} catch (Exception e) {
				return;
			}
		}

		rCloseHandler.removeCallbacksAndMessages(null);

		if (includeTimestamp) {
			rPrintWriter.printf("%s\t", DateTime.now().toString());
		}
		for (Object i : values) {
			rPrintWriter.printf("%s\t", String.valueOf(i).replace("\n", "\\n"));
		}
		rPrintWriter.printf("\n");
		rPrintWriter.flush();

		rCloseHandler.postDelayed(closeLog, 5000);
	}

	public synchronized void writeHeaders(String... values) {
		if (!this.isOpen()) {
			try {
				this.open();
			} catch (Exception e) {
				return;
			}
		}

		rCloseHandler.removeCallbacksAndMessages(null);

		if (includeTimestamp) {
			rPrintWriter.printf("Timestamp\t");
		}
		for (String i : values) {
			rPrintWriter.printf("%s\t", String.valueOf(i).replace("\n", "\\n"));
		}
		rPrintWriter.printf("\n");
		rPrintWriter.flush();

		rCloseHandler.postDelayed(closeLog, 5000);
	}

	Runnable closeLog = new Runnable() {

		@Override
		public void run() {
			StatisticsLog.this.close();
			Log.v("Logger", "Closed Log");
		}
	};
}
