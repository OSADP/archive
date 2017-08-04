/**
 * @file         GpsSpoofDevice.c
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


#include "GpsSpoofDevice.h"

#include "TimeStamp.h"

#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <sys/socket.h>

#include "Debug.h"

void *gpsSpoofDevice_run(void *arg);

void gpsSpoofDevice_init(GpsDevice *gps, char *file)
{
	if (!gps)
		return;

	memset(gps, 0, sizeof(GpsDevice));
	gps->lock = lock_create();
	
	strncpy(gps->deviceName, file, GPS_DEVICE_DEVICE_NAME_MAX_LENGTH);

	nmea_zero_INFO(&gps->nmeaInfo);
	nmea_parser_init(&gps->nmeaParser);

	gps->nmeaSentenceCallback = NULL;
	gps->threadId = 0;

	gps->func_run = gpsSpoofDevice_run;
	gps->func_injectRtcmData = NULL;
	gps->func_setNmeaSentenceRate = NULL;
}

void *gpsSpoofDevice_run(void *arg)
{
	GpsDevice *gps = (GpsDevice *)arg;

	const int MAX_BUFFER_LENGTH = 300;
	char buffer[MAX_BUFFER_LENGTH];

	FILE *file = NULL;

	struct timespec tstart={0,0}, tend={0,0};

	while(!file)
	{
		file = fopen(gps->deviceName, "r");

		if (!file)
		{
			DBG_ERR(DBGM_GPS, printf("GpsSpoofDevice '%s': Could not open connection to Spoof file '%s'\n", gps->deviceName, gps->deviceName));
			perror("GpsSpoofDevice");
			sleep(1);

			if (!gps->enabled)
				return NULL;
		}
		clock_gettime(CLOCK_MONOTONIC, &tstart);
	}

	DBG_INFO(DBGM_GPS, printf("GpsSpoofDevice '%s': Opened Spoof file '%s'\n", gps->deviceName, gps->deviceName));

	int timeOffsetInitialized = 0;
	while(!feof(file) && gps->enabled)
	{
		if (!fgets(buffer, MAX_BUFFER_LENGTH, file))
		{
			DBG_ERR(DBGM_GPS, printf("GpsSpoofDevice '%s': Err reading spoof file line\n", gps->deviceName));
			continue;
		}

		//Time, Lat, Long, Heading, Speed
		double lineDelay, latitude, longitude, heading, speed;

		if (sscanf(buffer, "%lf%*c %lf%*c %lf%*c %lf%*c %lf", &lineDelay, &latitude, &longitude, &heading, &speed) != 5)
		{
			DBG_ERR(DBGM_GPS, printf("GpsSpoofDevice '%s': Err reading spoof file line [%s]\n", gps->deviceName, buffer));
			continue;
		}
		if (!timeOffsetInitialized)
		{
			tsAddSeconds(&tstart, -lineDelay);
			timeOffsetInitialized = 1;
		}

		//Sleep while checking for enable flag to be dropped
		clock_gettime(CLOCK_MONOTONIC, &tend);
		double sleepTime;
		while(((sleepTime = lineDelay - tsDiff(tstart, tend)) > 1) && gps->enabled)
		{
			sleep(1);
			clock_gettime(CLOCK_MONOTONIC, &tend);

		}
		if (!gps->enabled)
			break;
		if (sleepTime > 0)
			usleep(sleepTime * 1000000);

		lock_lock(gps->lock);
		gps->data.latitude = latitude;
		gps->data.longitude = longitude;
		gps->data.speed = speed;
		gps->data.course = heading;
		gps->data.fix = 1;
		DBG_INFO(DBGM_GPS, printf("GpsSpoofDevice '%s': Updated GPS. [Line Delay: %f]\n", gps->deviceName, lineDelay));
		lock_unlock(gps->lock);
	}

	DBG_INFO(DBGM_GPS, printf("GpsSpoofDevice '%s': Closing GpsSpoofFile '%s'\n", gps->deviceName, gps->deviceName));
	fclose(file);

	return NULL;
}
