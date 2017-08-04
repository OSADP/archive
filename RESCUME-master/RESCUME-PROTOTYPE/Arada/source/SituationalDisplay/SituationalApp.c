/**
 * @file         SituationalApp.c
 * @author       Joshua Branch, Veronica Hohe
 * 
 * @copyright Copyright (c) 2014 Battelle Memorial Institute. All rights reserved.
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

/********************* INCLUDES **********************/
#include "SituationalApp.h"

#include "Debug.h"
#include "UiController.h"
#include "GpsAdapter.h"
#include "SituationalSender.h"
#include "Queue.h"
#include "Thread.h"
#include "Version.h"
#include "TimeStamp.h"
#include "VehicleData.h"

#include <EmergencyVehicleAlert.h>
#include <TravelerInformation.h>
#include <BasicSafetyMessage.h>

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>


/********************* TYPEDEFS AND STRUCTURES **********************/


/********************* INTERNAL PROTOTYPES **********************/
static int situationalApp_processUiMsg(cJSON *root);
static void *situationalApp_run(void *arg);

void situationalApp_txDiagnostics();
void situationalApp_txNmeaSentences(char *nmeaString, int length);


/********************* FILE GLOBAL VARIABLES **********************/
static Lock *queueLock;
static Queue *waveQueue;
static volatile int appRunning = 0;
static Thread *thread;
static struct timespec lastNmeaMessage = {0};


/********************* EXPOSED METHODS **********************/
void situationalApp_start(void)
{
	if (appRunning)
		return;

	//Attempt to create resources.
	if (!(thread = thread_create()))
		DBG_FATAL(printf("Application: Unable to create thread.\n"));
	
	if (!(waveQueue = queue_create(waveRadio_destroyRxPacket)))
		DBG_FATAL(printf("Application: Unable to create wave message queue.\n"));

	if (!(queueLock = lock_create()))
		DBG_FATAL(printf("Application: Unable to create lock for wave message queue.\n"));

	GpsDevice *gps = gps_getDefaultDevice();
	if (!gps)
		DBG_FATAL(printf("Application: Unable to get GPS Device.\n"));
	gpsDevice_setNmeaStringCallback(gps, situationalApp_txNmeaSentences);

	ui_startServer();
	ui_addRxMessageHandler(situationalApp_processUiMsg);

	SituationalSender *situationalSender = situationalSender_getInstance();
	if(!situationalSender)
		DBG_FATAL(printf("Application: Unable to create Situational Sender instance"));
	situationalSender_start(situationalSender);

	//Start the tread to pull and process WAVE messages.
	thread_start(thread, situationalApp_run, NULL);
	appRunning = 1;
}

void situationalApp_stop()
{
	if (!appRunning)
		return;

	appRunning = 0;

	DBG_INFO(DBGM_GEN, printf("Application: Stopping\n"));

	thread_join(thread);

	ui_stopServer();
	situationalSender_destroyInstance();
	gps_destroy();

	queue_destroy(waveQueue);
	lock_destroy(queueLock);

}

void situationalApp_pushWaveMessage(WaveRxPacket *packet)
{
	if (!appRunning)
		return;

	lock_lock(queueLock);
	queue_push(waveQueue, packet);
	lock_unlock(queueLock);
}

/***********************************************
 * INTERNAL METHODS
 **********************************************/
/**
 * Process all UI messages received.
 */
int situationalApp_processUiMsg(cJSON *root)
{
	if (!appRunning)
		return 0;

	cJSON *typeId = cJSON_GetObjectItem(root,"typeid");

	if (typeId == NULL || typeId->type != cJSON_String)
	{
		DBG_ERR(DBGM_APP, printf("SituationalApp::situationalApp_processUiMsg(): No \"typeid\" found\n"));
	}
	else
	{
		DBG(DBGM_APP, printf("SituationalApp::sebugApp_processUiMsg(): Found \"typeid\" = %s\n", typeId->valuestring));
		if (appRunning)
		{
		}
	}
	
	return 0;
}

/**
 * Pull WAVE messages off of the message queue, and allow situationalsender to process them. 
 * Also does timed app wide functions like diagnostic messages.
 */
void *situationalApp_run(void *arg)
{
	struct timespec lastDiaMessage;
	clock_gettime(CLOCK_MONOTONIC, &lastDiaMessage);

	while(thread_isRunning(thread))
	{	
		struct timespec currentTime;
		clock_gettime(CLOCK_MONOTONIC, &currentTime);

		//Process off of the message queue
		while (waveQueue->count > 0)
		{
			lock_lock(queueLock);
			WaveRxPacket *packet = queue_pop(waveQueue);
			lock_unlock(queueLock);

			if (!packet)
				continue;

			situationalSender_processAndDestroyWaveMessage(situationalSender_getInstance(), packet);
		}

		if (tsDiff(lastDiaMessage, currentTime) >= 1.0/4.0)
		{
			tsAddSeconds(&lastDiaMessage, 1.0/4.0);
			situationalApp_txDiagnostics(); 
		} 

		usleep(10000);
	}
	return NULL;
}

/**
 * Send diagnostics information.
 */
void situationalApp_txDiagnostics()
{
	cJSON *root = cJSON_CreateObject();

	GPSData gpsData;
	gpsDevice_getData(gps_getDefaultDevice(), &gpsData);

	cJSON_AddStringToObject(root, "typeid", "DIA");
	//GPS
	cJSON_AddNumberToObject(root, "gpsfix", gpsData.fix);
	cJSON_AddNumberToObject(root, "heading", gpsData.course);
	cJSON_AddNumberToObject(root, "speed", gpsData.speed);
	cJSON_AddNumberToObject(root, "latitude", gpsData.latitude);
	cJSON_AddNumberToObject(root, "longitude", gpsData.longitude);
	cJSON_AddNumberToObject(root, "vdop", gpsData.vdop);
	cJSON_AddNumberToObject(root, "hdop", gpsData.hdop);
	cJSON_AddNumberToObject(root, "satinuse", gpsData.numsats);

	//Application
	cJSON_AddStringToObject(root, "version", VERSION);

	ui_sendMessage(root);
	cJSON_Delete(root);
}

/**
 * Send NMEA information.
 */
void situationalApp_txNmeaSentences(char *nmeaString, int length)
{
	if (strstr(nmeaString, "$GPGGA") != nmeaString)
		return;

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	if (tsDiff(lastNmeaMessage, currentTime) < 1.0/0.5)
		return;

	clock_gettime(CLOCK_MONOTONIC, &lastNmeaMessage);

	cJSON *root = cJSON_CreateObject();

	cJSON_AddStringToObject(root, "typeid", "NMEA");
	cJSON_AddStringToObject(root, "gga", nmeaString);

	ui_sendMessage(root);
	cJSON_Delete(root);
}

