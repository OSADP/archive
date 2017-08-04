/*************************************************************************
 * 
 * SCSC CONFIDENTIAL
 * __________________
 * 
 * Copyright (c) [2009] - [2012] 
 * SubCarrier System Corp. (SCSC) 
 * All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is, and remains,
 * the property of SubCarrier System Corp. (SCSC) and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to SubCarrier System Corp. (SCSC)
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from SubCarrier System Corp. (SCSC).
 *
 *
 * This file is subject to the terms and conditions of use defined 
 * in the files 'LICENSE.rft' or 'LICENSE.pdf' which is part of this 
 * source code package.
 *
 * LIC: #Battelle_001_01_dbfff42a90727d02153511a33480572b#
 */

/*-
 * Copyright (c) 2003, 2004 Lev Walkin <vlm@lionet.info>. All rights reserved.
 * Redistribution and modifications are permitted subject to BSD license.
 */

#include "stdafx.h" 
#include <asn_internal.h>
#include <asn_SET_OF.h>
#include <errno.h>

/*
 * Add another element into the set.
 */
int
asn_set_add(void *asn_set_of_x, void *ptr) {
	asn_anonymous_set_ *as = _A_SET_FROM_VOID(asn_set_of_x);

	if(as == 0 || ptr == 0) {
		errno = EINVAL;		/* Invalid arguments */
		return -1;
	}

	/*
	 * Make sure there's enough space to insert an element.
	 */
	if(as->count == as->size) {
		int _newsize = as->size ? (as->size << 1) : 4;
		void *_new_arr;
		_new_arr = REALLOC(as->array, _newsize * sizeof(as->array[0]));
		if(_new_arr) {
			as->array = (void **)_new_arr;
			as->size = _newsize;
		} else {
			/* ENOMEM */
			return -1;
		}
	}

	as->array[as->count++] = ptr;

	return 0;
}

void
asn_set_del(void *asn_set_of_x, int number, int _do_free) {
	asn_anonymous_set_ *as = _A_SET_FROM_VOID(asn_set_of_x);

	if(as) {
		void *ptr;
		if(number < 0 || number >= as->count)
			return;

		if(_do_free && as->free) {
			ptr = as->array[number];
		} else {
			ptr = 0;
		}

		as->array[number] = as->array[--as->count];

		/*
		 * Invoke the third-party function only when the state
		 * of the parent structure is consistent.
		 */
		if(ptr) as->free(ptr);
	}
}

/*
 * Free the contents of the set, do not free the set itself.
 */
void
asn_set_empty(void *asn_set_of_x) {
	asn_anonymous_set_ *as = _A_SET_FROM_VOID(asn_set_of_x);

	if(as) {
		if(as->array) {
			if(as->free) {
				while(as->count--)
					as->free(as->array[as->count]);
			}
			FREEMEM(as->array);
			as->array = 0;
		}
		as->count = 0;
		as->size = 0;
	}

}



//
// SCSC Added
// Set the inital contents of the set (the list) to be all zeros
// do NOT set the _asn_ctx element to a null value, (is never used)
// which must by set by the caller as in listOwner->_asn_ctx.ptr = NULL
//
void asn_set_init(void *listOwner)
{
	asn_anonymous_set_ *as = (asn_anonymous_set_ *)(listOwner);
	if(as) {
		// set its elements to zero states for now
		as->count = 0;		// no current members
		as->size = 0;		// not allocated members
		as->array = NULL;	// nothing pointed to
		as->free = NULL;	// no tear down call function for type
	}
}

//
// SCSC Added
// Get Count of list contents
//
int asn_set_getCnt(void *listOwner)
{
	asn_anonymous_set_ *as = (asn_anonymous_set_ *)(listOwner);
	if(as) return (as->count);
	return (-1);
}

//
// SCSC Added
// Get pointer to Nth entry, if it exists
//
void* asn_set_getPtr(void *listOwner, int index)
{
	asn_anonymous_set_ *as = (asn_anonymous_set_ *)(listOwner);
	if(as) 
	{
		if (as->count > index) // if present
		{
			return (as->array[index]);
		}
	}
	return (NULL);
}