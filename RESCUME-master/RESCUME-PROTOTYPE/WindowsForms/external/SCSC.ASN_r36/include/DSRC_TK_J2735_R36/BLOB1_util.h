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
// File: BLOB1_util.h
//
// This file holds a utility code class to translate data
// into and out of the packed octets of the BSM BLOB1 data element
// unlike most of the toolkit, it doe snot require ANY of the ASN.1
// library routines to be present to create a simple Part I only BSM
// thus, for this simple jobs (or those fearful of ASN) it is
// very quick way to create and read a BSM
//
// DCK Create Aug 26th 2011  <davidkelley@ITSware.net> 
// SCSC Header to go here
//
// Change History
//  From several prior efforts, this copy deals ONLY with the 
//  bytes of the ASN so that the different app unit translate 
//  needs can live elsewhere and this code can be better 
//  re-used over different projects. 
//  Do not modify this code to add you own units, rather
//  call it from you own class and do the (any) translate there
//  the BasicVehicle class is an example of this for VISIM units
//  General Release Sept 10th 2011
//
//
/////////////////////////////////////////////////////////////////

#ifndef BLOB1_UTIL  // import this file for first time

#define BLOB1_UTIL

#ifndef BSM_BlOB_SIZE
    #define BSM_BlOB_SIZE  (38)
#endif
#ifndef BSM_BlOB_INDEX
    #define BSM_BlOB_INDEX  (07)  // where the blob data starts in the message itself
#endif
#ifndef BSM_MSG_SIZE
    #define BSM_MSG_SIZE  (45)
#endif
#ifndef DEG2ASNunits
    #define DEG2ASNunits  (10000000.0)  // used for ASN 1/10 MICRO deg to unit converts
#endif

// Default "no data available" values for for each item (found in the ASN modules as well)
#define DEF_Latitude        (900000001)
#define DEF_Longitude       (1800000001)
#define DEF_Elevation       (0xF000)
#define DEF_semiMajorAcc    (0xFF)
#define DEF_semiMinorAcc    (0xFF)
#define DEF_orientationAcc  (0xFFFF)
#define DEF_transAndSpeed   (0x0000)
#define DEF_transState      (0x00)      // This may not be what we always want
#define DEF_speed           (0x0000)
#define DEF_heading         (0x0000)    // there is no unavailable value, 
                            // it has been suggested (by CAMP) we use 28801 
							// but that is not in the std yet
#define DEF_steeringWheelAngle  (127)
#define DEF_Accel           (2001)      // use for lateral and long accels
#define DEF_AccelHEX        (0x07D1)    // use for lateral and long accels
#define DEF_VertAccel       (-127)  
#define DEF_VertAccelHEX    (0x7F)  
#define DEF_YawRate         (0)         // there is no unavailable value
#define DEF_brakeSystemStatusUpper  (0x00)  
#define DEF_brakeSystemStatusLow    (0x00)  
#define DEF_width           (0x00)  // std has no default, use 1 cm 
#define DEF_length          (0x00)  // std has no default, use 1 cm 


class BSM_BLOB1 
{
// Class Vars for the one BSM BLOB1 Data Element and its contents
// Note that all class values are kept in the native ASN1 message units
// And any translation to-from application units is done elsewhere
// This class simply packs and unpacks a byte array with 
// this data in the blob octet format defined by the SAE J2735 standard
public:
    // The comment lines below come from the ASN.1 of the 
    // adopted SAE J2735 standard, 
    // consult that standard for details about each element

    // NOTE we use UNSIGNED chars here (rather than a byte macro
    // which may be either signed or unsigned). This is to ensure the 
    // logical byte combining operations DO NOT extend the MSB
    // as if it were a sign bit in some operations.

    // -- msgCnt      MsgCount,            -x- 1 byte
    unsigned char msgCount;
    //-- id          TemporaryID,          -x- 4 bytes
    long tempID;
    //-- secMark     DSecond,              -x- 2 bytes
    unsigned short dSecond;
    //-- pos      PositionLocal3D,
        //  -- lat       Latitude,             -x- 4 bytes
        long lat;
        //  -- long      Longitude,            -x- 4 bytes
        long lon;
        //  -- elev      Elevation,            -x- 2 bytes
        unsigned short elev;
    //  -- accuracy  PositionalAccuracy,   -x- 4 bytes
    unsigned char posAccuracy[4];
        //-- And the bytes defined as follows
        //
        //-- Byte 1: semi-major accuracy at one standard dev 
        //-- range 0-12.7 meter, LSB = .05m
        //-- 0xFE=254=any value equal or greater than 12.70 meter
        //-- 0xFF=255=unavailable semi-major value 
        unsigned char semiMajorAcc;
        //
        //-- Byte 2: semi-minor accuracy at one standard dev 
        //-- range 0-12.7 meter, LSB = .05m
        //-- 0xFE=254=any value equal or greater than 12.70 meter
        //-- 0xFF=255=unavailable semi-minor value 
        unsigned char semiMinorAcc;
        //
        //-- Bytes 3-4: orientation of semi-major axis 
        //-- relative to true north (0~359.9945078786 degrees)
        //-- LSB units of 360/65535 deg  = 0.0054932479
        //-- a value of 0x0000 =0 shall be 0 degrees
        //-- a value of 0x0001 =1 shall be 0.0054932479degrees 
        //-- a value of 0xFFFE =65534 shall be 359.9945078786 deg
        //-- a value of 0xFFFF =65535 shall be used for orientation unavailable 
        //-- (In NMEA GPGST)
        unsigned short orientationAcc;
    //-- motion   Motion,
    //  -- speed     TransmissionAndSpeed, -x- 2 bytes
    unsigned char transAndSpeed[2];
        unsigned char transState;
        unsigned short speed;
    //  -- heading   Heading,              -x- 2 byte
    unsigned short heading;
    //  -- angle     SteeringWheelAngle    -x- 1 bytes
    unsigned char steeringWheelAngle;
    //  -- accelSet  AccelerationSet4Way,  -x- 7 bytes 
    unsigned char accelerationSet4Way[7];
        //-- composed of the following:
        //-- SEQUENCE {
        //--    long Acceleration,          -x- Along the Vehicle Longitudinal axis
        short lonAccel;
        //--    lat  Acceleration,          -x- Along the Vehicle Lateral axis
        short latAccel;
        //--    vert VerticalAcceleration,  -x- Along the Vehicle Vertical axis
        char vertAccel;
        //--    yaw  YawRate
        short yawRate;
    //-- control  Control,
    //-- brakes      BrakeSystemStatus,    -x- 2 bytes
    unsigned char brakeSystemStatus[2];
    //-- basic    VehicleBasic,
    //-- size        VehicleSize,          -x- 3 bytes
    unsigned char vehicleSize[3];
        unsigned short width;
        unsigned short length;

    // The byte array below MAY be used if the user passes in a null value 
    // for a pointer in a call, otherwise the array the passed pointer
    // points to will be used and this data will not be changed. By this means
    // a user can manage arrays as they see fit, or let the class deal with it. 
    // keep in mind these bytes are ONLY updated on a pack() call and if a class
    // value is change will not longer reflect the correct encoding for the 
    // new value. In a similar way, an unpack() call updates class vars from 
    // the bytes it is pointed out. 
    unsigned char theBlob[BSM_BlOB_SIZE];   // holds a working message

private:
    unsigned char startingBlob1[BSM_BlOB_SIZE];  // holds a static setup message

public:
BSM_BLOB1();
~BSM_BLOB1();

//
// Start of support functions/methods for the class 
//

// Reset the class vars to null values
// if doBlobBuffer == true we reset the class blob array
void Init (bool doBlobBuffer);

// Set the Blob (not the vars) to a predetermined 
// 'null' data state where each element is set to
// the "not available" data value when possible
// if the pointer is null, uses the class buffer as the destination
void ResetBlob (unsigned char* dest);

// Pack the current class vars into the passed octet array
// if the pointer is null, uses the class buffer as the destination
// returns = 0 on success, = -1 on fail, 
int Pack(unsigned char* dest);

// UnPack the passed octet array into the class vars
// if the pointer is null, uses the class buffer as the source
// returns = 0 on success, = -1 on fail, 
int UnPack(unsigned char* source);

// Compare the passed class instance with this one, 
// if very element matches, returns true, else false
// note this is an exact match, there is no 'error' used
bool IsEqual(BSM_BLOB1* otherOne);


// Given pointers to two arrays of bytes, copy the full length 
// of the  source to the destination. CAUTION Does no range checking. 
// taken from the Util class
void ByteCopy(unsigned char* dest, unsigned char* source, int cnt);


// Make composite value for the below multi-byte elements 
// using class vars current values to build them up
void Pack_posAccuracy();
void Pack_transAndSpeed();
void Pack_accelerationSet4Way();
void Pack_vehicleSize();

// Take the composite value for the below multi-byte elements
// and use it to update the class vars current values to match
void UnPack_posAccuracy();
void UnPack_transAndSpeed();
void UnPack_accelerationSet4Way();
void UnPack_vehicleSize();


// Below taken from Util class as some do not have ASN values
// Compare the passed two pointers for the indicated number of bytes,
// if they match at all points, return true, otherwise false
bool DoBytesMatch(unsigned char* first, unsigned char* second, int cnt);


//
// Extra credit but useful for testing, vital to some users
// Extra credit but useful for testing, vital to some users
//

// Given a blob, add other data to make a full BMS message
// and return a pointer to that octet stream, return null
// on error, this file can be saved and other tools used on it
// as with other calls, passing in NULL will cause the 
// class buffer to be used as the blob source.
unsigned char* MakeBSM(unsigned char* blobSource);


// Given a byte set for a full BSM message, extract the blob.
// section from it, place it in the class' blob storage.
// if load == true, this call will also set the class vars, 
// by calling UnPack() when done. 
void GetBLOBfromBSM(unsigned char* BSMSource, bool loadIt);



};





#endif	// BLOB1_UTIL (whole file)
// File: BLOB1_util.h   end

