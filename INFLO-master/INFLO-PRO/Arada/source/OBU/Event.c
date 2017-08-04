#include "Event.h"

#include <stdbool.h>
#include <errno.h>
#include <sys/time.h>

typedef unsigned long long uint64;

Event* eventCreate(EventMode mode)
{
    Event* pInstance = (Event*)malloc(sizeof(Event));
    
    pInstance->state = EVENT_RESET;
    pInstance->pLock = lockCreate();
    pInstance->mode = mode;
    pthread_cond_init(&pInstance->condId, 0);
    
    return pInstance;
}

int eventWait(Event* pInstance, unsigned int timeout_ms)
{
    if(timeout_ms == 0)
    {
        if(lockTryLock(pInstance->pLock) != 0)
        {
            return false;
        }
    }
    else // timeout_ms > 0
    {    
        if(lockLock(pInstance->pLock) != 0)
        {
            return false;
        }
    }
    
    int result = 0;
    
    if(pInstance->state == EVENT_RESET)
    {
        if(timeout_ms == 0)
        {
            result = ETIMEDOUT;
        }
        else if(timeout_ms == EVENT_WAIT_INFINITE)
        {
            result = pthread_cond_wait(&pInstance->condId, &pInstance->pLock->mutexId);
        }
        else
        {
            struct timeval tv;
            gettimeofday(&tv, NULL);
            
            // Check 64bit integer specification for this architecture.
            const uint64 ns = 
                (((uint64)tv.tv_sec) * 1000000000) + 
                ((uint64)timeout_ms * 1000000) + 
                (((uint64)tv.tv_usec) * 1000);
                
            struct timespec ts;
            ts.tv_sec = (__time_t)(ns / 1000000000);
            ts.tv_nsec = (long int)(ns - (((uint64)ts.tv_sec) * 1000000000));
            
            result = pthread_cond_timedwait(&pInstance->condId, &pInstance->pLock->mutexId, &ts);
        }
    }
    
    if((pInstance->mode == EVENT_AUTO_RESET) && (result == 0))
    {
        pInstance->state = EVENT_RESET;
    }
    
    if(lockUnlock(pInstance->pLock) != 0)
    {
        return false;
    }
    
    return (result == 0);
}

int eventSignal(Event* pInstance)
{
    if(lockLock(pInstance->pLock) != 0)
    {
        return false;
    }
    
    pInstance->state = EVENT_SET;
    
    if(lockUnlock(pInstance->pLock) != 0)
    {
        return false;
    }    
    
    switch(pInstance->mode)
    {
        case EVENT_AUTO_RESET:
        {
            if(pthread_cond_signal(&pInstance->condId) != 0)
            {
                return false;
            }
        }
        break;
        
        case EVENT_MANUAL_RESET:
        {
            if(pthread_cond_broadcast(&pInstance->condId) != 0)
            {
                return false;
            }
        }
        break;
    }
    
    return true;
}

int eventReset(Event* pInstance)
{
    if(lockLock(pInstance->pLock) != 0)
    {
        return false;
    }
    
    pInstance->state = EVENT_RESET;
    
    if(lockUnlock(pInstance->pLock) != 0)
    {
        return false;
    }
    
    return true;
}

void eventDestroy(Event* pInstance)
{
    lockDestroy(pInstance->pLock);
    
    pthread_cond_destroy(&pInstance->condId);
    
    free(pInstance);
    
    return;
}

