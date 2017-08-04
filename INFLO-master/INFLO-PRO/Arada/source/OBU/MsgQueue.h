#ifndef OBU_MsgQueue_h
#define OBU_MsgQueue_h

#include "Lock.h"

typedef struct MsgQueueNode
{
    void* pMessage;
    unsigned short messageType;
    struct MsgQueueNode* pNext;
    struct MsgQueueNode* pPrev;
    
} MsgQueueNode;

typedef struct MsgQueue
{
    MsgQueueNode* pFront;
    MsgQueueNode* pBack;    
    unsigned int count;
    unsigned int maxCount;
    Lock* pLock;

} MsgQueue;

extern MsgQueue*    msgQueueCreate(unsigned int maxCount);
extern void         msgQueueDestroy(MsgQueue* pInstance);

extern int          msgQueuePushFront(MsgQueue* pInstance, void* pMessage, unsigned short messageType);
extern int          msgQueuePopBack(MsgQueue* pInstance, void** ppMessage_out, unsigned short* pMessageType_out);
extern void         msgQueueFreeMessage(void* pMessage, unsigned short messageType);

#endif

