/**
 * @file         GpsDevice.c
 * @author       Joshua Branch
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


#include "GpsDevice.h"

#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <sys/socket.h>
#include <signal.h>

#include "Debug.h"

#define TWO_PI 6.283185307179586476925286766559

void *gpsDevice_run(void *arg);

/***********************************************
 * EXPOSED METHODS
 **********************************************/
void gpsDevice_setNmeaStringCallback(GpsDevice *gps, void (*nmeaSentenceCallback)(char *, int))
{
	gps->nmeaSentenceCallback = nmeaSentenceCallback;
}

void gpsDevice_start(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->func_run)
		return;

	if (!gps->threadId)
	{
		DBG_INFO(DBGM_GPS, printf("GpsDevice '%s': Starting...\n", gps->deviceName));

		gps->enabled = 1;
		pthread_create(&gps->threadId, NULL, gps->func_run, gps);
	}
}

void gpsDevice_stop(GpsDevice *gps)
{
	if (!gps)
		return;

	if(gps->threadId)
	{
		DBG_INFO(DBGM_GPS, printf("GpsDevice '%s': Stopping...\n", gps->deviceName));

		gps->enabled = 0;

		shutdown(gps->gpsfd, SHUT_RDWR);

		//Only using pthread_cancel because sometimes it doesn't actually shut down :( and it gets annoying.
		pthread_cancel(gps->threadId);

		DBG_INFO(DBGM_GPS, printf("GpsDevice '%s': Waiting for join...\n", gps->deviceName));
		pthread_join(gps->threadId, NULL);
		gps->threadId = 0;

		DBG_INFO(DBGM_GPS, printf("GpsDevice '%s': Stopped\n", gps->deviceName));
	}
}

void gpsDevice_getData(GpsDevice *gps, GPSData *out)
{
	if (!gps)
		return;

	lockLock(gps->lock);

	memcpy(out, &gps->data, sizeof(GPSData));

	lockUnlock(gps->lock);
}

