/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/


#include "wave.h"
#include "os.h"
#include "queue.h"
#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>

struct entry {

	uint64_t totalPackets;
	uint64_t lastSeqNoRcvd;
	uint64_t startPacketNo;
	uint32_t lostPackets;
	uint32_t latePackets;
	uint32_t sec;
	uint32_t usec;
	char src[20];
	TAILQ_ENTRY(entry) si_list;
	LIST_ENTRY(entry) si_hash;
};

struct src_loss_table {

	TAILQ_HEAD(st_user, entry) st_user;
	ATH_LIST_HEAD (st_hash, entry) st_hash[HASHSIZE];
};

void 		print_usage();
uint8_t     hash_src(char *src);
void           list_src(void);
struct entry*  find_src(char *src, uint64_t totalpackets, uint32_t sec, uint32_t usec);
struct entry*  add_src(char *src, uint64_t totalpackets, uint32_t sec, uint32_t usec);

static uint32_t interPacketDelay = 1000000;
static float tolerance = 1.3;
static int chop = 0;
static char file_name[255];
struct src_loss_table srcLossTable;

void
print_usage()
{
	printf("usage:csvparser [ <target csv file name> ] [<chop>] [<inter packet delay >]\n");
	printf("\n\n");
	printf("<target csv filename>  	  :   specifies the CSV to read the output\n");
	printf("<chop>  	              :   Type chop to generate logdata per source MAC in a file with name log<number>_<MAC>.csv\n");
	printf("<inter packet delay>      :   Type the delay thats is equal to x_delay parameter in the tx side's gps-wave.config file.\n");
	printf("Skip this argument for default 1000000 micro secs (1000000 usec = 1 sec). Only used for finding late packets\n");
	printf("Exanple      		      :   csvparser  debug.csv 2000000 \n");
	exit(-1);
}

uint8_t 
hash_src(char *src) 
{
	uint16_t sum = 0;
	uint8_t i;
	for (i = 0;i < MACADDRSIZE && src[i]; i++)  {
		sum = sum + src[i];
	}
	return sum % HASHSIZE;
}

void 
list_src(void)
{
	struct entry* ntry;
	int hash, i = 0;
	char command[300];

	for (hash = 0; hash < HASHSIZE; hash++) {
		LIST_FOREACH(ntry, &srcLossTable.st_hash[hash], si_hash) {
			printf("\nSrc MAC Addr      = %s \n" , ntry->src);
			printf("Start Packets No. = %llu \n"   , ntry->startPacketNo);
			printf("Last Packet No.   = %llu \n", ntry->lastSeqNoRcvd);
			printf("Lost Packets      = %u \n"   ,ntry->lostPackets );
			printf("Late Packets      = %u \n"   , ntry->latePackets);
			printf("Total Packets     = %llu \n\n", ntry->totalPackets);
			if(chop) {
					i++;
#ifdef WIN32
				{
					char windows_filename[100];
					char *index;
					strcpy (windows_filename, ntry->src);
					do {
						index = strchr(windows_filename, ':');
						if (index)
							*index = '-';

					
					} while (index);
					sprintf(command, "findstr \"%s\" %s > log%d_%s.csv", ntry->src, file_name, i, windows_filename);
					printf("%s", command);
				}
#else
					sprintf(command, "grep %s %s > log%d_%s.csv", ntry->src, file_name, i, ntry->src);
#endif
					system(command);
			}
		}
	}
}


struct entry*
find_src(char *src, uint64_t totalpackets, uint32_t sec, uint32_t usec) 
{
	struct entry *ntry;
	uint8_t hash;
	//int ret;
	int diff = 0;
	uint32_t timediff;

	hash_src(src);
	hash = hash_src(src);
	LIST_FOREACH(ntry, &srcLossTable.st_hash[hash], si_hash) {

		if (!(strcmp(ntry->src, src))) {
			ntry->totalPackets ++;
			diff = totalpackets - ntry->lastSeqNoRcvd ; 
			ntry->lastSeqNoRcvd = totalpackets;
			if ( diff > 1 ) {
					ntry->lostPackets += diff ;
			}
			timediff = (sec - ntry->sec) * 1000000 + (usec - ntry->usec);
			if (timediff > (tolerance * interPacketDelay)) 
					ntry->latePackets ++;
			ntry->sec = sec;
			ntry->usec = usec;
			return ntry;
		}
	}
	return NULL;
}

struct entry*
add_src(char *src, uint64_t totalpackets, uint32_t sec, uint32_t usec)
{	
	struct entry *ntry;
	uint8_t hash;
	//int i;

	ntry = (struct entry *) malloc( sizeof( struct entry));
	ntry->lostPackets = 0;
	ntry->latePackets = 0;
	ntry->totalPackets =1;
	ntry->startPacketNo = totalpackets;
	ntry->lastSeqNoRcvd = totalpackets;
	ntry->sec = sec;
	ntry->usec = usec;
	strncpy(ntry->src, src,  MACADDRSIZE + 1);
	hash = hash_src(src);
	TAILQ_INSERT_TAIL(&srcLossTable.st_user, ntry, si_list);
	LIST_INSERT_HEAD(&srcLossTable.st_hash[hash], ntry, si_hash);
	return ntry;
}

int
main(int argc, char* argv []) {

	FILE *fprsr;
	char logbuf[1024];
	//char *logfilename;
	uint32_t sec;
	uint32_t usec;
	uint64_t packetSeqNo;
	char *lastsrc;
	char *token;

	char *logType;
	int acid;
	int i;

	if(argc < 2) {
		print_usage(); 
	}

	strcpy(file_name, argv[1]);
	if(argc > 2) {
		if(!strcasecmp(argv[2], "chop")) {
			chop = 1;
		} else {
			printf("[CSVPARSER:Ignoring invalid option '%s'. Type 'CHOP' if you want to seperate source macs in to different log files.]\n", argv[2]);
		}
     }

	if(argc > 3) {
	interPacketDelay = atoi(argv[3]);
	}	

	if ((fprsr = fopen(file_name, "r")) == NULL) {
		printf("Cannot open %s. \n", file_name);
		exit(1);
	}
        TAILQ_INIT(&srcLossTable.st_user);
	while (1) {
		if ( !fgets ( logbuf, 1024, fprsr ))
			break;
		
		token = (char*)strtok(logbuf, ",");
		logType = token;

		if (!strcmp(logType, "gps_wsmp")|| !strcmp(logType, " gps_wsmp")) {
	
			token = strtok(NULL, ","); 
			acid = atoi(token);
			token = (char*)strtok(NULL, ",");
			logType = token;
			for(i=0; i<5;i++) {
				token = strtok(NULL, ","); 
				acid = atoi(token);
			}
			token = (char*)strtok(NULL, ",");
			logType = token;
			token = strtok(NULL, ",");
			acid = atoi(token);
			token = strtok(NULL, ",");
			sec = atoi(token);
			token = strtok(NULL, ",");
			usec = atoi(token);
			token = (char*)strtok(NULL, ",");
			lastsrc = token;
			token = strtok(NULL, ","); 
			packetSeqNo = atoi(token);
		} else if (!strcmp(logType, "gps_tx_local") || (!strcmp(logType, "gps_ip")) || (!strcmp(logType, "gps_udp")) ||  (!strcmp(logType, "gps_ip_udp")) ) {
			token = strtok(NULL, ","); 
			acid = atoi(token);
	
			token = strtok(NULL, ",");
			sec = atoi(token);

			token = strtok(NULL, ",");
			usec = atoi(token);

			token = (char*)strtok(NULL, ",");
			lastsrc = token;

			token = strtok(NULL, ","); 
			packetSeqNo = atoi(token);

		} else {
		printf("Invalid File format\n");
		exit(0);
		}

		if (find_src(&lastsrc[1], packetSeqNo, sec, usec) == NULL) {
			add_src(&lastsrc[1], packetSeqNo, sec, usec);
		}
		while (strtok(NULL,","));
	}
	list_src();
	fclose(fprsr);
	return 0;
}

