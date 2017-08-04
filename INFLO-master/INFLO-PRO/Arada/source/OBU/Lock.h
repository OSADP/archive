#ifndef OBU_Lock_h
#define OBU_Lock_h

#include <pthread.h>

typedef struct Lock
{
    pthread_mutex_t mutexId;        

} Lock;

extern Lock*    lockCreate();
extern int      lockLock(Lock* pInstance);
extern int      lockTryLock(Lock* pInstance);
extern int      lockUnlock(Lock* pInstance);
extern void     lockDestroy(Lock* pInstance);

#endif
