
#ifndef _QUEUE_H_
#define _QUEUE_H_

#include "SingleList.h"

typedef struct {
	SingleList *front;
	SingleList *back;
	int count;
	void (*func_free)(void *);
} Queue;

Queue *queue_create(void (*free_func)(void *));
void queue_destroy(Queue *queue);

void queue_push(Queue *queue, void *value);
void *queue_pop(Queue *queue);
void *queue_front(Queue *queue);
void *queue_back(Queue *queue);
SingleList *queue_getUnderlyingList(Queue *queue);

#endif
