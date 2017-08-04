// BSM_01.cpp : Defines the entry point for the console application.
//
// A basic BSM encode and decode example, a first hello-world test case
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
	sprintf(text, "BSM_01   SCSC DSRC toolkit example started at: %s \n", timeString); 
	cout << text << endl; 

	// Now do something useful with the toolkit
	//
	// Example:  BSM 01
	// Create a very simple BSM message (with nonsense blob contents)
	//

	// Some basic vars we will need
	BYTE buf[MAX_MSG_SIZE]; // used to hold the 'fake' blob
	size_t theSize; // returned message sizes in the buffers
	size_t resultCnt; // returned file read/write size 
	char filePath[] = "C:/BSM_01.asn";
	unsigned char* newMsgBuf = NULL;
	for (int i=0; i<MAX_MSG_SIZE; i++) buf[i] = 0;
	// we zero the buffer simply to aid the developer in looking at the debug listings
		
	BasicSafetyMessage_t theBSM;		// create a message structure in memory
	BasicSafetyMessage_Init(&theBSM);	// fill both msg structures with default values
	 									// as this example has no Part II we ignore it
										// note that this call deals with the pointer
										// and required buffers and set the msg ID for us
	BasicSafetyMessage_t*  pNewBSM = NULL; // create ptr to a message structure to use


	// Create data and Assign a simple 'fake' blob of counting numbers
	if (theBSM.blob1.buf != NULL)
		for (int i=0; i<BSMblob_BuffSize; i++) 
			theBSM.blob1.buf[i] = i;

	theBSM.blob1.size = BSMblob_BuffSize;  
		// Extra credit, in this case this value never changes, 
		// but in most other use cases it will vary. 
		// Look at the above function call BasicSafetyMessage_Init() 
		// to see how to allocate such buffers (in fact it is in BSMblob_Init() )

	// This simple message, while nonsense to J2735 rules, is still valid as an ASN message
	// so we display it then encode it for transmission or other uses.  
	theSize = DSRC_serializer(&asn_DEF_BasicSafetyMessage,
							  &theBSM, buf); // convert into DER bytes
	if (theSize > 0)
	{
		cout << "BSM message content created and encoded." << endl;
		asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, &theBSM); // display it
		resultCnt = WriteBytesToFile(filePath, buf, (int)theSize); // Save to a file in a binary format
		assert(theSize == resultCnt);
	}
	cout << "File written to: " << filePath << endl;
	// Congrats, you have built a BSM message

	// Now read it back into another memory array and return a pointer to it.
	resultCnt = -1;
	newMsgBuf = FileInToMemoryBytes(filePath, &resultCnt, true);
	if (resultCnt > 1)
	{	// Do these two match? 
		// One could do a simple brute force byte by byte compare of the two messages
		// a utility call called DoBytesMatch() can be used for this. 
		// or better yet use a call to the DoCRC() and compare two hash values
		// either of these can be faster than a full decode. Decoded messages need to be
		// compared element by element and this can consume more processing time
		// but you may also learn WHY they differ, if this is a need.
		cout << "File was read into memory." << endl;

		// Now we decode the array (presume it is a valid message to to show how)
		// decode what we have in the buffer as if it is a BSM message
		asn_dec_rval_t rval; // declare decoder return value 
		void  *basePtr = NULL; // set pointer to empty/null so code will allocate it
		rval = ber_decode( 0, &asn_DEF_BasicSafetyMessage,
			(void **)&basePtr, newMsgBuf, resultCnt); // decode into a struc

		if (rval.code == RC_OK) // asn_dec_rval_code_e::RC_OK
		{
			cout << "ASN decode worked, the structure is now loaded with the decoded data." << endl;
			pNewBSM = (BasicSafetyMessage_t *)basePtr; // cast result
			asn_fprint(NULL, &asn_DEF_BasicSafetyMessage, basePtr); // display it

		}

		// In general, presuming that a message is 'valid' is very risky, 
		// in the next example (BSM_02) a more robust decoding is demonstrated
		// recall that a well-formed valid ASN message may still be an ill-formed
		// SAE J2735 message so additional testing is needed at the app level as well

		// The App builders should look over the raw ASN.1 calls of 
		// ber_decode() and der_encode_to_buffer() to understand them
		// in time you will want to write such handlers yourself.
		// a stub of this has been provided for general decoding as an example
		// see DSRC_UNserializer() in the file DSRC_ASN_Utils.h
		
	}
	
	// Dispose of any heap allocated items using a stock macro
	// see also the alternative form: ASN_STRUCT_FREE() used in above
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessage, pNewBSM); 
	ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_BasicSafetyMessage, &theBSM);
	if (newMsgBuf != NULL) delete newMsgBuf;
	cout << endl << "Done." << endl;
	return 0;
}

