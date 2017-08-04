/**
 * @file         RescumeConfig.h
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

#ifndef _RESCUMECONFIG_H_
#define _RESCUMECONFIG_H_

#include <inttypes.h>
#include "WaveRadio.h"
#include "DsrcUtils.h"

#define APPMODE_UNKNOWN -1
#define APPMODE_ONCOMING 0
#define APPMODE_RESPONDER 1

typedef enum {
	VehicleIdLock_noLock = 0,
	VehicleIdLock_temporary,
	VehicleIdLock_permanent
} VehicleIdLock;

typedef struct RescumeConfig {

	//GPS
	uint8_t gpsEnableSpoof; //CONFIG FILE
	char gpsSpoofFile[200]; //CONFIG FILE

	//Application
	uint8_t appMode; //CONFIG FILE
	VehicleIdLock appVehicleIdLock;
	uint32_t appVehicleId; //CONFIG FILE

	uint8_t uiEnableConsoleMessages; //CONFIG FILE
	double uiDiaMaxRate; //CONFIG FILE
	double uiNmeaMaxRate; //CONFIG FILE

	double dsrcBsmMaxRate; //CONFIG FILE
	double dsrcEvaMaxRate; //CONFIG FILE
	double dsrcTimMaxRate; //CONFIG FILE
	double dsrcAcmMaxRate; //CONFIG FILE

	double oncomingStaleEvaTimeout; //CONFIG FILE
	//double oncomingLegibilityDistance; //CONFIG FILE
	double oncomingEmergencyDecelRate; //CONFIG FILE
	double oncomingAggressiveDecelRate; //CONFIG FILE
	//double oncoming...
	//int oncomingIntentionalityMode;
	double oncomingMaxCalculationRate;
	double oncomingCollisionDriverPrtTime;
	double oncomingCollisionResponderPrtTime;
	double oncomingCollisionResetTime;
	double oncomingCollisionLaneWidth;
	double oncomingCollisionMinSpeed;

	char responderPreloadedTimFile[200];
	double responderStaleAcmTimeout;
	double responderMaxThreatMessageRate;

	WaveRadioOptions waveOptions; //HARD CODED

	struct DsrcMatchOptions dsrcMatchOptions; //CONFIG FILE

} RescumeConfig;

void rescumeConfig_set(RescumeConfig *options);
void rescumeConfig_get(RescumeConfig *out);
void rescumeConfig_lockForEdit(RescumeConfig *out);
void rescumeConfig_updateAndUnlock(RescumeConfig *data);

void rescumeConfig_loadFromFile(const char *fileName);

#endif
