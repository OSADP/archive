#ifndef OBU_Lock_h
#define OBU_Lock_h

#include <pthread.h>

typedef struct Lock
{
    pthread_mutex_t mutexId;        

} Lock;

extern Lock*    lock_create();
extern int      lock_lock(Lock* pInstance);
extern int      lock_tryLock(Lock* pInstance);
extern int      lock_unlock(Lock* pInstance);
extern void     lock_destroy(Lock* pInstance);

#endif
