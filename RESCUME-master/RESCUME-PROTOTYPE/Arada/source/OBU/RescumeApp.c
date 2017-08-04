/**
 * @file         RescumeApp.c
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
#include "RescumeApp.h"

#include "Debug.h"
#include "UiController.h"
#include "GpsAdapter.h"
#include "RescumeConfig.h"
#include "Responder.h"
#include "Queue.h"
#include "Thread.h"
#include "Oncoming.h"
#include "version/version.h"
#include "TimeStamp.h"
#include "VehicleData.h"

#include <BasicSafetyMessage.h>

#include <wave.h>

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>


/********************* TYPEDEFS AND STRUCTURES **********************/


/********************* INTERNAL PROTOTYPES **********************/
static int rescumeApp_processUiMsg(cJSON *root);
static void *rescumeApp_run(void *arg);

void rescumeApp_txDiagnostics();
void rescumeApp_txNmeaSentences(char *nmeaString, int length);
void rescumeApp_parseVehicleData(cJSON *root);
void rescumeApp_parseRtcmData(cJSON *root);


/********************* FILE GLOBAL VARIABLES **********************/
static Lock *queueLock;
static Queue *waveQueue;
static volatile int appRunning = 0;
static Thread *thread;
static int rescumeAppMode = APPMODE_UNKNOWN;
static struct timespec lastNmeaMessage = {0};
static int idRandomization = 0;


/********************* EXPOSED METHODS **********************/
void rescumeApp_start(RescumeConfig *options)
{
	if (appRunning)
		return;

	printf("\n\n");
	printf("*********************************************************************\n");
	printf("PROJECT: R.E.S.C.U.M.E. (INC-ZONE)\n");
	printf("\n");
	printf("BUILD VERSION: %s\n", VERSION);
	printf("BUILD DATE: %s\n", VERSION_DATE);
	printf("REPOSITORY VERSION: %s\n", VERSION_REPO);
	printf("\n");
	printf("COPYRIGHT (c) 2014 Battelle Memorial Institute. All rights reserved.\n");
	printf("*********************************************************************\n\n");

	//Attempt to create resources.
	if (!(thread = thread_create()))
		DBG_FATAL(printf("Application: Unable to create thread.\n"));

	if (!(waveQueue = queue_create((void (*)(void *))waveRadio_destroyRxPacket)))
		DBG_FATAL(printf("Application: Unable to create wave message queue.\n"));

	if (!(queueLock = lock_create()))
		DBG_FATAL(printf("Application: Unable to create lock for wave message queue.\n"));

	GpsDevice *gps = gps_getDefaultDevice();
	if (!gps)
		DBG_FATAL(printf("Application: Unable to get GPS Device.\n"));
	gpsDevice_setNmeaStringCallback(gps, rescumeApp_txNmeaSentences);

	//Save passed in options, and load those from file.
	if (options)
		rescumeConfig_set(options);
	rescumeConfig_loadFromFile("/var/bin/configuration.txt");

	//Get a fresh copy of the configuration, save the app mode which is only loaded once.
	RescumeConfig config;
	rescumeConfig_get(&config);
	rescumeAppMode = config.appMode;
	idRandomization = config.appVehicleId == 0;
	if (idRandomization)
	{
		rescumeConfig_lockForEdit(&config);
		config.appVehicleId = rand() << 16 | rand();
		rescumeConfig_updateAndUnlock(&config);
	}

	config.waveOptions.txPower = generateMaxtxpowReq();

	printf("Tx Power: %d\n", config.waveOptions.txPower);

	waveRadio_setOptions(&config.waveOptions);

	ui_startServer();
	ui_addRxMessageHandler(rescumeApp_processUiMsg);

	if (config.gpsEnableSpoof)
		gps_enableSpoofDevice(config.gpsSpoofFile);

	VehicleData vehData;
	vehData_init(&vehData);
	vehData_set(&vehData);
	
	//Attempt to start the application.
	switch(rescumeAppMode)
	{
		case APPMODE_RESPONDER:
			DBG_INFO(DBGM_GEN, printf("Application: Started application in RESPONDER mode.\n"));

			Responder *responder = responder_getInstance();
			if (!responder)
				DBG_FATAL(printf("Application: Unable to create responder instance\n"));

			responder_start(responder);
			
			break;

		default:
			DBG_WARN(DBGM_GEN, printf("Application: Unknown Application Mode.\n"));
		case APPMODE_ONCOMING:
			DBG_INFO(DBGM_GEN, printf("Application: Started application in ONCOMING VEHICLE mode.\n"));

			Oncoming *oncoming = oncoming_getInstance();
			if (!oncoming)
				DBG_FATAL(printf("Application: Unable to create oncoming instance\n"));

			oncoming_start(oncoming);

			break;
	}
	//Start the tread to pull and process WAVE messages.
	thread_start(thread, rescumeApp_run, NULL);
	appRunning = 1;
}

void rescumeApp_stop()
{
	if (!appRunning)
		return;

	appRunning = 0;

	DBG_INFO(DBGM_GEN, printf("Application: Stopping\n"));

	thread_join(thread);
	responder_destroyInstance();
	oncoming_destroyInstance();

	ui_stopServer();
	
	gps_destroy();

	queue_destroy(waveQueue);
	lock_destroy(queueLock);
}

void rescumeApp_pushWaveMessage(WaveRxPacket *packet)
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
int rescumeApp_processUiMsg(cJSON *root)
{
	if (!appRunning)
		return 0;

	cJSON *typeId = cJSON_GetObjectItem(root,"typeid");

	if (typeId == NULL || typeId->type != cJSON_String)
	{
		DBG_ERR(DBGM_APP, printf("RescumeApp::rescumeApp_processUiMsg(): No \"typeid\" found\n"));
	}
	else
	{
		DBG(DBGM_APP, printf("RescumeApp::rescumeApp_processUiMsg(): Found \"typeid\" = %s\n", typeId->valuestring));

		if (!strcmp(typeId->valuestring, "VEH"))
		{
			rescumeApp_parseVehicleData(root);
			return 1;
		}
		else if (!strcmp(typeId->valuestring, "RTCM"))
		{
			rescumeApp_parseRtcmData(root);
			return 1;
		}
		else
		{
			if (appRunning)
			{
				switch(rescumeAppMode)
				{
					case APPMODE_RESPONDER:
						return responder_processUiMsg(responder_getInstance(), root);

					case APPMODE_ONCOMING:
					default:
						return oncoming_processUiMsg(oncoming_getInstance(), root);
				}
			}
		}
	}
	
	return 0;
}

/**
 * Pull WAVE messages off of the message queue, and allow responder or oncoming modules to process them. 
 * Also does timed app wide functions like diagnostic messages.
 */
void *rescumeApp_run(void *arg)
{
	RescumeConfig config;

	struct timespec lastDiaMessage;
	struct timespec lastIdRandomizationMessage;
	clock_gettime(CLOCK_MONOTONIC, &lastDiaMessage);
	clock_gettime(CLOCK_MONOTONIC, &lastIdRandomizationMessage);

	while(thread_isRunning(thread))
	{
		rescumeConfig_get(&config);
		struct timespec currentTime;
		clock_gettime(CLOCK_MONOTONIC, &currentTime);

		rescumeConfig_lockForEdit(&config);
		if (!config.appVehicleIdLock && tsDiff(lastIdRandomizationMessage, currentTime) >= 300.0)//config.uiDiaMaxRate)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastIdRandomizationMessage);
			config.appVehicleId = rand() << 16 | rand();
		}
		rescumeConfig_updateAndUnlock(&config);

		//Process off of the message queue
		while (waveQueue->count > 0)
		{
			lock_lock(queueLock);
			WaveRxPacket *packet = queue_pop(waveQueue);
			lock_unlock(queueLock);

			if (!packet)
				continue;

			//Destroy BSMs, pass rest on to proper application module.
			if (packet->type == &asn_DEF_BasicSafetyMessage)
			{
				waveRadio_destroyRxPacket(packet);
			}
			else
			{
				switch(rescumeAppMode)
				{
					case APPMODE_RESPONDER:
						responder_processAndDestroyWaveMessage(responder_getInstance(), packet);
						break;

					case APPMODE_ONCOMING:
					default:
						oncoming_processAndDestroyWaveMessage(oncoming_getInstance(), packet);
						break;
				}
			}
		}

		if (tsDiff(lastDiaMessage, currentTime) >= 1.0/config.uiDiaMaxRate)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastDiaMessage);
			rescumeApp_txDiagnostics();
		}

		usleep(10000);
	}

	return NULL;
}

void rescumeApp_txDiagnostics()
{
	cJSON *root = cJSON_CreateObject();

	char buf[100];

	RescumeConfig config;
	rescumeConfig_get(&config);
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

	snprintf(buf, 100, "0x%08x", config.appVehicleId);
	cJSON_AddStringToObject(root, "vehicleid", buf);
	cJSON_AddNumberToObject(root, "vehicleidlock", config.appVehicleIdLock);


	switch(rescumeAppMode)
	{
		case APPMODE_RESPONDER:
			responder_isEvasEnabled(responder_getInstance()) ? cJSON_AddTrueToObject(root, "evaenabled") : cJSON_AddFalseToObject(root, "evaenabled");
			responder_isTimsEnabled(responder_getInstance()) ? cJSON_AddTrueToObject(root, "timenabled") : cJSON_AddFalseToObject(root, "timenabled");
			break;

		case APPMODE_ONCOMING:
		default:
			cJSON_AddNumberToObject(root, "evacount", oncoming_getEvaCount(oncoming_getInstance()));
			oncoming_getTimPacketId(oncoming_getInstance(), buf, sizeof(buf));
			cJSON_AddStringToObject(root, "activetimid", buf);
			double laneLocation;
			if (oncoming_tryGetCurrentRawLanePosition(oncoming_getInstance(), &laneLocation))
				cJSON_AddNumberToObject(root, "rawlanelocation", laneLocation);
			break;
	}

	//Application
	cJSON_AddStringToObject(root, "version", VERSION);
	cJSON_AddStringToObject(root, "versiondate", VERSION_DATE);
	cJSON_AddStringToObject(root, "versionrepo", VERSION_REPO);

	ui_sendMessage(root);
	cJSON_Delete(root);
}

void rescumeApp_txNmeaSentences(char *nmeaString, int length)
{
	if (strstr(nmeaString, "$GPGGA") != nmeaString)
		return;

	RescumeConfig config;
	rescumeConfig_get(&config);

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	if (tsDiff(lastNmeaMessage, currentTime) < 1.0/config.uiNmeaMaxRate)
		return;

	clock_gettime(CLOCK_MONOTONIC, &lastNmeaMessage);

	cJSON *root = cJSON_CreateObject();

	cJSON_AddStringToObject(root, "typeid", "NMEA");
	cJSON_AddStringToObject(root, "gga", nmeaString);

	ui_sendMessage(root);
	cJSON_Delete(root);
}

void rescumeApp_parseVehicleData(cJSON *root)
{
	VehicleData vehData;
	vehData_lockForEdit(&vehData);

	clock_gettime(CLOCK_MONOTONIC, &vehData.time);

	if (!cJSONxtra_tryGetDouble(root, "speed", &vehData.speed))
		vehData.speed = -1;

	vehData_updateAndUnlock(&vehData);
}

void rescumeApp_parseRtcmData(cJSON *root)
{
	GpsDevice *gps = gps_getDefaultDevice();
	if (!gps)
		return;

	if (!root)
		return;

	cJSON *rtcm2_3 = cJSON_GetObjectItem(root, "2.3");

	if (!rtcm2_3 || !rtcm2_3->valuestring)
		return;

	char *data = rtcm2_3->valuestring;

	int bytecount = strlen(data)/2;
	if (bytecount > 2000)
		return;
	
	uint8_t bytearray[2000];
	int i;
	for(i = 0; i < bytecount; i++) {
		sscanf(data+(2*i), "%2hhx", &bytearray[i]);
	}

	gpsDevice_injectRtcmData(gps, bytearray, bytecount);
}


