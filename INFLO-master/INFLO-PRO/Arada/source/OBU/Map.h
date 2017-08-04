#ifndef OBU_Map_h
#define OBU_Map_h

#include "Vector.h"
#include "MapData.h"
#include "ValidRegion.h"
#include "VehicleStateMap.h"

typedef enum ApproachType
{
    PRIMARY,
    SECONDARY

} ApproachType;

typedef struct PositionHeading
{
    Vector position_m;
    double heading_rad;

} PositionHeading;

typedef struct RoutePositionResults
{
    double distance_m;          // Distance from the input position to the area of minimum distance on the route node
    Vector localPosition_m;     // Estimated position of input position along to the nearest route segment
    int intersectionIndex;      // Intersection index of the nearest route segment
    int segmentIndex;           // Nearest route segment index
    ApproachType approachType;  // Primary or Secondary approach

} RoutePositionResults;

typedef struct RouteSegment
{
    Vector offset_m;    // Position of route node relative to intersection position
    Vector direction;   // Normalized direction-vector from this node to the next (<0,0> for last node)
    double length_m;    // Length of this node segment (0m for last node)
    double mileMarker;  // Mile-marker associated with this node

} RouteSegment;

typedef struct RouteApproach
{
    RouteSegment* pSegments;
    int segmentCount;
    double length_m;
    Vector anchor_utm;

} RouteApproach;

typedef struct RouteNode
{
    Vector utmPosition_m;
    RouteApproach primaryApproach;
    RouteApproach secondaryApproach;
    
} RouteNode;

typedef struct Map
{
    char roadwayId[ROADWAY_ID_MAX_LEN];
    RouteNode* pIntersections;
    int intersectionCount;

} Map;

typedef Map** MapPointerArray_t;

int mapFindDistFromAtoBMap(
    MapPointerArray_t ppMaps, 
    int mapCount, 
    PositionHeading* pA, 
    PositionHeading* pB, 
    double* pDistance_m_out, 
    RoutePositionResults* pAResults_out, 
    RoutePositionResults* pBResults_out
);

extern int mapFindDistFromAtoB(
    Map* pInstance, 
    PositionHeading* pA, 
    PositionHeading* pB, 
    double* pDistance_m_out, 
    RoutePositionResults* pAResults_out, 
    RoutePositionResults* pBResults_out
);

extern int mapFindNearestMapRouteSegment(
    PositionHeading* pGlobalPosHead, 
    MapPointerArray_t ppMaps, 
    int mapCount, 
    RoutePositionResults* pResults_out,
    int* pMapIndex_out
);

extern int mapFindNearestIntersectionRouteSegment(
    PositionHeading* pGlobalPosHead, 
    RouteNode* pIntersections, 
    int count, 
    RoutePositionResults* pResults_out
);

extern int mapFindNearestRouteSegment(
    PositionHeading* pLocalPosHead, 
    RouteSegment* pRoute, 
    int count,    
    double* minimumDist_m_out, 
    Vector* pMinimumLocalPosition_m_out, 
    int* pIndex_out
);

extern int mapFindPositionAlongApproach(
    PositionHeading* pGlobalPosition,
    RouteApproach* pApproach,
    double* completion_pct_out
);

extern void mapHeadingToVector(double heading_rad, Vector* pHeadingVector_out);
extern int mapMapDataToMap(MapData_t* pMessage, Map* pMap_out);
extern int mapRegionListToRouteApproach(ValidRegion_t** ppRegions, int regionCount, RouteApproach* pApproach_out);
extern void mapDestroy(Map* pMap);
extern void mapPrintApproach(RouteApproach* pApproach);

#endif
