#ifndef VehicleStateMap_h
#define VehicleStateMap_h

#include "QWarnDefs.h"
#include "Lock.h"

#include <time.h>

#define ROADWAY_ID_MAX_LEN 64

typedef struct VehicleState
{
    struct timespec timestamp;
    uint32 id; 
    double lat_rad;
    double lon_rad;
    double easting_m;
    double northing_m;
    double heading_rad;
    double speed_m_s;
    double elevation_m;
    double mileMarker;
    char   roadwayId[ROADWAY_ID_MAX_LEN];
    
} VehicleState;

typedef struct VehicleStateMap
{
    int maxCount;
    VehicleState* pStates;
    int count;
    Lock* pLock;

} VehicleStateMap;

extern VehicleStateMap*    vsmapCreate(int maxCount);
extern void                vsmapDestroy(VehicleStateMap* pInstance);

extern void                vsmapPushState(VehicleStateMap* pInstance, VehicleState* pState);
extern int                 vsmapGetNumberOfCvs(VehicleStateMap* pInstance);
extern double              vsmapGetDistanceToClosestCv(VehicleStateMap* pInstance, double* minDistance_m_out);

#endif
