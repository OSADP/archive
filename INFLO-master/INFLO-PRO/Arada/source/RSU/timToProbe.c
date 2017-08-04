#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <wave.h>
#include <RoadSideAlert.h>
#include <ProbeVehicleData.h>
#include "tmeclient.h"
#include "configuration.h"

#define MAX_TIM_BUNDLE_SIZE 5
#define TIM_MSG_SIZE 5000

// TODO Structure for Probe messages based on ID

RoadSideAlert_t rsa;
ProbeVehicleData_t probe;

int timInBundleCount = 0;
char timBundle[TME_MAX_TRANSMIT_SIZE];


void processTIM(WSMData *incomingTIM)
{
	char tmpBuff[100];
	char timMessage[TIM_MSG_SIZE];
	int counter, length;
	uint8_t tmp;

	length = incomingTIM->length;

	strcpy(timMessage,"{\"typeid\":\"TIM\"");
	
	strcat(timMessage, ",\"payload\":\"");
	for(counter=0;counter<length;counter++) {
		tmp = incomingTIM->contents[counter];
		swap32(tmp);
		sprintf(tmpBuff,"%.2X",tmp);
		strcat(timMessage,tmpBuff);
	}

	strcat(timMessage,"\"}");
	//addToBSMBundle(timMessage);
}

void addToTIMBundle(char *tim) {
	if(strlen(timBundle)==0) {
		strncpy(timBundle, "{\"typeid\":\"TSB\"", TME_MAX_TRANSMIT_SIZE);
		strncat(timBundle, ",\"payload\":[", TME_MAX_TRANSMIT_SIZE);
		strncat(timBundle, tim, TME_MAX_TRANSMIT_SIZE);
	} else {
		strncat(timBundle,",", TME_MAX_TRANSMIT_SIZE);
		strncat(timBundle, tim, TME_MAX_TRANSMIT_SIZE);
	}
	timInBundleCount++;
	if(timInBundleCount >= MAX_TIM_BUNDLE_SIZE) {
		strncat(timBundle,"]}", TME_MAX_TRANSMIT_SIZE);
		printf("%s",timBundle);
		//tme_client_post_tim(timBundle); //Need URL
		timInBundleCount= 0;
		strncpy(timBundle,"", TME_MAX_TRANSMIT_SIZE);
	}
}
// TODO Thread to send messages every TBD seconds
