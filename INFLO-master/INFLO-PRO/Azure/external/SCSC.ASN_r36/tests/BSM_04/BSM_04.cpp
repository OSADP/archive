// BSM_04.cpp : Defines the entry point for the console application.
//
// This example builds a "verbose" BSM message where not blob is found,
// all the blob elements are present, but each is a tagged item
// this further show use of the toolkit and also an alternative 
// in-memory form of the blob structure which can be of use
//
// A useful translation class also exists for this need, 
// shown in  the next example BSM_05
//

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
	sprintf(text, "BSM_04   SCSC DSRC toolkit example started at: %s \n", timeString); 
	cout << text << endl; 
	// Some basic variables we will need
	BYTE buf[MAX_MSG_SIZE]; // used to hold the 'fake' blob
	size_t theSize; // returned message sizes in the buffers
	size_t resultCnt; // returned file read/write size 
	char filePath[] = "C:/BSM_04.asn";
	unsigned char* newMsgBuf = NULL;
	for (int i=0; i<MAX_MSG_SIZE; i++) buf[i] = 0;


	// Now do something useful with the toolkit
	//
	// Example:  BSM 04
	// This example builds a "verbose" BSM message where not blob is found,
	// all the blob elements are present, but each is a tagged item
	// this further show use of the toolkit and also an alternative 
	// in memory from of the blob structure which can be of use
	//

	// this time we create the 'alt' BSM form using the same pattern
	BasicSafetyMessageVerbose_t theVBSM; // create empty message struc	
	BasicSafetyMessageVerbose_Init(&theVBSM); 	
	BasicSafetyMessageVerbose_t*  pNewVBSM = NULL;

	// Create test data for our use,
	// rather then create a blob off line, and stuff it
	// we must set each value in turn

	theVBSM.msgCnt = (theVBSM.msgCnt + 1 )/128;  // inc the msg count each time
	theVBSM.id.buf[3] = 0xAA;		// Temp Id to be: 00.00.00.AA
	theVBSM.secMark = 30001;		// as in 30.001 seconds in the current minute, 0x7531
	theVBSM.Long = -1000050001;		// as in -100.0050001 degs
	theVBSM.lat  =   359900001;	// as in +35.9900001 degs
	theVBSM.elev.buf[0] = 0x01; ;	// as in +350.0 meters above, so 0x015E
	theVBSM.elev.buf[1] = 0x5E; ; 
	// A place near the Tx-Ok border line on the N-S County Road 30 near Canadian, TX 
	theVBSM.heading = 0; // North boundish direction
	TransmissionAndSpeed_Set(&theVBSM.speed, 
							 TransmissionState_forwardGears, 
							 400); // Car is in drive and moving 8 m/s

	// Note that any Part II content could also be added anywhere in this process
	// (see the BSM)3 example for this)

	// Once all these items have been set, the message creation process is as before...
	asn_fprint(NULL, &asn_DEF_BasicSafetyMessageVerbose, &theVBSM); // display it
	theSize = DSRC_serializer(&asn_DEF_BasicSafetyMessageVerbose, 
							  &theVBSM, buf); // convert into DER bytes
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
			pNewVBSM = (BasicSafetyMessageVerbose_t *)msgPtr; // cast result to your real pointer
			asn_fprint(NULL, &asn_DEF_BasicSafetyMessageVerbose, msgPtr); // display it as a V-BSM
		}
	}
	
	// Dispose of any heap allocated items using a stock macro
	// see also the alternative form: ASN_STRUCT_FREE() used in above
	// note that the additional content is torn down with the same single call
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessageVerbose, pNewVBSM); 
	ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_BasicSafetyMessageVerbose, &theVBSM);
	if (newMsgBuf != NULL) delete newMsgBuf;
	cout << endl << "Done." << endl;
	return 0;
}

