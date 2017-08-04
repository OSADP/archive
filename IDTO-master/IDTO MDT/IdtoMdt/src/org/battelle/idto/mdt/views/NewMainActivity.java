package org.battelle.idto.mdt.views;

import org.battelle.idto.mdt.R;
import org.battelle.idto.mdt.fragments.MainConnectionsFragment;
import org.battelle.idto.mdt.fragments.MainMenuFragment;
import org.battelle.idto.mdt.fragments.StopTimesFragment;
import org.battelle.idto.mdt.models.Gate;
import org.battelle.idto.mdt.utils.AssetFileManager;
import org.battelle.idto.mdt.utils.LocationService;

import com.google.gson.Gson;


import android.app.AlertDialog;
import android.content.ComponentName;
import android.content.ContentResolver;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.database.ContentObserver;
import android.os.Bundle;
import android.os.Handler;
import android.preference.PreferenceManager;
import android.provider.Settings;
import android.provider.Settings.SettingNotFoundException;

import android.support.v4.app.FragmentActivity;
import android.support.v4.app.FragmentManager;
import android.support.v4.app.FragmentManager.BackStackEntry;
import android.support.v4.app.FragmentTransaction;
import android.util.Log;
import android.view.WindowManager;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;

public class NewMainActivity extends FragmentActivity implements MainMenuFragment.OnGateSelectedListener, StopTimesFragment.OnRouteSelectedListener {
	
	StopTimesFragment mStopTimesFragment;
	MainConnectionsFragment mMainConnectionsFragment;

	LinearLayout mConnectionsLinearLayout;
	SeekBar mBrightnessSeekBar;
	MyContentObserver mSystemSettingsObserver;
	
	/** Called when the activity is first created. */

	@Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main_pane);

        SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getApplicationContext());
        int userId = prefs.getInt("UserID", -1);
        
        if(userId<0)
            selectUser();
        
        AssetFileManager.CopyAssets(getApplicationContext());
        
        mConnectionsLinearLayout = (LinearLayout)findViewById(R.id.main_menu_connections_layout);
        
        if (findViewById(R.id.fragment_container) != null) {

            if (savedInstanceState != null) {
                return;
            }

            MainMenuFragment firstFragment = new MainMenuFragment();

            firstFragment.setArguments(getIntent().getExtras());

            getSupportFragmentManager().beginTransaction()
                    .add(R.id.fragment_container, firstFragment).commit();
        }
        else
        {
        	mConnectionsLinearLayout.setBackground(this.getResources().getDrawable(R.drawable.gradient_row_selected));
        	
        	mBrightnessSeekBar = (SeekBar)findViewById(R.id.main_pane_seek_brightness);
        	mBrightnessSeekBar.setOnSeekBarChangeListener(brightnessSeekChangedListener);

        	mSystemSettingsObserver = new MyContentObserver(new Handler());
        	ContentResolver resolver = getContentResolver();
        	resolver.registerContentObserver(Settings.System.CONTENT_URI,true,mSystemSettingsObserver);
        	
			try {
				int brightnessMode = Settings.System.getInt(getContentResolver(), Settings.System.SCREEN_BRIGHTNESS_MODE);
				if (brightnessMode == Settings.System.SCREEN_BRIGHTNESS_MODE_AUTOMATIC) {
	        	    Settings.System.putInt(getContentResolver(), Settings.System.SCREEN_BRIGHTNESS_MODE, Settings.System.SCREEN_BRIGHTNESS_MODE_MANUAL);
	        	}
        	
				int brightnessVal = Settings.System.getInt(getContentResolver(), Settings.System.SCREEN_BRIGHTNESS);
				setBrightness(brightnessVal, true);
	        	
	        	
	        	
	        	
        	
			} catch (SettingNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
        	FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
    		mMainConnectionsFragment = new MainConnectionsFragment();
    		transaction.replace(R.id.fragment_container_two_pane, mMainConnectionsFragment);

    		transaction.commit();
        }
    }

	@Override
	protected void onStop() {
		// TODO Auto-generated method stub
		super.onStop();

		stopService(new Intent(this, LocationService.class));
	}



	@Override
	protected void onStart() {
		// TODO Auto-generated method stub
		super.onStart();
//		mGoogleAnalyticsTracker = GoogleAnalytics.getInstance(this).getTracker("UA-43128178-3");
//
//		mGoogleAnalyticsTracker.set(Fields.SCREEN_NAME, "Main Activity");

		ComponentName name = startService(new Intent(this, LocationService.class));
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		
		ContentResolver resolver = getContentResolver();
		resolver.unregisterContentObserver(mSystemSettingsObserver);
	}

	private void setBrightness(int brightnessValHex, boolean updateProgressBar) throws SettingNotFoundException {
		WindowManager.LayoutParams layoutParams = getWindow().getAttributes();
		layoutParams.screenBrightness = brightnessValHex/255f;
		getWindow().setAttributes(layoutParams);
		
		if(updateProgressBar)
		{
			int percent = (int)(layoutParams.screenBrightness * 100.0f);
			mBrightnessSeekBar.setProgress(percent);
		}
	}

	@Override
	public void onGateSelected(Gate selectedGate) {
		FrameLayout twoPaneFragmentContainer = (FrameLayout)findViewById(R.id.fragment_container_two_pane);
		Gson gson = new Gson();
        String jsonString = gson.toJson(selectedGate);
		// in two pane
		mStopTimesFragment = new StopTimesFragment();
		Bundle args = new Bundle();
		args.putString(StopTimesFragment.ARG_OBJ_JSON, jsonString);
		mStopTimesFragment.setArguments(args);
		
		if(twoPaneFragmentContainer!=null)
		{
			FragmentManager fm = getSupportFragmentManager();
			int bseCount = fm.getBackStackEntryCount();
			if(bseCount>0){
				BackStackEntry bse = fm.getBackStackEntryAt(bseCount-1);
				if(bse.getName().equals(StopTimesFragment.FRAGMENT_NAME))
				{
					fm.popBackStack(StopTimesFragment.FRAGMENT_NAME, FragmentManager.POP_BACK_STACK_INCLUSIVE);
				}
			}

		}
		
		FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
		if(twoPaneFragmentContainer!=null)
		{
			mConnectionsLinearLayout.setBackgroundColor(this.getResources().getColor(android.R.color.transparent));

			transaction.replace(R.id.fragment_container_two_pane, mStopTimesFragment);
		}
		else
		{
			transaction.replace(R.id.fragment_container, mStopTimesFragment);
		}
		transaction.addToBackStack(StopTimesFragment.FRAGMENT_NAME);
		transaction.commit();
	}

	@Override
	public void onRouteSelected() {
		FragmentManager fm = this.getSupportFragmentManager();
		fm.popBackStack(StopTimesFragment.FRAGMENT_NAME, FragmentManager.POP_BACK_STACK_INCLUSIVE);
		
		clearSelectedGate();

	}

	@Override
	public void onBackPressed() {
		clearSelectedGate();
		super.onBackPressed();
	}
    
	private void clearSelectedGate()
	{
		FragmentManager fm = this.getSupportFragmentManager();
		MainMenuFragment mmf = (MainMenuFragment)fm.findFragmentById(R.id.main_menu_fragment);
		if(mmf != null)
			mmf.clearSelection();
		
    	mConnectionsLinearLayout.setBackground(this.getResources().getDrawable(R.drawable.gradient_row_selected));
	}
    
	
	OnSeekBarChangeListener brightnessSeekChangedListener = new OnSeekBarChangeListener() {
		
		@Override
		public void onStopTrackingTouch(SeekBar seekBar) {
			// TODO Auto-generated method stub
			
		}
		
		@Override
		public void onStartTrackingTouch(SeekBar seekBar) {
			// TODO Auto-generated method stub
			
		}
		
		@Override
		public void onProgressChanged(SeekBar seekBar, int progress,
				boolean fromUser) {

			float brightnessVal = (float)progress/100.0f;
			int brightnessValHex = (int)(255f*brightnessVal);
			try {
				setBrightness(brightnessValHex, false);
			} catch (SettingNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}

		}
	};
	
	class MyContentObserver extends ContentObserver {
	    public MyContentObserver(Handler h) {
	        super(h);
	    }

	    @Override
	    public boolean deliverSelfNotifications() {
	        return true;
	    }

	    @Override
	    public void onChange(boolean selfChange) {
	        Log.d("IDTO MDT", "MyContentObserver.onChange("+selfChange+")");
	        super.onChange(selfChange);

	        int brightnessVal;
			try {
				brightnessVal = Settings.System.getInt(getContentResolver(), Settings.System.SCREEN_BRIGHTNESS);
				setBrightness(brightnessVal, true);
			} catch (SettingNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
	    }
	} 
	
	private void selectUser()
	{
		CharSequence colors[] = new CharSequence[] {"idto.tablet1@gmail.com", "idto.tablet2@gmail.com", "idto.tablet3@gmail.com", "idto.tablet4@gmail.com"};

		AlertDialog.Builder builder = new AlertDialog.Builder(this);
		builder.setTitle("Pick a user");
		builder.setItems(colors, new DialogInterface.OnClickListener() {
		    @Override
		    public void onClick(DialogInterface dialog, int which) {
		        int userId = 0;
		        String vehicleName= "";
		    	if(which ==0)
		    	{
		        	userId = 1009;
		        	vehicleName = "Tablet1";
		    	}
		    	else if(which ==1)
		    	{
		    		userId = 1010;
		    		vehicleName = "Tablet2";
		    	}
		    	else if(which ==2)
		    	{
		    		userId = 1011;
		    		vehicleName = "Tablet3";
		    	}
		    	else if(which ==3)
		    	{
		    		userId = 1026;
		    		vehicleName = "Tablet4";
		    	}
		    	else
		    	{
		    		userId = 1026;
		    		vehicleName = "Tablet4";
		    	}
		    	
		    	SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getApplicationContext());
		    	Editor edit = prefs.edit();
		    	edit.putInt("UserID", userId);
		    	edit.putString("VehicleName", vehicleName);
		    	edit.apply();
		    }
		});
		builder.show();
	}
}
