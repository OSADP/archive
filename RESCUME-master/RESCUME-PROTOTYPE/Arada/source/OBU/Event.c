#include "Event.h"

#include <stdlib.h>
#include <stdbool.h>
#include <errno.h>
#include <sys/time.h>

typedef unsigned long long uint64;

Event* event_create(EventMode mode)
{
	Event* pInstance = (Event*)malloc(sizeof(Event));
	
	pInstance->state = EVENT_RESET;
	pInstance->pLock = lock_create();
	pInstance->mode = mode;
	pthread_cond_init(&pInstance->condId, 0);
	
	return pInstance;
}

int event_wait(Event* pInstance, unsigned int timeout_ms)
{
	if(timeout_ms == 0)
	{
		if(lock_tryLock(pInstance->pLock) != 0)
		{
			return false;
		}
	}
	else // timeout_ms > 0
	{    
		if(lock_lock(pInstance->pLock) != 0)
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
	
	if(lock_unlock(pInstance->pLock) != 0)
	{
		return false;
	}
	
	return (result == 0);
}

int event_signal(Event* pInstance)
{
	if(lock_lock(pInstance->pLock) != 0)
	{
		return false;
	}
	
	pInstance->state = EVENT_SET;
	
	if(lock_unlock(pInstance->pLock) != 0)
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

int event_reset(Event* pInstance)
{
	if(lock_lock(pInstance->pLock) != 0)
	{
		return false;
	}
	
	pInstance->state = EVENT_RESET;
	
	if(lock_unlock(pInstance->pLock) != 0)
	{
		return false;
	}
	
	return true;
}

void event_destroy(Event* pInstance)
{
	lock_destroy(pInstance->pLock);
	
	pthread_cond_destroy(&pInstance->condId);
	
	free(pInstance);
	
	return;
}

