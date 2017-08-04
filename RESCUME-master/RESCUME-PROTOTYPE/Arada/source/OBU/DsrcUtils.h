/**
 * @file         DsrcUtils.h
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

#ifndef _DSRCUTILS_H_
#define _DSRCUTILS_H_

#include <EmergencyVehicleAlert.h>
#include <TravelerInformation.h>
#include <ShapePointSet.h>
#include "Vector.h"


struct DsrcMatchOptions {
	double headingAcceptance;
	double defaultLaneWidth;
};

struct DsrcPointToPathMatchResults {
	int onPath;
	double percentComplete;
	double pathLength;
	double distanceAlongPath;
	double percentOffCenter;
	Vector shortestVectToPath;
	int sideOfPath;
	int isPastPath;
	int isBeforePath;
};

extern const struct DsrcMatchOptions DSRC_MATCH_ALL;
extern const struct DsrcPointToPathMatchResults DSRC_POINT_TO_PATH_INIT;

#define DSRC_DEFAULT_LANE_WIDTH 3.0

/*struct DsrcPathToPathMatchResults {
	int isParrellel;
	double averageDistanceApart;
	double stdDevDistanceApart;
	int sideOfPath;
};*/

/************************ TO/FROM Structure Conversion *********************************/
long dsrc_convToStructLatitude(double latitude);
double dsrc_convFromStructLatitude(long latitude);

long dsrc_convToStructLongitude(double longitude);
double dsrc_convFromStructLongitude(long longitude);

uint32_t dsrc_convToStructHeading(double heading);
double dsrc_convFromStructHeading(double heading);

uint16_t dsrc_convToStructElevation(double altitude);
double dsrc_convFromStructElevation(uint16_t altitude);


/************************ TO/FROM Structure Helpers *********************************/
uint32_t dsrc_getEvaVehicleId32(EmergencyVehicleAlert_t *eva);

int dsrc_hasSameTimPacketId(TravelerInformation_t *tim1, TravelerInformation_t *tim2);

struct DsrcPointToPathMatchResults dsrc_matchPointToShapePointSet(ShapePointSet_t *shape, double latitude, double longitude, double elevation, struct DsrcMatchOptions options);
/*
 * Negative = 2ndary is left
 * Zero = Unknown
 * Positive = 2ndary is right
 */
int dsrc_compareParallelShapePointSets(ShapePointSet_t *baseShape, ShapePointSet_t *secondaryShape);

void dsrc_getBackOfShapePointSet(ShapePointSet_t *shape, double *outLatitude, double *outLongitude);

int dsrc_containsItisCode(ITIScodesAndText_t *list, long itisCode);


double dsrc_getBufferZoneImperial(double speed);

#endif
