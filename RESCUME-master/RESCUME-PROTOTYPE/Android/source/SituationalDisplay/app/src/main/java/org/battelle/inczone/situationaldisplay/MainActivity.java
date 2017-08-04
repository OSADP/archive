/**
 * @file         inczoneui/MainActivity.java
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

import android.app.TabActivity;
import android.content.Intent;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.WindowManager;
import android.widget.TabHost;
import android.widget.TabHost.TabSpec;

@SuppressWarnings("deprecation")
public class MainActivity extends TabActivity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN, WindowManager.LayoutParams.FLAG_FULLSCREEN);

		setContentView(R.layout.activity_main);

		startService(new Intent(this, ApplicationMonitorService.class));

		TabHost tabHost = getTabHost();

		TabSpec alertTab = tabHost.newTabSpec(getResources().getString(R.string.activity_dashboard));
		Intent alertsIntent = new Intent(this, DashboardActivity.class);
		alertTab.setIndicator(getResources().getString(R.string.activity_dashboard));
		alertTab.setContent(alertsIntent);

		TabSpec diagnosticsTab = tabHost.newTabSpec(getResources().getString(R.string.activity_diagnostics));
		Intent diagnosticsIntent = new Intent(this, DiagnosticsActivity.class);
		diagnosticsTab.setIndicator(getResources().getString(R.string.activity_diagnostics));
		diagnosticsTab.setContent(diagnosticsIntent);

		tabHost.addTab(alertTab);
		tabHost.addTab(diagnosticsTab);
	}

	@Override
	protected void onDestroy() {
		super.onDestroy();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	protected void onResume() {
		super.onResume();
	}

	@Override
	protected void onPause() {
		super.onPause();
	}

	public void onSettingsMenuClick(MenuItem item) {
		startActivity(new Intent(this, SettingsActivity.class));
	}

	public void onCloseMenuClick(MenuItem item) {
		finish();
		stopService(new Intent(this, ApplicationMonitorService.class));
	}
}
