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

//
// This code it more or less a collected extract of useful things that
// Battelle will want to use that were not parts of the 'core' ASN only
// development code modules.  These additional functions are added to the
// basic toolkit under the terms of use described elsewhere
//
//

#include "stdafx.h"
#include <asn_internal.h>


#include "stdafx.h"
#include "DSRC_ASN_Utils.h"
#include <stdio.h>
#include <cstdio>  // RESOLVE this is the real C++ one, do not need both!!!
// #include <tchar.h>
#include <math.h>
#include <float.h>
#include <assert.h>
#include <errno.h>
#include <time.h>
#include <iostream>

// share.h is Needed on MinGW and GCC (on Windows), 
// share.h is Not needed in VS200x (on Windows), does not seem to harm
// share.h is Needed on Ubuntu and GCC
#include <share.h>

using namespace std;    // access to cout stream


//
// Taken from the SCSC ASN helper code base
//

// Given one of the ASN1c tools IA5String  structures
// return an int
// representing that value (BIG endian logic used
// long  IA5str2Num(TBD)

// Given one of the ASN1c tools OCTET or IA5String structures
// return it placed into the passed string, and with a
// final null termination.  The number of chars on the whole
// string (includnig termination) is returned
// NOTE use this call because things like the below is NOT always
// safe in an ASN.1 buffer when/if no termination is present:
// myCharPtr = (char *)theApproach->name->buf

int  Octet2Chars(OCTET_STRING_t* basePtr, char* pDesc, int maxCnt)
{
    int i;
    if (basePtr != NULL) // if pointer is valid (null string == no pointer)
    {
        // copy it over one char at a time
        for (i=0; i<basePtr->size; i++)
        {
            if (i == maxCnt) // ran out of pDesc string
            {
                pDesc[i-1] = 0;
                return(i);
            }
            pDesc[i] = basePtr->buf[i];
        }
        pDesc[i] = 0;
    }
    else
        pDesc[0] = 0;  // '\n' to terminate the destination
    return(i);
}


// Given one of the ASN1c tools octet buffer structures
// which has a packed int value in it, return an int
// representing that value (BIG endian logic used)
long  Octet2Num(OCTET_STRING_t* basePtr)
{
    long value = 0; // the returned value
    if (basePtr != NULL) // if pointer is valid (silent on no pointer)
    {
        // DEBUG, get each item
        int a = *(basePtr->buf+0);
        int b = *(basePtr->buf+1);
        int c = *(basePtr->buf+2);
        int d = *(basePtr->buf+3);

        switch (basePtr->size)
        {
        case 1:
            {   // A One byte value
            value = *(basePtr->buf);
            break;
            }
        case 2:
            {   // A Two byte value
            value =  256 * ( *(basePtr->buf+0) ) +
                       1 * ( *(basePtr->buf+1) );
            break;
            }
        case 3:
            {   // A Three byte value
            value =  65536 * ( *(basePtr->buf+0) ) +
                       256 * ( *(basePtr->buf+1) ) +
                         1 * ( *(basePtr->buf+2) );
            break;
            }
        case 4:
            {   // A Four byte value
            value =  16777216 * ( *(basePtr->buf+0) ) +
                        65536 * ( *(basePtr->buf+1) ) +
                          256 * ( *(basePtr->buf+2) ) +
                            1 * ( *(basePtr->buf+3) );
            break;
           }
        default:
            cout << endl << "ERROR: Attempt on convert more then 5 octets to an integer" << endl;
        }
    }
    return(value);
}


// Given one the ASN 'list' objects used to hold sequence of things
// set all the values to null or zero, when first created ASN leaves these values
// in a random state that can cause problems, use only on new objects!
// use the ASN calls to add and remove items (see ASN_SEQUENCE_OF.h)
void ZeroNewList(void* theList) // (A_SEQUENCE_OF(type)* thePtr)
{   // Any use valid use of the list will do for the cast
    // note that the list and the parser context are in this structure
    // make sure you pass the holder and not the list itself
    Approaches* thePtr = (Approaches* ) theList; // cast it
    thePtr->list.array   = NULL;
    thePtr->list.free    = NULL;
    thePtr->list.count   = 0;
    thePtr->list.size    = 0;
}




// A simple helper call to append a well formed string, macro this?
void Append(const char* desc, const char* source)
{
	char* pD = (char *)desc;
	char* pS = (char *)source;
    strncat(pD, pS, strlen(pS));
	// why not just use strcat for this???
}


// Return only the path part of a full path name and file name
void JustPath(char* fullFilePath, char* tersePath)
{
	if ((fullFilePath != NULL) && (tersePath != NULL))
	{	// seek for "/" from the right side (PC use only for now)
		char* theEnd = strrchr(fullFilePath,'/');
		if (theEnd != NULL)
		{
			strncpy(tersePath, fullFilePath, (theEnd-fullFilePath));
			tersePath[theEnd-fullFilePath] = 0; // terminate it
		}
	}
}



// Given one of the ASN1c tools OCTET or IA5String structures
// return it placed into the passed string, and with a
// final null termination in the form where each byte is
// expressed in a Hex value with a "," between them
// if flag is set, returned value will be and lead by an "0x"
// and example would be:  0x00,01,02,03, ... FD, FE, FD
int  Octet2HexAsChars(OCTET_STRING_t* basePtr, char* pDesc, int maxCnt, bool add0x)
{
    // DCK July 30th, reworked to remove use of itoa() in favor of
    // the use of snprintf() / sprintf() in order
    // to be more portable and to handle negative input values
    char theValue = 0;
    char realValue[3] = {0,0,0};
#define ROOM_FOR_NEG 10
    char theValueAsText[ROOM_FOR_NEG]; // always leave room for termination
    int i = 0; // source data index
    int j = 0; // desc string index (moves 3 for each byte)
    if ((basePtr != NULL) && (pDesc != NULL)) // if pointers are valid (we have data and buffer)
    {
        if (add0x == true)
        {   // append starting chars
            pDesc[j++] = '0';
            pDesc[j++] = 'x';
        }
        for (i=0; i<basePtr->size; i++)
        {   // Do the work here. Get data and convert
            theValue = basePtr->buf[i];
			// RESOLVE BELOW PROBLEM in g++ code use
            snprintf(theValueAsText, ROOM_FOR_NEG, "%9X", theValue);
			sprintf(theValueAsText, "%9X", theValue);
			// RESOLVE Above line is a temp solution

            // Pick off the last two chars for use, ignore leading FFFF when negative
            realValue[0] = theValueAsText[ROOM_FOR_NEG-3];
            realValue[1] = theValueAsText[ROOM_FOR_NEG-2];
            // If value is positive but less then 16 (0x10), stuff a zero to upper char
            if ((theValue > -1) && (theValue < 16))  realValue[0] = '0';

            // DEBUG
            //char temp[50];
            //cout << "Given: " << itoa(theValue,temp,16) << " becomes: " << theValueAsText << endl;
            //cout << "Start: " << itoa(theValue,temp,16) << " end: " << realValue << endl;

            // Now stuff this value to the string we are building
            pDesc[j++] = realValue[0];
            pDesc[j++] = realValue[1];
            pDesc[j++] = ',';
            if (j > maxCnt-3) // ran out of pDesc string, punt
            {
                pDesc[j-1] = 0;
                return(j);
            }
        } // for each byte in the octet array
        pDesc[j--] = 0; // terminate it
        pDesc[j]   = 0; // drop last comma off
    }
    else
        pDesc[0] = 0;  // '\n' to terminate the destination
    return(i);
}


// Given a string representing hex values and commas, stuff it as ASN octets
// until termination is reached, return the count of bytes
int HexAsChars2OctetBuff(OCTET_STRING_t* basePtr, char* pSource)
{
    if ((basePtr != NULL) && (pSource != NULL)) // if valid looking
    {
#define MAX_STRING_LEN (12*3) // in fact 8 is as many as we can have in an offset
        uBYTE theArray[MAX_STRING_LEN];
        char theData[3] = {0,0,0};
        int cnt = 0;
        int i = 0; // the source loop index
        int j = 0; // the start point
        int k = 0; // the desc index
        // DEBUG: cout << pSource << endl;
        // detect (and skip over) any leading "0x" value
        if ((pSource[i] == '0') && (pSource[i+1] == 'x'))  j = 2;

        for(int i=0; i<MAX_STRING_LEN; i++) // find end of source
        {
            if (pSource[i+j] != 0) cnt++;
            else i = MAX_STRING_LEN;
        }
        if ((cnt+1)%3 != 0) // if we have an odd size
            cout << "CAUTION: Hex String to octets has odd length, is " << cnt << endl;

        for(int i=0; i<cnt; i=i+3) // process each entry
        {
            theData[0]    = pSource[i+j];
            theData[1]    = pSource[i+j+1];
            if ((pSource[i+j+2] != ',') && (pSource[i+j+2] != 0))
                cout << "CAUTION: Hex String delimiter [" << pSource[i+j+2] << "] is not expected comma at " << i+j+2 << endl;
            theArray[k++] = htoi(theData);
        }

        // from the ASN tool lib (handles all buffer work for us, including re-allocates)
        // This ASN tool tends to be rather picky about its own memory space
        // int OCTET_STRING_fromBuf(OCTET_STRING_t *s, const char *str, int size);
        i = OCTET_STRING_fromBuf(basePtr, (const char *)theArray, k);
        if      (i == 0) return (basePtr->size);
        else if (i == -1) return (-1);
        else return (i);  // only zero and -1 are valid
    }
    return(-1);
}

// Given a char pointer to data, place its contents into the
// all purpose octet struc used by ASN for all type of bytes
// any termination in the source will be copied over as well,
// often this is not a useful byte in ASN and could be dropped
// return the number of chars copied or -1 if bad outcome
// presumes OCTET_STRING_t pointer is valid
int  Chars2OctetBuff(OCTET_STRING_t* basePtr, char* pSource, int cnt)
{
    if ((cnt > 0) && (pSource != NULL)) // if we have anything to copy from
    {
        // from the ASN tool lib (handles all buffer work for us, including re-allocates)
        // This ASN tool tends to be rather picky about its own memory space
        // int OCTET_STRING_fromBuf(OCTET_STRING_t *s, const char *str, int size);
        int i = OCTET_STRING_fromBuf(basePtr, pSource, cnt);
        if (i == -1) return (-1);
        else return (i);

        /*** Below works but on a delete and re-allocate will cause problems
        if (basePtr->size > 0) delete (basePtr->buf); // delete any current contents found
        basePtr->buf = new uint8_t(cnt); // allocate new space for actual bytes
        for(int i=0; i<cnt; i++)  // copy it all over
            basePtr->buf[i] = (uint8_t)pSource[i];
        basePtr->size = cnt; // set size/count value
        return(cnt);
        ***/
    }
    else return(-1);
}


// Same thing, but bytes rather then chars, one just calls the other
// The user is presumed to be smart enough to know that an ASCII
// value is not a byte value, this call exists just for naming value
int  Bytes2OctetBuff(OCTET_STRING_t* basePtr, char* pSource, int cnt)
{
    return( Chars2OctetBuff(basePtr, pSource, cnt) );
}


// Given an int, determine how many bytes it needs,
// and then stuff it as an ASN octet with as few bytes as possible
// deal with the sign issues to remove unneeded bytes
// returns the number of bytes used (1,2,3,4),
// note that "zero" is stuffed as one byte,
// test before calling if this is not wanted for optional content
int Long2OctetBuff(OCTET_STRING_t* basePtr, long theValue)
{
    // make a 'string' from  theValue first
    // TEMP PUNT WORK FOR NOW, do an if by range here
    uBYTE theData[4];
	// DCK Sept 26 patch 'byte' to BYTE in below
	// DCK Oct 1 path BYTE to be uBYTE as part if clean up
    int cnt = 1;
    if (basePtr != NULL) // if we have anything to copy into
    {
        // determine how big from the value, then stuff bytes
        long i = abs(theValue);
        if (i < 128) // one byte
        {
            theData[0] = (uBYTE)((0x000000FF & theValue) >> 0);
            cnt = 1;
        }
        else if (i < 32768) // two bytes
        {
            theData[0] = (uBYTE)((0x0000FF00 & theValue) >> 8);
            theData[1] = (uBYTE)((0x000000FF & theValue) >> 0);
            cnt = 2;
        }
        else if (i < 8388608) // three bytes
        {
            theData[0] = (uBYTE)((0x00FF0000 & theValue) >> 16);
            theData[1] = (uBYTE)((0x0000FF00 & theValue) >> 8);
            theData[2] = (uBYTE)((0x000000FF & theValue) >> 0);
            cnt = 3;
        }
        else // presume 4 bytes
        {
            theData[0] = (uBYTE)((0xFF000000 & theValue) >> 24);
            theData[1] = (uBYTE)((0x00FF0000 & theValue) >> 16);
            theData[2] = (uBYTE)((0x0000FF00 & theValue) >> 8);
            theData[3] = (uBYTE)((0x000000FF & theValue) >> 0);
            cnt = 4;
        }
        // from the ASN tool lib (handles all buffer work for us, including re-allocates)
        // This ASN tool tends to be rather picky about its own memory space
        // int OCTET_STRING_fromBuf(OCTET_STRING_t *s, const char *str, int size);
        i = OCTET_STRING_fromBuf(basePtr,(const char *)theData, cnt);
        if      (i == 0) return (basePtr->size);
        else if (i == -1) return (-1);
        else return (i);  // only zero and -1 are valid
    }
    else return(-1);
}



//
// Various byte reversal calls, used to align ASN and Intel multi-word arrays
//


// reverse a 2 byte array
void Swap2bytes (uBYTE * ptr)
{
	uBYTE temp;
	temp	= ptr[0];
	ptr[0]	= ptr[1];
	ptr[1]	= temp;
}

// reverse a 3 byte array
void Swap3bytes (uBYTE * ptr)
{
	uBYTE temp;
	temp	= ptr[0];
	ptr[0]	= ptr[2];  // note that [1] is not changed
	ptr[2]	= temp;
}

// reverse a 4 byte array
void Swap4bytes (uBYTE * ptr)
{
	uBYTE temp;
	temp	= ptr[0];
	ptr[0]	= ptr[3];
	ptr[3]	= temp;
	temp	= ptr[1];
	ptr[1]	= ptr[2];
	ptr[2]	= temp;
}

// reverse an N byte array
void SwapNbytes (uBYTE * ptr, int byteCnt)
{
	// with more bytes we do the classic loop method.
	uBYTE temp;
	int i, j;
	for ( i = 0, j = byteCnt-1; i < j; i++, j-- )
		{
		temp	= ptr[i];
		ptr[i]	= ptr[j];
		ptr[j]	= temp;
		}
}



// Do a 16 bit CRC-CCITT using simple table method
// For further details look up CRC in your favorite C guide or see
// Numerical Recipes in C by Press, Teukolsky, Vetterling and Flannery for details
// but there are typos in that text, they are corrected here
// x16 + x12 + x5 + 1 (X.25, V.41, CDMA, Bluetooth, XMODEM, HDLC,PPP, IrDA, BACnet; known as CRC-CCITT, MMC,SD)

typedef unsigned char uchar;
#define crcLOBYTE(x) ((uchar)((x) & 0xFF))
#define crcHIBYTE(x) ((uchar)((x) >> 8))


// Given a stream of bytes, return the new CRC
// this is just a wrapper to allow cleaner calling in the DSRC work
// stuff the returned value in the last two bytes of messages that need it
unsigned short DoCRC (uBYTE * pBYTES, int theLen)
{
	return ( icrc (0, (unsigned char*) pBYTES, theLen, 0, -1) );
}


// Given a reminder up to now, return the new CRC after one char is added
// used to init the tables we will use.
unsigned short icrc1 (unsigned short crc, unsigned char onech)
{
	int i;
	unsigned short ans=(crc ^ onech << 8);
	for (i=0; i<8; i++)
	{
		if (ans & 0x8000)
			ans = (ans <<= 1) ^ 4129;
		else
			ans <<= 1;
	}
	return ans;
}

// return a 16 bit value using CRC-CCITT poly
// compute a 16 bit CRC for array bufptr of length len,
// call with with jinit=0 and jrev=-1 and crc=0 to compute a message CRC
// call with with jinit=-1 and jrev=-1 and crc=themessage CRC to check a message CRC (correct result =0)
unsigned short icrc(unsigned short crc, unsigned char* pBuffer, unsigned long len, short jinit, int jrev)
{
	//unsigned short icrc1(unsigned short crc, unsigned char onech);
	static unsigned short icrctb[256];
	static bool init=false;
	static uchar rchr[256];
	unsigned short j,cword=crc;
	static uchar it[16] = {0,8,4,12,2,10,6,14,1,9,5,13,3,11,7,15}; // table of 4 bit reverses
	if (init == false)
	{	// cause a table build the first time we are run
		init = true;
		// build the two tables (one is for bit reverse cases)
		for (j=0; j<256; j++)
		{
			icrctb[j] = icrc1( j << 8, (uchar)0 );
			rchr[j] = (uchar) ( it[j & 0xF] << 4 | it[j >> 4] );
		}
	}
	if (jinit >= 0)
		cword = ((uchar) jinit) | (((uchar) jinit) << 8);
		// initialize the remainder register
	else if (jrev < 0)
		cword = rchr[crcHIBYTE(cword)] | rchr[crcLOBYTE(cword)] << 8;
		// if not initialize, do we reverse the register?
	for (j=0; j<len; j++)
	{
		int i;
		i = (jrev < 0 ? rchr[pBuffer[j]] : pBuffer[j]);
		i = ((jrev < 0 ? rchr[pBuffer[j]] : pBuffer[j]) ^ crcHIBYTE(cword)) ^ crcLOBYTE(cword) << 8;
		// do main loop over the char array
		cword = icrctb[(jrev < 0 ? rchr[pBuffer[j]] : pBuffer[j]) ^ crcHIBYTE(cword)] ^ crcLOBYTE(cword) << 8;
	}
	return (jrev >= 0 ? cword : rchr[crcHIBYTE(cword)] | rchr[crcLOBYTE(cword)] << 8); // do we need to reverse the output
}





// ASN Support calls for DSCR Rev 36 uses
// ASN Support calls for DSCR Rev 36 uses
// ASN Support calls for DSCR Rev 36 uses
// Move to a file called ASN_R36_Util.h when mature
// All the nasty bit-bytes and pointer setup items are found here
// need to make the ASN1c output useful to an app developer




// Return an allocated by empty intersection object
// Assign the passed value to the ID octets, all else as null
// Used to create a starting point with pointers set to null
Intersection* ASN_CreateEmptyIntersection(long theId)
{
    Intersection* thePtr = new Intersection;
    Long2OctetBuff(&thePtr->id, theId);
    thePtr->orientation    = NULL;
    thePtr->preemptZones   = NULL;
    thePtr->priorityZones  = NULL;
    thePtr->refInterNum    = NULL;
    thePtr->refPoint       = NULL;
    thePtr->type           = NULL;
    thePtr->laneWidth      = NULL;
    thePtr->name           = NULL;
    ZeroNewList(&thePtr->approaches);
    return(thePtr);
}





// Return an allocated but empty approach object with requested
// empty in-bound and/or out-bound approach object stubs
// and all the approach lane content types of each set to null values
// typically the object pointed to is added to an intersection
// once additional lanes are added and completed
ApproachObject* ASN_CreateEmptyApproachObj(bool hasInbound, bool hasOutbound)
{
    ApproachObject* thePtr          = new ApproachObject;
    thePtr->approach                = NULL; // inbound side
    thePtr->egress                  = NULL; // outbound side
    thePtr->laneWidth               = NULL;
    thePtr->refPoint                = NULL;
    // if requested to add inbound lanes types
    if (hasInbound == true)
    {
        thePtr->approach                = new Approach;
        thePtr->approach->barriers      = NULL;
        thePtr->approach->computedLanes = NULL;
        thePtr->approach->crosswalks    = NULL;
        thePtr->approach->drivingLanes  = NULL;
        thePtr->approach->trainsAndBuses = NULL;
        thePtr->approach->name          = NULL;
        thePtr->approach->id            = NULL;
    }
    // if requested to add outbound lanes types
    if (hasOutbound == true)
    {
        thePtr->egress                  = new Approach;
        thePtr->egress->barriers        = NULL;
        thePtr->egress->computedLanes   = NULL;
        thePtr->egress->crosswalks      = NULL;
        thePtr->egress->drivingLanes    = NULL;
        thePtr->egress->trainsAndBuses  = NULL;
        thePtr->egress->name            = NULL;
        thePtr->egress->id              = NULL;
    }
    return(thePtr);
}
// Create a dispose call in time as well.




// Return an allocated but empty driving lane collection,
// set to null values and return its ptr
//(struct drivingLanes *) ASN_CreateEmptyDrivingLaneSet();
void* ASN_CreateEmptyDrivingLaneSet()
{
    // As in: theApproachSet->approach->drivingLanes = new struct drivingLanes;
    struct drivingLanes*  thePtr = new struct drivingLanes;
    ZeroNewList(thePtr);
    return(thePtr);
}
// Create a dispose call in time as well.



// Return an allocated but empty barrier lane collection,
// set to null values and return its ptr
void* ASN_CreateEmptyBarriersLaneSet()
{
    // As in: theApproachSet->approach->drivingLanes = new struct drivingLanes;
    struct barriers*  thePtr = new struct barriers;
    ZeroNewList(thePtr);
    return(thePtr);
}
// Create a dispose call in time as well.



// Return an allocated but empty cross walk lane collection,
// set to null values and return its ptr
void* ASN_CreateEmptyCrosswalksLaneSet()
{
    // As in: theApproachSet->approach->drivingLanes = new struct drivingLanes;
    struct crosswalks*  thePtr = new struct crosswalks;
    ZeroNewList(thePtr);
    return(thePtr);
}
// Create a dispose call in time as well.


// Return an allocated but empty driving lane,
// set to null values and return its ptr
VehicleReferenceLane* ASN_CreateEmptyDrivingLane()
{
    VehicleReferenceLane* theLane   = new VehicleReferenceLane;
    if (theLane != NULL)
    {
        theLane->connectsTo             = NULL;
        theLane->keepOutList            = NULL;
        theLane->laneAttributes         = 0;
        theLane->laneNumber.buf         = NULL;  // may be allocated already
        theLane->laneNumber.size        = 0;
        theLane->laneWidth              = NULL;
        ZeroNewList(&theLane->nodeList);
    }
    return(theLane);
}
// Create a dispose call in time as well.



// Return an allocated but empty barrier lane,
// set to null values and return its ptr
BarrierLane* ASN_CreateEmptyBarrierLane()
{
    BarrierLane* theLane   = new BarrierLane;
    if (theLane != NULL)
    {
        theLane->barrierAttributes      = 0;
        theLane->laneNumber.buf         = NULL;  // may be allocated already
        theLane->laneNumber.size        = 0;
        theLane->laneWidth              = NULL;
        ZeroNewList(&theLane->nodeList);
    }
    return(theLane);
}
// Create a dispose call in time as well.


// Return an allocated but empty cross walk lane,
// set to null values and return its ptr
CrosswalkLane* ASN_CreateEmptyCrosswalkLane()
{
    CrosswalkLane* theLane   = new CrosswalkLane;
    if (theLane != NULL)
    {
        theLane->connectsTo             = NULL;
        theLane->keepOutList            = NULL;
        theLane->laneAttributes         = 0;
        theLane->laneNumber.buf         = NULL;  // is this allocated allready?
        theLane->laneNumber.size        = 0;
        theLane->laneWidth              = NULL;
        ZeroNewList(&theLane->nodeList);
    }
    return(theLane);
}
// Create a dispose call in time as well.





// Given a hex string (0~9 and upper/lower A~F) return an UNSIGNED int value
// sign can be easily dealt with when you cast it the receiving var
unsigned int htoi (const char *ptr)
{
	// if first two chars are the pattern "0x" or 0X then remove them
	if ((ptr[0] == '0') && (ptr[1] == 'x'))
		ptr = ptr +2;
	else if ((ptr[0] == '0') && (ptr[1] == 'X'))
		ptr = ptr +2;
    unsigned int value = 0;
    char ch = *ptr;
    for (;;)
    {
        if (ch >= '0' && ch <= '9')
            value = (value << 4) + (ch - '0');
        else if (ch >= 'A' && ch <= 'F')
            value = (value << 4) + (ch - 'A' + 10);
        else if (ch >= 'a' && ch <= 'f')
            value = (value << 4) + (ch - 'a' + 10);
        else
            return value;
        ch = *(++ptr);
    }
}



// Given a value in radians, return its value in degrees
inline double Rads2degs(double rads)
{
    return( ( ( rads * 180.0 ) / M_PI ) );
}

// Given a value in degrees, return its value in radians
inline double Degs2rads(double degs)
{
    return( ( ( degs / 180.0 ) * M_PI ) );
}


// Given a value in radians, return its value in degrees
// But adjust for "upside down" cases so that any text
// plotted with this angle is, up, down or to the right
// but never "backwards" which people do not care to
// read, we allow a 'slight' cant of up to 20 deg this way
double Rads2degs4text(double rads)
{
	// we presume the rad are aligned to non-DSRC use
    double angle = Rads2degs(rads);
    if      (angle > +110.0) return(angle - 180.0);
    else if (angle < -110.0) return(angle + 180.0);
    return(angle);
}

double Degs2degs4text(double angle)
{
	angle = angle + 90;  // rotate the text angle
    if      (angle > +110.0) return(angle - 180.0);
    else if (angle < -110.0) return(angle + 180.0);
    return(angle);
}


// A utility to toggle between DSRC 'zero is north up' and
// math coordinate 'zero to the right along X' forms
double ToggleDeg(double angle)
{   // translate between the two systems
    return( -1 * (angle - 90.0));
}

// Given an angle (in deg) that may be 'near' an axis,
// snap to that axis and return the value.  This is used
// to make text at a slight angle much more readable
double SnapTo(double angle)
{
#define SNAP_THRESSHOLD  (7.0) // in degrees, half of snap's span
	// values between 5~12 work well, max should be < 40
	bool isNeg = false;
	if (angle < 0.0) // extract sign
	{
		isNeg = true;
		angle = -1 * angle;
	}
	while (angle > 360.0) angle = angle - 360.0; // normalize angle

	// test each quad
	if (((0     + angle) < SNAP_THRESSHOLD) ||
		((360.0 - angle) < SNAP_THRESSHOLD) )
		angle = 0;
	else if (abs(angle - 90.0) < SNAP_THRESSHOLD)
		angle = 90.0;
	else if (abs(angle - 180.0) < SNAP_THRESSHOLD)
		angle = 180.0;
	else if (abs(angle - 270.0) < SNAP_THRESSHOLD)
		angle = 270.0;
	// else, general value case do not change it

	// correct for negative sign and return
	if (isNeg == true)
		angle = -1 * angle;
    return(angle);
}



// Convert floating point lat values to be valid ASN
// units, dealing with sign and LSB precision, returning
// a valid value for direct use in the ASN messages
long FloatLatLong2_ASNLatLong(double theValue, bool isLong)
{
    if (isLong == true) // no need to do this with lat values
    {
        // Adjust for slightly larger values then should be found
        if (theValue > +360.0) theValue = theValue - 360.0;
        if (theValue < -360.0) theValue = theValue + 360.0;

        // adjust to correct range
        if (theValue > +180.0) theValue = theValue - 360.0;
        if (theValue < -180.0) theValue = theValue + 360.0;
    }
    return((long)(theValue * DEG2ASNunits)); // now scale it and return
}


// Convert a long LL to a float LL using DSRC 1/10th Micro degree units
double LLlong2Float(long theValue)
{
    return((double)(theValue / DEG2ASNunits));
}





/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
/* Vincenty Inverse Solution of Geodesics on the Ellipsoid (c) Chris Veness 2002-2010             */
/*                                                                                                */
/* from: Vincenty inverse formula - T Vincenty, "Direct and Inverse Solutions of Geodesics on the */
/*       Ellipsoid with application of nested equations", Survey Review, vol XXII no 176, 1975    */
/*       http://www.ngs.noaa.gov/PUBS_LIB/inverse.pdf                                             */
/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  */
/**
 * Calculates geodetic distance between two points specified by latitude/longitude using
 * Vincenty inverse formula for ellipsoids
 *
 * @param   {Number} lat1, lon1: first point in decimal degrees
 * @param   {Number} lat2, lon2: second point in decimal degrees
 * @returns (Number} distance in metres between points
 */
// DCK Updated, made into clean C, and merged into ultil file on Aug 16th 2011
// taken from http://www.movable-type.co.uk/scripts/latlong-vincenty.html


#ifdef WIN32
    #ifndef NaN
        static const unsigned long __nan[2] = {0xffffffff, 0x7fffffff};
        #define NaN (*(const float *) __nan)
    #endif
#else
    #define NaN     nan(NULL)  // strict C99 solution (Ramesh use this one)
#endif


// Re-implement for this use
double DistVincenty(double lat1, double lon1, double lat2, double lon2)
{
    // establish WSG 84 ellipsoid with std industry params
    double a  = 6378137;          // major axis
    double b  = 6356752.314245;   // minor axis
    double f  = 1/298.257223563;  // flattening ratio
    double L  = Degs2rads(lon2-lon1); // delta length in rads
    double U1 = atan((1-f) * tan(Degs2rads(lat1)));
    double U2 = atan((1-f) * tan(Degs2rads(lat2)));
    double sinU1 = sin(U1);
    double cosU1 = cos(U1);
    double sinU2 = sin(U2);
    double cosU2 = cos(U2);
    double lambda = L; // initial seed length in rads
    double lambdaP;
    int iterLimit = 100;
    double sinLambda;
    double cosLambda;
    double sinSigma;
    double cosSigma;
    double sigma;
    double sinAlpha;
    double cosSqAlpha;
    double cos2SigmaM;
    double C;
    double uSq;
    double A;
    double B;
    double deltaSigma;
    double s;

    do {
        sinLambda = sin(lambda);
        cosLambda = cos(lambda);
        sinSigma  = sqrt((cosU2 * sinLambda) * (cosU2 * sinLambda) +
                    (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda) *
                    (cosU1 * sinU2 - sinU1 * cosU2 * cosLambda));
        if (sinSigma==0)
            return 0;  // co-incident points
        cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosLambda;
        sigma = atan2(sinSigma, cosSigma);
        sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
        cosSqAlpha = 1 - sinAlpha * sinAlpha;
        cos2SigmaM = cosSigma - 2 * sinU1*sinU2 / cosSqAlpha;
        if (isnan(cos2SigmaM)) // same as: IsNan(xx) or use (a != a)
            cos2SigmaM = 0;  // equatorial line: cosSqAlpha=0 (§6)
        C = f/16 * cosSqAlpha * (4+f * (4-3 * cosSqAlpha));
        lambdaP = lambda;
        lambda  = L + (1-C) * f * sinAlpha *
            (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1+2 * cos2SigmaM * cos2SigmaM)));

    } while (fabs(lambda-lambdaP) > 1e-12 && --iterLimit>0); // while not converged

    if (iterLimit==0)
        return (NaN);  // formula failed to converge (antipodal points found?)

     uSq = cosSqAlpha * (a*a - b*b) / (b*b);
     A = 1 + uSq/16384*(4096+uSq*(-768+uSq*(320-175*uSq)));
     B = uSq/1024 * (256+uSq*(-128+uSq*(74-47*uSq)));
     deltaSigma = B*sinSigma*(cos2SigmaM+B/4*(cosSigma*(-1+2*cos2SigmaM*cos2SigmaM)-
                    B/6*cos2SigmaM*(-3+4*sinSigma*sinSigma)*(-3+4*cos2SigmaM*cos2SigmaM)));
     s = b*A*(sigma-deltaSigma);

    // do not round at all s = s.toFixed(3); // round to 1mm precision
    return (s); // distance in units of meters
}

/**** taken from below
function distVincenty(lat1, lon1, lat2, lon2) {
  var a = 6378137, b = 6356752.314245,  f = 1/298.257223563;  // WGS-84 ellipsoid params
  var L = (lon2-lon1).toRad();
  var U1 = Math.atan((1-f) * Math.tan(lat1.toRad()));
  var U2 = Math.atan((1-f) * Math.tan(lat2.toRad()));
  var sinU1 = Math.sin(U1), cosU1 = Math.cos(U1);
  var sinU2 = Math.sin(U2), cosU2 = Math.cos(U2);

  var lambda = L, lambdaP, iterLimit = 100;
  do {
    var sinLambda = Math.sin(lambda), cosLambda = Math.cos(lambda);
    var sinSigma = Math.sqrt((cosU2*sinLambda) * (cosU2*sinLambda) +
      (cosU1*sinU2-sinU1*cosU2*cosLambda) * (cosU1*sinU2-sinU1*cosU2*cosLambda));
    if (sinSigma==0) return 0;  // co-incident points
    var cosSigma = sinU1*sinU2 + cosU1*cosU2*cosLambda;
    var sigma = Math.atan2(sinSigma, cosSigma);
    var sinAlpha = cosU1 * cosU2 * sinLambda / sinSigma;
    var cosSqAlpha = 1 - sinAlpha*sinAlpha;
    var cos2SigmaM = cosSigma - 2*sinU1*sinU2/cosSqAlpha;
    if (isNaN(cos2SigmaM)) cos2SigmaM = 0;  // equatorial line: cosSqAlpha=0 (§6)
    var C = f/16*cosSqAlpha*(4+f*(4-3*cosSqAlpha));
    lambdaP = lambda;
    lambda = L + (1-C) * f * sinAlpha *
      (sigma + C*sinSigma*(cos2SigmaM+C*cosSigma*(-1+2*cos2SigmaM*cos2SigmaM)));
  } while (Math.abs(lambda-lambdaP) > 1e-12 && --iterLimit>0);

  if (iterLimit==0) return NaN  // formula failed to converge

  var uSq = cosSqAlpha * (a*a - b*b) / (b*b);
  var A = 1 + uSq/16384*(4096+uSq*(-768+uSq*(320-175*uSq)));
  var B = uSq/1024 * (256+uSq*(-128+uSq*(74-47*uSq)));
  var deltaSigma = B*sinSigma*(cos2SigmaM+B/4*(cosSigma*(-1+2*cos2SigmaM*cos2SigmaM)-
    B/6*cos2SigmaM*(-3+4*sinSigma*sinSigma)*(-3+4*cos2SigmaM*cos2SigmaM)));
  var s = b*A*(sigma-deltaSigma);

  s = s.toFixed(3); // round to 1mm precision
  return s;

  // note: to return initial/final bearings in addition to distance, use something like:
  var fwdAz = Math.atan2(cosU2*sinLambda,  cosU1*sinU2-sinU1*cosU2*cosLambda);
  var revAz = Math.atan2(cosU1*sinLambda, -sinU1*cosU2+cosU1*sinU2*cosLambda);
  return { distance: s, initialBearing: fwdAz.toDeg(), finalBearing: revAz.toDeg() };
}
***/

	/**********
	// Convert Frank's lat-long to its offset rates using std SCSC toolkit calls
    double startlat =  42.184727;
    double startlon = -83.189296;
	double oneLSB = 0.0000001; // a 1/10th micro degree (one LSB)
	double value;
	char text[200];
	cout << "\nDetermine Local convert rates... " << endl;
	sprintf(text, "At the point: %.8f (lat)  %.8f (long)\n", startlat, startlon);
    cout << text;
	value = DistVincenty(startlat, startlon, startlat+oneLSB, startlon);
    cout << "One 1/10th microDeg of  lat is " << value << " meters" << endl;
    value = DistVincenty(startlat, startlon, startlat, startlon+oneLSB);
    cout << "One 1/10th microDeg of long is " << value << " meters" << endl;
	cout << "Use these values to compute offsets from the anchor point." << endl;
	return 0;  //exit
	****/


// Given a precise lat-lon point and an 'step' amount (all doubles)
// Step is ypically a degree, or a mciro degree, or a 1/10th micro degree (the DSRC LSB unit)
// compute the convertion rate into meters (NOT cm) for lat and long and return them
// using the DistVincenty() call to compute a line (of step length in deg) along each axis
void VincentyConstants(double startlat, double startlon,
					   double step,
					   double* convertLat, double* convertLon)

{	// we presume caller knows what he is doing
	*convertLat = DistVincenty(startlat, startlon, startlat+step, startlon);
	*convertLon = DistVincenty(startlat, startlon, startlat,      startlon+step);
}







// Test Vincenty code here, dump results to console
void TestVincenty()
{
    double lat1;
    double lon1;
    double lat2;
    double lon2;

    lat1 =   23.4567;
    lon1 = -123.4567;
    lat2 =   23.4568;
    lon2 = -123.4568;
    cout << "\nTest #1 " << endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   33.843110558;
    lon1 = -112.134954881;
    lat2 =   33.843130851;
    lon2 = -112.134635740;
    cout << "\nTest #2, Lane 0x21, expecting about 29.6156 meters" << endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   33.843110558;
    lon1 = -112.134954881;
    lat2 =   lat1 + 0.0001;
    lon2 =   lon1;
    cout << "\nTest #3, for 0.0001 deg of lat only " << endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   33.843110558;
    lon1 = -112.134954881;
    lat2 =   lat1 ;
    lon2 =   lon1 + 0.0001;
    cout << "\nTest #4, for 0.0001 deg of lon only "<< endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   37.360651;
    lon1 = -121.941406;
    lat2 =   lat1 + 0.0001;
    lon2 =   lon1;
    cout << "\nTest #5a, for 0.0001 deg of lat only " << endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   37.360651;
    lon1 = -121.941406;
    lat2 =   lat1 ;
    lon2 =   lon1 + 0.0001;
    cout << "\nTest #5b, for 0.0001 deg of lon only "<< endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   37.36;
    lon1 =  -121.94 ;
    lat2 =    37.37 ;
    lon2 =  -121.93;
    cout << "\nTest #6, longer line "<< endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   23.45;
    lon1 =  -123.45;
    lat2 =    23.46;
    lon2 =  -123.44;
    cout << "\nTest #7, longer line "<< endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    cout << "Vin Length: " << DistVincenty(lat1, lon1, lat2, lon2) << " in meters" << endl;

    lat1 =   37.0;
    lon1 =  -121.0;
    lat2 =    38.0;
    lon2 =  -122.0;
    cout << "\nTest #8, one degee long line at 45 deg angle"<< endl;
    cout << "Start; " << lat1 << "  " << lon1 << "   End: " << lat2 << "  " << lon2 << endl;
    cout << "Deg Length: " << sqrt( ((lat1 - lat2) * (lat1 - lat2)) +
                                    ((lon1 - lon2) * (lon1 - lon2)) ) << " in degrees" << endl;
    char text[200];
    sprintf(text, "Vincenty Length: %02.6f in meters",
            DistVincenty(lat1, lon1, lat2, lon2));
    cout << text << endl;

	// Convert Frank's lat-long to its offset rates using std SCSC toolkit calls
    double startlat =  42.184727;
    double startlon = -83.189296;
	double oneLSB = 0.0000001; // a 1/10th micro degree (one LSB)
	double xRate;
	double yRate;
	cout << "\nDetermine Local convert rates... " << endl;
	sprintf(text, "At the point: %.8f (lat)  %.8f (long)\n", startlat, startlon);
    cout << text;
	VincentyConstants(startlat, startlon, oneLSB, &xRate, &yRate);
    cout << "One 1/10th microDeg of  lat is " << yRate << " meters" << endl;
    cout << "One 1/10th microDeg of long is " << xRate << " meters" << endl;
	cout << "Use these values to compute offsets from this anchor point." << endl;

}


// Display to cout the convert rates for the passed point,
// simply a clean way to get the value to the screen for others
// note this just uses the DSRC LSB as the distance unit
void DisplayVincentyConstants(double startlat, double startlon)
{
    char text[200];
	double oneLSB = 0.0000001; // a 1/10th micro degree (one LSB)
	double xRate;
	double yRate;
	cout << "\nDetermine Local convert rates... " << endl;
	sprintf(text, "At the WGS-84 point: %.8f (lat)  %.8f (long)\n", startlat, startlon);
    cout << text;
	VincentyConstants(startlat, startlon, oneLSB, &xRate, &yRate);
	sprintf(text, "%.7f", 100 * yRate);
    cout << "  One 1/10th microDeg of  lat is " << text << " centimeters" << endl;
	sprintf(text, "%.7f", 100 * xRate);
    cout << "  One 1/10th microDeg of long is " << text << " centimeters" << endl;
	cout << "  Use these values to compute X-Y offsets from this anchor point.\n\n";
}


// Write the passed bytes, using the passed count, to the passed file name
// return number of bytes written on success or -1 on error
size_t WriteBytesToFile(char* fileName, unsigned char* pData, int dataCnt)
{
    // Presumes inputs are valid, does no testing
    FILE* tempFileStream;
    size_t byteCnt = 0;
    // open a file by this name, and save into it, then close it.
    // we open to write a binary style file - you MUST do this with ASN !
	// Precise form will vary by OS and Compiler

	// Windows: if( (tempFileStream = _fsopen( fileName, "wb", _SH_DENYWR )) != NULL )
    // Unbuntu  if( (tempFileStream =  fopen ( fileName, "wb" )) != NULL )
    if( (tempFileStream = _fsopen( fileName, "wb", _SH_DENYWR )) != NULL )
	    {
	    byteCnt = fwrite(pData, 1, dataCnt, tempFileStream); // dump it all out here
	    fflush(tempFileStream);
	    fclose(tempFileStream);
        return(byteCnt);
	    }
    else
	    {   // write file open failed
            cout << "ERROR: Unable to write requested file." << endl;
            return(-1);
	    }
    cout << "ERROR: Unable to open requested file to write." << endl;
    return(-1);
}



// Given a valid file, read it into memory and return the passed pointer
// pointing to the created array on the heap to hold it
// on error  will return null for ptr and -1 for cnt
// error comments sent to cout
unsigned char* FileInToMemoryBytes(char* theFile, size_t* theCnt, bool vb)
{
    if ((theFile != NULL) && (theCnt != NULL)) // if inputs, proceed
    {
        // if the file exists, we open and read it
        FILE *f;
        unsigned char* buf = NULL;
        #define MAX_FILE_READ 1000000  // 1 meg, just picked as a a guess
        buf = new unsigned char[MAX_FILE_READ]; // should auto size this
        size_t  msgSize = 0;

        // Load the file to be used
        f = fopen(theFile, "rb"); // always open these files in binary mode
        // enable only if you are sure the file will exist: assert(f);
        if (f == NULL)
        {   // unable to find or open file, abort
            cout << "ERROR: Unable to Open file: " << \
                theFile << " Code: " << errno << endl;
            *theCnt = -1;
            return(NULL);
        }

        // get the raw chars
        msgSize = fread(buf, 1, MAX_FILE_READ, f);
        fclose(f);
        if (vb)
            cout << "Loading file: " << theFile << " of [" << msgSize << "] chars" << endl;
        if (msgSize != 0 || msgSize <= MAX_FILE_READ)
        {   // test for at/over buffer limit
            if (msgSize == MAX_FILE_READ)
            {
                cout << endl << "ERROR: File is larger then allocated buffer, exiting. " << endl;
                *theCnt = -1;
                return(NULL);
            }
            *theCnt = msgSize;  // return a char count
            return (buf);       // return buffer pointer
        }
        else
        {
            cout << "ERROR: Found, but unable to read file named: " << theFile << endl;
            *theCnt = -1;
            return(NULL);
        }

        if (f != NULL) delete f;
        if (buf != NULL) delete buf;
    }
    *theCnt = -1;
    return(NULL);
}







// Compare the passed two pointers for the indicated number of bytes,
// if they match at all points, return true, otherwise false
// basically just a string compare, but deals with nulls
bool DoBytesMatch(unsigned char* first, unsigned char* second, int cnt)
{
    if ((first != NULL) && (second != NULL) && (cnt > 0))
    {
        int i = 0;
        while (i < cnt)
        {
            if (first[i] != second[i])
                return (false);
            i++;
        }
        return (true);
    }
    else
        return(false); // bad inputs
}



// General purpose float rounding routine,
// taken from other SCSC DGPS work
// rounds the float to the passed quantization value
double RoundValue(double theValue, double quant)
{
	// do the requested rounding here and return
	// note that MS VS C does not support "round()" (a std routine in C99)
	// so we have to add a half LSB each time and use floor
	return( quant * floor( (theValue + (quant/2.0)) * (1 / quant) ) );
}



// Convert into and out of the heading units used in DSRC messages
// from degrees and rad values. Note that DSRC is a North-Up system
double DSRCHeading2Degs(int heading)
{
	if (heading == 28800) return (-1); // error value
	return ( heading * HEADINGunits2DEG);
}

double DSRCHeading2Rads(int heading)
{
	if (heading == 28800) return (-1); // error value
	return (Degs2rads(heading * HEADINGunits2DEG));
}

int    Degs2DSRCHeading(double dsrcAngle)
{
	if (dsrcAngle < 0)   dsrcAngle = dsrcAngle + 360.0;
	if (dsrcAngle > 360) dsrcAngle = dsrcAngle - 360.0;
	return ( (int)(dsrcAngle / HEADINGunits2DEG) );
}
int    Rads2DSRCHeading(double dsrcAngle)
{
	return( Degs2DSRCHeading ( Rads2degs (dsrcAngle) ) );
}


// Convert from feet to cm and back,
// returns doubles for each
double Ft2Cm(double length)
{
	return(length * CMperFoot);	// 1 foot = 0.3048 meters
}
double Cm2Ft(double length)
{
	return(length / CMperFoot);	// 1 foot = 0.3048 meters
}


// Express Time in MM:SS form into the passed string
void Time2HH_MM_SS(time_t theTime, char* text)
{
	if ((text != NULL) && (theTime != 0))
	{
		tm* pLT  = localtime(&theTime);
		sprintf(text, "%02i:%02i:%02i",
			pLT->tm_hour,
			pLT->tm_min,
			pLT->tm_sec);
	}
}


// Express Time in HH:MM:SS form into the passed string
void Time2MM_SS(time_t theTime, char* text)
{
	if ((text != NULL) && (theTime != 0))
	{
		tm* pLT  = localtime(&theTime);
		sprintf(text, "%02i:%02i",
			pLT->tm_min,
			pLT->tm_sec);
	}
}


// Convert two integer numbers in a modulo roll-over system
// to the distance between them, a match returns the limit
// value.  The value new is presumed to advance (by+)
// Returned sign is always positive
long ModuloDist(long newV, long oldV, long limitV)
{
	if (newV > oldV) // typical 'easy' case
		return(newV - oldV);
	else if (oldV > newV) // roll over case
		return(newV + (limitV-oldV));
	else // are equal, presume a complete loop
		return(limitV);
	// note we never return a zero
}




// Given a valid char ptr to read and one to write to, extract
// the next line found from the passed-in starting point
// using the passed-in delimiter and return the index
// point in the source string we ended up at
// (this value is typically used to start the next line read)
long GetNextLine(char* theSource, char* theLine, char* delimit, int startPt)
{
	char terminate[] = "\n"; // the line termination style to use
    long result; // returned 'next' start point
    long distance; // span of the found string
	long sourceLen = (long)strlen(theSource);
	char* startPtr = theSource + startPt; // the working start
	char* endPtr = NULL;  // the working end (to be found)
	if ((theSource != NULL) &&
		(theLine != NULL) &&
		(startPt < sourceLen))
	{	// find matching line end point
		endPtr =  strstr(startPtr, delimit);
		distance = (long)(strlen(delimit) + (endPtr - startPtr));
		if (endPtr == NULL) // not found, so must be at end of string
		{	// might be the very last bit or already at end
			if ((endPtr - startPtr) > 0)
			{	// return the very last bit, terminate it to be sure
				strncpy(theLine, startPtr, distance);
				strcpy(theLine + distance, terminate); // always terminate it (even if a dupe)
				result = sourceLen;
			}
			else // nothing to return, seem to have started at very end
			{
				strcpy(theLine, terminate); // always terminate it (even if a dupe)
				result = -1;
			}
		}
		else
		{	// Extract string, terminate it, and return
			strncpy(theLine, startPtr, distance);
			strcpy(theLine + distance, terminate);
			result = (int)(startPt + distance);
		}
		// DEBUG
		//cout << "Ptrs, from " << (long)startPtr << " to " << (long)endPtr << " dist: " << distance <<endl;
		//cout << "Result: " << theLine << endl;
	}
	else
	{
		if (theLine != NULL) strcpy(theLine, terminate); // terminate returned line
		result = -1;
	}
	return(result);
}


//
// Routines to assist in decoding and determining a raw message type
//


// Get msgID from first element in byte array, return as int
int GetMsgType(BYTE* thePtr, int skip)
{
	// seek of first "0x80", the proper 1st tag in struc
	// it should be in first few words, typically the 2nd
	// extract next word, should be set to one (single byte of length)
	// extract next byte (the msg ID value) and return it
	// We need the skip a value to avoid the chance of a prior value
	// of 80,01,xx ever getting seen as a valid message type.
	int i;
	for (i=skip; i<skip+4; i++)
	{
		if (thePtr[i] == 0x80)
		{
			// found tag, extract rest (word count and value)
			if (thePtr[i+1] == 0x01)
			{	// word count seems valid, proceed
				return(thePtr[i+2]); // return value
			}
			else return(-1);  // word count not set to one, invalid for DSRC uses
		}
	}
	return(-1);  // never found correct tag
}


// Return true if is a valid and useful DSRC msgID
// (the skips zero value of DSRCmsgID_reserved) but allows local content value
int IsValidMsgType(int value)
{
	if ((value >= DSRCmsgID_alaCarteMessage) &&
		(value <= DSRCmsgID_travelerInformation))
		return(true);
	else
	{	// check for local content range used
		if ((value >= 128) &&
			(value <= 255))
			return (true);
	}
	return(false);
}


// Get word count from first element encoding, return as int
int GetMsgWordCnt(BYTE* thePtr)
{
	// presume first word is tag (0x30),
	// read next word (one or more) until upper byte is NOT set (extender bit)
	// then combine all bytes to make result
	if ((thePtr[1] & 0x80 ) != 0)
	{
		// a multi-word count value, extract how many bytes to use for count
		int byteCnt =  thePtr[1] & 0x0F;  // more bytes than DSRC ever uses
		if      (byteCnt == 0) return(thePtr[1]);
		else if (byteCnt == 1) return(thePtr[2]);
		else if (byteCnt == 2) return(thePtr[3]+ (thePtr[2]*256) );
		else return(-1);  // too damn big for DSRC use (but legal in ASN)
	}
	else
	{
		// a simple one-word count value, extract and return
		return(thePtr[1]);
	}
}


// For the passed message size, how many bytes were used to encode this length
// used to correct the message length calculations
// includes one bytes for the "0x30" start of message
// presumed a positive value is passed in
int BytesUsed(int size)
{
	if      (size <= 127)	return(2);
	else if (size <= 255)	return(3);
	else if (size <= 65536)	return(4);
	return(5); // such a large message should never occur in DSRC work
}


// Given a type of message (by msgId, an int value) return
// the address of the message type that this is
asn_TYPE_descriptor_s* GetMesssageTypeFromId(int messageType)
{
	switch(messageType)  // values assigned by enum DSRCmsgID from ASN lib
	{
		case -1:									return ( &asn_DEF_BasicSafetyMessage ); // default case
		case DSRCmsgID_reserved:					return ( &asn_DEF_BasicSafetyMessage ); // default case
		case DSRCmsgID_alaCarteMessage:				return ( &asn_DEF_AlaCarte );
		case DSRCmsgID_basicSafetyMessage:			return ( &asn_DEF_BasicSafetyMessage );
		case DSRCmsgID_basicSafetyMessageVerbose:	return ( &asn_DEF_BasicSafetyMessageVerbose );
		case DSRCmsgID_commonSafetyRequest:			return ( &asn_DEF_CommonSafetyRequest );
		case DSRCmsgID_emergencyVehicleAlert:		return ( &asn_DEF_EmergencyVehicleAlert );
		case DSRCmsgID_intersectionCollisionAlert:	return ( &asn_DEF_IntersectionCollision );
		case DSRCmsgID_mapData:						return ( &asn_DEF_MapData );
		case DSRCmsgID_nmeaCorrections:				return ( &asn_DEF_NMEA_Corrections );
		case DSRCmsgID_probeDataManagement:			return ( &asn_DEF_ProbeDataManagement );
		case DSRCmsgID_probeVehicleData:			return ( &asn_DEF_ProbeVehicleData );
		case DSRCmsgID_roadSideAlert:				return ( &asn_DEF_RoadSideAlert );
		case DSRCmsgID_rtcmCorrections:				return ( &asn_DEF_RTCM_Corrections );
		case DSRCmsgID_signalPhaseAndTimingMessage: return ( &asn_DEF_SPAT );
		case DSRCmsgID_signalRequestMessage:		return ( &asn_DEF_SignalRequestMsg );
		case DSRCmsgID_signalStatusMessage:			return ( &asn_DEF_SignalStatusMessage );
		case DSRCmsgID_travelerInformation:			return ( &asn_DEF_TravelerInformation );
		default: return (&asn_DEF_BasicSafetyMessage); // default case
	}
	// Note that all of these are supported in the ASN lib and can be decoded
}









// An example DSRC_serializer for the BSM case,
// typically you will want to build this in your own app layer so that you
// can manage any errors with more control
// note that the code fragment below has a Win/Unix string use case
ssize_t DSRC_serializer(asn_TYPE_descriptor_t *type_descriptor, // message type
						void *theMsg,  //the ptr to the message struc
						BYTE *theBuffer ) // the ptr to the buff to fill
{
	asn_enc_rval_t er; // Encoder return value

	// this is the magic general call you will want to use:
	er = der_encode_to_buffer(type_descriptor,
							  theMsg,
							  theBuffer,
							  MAX_MSG_SIZE);

	// In a GUI use:
	//if (er.encoded >= MAX_MSG_SIZE)
	//	MessageBox("Increase buffer in BSM encode.!","Buffer overflow!",MB_OK+ MB_ICONWARNING);
	if (er.encoded >= MAX_MSG_SIZE)
		cout << "Message buffer overflow in DSRC_serializer()" << endl;

	if(er.encoded == -1) {
		// Failed to encode the data, report error
		//CString text;
		//text.Format("Cannot encode %s: %s\n", er.failed_type->name, strerror(errno));
		// MessageBox(text,"Encode Error!",MB_OK+ MB_ICONWARNING);

		// For the non-windows users or console only use:
		fprintf(stderr, "Cannot encode %s: %s\n",
			er.failed_type->name,
			strerror(errno));
		return -1;
		}
	else {
		// Return the number of bytes
		return er.encoded;
		}
}



// An example DSRC_UNserializer for the BSM message case,
// typically you will want to build this in your own app layer so that you
// can manage any errors with more control
// note that this code fragment looks at the message to determine if it is
// well formed and what type of message it in fact is before it calls the
// actual ASN decoding logic on it.  While this example shows BSM message use
// the logical flow works with all ITS / DSRC messages
ssize_t DSRC_UNserializer(void** theMsgStruc,   //the void** ptr to the struc to fill
						  BYTE*  theBuffer,		// the ptr to the buff to read from
						  int    theInputSize,  // the size of the buff contents
						  bool   vb) // verbose output control
{
	// Aside: Observe the terms 'de-code and en-code' reflect a point of view that can vary

	// Aside: Do not pass in a message structure with 'bad' pointers,
	// the code will presume any != NULL pointer is valid and attempt to use it,

	// Aside: Cast the ptr into and out-of the preferred type once out of this call

	int decodedMsgWdCnt = 0;	// msg wd count
	int wdCntByteCnt = 0;		// msg wd count count
	int decodedMsgType = 0;		// msg type
	char text[100];
	bool failPreTests = false;

	if (theBuffer != NULL) // if we have a source (a null target is simply allocated)
	{
		// Part One, Determine if this is in fact a valid looking message
		// and what type is it using some basic logic

		// First byte test
		if (theBuffer[0] != 0x30) failPreTests = true;

		// Extract the ASN.1 encoded word count that follows
		decodedMsgWdCnt = GetMsgWordCnt(theBuffer);
		if (decodedMsgWdCnt == -1) failPreTests = true; // can not determine count
		if (decodedMsgWdCnt < 2) failPreTests = true; // too small to be real
		if (decodedMsgWdCnt > MAX_MSG_SIZE) failPreTests = true; // too big to be real

		// How many bytes does the first tag and word count consume
		// (larger messages do not fit in one byte counts)
		wdCntByteCnt = BytesUsed(decodedMsgWdCnt);

		// Does this value match the actual length of the message?
		if (decodedMsgWdCnt + wdCntByteCnt != theInputSize) failPreTests = true;

		// Get the message type that this is message claims to be
		decodedMsgType = GetMsgType(theBuffer, wdCntByteCnt);
		// Here we are seeking for BSM, but a more general logic can easily be devised
		// Un-comment this line if you desire this check
		// DEBUG if ((decodedMsgType != DSRCmsgID_basicSafetyMessage) | (failPreTests == true))

		// Check for valid range in the DSRC message ID type
		if (IsValidMsgType(decodedMsgType) == false) failPreTests = true;


		// Part Two, Having passed part one, now we try and decode it.
		if (failPreTests == false)
		{
			asn_dec_rval_t rval; // declare decoder return value

			// the magic call, telling ASN the type, the struct ptr, the buff ptr, and the length
			rval = ber_decode( 0,
				(asn_TYPE_descriptor_s *)GetMesssageTypeFromId(decodedMsgType),
				theMsgStruc,  // rather then type specific like: (void **)&theBSM,
				theBuffer, theInputSize); // decode into a struc

			if (rval.code != RC_OK) // asn_dec_rval_code_e::RC_OK
			{
				cout << "ASN decode failed, Can not read this message, Bad data or wrong type." << endl;
				if (rval.code != RC_WMORE)
				{
					sprintf(text, "Error, ASN decoder starved on byte %i of %i,\nEither more data expected in encoding or incorrect tag found.",
						(int)rval.consumed, theInputSize);
					if (vb) cout << text << endl;
				}
				else
				{
					sprintf(text, "Error, ASN decoding failed on byte %i of %i\nNo further information available.",
						(int)rval.consumed, theInputSize);
					if (vb) cout << text << endl;
				}
				return -1; // fail due to a decoding issue
			}
			else
			{
				if (vb) cout << "Memory was decoded, a valid message was found, now in a structure." << endl;
				return(rval.consumed); // pass
			}
		}
		else
			if (vb) cout << "Memory image failed pre-testing for valid message." << endl;
			return -1; // fail due to pre testing
	}
	else
		if (vb) cout << "Bad inputs passed to function call DSRC_UNserializer()" << endl;
		return -1; // fail due to no inputs

}



// end of DSRC_ASN_Utils.cpp


