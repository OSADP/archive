

#include "gps_stat.h"
#include <stdio.h>
#include <ctype.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>
#include<sys/socket.h>
#include<sys/types.h>
#include<unistd.h>
#include<pthread.h>
#include<arpa/inet.h>
#include<sys/time.h> // for gettimeofday

static struct sockaddr_in gps_devaddr;
static int is_gps_devaddr_set = 0;
static char data_o_read[2048];
static char datatoparse[2048];
static char data_o_np[2048];

char tempstring[15];
int endnp =0;
int parse_gpsdata(GPSData *gps);
void get_gps_status(GPSData *);
static int sockfd = -1;
#define DEFAULT_DEVADDR "127.0.0.1"
int read_data(int sockfd,GPSData *gpsdata);
extern u_int64_t generatetsfRequest();
pthread_mutex_t wsmgpstlock = PTHREAD_MUTEX_INITIALIZER;
int get_time(int Ttime)
{
	char *token = NULL;
	char *str = NULL;
	char *temp = NULL;
	int ret_time = 0;
	time_t tme;
	tme = Ttime;
	str = ctime(&tme);

	temp = (char *)malloc(sizeof(int));
	token = strtok(str," ");		//week_day

	token = strtok(NULL," ");		//month

	token = strtok(NULL," ");		//date

	token = strtok(NULL,":");		//hour
 	sscanf(token,"%s",temp);
	memcpy(str,temp,sizeof(int));	

	token = strtok(NULL,":");		//min
 	sscanf(token,"%s",temp);	
	strcat(str,temp);
	
	token = strtok(NULL," ");		//sec
 	sscanf(token,"%s",temp);	
	strcat(str,temp);

	ret_time = atoi(str);

	free(temp);	

	return ret_time;
}

int get_date(int Tdate)
{
	char *token = NULL;
	char *str = NULL;
	char *temp = NULL;
	int ret_date = 0;
	time_t date;
	int i = 0, month_num = 0, day =0, year = 0;
        char mon[12][4]={ {"Jan\0"},
                          {"Feb\0"},
                          {"Mar\0"},
                          {"Apr\0"},
                          {"May\0"},
                          {"Jun\0"},
                          {"Jul\0"},
                          {"Aug\0"},
                          {"Sep\0"},
                          {"Oct\0"},
                          {"Nov\0"},
                          {"Dec\0"}
                        };
	date = Tdate;
	str = ctime(&date);

	temp = (char *)malloc(sizeof(int));
	token = strtok(str," ");		//week_day

	token = strtok(NULL," ");		//month
 	sscanf(token,"%s",temp);
	for(i=1; i <= 12; i++)
	{
		if(!strcmp(mon[i-1],temp))
			month_num = i;
	}
	
	token = strtok(NULL," ");		//date
 	sscanf(token,"%s",temp);	
	day = atoi(temp);
	//strcat(str,temp);	

	token = strtok(NULL,":");		//hour

	token = strtok(NULL,":");		//min

	token = strtok(NULL," ");		//sec
	
	token = strtok(NULL," ");		//year
 	sscanf(token,"%s",temp);	
	year = atoi(temp);
	
	//strcat(str,temp);
	ret_date = (day * 10000) + (month_num * 100) + (year % 100) ;
	free(temp);	
	return ret_date;
}

void  get_gps_status(GPSData *gpsdat)
{
	//GPSData gpsdat;	
	int skfd = 0;
	skfd = create_connect_sock();
	read_data(skfd,gpsdat);
	close_sock();
	//printf("Lat [%lf]   Lon [%lf]   Alt [%lf]  Time [%lf]  Sat [%d]\n",gpsdat->latitude,gpsdat->longitude,gpsdat->altitude,gpsdat->time,gpsdat->numsats);
	//return gpsdat;
}

char* get_gps_devaddr()
{
	if(is_gps_devaddr_set)
		return inet_ntoa(gps_devaddr.sin_addr);	
	else
		return (char *)DEFAULT_DEVADDR;
}

char* set_gps_devaddr(char *devaddr)
{
		int ret;
		is_gps_devaddr_set = 0;
#ifdef WIN32
		gps_devaddr.sin_addr.s_addr = inet_addr ((devaddr)? devaddr : (char*)DEFAULT_DEVADDR);
#else
		ret = inet_aton((devaddr)? devaddr : (char*)DEFAULT_DEVADDR, &gps_devaddr.sin_addr);
#endif
		if(!ret)
			return NULL;
		is_gps_devaddr_set = 1;
		return (devaddr)? devaddr : (char*)DEFAULT_DEVADDR ;		
}


int create_connect_sock()
{
	int ret, one =1; 	
	struct sockaddr_in gpsdaddr;
	int flags;
	
	if(sockfd > 0 )
		return sockfd;
		
	if ( (sockfd = socket(AF_INET, SOCK_STREAM,6)) < 0) 
		return -1;	
	
	if (sockfd > 0) {
		bzero(&gpsdaddr, sizeof(gpsdaddr));

		if(!is_gps_devaddr_set)
			set_gps_devaddr(DEFAULT_DEVADDR);

		gpsdaddr.sin_addr = gps_devaddr.sin_addr;
		gpsdaddr.sin_family = AF_INET;	
		gpsdaddr.sin_port = htons(2947);
	
		if(setsockopt(sockfd,SOL_SOCKET, SO_REUSEADDR,(char *)&one,sizeof(one)) == -1)
		{
			(void)close(sockfd);
			return -2;
		}
		ret = connect(sockfd, (struct sockaddr *) &gpsdaddr, sizeof(gpsdaddr));
		if (ret < 0) {
			(void)close(sockfd);
			return -2;			
		}
	}

#ifdef	WIN32

		if ( (ret = send(sockfd, "?WATCH={\"enable\":true,\"json\":true}\n", strlen("?WATCH={\"enable\":true,\"json\":true}\n"),0) ) < 1) {	
		return -3;
		}

#else

		if ( (ret = write(sockfd, "?WATCH={\"enable\":true,\"json\":true}\n", strlen("?WATCH={\"enable\":true,\"json\":true}\n")) ) < 1) {
			return -3;
		}

#endif
	sleep(3);	
	flags = fcntl(sockfd, F_GETFL, 0);
	fcntl(sockfd, F_SETFL, flags | O_NONBLOCK);
	return sockfd;	
}

int close_sock()
{
	close(sockfd);
	sockfd = -1;
	//close(sfd);
	return 0;
}

void make_time_packet(GPSData *gpsdata)
{
	GPSData localgpsdata;
	pthread_mutex_lock( &wsmgpstlock );
 	 memcpy(&localgpsdata,gpsdata,sizeof(GPSData));
         bzero(gpsdata,sizeof(GPSData));
         gpsdata->date=localgpsdata.date;
         gpsdata->time=localgpsdata.time;
	pthread_mutex_unlock( &wsmgpstlock );
}
#if 0
int read_data(int sockfd,GPSData *gpsdata)
{
        int status = -1, len;
	bzero(data_o,sizeof(data_o));
	bzero(gpsdata,sizeof(GPSData));
       	status = (int)read(sockfd,data_o,sizeof(data_o));
	if (status < 0) 	
		return status;
        len = status;
	//data_o[len] = '\0';
	//printf("> %s\n", data_o);
       	status = (int)read(sockfd,&data_o[len],sizeof(data_o)-len);
	if (status < 0) 	
		data_o[len] = '\0';
	else 
		data_o[status+len] = '\0';
	//printf("# %s\n", data_o);
		
	return parse_gpsdata(gpsdata);        
}
#endif


void parse_valid_data(char *gpsstring,int *isvalid,int *issky)
{
	char *token;
  token  = strtok(gpsstring, ",");
       if(strstr(token,"TPV"))
	{ *isvalid=1;
	  *issky=0;
	return;
	}
       if(strstr(token,"SKY"))    //check for class
        {
		*isvalid=1;
		*issky=1;
		return;
        }
	*isvalid=0;
	*issky=0;
	return;

}
int false_read_data(int sockfd,GPSData *gpsdata)
{
        int status = 1;
	while(status >0)
	{
        status = (int)read(sockfd,datatoparse,sizeof(datatoparse));
        //    printf("2parse---%s\n",datatoparse);
	}
            parse_gpsdata(gpsdata);
return;
}
int read_data(int sockfd,GPSData *gpsdata)
{
        int status = -1, len,i,j;
	int issky=0,isvalid=0;
	bzero(datatoparse,sizeof(datatoparse));
        int startparse =0;
        int endparse =0;
        status = (int)read(sockfd,&data_o_read[endnp],sizeof(data_o_read));
         if (status < 0)
                 return status; 
	
            len = status+endnp;
            for(i=0;i<len;i++)
                    {
                            if ((data_o_read[i]=='{'))
	        		{
	        			strncpy(tempstring,&data_o_read[i],14);
	        		    parse_valid_data(tempstring,&isvalid,&issky);
	    		
	        		if(isvalid)
	    			startparse=i;
	        		}
                    }
            if(startparse == 0)
		{
		endnp=len;
	//	make_time_packet(gpsdata);
            	return -1;
		}
            for(i=startparse;i<len;i++)
                    {	
                            if (data_o_read[i]=='}')
    			{
    			    if(issky)
    				{
    				   if (data_o_read[i+1]==']')
                                       endparse=i+2;
    				}
    				else
    				   endparse=i; 
    			}
    			
                }
            if(endparse == 0)
        	{
           	 status = (int)read(sockfd,&data_o_read[len],sizeof(data_o_read)-len);
       	     if (status < 0)
           	 return status;
           	 for(i=len;i<(len+status);i++)
                    {
                            if (data_o_read[i]=='}')
    			{
    			    if(issky)
    				{
    				   if (data_o_read[i+1]==']')
                                       endparse=i+2;
    				}
    				else
    				   endparse=i; 
    			}
    			
                    }
	
         	    if(endparse==0)
		    {
			endnp=len+status;
//			make_time_packet(gpsdata);
	            	return -1;
		    }
        	}
            for(i=startparse,j=0;i<=endparse;i++,j++)
            {
                    datatoparse[j]=data_o_read[i];
            }
            datatoparse[j]='\0';
        //   printf("2parse---%s\n",datatoparse);
            for(i=endparse+1,j=0;i<len;i++,j++)
            {
                    data_o_np[j]=data_o_read[i];
            }
            endnp=j;
            bzero(data_o_read,sizeof(data_o_read));
            for(i=0;i<endnp;i++)
            {
                    data_o_read[i]=data_o_np[i];
            }
          //  printf("2read---%s\n",data_o_read);
          //  printf("endnp---%d\n",endnp);
            bzero(data_o_np,sizeof(data_o_np));
	    pthread_mutex_lock( &wsmgpstlock );
            if ( (status=parse_gpsdata(gpsdata)) >0) {
        	if(gpsdata->fix <=1)
			make_time_packet(gpsdata);
	     }
	    pthread_mutex_unlock( &wsmgpstlock );
	     return status;
	
	
	
}

double pow(double x, double y)
{
	int i, n = 0;
	double ret = 1.0;
	n = (unsigned int)y;
	for(i = 0; i < n; i++)
		ret *= x;
	return ret;
	
}
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


int parse_gpsdata(GPSData *gps)
{
	char *token;
//	long  templ = 0;
	int temp = 0;
	double tempd = 0.0, temp_d = 0.0;
	char *str = NULL;
	struct timeval tv;
	int sats = 0;
	int temp_time;
	str = datatoparse;
	if (gps == NULL  || str == NULL)
		return -1;

	/*ADD LOCAL TIME: Used with TOD token in config file*/
	gettimeofday(&tv, NULL);
	gps->local_tod = (double)tv.tv_sec + ((double)tv.tv_usec)/ 1000000.0 ;

//#ifdef LIBWAVE
	/*ADD LOCAL MAC TSF: Used with TSF token in config file*/
//	gps->local_tsf = (uint64_t)generatetsfRequest();
//#endif
	  token  = strtok(str, ":");
     do{	
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
	
	if(strstr(token,"TPV")!=NULL)
	{
	     do
	      {
		token  = strtok(NULL,":");	//check for TPV objects
		if (token == NULL)
			return -1;
		
		else if (strstr(token,"tag") != NULL)
		{	
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
		}
		
		else if (strstr(token,"device") != NULL)
		{
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
		}
		
		else if (strstr(token,"time") != NULL)
		{
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &temp_d);
			gps->actual_time = temp_d;
		}
	
		else if (strstr(token,"ept") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}
	
		else if (strstr(token,"lat") != NULL)
		{
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->latitude = tempd;
		}

		else if (strstr(token,"lon") != NULL)
		{
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->longitude = tempd;	
		}
		
		else if (strstr(token,"alt") != NULL)
		{
			token  = strtok(NULL,",");	
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->altitude = tempd;
		}
		
		else if (strstr(token,"epx") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"epy") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}
		
		else if (strstr(token,"epv") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}
		
		else if (strstr(token,"track") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->course = true2magnetic(gps->latitude,
    				gps->longitude,
    				tempd);

		}
		
		else if (strstr(token,"speed") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->speed = tempd;
		}
		
		else if (strstr(token,"climb") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->climb = tempd;
		}
		
		else if (strstr(token,"eps") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"mode") != NULL)
		{
			token  = strtok(NULL,"}");
			if (token == NULL)
				return -1;
			sscanf(token, "%d", &temp);
			gps->fix = temp;
			break;	
		}
	   }while(token != NULL);
			temp_time = (int)temp_d;
			gps->time = get_time(temp_time);
			gps->date = get_date(temp_time);
			//printf("time -> [%lf] \t date -> [%d]\n",gps->time,gps->date);	
	}
	
	else if(strstr(token,"SKY")!=NULL)
	{
	   do{			
		token  = strtok(NULL,":");	//check for SKY objects	
		if (token == NULL)
			return -1;
	
		else if (strstr(token,"tag") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
			return -1;
		}

		else if (strstr(token,"device") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"xdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"ydop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"vdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->vdop = tempd;	
		}

		else if (strstr(token,"tdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"hdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
			sscanf(token, "%lf", &tempd);
			gps->hdop = tempd;	
		}

		else if (strstr(token,"gdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}

		else if (strstr(token,"pdop") != NULL)
		{
			token  = strtok(NULL,",");
			if (token == NULL)
				return -1;
		}
		else if (strstr(token,"satellites") != NULL)
		{	
		    do{
			token  = strtok(NULL,":");	//check for SATELLITE objects	
			if (token == NULL)
				return -1;
			
			else if (strstr(token,"PRN") != NULL)
			{
				sats++;
				token  = strtok(NULL,",");	
				if (token == NULL)
					return -1;				
			}
			else if (strstr(token,"el") != NULL)
			{
				token  = strtok(NULL,",");	
				if (token == NULL)
					return -1;				
			}
			else if (strstr(token,"az") != NULL)
			{
				token  = strtok(NULL,",");	
				if (token == NULL)
					return -1;				
			}
			else if (strstr(token,"ss") != NULL)
			{
				token  = strtok(NULL,",");	
				if (token == NULL)
					return -1;				
			}
			else if (strstr(token,"used") != NULL)
			{
				token  = strtok(NULL,"}");	
				if (token == NULL)
					return -1;				
		      		token  = strtok(NULL,"\"");	
				if (token == NULL)
					return -1;				
			}
		   }while(strstr(token,"]}") == NULL);
		gps->numsats = sats;	
		break;	
		}
	      }while(token != NULL);
	   } 
	    token  = strtok(NULL,":");		//check for class	
	}while(token != NULL);

	return 0;
}


