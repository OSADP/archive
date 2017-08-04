/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

/*3se create a user application to xmit wsmp packets in the following format:
- valid rate
- valid channel
- valid txpower
- acid
- acm


rate = 3, 4.5, 6, 9, 12, 18, 24, 27
power= 0, 2, 4, to 61 in increments of 2

xmit 5 packets in the above rate, power.

configure a sniffer

- capture the wsmp packets
- it should capture a total of 5 x 8 x 30 packets
  where 5 is number of packets
        8 is total number of rates
        30 is number of distinct power values
*/
#include <stdio.h>
#include "../wme/wave.h"

static	PSTEntry entry;//  This is the provider service table
static	WSMPacket pkt;// this is the WSMP packet

int main(int argc, char *argv[])
{

	int aniRate[8]={3, 4.5, 6, 9, 12, 18, 24, 27};
	int aniAcid,pwrvalues,pktcount,ratecount,aniChan,ret,drops=0,pid=1;
	char *aniAcm; 
	char *aniData;// max 512
	aniAcm = entry.acm.contents;
	printf(" \n ACID=");
	scanf("%d ",&aniAcid);
	printf(" \n ACM");
	scanf("%s",aniAcm);
//	printf("\n Enter the data u want to transmit max 512");
//	scanf("%s",aniData);
	printf("\n Enter the channel");
	scanf("%d",aniChan);

	/*Filling the PST */
	entry.acid = aniAcid;
	entry.acm.length = strlen(aniAcm);
//	strncpy(entry.acm.contents,aniAcm,OCTET_MAX_LENGTH);

	// coping the data
	memset(&pkt.data, 0, sizeof(WSMData));
//	memcpy(WSMData, aniData, sizeof(WSMData));

	/* Build Packet*/

	pkt.version = 7;
	pkt.security = 8;
	pkt.channel = 178;
//	pkt.rate = aniRate;
//	pkt.txpower = aniPower;
	pkt.app_class = aniAcid;
	pkt.acm.length = strlen(aniAcm);
	strncpy(pkt.acm.contents,aniData,512);
	
	entry.contents |= WAVE_ACID;  
	entry.contents |= WAVE_ACM;
	
	invokeWAVEDriver(0); // invoking the wave driver
	registerProvider(pid, &entry); 
	
	for (pwrvalues=0;pwrvalues<61;pwrvalues+=2){
		pkt.txpower = pwrvalues;
		for (ratecount=0;ratecount<8;ratecount++){
			pkt.rate = aniRate[ratecount];
			for (pktcount=0;pktcount<5;pktcount++){
				ret = 0;
				ret = txWSMPacket(pid, &pkt);
				if( ret < 0) 
					drops++;
			}
		}
	}
		printf("\n the number of drops = %d",drops);
	return 0;	
}
