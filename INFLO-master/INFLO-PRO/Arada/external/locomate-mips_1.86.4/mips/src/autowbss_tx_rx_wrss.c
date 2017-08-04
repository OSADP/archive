/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/
#include <pthread.h>
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
#define USER 0
#define PROVIDER 1
#define SAMPLE_APP_PSID  6
#define PKT_DELAY_SECS 0
#define PKT_DELAY_NSECS 100000000


enum { ADDR_MAC = 0, UINT8 };

//static PSTEntry entry;
//static USTEntry ust;
static WMEApplicationRequest wreq;
static WMEApplicationRequest entry;
static WMEApplicationRequest ust;
static WSMRequest wsmreq;
//static WMECancelTxRequest cancelReq;
static WMEWRSSRequest wrssrq;
static WMETARequest tareq;
void    receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void    receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication);
void    receiveTsfTimerIndication(TSFTimer *timer);
int 	confirmBeforeJoin(WMEApplicationIndication *);

int buildPSTEntry();
int buildWSMRequestPacket();
int buildWMEApplicationRequest();
int buildWMETARequest();
int txWSMPPkts(int);
void wrss_request();

void sig_int(void);
void sig_term(void);
int  extract_macaddr(u_int8_t *, char * );
void set_args( void * ,void *, int );

static pthread_t localtx, localrx, wrssi;

static void *tx_client( void *data );
static void *rx_client( void *data );
static void *wrssi_client( void *arg );

static int pid;
static  uint64_t count = 0, blank = 0;
static uint64_t packets;
static uint64_t drops = 0;
static struct timespec ntsleep, ntleft_sleep;
int sendreport = 1;
int retry = 0;
int rxclient = 1;
uint8_t channelaccess;

static WSMIndication rxpkt;
struct arguments{
    u_int8_t macaddr[ 17 ];
    u_int8_t channel;
};

struct ta_argument {
    uint8_t  channel;
    uint8_t channelinterval;
} taarg;

extern void mysleep(int sleep_secs, long int sleep_nsecs);

int waveappmode;

int main( int argc, char *argv[] )
{
//	int result;
    //int waveappmode;
         void *status_ptr;
  //       long *thread_id;
         long processid;
         int ret;
         pthread_attr_t attr;
        struct sched_param param;
	int chan; 

	pid = getpid();
    struct arguments arg[3];

    	if(argc < 4 )
    	{
		printf("usage:\nRegistering provider:\ngetwbsstxrx [<MAC ADDRESS>] [<CHANNEL>] 1 [sch channel access <1 - alternating> <0 - continous>] [TA channel ] [ TA channel interval <1- cch int> <2- sch int>]\nRegistering User:\ngetwbsstxrx [<MAC ADDRESS>] [<CHANNEL>] 0 [user req type<1-auto> <2-unconditional> <3-none>] [imm access] [extended access] \n");
		exit (-1);
    	}
    strncpy( arg->macaddr, argv[1], 17);
    //sscanf(argv[2], "%d", &arg->channel);
    chan = atoi(argv[2]);
    arg->channel = (char)chan;
    sscanf(argv[3], "%d", &waveappmode);

	/* catch control-c and kill signal*/
    signal(SIGINT,(void *)sig_int);
    signal(SIGTERM,(void *)sig_term);

    if( waveappmode == USER ) 
    {
        if (argc < 7) {
            printf("usage: getwbsstxrx [<MAC ADDRESS>] [<CHANNEL>] 1  [user req type<1-auto> <2-unconditional> <3-none>] [imm access] [extended access]\n");
        } 
        printf("Inside User process1\n");
        memset(&ust, 0 , sizeof(WMEApplicationRequest));
        ust.psid = SAMPLE_APP_PSID;
        if ((atoi(argv[4]) > USER_REQ_SCH_ACCESS_NONE) || (atoi(argv[4]) < USER_REQ_SCH_ACCESS_AUTO)) {
            printf("User request type invalid: setting default to auto\n");
	    ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO;

        } else {
	    ust.userreqtype = atoi(argv[4]);
        }
        if (ust.userreqtype == USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL) {
            ust.channel = atoi(argv[2]);
        }
        ust.schaccess = atoi(argv[5]);
        ust.schextaccess = atoi(argv[6]);

        printf("Invoking WAVE driver \n");
       
        registerLinkConfirm(confirmBeforeJoin);
        if (invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0)
        {
            printf("Open Failed. Quitting\n");
            exit(-1);
        }
     /* printf("Registering User %d  app pid = %d\n", ust.psid,pid);
        if ( registerUser(pid, &ust) < 0)
        {
            printf("ERR::Register User Failed \n");
            printf("Removing user if already present  %d\n", !removeUser(pid, &ust));
            printf("USER Registered %d with PSID =%d \n", registerUser(pid, &ust), ust.psid );
        } */ 
        
        printf("In User end\n"); 
    }


    else if( waveappmode == PROVIDER ) 
    {
        taarg.channel = atoi(argv[5]); 
        taarg.channelinterval = atoi(argv[6]);
        if (atoi(argv[4]) > 1) {
            printf("channel access set default to alternating access\n");
            channelaccess = CHACCESS_ALTERNATIVE;
        } else {
            channelaccess =  atoi(argv[4]); 
        }
 
        printf("Inside Provider process\n");
	    printf("Filling Provider Service Table entry %d\n",buildPSTEntry());
        printf("Building a WME Application  Request %d\n",buildWMEApplicationRequest());
        printf("Builing TA request %d\n", buildWMETARequest());
        
        printf("Invoking WAVE driver \n");
	    if (invokeWAVEDriver(0) < 0)
        {
                printf("Open Failed. Quitting\n");
                exit(-1);
        } else {
            printf("Driver Invoked\n");
        }

	    registerWMENotifIndication(receiveWME_NotifIndication);
        registerWRSSIndication(receiveWRSS_Indication);
        registertsfIndication(receiveTsfTimerIndication);
	
        printf("starting TA\n");
        if (transmitTA(&tareq) < 0)  {
            printf("send TA failed\n ");
        } else {
            printf("send TA successful\n") ;
        }

	/*    printf("Registering provider\n ");
        if ( registerProvider( pid, &entry ) < 0 ){
                printf("ERR::Register Provider failed\n");
                removeProvider(pid, &entry);
                registerProvider(pid, &entry);
        } else {
                printf("provider registered with PSID = %d\n",entry.psid );
        } */
        /*if ( startWBSS ( pid, &wreq) < 0) {
                printf("ERR::WBSS start failed\n" );
                exit (-1);
        } else {
                printf("\nWBSS started\n");
        }*/

   }

   else 
   {
        printf("ERR: Input value wrong for waveappmode\n");
   }
   pthread_attr_init (&attr);
   pthread_attr_setschedpolicy (&attr, SCHED_OTHER);
   pthread_attr_setschedparam (&attr, &param);
   pthread_attr_setinheritsched (&attr, PTHREAD_INHERIT_SCHED);
   processid = getpid();
        ret = pthread_create(&localrx, NULL, rx_client, NULL );
        if (ret) {
        }
        sched_yield();
        
        pthread_create(&localtx, NULL, tx_client, NULL );
        sched_yield();

        pthread_create(&wrssi, NULL, wrssi_client, (void *)arg );
        sched_yield();
        if ((ret = pthread_join( localrx, &status_ptr )) < 0) {
            printf("join err\n");
        }
        pthread_join( localtx, NULL );
        pthread_join( wrssi, NULL );
        while( 1 );

}

void *rx_client( void *data )
{
	int ret = 0;
	pid = getpid(); 

	if( waveappmode == USER ) 
	{
		printf("Registering User %d  app pid = %d\n", ust.psid,pid);
        	if ( registerUser(pid, &ust) < 0)
        	{
            		printf("ERR::Register User Failed \n");
            		printf("Removing user if already present  %d\n", !removeUser(pid, &ust));
            		printf("USER Registered %d with PSID =%u \n", registerUser(pid, &ust), ust.psid );
        	}  
	}
    	else if(waveappmode == PROVIDER)
	{
		printf("Registering provider\n ");
        	if ( registerProvider( pid, &entry ) < 0 )
		{
                	printf("ERR::Register Provider failed\n");
                	removeProvider(pid, &entry);
                	registerProvider(pid, &entry);
        	} 
		else 
		{
                	printf("provider registered with PSID = %u\n",entry.psid );
        	} 
	}

        while(1)  {
                ret = rxWSMPacket(pid, &rxpkt);
                printf("Ret = %d\n", ret );
                if (ret > 0){
                        printf("Received WSMP Packet Channel = %d, txpower= %d, rateindex=%d Packet No =#%llu#\n", rxpkt.chaninfo.channel, rxpkt.chaninfo.txpower, rxpkt.chaninfo.rate, count++);
                } else {
                        blank++;
                }
                sched_yield();
                mysleep( PKT_DELAY_SECS , PKT_DELAY_NSECS);
        }
}

void *tx_client( void *data )
{	
	int result;
	pid = getpid();
    printf("Tx client thread id = %d\n", pid);
    printf("Building a WSM Request Packet %d\n", buildWSMRequestPacket());
    while( 1 )
    {
	    result =txWSMPPkts(pid);
        sched_yield();
        mysleep( PKT_DELAY_SECS , PKT_DELAY_NSECS);
    }
	
}


void *wrssi_client( void *arg )
{
    //int sts;
    struct arguments *argument;
    argument = ( struct arguments *)arg;
  
 //   set_args( &wrssrq.macaddr, argument->macaddr, ADDR_MAC, 0);
   // set_args( &wrssrq.wrssreq_elem.request.channel, NULL, UINT8, argument->channel );
    set_args( &wrssrq.macaddr, argument->macaddr, ADDR_MAC);
    //set_args( &wrssrq.wrssreq_elem.request.channel, &argument->channel, UINT8);
    wrssrq.wrssreq_elem.request.channel = (char )argument->channel;
    registerWRSSIndication(receiveWRSS_Indication);

    signal(SIGINT,(void *)sig_int);
    signal(SIGTERM,(void *)sig_term);
	memset(&ntsleep, 0, sizeof(ntsleep));
    memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));

    while(1)
    {
        mysleep( PKT_DELAY_SECS , PKT_DELAY_NSECS);
	sendreport = 1;
        wrss_request();
    }
   
/*
    while(1)
    {
        ntsleep.tv_nsec = 0;
        ntsleep.tv_sec = 1;
        ntleft_sleep.tv_nsec = 0;
        ntleft_sleep.tv_sec = 0;
		do 
		{
		    	sts = nanosleep(&ntsleep, &ntleft_sleep);
                if((ntleft_sleep.tv_nsec == 0) && (ntleft_sleep.tv_sec == 0)) {
                wrss_request();
                break;
            	}
		        memcpy(&ntsleep,&ntleft_sleep, sizeof(ntsleep));
                memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
		}
		while( 1 );
        }
*/

}	

int buildPSTEntry(){

        entry.psid = SAMPLE_APP_PSID;
        entry.priority = 1;
        entry.channel = 172;
        entry.serviceport = 0;
	entry.repeatrate = 50; // repeatrate =50 per 5seconds = 1Hz
	entry.linkquality = 1;
        entry.channelaccess = channelaccess;

        return 1;
}

int buildWSMRequestPacket()
{
        wsmreq.chaninfo.channel = 172;
        wsmreq.chaninfo.rate = 3;
        wsmreq.chaninfo.txpower = 15;
        wsmreq.version = 1;
        wsmreq.security = 1;
        wsmreq.psid = SAMPLE_APP_PSID;
        wsmreq.txpriority = 1;
        memset ( &wsmreq.data, 0, sizeof( WSMData));
        return 1;

}

int  buildWMEApplicationRequest()
{
        wreq.psid =SAMPLE_APP_PSID ;
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
        //int i ;
        int ret = 0, count = 0;
            ret = txWSMPacket(pid, &wsmreq);
            if( ret < 0) {
                 printf("ERR::txWSMPacket status=%d\n", ret);
                 drops++;
            }
            else {
                 packets++;
                 count++;
            }
            printf("Transmitted #%llu#   dropped #%llu#\n", packets, drops);
		return 0;
}


void set_args( void *data ,void *argname, int datatype )
{
    u_int8_t string[1000];
   // int i;
   // int temp = 0;
   // u_int8_t temp8 = 0;
    struct arguments *argument1;
    argument1 = ( struct arguments *)argname;
    switch(datatype) {
        case ADDR_MAC:
            memcpy(string, argument1->macaddr, 17);
            string[17] = '\0';
            if(extract_macaddr( data, string) < 0 )
            {
                printf("invalid address\n");
            }
            break;

        case UINT8:
    
            //temp = atoi(argument1->channel);
            memcpy( data, (char *)argname, sizeof( u_int8_t));
            break;
    }
}



int extract_macaddr(u_int8_t *mac, char *str)
{
    int maclen = IEEE80211_ADDR_LEN;
    int len = strlen(str);
    int i = 0, j = 0, octet = 0, digits = 0, ld = 0, rd = 0;
    char num[2];
    u_int8_t tempmac[maclen];
    memset(tempmac, 0, maclen);
    memset(mac, 0, maclen);
    if( (len < (2 * maclen - 1)) || (len > (3 * maclen - 1)) )
        return -1;
    while(i < len)
    {
        j = i;
        while( str[i] != ':' && (i < len) ){
         i++;
        }
        if(i > len) exit(0);
        digits = i - j;
        if( (digits > 2) ||  (digits < 1) || (octet >= maclen)){
            return -1;
        }
        num[1] = tolower(str[i - 1]);
        num[0] = (digits == 2)?tolower(str[i - 2]) : '0';
        if ( isxdigit(num[0]) && isxdigit(num[1]) ) {
            ld  =  (isalpha(num[0]))? 10 + num[0] - 'a' : num[0] - '0';
            rd  =  (isalpha(num[1]))? 10 + num[1] - 'a' : num[1] - '0';
            tempmac[octet++] =  ld * 16 + rd ;
        } else {
            return -1;
        }
        i++;
    }
    if(octet > maclen)
        return -1;
    memcpy(mac, tempmac, maclen);
    return 0;
}

int confirmBeforeJoin(WMEApplicationIndication *appind)
{
    //    printf("\nJoin\n");
        return 1; /*Return 0 for NOT Joining the WBSS*/
}

void wrss_request()
{
	int result;
	if( sendreport || retry )
	{
    		result = getWRSSReport( pid, &wrssrq );
    		if( result < 0) {
            		//printf(" result = %d\n", result );
        		//printf( "WRSS Request Failed");
    		}
		sendreport = 0;
	}
}

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication)
{
}

void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication)
{
    	printf("WRSS receive Channel = %d   Report = %d\n", (u_int8_t)wrssindication->wrssreport.channel,
	                                                                (u_int8_t)wrssindication->wrssreport.wrss);
	    sendreport = 1; 	
}

void receiveTsfTimerIndication(TSFTimer *timer)
{
        printf("TSF Timer: Result=%d, Timer=%llu",(u_int8_t)timer->result,(u_int64_t)timer->timer);
}

void sig_int(void)
{
        int ret;
        
        pthread_cancel(localrx);
        pthread_cancel(localtx);
        pthread_cancel(wrssi);
        ret = removeUser(pid, &ust);
        ret = stopWBSS(pid, &wreq);
        removeProvider(pid, &entry);
        signal(SIGINT,SIG_DFL);
        printf("\n\nPacket received = %llu\n", count);
        printf("remoterx killed by kill signal\n");
        printf("\n\nPackets Sent =  %llu\n",packets);
        printf("Packets Dropped = %llu\n",drops);
        printf("localtx killed by control-C\n");
        exit(0);

}

void sig_term(void)
{
      //  int ret;

    sig_int();
}



