/*********************************************************
	genearation of random mac address
*********************************************************/

#include<stdio.h>
#include<stdlib.h>
#include<stdint.h>
#include<unistd.h>
#include<string.h>
#include<time.h>
#include<sys/syslog.h>
#include"wave.h"

int main(int argc, char *argv[])
{
	uint8_t macadd[6];
        if(invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0) {
                printf("Open failed. Quitting\n");
                exit(-1);
        }
	(void)generate_random_mac(macadd);
	printf("%02x:%02x:%02x:%02x:%02x\n",macadd[1],macadd[2],macadd[3],macadd[4],macadd[5]);
	return 0;
}
