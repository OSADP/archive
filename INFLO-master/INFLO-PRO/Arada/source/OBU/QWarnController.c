#include "QWarnController.h"
#include "QWarnDefs.h"
#include "Event.h"
#include "BsmProcessor.h"
#include "TimProcessor.h"
#include "Vector.h"
#include "GeoCoordConverter.h"
#include "Map.h"
#include "infloUIMsg.h"
#include "ConfigFile.h"
#include "TimeStamp.h"
#include "MsgQueue.h"

#include <string.h>
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <pthread.h>
#include "asnwave.h"
#include "MapData.h"
#include "constr_TYPE.h"
#include "dirent.h"

#define MAX_DIR_LENGTH 64

const double MAX_QUEUE_PROC_TIME_S = 0.1;

typedef struct QWarnController
{
    VehicleState    currVehicleState;
    BsmProcessor*   pBsmProcessor;
    TimProcessor*   pTimProcessor;
    Lock*           pVehicleStateLock;
    
    pthread_t       threadId;
    Event*          pWaitEvent;
    bool            keepRunning;
    bool            loggingEnabled;
    
    GeoCoordConverter* pGeoConv;    
    
    MapPointerArray_t   ppMaps;
    int                 mapCount;
    
} QWarnController;

QWarnController* gpQWarnController = NULL;

/* "Private" function Declarations     */
void qwarnProcessQueues();
void qwarnLogMessage(const char* msg, ...);
void* qwarnThreadFunction(void* pArgs);
int qwarnLoadMapData();
void qwarnSetMapData(void* pSerializedMapData, unsigned int size_bytes, Map** ppMapData_out);

/* Function implementations             */
int qwarnStart()
{
    gpQWarnController = (QWarnController*)malloc(sizeof(QWarnController));
    gpQWarnController->pWaitEvent = eventCreate(EVENT_AUTO_RESET);
    gpQWarnController->keepRunning = true;
    gpQWarnController->loggingEnabled = false;
    gpQWarnController->ppMaps = NULL;
    gpQWarnController->mapCount = 0;
    gpQWarnController->pVehicleStateLock = lockCreate();
    gpQWarnController->pGeoConv = geoConvCreate();
    
    QueuedStateParams params;
    params.queuedStateDistance_m = cfGetConfigFile()->queuedDistanceThreshold_m;
    params.queuedStateSpeed_m_s = cfGetConfigFile()->queuedSpeedThreshold_m_s;
    params.queuedStateResetTime_s = cfGetConfigFile()->queuedResetTime_s;        
    gpQWarnController->pBsmProcessor = bsmprocCreate(10, 10, &params);  
    gpQWarnController->pTimProcessor = timprocCreate(10, 10, 50);
    
    memset(&gpQWarnController->currVehicleState, 0, sizeof(VehicleState));
    
    if(!qwarnLoadMapData())
    {
        qwarnLogMessage("Q-Warn: No map data found!\n");
    }
    
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_JOINABLE);
    int result = pthread_create(&gpQWarnController->threadId, NULL, qwarnThreadFunction, gpQWarnController);    
    pthread_attr_destroy(&attr);
    
    if(result != 0)
    {
        qwarnLogMessage("Q-Warn: Could not start thread!\n");
        qwarnDestroy();
        
        return false;
    }
    
    return true;
}

int qwarnLoadMapData()
{
    const char* mapDir = cfGetConfigFile()->mapDir;
    Map** ppMaps = NULL;   
    int mapCount = 0; 
    DIR *dir;
    struct dirent *ent;
    
    // Count number of maps in dir
    {
        dir = opendir(mapDir);
        
        if(dir == NULL)
        {
            return false;
        }
        
        ent = readdir(dir);
        while(ent != NULL)
        {
            char* ext = strtok(ent->d_name, ".");            
                  ext = strtok(NULL, ".");
            
            if(ext != NULL && strcmp(ext, "ber") == 0)
            {
                mapCount++;
            }
            
            ent = readdir(dir);
        }
        
        closedir(dir);
    }
    
    // Load each map
    {
        printf("Found %i maps\n", mapCount);
    
        int index = 0;
        
        ppMaps = (Map**)malloc(sizeof(Map*) * mapCount);
        
        dir = opendir(mapDir);
        ent = readdir(dir);
        
        while(ent != NULL)
        {
            char* ext = strtok(ent->d_name, ".");            
                  ext = strtok(NULL, ".");
            
            if(ext != NULL && strcmp(ext, "ber") == 0)
            {
                char filename[MAX_DIR_LENGTH];
                filename[0] = 0;
                
                strcat(filename, mapDir);
                strcat(filename, "/");
                strcat(filename, ent->d_name);
                strcat(filename, ".ber");
                
                printf("Loading map %s...", filename);
                
                FILE* pFile = fopen(filename, "r");
                if(pFile)
                {
                    uint32 size_bytes = 0;
                    
                    fseek(pFile, 0, SEEK_END);
                    size_bytes = ftell(pFile);
                    fseek(pFile, 0, SEEK_SET);
                    
                    printf("%i bytes...\n", size_bytes);
                    
                    uint8* pBuffer = malloc(size_bytes);
                    
                    if(fread(pBuffer, 1, size_bytes, pFile) == size_bytes)
                    {
                        Map* pMap;
                        qwarnSetMapData(pBuffer, size_bytes, &pMap);
                        ppMaps[index] = pMap;
                    }
                    else
                    {
                        printf("QWarn: Error reading %s\n", filename);
                    }
                    
                    free(pBuffer);
                }
                else
                {
                    printf("QWarn: Could not open %s!\n", filename);
                }
                
                index++;
            }
            
            ent = readdir(dir);
        }
        
        closedir(dir);
    }
    
    gpQWarnController->ppMaps = ppMaps;
    gpQWarnController->mapCount = mapCount;
    
    return true;
}

void qwarnStop()
{
    qwarnLogMessage("Q-Warn: Stopping thread...\n");
    
    gpQWarnController->keepRunning = false;
    eventSignal(gpQWarnController->pWaitEvent);
    
    return;
}

void qwarnDestroy()
{
    eventDestroy(gpQWarnController->pWaitEvent);
    bsmprocDestroy(gpQWarnController->pBsmProcessor);
    timprocDestroy(gpQWarnController->pTimProcessor);
    geoConvDestroy(gpQWarnController->pGeoConv);
    
    if(gpQWarnController->mapCount != 0)
    {
        int i;
        for(i=0; i<gpQWarnController->mapCount; i++)
        {
            mapDestroy(gpQWarnController->ppMaps[i]);
        }
        
        free(gpQWarnController->ppMaps);
    }
    
    free(gpQWarnController);

    gpQWarnController = NULL;
    
    return;
}

void qwarnEnableLogging()
{
    gpQWarnController->loggingEnabled = true;
    return;
}

void qwarnDisableLogging()
{
    gpQWarnController->loggingEnabled = false;    
    return;
}

TimProcessor* qwarnGetTimProcessor()
{
    return gpQWarnController->pTimProcessor;
}

BsmProcessor* qwarnGetBsmProcessor()
{
    return gpQWarnController->pBsmProcessor;
}

void qwarnJoin()
{    
    pthread_join(gpQWarnController->threadId, 0);    
    return;
}

int qwarnSetAlertCallback(OnRecvAlert_t pCallback)
{
    timprocSetAlertCallback(gpQWarnController->pTimProcessor, pCallback);
}

int qwarnGetCurrentVehicleState(VehicleState* pState_out)
{
    lockLock(gpQWarnController->pVehicleStateLock);
    *pState_out = gpQWarnController->currVehicleState;
    lockUnlock(gpQWarnController->pVehicleStateLock);
    
    return;
}

void qwarnUpdateGpsData(GPSData* pData)
{    
    lockLock(gpQWarnController->pVehicleStateLock);    
    
    clock_gettime(CLOCK_MONOTONIC, &gpQWarnController->currVehicleState.timestamp);
    
    gpQWarnController->currVehicleState.lat_rad     = DEG2RAD * pData->latitude;
    gpQWarnController->currVehicleState.lon_rad     = DEG2RAD * pData->longitude;    
    gpQWarnController->currVehicleState.heading_rad = DEG2RAD * pData->course;
    if(btveh.speed != 8191)
    {
    	gpQWarnController->currVehicleState.speed_m_s   = btveh.speed * .278; 
    }
    else
    {
	gpQWarnController->currVehicleState.speed_m_s   = pData->speed;
    }
    gpQWarnController->currVehicleState.elevation_m = pData->altitude;
    
    char zone, band;
    double easting_m, northing_m;
    
    if(geoConvGeodetic2Utm(
        gpQWarnController->pGeoConv,
        gpQWarnController->currVehicleState.lat_rad, gpQWarnController->currVehicleState.lon_rad, 
        &zone, &band,
        &easting_m, &northing_m
    ))
    {
        gpQWarnController->currVehicleState.easting_m = easting_m;
        gpQWarnController->currVehicleState.northing_m = northing_m;
    }
    else
    {
        gpQWarnController->currVehicleState.easting_m = 0.0;
        gpQWarnController->currVehicleState.northing_m = 0.0;
    }
    
    gpQWarnController->currVehicleState.mileMarker = -9.9;      
    gpQWarnController->currVehicleState.roadwayId[0] = 0;
    
    if(gpQWarnController->mapCount != 0)
    {    
        RoutePositionResults results;
        
        PositionHeading posHead;
        posHead.position_m.x = easting_m;
        posHead.position_m.y = northing_m;
        posHead.heading_rad = gpQWarnController->currVehicleState.heading_rad;
        
        double mileMarker = -9.9;
        
        int mapIndex;
        if(mapFindNearestMapRouteSegment(
            &posHead,
            gpQWarnController->ppMaps,
            gpQWarnController->mapCount,
            &results,
            &mapIndex
        ))
        {         
            
            if(results.approachType == PRIMARY) 
            { 
                mileMarker = gpQWarnController->
                    ppMaps[mapIndex]->
                    pIntersections[results.intersectionIndex].
                    primaryApproach.
                    pSegments[results.segmentIndex].
                    mileMarker;
                    
                strBufferCopy(gpQWarnController->currVehicleState.roadwayId, 
                              gpQWarnController->ppMaps[mapIndex]->roadwayId,
                              ROADWAY_ID_MAX_LEN);
            }
            else // results.approachType == SECONDARY
            {
                mileMarker = gpQWarnController->
                    ppMaps[mapIndex]->
                    pIntersections[results.intersectionIndex].
                    secondaryApproach.
                    pSegments[results.segmentIndex].
                    mileMarker;
                    
                strBufferCopy(gpQWarnController->currVehicleState.roadwayId, 
                              gpQWarnController->ppMaps[mapIndex]->roadwayId,
                              ROADWAY_ID_MAX_LEN);
            }
        }
//        else
//        {
//            qwarnLogMessage("Could not snap current position to route!\n");
//        }
        
        gpQWarnController->currVehicleState.mileMarker = mileMarker;
             
    }
    
    //uiprintf("Snapped to %f\n", gpQWarnController->currVehicleState.mileMarker);
     
    lockUnlock(gpQWarnController->pVehicleStateLock);
    
    eventSignal(gpQWarnController->pWaitEvent);
    
    return;
}

int qwarnOnRecvMessage(void* pMessage, unsigned short messageType)
{
    switch(messageType)
    {
        case WSMMSG_BSM:
            bsmprocPushMessage(gpQWarnController->pBsmProcessor, pMessage);
            eventSignal(gpQWarnController->pWaitEvent);
            break;
        case WSMMSG_TIM:
            timprocPushMessage(gpQWarnController->pTimProcessor, pMessage);
            eventSignal(gpQWarnController->pWaitEvent);
            break;
        default:
            msgQueueFreeMessage(pMessage, messageType);
            return false;
    }
    
    return true;
}

void qwarnSetMapData(void* pSerializedMapData, unsigned int size_bytes, Map** ppMap_out)
{
    MapData_t* pMapData = NULL;
    
    asn_dec_rval_t rval = ber_decode(NULL, &asn_DEF_MapData, (void **)&pMapData, pSerializedMapData, size_bytes);
    
    if(rval.code != 0)
    {
        qwarnLogMessage("QWarn: Could not decode map data!\n");
    }
    
    Map* pMap = (Map*)malloc(sizeof(Map));
    mapMapDataToMap(pMapData, pMap);
    
    (*ppMap_out) = pMap;
    
    /*
    printf("Map %i - Intersection Count = %i\n", pMap, pMap->intersectionCount);
    
    int i, j;
    for(i=0; i<pMap->intersectionCount; i++)
    {
        RouteNode* pIntersection = &(pMap->pIntersections[i]);
        
        printf("Intersection %i - position (utm): { %f, %f }\n", i, pIntersection->utmPosition_m.x, pIntersection->utmPosition_m.y);
        printf(
            "\tPrimary Approach - position (utm): {%f, %f}, len=%f\n", 
            pIntersection->primaryApproach.anchor_utm.x, 
            pIntersection->primaryApproach.anchor_utm.y,
            pIntersection->primaryApproach.length_m
        );
        
        for(j=0; j<pIntersection->primaryApproach.segmentCount; j++)
        {
            RouteSegment* pSegment = &(pIntersection->primaryApproach.pSegments[j]);
            printf(
                "\t\tNode %i: pos={%f, %f}, dir={%f, %f}, len=%f, mim=%f\n", 
                j, 
                pSegment->offset_m.x, pSegment->offset_m.y,
                pSegment->direction.x, pSegment->direction.y,
                pSegment->length_m,
                pSegment->mileMarker
            );
        }
        
        printf(
            "\tSecondary Approach - position (utm): {%f, %f}, len=%f\n", 
            pIntersection->secondaryApproach.anchor_utm.x, 
            pIntersection->secondaryApproach.anchor_utm.y, 
            pIntersection->secondaryApproach.length_m
        );
        
        for(j=0; j<pIntersection->secondaryApproach.segmentCount; j++)
        {
            RouteSegment* pSegment = &(pIntersection->secondaryApproach.pSegments[j]);
            printf(
                "\t\tNode %i: pos={%f, %f}, dir={%f, %f}, len=%f, mim=%f\n", 
                j, 
                pSegment->offset_m.x, pSegment->offset_m.y,
                pSegment->direction.x, pSegment->direction.y,
                pSegment->length_m,
                pSegment->mileMarker
            );
        }
        
    }
    
        
    double headings[5] = 
    {
        0.0,
        PI_2,
        PI,
        THREE_PI_2,
        TWO_PI
    };
    
    qwarnLogMessage("Heading to Vector test:\n");
    for(i=0; i<5; i++)
    {
        Vector dir;
        mapHeadingToVector(headings[i], &dir);
        
        qwarnLogMessage("%f -> {%f, %f}\n", headings[i], dir.x, dir.y);
    }
    qwarnLogMessage("\n");
    
    #define NUM_POSITIONS 15
    PositionHeading positions[NUM_POSITIONS] = 
    { 
        {{327406.55, 4428364.37}, 4.7},     // West on 5th - Primary
        {{327393.07, 4428348.03}, 4.7},     // West on 5th - Primary
        {{327283.78, 4428361.08}, 4.7},     // West on 5th - Primary
        {{327173.18, 4428395.18}, 4.7},     // West on 5th - Primary
        {{327389.64, 4428344.39}, 1.57},    // East on 5th - Secondary
        
        {{327049.36, 4428436.63}, -0.2},    // North on Olentangy - Primary
        {{327047.40, 4428527.55}, -0.3},    // North on Olentangy - Primary
        {{327040.42, 4428643.52}, -0.0},    // North on Olentangy - Primary
        {{327155.74, 4428703.66}, -1.57},    // West on King - Secondary
        {{327042.65, 4428464.60}, 3.14},    // South on Olentangy - Secondary
        {{327635.97, 4428672.13}, -1.57},    // West on King - Secondary
              
        {{327521.84, 4428678.05}, 1.3},     // East on King - Primary
        {{327700.27, 4428662.72}, 1.8},     // East on King - Primary
        {{327611.30, 4428670.84}, -1.57},   // West on King - Secondary  
        {{327494.51, 4428681.61}, -1.6}     // West on King - Secondary
    };
    
    /*
    Vector aInApproach;
    vecSub(&positions[0], &map.pIntersections[0].primaryApproach.anchor_utm, &aInApproach);
    positions[0].position_m = aInApproach;
    
    double distance_m;
    Vector v;
    int index;
    
    bool r = mapFindNearestRouteSegment(
        &positions[0], 
        map.pIntersections[0].primaryApproach.pSegments, 
        map.pIntersections[0].primaryApproach.segmentCount,    
        &distance_m, 
        &v, 
        &index
    );
    
    if(r)
    {
        printf("NearestRouteSeg: index= %i, dist=%f, pos={%f, %f}\n\n", index, distance_m, v.x, v.y);
    }
    else
    {
        printf("Couldn't find a close enough route segment.\n");
    }
    
    */
    
    /*
    for(i=0; i<NUM_POSITIONS; i++)
    {    
        RoutePositionResults results;
        
        bool r = mapFindNearestIntersectionRouteSegment(
            &positions[i],
            map.pIntersections,
            map.intersectionCount,
            &results
        ); 
        
        printf("Results {%f, %f, %f}:\n", positions[i].position_m.x, positions[i].position_m.y, positions[i].heading_rad);
        
        if(r)
        {
            printf("\tDistance: %fm", results.distance_m);
            printf("\tLocalPosition: {%f, %f}\n", results.localPosition_m.x, results.localPosition_m.y);
            printf("\tIndices: i=%i, s=%i\n", results.intersectionIndex, results.segmentIndex);
            printf("\tApproach: %s\n", (results.approachType == PRIMARY) ? "Primary" : "Secondary");
            printf("\tSegment:\n");
            printf(
                "\t\tMileMarker:%f\n", 
                (results.approachType == PRIMARY) ? 
                    map.pIntersections[results.intersectionIndex].primaryApproach.pSegments[results.segmentIndex].mileMarker :
                    map.pIntersections[results.intersectionIndex].secondaryApproach.pSegments[results.segmentIndex].mileMarker
            );
            printf("\n");
        }
        else
        {
            printf("\tCould not bind coordinate to route!\n");
        }
    }
    */
    /*
    typedef struct Pair
    {
        PositionHeading* pA;
        PositionHeading* pB;
        
    } Pair;
    
    #define NUM_PAIRS 11
    Pair pairs[NUM_PAIRS] = 
    {
        {&positions[0], &positions[12]},    // Primary, B in front of A, ~ +1350m
        {&positions[12], &positions[0]},    // Primary, B behind A, ~ -1350m
        {&positions[0], &positions[13]},    // B on different approach than A, no distance
        {&positions[14], &positions[4]},    // Secondary, B in front of A, ~ +1100m
        {&positions[4], &positions[14]},    // Secondary, A in front of B, ~ -1100m
        {&positions[0], &positions[1]},     // Primary, B in front of A, ~ +10m
        {&positions[1], &positions[0]},     // Primary, A in front of B, ~ -10m
        {&positions[0], &positions[3]},     // Primary, B in front of A, ~ +230m
        {&positions[3], &positions[0]},     // Primary, A in front of B, ~ -230m
        {&positions[10], &positions[8]},    // Secondary, B in front of A ~ +480m
        {&positions[8], &positions[10]},    // Secondary, A in front of B ~ -480m
    };
    
    double distance_m = 0.0;
    RoutePositionResults a;
    RoutePositionResults b;
    
    for(i=0; i<NUM_PAIRS; i++)
    {
        qwarnLogMessage("FindDistance %i:\n", i);
        if(mapFindDistFromAtoB(pMap, pairs[i].pA, pairs[i].pB, &distance_m, &a, &b))
        {
            qwarnLogMessage("Distance from A to B is %f\n", distance_m);
        }
        else
        {
            qwarnLogMessage("Could not find distance between the two positions.\n");
        }
    }
    */
    
    return;
}

int qwarnIsQueued()
{
    return bsmprocIsInQueuedState(gpQWarnController->pBsmProcessor);
}

void qwarnProcessQueues()
{    
    timprocProcessMessages(gpQWarnController->pTimProcessor, MAX_QUEUE_PROC_TIME_S); 
    bsmprocProcessMessages(gpQWarnController->pBsmProcessor, MAX_QUEUE_PROC_TIME_S);
    
    return;
}

void qwarnLogMessage(const char* msg, ...)
{
    if(gpQWarnController->loggingEnabled)
    {
        static char ui_msg[2000];
    
        va_list args;
        va_start(args, msg);
        vsnprintf(ui_msg, 2000, msg, args); 
        va_end(args);
        
        printf("%s", ui_msg);
        //uiprintf("%s", ui_msg);
    }
    
    return;
}

void* qwarnThreadFunction(void* pArgs)
{    
    qwarnLogMessage("Q-Warn: Thread started.\n");

    while(gpQWarnController->keepRunning)
    {
        qwarnProcessQueues();
    
        eventWait(gpQWarnController->pWaitEvent, EVENT_WAIT_INFINITE);
    }
    
    qwarnLogMessage("Q-Warn: Thread shutdown.\n");
    
    pthread_exit(NULL);
    
    return;
}

double qwarnGetDistanceToClosestRv()
{
    double dist_m;
    vsmapGetDistanceToClosestRv(gpQWarnController->pBsmProcessor->pVehicleStates, &dist_m);
    printf("QWARN: MinDistance: %f\n", dist_m);

    return dist_m;
}

int qwarnGetNumberOfCvs()
{
    return vsmapGetNumberOfRvs(gpQWarnController->pBsmProcessor->pVehicleStates);
}

