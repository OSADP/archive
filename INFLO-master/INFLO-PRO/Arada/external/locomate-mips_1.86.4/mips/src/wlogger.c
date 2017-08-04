/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include "wave.h"
#include "wavelogger.h"

#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <ifaddrs.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <pthread.h>
#include "os.h"

#define APP_PSID 9
static uint16_t port  =  9876; /*NOTE*/ 
static struct sockaddr_in ip;
static char filename[255];
//static USTEntry ust;
static WMEApplicationRequest ust;
static WSMIndication wsmrxind;
static WMEApplicationRequest aregreq;
static additionalWSMP addwsmp;
static GPSData rxgpsdata;

static pthread_t wsmpthread, timerthread;

static int reqpending = 0;
static int killnow = 0;
static int start_reg = 0;
static uint8_t logformat = 0;
static uint64_t interPacketDelay;

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void receiveWSMIndication(WSMIndication *wsmindication);

extern int parseGPSBinData(GPSData *gps, char *str, int len);
void list_src(void);

int buildUSTEntry();

void *wsmp_thread(void *data);
void *timer(void *data);
void bye_bye(void);
void sig_int(void);
void sig_term(void);

void print_usage()
{
	printf("usage:wlogger <target ipaddr> < [ <host interface> ] [ <host port> ] [ <logfile> ] [ <logformat> ] [<inter packet delay>] > \n");
	printf("\n\n");
	printf("<target ipaddr>  	  :   specifies the ip address of the remote machine from\n");
	printf("                    	      which the packets are to be received. Note: Driver should be loaded at TARGET before invkoing wlogger\n");
	printf("<host interface>	  :   local machine ethernet interface. This option can be\n");
	printf("                    	      used when multiple domains are available in the local machine. Generally its ""eth0"" \n");
	printf("<host port>               :   local port to receive on (Default:9876) \n");
	printf("<logfile>      		  :   Output log filename, default = '/etc/allwavegps.log'\n");
	printf("<logformat>    		  :   Type xml or csv for logging in XML or CSV format. Skip this argument for default format.\n");
	printf("Example        		  :   wlogger 192.168.1.54 eth0 9876 datalog csv\n");
	printf("Example        		  :   Start logging from-192.168.1.54, on the first ethernet interface-eth0, on port-9876, to file- datalog in csv format\n");
	printf("<inter packet delay>      :   Type the delay thats is equal to x_delay parameter in the tx side's gps-wave.config file.\n");
	printf("			      Skip this argument for default 1000000 micro secs (1000000 usec = 1 sec). Only used for finding late packets\n");
	printf("Example       		  :   wlogger 192.168.1.54 9876 datalog csv 1000000\n");
	printf("Note: Supply the arguments in the given order, and none of the INTERMEDIATE arguments can be skipped \n");
	exit(-1);
}


int main(int argc, char* argv[])
{
//	int ret = 0;
//	int sfd;
//	char str[INET6_ADDRSTRLEN];
	struct ifreq ifr;
	struct sockaddr_in *sin = (struct sockaddr_in*)&ifr.ifr_addr;

#ifdef	WIN32

		int iResult,iMode=1;
		WSADATA wsaData;
		char szHostName[255];
		char *szLocalIP;
		struct hostent *host_entry;
	
//    This is required to invoke DLLs for Sockets in WIN32		//

		iResult=WSAStartup(MAKEWORD(2,2),&wsaData);
	
		if(iResult!=0)
		printf("WSASTARTUP Failed: %d\n",iResult);
		gethostname(szHostName, 255);
#endif
	

	if(argc < 2) {
		print_usage();
	}
	
		
	setRemoteDeviceIP(argv[1]);
	reqpending = 0;
	if ( invokeWAVEDevice(WAVEDEVICE_REMOTE, 0) < 0 ) {
		printf( "[WAVEDEVICE: Error==>Open Failed]\n ");
		exit(-1);
	} else {
		printf("[WAVEDEVICE:Invoked]\n");
	}
	
	getUSTIpv6Addr(&ust.ipv6addr,argv[2]);
    aregreq.ipv6addr = ust.ipv6addr;

	registerWMENotifIndication( receiveWME_NotifIndication );
	registerWSMIndication(receiveWSMIndication);
	
	aregreq.notif_port = 6666;
	setWMEApplRegNotifParams(&aregreq);

	printf("[WLOGGER: Logging WSMP from %s]\n", inet_ntoa(sin->sin_addr));	
	
	if(argc > 3) {
		port = atoi(argv[3]);
	} else {
		port = 9876 ;
	}
	if(argc > 4) {
		sprintf(filename, "%s", argv[4]);
	} else {
		#ifdef	WIN32
			sprintf(filename, "%s", ".\\allwavegps.log");
		#else
			sprintf(filename, "%s", "/etc/allwavegps.log");
		#endif
	}
	
	if(argc > 5) {
		if (!strcasecmp(argv[5], "xml")) 
			logformat = 1;
		else if (!strcasecmp(argv[5], "csv"))
			logformat = 2;
		else 
			logformat = 0;
	}
	if(argc > 6) {
		interPacketDelay = atoi(argv[6]);
	} else {
		interPacketDelay = 1000000;
	}

	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);

	/*LOGGING STARTS HERE*/
	reqpending = 0;
	killnow = 0;
	start_reg = 0;
	set_logging_mode(3);
	set_logging_addr(ip, port);
	set_logging_format(logformat); /*1=XML, 2=CSV*/
	set_logfile(filename);
    set_packet_delay(interPacketDelay); /*Call it before open_log*/
	open_log(APP_PSID);
	sleep(1);
	pthread_create(&timerthread, NULL, timer, NULL);
	pthread_create(&wsmpthread, NULL, wsmp_thread, NULL);
	
	printf("[WLOGGER: Listening for IP_UDP data at %s:%u]\n", inet_ntoa(ip.sin_addr), port);
	printf("[WLOGGER: Logging WSMP, IP_UDP to %s]\n", get_logfile());
	printf("[WLOGGER: Press Ctrl-C to Exit]\n");
	start_logging();
	
	while(1);		
}

int buildUSTEntry() {
	
	ust.psid = APP_PSID;
	ust.channel = 178;
	ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL;
	ust.serviceport = 8888;
	
	return 1;
}


void *timer(void *data)
{
	while(1) {
		sleep(20);
		if(killnow) {
			if(reqpending) {
				printf("[EXIT: Error==>Communication with WAVDEVICE timed out, Unregistration of USER failed]\n");
				exit(1);
			}
		}
		if(start_reg) {
			printf("[EXIT: Error==>Communication with WAVDEVICE timed out, USER Registration failed]\n");
			exit(1);
		}
	}

}

void *wsmp_thread(void *data)
{
	int ret;
	buildUSTEntry();
	reqpending = 1;
	start_reg = 1;
	printf("[WAVEDEVICE: Registering WSMP USER...]\n");
	ret = registerUser(getpid(), &ust);
	reqpending = 0;
	if(ret < 0) {
		reqpending = 1;
		printf("[WAVEDEVICE: Removing previous entry of WSMP USER...]\n");
		removeUser(getpid(), &ust);
		reqpending = 0;
		reqpending = 1;
		printf("[WAVEDEVICE: Registering WSMP USER...]\n");
		ret = registerUser(getpid(), &ust);
		reqpending = 0;
		
		if(ret < 0) {
			printf("[WAVEDEVICE: Error==>Registeration of GPS user failed]\n");
			exit(-1);
		}
			
	}
	start_reg = 0;
	return NULL;
}

void receiveWSMIndication(WSMIndication *wsmindication) 
{
	int ret = 0;
	int len = 0;
	char logbuf[1024];
	
	memcpy(&wsmrxind, wsmindication, sizeof(WSMIndication));
	if( (wsmrxind.psid == 9 )) {
		ret = -1;
		if(wsmrxind.data.length  > 11) {
			memcpy(&addwsmp.packetnum, wsmrxind.data.contents, 4);
			memcpy(&addwsmp.rssi, wsmrxind.data.contents + 4, 1);
			//memcpy(addwsmp.macaddr, wsmrxind.data.contents + 5, 6);
			memcpy(addwsmp.macaddr, wsmrxind.macaddr, 6);
			ret = parseGPSBinData(&rxgpsdata, wsmrxind.data.contents + 11, wsmrxind.data.length - 11);
			len = build_gps_logentry(0, logbuf, &wsmrxind, &addwsmp, &rxgpsdata, get_gps_contents());
			if(len > 0) {
				ret = write_logentry(logbuf, len);
				if(ret == FILE_SIZE_EXCEDDED) {
					printf("[WLOGGER: FILE SIZE EXCEDDED, Unregistering User...]\n");
					bye_bye();
				}
			}		
		}
	} else {
		len = build_gps_logentry(0, logbuf, &wsmrxind, NULL, NULL, 0);
		if(len > 0)write_logentry(logbuf, len);
	}

}

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication)
{

}

void bye_bye(void) 
{
	int ret;
	
	close_log();
	if(!reqpending) {
		reqpending = 1;
		killnow = 1;
		ret = removeUser(getpid(), &ust);
		reqpending  = 0;
		printf("[EXIT:Unregisterd USER]\n");
	} else {
		//printf("[EXIT: Error=>No Reply from WAVEDEVICE]\n");
	}
	
#ifndef WIN32	
	//pthread_kill_other_threads_np();		
#endif
	signal(SIGINT,SIG_DFL);
	list_src();
	exit(0);
}
void sig_int(void)
{
	bye_bye();
}

void sig_term(void)
{
	bye_bye();
}


