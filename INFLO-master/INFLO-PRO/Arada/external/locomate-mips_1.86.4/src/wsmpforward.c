/*
 * Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.
 * Proprietary and Confidential Material.
 *
 */

#include "wave.h"
#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <ifaddrs.h>


static WMEApplicationRequest entry;
static int devicemode = WAVEDEVICE_REMOTE;
static WMEApplicationRequest aregreq;
static int pid;

void sig_int(void);
void sig_term(void);

void usage() {
    printf("\nWSMP-Forward Application\n");
    printf("\n \n");
    printf("\nINFO: WSMP forward application receives the specified WSMP packets and and forwards to a remote host using UDP \n");
    printf("\n \n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -i:\tIPv6 address of remote host(currently supports only IPv6)\n");
    printf("\t -p:\tPort number of remote host side listener\n");
    printf("\t -y:\tPSID of the service of which packets are to be forwarded\n");
    printf("\t -u:\tUser Request Type [1:auto, 2:unconditional, 3:none]\n");
    printf("\t -x:\tExtended Access <0:alternate /1:continuous>\n");
    printf("\t -s:\tService Channel (Mandatory is user request is unconditional)\n");
    printf("\t DEFAULT Values for some options [ -u 1 ] [ -y 32] [ -x 0] [ -s 172]\n");

    exit(0);
}

void Options(int argc, char *argv[])
{
    int index = 0;
    int t,ret;
    static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"IP Address", required_argument, 0, 'i'},
        {"Port", required_argument, 0, 'p'},
        {"PSID", required_argument, 0, 'y'},
        {"user request type", required_argument, 0, 'u'},
        {"extended access", required_argument, 0, 'x'},
        {"service channel", required_argument, 0, 's'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hi:p:y:u:x:s:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
        case 'h':
            usage();
            break;
        case 'i':
            ret=inet_pton(AF_INET6,optarg,&entry.ipv6addr);
            if(ret<0)
            {
                printf("inet_pton failed");
                return 0;
            }
            break;
        case 'p':
            entry.serviceport=atoi(optarg);
            break;

        case 'y':
            entry.psid = atoi(optarg);
            break;

        case 'u':
            entry.userreqtype = atoi(optarg);
            break;

        case 'x':
            entry.schextaccess = atoi(optarg);
            break;

        case 's':
            entry.channel = atoi(optarg);
            break;

        default:
            usage();
            break;
        }
    }
}


/*This program demonstrates how to recive WSMP packets on target and forward them to Remote Host*/
int main (int argc, char *argv[])
{
    int ret;
    int blockflag = 0;
    pid = getpid();
    Options(argc,argv);
    entry.schaccess = 0;
    if(entry.serviceport == 0)
    {
        printf("Please enter IP address or service port\n");
    }

    if ((entry.userreqtype > USER_REQ_SCH_ACCESS_NONE) || (entry.userreqtype < USER_REQ_SCH_ACCESS_AUTO)) {
        printf("User request type invalid: setting default to auto\n");
        entry.userreqtype = USER_REQ_SCH_ACCESS_AUTO;
    }
    if(entry.psid == 0)
        entry.psid = 32;  //default values

    if(entry.channel == 0)
        entry.channel = 172;  //default values


    devicemode = WAVEDEVICE_LOCAL;
    ret =  invokeWAVEDevice(devicemode, blockflag ); /*blockflag is ignored in this case*/
    if (ret < 0 ) {
    } else {
        printf("Driver invoked\n");
    }
    /*Start recieving packets*/

    printf("Registering User\n");

    if (registerUser(pid, &entry) < 0)
    {
        printf("Register User Failed \n");
        printf("Removing user if already present  %d\n", !removeUser(pid, &entry));
        printf("USER Registered %d with PSID =%u \n", registerUser(pid, &entry), entry.psid);
    }


    signal(SIGINT,(void *)sig_int);
    signal(SIGTERM,(void *)sig_term);
    while(1) ;

    /*On exit its better to call removeUser(pid, &entry)*/
    return 0;
}

void sig_int(void)
{

    removeUser(pid, &entry);
    signal(SIGINT,SIG_DFL);
    printf("wsmpforward exit\n");
    exit(0);

}

void sig_term(void)
{

    removeUser(pid, &entry);
    signal(SIGINT,SIG_DFL);
    printf("wsmpforward exit\n");
    exit(0);

}
