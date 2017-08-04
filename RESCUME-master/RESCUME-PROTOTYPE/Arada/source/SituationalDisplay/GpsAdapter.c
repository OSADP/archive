/**
 * @file         GpsAdapter.c
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

#include "GpsAdapter.h"
#include "GpsSpoofDevice.h"
#include "GpsUBloxDevice.h"

#include "Debug.h"

#include <stdlib.h>
#include <stdio.h>
#include <pthread.h>


static int gpsInitialized = 0;
static GpsDevice *gpsDefaultDevice = NULL;
static GpsDevice *gpsSpoofDevice = NULL;
static pthread_mutex_t gpsInitLock = PTHREAD_MUTEX_INITIALIZER;

void gpsAdpater_init();

GpsDevice *gps_getDefaultDevice(void)
{
	GpsDevice *results = NULL;

	pthread_mutex_lock(&gpsInitLock);
	if (!gpsInitialized)
	{
		gpsAdpater_init();
		gpsInitialized = 1;
	}

	if (gpsSpoofDevice)
		results = gpsSpoofDevice;
	else
		results = gpsDefaultDevice;

	pthread_mutex_unlock(&gpsInitLock);

	return results;
}

void gps_enableSpoofDevice(char *file)
{
	pthread_mutex_lock(&gpsInitLock);

	if (gpsSpoofDevice)
		gpsDevice_stop(gpsSpoofDevice);
	else
		gpsSpoofDevice = (GpsDevice *)calloc(1, sizeof(GpsDevice));

	if (gpsSpoofDevice)
	{
		gpsSpoofDevice_init(gpsSpoofDevice, file);
		gpsDevice_start(gpsSpoofDevice);
	}
	else
		DBG_ERR(DBGM_GPS, printf("Unable to allocate memory for GpsSpoofDevice"));

	pthread_mutex_unlock(&gpsInitLock);
}

void gps_disableSpoofDevice()
{
	pthread_mutex_lock(&gpsInitLock);

	if (gpsSpoofDevice)
	{
		gpsDevice_stop(gpsSpoofDevice);
		free(gpsSpoofDevice);
		gpsSpoofDevice = NULL;
	}

	pthread_mutex_unlock(&gpsInitLock);
}

void gps_destroy()
{
	pthread_mutex_lock(&gpsInitLock);

	if (gpsSpoofDevice)
	{
		gpsDevice_stop(gpsSpoofDevice);
		free(gpsSpoofDevice);
		gpsSpoofDevice = NULL;
	}

	if (gpsDefaultDevice)
	{
		gpsDevice_stop(gpsDefaultDevice);
		free(gpsDefaultDevice);
		gpsDefaultDevice = NULL;
	}

	pthread_mutex_unlock(&gpsInitLock);	
}

void gpsAdpater_init()
{
	if (gpsDefaultDevice)
		return;

	#if defined(LOCOMATE_MINI) || defined(LOCOMATE_GO) || defined(LOCOMATE_ME)
		gpsDefaultDevice = (GpsDevice *)calloc(1, sizeof(GpsDevice));
		if (gpsDefaultDevice)
		{
			gpsUBloxDevice_init(gpsDefaultDevice, "/dev/ttyACM0");
			gpsDevice_start(gpsDefaultDevice);
		}
		else
			DBG_FATAL(printf("Unable to allocate memory for GpsDevice"));
	#else
		#error "GPS NOT SUPPORTED: Please select supported hardware type: { -DLOCOMATE_MINI, -DLOCOMATE_GO, -DLOCOMATE_ME }"
	#endif
}

