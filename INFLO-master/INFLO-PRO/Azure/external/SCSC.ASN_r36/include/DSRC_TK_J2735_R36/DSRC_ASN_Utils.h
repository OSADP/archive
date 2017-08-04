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

#ifndef	_DSRC_Utils_H_
#define	_DSRC_Utils_H_


#include <asn_application.h>

#include "AlaCarte.h"  // these allow getting to every message type
#include "BasicSafetyMessage.h"
#include "BasicSafetyMessageVerbose.h"
#include "CommonSafetyRequest.h"
#include "EmergencyVehicleAlert.h"
#include "IntersectionCollision.h"
#include "MapData.h"
#include "NMEA-Corrections.h"
#include "ProbeDataManagement.h"
#include "ProbeVehicleData.h"
#include "RoadSideAlert.h"
#include "RTCM-Corrections.h"
#include "SPAT.h"
#include "SignalRequestMsg.h"
#include "SignalStatusMessage.h"
#include "TravelerInformation.h"


#ifdef __cplusplus
extern "C" {
#endif



// ADD when NOT using VS2005/08/10 compilers
// as per Mother Microsoft and windef.h use
#ifndef BYTE
	#define BYTE  unsigned char
#endif

// CAUTION: Be aware: different byte defs can cause problems when
// the upper bit is set and logical operators are used
#ifndef sBYTE
    #define sBYTE  char
#endif
#ifndef uBYTE
    #define uBYTE  unsigned char
#endif



#if !defined(M_PI)
    #define M_PI       (3.14159265358979323846)	// 360 deg
    #define M_2PI      (2 * M_PI)				// 180 deg
    #define M_HalfPI   (M_PI / 2)				// 90 deg
#endif

// A few unit convert values used to go in and out of ASN units
#define DEG2ASNunits  (10000000.0)
#define VERT2ASNunits (10.0)
#define HEADINGunits2DEG (0.0125) // 80 DSRC unit per degree
#define CMperFoot (30.48)   // 1 foot = 0.3048 meters
							// and 1 Meter = 3.2808399 Feet


// Generic Bit defines (starts with one not zero)
// Note that these are used to define bit fields AND the order of
// the menu item indexes in the bit pop up menu
// (in which case it may not map to the bit that will change)
#define BIT_01  (0x0001)  // There is no bit zero!
#define BIT_02  (0x0002)
#define BIT_03  (0x0004)
#define BIT_04  (0x0008)
#define BIT_05  (0x0010)
#define BIT_06  (0x0020)
#define BIT_07  (0x0040)
#define BIT_08  (0x0080)
#define BIT_09  (0x0100)
#define BIT_10  (0x0200)
#define BIT_11  (0x0400)
#define BIT_12  (0x0800)
#define BIT_13  (0x1000)
#define BIT_14  (0x2000)
#define BIT_15  (0x4000)
#define BIT_16  (0x8000)
#define BIT_17  (0x00010000)
#define BIT_18  (0x00020000)
#define BIT_19  (0x00040000)
#define BIT_20  (0x00080000)
#define BIT_21  (0x00100000)
#define BIT_22  (0x00200000)
#define BIT_23  (0x00400000)
#define BIT_24  (0x00800000)
#define BIT_25  (0x01000000)
#define BIT_26  (0x02000000)
#define BIT_27  (0x04000000)
#define BIT_28  (0x08000000)
#define BIT_29  (0x10000000)
#define BIT_30  (0x20000000)
#define BIT_31  (0x40000000)
#define BIT_32  (0x80000000)

// Defines used by multiple apps using the ASN code base
#define MAX_MSG_SIZE  5000 // a pure guess for now

// the encoding styles we support for output files
#define ENCODE_ASN  (0)
#define ENCODE_XML  (1)
#define ENCODE_TXT  (2)


// Get core DSRC ASN1 message items we need
#include "OCTET_STRING.h"
#include "MapData.h"	// the MAP message type
#include "SPAT.h"	    // the SPAT message type
#include "BasicSafetyMessage.h"
#include "BasicSafetyMessageVerbose.h"
#include "asn_SEQUENCE_OF.h"


// Various utility calls here (no class)
// Various utility calls here (no class)
// Various utility calls here (no class)


// Return octet bytes as a long
long Octet2Num(OCTET_STRING_t* basePtr);

// Return octet bytes in desc string
// this, and others, returns a count of octets used
int Octet2Chars(OCTET_STRING_t* basePtr, char* pDesc, int maxCnt);

// Return octet bytes in desc string as a set of hex values
int Octet2HexAsChars(OCTET_STRING_t* basePtr, char* pDesc, int maxCnt, bool add0x);

// Given a char pointer to data, place its content into an octet buffer
int Chars2OctetBuff(OCTET_STRING_t* basePtr, char* pSource, int cnt);

// Given a pointer to bytes of data, place its content into an octet buffer
int Bytes2OctetBuff(OCTET_STRING_t* basePtr, char* pSource, int cnt);

// Given an integer value, stuff it as an ASN octets using as few bytes as can
int Long2OctetBuff(OCTET_STRING_t* basePtr, long theValue);

// Given a string representing hex values and commas, stuff it as an ASN octets
// until termination is reached, return the count of bytes
int HexAsChars2OctetBuff(OCTET_STRING_t* basePtr, char* pDesc);

// Given a 1~2 char terminated string representing a hex value,
// return its Signed byte value as an int
//int HexAsChars2SignedInt(char* pText);
// Given a 1~2 char terminated string representing a hex value,
// return its UNsigned byte value as an int
//int HexAsChars2UnSignedInt(char* pText);

// Given a hex string (0~9 and upper/lower A~F) return an int value
unsigned int htoi (const char *ptr);

// Given one the ASN 'list' objects used to hold sequence of things
// set all the values to null or zero, when first created ASN leaves these values
// in a random state that can cause problems, use only on new objects!
// use the ASN calls to add and remove items (see ASN_SEQUENCE_OF.h)
void ZeroNewList(void* thePtr);

void  Append(const char* desc, const char* source);

// Return only the path part of a full path name and file name
void JustPath(char* fullFilePath, char* justPath);

void Swap2bytes (uBYTE * ptr);
void Swap3bytes (uBYTE * ptr);
void Swap4bytes (uBYTE * ptr);
void SwapNbytes (uBYTE * ptr, int byteCnt);
	// Several byte swapping routines (to deal with big-endian packing rules of ASN)


unsigned short icrc(unsigned short crc, unsigned char* pBuffer, unsigned long len, short jinit, int jrev);
	// compute a 16 bit CRC for array bufptr of length len,
	// call with with jinit=0 and jrev=-1 and crc=0 to compute a message CRC
	// call with with jinit=-1 and jrev=-1 and crc=the message CRC to check a message CRC (correct result =0)

unsigned short DoCRC (uBYTE * pBYTES, int theLen);
	// Given a stream of bytes, return the new CRC




// ASN Support calls for DSCR Rev 36 uses
// ASN Support calls for DSCR Rev 36 uses
// ASN Support calls for DSCR Rev 36 uses
// Move to a file called ASN_R36_Util.h when mature
// All the nasty bit-bytes and pointer setup items are found here
// need to make the ASN1c output useful to an app developer



// Return an allocated by empty intersection object
// Assign the passed value to the ID octets, all else as null
// Used to create a starting point with pointers set to null
Intersection* ASN_CreateEmptyIntersection(long theId);


// Return an allocated but empty approach object with requested
// empty in-bound and/or out-bound approach object stubs
// and all the approach lane content types of each set to null values
// typically the object pointed to is added to an intersection
// once additional lanes are added and completed
ApproachObject* ASN_CreateEmptyApproachObj(bool hasInbound, bool hasOutbound);


// Return an allocated but empty driving lane collection,
// set to null values and return its ptr
//(struct Approach::drivingLanes *) ASN_CreateEmptyDrivingLaneSet();
// same for each type of lane
void* ASN_CreateEmptyDrivingLaneSet();
void* ASN_CreateEmptyBarriersLaneSet();
void* ASN_CreateEmptyCrosswalksLaneSet();


// Return an allocated but empty driving lane,
// set to null values and return its ptr
// same for each type of lane
VehicleReferenceLane* ASN_CreateEmptyDrivingLane();
BarrierLane* ASN_CreateEmptyBarrierLane();
CrosswalkLane* ASN_CreateEmptyCrosswalkLane();


// Convert floating point lat-long values to be valid ASN
// units, dealing with sign and LSB precision, returning
// a valid long for direct use in the ASN messages
// no need to do this with lats
#define FLL2ASNLL   FloatLatLong2_ASNLatLong
long FloatLatLong2_ASNLatLong(double theValue, bool isLong);

// Convert a long LL to a float LL using DSRC 1/10th Micro degree units
double LLlong2Float(long theValue);


// Given a value in radians, return its value in degrees
// surely we do not need this call?? Make a macro
double Rads2degs(double rads);

// Given a value in degrees, return its value in radians
double Degs2rads(double degs);

// Given a value in radians, return its value in degrees
// suitable for text alignments (avoiding inverted text)
double Rads2degs4text(double rads);
double Degs2degs4text(double angle);

// A utility to toggle between DSRC 'zero is north up' and
// math coordinate 'zero to the right along X' forms
double ToggleDeg(double angle);

// Given an angle (in deg) that may be 'near' an axis,
// snap to that axis and return the value.  This is used
// to make text at a slight angle more readable
double SnapTo(double angle);

// TODO: add calls to pass pointers as well
// TODO: Add a 3D method to this as well

// Compute a precise 2D Vincenty distance between two points on the ellipse
double DistVincenty(double lat1, double lon1, double lat2, double lon2);

// Given a precise lat-lon point and an 'step' amount (all doubles)
// Step is typically a degree, or a micro degree, or a 1/10th micro degree (the DSRC LSB unit)
// compute the conversion rate into meters (NOT cm) for lat and long and return them
// using the DistVincenty() call to compute a line (of step length in deg) along each axis
void VincentyConstants(double startlat, double startlon,
					   double step,
					   double* convertLat, double* convertLon);

// Display to cout the convert rates for the passed point,
// simply a clean way to get the value to the screen for others
// note this just uses the DSRC LSB as the distance unit
void DisplayVincentyConstants(double thelat, double thelon);

// Test Vincenty code here, dump results to console
void TestVincenty();


// Simple File Support Calls Follow
// Simple File Support Calls Follow
// Simple File Support Calls Follow


// Write the passed bytes, using the passed count, to the passed file name
// return number of bytes written on success or -1 on error
size_t WriteBytesToFile(char* fileName, unsigned char* pData, int dataCnt);

// Given a valid file, read it into memory and return the passed pointer
// pointing to the created array on the heap to hold it
// on error  will return null for ptr and -1 for cnt
unsigned char* FileInToMemoryBytes(char* theFile, size_t* theCnt, bool vebose);

// Compare the passed two pointers for the indicated number of bytes,
// if they match at all points, return true, otherwise false
bool DoBytesMatch(unsigned char* first, unsigned char* second, int cnt);

// General purpose float rounding routine,
// taken from other SCSC DGPS work
// rounds the float to the passed value
#define Rnd_One (1.0)
#define Rnd_TenCM (0.1)
#define Rnd_OneTenthMicroDegree (0.0000001)
double RoundValue(double theValue, double quant);


// Convert into and out of the heading units used in DSRC messages
// from degrees and rad values. Note that DSRC is a North-Up system
double DSRCHeading2Degs(int heading);
double DSRCHeading2Rads(int heading);
int    Degs2DSRCHeading(double dsrcAngle);
int    Rads2DSRCHeading(double dsrcAngle);


// Convert from feet to cm and back,
// returns doubles for each
double Ft2Cm(double length);
double Cm2Ft(double length);


// Express Time in HH:MM:SS form into the passed string
void Time2HH_MM_SS(time_t theTime, char* text);

// Express Time in MM:SS form into the passed string
void Time2MM_SS(time_t theTime, char* text);


// Convert two integer numbers in a modulo roll-over system
// to the distance between them, a match returns the limit
// value.  The value new is presumed to advance (by+)
// Returned sign is always positive
long ModuloDist(long newV, long oldV, long limitV);


// Given a valid char ptr to read and one to write to, extract
// the next line found from the passed-in starting point
// using the passed-in delimiter and return the index
// point in the source string we ended up at
// (to be used to start the next line read)
long GetNextLine(char* theSource, char* theLine, char* delimit, int startPt);


//
// Routines to assist in decoding and determining a raw message type
//

// Recover and return the message type from a byte array
int GetMsgType(BYTE* thePtr, int skip);

// Recover and return the value count for this byte array in ASN
int GetMsgWordCnt(BYTE* thePtr);

// Returns number of bytes used in the ASN "length" wd count of a message of this size
int BytesUsed(int size);

// Return true if is a valid and useful DSRC msgID (the skips zero value of
// DSRCmsgID_reserved but allows local content values
int IsValidMsgType(int value);

// Given a type of message (by msgId, an int value) return
// the address of the message type that this is
// used for various pointer casting needs
asn_TYPE_descriptor_s* GetMesssageTypeFromId(int messageType);



//
// Examples of ASN use which you will want to create in your own code base:
//


// Encode, An example any message structures into the passed array of bytes
ssize_t DSRC_serializer(asn_TYPE_descriptor_t *type_descriptor, // message type
						void *theMsg,  //the ptr to the message struc
						BYTE *theBuffer); // the ptr to the buff to fill

// Decode, An example DSRC_UNserializer for the general case,
ssize_t DSRC_UNserializer(void** theMsgStruc,   //the void** ptr to the struc to fill
						  BYTE*  theBuffer,		// the ptr to the buff to read from
						  int    theInputSize,  // the size of the buff contents
						  bool   vb);			// verbose output control



// Also, please look at these very useful calls found in OCTET_STRING.h
/*****

// This function clears the previous value of the OCTET STRING (if any)
int OCTET_STRING_fromBuf(OCTET_STRING_t *s, const char *str, int size);

// Handy conversion from the C string into the OCTET STRING.
#define	OCTET_STRING_fromString(s, str)	OCTET_STRING_fromBuf(s, str, -1)

// Allocate and fill the new OCTET STRING and return a pointer to the newly
OCTET_STRING_t *OCTET_STRING_new_fromBuf(asn_TYPE_descriptor_t *td, const char *str, int size);
// It is STRONGLY suggested you use the above to allocate space for this type

****/



#ifdef __cplusplus
}
#endif

#endif	/* _DSRC_Utils_H_ */

