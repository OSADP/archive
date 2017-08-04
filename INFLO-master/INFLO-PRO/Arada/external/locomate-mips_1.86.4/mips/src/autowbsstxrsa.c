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

#include <asn_application.h>
#include <asn_internal.h>
#include <RoadSideAlert.h>
#include <crc.h>

#define HIBYTE(t) (t >> 8) & 0xFF
#define LOBYTE(t) t & 0xFF


typedef struct RoadSideAlert__description description_t;
int m_itisItems = 9;
ITIScodes_t itiscodes[9] = {513, 514, 517, 519, 520, 521, 522, 524, 525};



//static PSTEntry entry;
static WMEApplicationRequest wreq;
static WMEApplicationRequest entry;
static WMETARequest tareq;
static WSMRequest wsmreq;
//static WMECancelTxRequest cancelReq;
static int pid;


void 	receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void 	receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication);
void 	receiveTsfTimerIndication(TSFTimer *timer);
//int	 confirmBeforeJoin(u_int8_t acid, ACM acm);  This is for user only


int buildPSTEntry();
int buildWSMRequestPacket();
int buildWMEApplicationRequest();
int buildWMETARequest();
int txWSMPPkts(int);
void sig_int(void);
void sig_term(void);
static uint64_t packets;
static uint64_t drops = 0;

struct ta_argument {
    uint8_t  channel;
    uint8_t channelinterval; 
} taarg; 

int main (int argc, char *argv[]){
	int result ;
	pid = getpid();
        
        if (argc < 4) {
            printf("usage: cmd [sch channel access <1 - alternating> <0 - continous>] [TA channel ] [ TA channel interval <1- cch int> <2- sch int>] \n");
            return 0; 
        } 
        taarg.channel = atoi(argv[2]); 
        taarg.channelinterval = atoi(argv[3]); 
	printf("Filling Provider Service Table entry %d\n",buildPSTEntry(argv));
	printf("Building a WSM Request Packet %d\n", buildWSMRequestPacket());
	printf("Building a WME Application  Request %d\n",buildWMEApplicationRequest());
        printf("Builing TA request %d\n", buildWMETARequest());
	
	if ( invokeWAVEDriver(0) < 0 ){
		printf( "Opening Failed.\n ");
		exit(-1);
	} else {
		printf("Driver invoked\n");

	}

	registerWMENotifIndication(receiveWME_NotifIndication);
	registerWRSSIndication(receiveWRSS_Indication);
	registertsfIndication(receiveTsfTimerIndication);

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
	/*if ( startWBSS ( pid, &wreq) < 0) {
		printf("\n WBSS start failed  " );
		exit (-1);
	} else {
		printf("\nWBSS started");
	}*/
	
	result =txWSMPPkts(pid);
	if ( result == 0 )
		printf("All Packets transmitted\n");
	else 
		printf("%d Packets dropped\n",result);

	return 1;



}
	

	
int buildPSTEntry(char **argv){
	
	entry.psid = 10;
	entry.priority = 1;
	entry.channel = 172;
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
	int i;//, j;
	asn_enc_rval_t rvalenc;
	RoadSideAlert_t *rsa;
	ITIScodes_t *code;
	crc m_crc;
	uint8_t *buf;
	

	wsmreq.chaninfo.channel = 172;
	wsmreq.chaninfo.rate = 3;
	wsmreq.chaninfo.txpower = 15;
	wsmreq.version = 1;
	wsmreq.security = 1;
	wsmreq.psid = 10;
	wsmreq.txpriority = 1;
	memset ( &wsmreq.data, 0, sizeof( WSMData));
	
  rsa = (RoadSideAlert_t *) calloc(1,sizeof(*rsa));
  rsa->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  rsa->msgID.size = sizeof(uint8_t);
  rsa->msgID.buf[0] = DSRCmsgID_roadSideAlert;
  rsa->msgCnt = 0;
  rsa->typeEvent = itiscodes[0];
  rsa->description = (description_t *)calloc(1, sizeof(description_t)); //create
  for (i=1; i<m_itisItems; i++) {
    code = (ITIScodes_t* )calloc(1, sizeof(ITIScodes_t));
    *code = itiscodes[i];
    ASN_SEQUENCE_ADD(&rsa->description->list, code);
  }

  rsa->priority = (Priority_t *) calloc(1, sizeof(Priority_t));
  rsa->priority->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  rsa->priority->size = sizeof(uint8_t);
  rsa->priority->buf[0] = 1;
  rsa->heading = (HeadingSlice_t *) calloc(1, sizeof(HeadingSlice_t));
  rsa->heading->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
  rsa->heading->size = 2 * sizeof(uint8_t);
  rsa->heading->buf[0] = 1;
  rsa->heading->buf[1] = 2;
  rsa->extent = (Extent_t *) calloc(1, sizeof(Extent_t));
  rsa->extent->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  rsa->extent->size = sizeof(uint8_t);
  rsa->extent->buf[0] = Extent_useFor5000meters;
  rsa->positon = (FullPositionVector_t *) calloc(1, sizeof(FullPositionVector_t));
  rsa->positon->Long = 0;
  rsa->positon->lat = 0;
  rsa->crc.buf = (uint8_t *) calloc(2, sizeof(uint8_t));
  rsa->crc.size = 2*sizeof(uint8_t);

	buf = (uint8_t *)&wsmreq.data.contents;
	rvalenc = der_encode_to_buffer(&asn_DEF_RoadSideAlert, rsa, buf, 1000);
  if (rvalenc.encoded == -1) {
    fprintf(stderr, "Cannot encode %s: %s\n",
                  rvalenc.failed_type->name, strerror(errno));
  } else  {
    printf("Structure successfully encoded %d\n", rvalenc.encoded);
		crcInit();
		m_crc = crcFast(buf, rvalenc.encoded - 2);
		buf[rvalenc.encoded - 1] = LOBYTE(m_crc);
		buf[rvalenc.encoded - 2] = HIBYTE(m_crc);
		m_crc = crcFast(buf, rvalenc.encoded);
		if (m_crc == 0) {
			printf("CRC check successful\n");
		} else {
			fprintf(stderr, "CRC check unsuccessful\n");
		}
    wsmreq.data.length = rvalenc.encoded;
    asn_DEF_RoadSideAlert.free_struct (&asn_DEF_RoadSideAlert, rsa, 0);
  }
	return 1;
}

int  buildWMEApplicationRequest(){	
	wreq.psid = 10 ;
	printf(" WME App Req %d \n",wreq.psid);
	//strncpy(wreq.acm.contents, entry.acm.contents, OCTET_MAX_LENGTH);
	//printf(" WME App Req %s \n",wreq.acm.contents);
	//wreq.acm.length = entry.acm.length;
	wreq.repeats = 1;

	wreq.persistence = 1;
	wreq.channel = 172;
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
	//int pwrvalues, ratecount, txprio, ret = 0, pktcount, count = 0;
	int  ret = 0, count = 0;

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
		printf("Transmitted #%llu#					Dropped #%llu#\n", packets, drops);
	}
	printf("\n Transmitted =  %d dropped = %llu\n",count,drops); 
	//cancelReq.aci = 0;
	//cancelReq.channel = 172;
	//cancelTX ( pid, &cancelReq); 
	return drops; 
}

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication)
{
}

void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication)
{

	printf("WRSS recv channel %d",(u_int8_t)wrssindication->wrssreport.channel);
	printf("WRSS recv reportt %d",(u_int8_t)wrssindication->wrssreport.wrss);

}


void receiveTsfTimerIndication(TSFTimer *timer)
{
	printf("TSF Timer: Result=%d, Timer=%llu",(u_int8_t)timer->result,(u_int64_t)timer->timer);
}

int confirmBeforeJoin(u_int8_t psid)
{
	printf("Link Confirmed PSID=%d\n",(u_int8_t)psid);
	return 0;
}

void sig_int(void)
{
	int ret;

	ret = stopWBSS(pid, &wreq);
	removeProvider(pid, &entry);
	signal(SIGINT,SIG_DFL);
	printf("\n\nPackets Sent =  %llu\n",packets); 
	printf("Packets Dropped = %llu\n",drops); 
	printf("localtx killed by control-C\n");
	exit(0);

}

void sig_term(void)
{
	int ret;

	ret = stopWBSS(pid, &wreq);
	removeProvider(pid, &entry);
	signal(SIGINT,SIG_DFL);
	printf("\n\nPackets Sent =  %llu\n",packets); 
	printf("\nPackets Dropped = %llu\n",drops); 
	printf("localtx killed by control-C\n");
	exit(0);
}

	
