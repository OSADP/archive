#ifndef OBU_Event_h
#define OBU_Event_h

#include <pthread.h>
#include "Lock.h"

typedef enum
{
    EVENT_RESET     = 0,
    EVENT_SET       = 1

} EventState;

typedef enum
{
    EVENT_AUTO_RESET    = 0,
    EVENT_MANUAL_RESET  = 1    

} EventMode;

typedef struct Event
{
    pthread_cond_t  condId;  
    EventState      state;
    EventMode       mode;
    Lock*           pLock;

} Event;

enum { EVENT_WAIT_INFINITE = 0xFFFFFFFF };

extern Event*   eventCreate(EventMode mode);
extern int      eventWait(Event* pInstance, unsigned int timeout_ms);
extern int      eventSignal(Event* pInstance);
extern int      eventReset(Event* pInstance);
extern void     eventDestroy(Event* pInstance);

#endif

