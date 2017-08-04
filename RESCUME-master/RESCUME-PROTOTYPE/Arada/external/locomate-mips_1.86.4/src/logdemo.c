/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#include <stdio.h>
#include <ctype.h>
#include <termio.h>
#include <arpa/inet.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <linux/errno.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <netinet/in.h>
#include <netinet/ip.h>
#include <netinet/in_systm.h>
#include <netinet/if_ether.h>
#ifdef SDK_NEW
#include <linux/if.h>
#endif
#include <linux/wireless.h>
#include "wavelogger.h"
#include "wavegps.h"

static uint16_t port  =  9876; /*NOTE*/ 
static struct sockaddr_in ip;
static uint8_t mode = 1;
static char filename[255];
static uint64_t interPacketDelay;

void print_usage()
{
	printf("Usage: logdemo mode ipaddr port [logfile] [delay]\n");
	exit(-1);
}


int main(int argc, char* argv[])
{
	int ret = 0;
	if(argc < 4)
		print_usage();

	
	mode = atoi(argv[1]);
	if( (mode != 1) && (mode != 2)) {
		printf("Invalid MODE, valid values = 1 for UDP or 2 for IP \n");
		print_usage();
	}
	ret = inet_aton(argv[2], &ip.sin_addr);
	
	if(ret == 0) {
		printf("Invalid IP address\n");
		print_usage();
	}
	
	port = atoi(argv[3]);
	
	if (port < 1025) {
		printf("Can't use reserved PORT, valide values > 1024\n");
		print_usage();
	}
	
	if(argc > 4) {
		sprintf(filename, "%s", argv[4]);
	} else {
		sprintf(filename, "%s", (mode == 2)? "/etc/ip_gps.log" : "/etc/udp_gps.log");
	}
	if(argc > 5) {
		interPacketDelay = atoi(argv[5]);
	} else {
		interPacketDelay = 1000000;
	}

		
	/*LOGGING STARTS HERE*/
	set_logging_mode(mode);
	set_logging_addr(ip, port);
	set_logging_format(1);
	set_logfile(filename);
	printf("Listening at %s:%u, Logging to:%s\n", inet_ntoa(ip.sin_addr), port, filename);
	printf("Press Ctrl-C to Exit\n");
	set_packet_delay(interPacketDelay); /*Call it before open_log*/
	open_log(0);
	start_logging();
	while(1);		
}
