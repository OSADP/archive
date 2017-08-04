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
// File: BLOB1_util.cpp
//
// This file holds a utility code class to translate data
// into and out of the packed octets of the BSM BLOB1 data element
//
// DCK Create Aug 26th 2011  <davidkelley@ITSware.net> 
// SCSC Header to go here
//
// Change History
//  From several prior efforts, this copy deals ONLY with the 
//  bytes of the ASN so that the different app unit translate 
//  needs can live elsewhere and this code can be better 
//  re-used over different projects. 
//
//  Do not modify this code to add you own units, rather
//  call it from you own class and do the (any) translate there
//  the BasicVehicle class is an example of this for VISIM units
//  General Release Sept 10th 2011
//
// DCK Trivial change from the Ubuntu side to test SVN uploads Sept 27th 
//
/////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "BLOB1_util.h"	
// Needs full ASN to include this one: #include "util.h"
// have patched routines needed to overcome this
#include <stdio.h>
#include <iostream>
#include <assert.h>
using namespace std;    // access to cout stream


BSM_BLOB1::BSM_BLOB1()
{
    // Init() does all the real work (called at the end), 
    // here we just build a  template message for the class

    // Build up the default blob bytes here for use, then copy over
    // NOTE: this is a BLOB not a BSM message, which in turn holds a BLOB 
    unsigned char emptyArray [] = { 
        0x00,   // start of blob data
        0x00, 
        0x00, 
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
        0x00, 0x00, 0xF0, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00, 
        0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 
        0x00, 0x00, 0x0F, 0xA2, 0x58
    }; // note that there is no termination in a binary file
    // above set to 600cm long, 250cm width, all brakes off, 
    // all safety systems unavailable, accuracy not avail
    // in a forward gear, steering angle not avail
    // elev set to 0xF000 (unavailable)
    // note that the above items will each be overwritten 
    // when there is any data for it

    // Test to confirm it is the correct and expected size here, 
    // this was added to avoid human edit errors in the above
    if (sizeof(emptyArray) != BSM_BlOB_SIZE)
    {
        cout << "ERROR: Blank BLOB1 array seems to be incorrect size, correct this.!" << endl;
        cout << "ERROR: Expected to find " << BSM_BlOB_SIZE << " but found " << \
            sizeof(emptyArray) << " bytes." << endl;
    }
    for(int i=0; i<BSM_BlOB_SIZE; i++) // Now load this to the class for use
    {
        startingBlob1[i] = emptyArray[i];
    }
    Init(true); // reset blob buffer as well
}

BSM_BLOB1::~BSM_BLOB1()
{
}

// Reset the class vars to null values
// or value defined as "no data available" in the std
// if doBlobBuffer == true we reset the class blob array
void BSM_BLOB1::Init (bool doBlobBuffer)
{
    // DEBUG: cout << "In BSM_BLOB1::Init() call" << endl;
    msgCount                = 0;
    tempID                  = 0;
    dSecond                 = 0;
    lat                     = DEF_Latitude;
    lon                     = DEF_Longitude;
    elev                    = DEF_Elevation;
    semiMajorAcc            = DEF_semiMajorAcc;
    semiMinorAcc            = DEF_semiMinorAcc;
    orientationAcc          = DEF_orientationAcc;
    Pack_posAccuracy();    // Make composite value
    transState              = DEF_transState;
    speed                   = DEF_speed;
    Pack_transAndSpeed();  // Make composite value
    transAndSpeed[0]        = transState | ((speed & 0x1F00) >> 8);
    transAndSpeed[1]        = (speed & 0x00FF);
    heading                 = DEF_heading;
    steeringWheelAngle      = DEF_steeringWheelAngle;
    lonAccel                = DEF_Accel;
    latAccel                = DEF_Accel;
    vertAccel               = DEF_VertAccel;
    yawRate                 = DEF_YawRate;
    Pack_accelerationSet4Way(); // Make composite value
    brakeSystemStatus[0]    = DEF_brakeSystemStatusUpper;
    brakeSystemStatus[1]    = DEF_brakeSystemStatusLow;
    width                   = DEF_width;
    length                  = DEF_length;
    Pack_vehicleSize();    // Make composite value

    // Reset the working blob to the template values, if wanted
    if (doBlobBuffer == true)
        for (int i=0; i<BSM_BlOB_SIZE; i++) theBlob[i] = startingBlob1[i];
    // (The startingBlob1 is set once in init)
}


// Given pointers to two arrays of bytes, copy the length of the 
// source to the dest. Does no range checking or check of cnt value. 
void BSM_BLOB1::ByteCopy(unsigned char* dest, unsigned char* source, int cnt)
{   // no offsets or counts, just a blind copy
    for (int i=0; i<cnt; i++) dest[i] = source[i];
}


// Make composite value for this element using other class members
void BSM_BLOB1::Pack_posAccuracy()
{
    posAccuracy[0]          = semiMajorAcc;
    posAccuracy[1]          = semiMinorAcc;
    posAccuracy[2]          = ((orientationAcc & 0xFF00) >> 8);
    posAccuracy[3]          =  (orientationAcc & 0x00FF);
    //cout << "  semiMajorAcc "   << semiMajorAcc     << endl;
    //cout << "  semiMinorAcc "   << semiMinorAcc     << endl;
    //cout << "  orientationAcc " << orientationAcc   << endl;
}

// Make composite value for this element using other class members
void BSM_BLOB1::Pack_transAndSpeed()
{
    transAndSpeed[0]        = transState | ((speed & 0x1F00) >> 8);
    transAndSpeed[1]        = (speed & 0x00FF);
    //cout << "  transState " << transState   << endl;
    //cout << "  speed "      << speed        << endl;
}

// Make composite value for this element using other class members
void BSM_BLOB1::Pack_accelerationSet4Way()
{
    accelerationSet4Way[0]  = (unsigned char)((lonAccel & 0xFF00) >> 8);
    accelerationSet4Way[1]  = (unsigned char) (lonAccel & 0x00FF);
    accelerationSet4Way[2]  = (unsigned char)((latAccel & 0xFF00) >> 8);
    accelerationSet4Way[3]  = (unsigned char) (latAccel & 0x00FF);
    accelerationSet4Way[4]  = (unsigned char) vertAccel;
    accelerationSet4Way[5]  = (unsigned char)((yawRate & 0xFF00) >> 8);
    accelerationSet4Way[6]  = (unsigned char) (yawRate & 0x00FF);
}


// Make composite value for this element using other class members
void BSM_BLOB1::Pack_vehicleSize()
{
    vehicleSize[0] = ((width & 0x03FF) >> 2); // 0x03FC == 0x03FF here
    vehicleSize[1] = ((width & 0x0003) << 6) + ((length & 0x03F00) >> 8);
    vehicleSize[2] = (length & 0x00FF);
    //cout << "  width: "   << width      << endl;
    //cout << "  length: "   << length    << endl;
}


// Take the composite value for the multi-byte element
// and use it to update the class vars current values to match
void BSM_BLOB1::UnPack_posAccuracy()
{
    semiMajorAcc    = posAccuracy[0];
    semiMinorAcc    = posAccuracy[1];
    orientationAcc  = (posAccuracy[2] << 8 ) + posAccuracy[3];
    //cout << "  semiMajorAcc "   << semiMajorAcc     << endl;
    //cout << "  semiMinorAcc "   << semiMinorAcc     << endl;
    //cout << "  orientationAcc " << orientationAcc   << endl;
}

// Take the composite value for the multi-byte element
// and use it to update the class vars current values to match
void BSM_BLOB1::UnPack_transAndSpeed()
{
    transState  =   transAndSpeed[0] & 0xE0;
    speed       = ((transAndSpeed[0] & 0x1F) << 8) + transAndSpeed[1];
}

// Take the composite value for the multi-byte element
// and use it to update the class vars current values to match
void BSM_BLOB1::UnPack_accelerationSet4Way()
{
    lonAccel  = (accelerationSet4Way[0] << 8) + accelerationSet4Way[1];
    latAccel  = (accelerationSet4Way[2] << 8) + accelerationSet4Way[3];
    vertAccel =  accelerationSet4Way[4];
    yawRate   = (accelerationSet4Way[5] << 8) + accelerationSet4Way[6];
}

// Take the composite value for the multi-byte element
// and use it to update the class vars current values to match
void BSM_BLOB1::UnPack_vehicleSize()
{
    width  = ( vehicleSize[0]         << 2) + ((vehicleSize[1] & 0xC0) >> 6); 
    length = ((vehicleSize[1] & 0x3F) << 8) +   vehicleSize[2]; 
}



// Set the Blob (not the other class vars) to a predetermined 
// 'null' data state where each element is set to
// the "not available" data value when possible
// if the pointer is null, uses the class buffer as the destination
void BSM_BLOB1::ResetBlob (unsigned char* dest)
{
    if (dest == NULL) dest = &theBlob[0];
    // Now do the work
    // copy each byte above over to the passed in pointer
    for(int i=0; i<BSM_BlOB_SIZE; i++)
    {
        dest[i] = startingBlob1[i];
    }
}

// Pack the current class vars into the passed octet array
// if the pointer is null, uses the class buffer as the destination
// returns = 0 on success, = -1 on fail, 
int BSM_BLOB1::Pack(unsigned char* ablob)
{    
    if (ablob == NULL) ablob = &theBlob[0]; // select where to pack

    // Note that the mapping to specific bytes is hard coded here
    int offset; // a counter for the packing bytes
    unsigned char* pByte; // pointer used (by cast) to get at each byte 

    // CRITICAL DETAIL:
    // This was written for Intel platforms (which are all little endian)
    // this code will not work on any native big-endian machine, in such
    // a case the byte reversals shown below are NOT needed. 
    // ASN.1 is ALWAYS and without exception encoded as big endian
	// In the below, as general pattern, byte arrays go "in order"
	// while other variables are "backwards" 

    offset = 0; // set to start of blob array

    // do msgCnt   (1 byte)
    ablob[offset] = msgCount;
    offset = offset + 1; // move past to next item

    // do temp ID  (4 bytes)
    pByte =  (unsigned char *) &tempID;
    ablob[offset+0] = (unsigned char) *(pByte + 3); // msb
    ablob[offset+1] = (unsigned char) *(pByte + 2); // Note the bytes swapping 
    ablob[offset+2] = (unsigned char) *(pByte + 1); // used here and below !!
    ablob[offset+3] = (unsigned char) *(pByte + 0); // lsb
    offset = offset + 4;

    // do secMark  (2 bytes)
    pByte = (unsigned char* ) &dSecond;
    ablob[offset+0] = (unsigned char) *(pByte + 1); 
    ablob[offset+1] = (unsigned char) *(pByte + 0); 
    offset = offset + 2;

    // do the latitude data element (4 bytes)
    pByte = (unsigned char* ) &lat;
    ablob[offset+0] = (unsigned char) *(pByte + 3); 
    ablob[offset+1] = (unsigned char) *(pByte + 2); 
    ablob[offset+2] = (unsigned char) *(pByte + 1); 
    ablob[offset+3] = (unsigned char) *(pByte + 0); 
    offset = offset + 4;

    // do the longitude data element (4 bytes)
    pByte = (unsigned char* ) &lon;
    ablob[offset+0] = (unsigned char) *(pByte + 3); 
    ablob[offset+1] = (unsigned char) *(pByte + 2); 
    ablob[offset+2] = (unsigned char) *(pByte + 1); 
    ablob[offset+3] = (unsigned char) *(pByte + 0); 
    offset = offset + 4;

    // do elevation data element
    pByte = (unsigned char* ) &elev;
    ablob[offset+0] = (unsigned char) *(pByte + 1); 
    ablob[offset+1] = (unsigned char) *(pByte + 0); 
    offset = offset + 2;

    // do the accuracy data frame 
    Pack_posAccuracy(); // Update struc before use
    pByte = (unsigned char* ) &posAccuracy;
    ablob[offset+0] = (unsigned char) *(pByte + 0); 
    ablob[offset+1] = (unsigned char) *(pByte + 1); 
    ablob[offset+2] = (unsigned char) *(pByte + 2); 
    ablob[offset+3] = (unsigned char) *(pByte + 3); 
    offset = offset + 4;
 
    // do the Trans and Speed data element 
    Pack_transAndSpeed(); // Update struc before use
    pByte = (unsigned char* ) &transAndSpeed;
    ablob[offset+0] = (unsigned char) *(pByte + 0); 
    ablob[offset+1] = (unsigned char) *(pByte + 1); 
    offset = offset + 2;

    // do the Heading data element 
    pByte = (unsigned char* ) &heading;
    ablob[offset+0] = (unsigned char) *(pByte + 1); 
    ablob[offset+1] = (unsigned char) *(pByte + 0); 
    offset = offset + 2;

    // do the SteeringWheelAngle data element 
    ablob[offset+0] = steeringWheelAngle;
    offset = offset + 1;

    // do Accel 4 way element
    Pack_accelerationSet4Way(); // Update struc before use
    pByte = (unsigned char* ) &accelerationSet4Way;
    ablob[offset+0] = (unsigned char) *(pByte + 0); 
    ablob[offset+1] = (unsigned char) *(pByte + 1); 
    ablob[offset+2] = (unsigned char) *(pByte + 2); 
    ablob[offset+3] = (unsigned char) *(pByte + 3); 
    ablob[offset+4] = (unsigned char) *(pByte + 4); 
    ablob[offset+5] = (unsigned char) *(pByte + 5); 
    ablob[offset+6] = (unsigned char) *(pByte + 6); 
    offset = offset + 7;

    // do the brakeSet data element 
    ablob[offset+0] = brakeSystemStatus[0]; 
    ablob[offset+1] = brakeSystemStatus[1];
    offset = offset + 2;

    // do the width and length data elements,
    Pack_vehicleSize(); // Update struc before use
    ablob[offset+0] = vehicleSize[0]; 
    ablob[offset+1] = vehicleSize[1]; 
    ablob[offset+2] = vehicleSize[2]; 
    offset = offset + 3;
    
    assert(offset == BSM_BlOB_SIZE); //test for right byte count at end
    return(0);
}



// UnPack the passed octet array into the class vars
// if the pointer is null, uses the class buffer as the source
// returns = 0 on success, = -1 on fail, 
int BSM_BLOB1::UnPack(unsigned char* ablob)
{
    // cout << "In method BSM_BLOB1::UnPack() which is not ready for use yet." << endl;
    if (ablob == NULL) ablob = &theBlob[0]; // select source to use

    // Note that the mapping to specific bytes is hard coded here
    int offset; // a counter into the blob bytes
    unsigned char   byteA;  // force to unsigned this time,
    unsigned char   byteB;  // we do not want a bunch of sign extension 
    unsigned char   byteC;  // math mucking up our combine logic
    unsigned char   byteD;  
    // CRITICAL DETAIL:
    // This was written for Intel platforms (which are all little endian)
    // this code will not work on any native big-endian machine, in such
    // a case the byte reversals shown below are NOT needed. 
    // ASN.1 is ALWAYS and without exception encoded as big endian

    // Reset the class vars, but NOT the local blob IF that is being 
    // used as the data course we will unpack from
    // else reset it, as it will no longer match the data anymore anyway
    if (ablob == &theBlob[0])
        Init(false); // do not wipe it as we are getting the data from here
    else
        Init(true); // wipe it clean, returns it the starting template values
    offset = 0; // set to start of blob array

    // do msgCnt   (1 byte)
    msgCount = ablob[offset];
    offset = offset + 1; // move past to next item

    // do temp ID  (4 bytes)
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    byteC = ablob[offset+2];
    byteD = ablob[offset+3];
    tempID = (long)((byteA << 24) + (byteB << 16) + (byteC << 8) + (byteD));
    offset = offset + 4;

    // do secMark  (2 bytes)
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    dSecond = (int)(((byteA << 8) + (byteB))); // in fact unsigned
    offset = offset + 2;

    // do the latitude data element (4 bytes)
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    byteC = ablob[offset+2];
    byteD = ablob[offset+3];
    lat = (unsigned long)((byteA << 24) + (byteB << 16) + (byteC << 8) + (byteD));
    offset = offset + 4;

    // do the longitude data element (4 bytes)
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    byteC = ablob[offset+2];
    byteD = ablob[offset+3];
    lon = (unsigned long)((byteA << 24) + (byteB << 16) + (byteC << 8) + (byteD));
    offset = offset + 4;

    // do elevation data element 
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    elev =  (unsigned short)( (unsigned long)((byteA << 8) + (byteB))); 
    offset = offset + 2;

    // do the accuracy data frame 
    posAccuracy[0] = ablob[offset+0];
    posAccuracy[1] = ablob[offset+1];
    posAccuracy[2] = ablob[offset+2];
    posAccuracy[3] = ablob[offset+3];
    offset = offset + 4;
    UnPack_posAccuracy();  // fill the children elements

    // do the Speed and Transmission state data element 
    transAndSpeed[0] = ablob[offset+0];
    transAndSpeed[1] = ablob[offset+1];
    offset = offset + 2;
    UnPack_transAndSpeed();  // fill the children elements

    // do the heading data element 
    byteA = ablob[offset+0];
    byteB = ablob[offset+1];
    heading = (short)((byteA << 8) + (byteB));
	// A valid heading is never be beyond 28001 but as this in theory fits into the
	// ASN.1 DE space it is possible to send it. A validating ASN tool will catch it,
	// if validation is enabled. here we correct this if ever found
	if (heading > 28800) 
	{
		heading = heading - 28800; // Subtract 360.0 degrees
		heading = 28800 - heading; // flip sign
	}
    offset = offset + 2;

    // do the steeringWheelAngle data element 
    steeringWheelAngle = ablob[offset];
    offset = offset + 1;

    // Do the 4-way acccel set blob
    accelerationSet4Way[0] = ablob[offset+0];
    accelerationSet4Way[1] = ablob[offset+1];
    accelerationSet4Way[2] = ablob[offset+2];
    accelerationSet4Way[3] = ablob[offset+3];
    accelerationSet4Way[4] = ablob[offset+4];
    accelerationSet4Way[5] = ablob[offset+5];
    accelerationSet4Way[6] = ablob[offset+6];
    offset = offset + 7;
    UnPack_accelerationSet4Way();  // fill the children elements

    // do the brakeSystemStatus data element
    brakeSystemStatus[0] = ablob[offset+0];
    brakeSystemStatus[1] = ablob[offset+1];
    offset = offset + 2;

    // do the vehicle size (width and length) data element
    vehicleSize[0] = ablob[offset+0];
    vehicleSize[1] = ablob[offset+1];
    vehicleSize[2] = ablob[offset+2];
    offset = offset + 3;
    UnPack_vehicleSize();  // fill the children elements

    assert(offset == BSM_BlOB_SIZE); // test for all bytes accounted for
    return(0);
}




// Extra credit but useful for testing:
// Extra credit but useful for testing:

// Given a blob, add other data to make a full BMS message
// and return a pointer to that octet stream, return null
// on error, this file can be saved and other tools used on it
// as with other calls, passing in NULL will cause the 
// class buffer to be used as the blob source.
unsigned char* BSM_BLOB1::MakeBSM(unsigned char* ablob)
{
    if (ablob == NULL) ablob = &theBlob[0]; // select source to use

    // allocate space on heap,
    unsigned char* theData = new unsigned char[BSM_MSG_SIZE];

    // build (static) header for message,
    int i = 0;
    theData[i++] = 0x30;    // ASN sequence tag
    theData[i++] = 0x2B;    // length of sequence to follow (rest of message)
    theData[i++] = 0x80;    // first content tag, message ID
    theData[i++] = 0x01;    // content length, 1 byte
    theData[i++] = 0x02;    // content value, msgID == 2 for BSM
                            // = DSRCmsgID_basicSafetyMessage
    theData[i++] = 0x81;    // next content tag, blob
    theData[i++] = 0x26;    // content length, 38 bytes, blob follows
    assert(i == BSM_BlOB_INDEX); // confirm header is right size

    // append rest of blob to it (copy it)and return
    for (int j=0; j<BSM_BlOB_SIZE; j++) 
        theData[i++] = ablob[j];

    assert(i == BSM_MSG_SIZE); // confirm final msg is right size
    return (theData); // user will delete it when done
}


// Given a byte set from a full BSM message, extract the blob.
// data from it, place it in the class' blob storage.
// if load == true, this call will also set the class vars, 
// by calling UnPack() when done. 
void BSM_BLOB1::GetBLOBfromBSM(unsigned char* BSMSource, bool loadIt)
{
    if (BSMSource != NULL)
    {
        // copy message blob into class storage
        int i = BSM_BlOB_INDEX;
        for (int j=0; j<BSM_BlOB_SIZE; j++) 
            theBlob[j] = BSMSource[i+j];
        if (loadIt == true) UnPack(theBlob); //update class to match
    }
}


// Below was added for a 'thin' client use as he does not have the full ASN defs
#define SIZEOF_AccelerationSet4Way	7  // sizeof(accelerationSet4Way)
#define SIZEOF_brakeSystemStatus	2  // sizeof(brakeSystemStatus)
#define SIZEOF_PosAccuracy			4  // sizeof(posAccuracy)
#define SIZEOF_TransAndSpeed		2  // sizeof(transAndSpeed))
#define SIZEOF_VehicleSize			3  // sizeof(vehicleSize)
#define SIZEOF_TheBlob	BSM_BlOB_SIZE  // sizeof(theBlob)


// Compare the passed class instance with this one, 
// if EVERY element matches, returns true, else false
// note this is an exact match, there is no 'error' used
bool BSM_BLOB1::IsEqual(BSM_BLOB1* otherOne)
{
    bool result = true;
    if (otherOne != NULL)
    {
        // check the primitive elements first
        if ((dSecond            == otherOne->dSecond) &&
            (elev               == otherOne->elev ) &&
            (heading            == otherOne->heading) &&
            (lat                == otherOne->lat) &&
            (latAccel           == otherOne->latAccel) &&
            (length             == otherOne->length) &&
            (lon                == otherOne->lon) &&
            (lonAccel           == otherOne->lonAccel) &&
            (msgCount           == otherOne->msgCount) &&
            (orientationAcc     == otherOne->orientationAcc) &&
            (semiMajorAcc       == otherOne->semiMajorAcc) &&
            (semiMinorAcc       == otherOne->semiMinorAcc) &&
            (speed              == otherOne->speed) &&
            (steeringWheelAngle == otherOne->steeringWheelAngle) &&
            (tempID             == otherOne->tempID) &&
            (transState         == otherOne->transState) &&
            (vertAccel          == otherOne->vertAccel) &&
            (width              == otherOne->width) &&
            (yawRate            == otherOne->yawRate) )
        {
            // check various complex elements
            // keep in mind that unless these have been updated since
            // the last change, they may be out of date even if
            // the above 'raw' inputs are not
            if (DoBytesMatch(accelerationSet4Way, 
                    otherOne->accelerationSet4Way, 
                    SIZEOF_AccelerationSet4Way) == true)
                    // sizeof(accelerationSet4Way)) == true)
                // next step, each indenting as we go
                if (DoBytesMatch(brakeSystemStatus, 
                        otherOne->brakeSystemStatus, 
                        SIZEOF_brakeSystemStatus) == true)
                        // sizeof(brakeSystemStatus)) == true)
                    if (DoBytesMatch(posAccuracy, 
                            otherOne->posAccuracy, 
                            SIZEOF_PosAccuracy) == true)
                            // sizeof(posAccuracy)) == true)
                        if (DoBytesMatch(transAndSpeed, 
                                otherOne->transAndSpeed, 
                                SIZEOF_TransAndSpeed) == true)
                                // sizeof(transAndSpeed)) == true)
                            if (DoBytesMatch(vehicleSize, 
                                    otherOne->vehicleSize, 
                                    SIZEOF_VehicleSize) == true)
                                    // sizeof(vehicleSize)) == true)
                                if (DoBytesMatch(theBlob, 
                                        otherOne->theBlob, 
                                        SIZEOF_TheBlob) == true)
                                        // sizeof(theBlob)) == true)
                                    return (true); // all items checked

            return (false); // failed on a compound element
        }
        else return (false); // failed on simple elements
    }
    else return (false); // fails, no objec ptr 
}



// Test scrap for use in other code
// Test scrap for use in other code
// Test scrap for use in other code
/*******
    // Aug 26th test the BLOB1_util code here

    // make two instances
    BSM_BLOB1 aBC;  // blob A
    BSM_BLOB1 bBC;  // blob B

    // load blob A with some test data 
    aBC.dSecond = 0xEA60;
    aBC.lat     = 0xAABBCCDD;
    aBC.lon     = 0x11223344;
    aBC.width   = 0x10;
    aBC.length  = 0x08;
    aBC.Pack(NULL);  // pack a blob for use (using the class storage)
    // create another blob (outside store) and pack it
    unsigned char* myBlob = new unsigned char[BSM_BlOB_SIZE];
    aBC.Pack(myBlob);

    // The above two blob contents should be identical, test this
    if ( DoBytesMatch(myBlob, aBC.theBlob, BSM_BlOB_SIZE) == false)
        cout << " We DO NOT have a BLOB match - between aBC and myBlob" << endl;

    // Make a message with it (adds other trival header bytes)
    unsigned char* theMsg = aBC.MakeBSM(NULL);
    if (theMsg != NULL) // save a file
        WriteBytesToFile("testBSMmsg_A.ber", theMsg, BSM_MSG_SIZE);

    // if we were to un-pack what we just created, the class vars
    // should be the same, and the process should be reversable
    // lets confirm this with the other blob class as the recipient
    bBC.UnPack( aBC.theBlob );  // load it in  (set vars)
    bBC.Pack(NULL);             // load it out (set blob)

    // Compare the two classes to detect any differences
   // This call is the same:  if (bBC.IsEqual( &aBC ) == true)
   if (aBC.IsEqual( &bBC ) == true)
        cout << " We have a 2-way msg match" << endl;
   else
        cout << " We DO NOT have a 2-way msg match - ERROR" << endl;

    // finally we should be able to compare the newest blob we just made  
    // and it should match the prior two 
    // (any message match will have the same results)
    if ( DoBytesMatch(bBC.theBlob, aBC.theBlob, BSM_BlOB_SIZE) == false)
        cout << " We DO NOT have a BLOB match - between aBC and bBC" << endl;
    if ( DoBytesMatch(bBC.theBlob, myBlob, BSM_BlOB_SIZE) == false)
        cout << " We DO NOT have a BLOB match - between bBC and myBlob" << endl;

**********/


// Below taken from the Util class for use here (no ASN present for this user)
// Compare the passed two pointers for the indicated number of bytes,
// if they match at all points, return true, otherwise false
// basically just a string compare, but deals with nulls
bool BSM_BLOB1::DoBytesMatch(unsigned char* first, unsigned char* second, int cnt)
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





// end of file BLOB1_util.cpp
