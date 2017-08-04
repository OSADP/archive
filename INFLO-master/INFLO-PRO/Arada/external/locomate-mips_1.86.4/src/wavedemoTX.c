/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <netinet/in.h>
#include <netinet/ip.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <time.h>
#include "wave.h"
#include <getopt.h>

#define SERVERPORT 4950 
#define IPPORT  8756

//static	PSTEntry entry;
//static	WMEApplicationRequest req; 
static	WMEApplicationRequest entry; 
//static	WSMPacket wsmpacket;
//static  IPPacket ippacket;
static	WSMRequest pktreq;
static  GPSData wsmgps;
//static additionalWSMP addwsmp;
//static struct sockaddr_in their_addr;
static struct sockaddr_in6 their_addr6;
static struct sockaddr_in their_addr4;
static int overIP = 0;
int usegps=1;
unsigned short csum (unsigned short *buf, int nwords);
void sig_int(void);
void sig_term(void);
int ipvfour = -1;
char datagram[4096];
extern int build_gps_wsmpacket(int sockfd,WSMRequest *wsmtxreq, GPSData *gpsdata, GPS_PACKET gpspkt);
extern long get_gps_txdelay();
void usage() {
	fprintf(stderr,"\nusage:xmitgpswave [ --gps {ip|udp|wsmp} ] | [ --ipaddr destipaddress ] |\n"); 
	fprintf(stderr,"                   [ -n if_gps_device_not_available]\n\n");
	fprintf(stderr,"                   [ --help print_this_message ]\n\n");
	fprintf(stderr,"                Note: Substitute first charcater of long options for short options.\n\n");
	fprintf(stderr,"                Example: xmitgpswave \n");
	fprintf(stderr,"                Transmit WSMP GPS packets\n\n");
	fprintf(stderr,"                Example: xmitgpswave --gps udp --ipaddr 192.168.0.255\n");
	fprintf(stderr,"                Broadcast UDP GPS packets on th 192.168.0.255 domain\n\n");
	exit(1);
}

void options(int argc, char *argv[])
{
	int index = 0;
	int ret = 0;
	int t;
	static struct option opts[] =
	{ 
		{"help", no_argument, 0, 'h'},
		{"gps", required_argument, 0, 'g'},
		{"ipaddr", required_argument, 0, 'i'},
		{"nogps", no_argument, 0, 'n'},
		{0, 0, 0, 0}
	};

	while(1) {
		t = getopt_long_only(argc, argv, "+hg:i:", opts, &index);
		if(t < 0)
			break;
			
		switch(t) {
			
			case 'h':
				usage();
			break;
	
			case 'g':
				if(!strcasecmp(optarg, "wsmp")) {
					overIP = 0;
				} else if(!strcasecmp(optarg, "udp")) {
					overIP = 1;
				} else if(!strcasecmp(optarg, "ip")) {
					overIP = 2;
				} else {
					usage();
				}
			break;
			
			case 'i':
				ret = inet_aton(optarg, &their_addr4.sin_addr);
				
				if(ret == 1)
					ipvfour = 1;

				/* if(ret == 0) {
					printf("xmitgpswave: Invalid IP address\n");
					usage();
				}*/
				if(ret != 1)
				{
					ret = inet_pton(AF_INET6, optarg, &their_addr6.sin6_addr);
		
					if(ret == 1)
						ipvfour = 0;

					if(ret == 0) {
						printf("xmitgpswave: Invalid IP address\n");
						usage();
					}
				}
			break;
			case 'n':
				usegps=0;
				break;
			default:	
				usage();
		}	
	}

}

int main(int argc, char *argv[])
{
	int sockfd;//, io;
	int broadcast = 1;
	int numbytes;
	int pid,skfd;
	int one = 1; 
//	int maccount;
	int ret = 0;
	const int *val = &one;
//	char macaddr[100];
//	char data_string[33];
//	static unsigned int packetcount = 0;
//	struct hostent *he;
	struct ip *iph = (struct ip *)& datagram;
	static struct timeval tv1, tv2;
	static struct timespec ntsleep, ntleft_sleep;
	static long int IPD = 100000000; //Nano-Seconds
	FILE *file=NULL;

	overIP = 0;
	options(argc, argv);
	
	//file = fopen("/proc/wsmp_tx_start", "w");
//	if(!file) {
//		printf("[XMITGPSWAVE: Unable to open proc entry]\n");
//	}
	switch(overIP) {
		case 0:
				
		break;
				
		case 1:

			ret = inet_pton( AF_INET, argv[4], &their_addr4.sin_addr );

			if(ret == 1)
			{

				if ((sockfd = socket(AF_INET, SOCK_DGRAM, 0)) == -1) {
					perror("socket");
					exit(-1);
				}

				/* Broadcasting not for now! */

				/*if (setsockopt(sockfd, SOL_SOCKET, SO_BROADCAST, &broadcast,
					sizeof(broadcast)) == -1) {
					perror("setsockopt (SO_BROADCAST)");
					exit(-1);
				} */
				
				their_addr4.sin_family = AF_INET;
				their_addr4.sin_port = htons(SERVERPORT);
				//memset(&(their_addr.sin_zero), '\0', 8);
				ipvfour = 1;
			}
			else
			{
				inet_pton(AF_INET6, argv[4], &their_addr6.sin6_addr );

				 if ((sockfd = socket(AF_INET6, SOCK_DGRAM, 0)) == -1) {
					perror("socket");
					exit(-1);
				} 

				/*if (setsockopt(sockfd, SOL_SOCKET, SO_REUSEADDR, (char *)&broadcast,
				sizeof(broadcast)) == -1) 
				{
					perror("setsockopt (SO_BROADCAST)");
					exit(-1);
				} */

				their_addr6.sin6_family = AF_INET6;
				their_addr6.sin6_port = htons(SERVERPORT);
				//memset(&(their_addr.sin_zero), '\0', 8);
				ipvfour = 0;

			}

		break;
		
		case 2:
			if(ipvfour == 1)
			{
				if ((sockfd = socket(PF_INET, SOCK_RAW, IPPROTO_RAW)) == -1) {
					perror("socket");
					exit(-1);
				}
				if (setsockopt(sockfd, IPPROTO_IP, IP_HDRINCL, val,
					sizeof(one)) == -1) {
					perror("setsockopt (HDRINCL)\n");
					exit(-1);
				}
				if (setsockopt(sockfd, SOL_IP, IP_TTL, val,
					sizeof(one)) == -1) {
					perror("setsockopt (HDRINCL)\n");
					exit(-1);
				}
				if (setsockopt(sockfd, SOL_SOCKET, SO_BROADCAST, &broadcast,
					sizeof(broadcast)) == -1) {
					perror("setsockopt (SO_BROADCAST)");
					exit(-1);
				}
				their_addr4.sin_family = AF_INET;
				their_addr4.sin_port = htons(IPPORT);
				memset(&(their_addr4.sin_zero), '\0', 8);
				memset(datagram, 0, 4096);
				iph->ip_hl = 5;
				iph->ip_v = 4;
				iph->ip_tos = 0;
				iph->ip_len = sizeof (struct ip) + sizeof (IPPacket);
				iph->ip_id = htonl (54321);
				iph->ip_off = 0;
				iph->ip_ttl = 255;
				iph->ip_p = 0xff; //  WAVE specific
				iph->ip_sum = 0;
				iph->ip_src.s_addr = INADDR_ANY;
				iph->ip_dst.s_addr = their_addr4.sin_addr.s_addr;
			}
			else if(ipvfour == 0)
			{
				/* printf("%s: Coming Here to ipv6 of overIP == 2\n", __func__);
				if ((sockfd = socket(PF_INET6, SOCK_RAW, IPPROTO_RAW)) == -1) {
					perror("socket");
					exit(-1);
				}
				if (setsockopt(sockfd, IPPROTO_IPV6, IPV6_HDRINCL, val,
					sizeof(one)) == -1) {
					perror("setsockopt (HDRINCL)\n");
					exit(-1);
				}
				if (setsockopt(sockfd, SOL_IPV6, IPV6_TTL, val,
					sizeof(one)) == -1) {
					perror("setsockopt (HDRINCL)\n");
					exit(-1);
				}
				 if (setsockopt(sockfd, SOL_SOCKET, SO_BROADCAST, &broadcast,
					sizeof(broadcast)) == -1) {
					perror("setsockopt (SO_BROADCAST)");
					exit(-1);
				} 
				their_addr6.sin6_family = AF_INET6;
				their_addr6.sin6_port = htons(IPPORT);
				//memset(&(their_addr6.sin_zero), '\0', 8);
				memset(datagram, 0, 4096);
				iph->ip_hl = 7;
				iph->ip_v = 4;
				iph->ip_tos = 0;
				iph->ip_len = sizeof (struct ip) + sizeof (IPPacket);
				iph->ip_id = htonl (54321);
				iph->ip_off = 0;
				iph->ip_ttl = 255;
				iph->ip_p = 0xff; //  WAVE specific
				iph->ip_sum = 0;
				//iph->ip6_src.sin6_addr = in6addr_any;
				iph->ip6_src = in6addr_any;
				//iph->ip6_dst.sin6_addr = their_addr6.sin6_addr.s6_addr;
				iph->ip6_dst = their_addr6.sin6_addr; */
			}
	
		break;
		
		default:
			printf("xmitgpswave: Unexpected mode error\n");
			usage();
	}
	
	pid = getpid();
	entry.psid = 9;
    entry.userreqtype = USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL;
    entry.channel = 178;
    entry.schaccess  = 0;
    entry.schextaccess = 0;

	pktreq.version = 7;
	pktreq.security = 8;
	pktreq.chaninfo.channel = 178;
	pktreq.chaninfo.rate = 3;
	pktreq.chaninfo.txpower = 15;
	pktreq.psid = 9;

	memset(&pktreq.data, 0, sizeof(WSMData) );

	//sprintf(entry.acm.contents,"gps");
	//entry.acm.length = strlen(entry.acm.contents);
	//strncpy(pktreq.acm.contents, entry.acm.contents, OCTET_MAX_LENGTH);

	//entry.contents |= WAVE_ACID;
	//entry.contents |= WAVE_ACM;
	//pktreq.acm.length = entry.acm.length;
	printf("Invoking  WAVE driver\n");
	
	if(invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0) {
		printf("Open failed. Quitting\n");
		exit(-1);
	}
	if(overIP == 0) {
		printf("Remove USER if already present(%d)\n", !removeUser(pid, &entry));
		printf("Registering as a USER [%d] PSID =%u \n",registerUser(pid, &entry),entry.psid );
	}

	if(overIP)
		set_gpsmode_notWSMP(); /*wavegps call*/

	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);
	memset(&ntsleep, 0, sizeof(ntsleep));
	memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
	if(usegps)
	{
		skfd = gpsc_connect("127.0.0.1");
		if(skfd < 0)
		{
			printf("gpsc is not running ......\n");
			printf("Build GPS Packet fail\n");
			exit(1);	
		}
	}
	while(1) {
		gettimeofday(&tv1, NULL);
		IPD = get_gps_txdelay() * 1000000;
		if(usegps)
		{
		if (build_gps_wsmpacket(skfd,&pktreq , &wsmgps, TX_GPS) < 0) {
			   printf("Build GPS Packet fail\n");
			   continue;
		}	
		}
		if(0 && file) {
			fprintf(file, "%ld", tv1.tv_usec);
			fflush(file);
		}
		if (overIP == 0) {
			pktreq.psid = 9;
			if (txWSMPacket(pid, &pktreq)< 0) {
				printf("Tx GPS WSMP fail\n");
			}

			gettimeofday(&tv2, NULL);
		} else if (overIP == 1) {
			memcpy(datagram, pktreq.data.contents, pktreq.data.length);

			if(ipvfour == 1)
			{
			if ((numbytes = sendto(sockfd,
					 	datagram,
						pktreq.data.length,
						0,
						(struct sockaddr *)&their_addr4,
			 			sizeof(struct sockaddr_in))) == -1) {
				perror("sendto");
				exit(-1);
				}
			}
			else if(ipvfour == 0)
			{
			if ((numbytes = sendto(sockfd,
					 	datagram,
						pktreq.data.length,
						0,
						(struct sockaddr *)&their_addr6,
			 			sizeof(struct sockaddr_in6))) == -1) {
				perror("sendto");
				exit(-1);
				}
			}
			
			gettimeofday(&tv2, NULL);
		} else if (overIP == 2) {
			memcpy(datagram + 20, pktreq.data.contents, pktreq.data.length);
			iph->ip_sum = 0;
			iph->ip_len = sizeof(struct ip) + pktreq.data.length; 
			iph->ip_sum = csum((unsigned short *) datagram, iph->ip_len >> 1);

			if(ipvfour == 1)
			{
				if ((numbytes = sendto(sockfd,
					 	datagram,
						iph->ip_len,
						0,
						(struct sockaddr *)&their_addr4,
		 				sizeof(struct sockaddr))) == -1) {
					perror("sendto");
					exit(-1);
				}
			}
			else if(ipvfour == 0)
			{

				if ((numbytes = sendto(sockfd,
					 	datagram,
						iph->ip_len,
						0,
						(struct sockaddr *)&their_addr6,
		 				sizeof(struct sockaddr))) == -1) {
					perror("sendto");
					exit(-1);
				}
			}
			gettimeofday(&tv2, NULL);
		}
		ntsleep.tv_sec = tv2.tv_sec - tv1.tv_sec;
		ntsleep.tv_nsec = 1000 * (((tv2.tv_sec * 1000000) + tv2.tv_usec) - ((tv1.tv_sec * 1000000) + tv1.tv_usec));
		ntsleep.tv_nsec = IPD - (ntsleep.tv_nsec);
		while(ntsleep.tv_nsec != 0) {
     			nanosleep(&ntsleep, &ntleft_sleep);	
			ntsleep = ntleft_sleep;
			if(ntleft_sleep.tv_nsec == 0) break;
 		}
	}
	if (overIP >= 1) {
		close(sockfd);
	}
	return 0;
}

unsigned short csum (unsigned short *buf, int nwords)
{
  unsigned long sum;
  for (sum = 0; nwords > 0; nwords--)
    sum += *buf++;
  sum = (sum >> 16) + (sum & 0xffff);
  sum += (sum >> 16);
  return ~sum;
}

void sig_int(void)
{
	int ret;

	ret = removeAll();
	gpsc_close_sock();
	printf("[EXIT]\n");
	signal(SIGINT,SIG_DFL);
	exit(0);

}

void sig_term(void)
{
	int ret;

	gpsc_close_sock();
	ret = removeAll();
	printf("[EXIT]\n");
	signal(SIGINT,SIG_DFL);
	exit(0);

}

