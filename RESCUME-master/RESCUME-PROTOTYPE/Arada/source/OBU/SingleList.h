
#ifndef _SINGLELIST_H_
#define _SINGLELIST_H_

/*!
 * Generic singly linked list.  Any form of data can be stored at the node
 * using the 'value' element.
 */
typedef struct SingleList {
	struct SingleList *nextNode;
	void *value;
	void (*func_free)(void *);
	void (*func_print)(void *);
} SingleList;


/******************************************/
/********** NODE ONLY OPERATIONS **********/
/******************************************/

/*!
 * Allocates a new SingleList node and initializes elements.  
 * Optionally sets value ptr from parameter.
 * 
 * @param value
 *		Optional ptr to set the node's value element to. Ptr should be 
 * 		allocated with m/calloc.  Ptr will be freed upon call to 
 * 		singleList_nodeDestroy.
 *
 * @return
 * 		The newly allocated node.  NULL if error.
 */
SingleList *singleList_nodeCreate(void *value, void (*func_free)(void *));

/*!
 * Properly destroys a node.  Free's the value ptr if set (not null) and 
 * free's the node itself.  No more operations should be used with the node
 * after singleList_nodeDestroy is called.
 *
 * @param node
 *		Node to destroy.
 */
void singleList_nodeDestroy(SingleList *node) __attribute__((nonnull));


/******************************************/
/********** FULL LIST OPERATIONS **********/
/******************************************/

/*!
 * Returns the size (number of nodes) in the list.
 * 
 * @param list
 * 		The list to count the number of nodes in.
 * 
 * @return
 * 		The number of nodes in the list.
 */
int singleList_size(SingleList *list);

/*!
 * Inserts a node at the given position in a list.
 * 
 * @param list
 *		List to insert the node into.
 *
 * @param position
 *		Position to place the new node at.  
 *		Requires '0 <= position <= size(list)'
 *
 * @param node
 *		Node to insert.
 *
 */
void singleList_insert(SingleList **list, int position, SingleList *node) __attribute__((nonnull));


void singleList_reverse(SingleList **list) __attribute__((nonnull));

//TODO docs
SingleList *singleList_replace(SingleList **list, int position, SingleList *node);

//TODO docs
void singleList_replaceAndDestroy(SingleList **list, int position, SingleList *node);

/*!
 * Removes a node at a given position from a list.
 *
 * @param list
 * 		The list to remove the node from.
 *
 * @param position
 * 		The position to remove the node from.
 *		Requires '0 <= position <= size(list)'
 *
 * @return
 * 		A pointer to the removed node.  
 *		NULL if there is an error (such as position out of range).
 */
SingleList *singleList_remove(SingleList **list, int position) __attribute__((nonnull));

//TODO docs
void singleList_removeAndDestroy(SingleList **list, int position);

/*!
 * Destroy's an entire list.
 *
 * @param list
 * 		The list to destroy.
 */
void singleList_destroy(SingleList **list) __attribute__((nonnull));

/*!
 * Print's an entire list.
 *
 * @param list
 * 		The list to print.
 */
void singleList_printf(SingleList *list);

#endif
