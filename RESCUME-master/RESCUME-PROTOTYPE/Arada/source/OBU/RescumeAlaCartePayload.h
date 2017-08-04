/**
 * @file         RescumeAlaCartePayload.h
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


#ifndef _RESCUMEALACARTEPAYLOAD_H_
#define _RESCUMEALACARTEPAYLOAD_H_

typedef enum {
	RescumeThreatLevel_reset = -1, 
	RescumeThreatLevel_noThreat, 
	RescumeThreatLevel_approachingViolation, 
	RescumeThreatLevel_inViolation, 
	RescumeThreatLevel_collision, 
	RescumeThreatLevel__length
} RescumeThreatLevel;

typedef struct RescumeAlaCartePayload {
	uint8_t oncomingId[4];
	uint8_t responderId[4];
	uint16_t secMark;
	int tCategory;
	RescumeThreatLevel tLevel;
}__attribute__ ((packed)) RescumeAlaCartePayload;

#endif
