/*

 * Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

 * Proprietary and Confidential Material.

 *

 */

#include <stdio.h>
#include <ctype.h>
#include <termio.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <time.h>
#include <signal.h>
#include <unistd.h>
#include <pthread.h>
#include "wave.h"


enum { ADDR_MAC = 0, UINT8 };

// User with ACID = 1 and ACM = demo
void sig_int(void);
void sig_term(void);
void wrss_request();   
static int pid ;
struct timespec ntsleep, ntleft_sleep;
//static USTEntry entry;
static WMEApplicationRequest entry;
static	uint64_t count = 0; 
static WMEWRSSRequest wrssrq;  
int  extract_macaddr(u_int8_t *, char * );
void set_args( void * ,char *, int );
void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication );   
int	confirmBeforeJoin(WMEApplicationIndication *);

static pthread_t wrssi;
static void *wrssi_client( void *data );
int sendreport = 1;
int retry = 0;

int main(int argc, char *argv[])
{

	//WSMIndication rxpkt;
//	int i =0, attempts = 10, drops = 0, result;
//	int  ret = 0;
  //  	u_int8_t string[1000];

    	if(argc < 3 )
    	{
		printf("usage:getwrssrx [<MAC ADDRESS>] [<CHANNEL>]\n");
		exit (-1);
    	}

	pid = getpid();
	memset(&entry, 0 , sizeof(WMEApplicationRequest));
	entry.psid = 5;
	printf("Invoking WAVE driver \n");

	if (invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0)
	{
		printf("Open Failed. Quitting\n");
		exit(-1);
	}

	printf("Registering User %d\n", entry.psid);
	if ( registerUser(pid, &entry) < 0)
	{
		printf("Register User Failed \n");
		printf("Removing user if already present  %d\n", !removeUser(pid, &entry));
		printf("USER Registered %d with PSID =%u \n", registerUser(pid, &entry), entry.psid);
	}

	    registerLinkConfirm(confirmBeforeJoin);
    	set_args( &wrssrq.macaddr, argv[1], ADDR_MAC );
    	set_args( &wrssrq.wrssreq_elem.request.channel, argv[2], UINT8 );
    	registerWRSSIndication(receiveWRSS_Indication);

	/* catch control-c and kill signal*/
	signal(SIGINT,(void *)sig_int);
	signal(SIGTERM,(void *)sig_term);
    
    pthread_create( &wrssi, NULL, wrssi_client, NULL );
    pthread_join( wrssi, NULL );
	return 0;
}
    
void *wrssi_client( void *data )
{
    int sts;
    memset(&ntsleep, 0, sizeof(ntsleep));
	memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
        
	while(1)  
    {
		ntsleep.tv_nsec = 0;
		ntsleep.tv_sec = 1;
		ntleft_sleep.tv_nsec = 0;
		ntleft_sleep.tv_sec = 0;
        do {
		    sts = nanosleep(&ntsleep, &ntleft_sleep);
		    if((ntleft_sleep.tv_nsec == 0) && (ntleft_sleep.tv_sec == 0)) {
                wrss_request();
                break;
            }
		    memcpy(&ntsleep,&ntleft_sleep, sizeof(ntsleep));
	        memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
	    }
		while(1);
    }
}



void sig_int(void)
{
	int ret;

	ret = removeUser(pid, &entry);
    	if(ret < 0)
        printf("\n USER Unregistration failed \n");
	signal(SIGINT,SIG_DFL);
	printf("\n\nPacket received = %llu\n", count); 
	printf("remoterx killed by kill signal\n");
	exit(0);

}

void sig_term(void)
{
	int ret;

	ret = removeUser(pid, &entry);
    	if(ret < 0)
        printf("\n USER Unregistration failed \n");
	signal(SIGINT,SIG_DFL);
	printf("\n\nPackets received = %llu\n", count); 
	printf("remoterx killed by kill signal\n");
	exit(0);
}

void set_args( void *data ,char *argname, int datatype )
{
    u_int8_t string[1000];
  //  int i;
    int temp = 0;
    u_int8_t temp8 = 0;

    switch(datatype) {
        case ADDR_MAC:
            memcpy(string, argname, 17);
            printf("MAC Address is %s\n", string );
            if(extract_macaddr( data, string) < 0 )
            {
                printf("invalid address\n");
            }

        break;

        case UINT8:
           temp = atoi(argname);
            temp8 = ( u_int8_t) temp;
            memcpy( data, &temp8, sizeof( u_int8_t));

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
        while( str[i] != ':' && (i < len) ){
         i++;
        }
        if(i > len) exit(0);
        digits = i - j;
        if( (digits > 2) ||  (digits < 1) || (octet >= maclen)){
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


int confirmBeforeJoin(WMEApplicationIndication *appind)
{
	printf("\nJoin\n");
	return 1; /*Return 0 for NOT Joining the WBSS*/
}

void wrss_request()     
{   
    int result;
    if (sendreport || retry) 
    {
    	result = getWRSSReport( pid, &wrssrq );
    	if( result < 0) {
            printf( "WRSS Request Failed"); 
    	} 
        sendreport = 0;
    }
}

void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication)
{
    	printf("WRSS receive Channel = %d   Report = %d\n", (u_int8_t)wrssindication->wrssreport.channel, 
                                                                            (u_int8_t)wrssindication->wrssreport.wrss);
        sendreport = 1;
    
}













