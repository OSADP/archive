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
#include <netinet/in.h>
#include <fcntl.h>
#include <math.h>
#include <pthread.h>

#ifdef WIN32
#include "win_getopt.h"
#else
#include <getopt.h>
#endif
#include "os.h"

#define APP_PSID 9
//static uint16_t port  =  9876; /*NOTE*/
static uint64_t packets, drops;
static struct sockaddr_in ip;
//static PSTEntry pst;
//static USTEntry ust;
static WMEApplicationRequest ust;
static WMEApplicationRequest pst;
static WSMIndication wsmrxind;
static WMEApplicationRequest aregreq;
static WSMRequest pktreq;
static additionalWSMP addwsmp;
static GPSData wsmgps;
static GPSData rxgpsdata;

static pthread_t wsmpthread, timerthread;

static int reqpending = 0;
static int killnow = 0;
static int start_reg = 0;
static int skfd;
static char remoteip[INET6_ADDRSTRLEN];
static char interface_name[10];
static char logfile[255];
static uint8_t logformat = 0;
static uint64_t interPacketDelay;
static uint8_t txgps = 0;
static uint8_t write_local = 0;

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void receiveWSMIndication(WSMIndication *wsmindication);
int buildPSTEntry();
int buildUSTEntry();
int buildWSMReq();
extern int build_gps_wsmpacket(int sockfd,WSMRequest *wsmtxreq, GPSData *gpsdata, GPS_PACKET gpspkt);
extern long get_gps_txdelay();
extern int parseGPSBinData(GPSData *gps, char *str, int len);
void list_src(void);

void *wsmp_thread(void *data);
void *timer(void *data);
void bye_bye(void);
void sig_int(void);
void sig_term(void);

void usage()
{
	printf("usage:logxmitgps [--remote <deviceip>] [--interface <devicename>] [--logfile <logfilename>] [--format <logformat>] [--transmit] [--write_local] [--delay <interpacketdelay>]\n");
	printf("\n\n");
	printf("<device>              :   Specifies the ip address of the remote/local machine from\n");
	printf("                          which the packets are to be received. Note: Driver should be loaded at TARGET before invkoing this application\n");
	printf("<interface>      	  :   Interfce to recieve remote packets, such as eth0 or br0\n");
	printf("<logfilename>         :   Output log filename, default = '/etc/allwavegps.log'\n");
	printf("<logformat>    		  :   Type xml or csv for logging in XML or CSV format. Type default or ignore to use default format\n");
	printf("<interpacketdelay>    :   Set inter packet delay in Microseconds\n");
	printf("--transmit    		  :   Set this flag to do a WSMP_GPS TX\n");
	printf("--write_local    	  :   Use this flag with --transmit to log the local GPS informatation to logfile\n");
	printf("Example        		  :   logxmitgps --remote 192.168.1.54 --logfile datalog --format csv\n");
	printf("Example        		  :   Start logging from-192.168.1.54, to file- datalog in csv format\n");
	printf("Example        		  :   logxmitgps --remote 192.168.1.54 --logfile datalog --format csv --transmit --write_local\n");
	printf("Example        		  :   Start logging from-192.168.1.54, to file- datalog in csv format and also do a TX and local logging\n");
	printf("Note: Use FIRST letter of long options for short options\n");
	exit(-1);
}

void options(int argc, char *argv[])
{
	int index = 0;
	//int ret = 0;
	int t;
	static struct option opts[] =
	{ 
		{"help", no_argument, 0, 'h'},
		{"transmit", no_argument, 0, 't'},
		{"write_local", no_argument, 0, 'w'},
		{"remote", required_argument, 0, 'r'},
		{"interface", required_argument, 0, 'i'},
		{"logfile", required_argument, 0, 'l'},
		{"format", required_argument, 0, 'f'},
		{"delay", required_argument, 0, 'd'},
		{0, 0, 0, 0}
	};


	/*Initialize the Options*/

	strcpy(remoteip, "192.168.0.1");
	strcpy(interface_name, "eth0");
#ifdef	WIN32
			sprintf(logfile, "%s", ".\\wsmpgps.log");
#else
			sprintf(logfile, "%s", "/etc/wsmpgps.log");
#endif
	logformat = 0;
	txgps = 0;
	write_local = 0;
	interPacketDelay = 1000000;
	
	while(1) {
		t = getopt_long_only(argc, argv, "+htwr:i:l:f:d:", opts, &index);
		if(t < 0)
			break;
			
		switch(t) {
			
			case 'h':
				usage();
			break;
	
			case 'r':
				strcpy(remoteip, optarg);
			break;

			case 'i':
				strcpy(interface_name, optarg);
			break;
			
			case 'l':
				strcpy(logfile, optarg);
			break;
			
			case 'f':
				if(!strcasecmp(optarg, "xml")) {
					logformat = 1;
				} else if (!strcasecmp(optarg, "csv")) {
					logformat = 2;
				} else if (!strcasecmp(optarg, "default")) {
				 	logformat = 0;	
				} else {
					printf("[LOGXMITGPS: Logformat %s not supported. Using default format]\n", optarg);
				}
			break;

			case 'd':
				interPacketDelay = (uint64_t) atoi(optarg);
			break;

			case 't':
				txgps = 1;
			break;

			case 'w':
				write_local = 1;
			break;

			default:	
				usage();
		}
	}
	if(write_local && !txgps) {
			printf("[LOGXMITGPS: Use --write_local option with --transmit to enable local writes of transmited gps packets\n]");
			exit(1);
	}	

}


int main(int argc, char* argv[])
{
//	int ret = 0, c=0;
//	int sfd, family;
//	struct ifreq ifr;
	//struct sockaddr_in *sin = (struct sockaddr_in*)&ifr.ifr_addr;
//	struct sockaddr_in *sin;
//	struct in6_addr inaddr;
	
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
	options(argc, argv);	
	
	setRemoteDeviceIP(remoteip);
	reqpending = 0;
	if(txgps)
	{
		if ( invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0 ) {
			printf( "[WAVEDEVICE: Error==>Open Failed]\n ");
			exit(-1);
		} else {
			printf("[WAVEDEVICE:Invoked]\n");
		}
	} else {
		if ( invokeWAVEDevice(WAVEDEVICE_REMOTE, 0) < 0 ) {
                        printf( "[WAVEDEVICE: Error==>Open Failed]\n ");
                        exit(-1);
                } else {
                        printf("[WAVEDEVICE:Invoked]\n");
                }
	}
	getUSTIpv6Addr(&ust.ipv6addr,interface_name);
	aregreq.ipv6addr = ust.ipv6addr;
	/*Set the notification and service PORTS*/
	aregreq.notif_port = 9999;
	/*if((inet_pton(AF_INET6, remoteip, &sin6->sin6_addr)) <= 0)
		perror("inet_pton() failed");
	else
		ust.ipv6addr = sin6->sin6_addr;*/
		

	ust.serviceport = 8888;
	registerWMENotifIndication( receiveWME_NotifIndication );
	registerWSMIndication(receiveWSMIndication);
	//Add WIN32 code
	//pst.ipaddr = ip.sin_addr;
	//ust.ipv6addr = ip.sin_addr;
//	memcpy( &ust.ipv6addr, &sin->sin_addr, sizeof(struct sockaddr_in) ); vinaya
	//aregreq.ipv6addr = ip.sin_addr;
	//memcpy( &aregreq.ipv6addr, &sin->sin_addr, sizeof(struct sockaddr_in) ); vinaya
	//aregreq.notif_port = 6666; vinaya
	setWMEApplRegNotifParams(&aregreq);

	/*Set the GPSDevice's HOSTIP*/
	/*if(!set_gps_devaddr(remoteip)) {
		printf("[EXIT: Unable to Set GPSDevice's HOSTIP]\n");
		exit(1);
	}*/
	printf("[LOGXMITGPS: Logging WSMP from %s]\n", inet_ntoa(ip.sin_addr));


	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);

	/*LOGGING STARTS HERE*/
	reqpending = 0;
	killnow = 0;
	start_reg = 0;
	if(!txgps)
	{
		set_logging_mode(0);
		set_logging_format(logformat); /*1=XML, 2=CSV*/
		set_logfile(logfile);
		set_packet_delay(interPacketDelay);
		open_log(APP_PSID);
		sleep(1);
		printf("[LOGXMITGPS: Logging WSMP to %s]\n", get_logfile());
	}
	printf("[LOGXMITGPS: Press Ctrl-C to Exit]\n");
	pthread_create(&timerthread, NULL, timer, NULL);
	pthread_create(&wsmpthread, NULL, wsmp_thread, NULL);

	while(1); 
}

int buildPSTEntry() 
{
	pst.psid = APP_PSID;
	pst.serviceport = 8888;
	//pst.ipv6addr = ip.sin_addr;	
	return 0;
}
int buildUSTEntry() 
{
	//ust.psid = 0x09000000;
	ust.psid = APP_PSID;
	ust.channel = 178;
	ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL;
	ust.serviceport = 8888;
	ust.channelaccess = CHACCESS_ALTERNATIVE;
	return 0;
}
int buildWSMReq()
{
	pktreq.version = 7;
	pktreq.security = 8;
	pktreq.chaninfo.channel = 255;
	pktreq.chaninfo.rate = 3;
	pktreq.chaninfo.txpower = 15;
	pktreq.psid = 9;
	//strncpy(pktreq.acm.contents, pst.acm.contents, OCTET_MAX_LENGTH);
	return 0;
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
	int ret,res;
	int pid = getpid();
	static struct timeval tv1, tv2;
	static struct timespec ntsleep, ntleft_sleep;
	static long IPD = 100000000; //Nano-seconds
	//int x = 3 , y = 5;

	//buildPSTEntry();
	buildUSTEntry();
	buildWSMReq();
	reqpending = 1;
	start_reg = 1;
	printf("[WAVEDEVICE: Registering GPS USER...]\n");
	//ret = registerProvider(pid, &pst);
	ret = registerUser(pid, &ust);
	reqpending = 0;
	if(ret < 0) {
		reqpending = 1;
		printf("[WAVEDEVICE: Removing previous entry of GPS User...]\n");
		//removeProvider(pid, &pst);
		removeUser(pid, &ust);
		reqpending = 0;
		reqpending = 1;
		printf("[WAVEDEVICE: Registering GPS User...]\n");
		//ret = registerProvider(pid, &pst);
		ret = registerUser(pid, &ust);
		reqpending = 0;

		if(ret < 0) {
			printf("[WAVEDEVICE: Error==>Registeration of GPS User failed]\n");
			exit(-1);
		}

	}
	start_reg = 0;
	/*TX GPS Data*/
	if(txgps)
	{
		skfd = gpsc_connect("127.0.0.1"); 
		if(skfd < 0)
		{
			printf("\ngpsd is not running .....\n");
			printf("BUILD GPS PACKET FAILED.....\n");
		}
		else
			printf("[LOGXMITGPS: Transmitting WSMP-GPS...]\n");
	memset(&ntsleep, 0, sizeof(ntsleep));
	memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));

		while(1) {
			gettimeofday(&tv1,NULL);
			if (build_gps_wsmpacket(skfd,&pktreq , &wsmgps, TX_GPS) < 0)  {
				continue;
			}
			IPD = get_gps_txdelay() * 1000000;
			res = txWSMPacket(pid, &pktreq);
			
			if(res < 0){
				drops++;
			} else {
				packets++;
			}
			printf("Transmitted #%llu#  			  Dropped #%llu#\n",packets,drops);
	
#ifdef WIN32
			Sleep(get_gps_txdelay());
#else
			gettimeofday(&tv2,NULL);
			ntsleep.tv_sec = tv2.tv_sec - tv1.tv_sec;
			ntsleep.tv_nsec = 1000 * (((tv2.tv_sec * 1000000) + tv2.tv_usec) - ((tv1.tv_sec * 1000000) + tv1.tv_usec));
			ntsleep.tv_nsec = IPD - (ntsleep.tv_nsec);

			while(ntsleep.tv_nsec != 0) {
				nanosleep(&ntsleep, &ntleft_sleep);
				ntsleep = ntleft_sleep;
				if(ntleft_sleep.tv_nsec == 0) break;
			}
#endif
		}
	} 
}

void receiveWSMIndication(WSMIndication *wsmindication) 
{
	WSMIndication rxwsm_local;
	WSMRequest pktreq_local;
	additionalWSMP addwsmp_local;
	GPSData wsmgps_local;
	int ret = 0;
	int len = 0;
	//int i = 0;
	char logbuf[1024];
	//static uint64_t delay = 0, rx_tsf = 0;
	//static double diff_tsf = 0.0, avg_tsf = 0.0, sum_tsf = 0.0;
	static int overflow = 0;//, icount = 0, ignored = 0;

	//printf(" \b");
	memcpy(&wsmrxind, wsmindication, sizeof(WSMIndication));
	if( (wsmrxind.psid == 9 )) {
		ret = -1;
		if(wsmrxind.data.length  > 11) {
			memcpy(&addwsmp.packetnum, wsmrxind.data.contents, 4);
			memcpy(&addwsmp.rssi, wsmrxind.data.contents + 4, 1);
			memcpy(addwsmp.macaddr, wsmrxind.macaddr, 6);
			ret = parseGPSBinData(&rxgpsdata, wsmrxind.data.contents + 11, wsmrxind.data.length - 11);
			len = build_gps_logentry(0, logbuf, &wsmrxind, &addwsmp, &rxgpsdata, get_gps_contents());
			if((len > 0) && !overflow) {
			/*Calculate Propogation delay when both Tx and Rx are gps synced, and TX config file has TSF field*/
								/*
				rx_tsf =  (uint64_t)generatetsfRequest();
				diff_tsf = rx_tsf - rxgpsdata.local_tsf  ;
				if(diff_tsf > 0 && diff_tsf < 50000) {
					icount++;	
					sum_tsf =  sum_tsf + diff_tsf;
					avg_tsf = sum_tsf / icount ;
					if(!(icount % 10))
						printf("Pkts=%5d Ignored=%5d AVG=%6.0llf\n",icount, ignored, avg_tsf);	
				} else {
					ignored++;
				}
				*/
				//printf("tim_tsf=%lf\n", rxgpsdata.time);
				ret = write_logentry(logbuf, len);
				if(!overflow && (ret == FILE_SIZE_EXCEDDED) ) {
					printf("[LOGXMITGPS: FILE SIZE EXCEDDED]\n");
					overflow = 1;
					if(!txgps)
						bye_bye();
				}
				/*WRITE TX SIDE LOGENTRY*/
				if(write_local && txgps) {
					ret = build_gps_wsmpacket(skfd,&pktreq_local, &wsmgps_local, LOCAL_GPS);
					memcpy(&rxwsm_local.data, &pktreq_local.data, sizeof(WSMData) );
					memcpy(&addwsmp_local.packetnum, rxwsm_local.data.contents, 4);
					memcpy(&addwsmp_local.rssi, rxwsm_local.data.contents + 4, 1);
					memcpy(addwsmp_local.macaddr, rxwsm_local.data.contents + 5, 6);
					ret = parseGPSBinData(&wsmgps_local, rxwsm_local.data.contents + 11, rxwsm_local.data.length - 11);
					len = build_gps_logentry(4, logbuf, &rxwsm_local, &addwsmp_local, &wsmgps_local, get_gps_contents());
					if((len > 0) && !overflow) {
						ret = write_logentry(logbuf, len);
							if(!overflow && (ret == FILE_SIZE_EXCEDDED)) {
								printf("[LOGXMITGPS: FILE SIZE EXCEDDED]\n");
								overflow = 1;
							}
					}
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
		//ret = removeProvider(getpid(), &pst);
		ret = removeUser(getpid(), &ust);
		reqpending  = 0;
		//printf("[EXIT:Unregisterd Provider]\n");
		printf("[EXIT:Unregisterd User]\n");
	} else {
		//printf("[EXIT: Error=>No Reply from WAVEDEVICE]\n");
	}

#ifndef WIN32	
	//pthread_kill_other_threads_np();		
#endif
	signal(SIGINT,SIG_DFL);
	if(txgps)
	{
		gpsc_close_sock();
		printf("\n\nPackets Sent = %llu\n",packets);
		printf("Packets Dropped = %llu\n",drops);
		printf("logxmitgps killed by control-c\n");
	}
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


