/**
 * @file         DsrcUtils.c
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

#include "DsrcUtils.h"
#include <assert.h>
#include <wave.h>
#include <math.h>

#ifndef GPS_INVALID_DATA
 #error GPS_INVALID_DATA is not defined...
#endif

#define DSRC_POLAR_RADIUS 6356752.314247833
#define DSRC_EQUATORIAL_RADIUS 6378137
#define PI 3.141592653589793238462643383
#define PI_2 1.57079632679

#define LATLONG_MULT 10000000.0
#define LANEWIDTH_MULT 100.0
#define OFFSET_MULT 10.0

#define NDSRC_DEBUG

#if defined(NDSRC_DEBUG)
	#define DSRC_DEBUG(_x) ((void)0)
 	#define dsrc_assert(_x) (assert(_x))
#else
	#define DSRC_DEBUG(_x) (printf("(%s, %d) [DSRC] | ", __FILE__, __LINE__), (void)(_x))
 	#define dsrc_assert(_x) (assert(_x))
#endif

const struct DsrcMatchOptions DSRC_MATCH_ALL = { .headingAcceptance = -2.0, .defaultLaneWidth = DSRC_DEFAULT_LANE_WIDTH };
const struct DsrcPointToPathMatchResults DSRC_POINT_TO_PATH_INIT = { .onPath = 0, .percentOffCenter = 1001.00, .isPastPath = 1, .isBeforePath = 1};

//TODO: take this out of this file?
void dsrc_createVectorFromHeading(double heading_deg, Vector* out)
{
	heading_deg *= PI / 180.0;
	heading_deg -= PI_2; // Headings are 0 rad North. Need to remap to PI/2 rad North.
	out->x = cos(heading_deg);
	out->y = -sin(heading_deg);
	
	return;
}

int dsrc_createVectorFromOffset(Offsets_t *offset, Vector *out)
{
	if (offset->size < 4)
		return 0;
	
	out->x = *((int16_t*)(&offset->buf[0])) / OFFSET_MULT;
	out->y = *((int16_t*)(&offset->buf[2])) / OFFSET_MULT;

	return 1;
}

void dsrc_createVectorFromOffsets(Offsets_t* start, Offsets_t* end, Vector* out)
{
	if (start->size < 4)
		return;
	if (end->size < 4)
		return;
	
	double x1 = *((int16_t*)(&start->buf[0])) / OFFSET_MULT;
	double y1 = *((int16_t*)(&start->buf[2])) / OFFSET_MULT;

	double x2 = *((int16_t*)(&end->buf[0])) / OFFSET_MULT;
	double y2 = *((int16_t*)(&end->buf[2])) / OFFSET_MULT;

	out->x = x2 - x1;
	out->y = y2 - y1;

	return;
}


















long dsrc_convToStructLatitude(double latitude)
{
	if (latitude == GPS_INVALID_DATA)
		return 900000001;
	return latitude * 10000000;
}
double dsrc_convFromStructLatitude(long latitude)
{
	if (latitude == 900000001)
		return GPS_INVALID_DATA;
	return latitude / 10000000.0;
}

long dsrc_convToStructLongitude(double longitude)
{
	if (longitude == GPS_INVALID_DATA)
		return 1800000001;
	else
		return longitude * 10000000;
}
double dsrc_convFromStructLongitude(long latitude)
{
	if (latitude == 1800000001)
		return GPS_INVALID_DATA;
	return latitude / 10000000.0;
}

uint32_t dsrc_convToStructHeading(double heading)
{
	if (heading == GPS_INVALID_DATA)
		return 28800;
	else
		return heading * 80;
}
double dsrc_convFromStructHeading(double heading)
{
	if (heading == 28800)
		return GPS_INVALID_DATA;
	return heading / 80.0;
}

uint16_t dsrc_convToStructElevation(double altitude)
{
	if (altitude >= 0 && altitude <= 6143.9) {
		return altitude * 10;
	} else if (altitude > -409.5 && altitude < 0) {
		int value = altitude * 10;
		value = 0xFFFF + value;
		return (uint16_t)(value);
	}
	//Invalid...
	return 0xF000;
}
double dsrc_convFromStructElevation(uint16_t altitude)
{
	if (altitude == 0xF000)
		return GPS_INVALID_DATA;
	else if (altitude > 0xF000)
		return (((int)altitude) - 0xFFFF) / 10.0;
	else
		return altitude / 10.0;
}

uint32_t dsrc_getEvaVehicleId32(EmergencyVehicleAlert_t *eva)
{
	if (!eva)
		return 0;
	if (!eva->id)
		return 0;

	if (eva->id->size == 1)
		return *((uint8_t *)eva->id->buf);
	if (eva->id->size == 2)
		return *((uint16_t *)eva->id->buf);
	if (eva->id->size >= 4)
		return *((uint32_t *)eva->id->buf);

	return 0;
}

int dsrc_hasSameTimPacketId(TravelerInformation_t *tim1, TravelerInformation_t *tim2)
{
	assert(tim1 != NULL);
	assert(tim2 != NULL);

	int results = 0;

	if (tim1->packetID && tim2->packetID && tim1->packetID->size == tim2->packetID->size)
	{
		int i;
		for(i = 0; i < tim1->packetID->size; i++)
		{
			if (tim1->packetID->buf[i] != tim2->packetID->buf[i])
			{
				results = 1;
				break;
			}
		}
	}
	else
	{
		results = 1;
	}

	return results;
}

/*
 * @requires:
 *		shape != null
 *		shape->anchor != null
 *		shape->nodeList.list.size > 1
 */
struct DsrcPointToPathMatchResults dsrc_matchPointToShapePointSet(ShapePointSet_t *shape, double latitude, double longitude, double heading, struct DsrcMatchOptions options)
{
	struct DsrcPointToPathMatchResults results = DSRC_POINT_TO_PATH_INIT;

	dsrc_assert(shape != NULL);
	dsrc_assert(shape->anchor != NULL);
	dsrc_assert(shape->nodeList.list.count > 1);
	/*if (!shape)
	{
		DSRC_DEBUG(printf("ERROR: ShapePointSet argument is null\n"));
		results.hasError = 1;
		return results;
	}
	if (!shape->anchor)
	{
		DSRC_DEBUG(printf("ERROR: ShapePointSet doesn't have an anchor point\n"));
		results.hasError = 1;
		return results;
	}
	if (shape->nodeList.list.size <= 1)
	{
		DSRC_DEBUG(printf("ERROR: ShapePointSet \n"));
		results.hasError = 1;
		return results;
	}*/

	double laneWidth;
	if (shape->laneWidth)
		laneWidth = *shape->laneWidth / LANEWIDTH_MULT;
	else
		laneWidth = options.defaultLaneWidth;
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Lane Width: %0.2f\n", laneWidth));

	double baseLatitude = dsrc_convFromStructLatitude(shape->anchor->lat);
	double baseLongitude = dsrc_convFromStructLongitude(shape->anchor->Long);

	double xOffset = cos(latitude * PI / 180) * 2 * PI * DSRC_EQUATORIAL_RADIUS * (longitude - baseLongitude) / 360.0;
	double yOffset = 2 * PI * DSRC_POLAR_RADIUS * (latitude - baseLatitude) / 360.0;
	Vector vehVect = { .x = xOffset, .y = yOffset };
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Vehicle's offset (%.4f, %.4f)\n", xOffset, yOffset));

	Vector vehHeadingVect;
	dsrc_createVectorFromHeading(heading, &vehHeadingVect);

	results.percentOffCenter = 1001;
	//TODO add directionality
	int i;
	for(i = 0; i < shape->nodeList.list.count - 1; i++)
	{
		DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Working segment between node %d and %d.\n", i, i + 1));

		Vector pointAVect, pointBVect, segmentVect;
		dsrc_createVectorFromOffset(shape->nodeList.list.array[i], &pointAVect);  //TODO: should add error checking to this calculation.
		dsrc_createVectorFromOffset(shape->nodeList.list.array[i + 1], &pointBVect);  //TODO: should add error checking to this calculation.
		vecSub(&pointBVect, &pointAVect, &segmentVect);

		double segmentLength = vecMag(&segmentVect);
		if (segmentLength <= 0)
		{
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():(ERROR) Segment length is zero\n"));
			continue;
		}

		DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Segment length is %0.2f.\n", segmentLength));
		results.pathLength += segmentLength;

		Vector segmentNormalizedVect;
		vecNormalize(&segmentVect, &segmentNormalizedVect);

		double dotProdHeadingWithSegment = vecDot(&segmentNormalizedVect, &vehHeadingVect);
		if (dotProdHeadingWithSegment < options.headingAcceptance)
		{
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():(MATCH FAILURE) Dot product of heading and path is %0.5f\n", dotProdHeadingWithSegment));
			continue;
		}
		DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Dot product of heading and path is %0.5f\n", dotProdHeadingWithSegment));

		Vector pointAToVehVect, pointAToVehProjOntoSegmentVect;
		vecSub(&vehVect, &pointAVect, &pointAToVehVect);
		vecProjAontoB(&pointAToVehVect, &segmentVect, &pointAToVehProjOntoSegmentVect);

		double pointAToVehProjOntoSegmentLength = vecMag(&pointAToVehProjOntoSegmentVect);

		if (vecDot(&pointAToVehVect, &segmentVect) < 0)
		{
			//Vehicle is before the segment.
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():(MATCH FAILURE) Vehicle is before the segment... moving on.\n"));
			results.isPastPath = 0;
			continue;
		}
		else if (pointAToVehProjOntoSegmentLength > segmentLength + laneWidth / 2.0)
		{
			//Vehicle is past the segment.
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():(MATCH FAILURE) Vehicle is past the segment... moving on.\n"));
			results.isBeforePath = 0;
			continue;
		}
		else
		{
			results.isPastPath = 0;
			results.isBeforePath = 0;

			Vector vehToSegmentVect;
			vecSub(&pointAToVehProjOntoSegmentVect, &pointAToVehVect, &vehToSegmentVect);

			double distanceToSegment = vecMag(&vehToSegmentVect);
			double percentOffCenter = distanceToSegment / (laneWidth / 2.0);

			DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Vehicle is within segment...\n"));
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Distance along segment = %.1f\n", pointAToVehProjOntoSegmentLength));
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Distance to center of lane = %0.2f\n", distanceToSegment));
			DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Percent off center = %.1f\n", percentOffCenter * 100.0));

			//Better match then prior... So save this one.
			if (percentOffCenter < results.percentOffCenter)
			{
				DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Current segment is better match, updating results.\n"));
				results.percentOffCenter = percentOffCenter;
				results.distanceAlongPath = results.pathLength - segmentLength + pointAToVehProjOntoSegmentLength;
				results.shortestVectToPath = vehToSegmentVect;

				double vecCrossResult = vecCross(&pointAVect, &pointBVect, &vehVect);
				results.sideOfPath = vecCrossResult > 0.0 ? -1 : vecCrossResult < 0.0 ? 1 : 0;
			}
		}

		//TODO:...?

	}

	results.onPath = results.percentOffCenter <= 1.0;
	results.percentComplete = results.distanceAlongPath / results.pathLength;
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet(): Completed...\n"));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> On Path = %s\n", results.onPath ? "Yes" : "No"));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Percent Complete = %.2f%%\n", results.percentComplete * 100));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Path Length = %.2f meters\n", results.pathLength));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Distance Along Path = %.2f meters\n", results.distanceAlongPath));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Percent Off Center = %.2f%%\n", results.percentOffCenter * 100));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Shortest Vector to Path = (%0.2f, %0.2f)\n", results.shortestVectToPath.x, results.shortestVectToPath.y));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Side of Path = %s\n\n", results.sideOfPath > 0 ? "Right" : results.sideOfPath < 0 ? "Left" : "Center"));
	DSRC_DEBUG(printf("dsrc_matchToShapePointSet():       -> Is Past Path = %s\n\n", results.isPastPath > 0 ? "True" : "False"));


	return results;
}

//TODO: This is very very rough and needs more testing.
int dsrc_compareParallelShapePointSets(ShapePointSet_t *baseShape, ShapePointSet_t *secondaryShape)
{
	dsrc_assert(baseShape != NULL);
	dsrc_assert(baseShape->anchor != NULL);
	dsrc_assert(baseShape->nodeList.list.count > 1);
	dsrc_assert(secondaryShape != NULL);
	dsrc_assert(secondaryShape->anchor != NULL);
	dsrc_assert(secondaryShape->nodeList.list.count > 1);

	double baseLatitude = dsrc_convFromStructLatitude(baseShape->anchor->lat);
	double baseLongitude = dsrc_convFromStructLongitude(baseShape->anchor->Long);
	double secondaryLatitude = dsrc_convFromStructLatitude(secondaryShape->anchor->lat);
	double secondaryLongitude = dsrc_convFromStructLongitude(secondaryShape->anchor->Long);

	double xOffset = cos(baseLatitude * PI / 180) * 2 * PI * DSRC_EQUATORIAL_RADIUS * (secondaryLongitude - baseLongitude) / 360.0;
	double yOffset = 2 * PI * DSRC_POLAR_RADIUS * (secondaryLatitude - baseLatitude) / 360.0;
	Vector secondaryPathBaseVect = { .x = xOffset, .y = yOffset };
	Vector secondaryVect;
	dsrc_createVectorFromOffset(secondaryShape->nodeList.list.array[0], &secondaryVect);
	vecAdd(&secondaryPathBaseVect, &secondaryVect, &secondaryVect);

	DSRC_DEBUG(printf("dsrc_compareParrellelShapePointSets(): Secondary Path's First Node's offset (%.4f, %.4f)\n", secondaryVect.x, secondaryVect.y));

	//TODO add directionality
	int i;
	for(i = 0; i < baseShape->nodeList.list.count - 1; i++)
	{
		DSRC_DEBUG(printf("dsrc_compareParrellelShapePointSets(): Working segment between node %d and %d.\n", i, i + 1));

		Vector pointAVect, pointBVect, segmentVect;
		dsrc_createVectorFromOffset(baseShape->nodeList.list.array[i], &pointAVect);  //TODO: should add error checking to this calculation.
		dsrc_createVectorFromOffset(baseShape->nodeList.list.array[i + 1], &pointBVect);  //TODO: should add error checking to this calculation.
		vecSub(&pointBVect, &pointAVect, &segmentVect);

		Vector pointAToSecondaryVect, pointAToSecondaryProjOntoSegmentVect;
		vecSub(&secondaryVect, &pointAVect, &pointAToSecondaryVect);
		vecProjAontoB(&pointAToSecondaryVect, &segmentVect, &pointAToSecondaryProjOntoSegmentVect);

		double pointAToSecondaryProjOntoSegmentLength = vecMag(&pointAToSecondaryProjOntoSegmentVect);

		double segmentLength = vecMag(&segmentVect);

		if (vecDot(&pointAToSecondaryVect, &segmentVect) < 0)
		{
			DSRC_DEBUG(printf("dsrc_compareParrellelShapePointSets():(MATCH FAILURE) Secondary is before the segment... Swapping roles.\n"));
			return -dsrc_compareParallelShapePointSets(secondaryShape, baseShape);
		}
		else if (pointAToSecondaryProjOntoSegmentLength > segmentLength + 3.0)
		{
			DSRC_DEBUG(printf("dsrc_compareParrellelShapePointSets():(MATCH FAILURE) Secondary is past the segment... moving on.\n"));
			continue;
		}
		else
		{
			double vecCrossResult = vecCross(&pointAVect, &pointBVect, &secondaryVect);
			DSRC_DEBUG(printf("dsrc_compareParrellelShapePointSets(): Results = %f.\n", vecCrossResult));
			return vecCrossResult > 0.0 ? -1 : vecCrossResult < 0.0 ? 1 : 0;
		}
	}

	return 0;
}

void dsrc_getBackOfShapePointSet(ShapePointSet_t *shape, double *outLatitude, double *outLongitude)
{
	dsrc_assert(shape != NULL);
	dsrc_assert(shape->anchor != NULL);
	dsrc_assert(outLatitude != NULL);
	dsrc_assert(outLongitude != NULL);

	double baseLatitude = dsrc_convFromStructLatitude(shape->anchor->lat);
	double baseLongitude = dsrc_convFromStructLongitude(shape->anchor->Long);

	double latOffset = 0;
	double longOffset = 0;

	if (shape->nodeList.list.count > 0)
	{
		Vector firstNode;
		dsrc_createVectorFromOffset(shape->nodeList.list.array[0], &firstNode);
		double xOffset = firstNode.x;
		double yOffset = firstNode.y;

		latOffset = yOffset * 360.0 / (2 * PI * DSRC_POLAR_RADIUS);
		longOffset = xOffset * 360.0 / (2 * PI * DSRC_EQUATORIAL_RADIUS * cos(baseLatitude * PI / 180));
	}

	*outLongitude = baseLongitude + longOffset;
	*outLatitude = baseLatitude + latOffset;
	DSRC_DEBUG(printf("dsrc_getBackOfShapePointSet(): Base Latitude: %.8f, Offset %.8f, New Latitude %.8f\n", baseLatitude, latOffset, *outLatitude));
	DSRC_DEBUG(printf("dsrc_getBackOfShapePointSet(): Base Longitude: %.8f, Offset %.8f, New Longitude %.8f\n", baseLongitude, longOffset, *outLongitude));
}

int dsrc_containsItisCode(ITIScodesAndText_t *itisCodesAndText, long itisCode)
{
	int results = 0;

	int i;
	for(i = 0; i < itisCodesAndText->list.count; i++)
	{
		if (itisCodesAndText->list.array[i]->item.present != item_PR_itis_it)
			continue;
		results |= itisCodesAndText->list.array[i]->item.choice.itis == itisCode;
	}

	return results;
}


double dsrc_getBufferZoneImperial(double speed)
{
	return 0.0977 * speed * speed + 3.5015 * speed + 6.6309;
}
