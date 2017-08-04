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
#include <math.h>

#define A_LATE 0.00668
#define A_LOSS 0.03791
#define COMMON_RATIO 1.5

void 		print_usage();

void r_metric(uint64_t seq);
void confidence_analysis (uint64_t , double );
void covarience_analysis (uint64_t , double );

//static uint32_t interPacketDelay = 1000000;
//static float tolerance = 1.3;
static	char logfilename[255];
static	uint64_t latedata[200][3]; /*Rows=Packets Col0 = Sequence, Col1= PacketNumber, Col2=DelayJitter*/
static  uint64_t late_quart[5]; /*late_quart[i]: i = 0, Packets within 0-25% of max_tolerance, i = 1,25-50%  i=2, 50-75%, i=3,75-100%, i=4>100%*/ 
static  uint64_t lostdata[20]; /*lostdata[i] gives number of 'i' lost packets*/  
//static int chop = 0;
static float Plate[9], Ploss[4], reliability = 0.0, confidence_upper = 0.0, confidence_lower = 0.0;//, cam = 0.0;
static float delay[9999];

	void
print_usage()
{
	printf("usage:jitter [ <target csv file name> ] [<tolerance >] \n");
	printf("\n\n");
	printf("<target csv filenaem>  	  :   specifies the CSV to read the output\n");
	printf("<tolerance>      :   Type maximum %%tolerance of delay\n");
	printf("Skip this argument for default 1000000 micro secs (1000000 usec = 1 sec). Only used for finding late packets\n");
	printf("Exanple:delay_jitter  source_1_XX:XX:XX:XX:XX:XX.csv 2000000 \n");
	exit(-1);
}

void plot_late(int num_points, uint64_t min, uint64_t max, uint64_t late[][3])
{
	int max_chars = 100;
	char dot = '=';
	int i, j;
	uint64_t scale =  (uint64_t)(0.0f + (float)((max - min)/max_chars)) ;

	printf("[SeqNo.] [PacketNo.]                          [1 '%c' is %4lluus Max is %6lluus]                                            [delay_jitter]\n", 
			dot, scale, max);
	for(i = 0; i < num_points; i++) {
		printf("(%5llu)   (%5llu)  |", late[i][0], late[i][1]);
		for(j = 0; j <= max_chars; j++) {
			if(j <= ( (late[i][2] - min)/ scale))
				putchar(dot);
			else
				putchar(' ');
		}
		printf("|  (%6lluus)\n",late[i][2]);
	}

}

int
main(int argc, char* argv []) {

	FILE *fp;
	char logbuf[1024];
	uint32_t sec;
	uint32_t usec;
	uint64_t startseq = 0,  starttime = 0, last = 0, now = 0;//eta
	uint64_t delta = 0, tolerance = 0, delay_jitter = 0, min = 1111111111, max = 0;
	double avg_delay_jitter = 0.0;
	uint64_t packetSeqNo = 0, seq = 0, late = 0, nextpkt = 0;
	char *lastsrc;
	char *token;
	char *logType;
	int acid;
	int i = 0, bucket = 0;
	float t = 0.1f, temp;
#define USECS (uint64_t)1000000


	if(argc < 2) {
		print_usage(); 
	}

	if(argc > 2) {
		t = (atof(argv[2]) * 0.01f);
	}	
	if ((fp = fopen(argv[1], "r")) == NULL) {
		printf("Cannot open %s. \n", argv[1]);
		exit(1);
	}
	strcpy(logfilename, argv[1]);
	memset(latedata, 0 , 200 * sizeof(uint64_t));
	memset(late_quart, 0 , 5 * sizeof(uint64_t));
	memset(lostdata, 0 , 20 * sizeof(uint64_t));
	while (1) {
		if ( !fgets ( logbuf, 1024, fp ))
			break;
		token = (char*)strtok(logbuf, ",");
		logType = token;
		if (!strcmp(logType, " gps_wsmp") || !strcmp(logType, " gps_wsmp")) {
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
		} else if ((!strcmp(logType, "gps_tx_local")) ||
				(!strcmp(logType, "gps_ip")) || 
				(!strcmp(logType, "gps_udp")) ||  
				(!strcmp(logType, "gps_ip_udp")) ) {
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

		} 
		seq++;
		now = (uint64_t)(sec * USECS) + usec;
		if(seq < 3) {
			if(seq == 1) {
				nextpkt = packetSeqNo;
				startseq = packetSeqNo;
				starttime = now;
				printf("First packet Seq = %llu\n", packetSeqNo);
			}
			if (seq == 2) {
				delta = now - last;
				tolerance = (uint64_t)(t * (float)delta) ;
				min = delta;
				/*2nd Packet is put in late packet list so that we get an idea of our maximum tolerable ETA*/
				latedata[late][0] = 0;
				latedata[late][1] = packetSeqNo;
				latedata[late][2] = delta + tolerance;
				late++;
			}
		} else {
			/*DELAY JITTER*/
			delay_jitter = (now - last);
			delay[seq] = delay_jitter; // for now this delay is saved in an array. Later change to it to a file read one more time 
			if(delay_jitter <= min)
				min = delay_jitter;
			if(delay_jitter >= max)
				max = delay_jitter;

			avg_delay_jitter = avg_delay_jitter + delay_jitter;	
			if( delay_jitter > (delta + tolerance)) {
				if(late < 200) {
					latedata[late][0] = seq;	
					latedata[late][1] = packetSeqNo;
					latedata[late][2] = delay_jitter;
				}	
				late++ ;
				//printf("Late: Seq=%llu PacketNo=%llu delay_jitter=%llu\n", latedata[late][0], latedata[late][1], latedata[late][2], delta);

			} 
			/*Which quarter does the late packet falls into? 0-25%, 25-50%, 50-75% or 75-100% of Delta*/
			if(delay_jitter > delta) {
				temp = 100.0f * ((delay_jitter - delta) / (float)delta) ;
				bucket = (int)(temp / 4.0f);
				if(bucket > 4)
					bucket = 4;
				//printf("Packet=%llu:Jitter=%llu delay=%llu(%f,%d)\n",packetSeqNo, delay_jitter, delta, temp,bucket);
				late_quart[bucket]++ ;		
			}

		}
		/*ETA*/
		//eta = starttime + (packetSeqNo - startseq) * (seq - 1) * (150000); /*Can take a fixed interval other than delta too*/
		//if((now - (eta + tolerance))>0)
		//printf("Seq=%llu Pkt=%llu ETA=%llu NOW=%llu\n", seq, packetSeqNo, eta, now);

		/*LOST PACKETS*/
		if( ((packetSeqNo - nextpkt))< 20) {
			lostdata[(packetSeqNo - nextpkt)]++ ;
			//printf("%llu %d Consecutive losses\n", lostdata[(packetSeqNo - nextpkt)], packetSeqNo - nextpkt);
		}
		nextpkt = packetSeqNo + 1;

		last = now;
		while (strtok(NULL,","));
	}
	if(seq > 2)
		avg_delay_jitter = avg_delay_jitter /(double) (seq - 2) ;
	printf("Transmitter=%s\n",lastsrc);	
	printf("\nDelta=%lluus Tolerance=%lluus(%4.1f%%) ActualLatePkts=%llu of %llu Packets \ndelay_jitter[Avg=%fus, Min=%lluus, Max=%lluus] \n\n", 
			delta, tolerance, t * 100.0, late - 1, seq, avg_delay_jitter, min, max);
	plot_late((late > 200) ? 200 : late , min, max, latedata);

	printf("\n\n******LATE PACKETS DISTRIBUTION w.r.t Delta*****\n");
	printf(" 0-25%%  ==> %llu\n", late_quart[0]);
	printf("25-50%%  ==> %llu\n", late_quart[1]);
	printf("50-75%%  ==> %llu\n", late_quart[2]);
	printf("75-100%% ==> %llu\n", late_quart[3]);
	printf("100%%+   ==> %llu\n", late_quart[4]);


	printf("\n\n**********LOST PACKETS********** \n");
	for(i = 1; i < 20; i++)
		if(lostdata[i]) 
			printf("%d Consecutive lost packets==>%llu\n", i, lostdata[i]);
	r_metric(seq);
	confidence_analysis(seq, avg_delay_jitter);
	//covarience_analysis(seq, avg_delay_jitter);
	return 0;
}


void r_metric(uint64_t seq) {
	int i;
	uint64_t temp = 0;
	float tempLoss = 0.0, tempLate =0.0, tempTotal = 0.0;

		Plate[0] = (float)late_quart[0]/seq;
		Plate[1] = (float)late_quart[1]/seq;
		Plate[2] = (float)late_quart[2]/seq;
		Plate[3] = (float)(late_quart[3] + (late_quart[4]))/seq;
	
		for (i = 1; i<=7; i++) {
			Ploss[i] = (float) lostdata[i]/seq;
		}
		for (i = 8; i<20; i++) {
			temp += lostdata[i];
		}	
		Ploss[8] = (float)temp/seq;
	
		printf("\n\n**********Late PACKETS PROBABILITIES********** \n");
		printf("plate[0] %f\n", Plate[0]);
		printf("plate[1] %f\n", Plate[1]);
		printf("plate[2] %f\n", Plate[2]);
		printf("plate[3] %f\n", Plate[3]);
		printf("\n\n**********LOST PACKETS PROBABILITIES********** \n");

		for(i = 1; i<=8; i++) {
			printf("Ploss[%d] %f\n", i, Ploss[i]);
			tempLoss += A_LOSS * pow(COMMON_RATIO, (i-1));
		}
		
		for(i =0; i<4; i++) {
			tempLate += A_LATE * pow(COMMON_RATIO, (i));
		}
		tempTotal = tempLate + tempLoss;
		reliability = 1 - tempTotal;
		printf("Reliability Metric considering both late and lost packets = %f\n", -reliability);
		printf("Reliability Metric considering late packets only = %f\n",   (1 - tempLate)); // bcoz of the half weightage
		printf("Reliability Metric considering lost packets only = %f\n",   -(1 - tempLoss)); // bcoz of the half weightage
}	


void confidence_analysis (uint64_t seq, double avg_delay_jitter) {

	//float tow, sigma, temp;
	double varience = 0;
	uint64_t i;
	for (i=1; i<=seq; i++) {
		varience += pow((delay[seq] - avg_delay_jitter), 2);
		//printf("Varience %f %llu\n", varience, seq);		
	}
	confidence_lower = avg_delay_jitter -  1.96 * (varience/sqrt(seq));
	confidence_upper = avg_delay_jitter +  1.96 * (varience/sqrt(seq));
	printf("Lower Confidence Limit at 95%% %f \n", confidence_lower);
	printf("Upper Confidence Limit at 95 %f \n", confidence_upper);
	printf("Confidence Interval at 95%% %f\n", (confidence_upper - confidence_lower));
	
}

void
covarience_analysis(uint64_t seq, double avg_delay_jitter) {

}
