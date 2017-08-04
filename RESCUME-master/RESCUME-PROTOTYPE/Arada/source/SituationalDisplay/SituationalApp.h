/**
 * @file         SituationalApp.h
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

#ifndef _APPLICATION_H_
#define _APPLICATION_H_

/********************* INCLUDES **********************/
#include "WaveRadio.h"
#include "SingleList.h"
#include "Lock.h"

/********************* TYPEDEFS AND STRUCTURES **********************/
typedef struct {
	uint32_t *vehicleId;
	int count;
} vehicleIdNode;

typedef struct {
	SingleList *evaList; //NEEDS CLEANED
	Lock *lock; //NEEDS CLEANED
} EvaList;

/********************* PROTOTYPES **********************/
/**
 * Starts the primary Rescume Application.  This method is NOT THREAD SAFE.
 */
void situationalApp_start(void);
/**
 * Stops the primary Rescume Application.  This method is NOT THREAD SAFE.
 */
void situationalApp_stop();

/**
 * Pushes WAVE messages into the application.  Receiving WAVE messages is currently the only item 
 * that is NOT encapulated inside of RESCUME's code.
 */
void situationalApp_pushWaveMessage(WaveRxPacket *packet);

#endif
