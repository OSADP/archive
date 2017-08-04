
#include "Queue.h"
#include <stdlib.h>

Queue *queue_create(void (*func_free)(void *))
{
	Queue *results = (Queue *)calloc(1, sizeof(Queue));
	if (!results)
		return NULL;

	results->func_free = func_free;

	//Create and insert virtual front of queue
	SingleList *node = singleList_nodeCreate(NULL, free);
	if (!node)
	{
		queue_destroy(results);
		return NULL;
	}
	singleList_insert(&results->front, 0, node);
	results->back = results->front;

	return results;
}

void queue_destroy(Queue *queue)
{
	singleList_destroy(&queue->front);
	free(queue);
}

void queue_push(Queue *queue, void *value)
{
	SingleList *node = singleList_nodeCreate(value, queue->func_free);
	if (!node)
		return; //TODO: ERROR

	queue->count++;
	singleList_insert(&queue->back, 1, node);
	queue->back = queue->back->nextNode;
}

void *queue_pop(Queue *queue)
{
	if (queue->front == queue->back)
	{
		return NULL;
	}
	
	queue->count--;
	singleList_removeAndDestroy(&queue->front, 0);

	void *results = queue->front->value;
	queue->front->value = NULL;
	return results;
}
