/**
 * @file         SituationalSender.h
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


#ifndef _SITUATIONALSENDER_H_
#define _SITUATIONALSENDER_H_

/********************* INCLUDES **********************/
#include "Thread.h"
#include "cJSONxtra.h"
#include "SingleList.h"
#include "WaveRadio.h"

/********************* TYPEDEFS AND STRUCTURES **********************/
typedef struct {
	SingleList *evaList; //NEEDS CLEANED
	SingleList *bsmList; //NEEDS CLEANED
	SingleList *timList; //NEEDS CLEANED
	Lock *lock; //NEEDS CLEANED
} SituationalSharedData;

typedef struct {
	Thread *thread; //NEEDS CLEANED
	SituationalSharedData data; //[CHILD NEEDS CLEANED]
} SituationalSender;

typedef enum {
	TimLaneType_Closed,
	TimLaneType_SpeedRestriction,
	TimLaneType_Incident,
	TimLaneType_Other
} TimLaneType;

/********************* PROTOTYPES **********************/
SituationalSender *situationalSender_getInstance();
void situationalSender_destroyInstance();
void situationalSender_processAndDestroyWaveMessage(SituationalSender *situationalSender, WaveRxPacket *packet);
void situationalSender_start(SituationalSender *situationalSender);
void situationalSender_stop(SituationalSender *situationalSender);
#endif
