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

package org.battelle.inczone.situationaldisplay;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Locale;

import android.os.Environment;
import android.os.Handler;
import android.util.Log;

public class ApplicationLog {

	private static ApplicationLog srInstance;

	public synchronized static ApplicationLog getInstance() {

		if (srInstance == null) {
			srInstance = new ApplicationLog();
		}

		return srInstance;
	}

	public static void resetInstance() {
		srInstance = null;
	}

	private final Handler rCloseHandler;
	private File rLogFile;
	private PrintWriter rPrintWriter;
	private FileWriter rFileWriter;
	private final SimpleDateFormat rDateFormatter = new SimpleDateFormat("yyyy-MM-dd_HH-mm-ss-SSSS", Locale.US);

	private ApplicationLog() {

		if (Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED)) {
			File baseDirectory = new File(Environment.getExternalStorageDirectory() + "/inczonelogs");

			// Make directory for file
			if (!baseDirectory.exists())
				baseDirectory.mkdirs();

			// Create file
			rLogFile = new File(baseDirectory + "/" + rDateFormatter.format(Calendar.getInstance().getTime()) + ".txt");
			try {
				rLogFile.createNewFile();
			} catch (IOException e) {

				Log.e("Logger", "Error creating log file", e);
				rLogFile = null;
			}
		}

		rCloseHandler = new Handler();
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

		Log.i("Logger", "Closing log file");

		try {
			rFileWriter.close();
		} catch (IOException e) {
			e.printStackTrace();
		}

		rFileWriter = null;
		rPrintWriter = null;
	}

	public synchronized void v(String tag, String msg) {
		//Log.v(tag, msg);
		//writeToFile("v", tag, msg);
	}

	public synchronized void d(String tag, String msg) {
		//Log.d(tag, msg);
		//writeToFile("d", tag, msg);
	}

	public synchronized void i(String tag, String msg) {
		//Log.i(tag, msg);
		//writeToFile("i", tag, msg);
	}

	public synchronized void w(String tag, String msg) {
		Log.w(tag, msg);
		writeToFile("w", tag, msg);
	}

	public synchronized void e(String tag, String msg) {
		Log.e(tag, msg);
		writeToFile("e", tag, msg);
	}

	public synchronized void e(String tag, String msg, Throwable arg1) {
		StringWriter sw = new StringWriter();
		PrintWriter pw = new PrintWriter(sw);
		arg1.printStackTrace(pw);

		Log.e(tag, msg + ": " + sw.toString());

		writeToFile("e", tag, msg + ": " + sw.toString());
	}

	private synchronized void writeToFile(String type, String tag, String msg) {

		if (!this.isOpen()) {
			try {
				this.open();
			} catch (Exception e) {
				srInstance.e("Logger", "Could not open log file", e);
				return;
			}
		}

		rCloseHandler.removeCallbacksAndMessages(null);

		rPrintWriter.printf("%s\t%s\t%s\t%s\n", type, rDateFormatter.format(Calendar.getInstance().getTime()), tag
				.replace("\t", " ").replace("\n", "\\n"), msg.replace("\t", " ").replace("\n", "\\n"));

		rPrintWriter.flush();

		rCloseHandler.postDelayed(closeLog, 5000);
	}

	Runnable closeLog = new Runnable() {

		@Override
		public void run() {
			ApplicationLog.this.close();
			Log.v("Logger", "Closed Log");
		}
	};
}
