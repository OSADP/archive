// BSM_05.cpp : Defines the entry point for the console application.
//
// This example builds a 'simple' BSM (no Part I content) but
// uses only a simple single class that deal with all the BLOB
// object using class members and *DOES NOT* use the rest of the 
// ASN library at all.  One can write very small BSM readers and
// and writer code with this class.  While the class is
// called "BLOB1_util" (the actual class is called BSM_BLOB1) 
// it will in fact create a full BSM message which you can then 
// pass to/from other layers (UDP, TCP/IP, 1609 etc.)
// 
// Unlike most of the ASN.1 toolkit which is for C or C++ use
// the BSM_BLOB1 implementation is a real C++ class and 
// you must use C++ with it.  The units in that class reflect
// the native message set units, so any translation is the callers
// responsibility to deal with, i.e. a float of 123.456 degrees
// becomes 1234560000 native units.  
//
//

#include "stdafx.h" 
#include "stdio.h" 
#include <time.h>
#include <iostream>  
using namespace std;

#include "BLOB1_util.h"	 // does not use any other ASN includes
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
	sprintf(text, "BSM_05   SCSC DSRC toolkit example started at: %s \n", timeString); 
	cout << text << endl; 
	size_t resultCnt; // returned file read/write size 
	char filePath[] = "C:/BSM_05.asn";
	unsigned char* newMsgBuf = NULL;
	BasicSafetyMessage_t*  pNewBSM = NULL;


	// Now do something useful with the toolkit
	//
	// Example:  BSM 05
	// This example builds a 'simple' BSM (no Part I content) but
	// uses only a simple single class that deal with all the BLOB
	// object using class members and *DOES NOT* use the rest of the 
	// ASN library at all.   
	//

	// Create an instance of the basic class
	BSM_BLOB1 aBLOB; // note that it does it's own init when created

	aBLOB.msgCount = (aBLOB.msgCount + 1 )/128;  // inc the msg count each time
	aBLOB.tempID = 0xAA;		// Temp Id to be: 00.00.00.AA
	aBLOB.dSecond = 30001;		// as in 30.001 seconds in the current minute, 0x7531
	aBLOB.lon	= -1000050001;		// as in -100.0050001 degs
	aBLOB.lat	=   359900001;	// as in +35.9900001 degs
	aBLOB.elev	= 0x015E; ;	// as in +350.0 meters above, so 0x015E
	aBLOB.heading = 0; // North boundish direction
	aBLOB.transState = 0x40; // no class to call with TransmissionState_forwardGears
	aBLOB.speed = 400; // Car is in drive and moving 8 m/s

	aBLOB.Pack(NULL); // Create the blob with the above class values
	
	// You could access this blob (to place it into a BSM) with a call like
	// aBLOB.ByteCopy(pNewBSM->blob1.buf,aBLOB.theBlob, BSM_BlOB_SIZE)

	// The file writing from the blob class is a bit different
	// Allow the class to make a complete message payload with its own blob value
	unsigned char* theBSMdata; // create a pointer
	theBSMdata = aBLOB.MakeBSM(NULL);
	resultCnt = WriteBytesToFile(filePath, theBSMdata, BSM_MSG_SIZE); // Save to a file in a binary format
	assert(BSM_MSG_SIZE == resultCnt);

	//
	// end new content
	//

	cout << "BSM message content created and encoded." << endl;
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
			asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, msgPtr); // display it as a BSM
		}
	}
	
	// Dispose of any heap allocated items using a stock macro
	// see also the alternative form: ASN_STRUCT_FREE() used in above
	// note that the additional content is torn down with the same single call
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessage, pNewBSM); 
	if (newMsgBuf != NULL) delete newMsgBuf;
	cout << endl << "Done." << endl;
	return 0;
}

