package com.fratis.emergencyresponse;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
@SuppressWarnings("deprecation")

public class NotificationHelper 
{
	private Context mContext;
	public int NOTIFICATION_ID = 1;
	private Notification mNotification;
	private NotificationManager mNotificationManager;
	private PendingIntent mContentIntent;
	private CharSequence mContentTitle;

	public NotificationHelper(Context context, int id) 
	{
		NOTIFICATION_ID = id;
		mContext = context;
	}

	public void createNotification(String title, int icon) 
	{
		mNotificationManager = (NotificationManager) mContext
				.getSystemService(Context.NOTIFICATION_SERVICE);

		CharSequence tickerText = title; // Initial text that appears in the status bar
		long when = System.currentTimeMillis();
		mNotification = new Notification(icon, tickerText, when);
		mContentTitle = title; // Full title of the notification
		CharSequence contentText = "0% complete"; // Text of the notification 
		Intent notificationIntent = new Intent();
		mContentIntent = PendingIntent.getActivity(mContext, 0,	notificationIntent, 0);
		mNotification.setLatestEventInfo(mContext, mContentTitle, contentText, mContentIntent);
		mNotification.flags = Notification.FLAG_ONGOING_EVENT;
		mNotificationManager.notify(NOTIFICATION_ID, mNotification);
	}

	public void setIcon(int icon) 
	{
		mNotification.icon = icon;
	}

	public void textUpdate(CharSequence text) 
	{
		mNotification.setLatestEventInfo(mContext, mContentTitle, text, mContentIntent);
		mNotificationManager.notify(NOTIFICATION_ID, mNotification);
	}

	public void progressUpdate(int percentageComplete) 
	{
		CharSequence contentText = percentageComplete + "% complete";
		mNotification.setLatestEventInfo(mContext, mContentTitle, contentText, mContentIntent);
		mNotificationManager.notify(NOTIFICATION_ID, mNotification);
	}

	public void completed() 
	{
		mNotificationManager.cancel(NOTIFICATION_ID);
	}
}