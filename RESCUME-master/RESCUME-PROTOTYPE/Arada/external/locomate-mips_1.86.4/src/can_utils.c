#include<stdio.h>
#include<stdlib.h>
#include<stdint.h>
#include<semaphore.h>
#include<string.h>
#include<unistd.h>
#include<signal.h>
#include<pthread.h>
#include<sys/time.h>
#include "can_gds.h"


//function forward declarations
char checksum(uint8_t *data, uint8_t len); 
void rx_can_client();
int construct_can_cmd(uint8_t *data, uint8_t len);
int read_one_cmd(void);
int convert (char *hexNumber);  /*Need to remove bcase file_parser.c contatins*/
int char_to_int(char digit);
int write_to_can(char *fname);
void sig_can(void);
void sig_alarm(void);
extern int bt_write(char *,int);
extern int bt_read(char *,int);
extern int Btooth_forward;

static struct timeval tv;
static int cfmCANId=0;
static uint8_t can_data[64],can_cmd[64],single_canCmd[32];
static uint8_t recvdSleepTime=0,sindx=0;
static uint8_t cmdId=0,respFlag=0,timerExp=0;
static int8_t res=0;
static FILE *fp=NULL;	
static char *str;
static char timestr[18]={0};
sem_t sem_res;
FILE *logfd = NULL;
char logFname[100];
char logLine[100];
uint8_t logLvl = 0;
pthread_mutex_t LogLock = PTHREAD_MUTEX_INITIALIZER;
extern can_GDSData_t candata;

int CANLOG(uint8_t level,char * buf,uint8_t *data, uint32_t len)
{
	gettimeofday(&tv,NULL);
    if(logLvl && (logLvl >= level))
    {
		pthread_mutex_lock( &LogLock );
        time(&tv.tv_sec);
        str=ctime(&tv.tv_sec);
        memcpy(timestr,str+4,15);
        fprintf(logfd,"%s.%03d: %s",timestr,tv.tv_usec/1000,buf);

     
    	if(data!=NULL && len != 0){
        	int i=0;
        	for(i=0; i<len; i++) 
                fprintf(logfd,"%02x ",data[i]);
            fprintf(logfd,"\n");
    	}
        fflush(logfd);
		pthread_mutex_unlock( &LogLock );
        return 1;
    }
    return -1;
}

int get_canOption(char *option, char *outStr)
{
	FILE *fdrd;
	char mline[200]="";
	char *sts;
	char *token = NULL;
	
	fdrd = fopen("/var/can.conf", "r");
	if (fdrd <=0) {
     	    printf("Error opening %s file\n", "/var/can.conf");
     	    return -1;
  	}
  	while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {
	    if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' '){
	        token = strtok(mline, "=");
                
            if(strcasecmp(token, option) == 0 ){
            	token = strtok(NULL,"\r\n ");
				if(!strcasecmp(token,"none"))
					break;
				strcpy(outStr,token);
				printf("Read value for %s : %s\n",option,outStr);
				fclose(fdrd);
				return 1;
			}
		}
	}

	fclose(fdrd);
	return -1;
}


int write_to_can(char *fname)
{
	uint8_t fcmd[64],mline[200];
	char *token=NULL;
	uint8_t timeOut=2,retryCnt=0,sleepTime=0;
	uint8_t writeFlag=0,i=0;
	  
  	signal(SIGALRM,(void *)sig_alarm);  
	fp = fopen(fname, "r"); 
	if(fp <=0) 
	{ 
		sprintf(logLine,"Error opening %s file\n", fname);
		CANLOG(1,logLine,NULL,0);
		return -1; 
	}
		memset(fcmd,0,sizeof(fcmd));
		
		while((fgets((char*)mline, sizeof(mline), fp)) != NULL) 
		{ 
			token=strtok((char*)mline," ");
			if(convert(token) == 0xC0)
			{
				while(token)
				{
					fcmd[i]=convert(token);
					++i;
					token=strtok(NULL," ");
				}
				//printf("\nmline %s",fcmd);		
				sleepTime=fcmd[i-1];
				fcmd[i-1]='\0';
				retryCnt=fcmd[i-2];
				fcmd[i-2]='\0';
				timeOut=fcmd[i-3];
				fcmd[i-3]='\0';
				i=i-3;	
				writeFlag=1;
				cmdId = fcmd[1]; 
				cmdId ^= 0x40;	
				if(cmdId & 0x80)
					respFlag=1;
				else
					respFlag=0;
				i=construct_can_cmd(fcmd, i);  
				//printf("\nretryCnt %x timeOut %x sleeTime %x",retryCnt,timeOut,sleepTime);
			}
			else if(convert(token) == 0xFE)
			{
				while(token)
				{
			
					fcmd[i]=convert(token);
					++i;
					token=strtok(NULL," ");
				}
				memcpy(&cfmCANId,&fcmd[1],4);				
				sleepTime=fcmd[7];
				retryCnt= fcmd[6];
				timeOut = fcmd[5];  
				respFlag=0;
				writeFlag=0;
				//printf("\nretryCnt %x timeOut %x CnfrmId %x",retryCnt,timeOut,confmId);
			}

			while(1)
			{
				if(writeFlag)				
				{
					if(bt_write((char *)can_cmd,i) < 0)
						perror("\nwrite cmd failed");
					else	
					{	
						sprintf(logLine,"Sent CanCmd: ");	
						CANLOG(1,logLine,can_cmd,i);
						//for(k=0;k<i;k++)
						//printf(" %x ",(uint8_t)can_cmd[k]);	
					}
				}
				retryCnt--;	
				if(timeOut != 0xFF)
					alarm(timeOut);
				printf("\nWaiting for response ");	
				if(!respFlag)
					sem_wait(&sem_res);
				if(!respFlag && retryCnt!=0)			
					continue;
				else if(respFlag)   // Received response message, send next cmd
				{
					//time(&now);
					//printf("\n\t--- Got response for CMDID  %x, timestamp %s",fcmd[1],ctime(&now));
					break;
				}
				else if(timerExp)
				{
					sprintf(logLine,"Timer Expired\n");
					CANLOG(1,logLine,NULL,0);
					break;
				}
			}
			i=0;timerExp=0;alarm(0);   //Timer Reset
			if(sleepTime == 0xFF)
				sleepTime = recvdSleepTime;
			if(sleepTime)
				usleep(1000*sleepTime);
			memset(fcmd,0,sizeof(fcmd));
			memset(mline,0,sizeof(mline));
		}
	fclose(fp);
	return -1;
}


void rx_can_client(void)
{
	int8_t i=0,len=0,dataLen=0,data[8];
	uint32_t canID=0;
	uint8_t VerifyCsum;
	char ll[2]={0};

	printf("\nwaiting to read data from GateWay");
	while(1){
		if (Btooth_forward == 1){
        	
			if(get_canOption("LOG_LEVEL", ll) < 0)
            			printf("No LOG Option in file (or) value is none\n");
            		else if((logLvl = atoi(ll)) > 0){
        		if(get_canOption("LOG_FILE", logFname) < 0)
            			printf("No LOG Option in file (or) value is none\n");
				else
                		logfd = fopen(logFname,"a+");
			}
			break;
		}
		sleep(3);
	}
		
	while((res = read_one_cmd())>0)
	{
		sprintf(logLine,"Rcvd CmdData: ");
		CANLOG(2,logLine,single_canCmd,sindx+1);
		//for(i=0; i<=sindx; i++)                 
		// 	printf("%x ",(uint8_t)single_canCmd[i]);

															
		VerifyCsum = checksum(single_canCmd,sindx+1);   
		if((VerifyCsum != single_canCmd[sindx-1]) || len>30){ // Discord packet if checksum is not valid   
			sprintf(logLine,"INcorrect Checksum, Data discorded: "); // or if maximum lenght exceeds.
			CANLOG(1,logLine,single_canCmd,sindx+1);
			sindx = 0;
			memset(single_canCmd,0,sizeof(single_canCmd));
			continue; //error
		}
		
		if(single_canCmd[1] & 0x40)
		{
       		if(cmdId == (single_canCmd[1]))
			{
				sprintf(logLine,"Recvd Respns msg for cmdId %x Can Cmd Data: ",cmdId);	
				CANLOG(1,logLine,single_canCmd,sindx+1);
				//for(i=0; i<=sindx; i++)                 		
				//	printf("%x ",(uint8_t)single_canCmd[i]);
				respFlag=1;           
				sem_post(&sem_res);	
			}	        			  	   		// sets here recvd respns flag 

		}
		else if(!(single_canCmd[1] & 0x80)) // checking the msg whether its requires 
		{                                                // to send response message.
			sprintf(logLine,"Response required message: ");
			CANLOG(2,logLine,single_canCmd,sindx+1);
		       /*set bit-6 in cmdid,
			[TBD] add any extra data based on requirement and send to BT-CAN GW */
			single_canCmd[1] = single_canCmd[1]^0x40; 
			i = construct_can_cmd(&single_canCmd[0],4); 
			bt_write((char *)can_cmd,i); 
		}
		else 
		{
			switch(single_canCmd[1]){
			case 0xA0:
			case 0xA2:
				memcpy(&canID,&single_canCmd[2],2);
				dataLen = sindx - 4; //4->1byte of cmdid + 2bytes of CANID + 1byte of checksum
				memcpy(&data,&single_canCmd[4],dataLen);
				break;

			case 0xA1:
			case 0xA3:
				memcpy(&canID,&single_canCmd[2],4);
				dataLen = sindx - 6; //4->1byte of cmdid + 4bytes of CANID + 1byte of checksum
				memcpy(&data,&single_canCmd[6],dataLen);
				break;

			default:
			break;
			}
			/* As per the requirment add cases in below switch */
			switch(canID){
			case 0x10B22080:
				recvdSleepTime = data[2];
				if(!memcmp(&cfmCANId,&canID,4))
				{
					sprintf(logLine,"Got ConfirmationId %x received sleep time %x: \n",cfmCANId, recvdSleepTime);
					CANLOG(1,logLine,single_canCmd,sindx+1);
					respFlag=1;
					sem_post(&sem_res);
					cfmCANId=0x0000000;	
				}
				break;

			case 0x0F1:
				if(data[0] & 0x40){
					sprintf(logLine,"HardBreakEvent: ");
					CANLOG(1,logLine,single_canCmd,sindx+1);
					//set hard break event 
					candata.eventFlag = 1;
				}
				break;

			default:
			break;
			}
		}

		sindx = 0;
		canID = 0;
		memset(single_canCmd,0,sizeof(single_canCmd));
	}
}

//It read one command at a time 
int read_one_cmd(void)
{
	uint8_t byte;
	while(bt_read((char *)&byte,1))
	{
		if((uint8_t)byte==0x7D){
			bt_read((char *)&byte,1);
			single_canCmd[sindx++] = byte ^ 0x20;
		}
		else
			single_canCmd[sindx++] = byte;
	
		if((uint8_t)byte == 0xfe)
			return sindx--;
	}
  return -1;
}

//Check sum calcualtions
//It expects complete one cmd as i/p
char checksum(uint8_t *data, uint8_t len) 
{
   	char cksum = 0;
	uint8_t i=0;
   	for (i=1; i<=len-3; i++)						
   		cksum = cksum + (data[i] ^ 0xAA);

    	cksum = 0xFF - (cksum ^ 0xAA);
   	//printf("\ncheck sum is %x",(uint8_t)cksum);
	return cksum;
}

//this function convert data into HDLC protocol CAN cmd format
int construct_can_cmd(uint8_t *data, uint8_t len)
{
	int i=0,cmdlen =0;
	memset(can_cmd,0,sizeof(can_cmd));		
	memcpy(&can_cmd,data,len);
	can_cmd[len-2] = checksum(can_cmd,len);
	cmdlen = len;

	for(i=1; i<cmdlen-1; i++)                          // Verifying the data if it contains any SOH(0xC0),EOH(0XFE),  
	{                                                  // Delimiters,(0x7D) modifying them according protocol.
		if((uint8_t)can_cmd[i]==0XC0 || (uint8_t)can_cmd[i]==0XFE || (uint8_t)can_cmd[i]==0x7D)
		{
			memmove(&can_cmd[i+1],&can_cmd[i], cmdlen-i);
			can_cmd[i] = 0x7D;
			can_cmd[i+1] ^= 0x20;
			cmdlen = cmdlen+1;	
		} 

	}
	return cmdlen;
}

void sig_can(void)
{
	int ret=0,sem_val=0;
		
	ret=sem_getvalue(&sem_res,&sem_val);/*getting semaphore value and unlocking if locked */
	if(sem_val <= 0 && ret == 0)
		sem_post(&sem_res);
   	sem_destroy(&sem_res);
	
	if(pthread_mutex_destroy(&LogLock))
			perror("mutex_destroy");
	if(logfd)
		fclose(logfd);
																		
	//printf("can-utils exited\n");
	exit(0);
}

void sig_alarm(void)
{
	sem_post(&sem_res);
	timerExp=1;
}
