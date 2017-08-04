#include "Map.h"
#include "GeoCoordConverter.h"
#include "QWarnDefs.h"
#include "ConfigFile.h"

#include <stdbool.h>
#include <float.h>

bool mapValidateRegion(ValidRegion_t* pRegion);
bool mapExtractRegionPointList(ValidRegion_t* pRegion, Offsets_t*** pppOffsets_out, int* pPointCount_out, struct Position3D** ppAnchor_out);

typedef struct MapOffsetCoord
{
    int16 xOffset_dm;
    int16 yOffset_dm;
    uint16 mileMarker_0_01mi;
    
}__attribute__ ((packed)) MapOffsetCoord;

void mapSwapRoutePositionResults(RoutePositionResults* pA, RoutePositionResults* pB)
{
    RoutePositionResults copy = *pB;
    *pB = *pA;
    *pA = copy;
    
    return;
}

void mapFindDistFromAtoBResults(Map* pMap, double* pDistance_m_out, RoutePositionResults* pAResults_out, RoutePositionResults* pBResults_out)
{
    RoutePositionResults aPosition = (*pAResults_out);
    RoutePositionResults bPosition = (*pBResults_out);
    double directionMultiplier = 0.0;
    double distance_m = 0.0;
    
    if(aPosition.intersectionIndex == bPosition.intersectionIndex)
    {
        if(aPosition.segmentIndex == bPosition.segmentIndex)
        {
            // Find distance between two vector positions
            Vector bsuba;
            vecSub(&aPosition.localPosition_m, &bPosition.localPosition_m, &bsuba);
            
            distance_m += vecMag(&bsuba);
            
            directionMultiplier = (vecMag(&(bPosition.localPosition_m)) > vecMag(&(aPosition.localPosition_m))) ? 1.0 : -1.0;
        }
        else // aPosition.segmentIndex != bPosition.segmentIndex
        {
            bool forward = (aPosition.segmentIndex < bPosition.segmentIndex);
            bool primary = (aPosition.approachType == PRIMARY);
            
            if(!forward)
            {
                //printf("Swapping start/end positions...\n");
                mapSwapRoutePositionResults(&aPosition, &bPosition);
            }
            
            RouteSegment* pApproach = 
                primary ? 
                pMap->pIntersections[aPosition.intersectionIndex].primaryApproach.pSegments : 
                pMap->pIntersections[aPosition.intersectionIndex].secondaryApproach.pSegments;
            
            // Sum all segment lengths between a and b
            int i;
            for(i=aPosition.segmentIndex + 1; i<bPosition.segmentIndex; i++)
            {
                distance_m += pApproach[i].length_m;
            }
            
            // Add remainder of a and b segments
            {
                distance_m += pApproach[aPosition.segmentIndex].length_m - vecMag(&(aPosition.localPosition_m));
                distance_m += vecMag(&(bPosition.localPosition_m));
            }
            
            directionMultiplier = forward ? 1.0 : -1.0;
        }
    }
    else // (startIntersectionIndex != endIntersectionIndex)
    {
        bool forward = (bPosition.intersectionIndex > aPosition.intersectionIndex);
        bool primary = (aPosition.approachType == PRIMARY);
        
        if(!forward)
        {
            //printf("Swapping start/end positions...\n");
            mapSwapRoutePositionResults(&aPosition, &bPosition);
        }
        
        int i;
        
        // Sum route segment distances for first intersection at start index
        {
            RouteNode* pStartIntersection = &pMap->pIntersections[aPosition.intersectionIndex];
        
            if(primary)
            {
                for(i=aPosition.segmentIndex + 1; i<pStartIntersection->primaryApproach.segmentCount; i++)
                {
                    distance_m += pStartIntersection->primaryApproach.pSegments[i].length_m;
                    
                    //printf("Map::findDistBtw: (%i) First Intersection[%i]=%f. Total = %f\n", aPosition.intersectionIndex, i, pStartIntersection->primaryApproach.pSegments[i].length_m, distance_m);
                }
                
                // Add remainder of current road segment
                RouteSegment* pStartSegment = &(pStartIntersection->primaryApproach.pSegments[aPosition.segmentIndex]);                
                distance_m += pStartSegment->length_m - vecMag(&(aPosition.localPosition_m));
                
                //printf("Map::findDistBtw: (%i) First Intersection[%i]=%f, Mag=%f, Total = %f\n", aPosition.intersectionIndex, aPosition.segmentIndex, pStartSegment->length_m, vecMag(&(aPosition.localPosition_m)), distance_m);
            }
            else // secondary
            {
                for(i=pStartIntersection->secondaryApproach.segmentCount - 1; i>aPosition.segmentIndex; i--)
                {                
                    distance_m += pStartIntersection->secondaryApproach.pSegments[i].length_m;
                    
                    //printf("Map::findDistBtw: (%i) First Intersection[%i]=%f. Total = %f\n", aPosition.intersectionIndex, i, pStartIntersection->secondaryApproach.pSegments[i].length_m, distance_m);
                }
                
                // Add remainder of current road segment
                RouteSegment* pStartSegment = &(pStartIntersection->secondaryApproach.pSegments[aPosition.segmentIndex]);                
                distance_m += pStartSegment->length_m - vecMag(&(aPosition.localPosition_m));
                
                //printf("Map::findDistBtw: (%i) First Intersection[%i]=%f, Mag=%f, Total = %f\n", aPosition.intersectionIndex, aPosition.segmentIndex, pStartSegment->length_m, vecMag(&(aPosition.localPosition_m)), distance_m);
            }
        }
        
        // Sum route segment distances for last intersection at end index
        {
            RouteNode* pEndIntersection = &(pMap->pIntersections[bPosition.intersectionIndex]);
        
            if(primary)
            {                
                for(i=0; i<bPosition.segmentIndex; i++)
                {
                    distance_m += pEndIntersection->primaryApproach.pSegments[i].length_m;
                    
                    //printf("Map::findDistBtw: (%i) Last Intersection.s[%i]=%f. Total = %f\n", bPosition.intersectionIndex, i, pEndIntersection->primaryApproach.pSegments[i].length_m, distance_m);
                }
                
                // Add remainder of current road segment
                RouteSegment* pEndSegment = &(pEndIntersection->primaryApproach.pSegments[bPosition.segmentIndex]);
                distance_m += vecMag(&(bPosition.localPosition_m));
                
                //printf("Map::findDistBtw: (%i) Last Intersection.s[%i]=%f, Mag=%f, Total = %f\n", bPosition.intersectionIndex, bPosition.segmentIndex, pEndSegment->length_m, vecMag(&(bPosition.localPosition_m)), distance_m);              
            }
            else // secondary
            {
                for(i=pEndIntersection->secondaryApproach.segmentCount - 1; i>bPosition.segmentIndex; i--)
                {
                    distance_m += pEndIntersection->secondaryApproach.pSegments[i].length_m;
                    
                    //printf("Map::findDistBtw: (%i) Last Intersection.s[%i]=%f. Total = %f\n", bPosition.intersectionIndex, i,  pEndIntersection->secondaryApproach.pSegments[i].length_m, distance_m);
                }
                
                // Add remainder of current road segment
                RouteSegment* pEndSegment = &(pEndIntersection->secondaryApproach.pSegments[bPosition.segmentIndex]);
                distance_m += pEndSegment->length_m - vecMag(&(bPosition.localPosition_m));
                
                //printf("Map::findDistBtw: (%i) Last Intersection.s[%i]=%f, Mag=%f, Total = %f\n", bPosition.intersectionIndex, bPosition.segmentIndex, pEndSegment->length_m, vecMag(&(bPosition.localPosition_m)), distance_m);
            }
        }
        
        //printf("%i, %i\n", aPosition.intersectionIndex + 1, bPosition.intersectionIndex);
        
        // Sum up intersection distances for each intersection between start and end index        
        for(i=aPosition.intersectionIndex + 1; i<bPosition.intersectionIndex; i++)
        {
            distance_m += (aPosition.approachType==PRIMARY) ? 
                pMap->pIntersections[i].primaryApproach.length_m :
                pMap->pIntersections[i].secondaryApproach.length_m;
                
            //printf("Map::findDistBtw: intersection[%i]=%f, Total = %f\n", i, pMap->pIntersections[i].primaryApproach.length_m, distance_m);
        }
    
        directionMultiplier = 
            forward ? // Later intersection, or previous?
                (primary ?  1.0 : -1.0) : // In later intersection;     On Primary or Secondary route?
                (primary ? -1.0 :  1.0);  // In previous intersection;  On Primary or Secondary route?
                
    }
    
    (*pDistance_m_out) = directionMultiplier * distance_m;
}

int mapFindDistFromAtoBMap(Map** ppMaps, int mapCount, PositionHeading* pA, PositionHeading* pB, double* pDistance_m_out, RoutePositionResults* pAResults_out, RoutePositionResults* pBResults_out)
{
    int aMapIndex = 0;
    if(!mapFindNearestMapRouteSegment(pA, ppMaps, mapCount, pAResults_out, &aMapIndex))
    {
        printf("Could not find map for point a!\n");
        return false;
    }
    
    int bMapIndex = 0;
    if(!mapFindNearestMapRouteSegment(pB, ppMaps, mapCount, pBResults_out, &bMapIndex))
    {
        printf("Could not find map for point b {%f, %f, %f}!\n", pB->position_m.x, pB->position_m.y, pB->heading_rad);
        return false;
    }
    
    //printf("aMapIndex = %i\n", aMapIndex);
    //printf("bMapIndex = %i\n", bMapIndex);
    
    if(aMapIndex != bMapIndex)
    {
        return false;
    }
    
    mapFindDistFromAtoBResults(ppMaps[aMapIndex], pDistance_m_out, pAResults_out, pBResults_out);
    
    return true;
}

int mapFindDistFromAtoB(Map* pMap, PositionHeading* pA, PositionHeading* pB, double* pDistance_m_out, RoutePositionResults* pAResults_out, RoutePositionResults* pBResults_out)
{    
    RoutePositionResults aPosition;    
    bool r1 = mapFindNearestIntersectionRouteSegment(
        pA,
        pMap->pIntersections,
        pMap->intersectionCount,
        &aPosition
    );
    
    *pAResults_out = aPosition;
    
    RoutePositionResults bPosition;
    bool r2 = mapFindNearestIntersectionRouteSegment(
        pB,
        pMap->pIntersections,
        pMap->intersectionCount,
        &bPosition
    );
    
    *pBResults_out = bPosition;
        
    if(!(r1 && r2))
    {
        return false; // Could not find route segment for a position
    }
    
    if(aPosition.approachType != bPosition.approachType)
    {
        return false; // Not on same side of road
    }
    
    mapFindDistFromAtoBResults(pMap, pDistance_m_out, &aPosition, &bPosition);
    
    return true;
}

int mapFindNearestMapRouteSegment(
    PositionHeading* pGlobalPosHead, 
    Map** ppMaps, 
    int mapCount, 
    RoutePositionResults* pResults_out,
    int* pMapIndex_out
)
{
    RoutePositionResults minResults;
    int minMapIndex = 0;

    mapFindNearestIntersectionRouteSegment(
        pGlobalPosHead,
        ppMaps[0]->pIntersections,
        ppMaps[0]->intersectionCount,
        &minResults
    );
    
    if(mapCount > 1)
    {
        RoutePositionResults currResults;
        int i;
        for(i=1; i<mapCount; i++)
        {
            mapFindNearestIntersectionRouteSegment(
                pGlobalPosHead,
                ppMaps[i]->pIntersections,
                ppMaps[i]->intersectionCount,
                &currResults
            );
            
            if(currResults.distance_m < minResults.distance_m)
            {
                minResults = currResults;
                minMapIndex = i;
            }
        }
    }
    
    (*pResults_out) = minResults;
    (*pMapIndex_out) = minMapIndex;
    
    return (minResults.intersectionIndex != -1);
}

int mapFindNearestIntersectionRouteSegment(
    PositionHeading* pGlobalPosHead, 
    RouteNode pIntersections[], 
    int count, 
    RoutePositionResults* pResults_out
)
{
    double minimumDist_m = DBL_MAX;
    Vector minLocalPos_m = {0};
    int minIntersectionIndex = -1;
    int minRouteSegmentIndex = -1;
    ApproachType type;

    int i;
    for(i=0; i<count; i++)
    {
        PositionHeading posHeadInRoute = *pGlobalPosHead;
        vecSub(pGlobalPosHead, &(pIntersections[i].primaryApproach.anchor_utm), &(posHeadInRoute.position_m));
        
        //printf("Map::mapFindNearestIntersectionRouteSegment: globalPos={%f, %f}\n", pGlobalPosHead->position_m.x, pGlobalPosHead->position_m.y);
        //printf("Map::mapFindNearestIntersectionRouteSegment: localPos={%f, %f}\n", posHeadInRoute.position_m.x, posHeadInRoute.position_m.y);
                
        double dist_m;
        Vector position;
        int routeSegmentIndex;

        if(mapFindNearestRouteSegment(
            &posHeadInRoute,
            pIntersections[i].primaryApproach.pSegments,
            pIntersections[i].primaryApproach.segmentCount,
            &dist_m,
            &position,
            &routeSegmentIndex
        ))
        {
            if(dist_m < minimumDist_m)
            {
                minimumDist_m = dist_m;
                minLocalPos_m = position;
                minRouteSegmentIndex = routeSegmentIndex;
                minIntersectionIndex = i;
                type = PRIMARY;
            }
        }
        
        vecSub(pGlobalPosHead, &(pIntersections[i].secondaryApproach.anchor_utm), &(posHeadInRoute.position_m));
        
        if(mapFindNearestRouteSegment(
            &posHeadInRoute,
            pIntersections[i].secondaryApproach.pSegments,
            pIntersections[i].secondaryApproach.segmentCount,
            &dist_m,
            &position,
            &routeSegmentIndex
        ))
        {
            if(dist_m < minimumDist_m)
            {
                minimumDist_m = dist_m;
                minLocalPos_m = position;
                minRouteSegmentIndex = routeSegmentIndex;
                minIntersectionIndex = i;
                type = SECONDARY;
            }
        }
    }
    
    pResults_out->distance_m = minimumDist_m;
    pResults_out->localPosition_m = minLocalPos_m;
    pResults_out->intersectionIndex = minIntersectionIndex;
    pResults_out->segmentIndex = minRouteSegmentIndex;
    pResults_out->approachType = type;
    
    return (minIntersectionIndex != -1);
}

int mapFindNearestRouteSegment(
    PositionHeading* pLocalPosHead, 
    RouteSegment pRoute[], 
    int count,
    double* minimumDist_m_out, 
    Vector* pMinimumLocalPosition_m_out, 
    int* pIndex_out
)
{
    //printf("Map::mapFindNearestRouteSegment(): Finding nearest route segment...\n");

    int i;
    double minimumDist_m = DBL_MAX;
    Vector minLocalPos_m = {0};
    int minIndex = -1;
    
    for(i=1; i<count; i++)
    {
        //printf("\tSegment %i to %i:\n", i-1, i);
    
        Vector roadVecA = pRoute[i-1].offset_m;
        Vector roadVecB = pRoute[i].offset_m;
        Vector roadVector;
        vecSub(&roadVecB, &roadVecA, &roadVector);
        
        //printf("\t\tRoadVector {%f, %f}\n", roadVector.x, roadVector.y);
        
        Vector vehicleHeading;
        mapHeadingToVector(pLocalPosHead->heading_rad, &vehicleHeading);
        
        //printf("\t\tVehicleHeading {%f, %f}\n", vehicleHeading.x, vehicleHeading.y);
        
        // If vehicle is not heading in the same direction as this segment,
        // throw out this result 
        if(vecDot(&vehicleHeading, &pRoute[i-1].direction) < cfGetConfigFile()->dirSimilarityTolerance)
        {
            //printf("\t\tVehicleHeading dot route direction < %f! Skipping.\n", cfGetConfigFile()->dirSimilarityTolerance);
            continue;
        }
        
        double roadVectorMag = vecMag(&roadVector);
        
        // Local position in Road-Vector frame
        Vector lpInRvs;
        vecSub(&pLocalPosHead->position_m, &roadVecA, &lpInRvs);
        
        //printf("\t\tLocal Pos in RoadVector pos {%f, %f}\n", lpInRvs.x, lpInRvs.y);
        
        Vector lpProjRoad;
        vecProjAontoB(&lpInRvs, &roadVector, &lpProjRoad);
        
        //printf("\t\tLocal Pos projected onto RoadVector {%f, %f}\n", lpProjRoad.x, lpProjRoad.y);
        
        double dist;
        Vector localMin;
        
        Vector lpTolpProjRoad;
        vecSub(&lpInRvs, &lpProjRoad, &lpTolpProjRoad);
        
        //printf("\t\tLocal Pos rejected from RoadVector {%f, %f}\n", lpTolpProjRoad.x, lpTolpProjRoad.y);
        
        double magLpToLpProjRoad = vecMag(&lpTolpProjRoad);
        double magLpProjRoad = vecMag(&lpProjRoad);
        
        if(vecDot(&roadVector, &lpInRvs) < 0.0) // projected behind vector
        {
            //printf("\t\tLocalPose behind RoadVector\n");
        
            dist = magLpToLpProjRoad + magLpProjRoad;
            localMin.x = 0.0;
            localMin.y = 0.0;
        }
        else if(vecMag(&lpProjRoad) > roadVectorMag) // projected ahead of vector
        {
            //printf("\t\tLocalPose past RoadVector\n");
        
            dist = magLpToLpProjRoad + (magLpProjRoad - roadVectorMag);
            localMin = roadVector;
        }
        else // projected onto vector
        {
            //printf("\t\tLocalPose in RoadVector\n");
        
            dist = magLpToLpProjRoad;
            localMin = lpProjRoad;
        }
        
        if(dist < cfGetConfigFile()->maximumRoadDistance && dist < minimumDist_m)
        {
            //printf("\t\tFound minimum distance %f!\n", dist);
        
            minimumDist_m = dist;
            minIndex = i-1;
            *pMinimumLocalPosition_m_out = localMin;
        }
        
        *minimumDist_m_out = minimumDist_m;
        *pIndex_out = minIndex;
    }
    
    return (minIndex != -1);
}

int mapFindPositionAlongApproach(
    PositionHeading* pGlobalPosition,
    RouteApproach* pApproach,
    double* completion_pct_out
)
{
    Vector localPosition;
    vecSub(&(pGlobalPosition->position_m), &(pApproach->anchor_utm), &localPosition);
    
    PositionHeading localPosHead = { localPosition, pGlobalPosition->heading_rad };
    double dist_m;
    Vector snappedPosition;
    int segmentIndex;
    
    if(!mapFindNearestRouteSegment(
        &localPosHead, 
        pApproach->pSegments, 
        pApproach->segmentCount,
        &dist_m, 
        &snappedPosition, 
        &segmentIndex
    ))
    {
        printf("Map::mapFindPositionAlongApproach: Could not snap current position to approach!\n");
        return false;
    }
    
    double distanceToEnd_m = 0.0;
    
    // Find total distance to end of region
    {    
        // Sum intermediate segments
        int i;        
        for(i=segmentIndex+1; i<pApproach->segmentCount; i++)
        {
            distanceToEnd_m += pApproach->pSegments[i].length_m;
            
            //printf("Computing Distance To End: Segment %i; seg=%f, total=%f\n", i, pApproach->pSegments[i].length_m, distanceToEnd_m); 
        }
        
        // Add remainder of current segment
        Vector posInSegFrame;    
        vecSub(&snappedPosition, &pApproach->pSegments[segmentIndex].offset_m, &posInSegFrame);        
        distanceToEnd_m += pApproach->pSegments[segmentIndex].length_m - vecMag(&snappedPosition); 
        
        //printf("Computing Distance: First segment %i; seg=%f, t=%f, total=%f\n", segmentIndex, pApproach->pSegments[segmentIndex].length_m, vecMag(&snappedPosition), distanceToEnd_m);   
    }
    
    double completion_pct = ((pApproach->length_m - distanceToEnd_m) / pApproach->length_m);
    if(completion_pct > 1.0)
    {
        completion_pct = 1.0;
    }
    else if(completion_pct < 0.0)
    {
        completion_pct = 0.0;
    }
    
    (*completion_pct_out) = completion_pct;
    
    return true;
}

void mapHeadingToVector(double heading_rad, Vector* pHeadingVector_out)
{
    heading_rad -= PI_2; // Headings are 0 rad North. Need to remap to PI/2 rad North.
    pHeadingVector_out->x = cos(heading_rad);
    pHeadingVector_out->y = -sin(heading_rad);
    
    return;
}

int mapMapDataToMap(MapData_t* pMessage, Map* pMap_out)
{
    GeoCoordConverter* pGeoConv = geoConvCreate();
    
    int intersectionCount = pMessage->intersections->list.count;
    
    printf("Roadway Id: %s\n", pMessage->name->buf);
    
    pMap_out->intersectionCount = intersectionCount;
    pMap_out->pIntersections = (RouteNode*)malloc(intersectionCount * sizeof(RouteNode));
    strcpy(pMap_out->roadwayId, pMessage->name->buf);
    
    int i, j, k;
    char zone = 0, band = 0;
    double easting_m = 0.0, northing_m = 0.0;
    
    for(i=0; i<intersectionCount; i++)
    {    
        const int NUM_APPROACHES = 2;
        
        struct Intersection* pIntersection = pMessage->intersections->list.array[i];
        int approachCount = pIntersection->approaches.list.count;
        
        double lat_rad = (pIntersection->refPoint->lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD;
        double lon_rad = (pIntersection->refPoint->Long / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD;
        
        if(!geoConvGeodetic2Utm(
            pGeoConv, 
            lat_rad, lon_rad,
            //lon_rad, lat_rad, // !Temporary! The current map messages have lat/long swapped
            &zone, &band,
            &easting_m, &northing_m
        ))
        {
            printf("Could not calculate UTM for intersection %i!\n", i);
        }
        
        pMap_out->pIntersections[i].utmPosition_m.x = easting_m;
        pMap_out->pIntersections[i].utmPosition_m.y = northing_m;        
        
        if(approachCount != NUM_APPROACHES)
        {
            printf("Map: Invalid number of approaches (%i) in s\n", approachCount);
            break;
        }
        
        for(j=0; j<NUM_APPROACHES; j++)
        {
            struct ApproachObject* pApproach = pIntersection->approaches.list.array[j];
            struct VehicleReferenceLane* pDrivingLane = pApproach->approach->drivingLanes->list.array[0];              
            struct Position3D approachPos = *(pApproach->refPoint);
            int nodeCount = pDrivingLane->nodeList.list.count;
            
            lat_rad = (approachPos.lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD;
            lon_rad = (approachPos.Long/ BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD;
            
            if(!geoConvGeodetic2Utm(
                pGeoConv, 
                lat_rad, lon_rad, 
                //lon_rad, lat_rad, // !Temporary! Current map messages have lat/long swapped
                &zone, &band,
                &easting_m, &northing_m
            ))
            {
                printf("Could not calculate UTM for approach %i!\n", j);
            }
            
            printf(
                "Intersection %i, Approach %i - %f, %f, %f, %f, %c%c\n", 
                i, j,
                approachPos.lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG, approachPos.Long / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG,
                easting_m, northing_m,
                zone, band
            );
            
            
            RouteApproach* pRouteApproach = 
                (j==0) ? 
                &(pMap_out->pIntersections[i].primaryApproach) :
                &(pMap_out->pIntersections[i].secondaryApproach);
            
            pRouteApproach->anchor_utm.x = easting_m;
            pRouteApproach->anchor_utm.y = northing_m;
            pRouteApproach->segmentCount = nodeCount;
            pRouteApproach->pSegments = (RouteSegment*)malloc(nodeCount * sizeof(RouteSegment));
            pRouteApproach->length_m = 0.0; // Override
            
            for(k=0; k<nodeCount; k++)
            {
                MapOffsetCoord* pOffset = (MapOffsetCoord*)(pDrivingLane->nodeList.list.array[k]->buf);                
                
                printf(
                    "\tNode %i: [%f, %f, %f]\n", 
                    k, 
                    pOffset->xOffset_dm * 0.1,
                    pOffset->yOffset_dm * 0.1,
                    pOffset->mileMarker_0_01mi * 0.1
                );
                
                RouteSegment* pSegment = &(pRouteApproach->pSegments[k]);
                pSegment->offset_m.x = pOffset->xOffset_dm * 0.1;
                pSegment->offset_m.y = pOffset->yOffset_dm * 0.1;
                pSegment->mileMarker = pOffset->mileMarker_0_01mi * 0.01;
                pSegment->direction.x = 0.0;
                pSegment->direction.y = 0.0;
                pSegment->length_m = 0.0;
            }
            
            double approachLength_m = 0.0;
            for(k=0; k<nodeCount-1; k++)
            {
                RouteSegment* pSegmentA = &(pRouteApproach->pSegments[k]);
                RouteSegment* pSegmentB = &(pRouteApproach->pSegments[k+1]);         
                
                Vector aToB;
                vecSub(&(pSegmentB->offset_m), &(pSegmentA->offset_m), &aToB);
                
                Vector dir;
                vecNormalize(&aToB, &dir);
                
                pSegmentA->direction = dir;
                pSegmentA->length_m = vecMag(&aToB);
                
                approachLength_m += pSegmentA->length_m;
            }
            
            pRouteApproach->length_m = approachLength_m;
        }
    }
    
    geoConvDestroy(pGeoConv);
    
    return true;
}

int mapRegionListToRouteApproach(ValidRegion_t** ppRegions, int regionCount, RouteApproach* pApproach_out)
{
    if(regionCount < 1)
    {
        printf("Map::mapRegionListToRouteApproach: No Regions to process.\n");
        
        return false;
    }
    
    int i, j, n;
    char zone, band;
    double easting_m, northing_m;   
    
    // Initialize RouteApproach segment array
    {
        if(!mapValidateRegion(ppRegions[0]))
        {
            printf("Map::mapRegionListToRouteApproach: Region[0] is invalid\n");
            
            return false;
        }
    
        pApproach_out->segmentCount = 0;
        
        for(i=0; i<regionCount; i++)
        {        
            pApproach_out->segmentCount += ppRegions[i]->area.choice.shapePointSet.nodeList.list.count;
        }    
        
        pApproach_out->pSegments = (RouteSegment*)malloc(pApproach_out->segmentCount * sizeof(RouteSegment));
    }
    
    GeoCoordConverter* pGeoConv = geoConvCreate();
    
    // Compute approach root anchor
    {
        int pointCount;
        RegionOffsets_t** ppOffsets;
        struct Position3D* pRootRegionAnchor;
        if(!mapExtractRegionPointList(ppRegions[0], &ppOffsets, &pointCount, &pRootRegionAnchor))
        {
            printf("Map::mapRegionListToRouteApproach: Region[%i] - Could not extract region data.\n", 0);
            
            geoConvDestroy(pGeoConv);
            
            return false;
        }

        geoConvGeodetic2Utm(
            pGeoConv, 
            (pRootRegionAnchor->lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD, (pRootRegionAnchor->Long / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD, 
            &zone, &band,
            &easting_m, &northing_m
        );
        
        pApproach_out->anchor_utm.x = easting_m;
        pApproach_out->anchor_utm.y = northing_m;
    }
    
    // Transform all points from all regions into approach root-anchor frame
    {
        for(i=0, n=0; i<regionCount; i++)
        {
            if(!mapValidateRegion(ppRegions[i]))
            {
                printf("Map::mapRegionListToRouteApproach: Region[%i] is invalid\n", i);
                
                geoConvDestroy(pGeoConv);
                
                return false;
            }
            
            int pointCount = ppRegions[i]->area.choice.shapePointSet.nodeList.list.count;
            struct Position3D* pRootRegionAnchor = ppRegions[i]->area.choice.shapePointSet.anchor;
            
            Vector regionAnchor_utm;
            geoConvGeodetic2Utm(
                pGeoConv, 
                (pRootRegionAnchor->lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD, (pRootRegionAnchor->Long / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG) * DEG2RAD, 
                &zone, &band,
                &regionAnchor_utm.x, &regionAnchor_utm.y
            );
            
            for(j=0; j<pointCount; j++, n++)
            {
                RouteSegment* pSegment = &(pApproach_out->pSegments[n]);
                
                MapOffsetCoord* pOffset = (MapOffsetCoord*)ppRegions[i]->area.choice.shapePointSet.nodeList.list.array[j]->buf;
                
                Vector a = {pOffset->xOffset_dm * 0.1, pOffset->yOffset_dm * 0.1};
                
                Vector a1;
                vecAdd(&a, &regionAnchor_utm, &a1);
                vecSub(&a1, &(pApproach_out->anchor_utm), &a);
                
                pSegment->offset_m = a;
                pSegment->mileMarker = pOffset->mileMarker_0_01mi * 0.01;
            }
        }
    }
    
    // Compute length and direction of segments and total length
    {
        pApproach_out->length_m = 0.0;
    
        for(i=0; i<pApproach_out->segmentCount-1; i++)
        {    
            Vector a = pApproach_out->pSegments[i].offset_m;
            Vector b = pApproach_out->pSegments[i+1].offset_m;
            
            Vector bSubA, dir;
            vecSub(&b, &a, &bSubA);
            
            double length_m = vecMag(&bSubA);
            
            if(length_m != 0.0)
            {
                vecNormalize(&bSubA, &dir);
            }
            else // length_m == 0.0
            {
                if(i != 0)
                {
                    dir = pApproach_out->pSegments[i-1].direction;
                }
                else // First node
                {
                    dir.x = 0.0;
                    dir.y = 1.0;
                }
            }
            
            pApproach_out->pSegments[i].length_m = length_m;
            pApproach_out->pSegments[i].direction = dir;
            
            pApproach_out->length_m += length_m;
        }
        
        Vector direction = {0.0, 0.0};
        pApproach_out->pSegments[pApproach_out->segmentCount-1].direction = direction;
        pApproach_out->pSegments[pApproach_out->segmentCount-1].length_m = 0.0;
    }
    
    /*
    printf("Extracted approach from TIM:\n");
    for(i=0; i<pApproach_out->segmentCount; i++)
    {
        Vector p = pApproach_out->pSegments[i].offset_m;
        Vector d = pApproach_out->pSegments[i].direction;
        double mm = pApproach_out->pSegments[i].mileMarker;
        double len = pApproach_out->pSegments[i].length_m;
        printf("%i { %f, %f }-{%f, %f}-%f m-%f\n", i, p.x, p.y, d.x, d.y, len, mm);
    }
    */
    
    geoConvDestroy(pGeoConv);
    
    return true;
}

void mapDestroy(Map* pMap)
{
    int i;
    for(i=0; i<pMap->intersectionCount; i++)
    {
        RouteNode* pNode = &(pMap->pIntersections[i]);        
        free(pNode->primaryApproach.pSegments);
        free(pNode->secondaryApproach.pSegments);
    }
    
    free(pMap->pIntersections);
    free(pMap);    
    
    return;
}

bool mapValidateRegion(ValidRegion_t* pRegion)
{
    if(pRegion->area.present != ValidRegion__area_PR_shapePointSet )
    {
        printf("Map::validateRegion: Malformed Region - Region %i is not using a ValidRegion__area_PR_shapePointSet. Using %i\n", pRegion, pRegion->area.present);
        return false;
    }
    
    int pointCount = pRegion->area.choice.shapePointSet.nodeList.list.count;

    if(pointCount < 2)
    {
        printf("Map::validateRegion: Malformed Region - Region %i does not sufficient number of points! Has %i. Aborting...", pRegion, pointCount);
        
        return false;
    }
      
    if(pRegion->area.choice.regionPointSet.anchor == NULL)
    {
        printf("Map::validateRegion: Malformed Region - Region %i does not have an anchor! Aborting...", pRegion);
        
        return false;
    }
    
    return true;
}

bool mapExtractRegionPointList(ValidRegion_t* pRegion, Offsets_t*** pppOffsets_out, int* pPointCount_out, struct Position3D** ppAnchor_out)
{
    if(pRegion->area.present == ValidRegion__area_PR_regionPointSet)
    {
        *(pPointCount_out) = pRegion->area.choice.regionPointSet.nodeList.list.count;            
        *(pppOffsets_out) = pRegion->area.choice.regionPointSet.nodeList.list.array;            
        *(ppAnchor_out) = pRegion->area.choice.regionPointSet.anchor;
        
        return true;
    }
    else if(pRegion->area.present == ValidRegion__area_PR_shapePointSet)
    {
        *(pPointCount_out) = pRegion->area.choice.shapePointSet.nodeList.list.count;            
        *(pppOffsets_out) = pRegion->area.choice.shapePointSet.nodeList.list.array;            
        *(ppAnchor_out) = pRegion->area.choice.shapePointSet.anchor;
        
        return true;
    }
    else
    {
        return false;
    }    
}

void mapPrintApproach(RouteApproach* pApproach)
{
    printf("Origin: { %f, %f }\n", pApproach->anchor_utm.x, pApproach->anchor_utm.y);

    int i;
    for(i=0; i<pApproach->segmentCount; i++)
    {
        printf("%i: [%f, %f]\n", i, pApproach->pSegments[i].offset_m.x, pApproach->pSegments[i].offset_m.y);
    }
}
