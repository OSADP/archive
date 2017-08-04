/**
 * @file         GpsDevice.h
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


#ifndef _GPSDEVICE_H_
#define _GPSDEVICE_H_

#include <pthread.h>

#include <gpsc_probe.h>
#include <nmea/nmea.h>
#include "Lock.h"

#define TWO_PI 6.283185307179586476925286766559

#define NMEA_GPGLL 0
#define NMEA_GPGGA 1
#define NMEA_GPGSA 2
#define NMEA_GPGSV 3
#define NMEA_GPRMC 4
#define NMEA_GPVTG 5

#define GPS_DEVICE_DEVICE_NAME_MAX_LENGTH 200

typedef struct GpsDevice {
	pthread_t threadId;
	Lock *lock;
	int gpsfd;
	volatile int enabled;
	char deviceName[GPS_DEVICE_DEVICE_NAME_MAX_LENGTH];

	void (*nmeaSentenceCallback)(char *, int);

	nmeaINFO nmeaInfo;
	nmeaPARSER nmeaParser;

	GPSData data;

	void *(*func_run)(void*);
	void (*func_injectRtcmData)(struct GpsDevice*, uint8_t*, int);
	void (*func_setNmeaSentenceRate)(struct GpsDevice*, uint8_t, uint8_t);

} GpsDevice;

//void gpsDevice_init(GpsDevice *gps, const char *deviceName);
void gpsDevice_setNmeaStringCallback(GpsDevice *gps, void (*nmeaSentenceCallback)(char *, int));

void gpsDevice_start(GpsDevice *gps);
void gpsDevice_stop(GpsDevice *gps);
void gpsDevice_getData(GpsDevice *gps, GPSData *out);

//UTILITIY
int gpsDevice_parseNmeaSentence(GpsDevice *gps, char *data, size_t length);

//OVERRIDE
void gpsDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length);
void gpsDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate);

#endif
