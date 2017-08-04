#include <netinet/in.h>
#include <sys/time.h>
#include <sys/ioctl.h>
#include <sys/types.h>
#include <sys/select.h>
#include <sys/socket.h>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <syslog.h>
#include <string.h>
#include <ctype.h>
#include <math.h>
#include <errno.h>
#include <assert.h>
#include <signal.h>
#include <stdbool.h>
#include "wave.h"
#include "gpsd_config.h"
#include "gps.h"
#include "gpsdclient.h"
#include "revision.h"
#include "TrackComputations.h"
extern int configuregps(int,int);
extern char Ip[20],PortNo[10];
static struct gps_data_t gpsdata;
GPSData wsmgps;
GPSSAEData saegps;
static pthread_t gpsdthread;
static pthread_t gpscthread;
void *gpsc_server( void *data );
void *gpsd_client( void *data );
int convert_gps_sae();
void catch_user(int);
void update_offsets(void);
static pthread_mutex_t wsmgpslock = PTHREAD_MUTEX_INITIALIZER;
uint8_t firsttime=1,Stationary=FALSE;
struct sockaddr_in server_address;
struct sockaddr_in client_address;
fd_set readfds, testfds;
fd_set rfds;
static uint8_t valid_data=0, OffsetFlag=1;
static uint8_t valid_signal=1;
static double OffsetLat=0,OffsetLon=0,OffsetElev=0;
long double Heading = 2*PI;
struct ECEFCoords gECEFCoords;
struct EllipsoidalCoords gEllipsoidalCoords;
struct  EllipsoidalCoords PrevPoint ;
struct  EllipsoidalCoords CurPoint ;
CARTESIANCOORDS OffsetCoord ;

double true2magnetic(double lat, double lon, double heading)
{
    /* Western Europe */
    /*@ -evalorder +relaxtypes @*/
    if ((lat > 36.0) && (lat < 68.0) && (lon > -10.0) && (lon < 28.0)) {
        heading =
            (10.4768771667158 - (0.507385322418858 * lon) +
             (0.00753170031703826 * pow(lon, 2))
             - (1.40596203924748e-05 * pow(lon, 3)) -
             (0.535560699962353 * lat)
             + (0.0154348808069955 * lat * lon) -
             (8.07756425110592e-05 * lat * pow(lon, 2))
             + (0.00976887198864442 * pow(lat, 2)) -
             (0.000259163929798334 * lon * pow(lat, 2))
             - (3.69056939266123e-05 * pow(lat, 3)) + heading);
    }
    /* USA */
    else if ((lat > 24.0) && (lat < 50.0) && (lon > 66.0) && (lon < 125.0)) {
        lon = 0.0 - lon;
        heading =
            ((-65.6811) + (0.99 * lat) + (0.0128899 * pow(lat, 2)) -
             (0.0000905928 * pow(lat, 3)) + (2.87622 * lon)
             - (0.0116268 * lat * lon) - (0.00000603925 * lon * pow(lat, 2)) -
             (0.0389806 * pow(lon, 2))
             - (0.0000403488 * lat * pow(lon, 2)) +
             (0.000168556 * pow(lon, 3)) + heading);
    }
    /* AK */
    else if ((lat > 54.0) && (lon > 130.0) && (lon < 172.0)) {
        lon = 0.0 - lon;
        heading =
            (618.854 + (2.76049 * lat) - (0.556206 * pow(lat, 2)) +
             (0.00251582 * pow(lat, 3)) - (12.7974 * lon)
             + (0.408161 * lat * lon) + (0.000434097 * lon * pow(lat, 2)) -
             (0.00602173 * pow(lon, 2))
             - (0.00144712 * lat * pow(lon, 2)) +
             (0.000222521 * pow(lon, 3)) + heading);
    } else {
        /* We don't know how to compute magnetic heading for this
         * location. */
        //magnetic_flag = false;
    }

    /* No negative headings. */
    if (heading < 0.0)
        heading += 360.0;

    return (heading);
    /*@ +evalorder -relaxtypes @*/
}

void update_offsets(void){
	FILE *fdLat,*fdLon,*fdElev;
	char temp_buf[100]="\0",tmpstr[50]="\0";
	double offLat=0,offLon=0,offElev=0;
        char *token;
	fdLat = popen("grep -i gpsLatOffset /var/config | awk '{print $2}'","r");
        if(fdLat != NULL)
        {
            fgets(temp_buf,100,fdLat);
	    token = strtok(temp_buf,"\\ ");
	    if(token != NULL)
		sscanf(token,"%lf",&offLat);
            pclose(fdLat);
        }	
	fdLon = popen("grep -i gpsLonOffset /var/config | awk '{print $2}'","r");
        if(fdLon != NULL)
        {
            fgets(temp_buf,100,fdLon);
	    token = strtok(temp_buf,"\\ ");
	    if(token != NULL)
		sscanf(token,"%lf",&offLon);
            pclose(fdLon);
        }	
	fdElev = popen("grep -i gpsElevOffset /var/config | awk '{print $2}'","r");
        if(fdElev != NULL)
        {
            fgets(temp_buf,100,fdElev);
	    token = strtok(temp_buf,"\\ ");
	    if(token != NULL)
		sscanf(token,"%lf",&offElev);
            pclose(fdElev);
        }	
	//syslog(LOG_INFO,"Previous antenna Offset lat:lon:elev : %f : %f : %f :\n",OffsetLat,OffsetLon,OffsetElev);
	OffsetLat = offLat;
	OffsetLon = offLon;
	OffsetElev = offElev;
	if(OffsetLat == 0 && OffsetLon ==0)
		OffsetFlag=0;
	else
		OffsetFlag=1;
	syslog(LOG_INFO,"Antenna Offsets lat:lon:elev : %f : %f : %f :\n",OffsetLat,OffsetLon,OffsetElev);
}

void sig_int(void)
{

	pthread_cancel(gpsdthread);
	pthread_cancel(gpscthread);
	pthread_mutex_destroy(&wsmgpslock);
        exit(0);
}
void catch_user( int sig )
{
	update_offsets();
	//syslog(LOG_INFO,"Updated antenna Offsets lat:lon:elev : %f : %f : %f :\n",OffsetLat,OffsetLon,OffsetElev);
}

int main(int argc, char *argv[])
{
int ret;
update_offsets();

/**********************initializing wsmgps data structure to GPS_INVALID_DATA ****************/

wsmgps.actual_time = GPS_INVALID_DATA;
wsmgps.latitude = GPS_INVALID_DATA;
wsmgps.longitude = GPS_INVALID_DATA;
wsmgps.altitude = GPS_INVALID_DATA;
wsmgps.course = GPS_INVALID_DATA;
wsmgps.speed = GPS_INVALID_DATA;
wsmgps.climb = GPS_INVALID_DATA;
wsmgps.hdop = GPS_INVALID_DATA; 
wsmgps.vdop = GPS_INVALID_DATA;
wsmgps.time = GPS_INVALID_DATA;
wsmgps.date = GPS_INVALID_DATA;
wsmgps.epx = GPS_INVALID_DATA;
wsmgps.epy = GPS_INVALID_DATA;
wsmgps.epv = GPS_INVALID_DATA;
wsmgps.numsats = 255;

ret = pthread_create(&gpscthread, NULL, gpsc_server, NULL );
sched_yield();
ret = pthread_create(&gpsdthread, NULL, gpsd_client, NULL );
sched_yield();

#if 0
    while(gpsdata.dev.baudrate == 0)
    {
        sleep(1);
    }   
/*retreving waas status value from database */
waas = popen("conf_get system:timeSettings:gpsWAASStatus","r");
fgets(temp_buf,strlen("system:timeSettings:gpsWAASStatus")+3,waas);
sscanf(temp_buf,"%s %d",tmpstr,&waas_status);
syslog(LOG_INFO,"String is %s waas_status is %d",tmpstr,waas_status);
pclose(waas);
ret = configuregps(waas_status,gpsdata.dev.baudrate);
#endif

    signal(SIGINT,(void *)sig_int);
    signal(SIGTERM,(void *)sig_int);
    struct sigaction sigact;
    sigemptyset( &sigact.sa_mask );
    sigact.sa_flags = 0;
    sigact.sa_handler = catch_user;
    sigaction( SIGUSR1, &sigact, NULL );

	pthread_join( gpscthread, NULL );
	pthread_join( gpsdthread, NULL );
	while(1);
}

void *gpsd_client( void *threaddata ) {
struct timeval timeout,tv;
int data;
struct  EllipsoidalCoords    *ptrToOffset;

	if(GetGpsOptions()<0){
		syslog(LOG_INFO,"Error In Fetching Gps server Ip and Port\n");
	    return -1;
		}
		/* Open the stream to gpsd. */
reopengpsd:
	memset(&wsmgps,0,sizeof(GPSData));
    if (gps_open_r(Ip,PortNo, &gpsdata) != 0) {
        (void)fprintf(stderr,
                      "gpsc: no gpsd running or network error: %d, %s\n",
                      errno, gps_errstr(errno));
	goto reopengpsd;
        //exit(2);
    }

    (void)gps_stream(&gpsdata, WATCH_ENABLE, NULL);

    /* heart of the client */
    for (;;) {
        /* watch to see when it has input */
        FD_ZERO(&rfds);
        FD_SET(gpsdata.gps_fd, &rfds);

        /* wait up to five seconds. */
        timeout.tv_sec = 5;
        timeout.tv_usec = 0;

        /* check if we have new information */
        data = select(gpsdata.gps_fd + 1, &rfds, NULL, NULL, &timeout);

        if (data == -1) {
            fprintf(stderr, "gpsc: socket error 3\n");
            exit(2);
        } else if (data) {
            errno = 0;
            if (gps_read(&gpsdata) == -1) {
                fprintf(stderr, "gpsc: socket error 4\n");
		gps_close(&gpsdata);
		firsttime = 1;
		goto reopengpsd;
            }
        }
	pthread_mutex_lock( &wsmgpslock );
	wsmgps.actual_time = gpsdata.fix.time;
	if(wsmgps.actual_time > 0 &&firsttime ==1)
	{
	syslog(LOG_INFO,"GPSC:got  first valid time %lf",wsmgps.actual_time);
	firsttime=0;
	}
	//adding 0.125 to offset latency of 1.gpsd data processing 2.serial communication 3.socket communication
	//wsmgps.actual_time = (isnan(gpsdata.fix.time))?0: (gpsdata.fix.time  + 0.125); 

    // checks needed also for NAN values as we can miss one or more nmea sentence in a reporting cycle

	wsmgps.fix = gpsdata.fix.mode;
	if(wsmgps.fix >= 2 ){
       		wsmgps.actual_time =  gpsdata.fix.time;
		wsmgps.latitude = (isnan(gpsdata.fix.latitude))?GPS_INVALID_DATA:gpsdata.fix.latitude;	
		wsmgps.longitude = (isnan(gpsdata.fix.longitude))?GPS_INVALID_DATA:gpsdata.fix.longitude;
		if(wsmgps.fix >= 3 )
			wsmgps.altitude = (isnan(gpsdata.fix.altitude))?GPS_INVALID_DATA:(gpsdata.fix.altitude + gpsdata.separation);                           //Altitude from WGS84
		wsmgps.course = (isnan(gpsdata.fix.track))?GPS_INVALID_DATA:(true2magnetic(gpsdata.fix.latitude,gpsdata.fix.longitude,gpsdata.fix.track));
		wsmgps.speed = (isnan(gpsdata.fix.speed))?GPS_INVALID_DATA:gpsdata.fix.speed; 
		wsmgps.climb = (isnan(gpsdata.fix.climb))?GPS_INVALID_DATA:gpsdata.fix.climb;
		wsmgps.hdop = (isnan(gpsdata.dop.hdop))?GPS_INVALID_DATA:gpsdata.dop.hdop;
		wsmgps.vdop = (isnan(gpsdata.dop.vdop))?GPS_INVALID_DATA:gpsdata.dop.vdop;
		wsmgps.epx = (isnan(gpsdata.fix.epx))?GPS_INVALID_DATA:gpsdata.fix.epx;
		wsmgps.epy = (isnan(gpsdata.fix.epy))?GPS_INVALID_DATA:gpsdata.fix.epy;
		wsmgps.epv = (isnan(gpsdata.fix.epv))?GPS_INVALID_DATA:gpsdata.fix.epv;
		wsmgps.time = get_time(wsmgps.actual_time);
		wsmgps.date = get_date(wsmgps.actual_time);
		wsmgps.numsats = (isnan(gpsdata.satellites_used))?255:gpsdata.satellites_used; 
                wsmgps.longdir = (wsmgps.longitude < 0)? 'W' : 'E';
                wsmgps.latdir = (wsmgps.latitude < 0)? 'S' : 'N';
        	gettimeofday(&tv,NULL);	
		wsmgps.local_tod = (double)tv.tv_sec + ((double)tv.tv_usec)/ 1000000.0;
		valid_signal = 1;
	}	
	else{
		wsmgps.actual_time = GPS_INVALID_DATA;
		wsmgps.latitude = GPS_INVALID_DATA;
		wsmgps.longitude = GPS_INVALID_DATA;
		wsmgps.altitude = GPS_INVALID_DATA;
		wsmgps.course = GPS_INVALID_DATA;
		wsmgps.speed = GPS_INVALID_DATA;
		wsmgps.climb = GPS_INVALID_DATA;
		wsmgps.hdop = GPS_INVALID_DATA; 
		wsmgps.vdop = GPS_INVALID_DATA;
		wsmgps.time = GPS_INVALID_DATA;
		wsmgps.date = GPS_INVALID_DATA;
		wsmgps.epx = GPS_INVALID_DATA;
		wsmgps.epy = GPS_INVALID_DATA;
		wsmgps.epv = GPS_INVALID_DATA;
                wsmgps.longdir = '\0';
                wsmgps.latdir = '\0';
		wsmgps.numsats = 255;
		gettimeofday(&tv,NULL);
		wsmgps.local_tod = (double)tv.tv_sec + ((double)tv.tv_usec)/ 1000000.0;
		valid_signal = 0;
	}
	//Here We are calculating antenna offset values
	 if(valid_signal == 1 && OffsetFlag > 0)
	 {
	 
	 	CurPoint.lon = (wsmgps.longitude)*(PI/180); //converting degrees to radians 	
	 	CurPoint.lat = (wsmgps.latitude)*(PI/180);	

		if((1.0E-15 > fabs(PrevPoint.lon - CurPoint.lon)) && (1.0E-15 > fabs(PrevPoint.lat - CurPoint.lon)))	
			Stationary = TRUE;
		else
			Stationary = FALSE;	
		
	 	OffsetCoord.X = -OffsetLon; //Mentioned offset values are in meters and 	
	 	OffsetCoord.Y = OffsetLat; // here we need to change sign accoriding to  SAE J2735 standard
		ptrToOffset = fnComputeOffsetPointUsingHeadingLatLong(&PrevPoint,&CurPoint,&OffsetCoord,Stationary,FALSE,&Heading);
		
		if(ptrToOffset == NULL){
		//	syslog(LOG_INFO,"Antenna Offset not applied \n");
			memcpy(&PrevPoint,&CurPoint,sizeof(struct  EllipsoidalCoords));
		}	
		else {
			wsmgps.longitude = (ptrToOffset->lon)*(180/PI); //Converting radians to degrees and 
			wsmgps.latitude =  (ptrToOffset->lat)*(180/PI);//Writing offset values to wsmgps
			memcpy(&PrevPoint,&CurPoint,sizeof(struct  EllipsoidalCoords));
		}
	 }

	if(valid_signal == 0 )
	{
	   if((valid_data == 0))
           {
	       syslog(LOG_INFO,"Gps Signal Lost/ Disconnected(fix=%d)  \n", wsmgps.fix);
               valid_data = 1;
	   } 	
	}
	else if( valid_signal == 1)
	{
	   if((valid_data == 1) )
           {
	       syslog(LOG_INFO,"Gps Signal Received (fix=%d)  \n", wsmgps.fix);
               valid_data = 0;
	   } 	
	}
	
	convert_gps_sae();
	pthread_mutex_unlock( &wsmgpslock );
        sched_yield();
  }
}

void *gpsc_server( void *data )
{
    int server_sockfd, client_sockfd;
    int server_len, client_len;
    int result;
    char ch;
    int fd;
    int nread;
/*  Create and name a socket for the server.  */

    server_sockfd = socket(AF_INET, SOCK_STREAM, 0);

    server_address.sin_family = AF_INET;
    server_address.sin_addr.s_addr = htonl(INADDR_ANY);
    server_address.sin_port = htons(8947);
    server_len = sizeof(server_address);

    bind(server_sockfd, (struct sockaddr *)&server_address, server_len);

/*  Create a connection queue and initialize readfds to handle input from server_sockfd.  */

    listen(server_sockfd, 5);

    FD_ZERO(&readfds);
    FD_SET(server_sockfd, &readfds);

/*  Now wait for clients and requests.
    Since we have passed a null pointer as the timeout parameter, no timeout will occur.
    The program will exit and report an error if select returns a value of less than 1.  */

    while(1) {

        testfds = readfds;

        result = select(FD_SETSIZE, &testfds, (fd_set *)0,
            (fd_set *)0, (struct timeval *) 0);

        if(result < 1) {
            syslog(LOG_INFO,"GPSC:select error");
            exit(1);
        }

/*  Once we know we've got activity,
    we find which descriptor it's on by checking each in turn using FD_ISSET.  */

        for(fd = 0; fd < FD_SETSIZE; fd++) {
            if(FD_ISSET(fd,&testfds)) {

/*  If the activity is on server_sockfd, it must be a request for a new connection
    and we add the associated client_sockfd to the descriptor set.  */
               if(fd == server_sockfd) {
                    client_len = sizeof(client_address);
                    client_sockfd = accept(server_sockfd,
                        (struct sockaddr *)&client_address, &client_len);
                    FD_SET(client_sockfd, &readfds);
                   // printf("adding client on fd %d\n", client_sockfd);
                }

/*  If it isn't the server, it must be client activity.
    If close is received, the client has gone away and we remove it from the descriptor set.
    Otherwise, we 'serve' the client as in the previous examples.  */

                else {
                    ioctl(fd, FIONREAD, &nread);

                    if(nread == 0) {
                        close(fd);
                        FD_CLR(fd, &readfds);
                     //   printf("removing client on fd %d\n", fd);
                    }

                    else {
                      read(fd, &ch, 1);
				 	  	switch(ch){
							case '1':			
								pthread_mutex_lock( &wsmgpslock );
                        		write(fd, &wsmgps, sizeof(GPSData));
								pthread_mutex_unlock( &wsmgpslock );
								break;
						
							case '2':
								pthread_mutex_lock( &wsmgpslock );
                        		write(fd, &saegps, sizeof(GPSSAEData));
								pthread_mutex_unlock( &wsmgpslock );
								break;
							default:
								syslog(LOG_INFO,"Unknown Command\n");
								break;
						}
                    }
                }
            }
        }
        sched_yield();
    }
}

int convert_gps_sae()
{
	saegps.actual_time =(uint32_t)(wsmgps.actual_time);
	saegps.time =(uint32_t)(wsmgps.time);
	saegps.local_tod =(uint64_t)(wsmgps.local_tod);
	saegps.local_tsf =(uint64_t)(wsmgps.local_tsf);
	if(wsmgps.latitude ==GPS_INVALID_DATA)
		saegps.latitude =900000001;
	else
		saegps.latitude =(long)(wsmgps.latitude*10000000);
	saegps.latdir =wsmgps.latdir;
	if(wsmgps.longitude ==GPS_INVALID_DATA)
		saegps.longitude =1800000001;
	else
		saegps.longitude =(long)(wsmgps.longitude*10000000);
	saegps.longdir =wsmgps.longdir;
	if(wsmgps.longitude ==GPS_INVALID_DATA)
		saegps.altitude =61440;
	else
		saegps.altitude =(int16_t)(wsmgps.altitude*10);
	saegps.altunit	=wsmgps.altunit;
	saegps.course =(uint16_t)(wsmgps.course*80);
	saegps.speed =(uint16_t)(wsmgps.speed);
	saegps.climb =(uint64_t)(wsmgps.climb);
	saegps.tee =(uint64_t)(wsmgps.tee);
	saegps.hee =(uint64_t)(wsmgps.hee);
	saegps.vee =(uint64_t)(wsmgps.vee);
	saegps.cee =(uint64_t)(wsmgps.cee);
	saegps.see =(uint64_t)(wsmgps.see);
	saegps.clee =(uint64_t)(wsmgps.clee);
	saegps.hdop =(uint64_t)(wsmgps.hdop);
	saegps.vdop =(uint64_t)(wsmgps.vdop);
	saegps.numsats =wsmgps.numsats;
	saegps.fix =wsmgps.fix;
	saegps.tow =(uint64_t)(wsmgps.tow);
	saegps.date =(uint32_t)(wsmgps.date);
	saegps.epx =(uint64_t)(wsmgps.epx*100);
	saegps.epy =(uint64_t)(wsmgps.epy*100);
	saegps.epv =(uint64_t)(wsmgps.epv*100);

	return 0;	
}

