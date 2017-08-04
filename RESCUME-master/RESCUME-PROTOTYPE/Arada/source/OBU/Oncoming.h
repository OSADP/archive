/**
 * @file         Oncoming.h
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

#ifndef _ONCOMING_H_
#define _ONCOMING_H_

#include <TravelerInformation.h>
#include "WaveRadio.h"
 
#include "Thread.h"
#include "cJSONxtra.h"
#include "SingleList.h"


typedef struct {
	WaveRxPacket *packet;
	uint32_t vehicleId;
	double latitude;
	double longitude;

	int collisionDetected;
	struct timespec collisionDetectionTime;

} EvaMetadata;

typedef enum {
	TimLaneType_Closed,
	TimLaneType_SpeedRestriction,
	TimLaneType__length
} TimLaneType;

typedef struct {
	ShapePointSet_t *shapePointSet;
	TimLaneType type;
	int laneNumber;
	double distanceToBackOfIncident;
	double distanceToAlert2;
	double distanceToAlert1;
} TimLaneMetaData;

typedef struct {
	WaveRxPacket *packet;
	int postedSpeedMph;
	int reducedSpeedMph;

	int sideOfRoad;
	SingleList *laneInformation; //Of TimLaneMetaData
	int numberOfLanes;
	int numberOfClosedLanes;

} TimMetaData;

typedef struct {
	SingleList *evaList; //NEEDS CLEANED
	TimMetaData tim; //NEEDS CLEANED

	Lock *lock; //NEEDS CLEANED
} OncomingSharedData;

typedef struct {
	Thread *thread; //NEEDS CLEANED
	OncomingSharedData data; //[CHILD NEEDS CLEANED]
	
	struct timespec lastTimReceiveTime;
} Oncoming;

Oncoming *oncoming_getInstance();
void oncoming_destroyInstance();
void oncoming_start(Oncoming *oncoming);
void oncoming_stop(Oncoming *oncoming);
void oncoming_processAndDestroyWaveMessage(Oncoming *oncoming, WaveRxPacket *packet);
int oncoming_processUiMsg(Oncoming *oncoming, cJSON *root);

int oncoming_getEvaCount(Oncoming *oncoming);
void oncoming_getTimPacketId(Oncoming *oncoming, char *buf, int length);
int oncoming_tryGetCurrentRawLanePosition(Oncoming *oncoming, double *out);

#endif
