#include "Lock.h"
#include <malloc.h>

Lock* lock_create()
{
    Lock* pInstance = (Lock*)malloc(sizeof(Lock));
    
    pthread_mutexattr_t attr;
    pthread_mutexattr_init(&attr);
    pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
    pthread_mutex_init(&pInstance->mutexId, &attr);
    pthread_mutexattr_destroy(&attr);
    
    return pInstance;
}

int lock_lock(Lock* pInstance)
{ 
    return pthread_mutex_lock(&pInstance->mutexId);
}

int lock_tryLock(Lock* pInstance)
{
    return pthread_mutex_trylock(&pInstance->mutexId);
}

int lock_unlock(Lock* pInstance)
{
    return pthread_mutex_unlock(&pInstance->mutexId);
}

void lock_destroy(Lock* pInstance)
{
    pthread_mutex_destroy(&pInstance->mutexId);
    
    free(pInstance);
    
    return;
}
