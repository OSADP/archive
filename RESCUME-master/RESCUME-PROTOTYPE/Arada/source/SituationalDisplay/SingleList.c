

#include <stdio.h>
#include <stdlib.h>
#include "SingleList.h"

SingleList *singleList_nodeCreate(void *value, void (*func_free)(void *))
{
	//Malloc and initalize elements of the structure
	SingleList *node = (SingleList *)malloc(sizeof(SingleList));
	if (!node)
		return NULL;
	
	node->value = value;
	node->nextNode = NULL;
	node->func_print = NULL;
	node->func_free = func_free;
	
	return node;
}

void singleList_nodeDestroy(SingleList *node)
{
	if (!node)
		return;

	//Free value only if it exists
	if (node->value != NULL)
	{
		node->func_free(node->value);
	}
	free(node);
}


int singleList_size(SingleList *list)
{
	//Recursively count number of valid (non null) nodes.
	return (list == NULL) ? 0 : (1 + singleList_size(list->nextNode));
}

void singleList_insert(SingleList **list, int position, SingleList *node)
{
	//Insert at current location is position is zero or
	//the current position is null (end of the list)
	if (position <= 0 || *list == NULL)
	{
		node->nextNode = *list;
		*list = node;
	}
	else //Otherwise recursively move forward in list
	{
		singleList_insert(&(*list)->nextNode, position - 1, node);
	}
}

// Step 1:
//
// prev head  next
//   |    |    |
//   v    v    v
// NULL  [0]->[1]->[2]->NULL
//
// Step 2:
//
//      prev head  next
//        |    |    |
//        v    v    v
// NULL<-[0]  [1]->[2]->NULL
//
void singleList_reverse(SingleList **list)
{
	SingleList *prev = NULL;
	SingleList *head = *list;
	SingleList *next;

	while(head)
	{
		next = head->nextNode;
		head->nextNode = prev;
		prev = head;
		head = next;
	}

	*list = prev;
}

SingleList *singleList_replace(SingleList **list, int position, SingleList *node)
{
	//Insert at current location is position is zero or
	//the current position is null (end of the list)
	if (*list == NULL || position < 0)
		return NULL;
	else if (position == 0)
	{
		SingleList *results = *list;

		node->nextNode = (*list)->nextNode;
		*list = node;

		results->nextNode = NULL;
		return results;
	}
	else //Otherwise recursively move forward in list
	{
		return singleList_replace(&(*list)->nextNode, position - 1, node);
	}
}

void singleList_replaceAndDestroy(SingleList **list, int position, SingleList *node)
{
	//Insert at current location is position is zero or
	//the current position is null (end of the list)
	if (*list == NULL || position < 0)
		return;
	else if (position == 0)
	{
		SingleList *results = *list;

		node->nextNode = (*list)->nextNode;
		*list = node;

		results->nextNode = NULL;
		singleList_nodeDestroy(results);
	}
	else //Otherwise recursively move forward in list
	{
		singleList_replaceAndDestroy(&(*list)->nextNode, position - 1, node);
	}
}

SingleList *singleList_remove(SingleList **list, int position)
{
	//Return NULL (error) if list is NULL (pass end of list) or
	//position is less then zero (before beginning of list)
	if (*list == NULL || position < 0)
		return NULL;
	else if (position == 0) //Remove node, relink list, and return node.
	{
		SingleList *results = *list;
		*list = (*list)->nextNode;

		//Ensure that the removed node isn't pointing to it's next
		//node now that it is out of the list.
		results->nextNode = NULL;
		
		return results;
	}
	else //Otherwise recursively move forward in list
	{
		return singleList_remove(&(*list)->nextNode, position - 1);
	}
}

void singleList_removeAndDestroy(SingleList **list, int position)
{
	//Return NULL (error) if list is NULL (pass end of list) or
	//position is less then zero (before beginning of list)
	if (*list == NULL || position < 0)
		return;
	else if (position == 0) //Remove node, relink list, and return node.
	{
		SingleList *results = *list;
		*list = (*list)->nextNode;

		//Ensure that the removed node isn't pointing to it's next
		//node now that it is out of the list.
		results->nextNode = NULL;
		singleList_nodeDestroy(results);
	}
	else //Otherwise recursively move forward in list
	{
		return singleList_removeAndDestroy(&(*list)->nextNode, position - 1);
	}
}

void singleList_destroy(SingleList **list)
{
	if (*list == NULL)
		return;
	
	//Recursivly destroy the next node in the list before destroying
	//this node.  singleList_destroy will automatically set the passed
	//in ptr to NULL, so no need to do it here.
	singleList_destroy(&(*list)->nextNode);
	singleList_nodeDestroy(*list);
	*list = NULL;
}

void singleList_printf(SingleList *list)
{	
	int count = 0;
	printf("\n\nNODE NUMBER\t\tVALUE\n");
	printf("--------------------------------------\n");
	
	//This is the only iterative method because of the
	//need to print out the header only once.  "Recursiveness" is
	//achieved by just simply setting 'list' param to the next node at the
	//end of each iteration.
	while(list != NULL)
	{
		printf("%8d\t|\t", count);
		//If the func_print is not set, then just print the ptr to the value.
		if (list->func_print == NULL || list->value == NULL)
		{
			printf("ptr: %p\n", list->value);
		}
		else //Otherwise use the nice print function for that value.
		{
			list->func_print(list->value);
			printf("\n");
		}
		
		//Move to the next list before iterating
		list = list->nextNode;
		count++;
	}
	printf("\n");
}
