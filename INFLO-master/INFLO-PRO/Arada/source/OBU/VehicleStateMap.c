#include "VehicleStateMap.h"
#include "Vector.h"
#include "QWarnController.h"
#include "TimeStamp.h"

#include <stdbool.h>

const double MAX_VEHICLESTATE_AGE_S = 5.0;

VehicleStateMap* vsmapCreate(int maxCount)
{
    VehicleStateMap* pInstance = (VehicleStateMap*)malloc(sizeof(VehicleStateMap));
    pInstance->maxCount = maxCount;
    pInstance->pStates = (VehicleState*)malloc(sizeof(VehicleState) * maxCount);
    pInstance->count = 0;
    pInstance->pLock = lockCreate();
    
    return;
}

void vsmapPushState(VehicleStateMap* pInstance, VehicleState* pState)
{
    lockLock(pInstance->pLock);

    // Find if vehicle state id already exists; if so, update data
    int i;
    for(i=0; i<pInstance->count; i++)
    {
        if(pState->id == pInstance->pStates[i].id)
        {
            pInstance->pStates[i] = (*pState);
            lockUnlock(pInstance->pLock);
            return;
        }
    }
    
    // This is new vehicle information:    
    // If map is full, find oldest entry and replace with this data
    if(pInstance->count == pInstance->maxCount)
    {
        int minIndex = 0;
        struct timespec minTime = pInstance->pStates[0].timestamp;
        
        for(i=1; i<pInstance->count; i++)
        {        
            double timediff_s = tsToSeconds(tsSubtract(minTime, pInstance->pStates[i].timestamp));
        
            if(timediff_s < 0.0)
            {
                minTime = pInstance->pStates[i].timestamp;
                minIndex = i;
            }
        }
        
        pInstance->pStates[minIndex] = (*pState);
    }
    else // pInstance->count < pInstance->maxCount
    {
        pInstance->pStates[pInstance->count] = (*pState);
        pInstance->count++;
    }
    
    lockUnlock(pInstance->pLock);
    
    return;
}

int vsmapGetNumberOfRvs(VehicleStateMap* pInstance)
{
    lockLock(pInstance->pLock);
    
    int numberOfCvs = 0;
    int i;
    for(i=0; i<pInstance->count; i++)
    {
        VehicleState* pOtherVehicleState = &(pInstance->pStates[i]);
        
        struct timespec timeNow;
        clock_gettime(CLOCK_MONOTONIC, &timeNow);
        
        double timediff_s = tsToSeconds(tsSubtract(timeNow, pOtherVehicleState->timestamp));
        
        if(timediff_s <= MAX_VEHICLESTATE_AGE_S)
        {
            numberOfCvs++;
        }
    }
    
    lockUnlock(pInstance->pLock);
    
    return numberOfCvs;    
}

double vsmapGetDistanceToClosestRv(VehicleStateMap* pInstance, double* minDistance_m_out)
{
    double minDistance_m = 9999.0;
    int minIndex = 0;
    
    VehicleState currentVehicleState;
    qwarnGetCurrentVehicleState(&currentVehicleState);
    
    lockLock(pInstance->pLock);
    
    int i;
    for(i=0; i<pInstance->count; i++)
    {
        VehicleState* pOtherVehicleState = &(pInstance->pStates[i]);
        
        struct timespec timeNow;
        clock_gettime(CLOCK_MONOTONIC, &timeNow); 
        
        double timediff_s = tsToSeconds(tsSubtract(timeNow, pOtherVehicleState->timestamp));
        
        Vector a = { currentVehicleState.easting_m, currentVehicleState.northing_m};
        Vector b = { pOtherVehicleState->easting_m, pOtherVehicleState->northing_m};
        
        Vector r;
        vecSub(&b, &a, &r);
        
        double distance_m = vecMag(&r);
        
        if(distance_m < minDistance_m && timediff_s <= MAX_VEHICLESTATE_AGE_S)
        {
            minDistance_m = distance_m;
            minIndex = i;
        }
    }
    
    lockUnlock(pInstance->pLock);
    
    printf("MinDistance: %f\n", minDistance_m);
    
    (*minDistance_m_out) = minDistance_m;
    
    return minDistance_m;
}

void vsmapDestroy(VehicleStateMap* pInstance)
{
    lockDestroy(pInstance->pLock);
    free(pInstance->pStates);
    free(pInstance);
}
