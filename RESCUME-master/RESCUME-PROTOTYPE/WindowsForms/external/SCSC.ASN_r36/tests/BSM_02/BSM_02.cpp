// BSM_02.cpp : Defines the entry point for the console application.
//
// A demo showing how to decode any message type correctly with basic test logic
// 
#include "stdafx.h" 
#include "stdio.h" 
#include <time.h>
#include <iostream>  
using namespace std;
#include "DSRC_ASN_Utils.h"
#include "BasicSafetyMessage.h"  // Add a path to: ../../DSRC_TK/ASN_r36/* for these files


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
	sprintf(text, "BSM_02   SCSC DSRC toolkit example started at: %s \n", timeString); 
	cout << text << endl; 

	// Now do something useful with the toolkit
	//
	// Example:  BSM 02
	// Show use of better message decoding logic
	// items comments on in BSM_01 are not mentioned here for readability
	//

	// Some basic vars we will need
	BYTE buf[MAX_MSG_SIZE]; // used to hold the 'fake' blob
	size_t theSize; // returned message sizes in the buffers
	size_t resultCnt; // returned file read/write size 
	char filePath[] = "C:/BSM_02.asn";
	unsigned char* newMsgBuf = NULL;
	for (int i=0; i<MAX_MSG_SIZE; i++) buf[i] = 0;
	BasicSafetyMessage_t theBSM;	// create empty message struc	
	BasicSafetyMessage_Init(&theBSM);	
	BasicSafetyMessage_t*  pNewBSM = NULL;

	// Create test data for our use, same as BSM_01 made
	if (theBSM.blob1.buf != NULL)
		for (int i=0; i<BSMblob_BuffSize; i++) 
			theBSM.blob1.buf[i] = BSMblob_BuffSize - i;
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
	resultCnt = -1;
	newMsgBuf = FileInToMemoryBytes(filePath, &resultCnt, true);
	if (resultCnt > 1)
	{	
		cout << "File was read into memory." << endl;

		//
		// Begin BSM_02 Unique content here 
		//
		// Look over the source code found in  DSRC_UNserializer() in the file DSRC_ASN_Utils.h
		//
		// If we now what sort of message the expect we can decode to it.
		// At other times we must test the message to determine this
		// this routine bundles these steps into a simple single call
		// If you are using SAE/IEEE PSIDS you may also want to confirm the message
		// is in fact part of that right application ID class as well. 

		ssize_t size;  
		void  *msgPtr = NULL; // set a generic pointer to empty/null so code will allocate it
		size = DSRC_UNserializer((void **)&msgPtr, newMsgBuf, (int)resultCnt, true); // decode into a struc
		if (size > 0) // asn_dec_rval_code_e::RC_OK is inside above call
		{
			pNewBSM = (BasicSafetyMessage_t *)msgPtr; // cast result to your real pointer
			asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, msgPtr); // display it
		}
		// As rule, your decoding logic should follow this sort of flow.
		// place such a thread close to the lower layer event handler
		
	}
	
	// Dispose of any heap allocated items using a stock macro
	// see also the alternative form: ASN_STRUCT_FREE() used in above
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessage, pNewBSM); 
	ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_BasicSafetyMessage, &theBSM);
	if (newMsgBuf != NULL) delete newMsgBuf;
	cout << endl << "Done." << endl;
	return 0;
}

