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

	lock_lock(gps->lock);

	memcpy(out, &gps->data, sizeof(GPSData));

	lock_unlock(gps->lock);
}

void gpsDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length)
{
	if (!gps)
		return;

	if (gps->func_injectRtcmData)
		gps->func_injectRtcmData(gps, data, length);
}

void gpsDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate)
{
	if (!gps)
		return;

	if (gps->func_setNmeaSentenceRate)
		gps->func_setNmeaSentenceRate(gps, sentenceType, rate);
}

int gpsDevice_parseNmeaSentence(GpsDevice *gps, char *data, size_t length)
{
	if (strstr(data, "$GPGGA") != data && 
		strstr(data, "$GPGSA") != data && 
		strstr(data, "$GPGSV") != data && 
		strstr(data, "$GPRMC") != data && 
		strstr(data, "$GPVTG") != data )
		return 1; //Just return success because we can't actually parse these.

	int results = nmea_parse(&gps->nmeaParser, data, length, &gps->nmeaInfo);

	if (!results)
		return 0;
	
	nmeaPOS dpos;

	nmea_info2pos(&gps->nmeaInfo, &dpos);

	lock_lock(gps->lock);

	gps->data.latitude = dpos.lat*360/TWO_PI;
	gps->data.longitude = dpos.lon*360/TWO_PI;
	gps->data.fix = gps->nmeaInfo.sig;
	gps->data.speed = gps->nmeaInfo.speed * 0.2777777777778;
	
	if (gps->data.speed > 1.0)
		gps->data.course = gps->nmeaInfo.direction;
	
	gps->data.altitude = gps->nmeaInfo.elv;
	gps->data.numsats = gps->nmeaInfo.satinfo.inuse;

	gps->data.vdop = gps->nmeaInfo.VDOP;
	gps->data.hdop = gps->nmeaInfo.HDOP;

	lock_unlock(gps->lock);
	
	return results;
}

