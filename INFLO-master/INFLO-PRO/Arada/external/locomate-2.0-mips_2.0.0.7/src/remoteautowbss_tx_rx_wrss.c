/*copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/ioctl.h>
#include <errno.h>
#include <ifaddrs.h>
#include <fcntl.h>
#include <pthread.h>
#include "wave.h"

#define USER 0
#define PROVIDER 1
#define SAMPLE_APP_PSID 5
#define PKT_DELAY_SECS 0
#define PKT_DELAY_NSECS 100000000

enum { ADDR_MAC = 0, UINT8 };

static WMEWRSSRequest wrssrq;
static WMEApplicationRequest wreq;
static WMEApplicationRequest ust;
static WMEApplicationRequest entry;
//static WSMIndication wsmrxind;
static WSMRequest wsmreq;
static WMETARequest tareq;
//static WMECancelTxRequest cancelReq;
static WMEApplicationRequest aregreq;

static int devicemode = WAVEDEVICE_REMOTE;

void    receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void    receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication);
void    receiveWSMIndication(WSMIndication *wsmindication);
int     confirmBeforeJoin(WMEApplicationIndication *);

void sig_int(void);
void sig_term(void);

int  extract_macaddr(u_int8_t *, char * );
void set_args( void * ,void *, int );
int buildUSTEntry();
int buildPSTEntry();
int buildWSMRequestPacket();
int buildWMEApplicationRequest();
int buildWMETARequest();
int txWSMPPkts(int);
int rxWSMPPkts(int);
void wrss_request();

static uint64_t packets = 0;
static uint64_t drops = 0;
static uint64_t count = 0;
//int sendreport = 1;
//int retry = 0;
uint8_t channelaccess;

static pthread_t remotetx, remoterx, wrssi;

static void *tx_client( void *data );
static void *rx_client( void *data );
static void *wrssi_client( void *arg );

static struct timespec ntsleep, ntleft_sleep;

static int pid;

struct arguments{
    u_int8_t macaddr[ 17 ];
    u_int8_t channel;
};
struct arguments arg[4];
struct ta_argument {
	uint8_t channel;
	uint8_t channelinterval;
} taarg;


extern void mysleep(int sleep_secs, long int sleep_nsecs);

int main (int argc, char *argv[]) 
{
	int ret;
	//char server[255];
    	int blockflag = 0 ;
    	//struct ifreq ifr;
    	int waveappmode;
    	//WSMIndication rxpkt;
    
    	if( argc < 5 )
    	{
        	printf("Usage:\nRegistering Provider::\nremotetxrx [<IP ADDRESS] [<MAC ADDRESS>] [<CHANNEL>] 1 [sch-channel access <1-alternating> <0-continous> ] [TA Channel] [TA Channel interval <1-cch int> <2-sch int> ]\nRegistering User::\nremotetxrx [IP ADDRESS] [MAC ADDRESS] [CHANNEL] 0 [User-req type <1-auto> <2-unconditional> <3-none> ] [imm access] [extended access] \n");
        	exit (-1);
    	}

    	strncpy( arg->macaddr, argv[2], 17 );
    	sscanf( argv[3], "%hhd", &arg->channel );
    	sscanf( argv[4], "%d", &waveappmode );


	pid = getpid();
    	signal(SIGINT,(void *)sig_int);
    	signal(SIGTERM,(void *)sig_term);

    	if( waveappmode == USER )
    	{
        
#ifdef  WIN32

        	char szHostName[255];
            	char *szLocalIP;
            	struct hostent *host_entry;
            	WIN_SOCK_DLL_INVOKE
            	gethostname(szHostName, 255);
#endif
            	//buildUSTEntry();
            	memset(&ust, 0 , sizeof(USTEntry));
            	ust.psid = SAMPLE_APP_PSID;
		if( (atoi( argv[5] ) > USER_REQ_SCH_ACCESS_NONE ) || (atoi(argv[5]) < USER_REQ_SCH_ACCESS_AUTO )) {
			printf("User request type invalid: setting default to auto\n");
			ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO;
		}
		else {
			ust.userreqtype = atoi( argv[5] );
		}
		ust.schaccess = atoi( argv[5] );
		ust.schextaccess = atoi( argv[6] );
		
            	setRemoteDeviceIP(argv[1]);
            	registerLinkConfirm(confirmBeforeJoin);
            	devicemode = WAVEDEVICE_REMOTE;
            	ret =  invokeWAVEDevice(devicemode, blockflag); /*blockflag is ignored in this case*/

            	if (ret < 0 ) {
            	} else {
                	printf("Driver invoked\n");
            	}

#if 0
            	/*Get the IP of eth0*/
            	sfd = socket(AF_INET6, SOCK_STREAM, 0);
            	if(sfd >= 0) {
                	memset(&ifaddr, 0, sizeof(ifaddr));

#ifdef WIN32
            	if((host_entry=gethostbyname(szHostName))!=NULL)
            	{
                	szLocalIP = inet_ntoa (*(struct in_addr *)*host_entry->h_addr_list);
                	sin->sin_addr.s_addr = inet_addr(szLocalIP);
                	aregreq.ipv6addr = sin->sin_addr;
                	ust.ipaddr = sin->sin_addr;
            	}
#else
		if( getifaddrs(&ifaddr) == 0 )
            	{
			for( ifa = ifaddr; ifa != NULL; ifa = ifa->ifa_next )
			{
				if( ifa->ifa_addr == NULL ) continue;
                                if( ( ifa->ifa_flags & IFF_UP ) == NULL ) continue;

				if(ifa->ifa_addr->sa_family == AF_INET )
				{
					sin4 = (struct sockaddr_in *)(ifa->ifa_addr);
					if(inet_ntop(ifa->ifa_addr->sa_family,(void *)&(sin4->sin_addr), str, sizeof(str)) == NULL )
					{
						 printf("IPV4 Interface = %s: inet_ntop failed\n", ifa->ifa_name );
					}
					else
					{
						inet_pton( AF_INET, argv[1], &inaddr );
						printf("IPV4 Interface =%s      %s\n",ifa->ifa_name, str );
					}
				}
				else if( ifa->ifa_addr->sa_family = AF_INET6 )
				{
					sin6 = (struct sockaddr_in6 *)(ifa->ifa_addr);
					if(inet_ntop(ifa->ifa_addr->sa_family,(void *)&(sin6->sin6_addr), str, sizeof(str)) == NULL )
					{
						printf("IPV6 Interface = %s: inet_ntop failed\n", ifa->ifa_name );
					}
					else
					{
						inet_pton( AF_INET6, argv[1], &inaddr );
                                                printf("IPV6 Interface =%s      %s\n",ifa->ifa_name, str );
					}
				}
			}
                	aregreq.ipv6addr = sin6->sin6_addr;
                	ust.ipv6addr = sin6->sin6_addr;
            	} else 
            	{
                	ret = inet_pton (AF_INET, argv[1], &inaddr);
			if( ret != 1 )
			{
                		inet_pton (AF_INET6, argv[1], &inaddr);
				if( ret != 1 )
					perror("inet_pton() failed");
			}
                	aregreq.ipv6addr = inaddr;
                	ust.ipv6addr = inaddr;
            	}
#endif
	}
#endif
		getUSTIpv6Addr(&ust.ipv6addr,"eth0");
	    aregreq.ipv6addr = ust.ipv6addr;

        /*Register a call back function with LIBWAVE to receive WME Notifications and WSMIndications from TARGET*/
        registerWMENotifIndication( receiveWME_NotifIndication );
        registerWSMIndication(receiveWSMIndication);

        /*Set the notification and service PORTS*/
        aregreq.notif_port = 9999;
        ust.serviceport = 8888;
        //ust.serviceport = 7777;

        /*Tell LIBWAVE where to listen for notifications*/
        setWMEApplRegNotifParams(&aregreq);

        /*Start recieving packets*/
        printf("Registering User\n");

        if (registerUser(pid, &ust) < 0)
        {
            	printf("Register User Failed \n");
            	printf("Removing user if already present  %d\n", !removeUser(pid, &ust));
            	printf("USER Registered %d with PSID =%u \n", registerUser(pid, &ust), ust.psid );
        }

	}
   
    	else if( waveappmode == PROVIDER )
    	{
#ifdef  WIN32
		char szHostName[255];
            	char *szLocalIP;
            	struct hostent *host_entry;
            	WIN_SOCK_DLL_INVOKE
            	gethostname(szHostName, 255);

#endif
		taarg.channel = atoi(argv[5]);
		taarg.channelinterval = atoi(argv[6]);
		if( atoi(argv[5]) > 1 ) {
			printf("channel access set default to alternating access\n");
			channelaccess = CHACCESS_ALTERNATIVE;
		}
		else {
			channelaccess = atoi(argv[5] );
		}
            	printf("Filling Provider Service Table entry %d\n", buildPSTEntry() );
            	printf("Building a WME Application  Request %d\n", buildWMEApplicationRequest() );
            	//printf("Building a WSM Request Packet %d\n", buildWSMRequestPacket() );
		printf("Building TA Request %d\n", buildWMETARequest() );

            	/*Provide the IP address of the TARGET WAVE-device*/
            	setRemoteDeviceIP(argv[1]);
		registerLinkConfirm(confirmBeforeJoin);
            	devicemode = WAVEDEVICE_REMOTE;
            	ret = invokeWAVEDevice(devicemode, blockflag); /*blockflag is ignored in this case*/
            	if (ret < 0 ) {
            	/*Error*/
            	} else {
                	printf("Driver invoked\n");
            	}
#if 0
            	/*Get the IP address of eth0*/
            	sfd = socket(AF_INET6, SOCK_STREAM, 0);
            	if(sfd >= 0) {
                	memset(&ifaddr, 0, sizeof(ifaddr));

#ifdef WIN32
            	if((host_entry=gethostbyname(szHostName))!=NULL){
                szLocalIP = inet_ntoa (*(struct in_addr *)*host_entry->h_addr_list);
                sin->sin_addr.s_addr = inet_addr(szLocalIP);
                aregreq.ipv6addr = sin->sin_addr;
                entry.ipaddr = sin->sin_addr;
            	}

#else
		if( getifaddrs(&ifaddr) == 0 )
		{
			for( ifa = ifaddr; ifa != NULL; ifa = ifa->ifa_next )
			{
				if( ifa->ifa_addr == NULL ) continue;
                                if( ( ifa->ifa_flags & IFF_UP ) == NULL ) continue;

                                if(ifa->ifa_addr->sa_family == AF_INET )
				{
					sin4 = (struct sockaddr_in *)(ifa->ifa_addr);
					if(inet_ntop(ifa->ifa_addr->sa_family,(void *)&(sin4->sin_addr), str, sizeof(str)) == NULL )
					{
						printf("IPV4 Interface = %s: inet_ntop failed\n", ifa->ifa_name );
					}
					else
					{
						inet_pton( AF_INET, argv[1], &inaddr );
						printf("IPV4 Interface =%s      %s\n",ifa->ifa_name, str );
					}
				}
				else if( ifa->ifa_addr->sa_family = AF_INET6 )
				{
					sin6 = (struct sockaddr_in6 *)(ifa->ifa_addr);
                                        if(inet_ntop(ifa->ifa_addr->sa_family,(void *)&(sin6->sin6_addr), str, sizeof(str)) == NULL )
					{
						printf("IPV6 Interface = %s: inet_ntop failed\n", ifa->ifa_name );
					}
					else			
					{
						inet_pton( AF_INET6, argv[1], &inaddr );
                                                printf("IPV6 Interface =%s      %s\n",ifa->ifa_name, str );
					}
				}
			}
			aregreq.ipv6addr = sin6->sin6_addr;
                        entry.ipv6addr = sin6->sin6_addr;
            	} 
		else {
                	ret = inet_pton (AF_INET, argv[1], &inaddr);
			if( ret != 1 )
			{
                		inet_pton (AF_INET6, argv[1], &inaddr);
				if( ret != 1 )
					perror("inet_pton() failed");
			}
                	aregreq.ipv6addr = inaddr;
                	entry.ipv6addr = inaddr;
           	}

#endif
        }
#endif
		getUSTIpv6Addr(&entry.ipv6addr,"eth0");
        aregreq.ipv6addr = entry.ipv6addr;
		
	
	/*Register a call back function with LIBWAVE to receive WME Notifications and WSMIndications from TARGET*/
        //registerWMENotifIndication(receiveWME_NotifIndication);
        registerWSMIndication(receiveWSMIndication);
        registerWRSSIndication(receiveWRSS_Indication);

        /*Set the notification IP and PORT*/
        aregreq.notif_port = 6666;
	entry.serviceport = 8888;

        /*Tell LIBWAVE where to listen for notifications*/
        setWMEApplRegNotifParams(&aregreq);

        printf("Registering provider\n ");

        /*Remove any provider with the same ACID-ACM*/
        /*NOTE:If the TARGET device is not up or the link is down the libwave calls will wait indefinetly*/
        removeProvider(pid, &entry );

        /*Register a  Provider on the TARGET, Note: Most of the libawave functions are identical whether the device is local or remote*/
        /*the only difference being that remote calls may hang (when no reply comes from TARGET) and the way WSMIndications are received*/

        	if (registerProvider(pid, &entry ) < 0 ) {
            		printf("Register Provider failed\n");
            		exit(-1);
        	} else {
        		printf("Provider registered with PSID = %u \n", entry.psid);
        	}
	printf("starting TA\n");
	if( transmitTA(&tareq) < 0 ) {
		printf("send TA failed\n");
	}
	else {
		printf("send TA Successful\n");
	}
	}

    	else
    	{
        	printf("ERROR: Input Value wrong for Waveappmode\n");
    	} 

        pthread_create( &remoterx, NULL, rx_client, NULL );
        sched_yield();
    
        pthread_create( &remotetx, NULL, tx_client, NULL );
        sched_yield();
        
        pthread_create( &wrssi, NULL, wrssi_client, (void *)arg );
        sched_yield();

        pthread_join( remoterx, NULL );
        pthread_join( remotetx, NULL );
        pthread_join( wrssi, NULL );
        while(1);
     
}

void *rx_client( void *data )
{
	return NULL;
    while( 1 )
    {
        sched_yield();
        mysleep( PKT_DELAY_SECS, PKT_DELAY_NSECS );
    }
}

void *tx_client( void *data )
{
	int ret;
    	pid = getpid();
    	printf("Building a WSM Request Packet %d\n", buildWSMRequestPacket() );
    	while( 1 )
    	{
        	ret = txWSMPPkts(pid);
        	sched_yield();
        	mysleep( PKT_DELAY_SECS, PKT_DELAY_NSECS );
    	}
}

void *wrssi_client( void *arg )
{
    	struct arguments *argument;
    	argument = ( struct arguments *)arg;
    
    	set_args( &wrssrq.macaddr, argument->macaddr, ADDR_MAC);
    	set_args( &wrssrq.wrssreq_elem.request.channel, &argument->channel, UINT8);
    	registerWRSSIndication(receiveWRSS_Indication);

    	signal(SIGINT,(void *)sig_int);
    	signal(SIGTERM,(void *)sig_term);
    	memset(&ntsleep, 0, sizeof(ntsleep));
    	memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
   
    	while(1)
    	{
        	wrss_request();
        	mysleep( PKT_DELAY_SECS , PKT_DELAY_NSECS);
    	}
} 

/*Fill up the data structure  to register a PROVIDER application*/
int buildPSTEntry(){
    	entry.psid = SAMPLE_APP_PSID;
    	entry.priority = 1;
    	entry.channel = arg->channel;
    	/*This is the Port where WSMIndications will be received*/
    	entry.serviceport = 8888;
	entry.repeatrate = 50; // repeatrate =50 per 5seconds = 1Hz
    	entry.linkquality = 1;
	entry.channelaccess = channelaccess;
    	return 1;
}

/*Build a request to transmit a WSM packet*/
int buildWSMRequestPacket() {
    	wsmreq.chaninfo.channel = arg->channel;
    	wsmreq.chaninfo.rate = 3 ;
    	wsmreq.chaninfo.txpower = 15 ;
    	wsmreq.version = 1 ;
    	wsmreq.security = 1 ;
    	/*The ACID acm ACM should be the same as one of the Registerd PROVIDER'S or USER'S*/
    	wsmreq.psid = SAMPLE_APP_PSID;
    	wsmreq.txpriority = 1;
    	memset ( &wsmreq.data, 0, sizeof( WSMData ));
    	return 1;
}

/*Build a request to start a WBSS*/
int  buildWMEApplicationRequest() {
    	wreq.psid = SAMPLE_APP_PSID ;
    	wreq.repeats = 1;
    	wreq.persistence = 1;
    	/*WRSS Request channel should be same as that of PROVIDER*/
    	wreq.channel = arg->channel;
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

/*Transmit the packets here*/
int txWSMPPkts(int pid){
//        int pwrvalues, ratecount, txprio, ret = 0, pktcount, count = 0;
        int  ret = 0, count = 0;
  //	      int i ;
	    usleep(2000);
            ret = txWSMPacket(pid, &wsmreq);
            registerWSMIndication(receiveWSMIndication);
            if( ret < 0) {
                 printf("ERR::txWSMPacket status=%d\n", ret);
                 drops++;
            }
            else {
                 packets++;
                 count++;
            }
            printf("Transmitted #%llu#   	dropped #%llu#\n", packets, drops);
		return 0;
}

 
        
void set_args( void *data ,void *argname, int datatype )
{
    	u_int8_t string[1000];
    	//int i;
    	//int temp = 0;
    	//u_int8_t temp8 = 0;
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
        	while( str[i] != ':' && (i < len) ) {
         		i++;
        	}
        	if(i > len) exit(0);
        	digits = i - j;
        	if( (digits > 2) ||  (digits < 1) || (octet >= maclen)) {
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

void wrss_request()
{
    	int result;
	result = getWRSSReport( pid, &wrssrq );
	if( result < 0) {
		printf(" result = %d\n", result );
            	printf( "WRSS Request Failed");
	}
}

int confirmBeforeJoin(WMEApplicationIndication *appind)
{
    	// printf("\nJoin\n");
    	return 1; /*Return 0 to stop Joining the WBSS*/
}

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication)
{

}

void receiveWSMIndication(WSMIndication *wsmindication)
{
	printf("WSMP Packet received,              packet number=%llu\n", ++count);
}

void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication)
{
        printf("WRSS receive Channel = %d   Report = %d\n", (u_int8_t)wrssindication->wrssreport.channel,
                                                                    (u_int8_t)wrssindication->wrssreport.wrss);
}


void sig_int(void)
{
    	int ret;
    
    	ret = removeUser(pid, &ust);
    	ret = stopWBSS(pid, &wreq);
    	removeProvider(pid, &entry);
    	signal(SIGINT,SIG_DFL);
    	printf("\n\nPackets received = %llu\n", count);
    	printf("remoterx killed by kill signal\n");
    	printf("\n\nPACKTES SENT = %llu\n",packets);
    	printf("PACKTES DROPPED = %llu\n",drops);
    	printf("remotetx killed by control-C\n");
    	exit(0);

}

void sig_term(void)
{
    	int ret;
    
    	ret = removeUser(pid, &ust);
    	ret = stopWBSS(pid, &wreq);
    	removeProvider(pid, &entry);
    	signal(SIGINT,SIG_DFL);
    	printf("\n\nPackets received = %llu\n", count);
    	printf("remoterx killed by kill signal\n");
    	printf("\n\nPACKTES SENT = %llu\n",packets);
    	printf("PACKTES DROPPED = %llu\n",drops);
    	printf("remotetx killed by control-C\n");
    	exit(0);
}
       
