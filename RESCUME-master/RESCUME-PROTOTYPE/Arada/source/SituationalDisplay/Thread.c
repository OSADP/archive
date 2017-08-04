
#include "Thread.h"
#include <stdlib.h>

Thread *thread_create()
{
	Thread *results = (Thread*)malloc(sizeof(Thread));
	if (!results)
		return NULL;

	if (!(results->lock = lock_create()))
	{
		thread_destroy(results);
		return NULL;
	}

	results->running = 0;

	return results;
}

void thread_start(Thread *thread, void *(*func)(void *), void *arg)
{
	if (!thread)
		return;

	lock_lock(thread->lock);

	if (!thread->running)
	{
		thread->running = 1;
		pthread_create(&thread->thread, NULL, func, arg);
	}

	lock_unlock(thread->lock);
}

void *thread_join(Thread *thread)
{
	if (!thread)
		return NULL;

	lock_lock(thread->lock);

	void *results = NULL;
	if (thread->running)
	{
		thread->running = 0;
		pthread_join(thread->thread, &results);
	}

	lock_unlock(thread->lock);

	return results;
}

int thread_isRunning(Thread *thread)
{
	return thread->running;
}

void thread_destroy(Thread *thread)
{
	if (!thread)
		return;

	thread_join(thread);
	if (thread->lock)
		lock_destroy(thread->lock);
	free(thread);
}
