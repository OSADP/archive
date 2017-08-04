/**
 * @file         Responder.h
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

#ifndef _RESPONDER_H_
#define _RESPONDER_H_

/********************* INCLUDES **********************/
#include <TravelerInformation.h>
#include "WaveRadio.h"
#include "RescumeAlaCartePayload.h"
#include "SingleList.h"
#include "Lock.h"

#include "Thread.h"
#include "cJSONxtra.h"


/********************* TYPEDEFS AND STRUCTURES **********************/

typedef struct {
	RescumeAlaCartePayload payload;
	struct timespec rxTime;
} AcmMetaData;

typedef struct  {
	int enabled;
	int itisCode;
} EvaGenerationInfo;

typedef struct  {
	int enabled;
	int active;
	uint8_t encodedMsg[1200];
	int encodedMsgLength;
	TravelerInformation_t *msgStruct;
	struct timespec differentTimRxTime;
	//double startDelay;
} TimGenerationInfo;

typedef struct {
	EvaGenerationInfo evaInfo;
	TimGenerationInfo timInfo;
	SingleList *activeThreatAcms;
	SingleList *activeCollisionAcms;

	Lock *dataLock;
	Thread *thread;
} Responder;


/********************* PROTOTYPES **********************/
Responder *responder_getInstance();
void responder_destroyInstance();
void responder_start(Responder *responder);
void responder_stop(Responder *responder);
void responder_processAndDestroyWaveMessage(Responder *responder, WaveRxPacket *packet);
int responder_processUiMsg(Responder *responder, cJSON *root);

int responder_isEvasEnabled(Responder *responder);
int responder_isTimsEnabled(Responder *responder);

#endif
