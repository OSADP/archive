/**
 * @file         GpsUBloxDevice.h
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


#ifndef _GPSUBLOXDEVICE_H_
#define _GPSUBLOXDEVICE_H_

#include "GpsDevice.h"

typedef enum {
	UBloxDynamicMode_Portable = 0,
	UBloxDynamicMode_NotUsed1,
	UBloxDynamicMode_Stationary,
	UBloxDynamicMode_Pedestrian,
	UBloxDynamicMode_Automotive,
	UBloxDynamicMode_Sea,
	UBloxDynamicMode_Airborn1,
	UBloxDynamicMode_Airborn2,
	UBloxDynamicMode_Airborn4,

} UBloxDynamicMode;

void gpsUBloxDevice_init(GpsDevice *gps, const char *deviceName);

void gpsUBloxDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate);
void gpsUBloxDevice_pollForNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType);
void gpsUBloxDevice_pollForNav5Info(GpsDevice *gps);
void gpsUBloxDevice_pollForCfgRateInfo(GpsDevice *gps);
void gpsUBloxDevice_setCfgRateInfo(GpsDevice *gps, uint16_t rate);
void gpsUBloxDevice_pollForPowerMode(GpsDevice *gps);
void gpsUBloxDevice_setDynamicMode(GpsDevice *gps, UBloxDynamicMode mode);
void gpsUBloxDevice_setStaticHoldThreshold(GpsDevice *gps, int cmPerSec);
void gpsUBloxDevice_loadDefaults(GpsDevice *gps);

#endif
