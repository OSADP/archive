#include "MsgQueue.h"

#include <string.h>
#include <stdbool.h>
#include "asnwave.h"
#include "BasicSafetyMessage.h"
#include "TravelerInformation.h"

extern asn_TYPE_descriptor_t *asn_pdu_collection[];

MsgQueue* msgQueueCreate(unsigned int maxCount)
{
    MsgQueue* pInstance = (MsgQueue*)malloc(sizeof(MsgQueue));
    pInstance->pFront = NULL;
    pInstance->pBack = NULL;
    pInstance->count = 0;
    pInstance->maxCount = maxCount;
    pInstance->pLock = lockCreate();
    
    return pInstance;
}

int msgQueuePushFront(MsgQueue* pInstance, void* pMessage, unsigned short messageType)
{
    lockLock(pInstance->pLock);
    
    if(pInstance->count == pInstance->maxCount)
    {
        lockUnlock(pInstance->pLock);
        return false;
    }
    
    MsgQueueNode* pNode = (MsgQueueNode*)malloc(sizeof(MsgQueueNode));
    pNode->pNext = pInstance->pFront;
    pNode->pPrev = NULL;
    pNode->pMessage = pMessage;
    pNode->messageType = messageType;
       
    if(pInstance->count == 0)
    {
        pInstance->pBack = pNode;
    }
    else // pInstance->count > 0
    {
        pInstance->pFront->pPrev = pNode;
    }
    
    pInstance->pFront = pNode;    
    pInstance->count++;
    
    lockUnlock(pInstance->pLock);
    
    return true;
}

int msgQueuePopBack(MsgQueue* pInstance, void** ppMessage_out, unsigned short* pMessageType_out)
{
    lockLock(pInstance->pLock);
    
    MsgQueueNode* pBack = pInstance->pBack;

    if(pInstance->count == 0)
    {
        lockUnlock(pInstance->pLock);
        return false;
    }
    
    *ppMessage_out = pBack->pMessage;
    *pMessageType_out = pBack->messageType;
            
    pInstance->pBack = pInstance->pBack->pPrev;
    pInstance->count--;
    
    free(pBack);
    
    lockUnlock(pInstance->pLock);
    
    return true;
}

void msgQueueDestroy(MsgQueue* pInstance)
{
    MsgQueueNode* pCurr = pInstance->pFront;
    MsgQueueNode* pNext;
    
    while(pCurr != NULL)
    {
        pNext = pCurr->pNext;
        
		msgQueueFreeMessage(pCurr->pMessage, pCurr->messageType);
        
        free(pCurr);
        
        pCurr = pNext;
    }
    
    lockDestroy(pInstance->pLock);
    
    free(pInstance);
}

void msgQueueFreeMessage(void* pMessage, unsigned short messageType)
{
    asn_TYPE_descriptor_t **pdu = asn_pdu_collection;
	asn_TYPE_descriptor_t *pduType = pdu[messageType];
	ASN_STRUCT_FREE(*pduType, pMessage);
	
	return;
}
