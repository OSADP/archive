#include <stdio.h>
#include <errno.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <arpa/inet.h>
#include <sys/types.h>
#include <signal.h>
#include <unistd.h>
#include <getopt.h>
#include "can_gds.h"

//function forward declarations
void sig_int();
int Btooth_forward=-1;
pthread_t bluetooth=0;
pthread_t rxCan_tId=0;
extern void * main_bluetooth(void *);
extern void * rx_can_client(void *);
extern int write_to_can(char *);
extern void sig_int_bluetooth();		
extern void sig_can();
can_GDSData_t candata;
char fname[30];

void usage() {
    printf("\nbluetooth_can Application\n");
    printf("\nINFO: bluetooth_can configure commands on Bluetooth-can Gateway \n      please mention commands in file\n      It uses the mac addres mentioned in can.conf");
    printf("\n\n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -f:\tFile name which had commands to configure on BTGW\n");
    exit(0);
}

int main(int argc, char **argv) 
{
int32_t app_id=4;  
	if(argc < 2)
		usage();
	Options(argc,argv);
	
	signal(SIGINT, (void *)sig_int);
	pthread_create(&bluetooth,NULL,main_bluetooth,&app_id);
	pthread_create(&rxCan_tId, NULL,(void *)rx_can_client,NULL);
	while(1){
		if (Btooth_forward == 1){
			if(write_to_can(fname)<0){
				if(!errno)	
					printf("\n\t---Commands Cmpltd, EOF reached---");
				else
					printf("\n\twrite_to_can failed");
			}
			break;
		}
		sleep(3);
	}
	printf("\n Waiting on pthread_join for main_bluetooth");
	pthread_join(bluetooth,NULL);
return 0;
}

void sig_int()
{

	pthread_cancel(bluetooth);	
	if(rxCan_tId != 0)
    		pthread_cancel(rxCan_tId);
	sig_int_bluetooth();		
	sig_can();
	printf("\nbluetooth_can exited\n");
}

void Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"File Name", required_argument, 0, 'f'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hf:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
        case 'h':
            usage();
            break;
        case 'f':
            	strcpy(fname,optarg);
				printf("\nfile Name %s",fname);
			break;
        default:
            usage();
            break;
		}
	}
}
