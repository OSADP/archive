#ifndef OBU_TimFilter_h
#define OBU_TimFilter_h

#include "QWarnDefs.h"
#include "Lock.h"

#include "TravelerInformation.h"

typedef struct TimFilter
{
    int     maxCount;
    uint64* ids;
    int     count;
    int     currentIndex;
    Lock*   pLock;

} TimFilter;

extern TimFilter*   timfCreate(int maxCount);
extern void         timfDestroy(TimFilter* pInstance);

extern int          timfIsNewMessage(TimFilter*, TravelerInformation_t* pMessage);

#endif
