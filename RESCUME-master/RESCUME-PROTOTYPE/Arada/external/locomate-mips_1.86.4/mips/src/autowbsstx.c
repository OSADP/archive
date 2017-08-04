/*
* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.
* Proprietary and Confidential Material.
*
*/

#include <stdio.h>
#include <ctype.h>
#include <termio.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/ioctl.h>
#include <time.h>
#include <signal.h>
#include "wave.h"


static WMEApplicationRequest entry;
static WMETARequest tareq;
static WSMRequest wsmreq;


int buildPSTEntry();
int buildWSMRequestPacket();
int buildWMETARequest();
int txWSMPPkts(int);
void sig_int(void);
void sig_term(void);


static uint64_t packets;
static uint64_t drops = 0;
static int pid;
static char Data[30]="LOCOMATE-ARADA SYSTEMS";
static uint16_t len=500;


struct ta_argument {
    uint8_t  channel;
    uint8_t channelinterval; 
} taarg; 

int main (int argc, char *argv[]){
	int result ;
	pid = getpid();
        
        if (argc < 6) {
            printf("usage: localtx [sch channel access <1 - alternating> <0 - continous>] [TA channel ] [ TA channel interval <1- cch int> <2- sch int>] [SCH Channel] [Priority]\n");
            return 0; 
        } 
        taarg.channel = atoi(argv[2]); 
        taarg.channelinterval = atoi(argv[3]);
 
	printf("Filling Provider Service Table entry %d\n",buildPSTEntry(argv));
	printf("Building a WSM Request Packet %d\n", buildWSMRequestPacket());
        printf("Builing TA request %d\n", buildWMETARequest());
	
	if ( invokeWAVEDriver(0) < 0 ){
		printf( "Opening Failed.\n ");
		exit(-1);
	} else {
		printf("Driver invoked\n");

	}

	printf("Registering provider\n ");
	if ( registerProvider( pid, &entry ) < 0 ){
		printf("\nRegister Provider failed\n");
		removeProvider(pid, &entry);
		registerProvider(pid, &entry);
	} else {
		printf("provider registered with PSID = %u\n",entry.psid );
	}
        printf("starting TA\n");
        if (transmitTA(&tareq) < 0)  {
            printf("send TA failed\n "); 
        } else {
            printf("send TA successful\n") ;
        }
	result =txWSMPPkts(pid);
	if ( result == 0 )
		printf("All Packets transmitted\n");
	else 
		printf("%d Packets dropped\n",result);

	return 1;

}
	

	
int buildPSTEntry(char **argv){
	
	entry.psid = 5;
	entry.priority = atoi(argv[5]);
	entry.channel = atoi(argv[4]);
	entry.repeatrate = 50; // repeatrate =50 per 5seconds = 1Hz
        if (atoi(argv[1]) > 1) {
            printf("channel access set default to alternating access\n"); 
            entry.channelaccess = CHACCESS_ALTERNATIVE; 
        } else {
            entry.channelaccess = atoi(argv[1]); 
        }
	
	return 1;
}

	
int buildWSMRequestPacket(){
	wsmreq.chaninfo.channel = entry.channel;
	wsmreq.chaninfo.rate = 3;
	wsmreq.chaninfo.txpower = 15;
	wsmreq.version = 1;
	wsmreq.security = 1;
	wsmreq.psid = 5;
	wsmreq.txpriority = 1;
	memset ( &wsmreq.data, 0, sizeof( WSMData));
	memcpy ( &wsmreq.data.contents, &Data, sizeof( Data));
	memcpy ( &wsmreq.data.length, &len, sizeof( len));
	return 1;

}

int buildWMETARequest()
{
    tareq.action = TA_ADD;
    tareq.repeatrate = 100;
    tareq.channel = taarg.channel;
    tareq.channelinterval = taarg.channelinterval;
    tareq.servicepriority = 1;
	return 0;
}

int txWSMPPkts(int pid){
	int ret = 0 , count = 0;
	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);

	while(1) {	
                usleep(2000);
		ret = txWSMPacket(pid, &wsmreq);
		if( ret < 0) { 
			drops++;
		}
		else {
			packets++;
			count++;
		}
		printf("Transmitted #%llu#					Dropped #%llu# len #%u#\n", packets, drops,wsmreq.data.length);
	}
	printf("\n Transmitted =  %d dropped = %llu\n",count,drops); 
	return drops; 
}

void sig_int(void)
{

	removeProvider(pid, &entry);
	signal(SIGINT,SIG_DFL);
	printf("\n\nPackets Sent =  %llu\n",packets); 
	printf("Packets Dropped = %llu\n",drops); 
	printf("localtx killed by control-C\n");
	exit(0);

}

void sig_term(void)
{

	removeProvider(pid, &entry);
	signal(SIGINT,SIG_DFL);
	printf("\n\nPackets Sent =  %llu\n",packets); 
	printf("\nPackets Dropped = %llu\n",drops); 
	printf("localtx killed by control-C\n");
	exit(0);
}

	
