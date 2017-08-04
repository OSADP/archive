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
#include <ProbeVehicleData.h>


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
	//int j;
	asn_enc_rval_t rvalenc;
	ProbeVehicleData_t *pvd;
  DDateTime_t  *utcTime;
  Elevation_t *elevation;
  TransmissionAndSpeed_t *speed;
  PositionalAccuracy_t *posAccuracy;
  PositionConfidenceSet_t *posConfidence;
  SpeedandHeadingandThrottleConfidence_t *speedConfidence;
  Snapshot_t *snapshot;
  VehicleSafetyExtension_t *safetyExt;

	wsmreq.chaninfo.channel = 172;
	wsmreq.chaninfo.rate = 3;
	wsmreq.chaninfo.txpower = 15;
	wsmreq.version = 1;
	wsmreq.security = 1;
	wsmreq.psid = 10;
	wsmreq.txpriority = 1;
	memset ( &wsmreq.data, 0, sizeof( WSMData));

	pvd = (ProbeVehicleData_t *) calloc(1,sizeof(ProbeVehicleData_t));
  pvd->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  pvd->msgID.size = sizeof(uint8_t);
  pvd->msgID.buf[0] = DSRCmsgID_probeVehicleData;
  pvd->startVector.Long = 792632831;
  pvd->startVector.lat = 707074190;
  utcTime = (DDateTime_t *) calloc(1, sizeof(DDateTime_t));
  pvd->startVector.utcTime = utcTime;
  utcTime->year = (DYear_t *) calloc(1, sizeof(DYear_t));
  utcTime->month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
  utcTime->day = (DDay_t *) calloc(1, sizeof(DDay_t));
  utcTime->hour = (DHour_t *) calloc(1, sizeof(DHour_t));
  utcTime->minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
  utcTime->second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
  utcTime->year[0] = 2010;
  utcTime->month[0] = 1;
  utcTime->day[0] = 16;
  utcTime->hour[0] = 16;
  utcTime->minute[0] = 16;
  utcTime->second[0] = 15000;
  elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
  elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
  elevation->size = 2 * sizeof(uint8_t);
  elevation->buf[0] = 63;
  elevation->buf[1] = 246;
  pvd->startVector.elevation = elevation;
  pvd->startVector.heading = (Heading_t *) calloc(1, sizeof(Heading_t));
  pvd->startVector.heading[0] = 2983;
  speed = (TransmissionAndSpeed_t *) calloc(1, sizeof(TransmissionAndSpeed_t));
  speed->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
  speed->size = 2 * sizeof(uint8_t);
  speed->buf[0] = 0;
  speed->buf[1] = 0;
  pvd->startVector.speed = speed;
  posAccuracy = (PositionalAccuracy_t *) calloc(1, sizeof(PositionalAccuracy_t));
  posAccuracy->buf = (uint8_t *) calloc(4, sizeof(uint8_t));
  posAccuracy->size = 4 * sizeof(uint8_t);
  posAccuracy->buf[0] = 0;
  posAccuracy->buf[1] = 0;
  posAccuracy->buf[2] = 0;
  posAccuracy->buf[3] = 0;
  pvd->startVector.posAccuracy = posAccuracy;
  posConfidence = (PositionConfidenceSet_t *) calloc(1, sizeof(PositionConfidenceSet_t));
  posConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  posConfidence->size = sizeof(uint8_t);
  posConfidence->buf[0] = 0;
  pvd->startVector.posConfidence = posConfidence;
  speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1, sizeof(SpeedandHeadingandThrottleConfidence_t));
  speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  speedConfidence->size = sizeof(uint8_t);
  speedConfidence->buf[0] = 0;
  pvd->startVector.speedConfidence = speedConfidence;
  pvd->vehicleType.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
  pvd->vehicleType.size = sizeof(uint8_t);
  pvd->vehicleType.buf[0] = 4;
  pvd->cntSnapshots = (Count_t *) calloc(1, sizeof(Count_t));
  pvd->cntSnapshots[0] = 1;
  snapshot = (Snapshot_t *) calloc(1, sizeof(Snapshot_t));
  snapshot->thePosition.Long = 0;
  snapshot->thePosition.lat = 0;
  safetyExt = (VehicleSafetyExtension_t *) calloc(1, sizeof(VehicleSafetyExtension_t));
  safetyExt->events = (EventFlags_t *) calloc(1, sizeof(EventFlags_t));
  safetyExt->events[0] = 1;
  snapshot->safetyExt = safetyExt;
  ASN_SEQUENCE_ADD(&pvd->snapshots.list, snapshot); 
  snapshot = (Snapshot_t *) calloc(1, sizeof(Snapshot_t));
  safetyExt = (VehicleSafetyExtension_t *) calloc(1, sizeof(VehicleSafetyExtension_t));
  safetyExt->events = (EventFlags_t *) calloc(1, sizeof(EventFlags_t));

  snapshot->thePosition.Long = 1000;
  snapshot->thePosition.lat = 1000;
  safetyExt->events[0] = 1;
  ASN_SEQUENCE_ADD(&pvd->snapshots.list, snapshot);
	
	rvalenc = der_encode_to_buffer(&asn_DEF_ProbeVehicleData, pvd, &wsmreq.data.contents, 1000);
  if (rvalenc.encoded == -1) {
    fprintf(stderr, "Cannot encode %s: %s\n",
                  rvalenc.failed_type->name, strerror(errno));
  } else  {
    printf("Structure successfully encoded %d\n", rvalenc.encoded);
    wsmreq.data.length = rvalenc.encoded;
    asn_DEF_ProbeVehicleData.free_struct (&asn_DEF_ProbeVehicleData, pvd, 0);
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
//	int pwrvalues, ratecount, txprio, ret = 0, pktcount, count = 0;
	int ret = 0, count = 0;

	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);

	while(1) {	
		buildWSMRequestPacket();
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

	
