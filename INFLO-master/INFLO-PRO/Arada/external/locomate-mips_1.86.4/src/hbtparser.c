#include <stdio.h>
#include <ctype.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include <sys/syslog.h>
#define COMMAND_LEN 20
#define DATA_SIZE 1512
#include "usbd.h"

extern int usb_connect();
extern int usb_close_sock(int usbsockfd);
char* get_usb_storage();
void get_logconf(void);
char* MessageTimestamp(void);
void get_HBT_options(char *);
void usbpurge(void);

uint8_t priority_rse=0;
uint8_t SecurityType_rse;
uint8_t MessageType;
char ttlp[4],ip[25];
char msgstart;
uint32_t pktdelaysec,threshdelaysec; 
uint32_t start_utctime_sec,stop_utctime_sec,payload_size;
char payload[5][512],msgpld[100],defaultstcode[2]="0";
int port,tcount;
static char strn[20];
void getpayload(void);
int threshold1,threshold2,threshold3,halt,t2;
int usb_usage = 0;
usbsock_cmd usbsock;



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
	temp = (char *)malloc(10);
        token = strtok(str," ");                //week_day

        token = strtok(NULL," ");               //month
        sscanf(token,"%s",temp);
        for(i=1; i <= 12; i++)
        {
                if(!strcmp(mon[i-1],temp))
                        month_num = i;
        }

        token = strtok(NULL," ");               //date
        sscanf(token,"%s",temp);
        day = atoi(temp);
        
        token = strtok(NULL,":");               //hour
        token = strtok(NULL,":");               //min
        token = strtok(NULL," ");               //sec
        token = strtok(NULL," ");               //year
        sscanf(token,"%s",temp);
        year = atoi(temp);
        
        ret_date = (day * 10000) + (month_num * 100) + (year % 100) ;
        free(temp);
        return ret_date;
        }
                          
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
        token = strtok(str," ");                //week_day

        token = strtok(NULL," ");               //month

        token = strtok(NULL," ");               //date

        token = strtok(NULL,":");               //hour
        sscanf(token,"%s",temp);
        memcpy(str,temp,sizeof(int));
        

        token = strtok(NULL,":");               //min
        sscanf(token,"%s",temp);
        strcat(str,temp);

        token = strtok(NULL," ");               //sec
        sscanf(token,"%s",temp);
        strcat(str,temp);

        ret_time = atoi(str);

        free(temp);

        return ret_time;
}



void get_HBT_options(char *filename)
{
        FILE *fdrd,*name;
        int q=0;
        char rdfile[100];
        char mline[200];
        int tempstr,i;
        char *sts;char str[10],str1[10],str2[10],str3[10],str4[10],str5[10];
        char *token = NULL,*subtoken1=NULL,*subtoken2,*subtoken3,*token1;
        char *svptr1,*svptr2,*svptr3,*t=NULL,tmpstr[50]={0},tmp_buf[100];
        struct tm *all;
        all = (struct tm *)calloc(1,sizeof(struct tm));
        static struct timeval tv;
	strcpy(rdfile, filename);
        fdrd = fopen(rdfile, "r");
	
	bzero(payload,sizeof(payload));
       
        if (fdrd <=0) 
	{
                syslog(LOG_INFO,"Error opening %s file\n", rdfile);
                exit(0);
        }

        memset(mline, 0, sizeof(mline));
        
        while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {
                
                if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' '){
                        token = strtok(mline, "=");
                        if (strcasecmp(token, "MessageType") == 0) {
                                token = strtok(NULL, " ");
                        }
                
                if (strcasecmp(token, "MessagePriority") == 0) {
                        token = strtok(NULL, " ");
                        priority_rse = atoi(token);
                }
                
                if (strcasecmp(token,"TransmissionTransportLayerProtocol") == 0) {
                        token = strtok(NULL, " ");
                        sscanf(token, "%s",ttlp);
                }
                
                if (strcasecmp(token,"TransmissionDestinationIPAddress") == 0) {
                        token = strtok(NULL, " ");
                        sscanf(token, "%s",ip);
                }
                
                if (strcasecmp(token,"TransmissionDestinationPortNumber") == 0) {
                        token = strtok(NULL, " ");
                        port = atoi(token);
                }
                
                if (strcasecmp(token,"TransmissionBroadcastInterval") == 0) {
                        token = strtok(NULL, " ");
                        sscanf(token, "%d",&pktdelaysec);
                      
                }
		
		if (strcasecmp(token,"TransmissionInterval") == 0) {
                        token = strtok(NULL, " ");
                        sscanf(token, "%d",&threshdelaysec);
		}

		if (strcasecmp(token,"TransmissionCount") == 0) {
                        token = strtok(NULL, " ");
                        sscanf(token, "%d",&tcount);
		}

                if(strcasecmp(token,"MessageDeliveryStart")==0) {
                        token = strtok(NULL, ",");
                        subtoken1 = strtok_r(token, "/",&svptr1);
                        subtoken2 = strtok_r(svptr1, "/",&svptr2);
                        subtoken3 = strtok_r(svptr2, ",",&svptr3);
                        all->tm_mon = atoi(subtoken1)-1;
                        all->tm_mday = atoi(subtoken2);
                        all->tm_year = atoi(subtoken3)-1900;
                        token = strtok(NULL," ");
                        token1 = strtok_r(token,":",&svptr1);
                        all->tm_hour = atoi(token1);
                        token = strtok_r(svptr1," ",&svptr2);
                        all->tm_min = atoi(token);
                        start_utctime_sec=mktime(all);
                }
                
                if(strcasecmp(token,"MessageDeliveryStop")==0) {
                        token = strtok(NULL, ",");
                        subtoken1 = strtok_r(token, "/",&svptr1);
                        subtoken2 = strtok_r(svptr1, "/",&svptr2);
                        subtoken3 = strtok_r(svptr2, ",",&svptr3);
                        all->tm_mon = atoi(subtoken1)-1;
                        all->tm_mday = atoi(subtoken2);
                        all->tm_year = atoi(subtoken3)-1900;
                        token = strtok(NULL," ");
                        token1 = strtok_r(token,":",&svptr1);
                        all->tm_hour = atoi(token1);
                        token = strtok_r(svptr1," ",&svptr2);
                        all->tm_min = atoi(token);
                        stop_utctime_sec=mktime(all);
                }
                
                if(strcasecmp(token,"MessageSignature")==0) {
                        token = strtok(NULL, " ");
                }
                
                if(strcasecmp(token,"MessageEncryption")==0) {
                        token = strtok(NULL, " ");
                }
                
                //message payload
                
                if(strcasecmp(token,"RSEUnitID")==0) {
                        strcpy(payload[0],token);
                        strcat(payload[0],"=");
			name = popen("conf_get system:basicSettings:apName | awk {'print $2'}","r");
			fgets(tmp_buf,50,name);
		        strcat(payload[0],tmp_buf);
			pclose(name);
                       }
                
                if(strcasecmp(token,"MessageTimeStamp")==0) {
                        strcat(payload[1],token);
                        strcat(payload[1],"=");
                       }
               
                if(strcasecmp(token,"RSEStatusCode")==0) {
                        strcat(payload[2],token);
                        strcat(payload[2],"=");
			}
               
               }
                
               memset(mline, 0, sizeof(mline));
               
               }
		getpayload();
		get_logconf();
		close((int)fdrd);
                free(all);
}



void getpayload()
{
 	char *t=NULL;
	int i=0;
	bzero(msgpld,100);
        strcpy(msgpld,payload[i]);
	i++;
	
	strcat(msgpld,payload[i]);
	MessageTimestamp();
        strcat(msgpld,strn);
	strcat(msgpld,"\n");                
	i++;
	
	strcat(msgpld,payload[i]);
	t=get_usb_storage();
	
	if(t!=NULL)
	   strcat(msgpld,t);
	else
	   strcat(msgpld,defaultstcode);
	   strcat(msgpld,"\0"); 

	payload_size = strlen(msgpld);
}
		   
		


char* MessageTimestamp()
{
	static struct timeval tv;
	int tempstr,q=0;
	char str1[10],str2[10],str3[10],str4[10],str5[10];
	char *str=strn;

	gettimeofday(&tv, NULL);
        tempstr = get_date(tv.tv_sec);
        q=tempstr/10000;
        sprintf(str2, "%d", q);
        q=tempstr%10000;
        q/=100;
        sprintf(str1, "%d", q);
        strcpy(str,str1);
        strcat(str,"/");
        strcat(str,str2);
        strcat(str,"/");
        q=20;
        sprintf(str1, "%d", q);
        strcat(str,str1);
        q=tempstr%100;
        sprintf(str1, "%d", q);
        strcat(str,str1);
        strcat(str,",");
        tempstr = get_time(tv.tv_sec);
        q=tempstr/10000;
        sprintf(str3, "%d", q);
        strcat(str,str3);
        strcat(str,":");
        q=tempstr%10000;
        q/=100;
        sprintf(str4, "%d", q);
        strcat(str,str4);
        strcat(str,":");
        q=tempstr%100;
        sprintf(str5, "%d", q);
        strcat(str,str5);

}

void get_logconf(){
    
	FILE *confd;
	char input[255],tmp[50];
	char *sts;
	char *token, *temp_token;
    
	confd = fopen("/var/logoffload.conf", "r");
   	
	if (confd <=0) {
            syslog(LOG_ALERT,"Error opening file\n");
            }
            
	while( (sts = fgets(input, sizeof(input), confd)) != NULL ) {
            
	if (input[0] != '#' && input[0] != ';' && input[0] != ' ')
	{
        	token = strtok(input, "=");
		if(strcasecmp(token, "Threshold1") == 0 ){
                token=strtok(NULL,"\r\n ");

                	if(token != NULL){
                    	     threshold1=atoi(token);
               		 }
               
		else{
                   
			 syslog(LOG_ALERT,"Threshold1 missing in conf file\n");
            
               	}
          	}

		if(strcasecmp(token, "Threshold2") == 0 ){
                token=strtok(NULL,"\r\n ");

                	if(token != NULL){
                    	     threshold2=atoi(token);
                	}
                
                else{
                    
                         syslog(LOG_ALERT,"Threshold2 missing in conf file\n");
             
                }
		}

		if(strcasecmp(token, "Threshold3") == 0 ){
                token=strtok(NULL,"\r\n ");

                	if(token != NULL){
                    		threshold3=atoi(token);
                	}
                
                else{

                         syslog(LOG_ALERT,"Threshold3 missing in conf file\n");
             
                }
                }


	 memset(input,0,sizeof(input));
        }
    }
    fclose(confd);
}


char* get_usb_storage()
{
        int status;
	char *s=NULL;
        int usbsockfd;
        if(halt !=4)
            halt=0;
        t2=0;

	usbsockfd = usb_connect();
        usbsock.cmd = USBUSAGE;
        
	if((write(usbsockfd,&usbsock,sizeof(usbsock))<0))
        {    
	   syslog(LOG_ERR,"write to usbsock fails \n");
	   halt=3;
	   return s;
	}
        
	read(usbsockfd,&usbsock,sizeof(usbsock));
        usb_usage = usbsock.ret;
	status = usbsock.cmd;
        
        
    	    if(usbsockfd >0)
 	        usbsockfd=usb_close_sock(usbsockfd);

	if(!strcmp(usbsock.fname,"MNTERROR"))  //USB mount failure
		halt=2;
	else if(usb_usage >= threshold3) //halt transmission if usb storage exceeds threshold3
	        halt=1;
	else if(usb_usage >= threshold2){
                s="2";
   		t2=1;
	}
	else if(status == 9)  //scp_filetx failure
		s="1";
	
        return s;
}

void usbpurge(void)
{

    int usbsockfd = -1;
    usbsockfd = usb_connect();
        usbsock.cmd = USBPURGE;
        usbsock.ret = usb_usage - threshold3; //using ret to pass the excess value
    if((write(usbsockfd,&usbsock,sizeof(usbsock))<0))
        {
       syslog(LOG_ERR,"write to usbsock fails \n");
    }
    if(usbsockfd>0)
    usbsockfd=usb_close_sock(usbsockfd);
}
