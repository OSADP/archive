/*

* Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

* Proprietary and Confidential Material.

*

*/

#ifndef _WIN_IF_H
#define _WIN_IF_H

#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <sys/time.h>
#include <netinet/in.h>
#include "wave.h"
#include<sys/stat.h>
#include<unistd.h>
#include<syslog.h>
#include<string.h>

#define DEFAULT_DEVADDR "127.0.0.1"
#define LEAP(yr) ((yr%4 == 0 && yr%100 != 0) || (yr%400 == 0)? 1 : 0)

//**************************************************************************************** 

static int sockfd = 0;
//static char gps_data[1024];
GPSData wsmgps;
//static struct sockaddr_in gps_devaddr;
//static int is_gps_devaddr_set = 0;
int sleeptime = 900;
typedef struct local_time {
		char *month;
		int date;
		int hours;
		int mins;
		int sec;
		int year;	
		char *day_of_week;
}time_local;

//int parse_time();
//**************************************************************************************
#if 0
int parse_time()
{
	char *token;
	int time = 0;
	char *str = NULL;
	str = gps_data;
	if (str == NULL)
		return -1;

	token  = strtok(str, ":");
     	
	if (token == NULL)
		return -1;

	while(strstr(token,"class") == NULL)  	//check for class
	{
		token  = strtok(NULL, ":");
		if (token == NULL)
			return -1;
	}

	token  = strtok(NULL,",");	//check for TPV or SKY
	if (token == NULL)
		return -1;
	
	while(strstr(token,"TPV") == NULL)
	{
		token  = strtok(NULL,",");	
		if (token == NULL)
			return -1;
	}
	
	if(strstr(token,"TPV") != NULL)
	{
		token  = strtok(NULL,":");	
		if (token == NULL)
			return -1;
		while(strstr(token,"time") == NULL)
		{
			token  = strtok(NULL,":");	//check for Time only
			if (token == NULL)
				return -1;				
		}
	
		token  = strtok(NULL,",");	
		if (token == NULL)
			return -1;
		sscanf(token, "%d", &time);
	}
	
	return (time);
}


//***************************************************************************************

int set_local_time(int time)
{
	enum {
		January,
		February,
		March,
		April,
		May,
		June,
		July,
		August,
		September,
		October,
		November,
		December
	};

	int year,month,day,hours,mins,sec;
	int res,rem;
	int one_year,one_day,one_hour,one_min;
	int i,count = -1;
	
	time_t timevar;
	struct tm *new_time;
	
	new_time = (struct tm *)malloc(sizeof(struct tm));
	struct timeval tim;
	one_min = 60;
	one_hour = 3600;
	one_day = 86400;
	one_year = 31536000;
//	printf("time from gpsd -> %d\n",time);
	
	res = time / one_year;
	year = 1970 + res;
//	printf("Year = %d\t",year);
	
	rem = time % one_year;
	day =  (rem / one_day);

	for(i = 1970; i <=year; i++)
	{
		if(LEAP(i))
			count++;
		else
			continue;
	}
	day = day - count;
	
	if((1 <= day) && (day<= 31))
	{
		month = January;
	//	printf("Month = %d\t",month + 1);
	//	printf("Date = %d\n",day);
	 	//printf("DATE = %d:%d:%d\n",day,month+1,year);
		goto end;
	}
	day = day - 31;
		//check for LEAP year
		res = LEAP(year);
	if(res == 1 )
	{
		if((1 <= day) && (day<= 29))
		{
			month = February;
	    	//	printf("Month = %d\t",month + 1);
		//	printf("Date = %d\n",day);
			//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 29;

		if((1 <= day) && (day<= 31))
		{
			month = March;
	    	//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

	  	if((1 <= day) && (day<= 30))
	    	{
			month = April;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;
		
	  	if((1 <= day) && (day<= 31))
            	{
			month = May;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

          	if((1 <= day) && (day<= 30))
            	{
			month = June;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;

          	if((1 <= day) && (day<= 31))
            	{
			month = July;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

	       	if((1 <= day) && (day<= 31))
        	{
			month = August;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;	
		
		if((1 <= day) && (day<= 30))
            	{
			month = September;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
		 	//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;
		
          	if((1 <= day) && (day<= 31))
            	{
			month = October;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

          	if((1 <= day) && (day<= 30))
            	{
			month = November;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;

	  	if((1 <= day) && (day<= 31))
            	{
			month = December;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
	}
	else{
		if((1 <= day) && (day<= 28))
		{
			month = February;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 28;
		
		if((1 <= day) && (day<= 31))
		{
			month = March;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

	  	if((1 <= day) && (day<= 30))
	    	{
			month = April;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;
		
	  	if((1 <= day) && (day<= 31))
            	{
			month = May;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

          	if((1 <= day) && (day<= 30))
            	{
			month = June;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;

          	if((1 <= day) && (day<= 31))
            	{
			month = July;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

	       	if((1 <= day) && (day<= 31))
        	{
			month = August;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;	
		
		if((1 <= day) && (day<= 30))
            	{
			month = September;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;
		
          	if((1 <= day) && (day<= 31))
            	{
			month = October;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 31;

          	if((1 <= day) && (day<= 30))
            	{
			month = November;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
			goto end;
		}
		day = day - 30;

	  	if((1 <= day) && (day<= 31))
            	{
			month = December;
		//	printf("Month = %d\t",month +1);
		//	printf("Date = %d\n",day);
	 		//printf("DATE = %d:%d:%d\n",day,month+1,year);
		}
	  }
end:
	 //printf("DATE = %d:%d:%d\n",day,month+1,year);
	 rem = rem % one_day;
	 hours = rem / one_hour;
	 //printf("TIME = %d:",hours);
	
	 rem = rem % one_hour;
	 mins = rem / one_min;	
	 //printf("%d:",mins);
	
	 sec = rem % one_min;
	 //printf("%d\n",sec);
	
	new_time->tm_year = year - 1900;
	new_time->tm_mon = month;
	new_time->tm_mday = day;
	new_time->tm_hour = hours;
	new_time->tm_min = mins;
	new_time->tm_sec = sec;
	
	setenv("TZ","UTC",1);	
	timevar = mktime(new_time);
	tim.tv_sec = timevar;
	tim.tv_usec = 0;	
	settimeofday(&tim, NULL);
	
	free(new_time);
	//sleep(sleeptime);
	
	return 0;
    
}
#endif

//******************************************************************************************

int main(int argc, char *argv[])
{
/*	pid_t pid, sid;		//process id & session id
	
	pid = fork();
	
	if(pid < 0)
	{
		exit(EXIT_FAILURE);	//exit on failure
	}
	
	if(pid > 0)
	{
		exit(EXIT_SUCCESS); 	//exit from parent
	}
	
	umask(0);
	
	sid = setsid();			//new session id for child

	if(sid < 0)
	{
		exit(EXIT_FAILURE);
	}

	//change the current working directory*
	if((chdir("/"))< 0)		
	{
		exit(EXIT_FAILURE);
	}

	// close standard file descriptors*
	close(STDIN_FILENO);
	close(STDOUT_FILENO);
	close(STDERR_FILENO);

	// gpstime daemon starts from here*/
	int status = 0;
	int msecs = 0;
	char ch = '1';
	int localtime;
	struct timeval tim;
	(void)syslog(LOG_INFO," starting gpstime \n");
	if(argc < 2)
	{
		printf("\nUsage: gpstime [sleep time in minutes]\n");
		printf("Default value 15 minute\n\n");
	//	exit(1);
	}
	else 
		sleeptime = atoi(argv[1]) * 60;

		//sleep(2);			//wait for some time to update data on gpsd
	while(1)
	{
back:
		sockfd = gpsc_connect(DEFAULT_DEVADDR);		//gpsd socket creation
		if(sockfd < 0)
		{
			(void)syslog(LOG_ERR,"gpstime: gpsc is not running...(Retrying)%d\n",sockfd);
			sleep(1);
			goto back;	
		}
readagain:		
		status = write(sockfd,&ch,1);
		if(status < 1)
		{
		syslog(LOG_INFO,"gpstime: write error %d (err %d)!!\n", status, errno);
        }
		status = read(sockfd,&wsmgps,sizeof(GPSData));	//read from gpsc
		if(status < sizeof(GPSData))
		{
		syslog(LOG_INFO,"gpstime: read error %d (err %d) exp %d!!\n", status, errno, sizeof(GPSData));
			gpsc_close_sock();
			sockfd=-1;
			sleep(1);
			goto back;
		}
		if(wsmgps.actual_time == GPS_INVALID_DATA || (int)wsmgps.actual_time == 0)
		{
		  //(void)syslog(LOG_INFO,"gpstime:wrg at=%lf fix=%d\n",wsmgps.actual_time,wsmgps.fix);
			gpsc_close_sock();	
			sockfd=-1;
			sleep(1);
			goto back;
		}	
		else
		{
		msecs= (wsmgps.actual_time - (int)wsmgps.actual_time) * 1000;
	

        	if(msecs < 199 && msecs >= 0)
       		 {
				tim.tv_sec = (long)wsmgps.actual_time;
			        tim.tv_usec = msecs*1000;
                 
			        settimeofday(&tim, NULL);
				(void)syslog(LOG_INFO,"gpstime: SLT tv=%lu - usecs=%lu at=%lf fix=%d\n",tim.tv_sec, tim.tv_usec, wsmgps.actual_time,wsmgps.fix);
		 }
		 else
		 {
		//	(void)syslog(LOG_INFO,"readagain %lu\n",msecs);
			goto readagain;
		 }
		}
		gpsc_close_sock();
		sleep(sleeptime);		//sleep for specified time
	}
	return 0;
}
#endif
