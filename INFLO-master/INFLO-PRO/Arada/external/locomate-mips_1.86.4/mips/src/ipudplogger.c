/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <errno.h>
#include <string.h>
#include <fcntl.h>
#include <math.h>

#include "wavelogger.h"
#include "wave.h"
#include "os.h"

#ifdef WIN32
#define WIN_LOG_PATH	sprintf(filename, "%s", (mode == 2)? ".\\ip_gps.log" : ".\\udp_gps.log");
#define LIN_LOG_PATH
#define WIN_INET_ATON()	ret=ip.sin_addr.s_addr = inet_addr(argv[2]);
#define LIN_INET_ATON()
#else
#define LIN_LOG_PATH	sprintf(filename, "%s", (mode == 2)? "/etc/ip_gps.log" : "/etc/udp_gps.log");
#define WIN_LOG_PATH

#define WIN_INET_ATON()
#define LIN_INET_ATON()	ret = inet_aton(AF_INET, argv[2], &ip);
#endif



#define XML 1
#define CSV 2

static uint16_t port  =  9876; /*NOTE*/ 
static struct sockaddr_in ip;
static uint8_t mode = 1 ;
static char filename[255];
static uint64_t interPacketDelay;
extern void list_src(void);

int Inet_Aton(char *x, struct in_addr * aton)
{
#ifdef WIN32
        aton->s_addr=inet_addr(x);
	return aton->s_addr;
#else
       return  inet_aton(x, (struct in_addr *)aton);
#endif
}

void print_usage()
{
	printf("usage:ipudplogger <mode> <host ipaddr> <host port> [ <logfile> ] [<logformat>] [<inter packet delay>]\n");
	printf("\n\n");
	printf("<mode>          	:    Selects IP or UDP\n");
	printf("                	     udp - for UDP-GPS packet receive\n");
	printf("                  	     ip - for IP-GPS packet receive\n");
	printf("<host ipaddr>   	:    local machine ip address. This option can be\n");
	printf("               	   	     used when multiple domains are available in the local machine\n");
	printf("                	     In general use the ip of the ethernet port thats connected to the host. \n");
	printf("<host port>     	:    local port to receive on (Default:9876) \n");
	printf("<logfilename>   	:    Output log filname\n");
	printf("                   	     Default values\n");
	printf("           	             /etc/udp_gps.log  - When mode udp is selected\n");
	printf("                	     /etc/ip_gps.log  - When mode ip is selected\n");
	printf("<logformat>    		 :   Type xml or csv for logging in XML or CSV format. Skip this argument for default format.\n");
	printf("Example        		 :   ipudplogger ip 192.168.1.54 9876 datalog csv\n");
	printf("              		 :   For logging from the forwarding machine 192.168.1.54 on port 9876 to file datalog in CSV format\n");
	printf("<inter packet delay>	 :   Type the delay thats is equal to x_delay parameter in the tx side's gps-wave.config file.\n");
	printf("			     Skip this argument for default 1000000 micro secs (1000000 usecs = 1 sec). Only used for finding late packets\n");
	printf("Example       		 :   ipudplogger ip 192.168.1.54 9876 datalog csv 1000000\n");
	printf("Note: Supply the arguments in the given order, and none of the INTERMEDIATE arguments can be skipped \n");
		exit(-1);
}

void sig_int(void)
{
	//int ret=0;

	printf("[EXIT]\n");
	/*MUST for XML files*/
	stop_logging();
	close_log();
	signal(SIGINT,SIG_DFL);
	printf("Packets profiles from all sources\n");
	list_src();
	exit(0);

}

void sig_term(void)
{
	//int ret=0;

	printf("[EXIT]\n");
	/*MUST for XML files*/
	stop_logging();
	close_log();
	signal(SIGINT,SIG_DFL);
	printf("Packets profiles from all sources\n");
	list_src();
	exit(0);

}

int main(int argc, char* argv[])
{
	int ret = 0;
	//char str[INET6_ADDRSTRLEN]; 
	uint8_t logformat = 0;
	
	port = 9876;
	
	if(argc < 3)
		print_usage();

	if (strcasecmp(argv[1],"udp") == 0 ) {
		mode = 1;
	}
	else if(strcasecmp(argv[1],"ip") == 0) {
		mode = 2;
	}
	else {
		printf("[IPUDPLOGGER: Error==>Invalid MODE, valid values = 1 for UDP or 2 for IP]\n");
		print_usage();
	}
	//mode = atoi(argv[1]);
	if( (mode != 1) && (mode != 2)) {
		printf("[IPUDPLOGGER: Error==>Invalid MODE, valid values = 1 for UDP or 2 for IP]\n");
		print_usage();
	}
	
	//WIN_INET_ATON()
	ret = Inet_Aton(argv[2],&ip.sin_addr);

	if (ret == 0) {
		printf("[IPUDPLOGGER: Error==>Invalid IP ADDRESS]\n");
		print_usage();
	}
	
	if (argc > 3)
	port = atoi(argv[3]);
	
	if (port < 1025) {
		printf("[IPUDPLOGGER: Error==>Invalid PORT, Valid values > 1024]\n");
		print_usage();
	}
	
	if (argc > 4) {
		sprintf(filename, "%s", argv[4]);
	} else {
	
			WIN_LOG_PATH

			LIN_LOG_PATH
	}

	if (argc > 5) {
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
	set_logging_mode(mode);
	set_logging_addr(ip, port);
	
	/*Set the format before opening the log, Explicitly call open_log and close_log to begin and end logging*/
	set_logging_format(logformat);
	
	set_logfile(filename);
	set_packet_delay(interPacketDelay); /*Call it before open_log*/
	open_log(0);
	printf("[IPUDPLOGGER: Listening at %s:%u, Logging to:%s]\n", inet_ntoa(ip.sin_addr), port, get_logfile());
	printf("[IPUDPLOGGER: Press Ctrl-C to Exit]\n");
	/*Starts the thread that listens for forwarded  GPS data*/
	start_logging();
	while(1);		
}
