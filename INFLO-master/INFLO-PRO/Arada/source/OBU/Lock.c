#include "Lock.h"

Lock* lockCreate()
{
    Lock* pInstance = (Lock*)malloc(sizeof(Lock));
    
    pthread_mutexattr_t attr;
    pthread_mutexattr_init(&attr);
    pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
    pthread_mutex_init(&pInstance->mutexId, &attr);
    pthread_mutexattr_destroy(&attr);
    
    return pInstance;
}

int lockLock(Lock* pInstance)
{ 
    return pthread_mutex_lock(&pInstance->mutexId);
}

int lockTryLock(Lock* pInstance)
{
    return pthread_mutex_trylock(&pInstance->mutexId);
}

int lockUnlock(Lock* pInstance)
{
    return pthread_mutex_unlock(&pInstance->mutexId);
}

void lockDestroy(Lock* pInstance)
{
    pthread_mutex_destroy(&pInstance->mutexId);
    
    free(pInstance);
    
    return;
}
