/* Circular buffer example, keeps one slot open */
 
#include <stdio.h>
#include <malloc.h>
#include "pathhist_defs.h"
 
/* Opaque buffer element type.  This would be defined by the application. */
 
void cbInit(CircularBuffer *cb, int size,void * buff) {
    cb->size  = size ; 
    cb->start = 0;
    cb->end   = 0;
    cb->count   = 0;
    cb->elems = buff;
}
 
void cbFree(CircularBuffer *cb) {
    cb->elems=NULL;
    cb->count = 0;
    cb->size  = 0 ; 
    cb->start = 0;
    cb->end   = 0;
}
 
int cbIsFull(CircularBuffer *cb) {
    return cb->count == cb->size; 
}
 
int cbIsEmpty(CircularBuffer *cb) {
    return cb->count == 0; 
}
 
/* Write an element, overwriting oldest element if buffer is full. App can* Write an element, overwriting oldest element if buffer is full. App can
   choose to avoid the overwrite by checking cbIsFull(). */
void cbWrite(CircularBuffer *cb, ElemType *elem) {
    int end = (cb->start + cb->count) % cb->size;
    cb->elems[end] = *elem;
    //printf("wr: st %d e %d [%f] -> ", cb->start, cb->count, cb->elems[end].value);
   // printf("wr: st %d c%d e %d [%lf %lf %lf] -> ", cb->start, cb->count,cb->end, cb->elems[end].lat, cb->elems[end].lon, cb->elems[end].distPrev);
   //  printf("%lf,%lf\n",cb->elems[end].lat,cb->elems[end].lon);
    if (cb->count == cb->size)
        cb->start = (cb->start + 1) % cb->size; /* full, overwrite */
    else
        ++ cb->count;

    cb->end = (cb->end + 1 ) % cb->size;
   // printf("st %d c%d e %d \n", cb->start, cb->count, cb->end);
}
 
/* Read oldest element. App must ensure !cbIsEmpty() first. */
void cbRead(CircularBuffer *cb, ElemType *elem) {
//    printf("rd: st %d e %d -> ", cb->start, cb->count);
    *elem = cb->elems[cb->start];
    cb->start = (cb->start + 1) % cb->size;
    -- cb->count;
//    printf("st %d e %d \n", cb->start, cb->count);
}

void cbPeek(CircularBuffer *cb, int *newest) {

    *newest = cb->end;

    if (*newest-1 < 0) {
	*newest = cb->size-1;
    }
    else {
	*newest = cb->end - 1;
    }

}

void getindexes(CircularBuffer *cb, double sumvalue, int *indexes, int *num) {
    double sum=0.0;
    int sindex;
    int iindex=0, i, eindex, numelementsparsed=0;

    if (cbIsEmpty(cb)) {
	    //printf("Buffer empty\n");
		*num = 0;
	    return;
    }

    // clear indexes of buffer size
    for (i=0;i<cb->size;i++) {
	    indexes[i] = 0;
    }

    // first point
    sindex = cb->end;
    	// get next index
    	if (sindex-1 < 0) {
		sindex = cb->size-1;
    	}
    	else {
		sindex = (sindex - 1 );
    	}
    indexes[iindex++] = sindex;
    numelementsparsed++;
    *num = 1; 
	//printf(">%lf %lf %lf %lf %lf %d\n", sum, cb->elems[sindex].distPrev, cb->elems[sindex].course, cb->elems[sindex].lat,cb->elems[sindex].lon,*num);

    // compute from next point
    do {

    // check if next point is the last point
	numelementsparsed++;
	// all elements parsed condition
	if (numelementsparsed > cb->count)
		break;

	// add the first point to sum
	//if (sindex != cb->start)
	sum = (double)sum + cb->elems[sindex].distPrev;

	// parse next element only if available
	
    	// get next index
    	if (sindex-1 < 0) {
		sindex = cb->size-1;
    	}
    	else {
		sindex = (sindex - 1 );
    	}

	*num = *num + 1;
	// add the current(i.e next) index, next index added to list but not included in sum
	indexes[iindex++] = sindex;

	//printf(">%lf %lf %lf %lf %d\n", sum, cb->elems[sindex].distPrev, cb->elems[sindex].lat,cb->elems[sindex].lon,*num);
        //printf("%d(%d) ", iindex, sindex);
        
    } while ((sum < (double)sumvalue));


    if (sum >= sumvalue) {
	    // indexes found
	    //printf("sum %lf found in %d indexes\n", sumvalue, *num);
    }
    else {
	if (cb->count == cb->size)
	{ // max (=23) points available
	      //  printf("sum %lf not found but buffer full, so send %d indexes\n", sumvalue, *num);
    		// set all indexes of buffer size, as buffer full
		*num = cb->size;
		sindex = cb->start;
    		for (i=cb->size-1;i>=0;i--) {
	    		indexes[i] = sindex;
			sindex = (sindex + 1) % cb->size; /* full, overwrite */
    		}
	}
	else {
	     //   printf("%d indexes\n", *num);
    		// clear indexes of buffer size, as sum not found
#if 0
		*num = 0;
    		for (i=0;i<cb->size;i++) {
	    		indexes[i] = 0;
    		}
#endif
	}
    }

 

}
