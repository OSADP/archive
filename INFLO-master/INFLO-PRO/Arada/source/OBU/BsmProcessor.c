#include "BsmProcessor.h"
#include "QWarnController.h"
#include "Vector.h"
#include "TimeStamp.h"

#include "asnwave.h"
#include <stdbool.h>

void bsmprocProcessBsm(BsmProcessor* pInstance, BasicSafetyMessage_t* pMessage);
bool bsmprocCheckIfQueued(BsmProcessor* pInstance, VehicleState* pOtherVehicle);

BsmProcessor* bsmprocCreate(int queueSize, int maxVehicleStates, QueuedStateParams* pParams)
{
    BsmProcessor* pInstance = (BsmProcessor*)malloc(sizeof(BsmProcessor));
    
    pInstance->pMsgQueue = msgQueueCreate(queueSize);
    pInstance->pVehicleStates = vsmapCreate(maxVehicleStates);
    pInstance->pGeoConv = geoConvCreate();
    
    pInstance->lastTimeInQueuedState = 0;
    pInstance->queuedStateParams = *pParams;
    
    return pInstance;
}

int bsmprocPushMessage(BsmProcessor* pInstance, BasicSafetyMessage_t* pMessage)
{
    msgQueuePushFront(pInstance->pMsgQueue, pMessage, WSMMSG_BSM);
    
    return;
}

void bsmprocDestroy(BsmProcessor* pInstance)
{
    vsmapDestroy(pInstance->pVehicleStates);
    msgQueueDestroy(pInstance->pMsgQueue);
    geoConvDestroy(pInstance->pGeoConv);
    
    free(pInstance);
    
    return;
}

void bsmprocProcessMessages(BsmProcessor* pInstance, double maxTimeToProcess_s)
{
 //   printf("%i\tBsmProcessor: Incoming = %i\n", time(NULL), pInstance->pMsgQueue->count);

    void* pMessage = NULL;
    unsigned short messageType = 0;
    struct timespec tStart={0,0}, tEnd={0,0};
    double timediff_ms = 0.0;
    
    clock_gettime(CLOCK_MONOTONIC, &tStart);
    while(msgQueuePopBack(pInstance->pMsgQueue, &pMessage, &messageType))
    {
        bsmprocProcessBsm(pInstance, (BasicSafetyMessage_t*)pMessage);
        
        msgQueueFreeMessage(pMessage, messageType);
        
        clock_gettime(CLOCK_MONOTONIC, &tEnd);        
        double timediff_s = tsToSeconds(tsSubtract(tEnd, tStart));
        
        if(timediff_ms > maxTimeToProcess_s)
        {
            break;
        }
    }
}

int bsmprocIsInQueuedState(BsmProcessor* pInstance)
{
    return (int)(difftime(time(NULL), pInstance->lastTimeInQueuedState) < pInstance->queuedStateParams.queuedStateResetTime_s);
}

void bsmprocProcessBsm(BsmProcessor* pInstance, BasicSafetyMessage_t* pMessage)
{
    BsmBlob1* pBsmBlob = (BsmBlob1*)pMessage->blob1.buf;
    
    VehicleState otherVehicleState;
    otherVehicleState.lat_rad = DEG2RAD * (pBsmBlob->lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG);
    otherVehicleState.lon_rad = DEG2RAD * (pBsmBlob->lon / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG);
    otherVehicleState.heading_rad = DEG2RAD * (pBsmBlob->heading * BSM_BLOB1_HEADING_CONV_DEG);
    otherVehicleState.speed_m_s = (pBsmBlob->speed & BSM_BLOB1_SPEED_MASK) * BSM_BLOB1_SPEED_UNIT_CONV_M_S;
    otherVehicleState.id = pBsmBlob->id;
    clock_gettime(CLOCK_MONOTONIC, &otherVehicleState.timestamp);   
    
    otherVehicleState.roadwayId[0] = '\0';  // Unnecessary for now
    otherVehicleState.mileMarker = 0.0;     // Unnecessary for now               
    
    double easting_m = -1.0, northing_m = -1.0;
    char zone = 0, band = 0;
    
    if(!geoConvGeodetic2Utm(
        pInstance->pGeoConv,
        otherVehicleState.lat_rad, otherVehicleState.lon_rad, 
        &zone, &band,
        &easting_m, &northing_m
    ))
    {
        printf("BsmProcessor: Could not find UTM position for BSM GeoCoord!\n");
        
        return;
    }
    
    otherVehicleState.easting_m = easting_m;
    otherVehicleState.northing_m = northing_m;
    
    vsmapPushState(pInstance->pVehicleStates, &otherVehicleState);
    
    if(bsmprocCheckIfQueued(pInstance, &otherVehicleState))
    {
        pInstance->lastTimeInQueuedState = time(NULL);
    }
    
    return;
}

bool bsmprocCheckIfQueued(BsmProcessor* pInstance, VehicleState* pOtherVehicle)
{
    VehicleState currentVehicleState;
    qwarnGetCurrentVehicleState(&currentVehicleState);

    // Are both vehicles moving slow?
    if(pOtherVehicle->speed_m_s < pInstance->queuedStateParams.queuedStateSpeed_m_s && 
       currentVehicleState.speed_m_s < pInstance->queuedStateParams.queuedStateSpeed_m_s) 
    {
        Vector otherVehicleHeading;
        Vector currentVehicleHeading;
        
        mapHeadingToVector(pOtherVehicle->heading_rad, &otherVehicleHeading);
        mapHeadingToVector(currentVehicleState.heading_rad, &currentVehicleHeading);
        
        // Are both vehicles on the same side of the road (heading the same direction)?
        if(vecDot(&otherVehicleHeading, &currentVehicleHeading) > 0.71)
        {
            Vector otherPosition = { pOtherVehicle->easting_m, pOtherVehicle->northing_m };
            Vector currentPosition = { currentVehicleState.easting_m, currentVehicleState.northing_m };
            
            Vector currentToOther;
            vecSub(&otherPosition, &currentPosition, &currentToOther);
            
            // Are they at the _exact_ same position?
            if(currentToOther.x == 0.0 && currentToOther.y == 0.0) 
            {
                return true;
            }
            
            // Are the vehicles close to each other?
            if(vecMag(&currentToOther) < pInstance->queuedStateParams.queuedStateDistance_m) 
            {
                Vector directionToOther;
                vecNormalize(&currentToOther, &directionToOther);
                
                // Is the other vehicle in front?
                if(vecDot(&currentVehicleHeading, &directionToOther) > 0.1)
                {
                    return true;
                }
            }
        }
    }
    
    return false;
}

/*

RoutePositionResults currentPosResults;
RoutePositionResults otherPosResults;
double distance_m = 0.0;

PositionHeading otherPositionHeading = { {easting_m, northing_m}, otherVehicleState.heading_rad};
PositionHeading currPosHead = { {currentVehicleState.easting_m, currentVehicleState.northing_m}, currentVehicleState.heading_rad };

if(mapFindDistFromAtoBMap(
    gpQWarnController->ppMaps,
    gpQWarnController->mapCount,
    &currPosHead,
    &otherPositionHeading,
    &distance_m,
    &currentPosResults,
    &otherPosResults
))
{ 
    if(distance_m >= 0.0 && distance_m < pInstance->queuedStateDistance_m)
    {
        pInstance->lastTimeInQueuedState = time(NULL);
    }
}
else
{
    otherVehicleState.mileMarker = -1.0;
    otherVehicleState.roadwayId[0] = 0;
    qwarnLogMessage("QWarn: ProcessBsm()-Could not find distance between current position and BSM target position!\n"); 
}

*/
