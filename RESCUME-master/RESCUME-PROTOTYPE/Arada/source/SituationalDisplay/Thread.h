
#ifndef _THREAD_H_
#define _THREAD_H_

#include <pthread.h>
#include "Lock.h"

typedef struct {
	pthread_t thread;
	Lock *lock;
	volatile int running;
} Thread;

Thread *thread_create();
void thread_start(Thread *thread, void *(*func)(void *), void *arg);
void *thread_join(Thread *thread);
int thread_isRunning(Thread *thread);
void thread_destroy(Thread *thread);

#endif
