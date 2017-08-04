// BSM_03.cpp : Defines the entry point for the console application.
//
// The example adds some Part II content to the BSM messages to show how this is done
// it follow the same overall flow of prior examples

#include "stdafx.h" 
#include "stdio.h" 
#include <time.h>
#include <iostream>  
using namespace std;
#include "DSRC_ASN_Utils.h"

// Windows: int _tmain(int argc, _TCHAR* argv[])
// Unbuntu: int   main(int argc,   char* argv[])
int main(int argc, char* argv[])
{
	// Get starting time and output a stock 'hello world' line
	time_t rawtime;
	struct tm * timeinfo;
	char timeString [80]; 
	time (&rawtime); // ask system for time count
	timeinfo = localtime ( &rawtime ); // convert to units
	strftime (timeString,40,"%I:%M:%S %p.",timeinfo); // to string
	char text[200];
	sprintf(text, "BSM_03   SCSC DSRC toolkit example started at: %s \n", timeString); 
	cout << text << endl; 

	// Now do something useful with the toolkit
	//
	// Example:  BSM 03
	// The example adds some Part II content to the BSM message to show how this is done
	// it follow the same overall flow of prior examples	
	//

	// Some basic variables we will need
	BYTE buf[MAX_MSG_SIZE]; // used to hold the 'fake' blob
	size_t theSize; // returned message sizes in the buffers
	size_t resultCnt; // returned file read/write size 
	char filePath[] = "C:/BSM_03.asn";
	unsigned char* newMsgBuf = NULL;
	for (int i=0; i<MAX_MSG_SIZE; i++) buf[i] = 0;
	BasicSafetyMessage_t theBSM;	// create empty message struc	
	BasicSafetyMessage_Init(&theBSM);	
	BasicSafetyMessage_t*  pNewBSM = NULL;

	// Create test data for our use, same as BSM_01 made
	if (theBSM.blob1.buf != NULL)
		for (int i=0; i<BSMblob_BuffSize; i++) 
			theBSM.blob1.buf[i] = (BSMblob_BuffSize - i)* 3;

	//
	// BSM_03 part starts here
	// Ignoring for the moment the blob contents (see BSM_04 and BSM_05 for this)
	// we now add some Part content to show the process steps
	//

	// We must first create the actual optional structure
	theBSM.safetyExt = new VehicleSafetyExtension;
	VehicleSafetyExtension_Init(theBSM.safetyExt);
	//DEBUG asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, &theBSM); // display it

	// we now add some 'event' content to the 'safetyExt'
	theBSM.safetyExt->events = new EventFlags_t;
	*theBSM.safetyExt->events = 22; // just a ptr to a 'long' so no further init is needed
	//DEBUG asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, &theBSM); // display it


	// we now add some RTCM message type content to  the 'safetyExt'
	theBSM.safetyExt->theRTCM = new RTCMPackage;
	RTCMPackage_Init(theBSM.safetyExt->theRTCM);
	//this individual message requires us to allocate memory by hand

	theBSM.safetyExt->theRTCM->msg1004 = new OCTET_STRING_t;  //RTCMPackage::msg1004
	// build a fake RTCM 1004 rev 3 message to append, 
	// always put these on heap, so the destroy and teardown will work
	int RTCMsize = 24;
	theBSM.safetyExt->theRTCM->msg1004->buf = (uint8_t *) malloc(RTCMsize);
	assert(theBSM.safetyExt->theRTCM->msg1004->buf);
	theBSM.safetyExt->theRTCM->msg1004->size = RTCMsize;
	theBSM.safetyExt->theRTCM->msg1004->_asn_ctx.ptr = NULL;
	// stuff a few bytes into this 3rd party defined blob
	for(int i=0; i<RTCMsize; i++) theBSM.safetyExt->theRTCM->msg1004->buf[i] = 0xAA;

	// Show what we have created to console... 
	asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, &theBSM); // display it

	// Once all these items have been added, the message creation process is as below...
	theSize = DSRC_serializer(&asn_DEF_BasicSafetyMessage,
							  &theBSM, buf); // convert into DER bytes
	if (theSize > 0)
	{
		cout << "BSM message content created and encoded." << endl;
		resultCnt = WriteBytesToFile(filePath, buf, (int)theSize); // Save to a file in a binary format
		assert(theSize == resultCnt);
	}
	cout << "File written to: " << filePath << endl;

	// Now read it back into another memory array and return a pointer to it.
	// see BSM_032 for details on this section
	// the decoding process is the same regardless of the message content that is present
	resultCnt = -1;
	newMsgBuf = FileInToMemoryBytes(filePath, &resultCnt, true);
	if (resultCnt > 1)
	{	
		ssize_t size;  
		void  *msgPtr = NULL; // set a generic pointer to empty/null so code will allocate it
		size = DSRC_UNserializer((void **)&msgPtr, newMsgBuf, (int)resultCnt, true); // decode into a struc
		if (size > 0) // asn_dec_rval_code_e::RC_OK is inside above call
		{
			pNewBSM = (BasicSafetyMessage_t *)msgPtr; // cast result to your real pointer
			asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, msgPtr); // display it
		}
	}
	
	// Dispose of any heap allocated items using a stock macro
	// see also the alternative form: ASN_STRUCT_FREE() used in above
	// note that the additional content is torn down with the same single call
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessage, pNewBSM); 
	ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_BasicSafetyMessage, &theBSM);
	if (newMsgBuf != NULL) delete newMsgBuf;
	cout << endl << "Done." << endl;
	return 0;
}

