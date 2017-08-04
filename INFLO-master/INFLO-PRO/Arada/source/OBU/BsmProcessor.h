#ifndef OBU_BsmProcessor_h
#define OBU_BsmProcessor_h

#include "MsgQueue.h"
#include "VehicleStateMap.h"
#include "GeoCoordConverter.h"

#include "BasicSafetyMessage.h"
#include <time.h>

typedef struct QueuedStateParams
{
    double queuedStateDistance_m;
    double queuedStateSpeed_m_s;
    int    queuedStateResetTime_s;

} QueuedStateParams;

typedef struct BsmProcessor
{
    MsgQueue*           pMsgQueue;
    VehicleStateMap*    pVehicleStates;
    GeoCoordConverter*  pGeoConv;
    
    time_t              lastTimeInQueuedState;
    QueuedStateParams   queuedStateParams;
    
} BsmProcessor;

extern BsmProcessor*   bsmprocCreate(int queueSize, int maxVehicleStates, QueuedStateParams* pParams);
extern void            bsmprocDestroy(BsmProcessor* pInstance);

extern int             bsmprocPushMessage(BsmProcessor* pInstance, BasicSafetyMessage_t* pMessage);
extern void            bsmprocProcessMessages(BsmProcessor* pInstance, double maxTimeToProcess_s);
extern int             bsmprocIsInQueuedState(BsmProcessor* pInstance);


#endif
