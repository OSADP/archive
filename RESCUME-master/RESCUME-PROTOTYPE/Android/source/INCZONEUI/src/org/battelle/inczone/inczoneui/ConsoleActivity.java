package org.battelle.inczone.inczoneui;

import java.util.ArrayList;

import org.battelle.inczone.inczoneui.obu.handlers.ConMessageHandler;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.support.v4.content.LocalBroadcastManager;
import android.view.View;
import android.view.WindowManager;
import android.widget.ScrollView;
import android.widget.TextView;

public class ConsoleActivity extends Activity {

	ArrayList<String> rReceivedMsgs = new ArrayList<String>();
	boolean attached = false;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
				WindowManager.LayoutParams.FLAG_FULLSCREEN);

		setContentView(R.layout.activity_console);

		if (savedInstanceState != null)
			rReceivedMsgs = savedInstanceState
					.getStringArrayList("rReceivedMsgs");
	}

	@Override
	protected void onResume() {
		super.onResume();

		synchronized (this) {
			if (!attached) {
				attached = true;
				LocalBroadcastManager.getInstance(this).registerReceiver(
						rConMsgReceiver,
						new IntentFilter(ConMessageHandler.ACTION_MESSAGE));
			}
		}
	}

	@Override
	protected void onPause() {
		synchronized (this) {
			if (attached) {
				attached = false;

				LocalBroadcastManager.getInstance(this).unregisterReceiver(
						rConMsgReceiver);
			}
		}

		super.onPause();
	}

	@Override
	protected void onSaveInstanceState(Bundle outState) {
		outState.putStringArrayList("rReceivedMsgs", rReceivedMsgs);
	}

	BroadcastReceiver rConMsgReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			rReceivedMsgs.add(intent
					.getStringExtra(ConMessageHandler.EXTRA_MSG));

			while (rReceivedMsgs.size() > 100) {
				rReceivedMsgs.remove(0);
			}

			StringBuilder results = new StringBuilder();

			for (String s : rReceivedMsgs) {
				results.append(s);
				results.append("\n");
			}

			((TextView) findViewById(R.id.con_txtMessages)).setText(results
					.toString());

			((ScrollView) findViewById(R.id.con_scrollMessages))
					.fullScroll(View.FOCUS_DOWN);
		}
	};

	public void onAttachClick(View v) {

		synchronized (this) {
			if (!attached) {
				attached = true;
				LocalBroadcastManager.getInstance(this).registerReceiver(
						rConMsgReceiver,
						new IntentFilter(ConMessageHandler.ACTION_MESSAGE));
			}
		}
	}

	public void onDetachClick(View v) {
		synchronized (this) {
			if (attached) {
				attached = false;

				LocalBroadcastManager.getInstance(this).unregisterReceiver(
						rConMsgReceiver);
			}
		}
	}
}
