#include "TimFilter.h"

#include "stdbool.h"

TimFilter* timfCreate(int maxCount)
{
    TimFilter* pInstance = (TimFilter*)malloc(sizeof(TimFilter));

    pInstance->maxCount = maxCount;
    pInstance->ids = (uint64*)calloc(maxCount, sizeof(uint64));
    pInstance->count = 0;
    pInstance->currentIndex = 0;
    pInstance->pLock = lockCreate();
    
    return pInstance;
}

int timfIsNewMessage(TimFilter* pInstance, TravelerInformation_t* pMessage)
{
    lockLock(pInstance->pLock);

    uint64 messageId = *((uint64*)pMessage->packetID->buf);

    // Find id in list of ids
    {
        int i;
        for(i=0; i<pInstance->maxCount; i++)
        {
            int index = (pInstance->currentIndex + i) % pInstance->maxCount;
            
            if(pInstance->ids[index] == messageId)
            { 
                lockUnlock(pInstance->pLock);
                
                return false;
            }
        }
    }
        
    pInstance->ids[pInstance->currentIndex] = messageId;
    
    pInstance->currentIndex++;
    
    if(pInstance->currentIndex == pInstance->maxCount)
    {
        pInstance->currentIndex = 0;
    }
    
    lockUnlock(pInstance->pLock);
    
    return true;
}

void timfDestroy(TimFilter* pInstance)
{
    lockDestroy(pInstance->pLock);
    free(pInstance->ids);
    free(pInstance);
    
    return;
}
