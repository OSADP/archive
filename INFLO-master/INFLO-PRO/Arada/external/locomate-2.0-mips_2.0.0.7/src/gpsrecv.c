
//*******gpsrecv************

#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/ioctl.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <sys/syslog.h>
#include <tgmath.h>
#include <errno.h>
#include <sys/mount.h>
#include <dirent.h>
#include <sys/stat.h>
#include <sys/syscall.h>
#include <sys/socket.h>
#include <sys/syscall.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <endian.h>
#include<stdint.h>
#define GPS_INVALID_DATA -5000.0


#define BIGENDIAN               \
( {                             \
        long x = 0x00000001;    \
        !(*(char *)(&x));       \
})


#ifndef __GPS_STAT_L_
#define __GPS_STAT_L_
#define uint_8          unsigned __int8


#define swap32_(x)       ((((x)&0xFF)<<24)       \
                         |(((x)>>24)&0xFF)       \
                         |(((x)&0x0000FF00)<<8)  \
                         |(((x)&0x00FF0000)>>8)  )

#define swap64(x1) \
({ \
        unsigned long long x = (x1);\
        (((x & 0xFFULL) << 56| (x & 0xFF00000000000000ULL) >> 56| \
        (x & 0xFF00ULL) << 40| (x & 0xFF000000000000ULL) >> 40| \
        (x & 0xFF0000ULL) <<24| (x & 0xFF0000000000ULL) >> 24| \
        (x & 0xFF000000ULL) <<8 | (x & 0xFF00000000ULL) >>8 ));\
})

typedef struct{
        double          actual_time;            //no. of sec from jan 1, 1970 00:00:00
        double          time;
        double          local_tod;
        uint64_t        local_tsf;
        double          latitude;
        char            latdir;
        double          longitude;
        char            longdir;
        double          altitude;
        char            altunit;
        double          course;
        double          speed;
        double          climb;
        double          tee;
        double          hee;
	double          vee;
        double          cee;
        double          see;
        double          clee;
        double          hdop;
        double          vdop;
	uint8_t         numsats;
	uint8_t         fix;
        double          tow;
        int             date;
        double          epx;
        double          epy;
        double          epv;
}__attribute__ ((packed)) GPSData;

#endif

int gpsc_connect();
int gpsc_close_sock();

static GPSData wsmgps;
static int gpssockfd = -1;
static int gpscsockfd = -1;

static struct sockaddr_in gpsc_devaddr;
static int is_gpsc_devaddr_set = 0;
#define DEFAULT_DEVADDR "127.0.0.1"
char ip[25];

char* get_gpsc_devaddr()
{
        if(is_gpsc_devaddr_set)
                return inet_ntoa(gpsc_devaddr.sin_addr);
        else
                return (char *)DEFAULT_DEVADDR;
}

char* set_gpsc_devaddr(char *devaddr)
{
                int ret;
                is_gpsc_devaddr_set = 0;
#ifdef WIN32
                gpsc_devaddr.sin_addr.s_addr = inet_addr ((devaddr)? devaddr : (char*)DEFAULT_DEVADDR);
#else
                ret = inet_aton((devaddr)? devaddr : (char*)DEFAULT_DEVADDR, &gpsc_devaddr.sin_addr);
#endif
                if(!ret){
			printf("Provide proper IPv4 Address: exiting\n");
                        exit(-1);
		}
                is_gpsc_devaddr_set = 1;
                return (devaddr)? devaddr : (char*)DEFAULT_DEVADDR ;
}


int gpsc_connect()
{
        int ret, one =1;
        struct sockaddr_in gpsdaddr;
        int flags;

        if(gpscsockfd > 0 )
                return gpscsockfd;

        if ( (gpscsockfd = socket(AF_INET, SOCK_STREAM,6)) < 0) {
                printf("gpsc %d\n", __LINE__);
                return -1;
        }

        if (gpscsockfd > 0) {
                bzero(&gpsdaddr, sizeof(gpsdaddr));

                if(!is_gpsc_devaddr_set)
                        set_gpsc_devaddr(ip);

                gpsdaddr.sin_addr = gpsc_devaddr.sin_addr;
                gpsdaddr.sin_family = AF_INET;
                gpsdaddr.sin_port = htons(8947);
                
		if(setsockopt(gpscsockfd,SOL_SOCKET, SO_REUSEADDR,(char *)&one,sizeof(one)) == -1)
                {
              		printf("setsock gpsc %d\n", __LINE__);
                        gpsc_close_sock();
                        return -2;
                }
                ret = connect(gpscsockfd, (struct sockaddr *) &gpsdaddr, sizeof(gpsdaddr));
                if (ret < 0) {
                        gpsc_close_sock();
                        printf("failing on connect to gpsc\n");
                        return -2;
                }
        }
	printf("gps socket fd:%d \n",gpscsockfd);
        return gpscsockfd;
}

int gpsc_close_sock()
{
        close(gpscsockfd);
        gpscsockfd = -1;
        return 0;
}


void main(int arg, char *argv[])
{

char ch ='1';
uint64_t *lat,*lng,*alt;
uint64_t *at,*ti,*pt,*ex,*ey,*ev;
uint32_t *dte,*fx,*nst;
int status=0;

		if (arg < 2) {
			printf("Usage:Enter Locomate ipaddress \n");
			exit(0);
		}

                strcpy(ip,argv[1]);
      	  	printf("##### Starting Application gpscrecv #####\n");

		while(1)
		{
        	gpssockfd = gpsc_connect();

	
		if(gpssockfd < 0)
                 {
                        printf("gpstime: gpsc is not running...%d\n",gpssockfd);
                        exit(0);
                 }
                
		memset(&wsmgps,0,sizeof(GPSData)); 
		status = write(gpssockfd,&ch,1);
                
		if(status < 1)
                {
                syslog(LOG_INFO,"gpstime: write error %d (err %d)!!\n", status, errno);
		exit(0);
       		}
                
		status = read(gpssockfd,&wsmgps,sizeof(GPSData));  //read from gpsc
                

		if(status < sizeof(GPSData))
                {
                syslog(LOG_INFO,"gpstime: read error %d (err %d) exp %d!!\n", status, errno, sizeof(GPSData));
                gpsc_close_sock();
                gpssockfd=-1;
                sleep(5);
                }

		if(!BIGENDIAN)
		{
		at=((uint64_t*)(&wsmgps.actual_time));
                *at=swap64(*at);
		ti=((uint64_t*)(&wsmgps.time));
                *ti=swap64(*ti);
		pt=((uint64_t*)(&wsmgps.local_tod));
                *pt=swap64(*pt);
		lat=((uint64_t*)(&wsmgps.latitude));
                *lat=swap64(*lat);
		lng=((uint64_t*)(&wsmgps.longitude));
                *lng=swap64(*lng);
		alt=((uint64_t*)(&wsmgps.altitude));
                *alt=swap64(*alt);
		dte=(uint32_t*)(&wsmgps.date);
                *dte=swap32_(*dte);
		ex=((uint64_t*)(&wsmgps.epx));
                *ex=swap64(*ex);
		ey=((uint64_t*)(&wsmgps.epy));
                *ey=swap64(*ey);
		ev=(uint64_t*)(&wsmgps.epv);
                *ev=swap64(*ev);
		}
		
		if(wsmgps.actual_time == GPS_INVALID_DATA || wsmgps.actual_time == 0.0)
                {
                    printf("gpstime:wrg at=%lf fix=%d\n",wsmgps.actual_time,wsmgps.fix);
                    gpsc_close_sock();
                    gpssockfd=-1;
                    sleep(10);
		    continue;
                 
                }
	
 
		printf("\n actual_time:\n %lf \n",wsmgps.actual_time);
		printf("\n time:\n %lf \n",wsmgps.time);
		printf("\n local_ tod:\n %lf \n",wsmgps.local_tod);
		printf("\n latitude:\n %lf \n",wsmgps.latitude);
		printf("\n longitude:\n %lf \n",wsmgps.longitude);
		printf("\n altitude:\n %lf \n",wsmgps.altitude);
                printf("\n date:\n %d \n",wsmgps.date);
		printf("\n numsats:\n %d \n",wsmgps.numsats);
		printf("\n fix:\n %d \n",wsmgps.fix);
		printf("\n epx:\n %lf \n",wsmgps.epx);
		printf("\n epy:\n %lf \n",wsmgps.epy);
		printf("\n epv:\n %lf \n",wsmgps.epv);

        	exit(0);
         
 		}
}


