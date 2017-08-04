#ifndef OBU_TimProcessor_h
#define OBU_TimProcessor_h

#include "MsgQueue.h"
#include "TimFilter.h"

#include "TravelerInformation.h"

#define ALERT_MSG_MAX_LEN 64

typedef struct AlertMessage
{
    double length_m;
    double completion_pct; // 0.0 to 1.0
    char message[ALERT_MSG_MAX_LEN];
    
} AlertMessage;

typedef void (*OnRecvAlert_t)(AlertMessage* pMsg);

typedef struct TimProcessor
{
    MsgQueue*  pIncomingQueue;
    TimFilter* pFilter;
    
    OnRecvAlert_t pAlertCallback;

} TimProcessor;

extern TimProcessor*    timprocCreate(int incomingQueueMaxLength, int outgoingQueueMaxLength, int filterSize);
extern void             timprocDestroy(TimProcessor* pInstance);

extern void             timprocSetAlertCallback(TimProcessor* pInstance, OnRecvAlert_t pCallback);

extern int              timprocPushMessage(TimProcessor* pInstance, TravelerInformation_t* pMessage);
extern void             timprocProcessMessages(TimProcessor* pInstance, double maxTimeToProcess_s);

extern unsigned long	timprocGetRelayCount();

#endif
