/*

 * Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

 * Proprietary and Confidential Material.

 *

 */


#include "wave.h"
#include "wavegps.h"
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

#define A_LATE 0.0615
#define A_LOSS 0.0101
#define COMMON_RATIO 1.5
#define MILLION 1000000

void 		print_usage();

void r_metric(int64_t seq);
void confidence_analysis (int64_t , double );
void covarience_analysis (int64_t , double );

static uint32_t interPacketDelay = 1000000;
static float tolerance = 1.3;
static	char logfilename[255];
static	int64_t latedata[9999][3]; /*Rows=Packets Col0 = Sequence, Col1= PacketNumber, Col2=DelayJitter*/
static  int64_t late_quart[5]; /*late_quart[i]: i = 0, Packets within 0-25% of max_tolerance, i = 1,25-50%  i=2, 50-75%, i=3,75-100%, i=4>100%*/ 
static  int64_t lostdata[20]; /*lostdata[i] gives number of 'i' lost packets*/  
static int chop = 0;
static float packet_late[9], packet_loss[4], reliability = 0.0, confidence_upper = 0.0, confidence_lower = 0.0 ;

        void
print_usage()
{
        printf("usage:pjitter [ <target csv file name> ] [<tolerance >] \n");
        printf("\n\n");
        printf("<target csv filenaem>  	  :   specifies the CSV to read the output\n");
        printf("<tolerance>      :   Type maximum %tolerance of delay\n");
        printf("Skip this argument for default 1000000 micro secs (1000000 usec = 1 sec). Only used for finding late packets\n");
        printf("Exanple:pjitter  log1_XX:XX:XX:XX:XX:XX.csv 2000000 \n");
        exit(-1);
}

void plot_late(int num_points, int64_t min, int64_t max, int64_t late[][3])
{
        int max_chars = 100;
        char dot = '=';
        int i, j;
        int64_t scale =  (int64_t)(1.0f + (float)((max - min)/max_chars)) ;

        printf("[SeqNo.] [PacketNo.]                          [1 '%c' is %4lldus Max is %6lldus]                                            [delay_jitter]\n", 
                        dot, scale, max);

        for(i = 0; i < num_points; i++) {
                printf("(%5lld)   (%5lld)  |", late[i][0], late[i][1]);
                for(j = 0; j <= max_chars; j++) {
                        if(j <= ( (late[i][2] - min) / scale)) {
                                putchar(dot);
                        } else {
                                putchar(' ');
                        }
                }
                printf("|  (%6lld us)\n",late[i][2]);

        }
}

int
main(int argc, char* argv []) {

        FILE *fp;
        char logbuf[1024];
        uint32_t sec;
        uint32_t usec;
        int rssi;
        double gps_time;
        uint32_t gps_usecs;
        int64_t startseq = 0,  starttime = 0, eta = 0,  last = 0, now = 0;
        int64_t delta = 0, tolerance = 10000, delay_jitter = 0, min = 1111111111, max = 0;
        double avg_delay_jitter = 0.0 , avg_tsf_ipd = 0.0, sum_tsf = 0.0, min_tsf = 11111111, max_tsf = -1111111;
        int64_t packetSeqNo = 0, seq = 0, late = 0, nextpkt = 0;
        char *lastsrc;
        char *token;
        char *logType;
        int acid;
        int i = 0, bucket = 0, temp32 = 0, icount = 0;
        float temp;

#define USECS (int64_t)1000000


        if(argc < 2) {
                print_usage(); 
        }

        if(argc > 2) {
                tolerance = atoi(argv[2]) ;
                if ((tolerance > 10000) || (tolerance < 0)) {
                        printf("Tolerance should be with in 0 & 10000 usec\n");
                }
                delta = tolerance * 10;
                delta = 0;
        }	
        if ((fp = fopen(argv[1], "r")) == NULL) {
                printf("Cannot open %s. \n", argv[1]);
                exit(1);
        }
        strcpy(logfilename, argv[1]);
        memset(latedata, 0 , 200 * sizeof(int64_t));
        memset(late_quart, 0 , 5 * sizeof(int64_t));
        memset(lostdata, 0 , 20 * sizeof(int64_t));
        while (1) {
                if ( !fgets ( logbuf, 1024, fp ))
                        break;
                token = (char*)strtok(logbuf, ",");
                logType = token;
                if (!strcmp(logType, "gps_wsmp") || !strcmp(logType, " gps_wsmp")) {
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
                        token = strtok(NULL, ",");
                        rssi = atoi(token);
                        /*TIM_TSF*/
                        token = strtok(NULL, ",");
                        gps_time = strtod(token, NULL);
                        gps_usecs = (gps_time - (int)gps_time) * MILLION;

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
                //now = (int64_t)(sec * USECS) + usec;
                now = (int64_t)(sec * USECS) + usec;
                last = (int64_t)(gps_time * USECS);
                //printf("Siva %llu now %llu last\n", now, last);
                printf("\n");	
                /*TIM_TSF: Cacluclate avg IPD based on TSF*/
                if(usec > gps_usecs) {
                        icount++;
                        temp32 = usec - gps_usecs;
                        sum_tsf = sum_tsf + temp32;
                        avg_tsf_ipd = sum_tsf / icount;
                        if(avg_tsf_ipd > max_tsf) 
                                max_tsf = avg_tsf_ipd;
                        else if(avg_tsf_ipd < min_tsf)
                                min_tsf = avg_tsf_ipd;
                        //printf("Siva at main %d\n", avg_tsf_ipd);
                }
                //if(seq < 3) {
                if(0) {
                        if(seq == 1) {
                                nextpkt = packetSeqNo;
                                startseq = packetSeqNo;
                                starttime = now;
                                //printf("First packet Seq=%ld\n, Time=%ld\n", packetSeqNo,starttime);
                        }
                        if (seq == 2) {
                                //delta = (now - last) / ( 1 + packetSeqNo - nextpkt);
                                //delta = (10 * tolerance) / (1 + packetSeqNo - nex);
                                //printf("DELTA=%ld\n",delta);
                                min = delta;
                                /*2nd Packet is put in late packet list so that we get an idea of our maximum tolerable ETA*/
                                latedata[late][0] = 0;
                                latedata[late][1] = packetSeqNo;
                                latedata[late][2] = delta + tolerance;
                                late++;
                        }
                } else {
                        /*DELAY JITTER*/
                        //	delay_jitter = (now - last) - delta * (packetSeqNo - nextpkt) ;
                        delay_jitter = (now - last);
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
                                //printf("Late: Seq=%lld PacketNo=%lld delay_jitter=%lld\n", latedata[late][0], latedata[late][1], latedata[late][2], delta);

                        } 
                        /*Which quarter does the late packet falls into? 0-25%, 25-50%, 50-75% or 75-100% of Delta*/
                        //printf("now%d: last%d: delay_jitter=%d:delta=%d:tolerance=%d\n", now, last, delay_jitter, delta, tolerance);
                        if(delay_jitter > (delta + tolerance)) {
                                //				temp = 100.0f * ((delay_jitter - delta) / (float)delta) ;
                                temp = delay_jitter - delta;
                                printf("temp=%d:delta=%d:tolerance=%d\n", temp, delta, tolerance);
                                if ((temp > tolerance) && (temp <= 25000)) {
                                        bucket = 0;
                                } else if ((temp > 25000) && (temp <= 50000)) {
                                        bucket = 1;
                                } else if ((temp > 50000) && (temp <= 75000)) {
                                        bucket = 2;
                                } else if ((temp > 75000) && (temp <= 100000)) {
                                        bucket = 3;
                                } else {
                                        bucket = 4;
                                }

                                //printf("Packet=%lld:Jitter=%lld delay=%lld(%f,%d)\n",packetSeqNo, delay_jitter, delta, temp,bucket);
                                late_quart[bucket]++ ;		
                                printf("bucket=%d:late_quart=%d\n", bucket, late_quart[bucket]);
                        }
                }
                /*ETA*/
                eta = starttime + (packetSeqNo - startseq) * (delta); /*Can take a fixed interval other than delta too*/
                //if((now - (eta + tolerance))>0)
                //printf("Seq=%lld Pkt=%lld NOW = %lld ETA =%lld NOW-ETA=%lld\n", seq, packetSeqNo, now , eta, now-eta);

                /*LOST PACKETS*/
                if( ((packetSeqNo - nextpkt))< 20) {
                        lostdata[(packetSeqNo - nextpkt)]++ ;
                        //printf("%lld %d Consecutive losses\n", lostdata[(packetSeqNo - nextpkt)], packetSeqNo - nextpkt);
                }
                nextpkt = packetSeqNo + 1;

                //last = now;
                while (strtok(NULL,","));
        }
        if(seq > 2)
                avg_delay_jitter = avg_delay_jitter /(double) (seq - 2) ;
        printf("Transmitter=%s\n",lastsrc);	
        printf("\nDelta=%lldus Tolerance=%lldus ActualLatePkts=%lld of %lld Packets \ndelay_jitter[Avg=%fus, Min=%lldus, Max=%lldus] \n\n", 
                        delta, tolerance, late - 1, seq, avg_delay_jitter, min, max);
        plot_late((late > 200) ? 200 : late , min, max, latedata);

        printf("\n\n******LATE PACKETS DISTRIBUTION w.r.t Delta*****\n");
        //	printf(" 0-10 ms  ==> %5d\n", late_quart[0]);
        printf(" %lld -25 ms ==> %5d\n", tolerance/1000, late_quart[0]);
        printf("25-50 ms  ==> %5d\n", late_quart[1]);
        printf("50-75 ms ==> %5d\n", late_quart[2]);
        printf("75-100ms ==> %5d\n", late_quart[3]);
        printf("100 ms+   ==> %5d\n", late_quart[4]);


        printf("\n\n**********LOST PACKETS********** \n");
        for(i = 1; i < 20; i++)
                if(lostdata[i]) 
                        printf("%d Consecutive lost packets==>%lld\n", i, lostdata[i]);
        fclose(fp);
        r_metric(seq);
        confidence_analysis(0, avg_tsf_ipd);
        //covarience_analysis(seq, avg_delay_jitter);
        printf("TSF based IPD=%lf usecs, Max=%lf, Min=%lf\n", avg_tsf_ipd, max_tsf, min_tsf);		
        return 0;
        }


        void r_metric(int64_t seq) {
                int i;
                int64_t temp = 0;
                float tempLoss = 0.0, tempLate =0.0, tempTotal = 0.0;

                packet_late[0] = (float)late_quart[0]/seq;
                packet_late[1] = (float)late_quart[1]/seq;
                packet_late[2] = (float)late_quart[2]/seq;
                packet_late[3] = (float)(late_quart[3] + (late_quart[4]))/seq;

                for (i = 1; i<=7; i++) {
                        packet_loss[i] = (float) lostdata[i]/seq;
                }
                for (i = 8; i<20; i++) {
                        temp += lostdata[i];
                }	
                packet_loss[8] = (float)temp/seq;

                printf("\n\n**********Late PACKETS PROBABILITIES********** \n");
                printf("packet_late[0] %f\n", packet_late[0]);
                printf("packet_late[1] %f\n", packet_late[1]);
                printf("packet_late[2] %f\n", packet_late[2]);
                printf("packet_late[3] %f\n", packet_late[3]);
                printf("\n\n**********LOST PACKETS PROBABILITIES********** \n");

                for(i = 1; i<=8; i++) {
                        printf("packet_loss[%d] %f\n", i, packet_loss[i]);
                        tempLoss += packet_loss[i] * A_LOSS * pow(COMMON_RATIO, (i-1));
                }

                for(i =0; i<4; i++) {
                        tempLate += packet_late[i] * A_LATE * pow(COMMON_RATIO, (i));
                }
                printf("%f %f \n", tempLoss, tempLate);
                tempTotal = tempLate + tempLoss;
                reliability = 1 - tempTotal;
                printf("Reliability Metric considering both late and lost packets = %f\n", reliability);
                printf("Reliability Metric considering late packets only = %f\n",   2 * (0.5 - tempLate)); // bcoz of the half weightage
                printf("Reliability Metric considering lost packets only = %f\n",   2 * (0.5 - tempLoss)); // bcoz of the half weightage
        }	


        void confidence_analysis (int64_t seq, double avg_delay_jitter) {

                float tow, sigma, temp;
                double varience = 0;
                int64_t i;
                FILE *fp;
                char logbuf[1024];
                char *token;
                char *lastsrc;
                char *logType;
                int acid;
                uint32_t sec;
                uint32_t usec;
                int rssi;
                double gps_time;
                uint32_t gps_usecs;   
                int64_t last = 0, now = 0, packetSeqNo = 0, starttime = 0, nextpkt = 0;
                int64_t delay_jitter = 0;
                int64_t delta = 0;
                int temp32 = 0, icount = 0;
                double avg_tsf_ipd = 0.0, sum_tsf = 0.0, min_tsf = 11111111, max_tsf = -1111111;
                if ((fp = fopen(logfilename, "r")) == NULL) {
                        printf("Cannot open %s. \n", logfilename);
                        exit(1);
                }

                while (1) {
                        if ( !fgets ( logbuf, 1024, fp ))
                                break;
                        token = (char*)strtok(logbuf, ",");
                        logType = token;
                        if (!strcmp(logType, "gps_wsmp") || !strcmp(logType, " gps_wsmp")) {
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
                                token = strtok(NULL, ",");
                                rssi = atoi(token);
                                /*TIM_TSF*/
                                token = strtok(NULL, ",");
                                gps_time = strtod(token, NULL);
                                gps_usecs = (gps_time - (int)gps_time) * MILLION;

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
                        now = (int64_t)(sec * USECS) + usec; // baban
                        //last = (uint64_t) (gps_time * USECS);
                        //printf("Siva1 %llu now %llu last\n", now, last);
                        /*TIM_TSF: Cacluclate avg IPD based on TSF*/
                        if(usec > gps_usecs) {
                                icount++;
                                temp32 = usec - gps_usecs;
                                sum_tsf = sum_tsf + temp32;
                                avg_tsf_ipd = sum_tsf / icount;
                                if(avg_tsf_ipd > max_tsf) 
                                        max_tsf = avg_tsf_ipd;
                                else if(avg_tsf_ipd < min_tsf)
                                        min_tsf = avg_tsf_ipd;
                        }
                        delay_jitter = (now - last);
                        varience += pow((delay_jitter - avg_delay_jitter), 2);
                        nextpkt = packetSeqNo + 1;
                        last = now;

                }
                confidence_lower = avg_tsf_ipd -  1.96 * (sqrt(varience/seq));
                confidence_upper = avg_tsf_ipd +  1.96 * ( sqrt(varience/seq));
                printf("Lower Confidence Limit at 95 percent %f usec\n", confidence_lower);
                printf("Upper Confidence Limit at 95 percent %f usec\n", confidence_upper);
                printf("Confidence Interval at 95 percent %f usec\n", (confidence_upper - confidence_lower));
                fclose(fp);
        }

        void
                covarience_analysis(int64_t seq, double avg_delay_jitter) {

                }
