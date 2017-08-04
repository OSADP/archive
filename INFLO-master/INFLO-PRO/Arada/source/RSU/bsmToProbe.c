#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <wave.h>
#include <RoadSideAlert.h>
#include <ProbeVehicleData.h>
#include "tmeclient.h"
#include "configuration.h"

#define BSM1_LENGTH 47
#define MAX_BSM_BUNDLE_SIZE 5

// TODO Structure for Probe messages based on ID

RoadSideAlert_t rsa;
ProbeVehicleData_t probe;

int bsmInBundleCount = 0;
char bsmBundle[TME_MAX_TRANSMIT_SIZE];


void processBSM(WSMData *incomingBSM)
{
	char tmpBuff[100];
	char bsmMessage[400];
	int counter, length;
	uint8_t tmp;

	length = incomingBSM->length;

	if(length > BSM1_LENGTH) {
		strcpy(bsmMessage,"{\"typeid\":\"BSM\"");

		strcat(bsmMessage,",\"time\":\"");
		time_t rawtime;
		time(&rawtime);
		struct tm *utctime = gmtime(&rawtime);
		strftime(tmpBuff, 100, "%FT%Xz", utctime);
		strcat(bsmMessage, tmpBuff);
		strcat(bsmMessage,"\"");

  		strcat(bsmMessage, ",\"payload\":\"");

		for(counter=0;counter<length;counter++) {
			tmp = incomingBSM->contents[counter];
			swap32(tmp);
			sprintf(tmpBuff,"%.2X",tmp);
			strcat(bsmMessage,tmpBuff);
		}

		strcat(bsmMessage,"\"}");
		addToBSMBundle(bsmMessage);
	}

	// TODO Parse ID and see if already tracking
	// TODO Add any new data
	//printf("Processing BSM in bsmToProbe");

	
}

void addToBSMBundle(char *bsm) {
	if(strlen(bsmBundle)==0) {
		strncpy(bsmBundle, "{\"typeid\":\"BSB\"", TME_MAX_TRANSMIT_SIZE);
		strncat(bsmBundle, ",\"payload\":[", TME_MAX_TRANSMIT_SIZE);
		strncat(bsmBundle, bsm, TME_MAX_TRANSMIT_SIZE);
	} else {
		strncat(bsmBundle,",", TME_MAX_TRANSMIT_SIZE);
		strncat(bsmBundle, bsm, TME_MAX_TRANSMIT_SIZE);
	}
	bsmInBundleCount++;
	if(bsmInBundleCount >= MAX_BSM_BUNDLE_SIZE) {
		strncat(bsmBundle,"]}", TME_MAX_TRANSMIT_SIZE);
		//printf("%s\n",bsmBundle);
		tme_client_post_bsm(bsmBundle);
		bsmInBundleCount= 0;
		strncpy(bsmBundle,"", TME_MAX_TRANSMIT_SIZE);
	}
}
// TODO Thread to send messages every TBD seconds

