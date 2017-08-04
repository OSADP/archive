
/********************************************* CML/SSL Automated Off-Load System *****************************************/

#include<stdio.h> //sprintf,printf
#include<unistd.h>   // sleep
#include<string.h> //strstr,memset
#include<getopt.h>   //getopt_long
#include<time.h>
#include<stdlib.h>   //system
#include<sys/time.h> //gettimeofday
#include<signal.h>
#include<syslog.h>
#include <sys/wait.h>
#include "usbd.h"
#include <sched.h>
#include <pthread.h>
#define LOG_FILENAME_LEN 100
#define FIVE_MIN 300

void usage();
int Options(int , char**);
int Off_Load(void);
int do_scp();
int do_scp_ssl();
int check_connection();
int get_time(int);
extern int usb_connect();
extern int usb_close_sock(int usbsockfd);
void sig_int(void);
void sig_segv(void);
void sig_term(void);
void sig_usr1(void);
void get_conf(void);

char logsyscmd_cml[500];
char logsyscmd_ssl[500];
char src_path[100];
char dest_path_cml[100] = {0},uname[50]={0},servadd[100]={0},filepath[150]={0};
char dest_path_ssl[100] = {0},subdirectories[200], apname[100]={0};
FILE *fds=NULL;
char file[150]= {0};
char sslfile[150]= {0};
char buff[512];
char key[100] = {0};
char conf_path[100]={0};
usbsock_cmd usbsock_cml;
int usbsockfd_cml = -1, conf_input=0;
int usbsockfd_ssl =-1;
int uploadtime =3600, threshold1 = 0;
int no_scp =0,RetryCount=3,RetryInterval=300,retry=0;
int usb_threshold=0,offload_status=0;
int siggiven = 0;
static pthread_t server_th;
static void *offload_ssl(void *data);
int check_cnt=0;
char app_name[64]="";
char pid[8] = "";
int ret_pid;
char cmd[64]="";
FILE *fp;


int list_ssl(void)
{

    static int nread=0;
    FILE *pf;
    int ret=0;
    char data[512]={0};
    char logsyscmd[250];
    bzero(logsyscmd,250);
    sprintf(logsyscmd,"ls -rt /tmp/usb/ssl/messages_*");
    pf = popen(logsyscmd,"r");
        if(!pf){
            syslog(LOG_INFO,"ssl-list:Could not open pipe for output.\n");
            return -2;
        }
    nread = fread(data,sizeof(char),512, pf);
    if(nread){
        sscanf(data, "%s",sslfile);
        ret = 0;
    }
    else{
        strcpy(sslfile,"NOFILE");
        ret = -1;
    }
        fflush(pf);
    if (pclose(pf) < 0)
        syslog(LOG_INFO,"ssl-list:Error: Failed to close command stream %d\n");

    return ret;
}

void mysleep(int sleep_secs, long int sleep_nsecs) {
    struct timespec myntsleep, myntleft_sleep;
    int sts;
        myntsleep.tv_nsec = sleep_nsecs;
        myntsleep.tv_sec = sleep_secs;
        myntleft_sleep.tv_nsec = 0;
        myntleft_sleep.tv_sec = 0;
                do
                {
                        sts = nanosleep(&myntsleep, &myntleft_sleep);
                if((myntleft_sleep.tv_nsec == 0) && (myntleft_sleep.tv_sec == 0)) {
                break;
                }
                        memcpy(&myntsleep,&myntleft_sleep, sizeof(myntsleep));
                memset(&myntleft_sleep, 0, sizeof(myntleft_sleep));
                }
                while( 1 );
}

int main(int argc, char *argv[]){
    int ret;
    unsigned int prev_time,cur_time, took_time;
    unsigned int multiples;
    int retval;
    FILE *apnamefd,*flush_status;
    struct timeval tv;
    char *token;
    pthread_attr_t attr;
    struct sched_param param;
    char tmpstr[50];
    int flush_t;
    signal(SIGINT,(void *)sig_int);
    signal(SIGSEGV,(void *)sig_segv);
    signal(SIGTERM,(void *)sig_term);
    signal(SIGUSR1,(void *)sig_usr1);	
    if(argc<2){
        printf("missing arguments\n");
        return -1; 
    }
    ret=Options(argc,argv);

    if(ret<0)
        return 0;
    
    get_conf();
    if(!no_scp){//for valid server address 
        if((apnamefd = popen("conf_get system:basicSettings:apName | awk {'print $2'}","r")) != NULL){
            if(fgets(apname,LOG_FILENAME_LEN,apnamefd)==NULL){
                syslog(LOG_ALERT,"apname not found: \n");
            }
            token = strtok(apname,"\r \n");
            strcpy(apname,token);
        }
        else{
            syslog(LOG_ALERT,"Did not got apName..");
        }
        pclose(apnamefd);
        sprintf(subdirectories,"RSE-logs/CML/%s",apname);
        sprintf(dest_path_cml,"%s:%s%s",servadd,filepath,subdirectories);
        memset(subdirectories,0,sizeof(subdirectories));
        sprintf(subdirectories,"RSE-logs/SSL/%s",apname);
        sprintf(dest_path_ssl,"%s:%s%s",servadd,filepath,subdirectories);
    

        sprintf(logsyscmd_cml,"mkdir -p /tmp/testdir/");//directory to use to copy files temporary in this application

        if(system(logsyscmd_cml)<0){
            printf("Temporary directory creation failed.. \n");
            return -2;
        }
   	
        check_connection();
    }

    pthread_attr_init (&attr);
    pthread_attr_setschedpolicy (&attr, SCHED_OTHER);
    pthread_attr_setschedparam (&attr, &param);
    pthread_attr_setinheritsched (&attr, PTHREAD_INHERIT_SCHED);

    ret = pthread_create(&server_th, NULL, offload_ssl, NULL );
	sched_yield();

    gettimeofday(&tv,NULL);
    prev_time = tv.tv_sec;
   // syslog(LOG_INFO,"[CML]before while %d",prev_time);
    while(1){
       
        if (!no_scp) {
        	usbsockfd_cml = usb_connect();

	        if(usbsockfd_cml <0){
        	    syslog(LOG_ALERT,"CML:Connect to usbd Failed,retrying after uploadtime");
	        }
		usbsock_cml.cmd = USBUSAGE;//this is for getting only value so USBUSAGE
		if((write(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))) <0 ) { // write request to usbd
                    syslog(LOG_INFO, "offload write to usbd fail\n");
        	    usb_threshold = 0;
		}
		else { 
        	    read(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml));//wait for file name to scp
        	    usb_threshold=usbsock_cml.ret;
		}
             gettimeofday(&tv,NULL);
             took_time = cur_time = tv.tv_sec;
	   if((cur_time-prev_time) >= (uploadtime - (FIVE_MIN * 2)) && (siggiven == 0))
	   {

               flush_status = popen("grep -i log-file-flush /var/config | awk '{print $2}'","r");
               if(flush_status != NULL)
               {
                  // sscanf(temp_buf,"%s %d",tmpstr,&flush_t);
		   fscanf(flush_status, "%s",tmpstr);
		   flush_t = atoi(tmpstr);
                   pclose(flush_status);
               }
	       //syslog(LOG_INFO,"--------file-flush----%d \n",flush_t);  	
	       if(flush_t)
	       {		
	           sprintf(app_name,"pidof capture_app");
	           if((fp = popen(app_name,"r")))  
	           {
                       fscanf(fp, "%s",pid);
                       pclose(fp);
                   }   
	           if(strcmp(pid,"")) 
	           {
                       sprintf(cmd, "kill -USR1 %s",pid);
                       system(cmd);
                   }
	           memset(app_name,0,sizeof(app_name));
 
 	           sprintf(app_name,"pidof eth_app");
 	           if((fp = popen(app_name,"r")))  
	           {
                       fscanf(fp, "%s",pid);
                       pclose(fp);
                   } 
	           if(strcmp(pid,"")) 
	           {
                       sprintf(cmd, "kill -USR1 %s",pid);
                       system(cmd);
                   }
	        }	
               siggiven = 1;	
	  }
          if((((cur_time-prev_time)>=uploadtime) || (usb_threshold >= threshold1) ) ){
                    syslog(LOG_WARNING,"starting offload cur=%d prev=%d",cur_time,prev_time);
		if((cur_time-prev_time)>=uploadtime){
         		prev_time = cur_time;
                        prev_time = prev_time - (prev_time % 60);
                siggiven = 0;
		}
                

                retval = Off_Load();
     
                if(retval<0)
                    retval = 10; //if cml offload fails
                
                switch(retval){ // error handling
                    case 10:
                        // error messages
                    break;

                    case 4:
                        syslog(LOG_INFO,"offload:No log files\n");
                    break; 
                    case 5:
                        syslog(LOG_ERR,"Cannot Mount Usb \n");
                    break; 

                    default:
                    break;
                }
        		memset(file,0,sizeof(file)); 
                gettimeofday(&tv,NULL);
                took_time=tv.tv_sec;
                took_time = took_time - prev_time;
                if(took_time  > uploadtime){ 
                 multiples = took_time/uploadtime;
                    syslog(LOG_WARNING,"offloads missed =%d since %d this offload took %d\n",multiples,prev_time,took_time);
                 prev_time = prev_time + (uploadtime * multiples);
                 siggiven = 0;
                }
            }
    	    if(usbsockfd_cml >0)
 	        usbsockfd_cml=usb_close_sock(usbsockfd_cml);
        }
        else {
		if((cur_time-prev_time)>=uploadtime){
         		prev_time = cur_time;
		}
	}
	took_time = took_time % FIVE_MIN;
        //syslog(LOG_INFO,"[CML]sleep %d",(FIVE_MIN - took_time));
        //sched_yield();
        sleep((FIVE_MIN - took_time));
        took_time = 0;
    }
	pthread_join(server_th, NULL);
    return 0;
}


void usage() {
    printf("\nCML Automated Off-Load Process\n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -c:\tConfiguration File Path\n"); 	
}

int Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"conf_path", required_argument, 0, 'c'},
        {0,0,0,0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hc:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
            case 'h':
                usage();
                return 2;
            break;
 
            case 'c':
                strcpy(conf_path, optarg);
                conf_input = 1; 
            break;
            
            default:
                usage();
                return -1;
            break;
        }
    }
    return 0;
}


int Off_Load()
{
    int retval = -1,recdval, pcloseret;
    //offload cml
    bzero(src_path,100);
    strcpy(src_path,"/tmp/testdir/");
    usbsock_cml.cmd = SCPCPY;//for scp copy this is command no.
    usbsock_cml.ret = offload_status; //initialization
           
    if((write(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))) <0 ) {// write request to usbd
        syslog(LOG_WARNING,"CML:Request to usbd Failed");
    }
    if(read(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))<0) {//wait for file name to scp
        syslog(LOG_WARNING,"CML:Read error from usbd");
    }
    strcpy(file,usbsock_cml.fname);
    retval = strcmp(file,"NOFILE");
    
    if(retval == 0)
        return 4;
    
    else if(!(strcmp(file,"MNTERROR")))
        return 5;

    while( retval != 0){
    
        sprintf(subdirectories,"RSE-logs/CML/%s",apname);
        sprintf(dest_path_cml,"%s:%s%s",servadd,filepath,subdirectories);
        memset(buff,0,sizeof(buff)); 
        recdval=do_scp();//scp done here
        if(recdval == 1)  { //for success status
	    retry=0;
            check_cnt++; // count number of files successfully offloaded
            sprintf(logsyscmd_cml,"rm -f %s",file);
            if(system(logsyscmd_cml)<0) {
                syslog(LOG_INFO,"Err: %s\n",logsyscmd_cml);
                return -1;
            }
	    usbsock_cml.cmd = SCPCPY;
	    offload_status=1; //1 for success
            usbsock_cml.ret = offload_status;//1
            if((write(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))) <0 ) {
                syslog(LOG_INFO,"CML:Request to usbd Failed");
            	return -1;
	    }
            if(read(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml)) <0) {
                syslog(LOG_INFO,"CML:Read error from usbd");
            	return -1;
	    }
            strcpy(file,usbsock_cml.fname);
            retval = strcmp(file,"NOFILE");

            if(retval == 0){// no more file to scp, exit condition of while
                return 4;
	    }
            else if(!(strcmp(file,"MNTERROR")))
                return 5;
       
                usbsock_cml.ret = 0;
	        usbsock_cml.cmd = USBCALUSAGE;//here we might have removed files from usb so USBCALUSAGE
	        if((write(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))) <0 ) { // write request to usbd
		    syslog(LOG_INFO, "offload write to usbd fail\n");
		    usb_threshold = 0;
	        }
	        else { 
		    read(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml));//wait for file name to scp
		    usb_threshold=usbsock_cml.ret;
	        }
        }
        else { //handle error
		
		offload_status=9; //9 for failure
                usbsock_cml.ret = offload_status;//9
		usbsock_cml.cmd = SCPCPY;
                if((write(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml))) <0 ) {
                    syslog(LOG_INFO,"retry:usb wr err\n");
                 }
                if(read(usbsockfd_cml,&usbsock_cml,sizeof(usbsock_cml)) <0) {
                    syslog(LOG_INFO,"CML:Read error from usbd");
                    return -1;
                }
                memset(&usbsock_cml,0,sizeof(usbsock_cml));

	    if(RetryCount != retry){
	        syslog(LOG_INFO, "retrying (%d) cml scp\n", retry);
        //sched_yield();
		sleep(RetryInterval);// 5 minutes to retry scp
            }
	    if (retry ==RetryCount){
		retry=0;
            	syslog(LOG_WARNING,"[CML_SCP]Could not scp cml at %s\n",dest_path_cml);
                break;
           }
		retry++;
        }

    }

    return 0;
}

int do_scp()
{
	int ret;
    sprintf(logsyscmd_cml,"/usr/sbin/rsync -avvz --progress --partial --inplace --timeout=180 --rsh='ssh -i %s' %s %s 2> /dev/null",key,file,dest_path_cml);
    ret = system(logsyscmd_cml); //start scp 
	if((WIFEXITED(ret)) && (WEXITSTATUS(ret)==21)){
        return 1;
    }
    else{
        syslog(LOG_INFO, "do_scp Err:[%d] %s \n",WEXITSTATUS(ret),logsyscmd_cml);
        return -1;
    }
    return 0;
}


int do_scp_ssl()
{
	int ret;
    sprintf(logsyscmd_ssl,"/usr/sbin/rsync -avvz --progress --partial --inplace --timeout=180 --rsh='ssh -i %s' %s %s 2> /dev/null",key,sslfile,dest_path_ssl);
	ret = system(logsyscmd_ssl); //start scp 
    if((WIFEXITED(ret)) && (WEXITSTATUS(ret)==21)){
        return 1;
    }
    else{
        syslog(LOG_INFO, "do_scp Err:[%d] %s \n",WEXITSTATUS(ret),logsyscmd_ssl);
        return -1;
    }
    return 0;
}

int check_connection()
{
    FILE *testfd;
	int recdval;
    char testchar[20]="Arada test file";
    strcpy(file,"/tmp/testdir/testfile");
    testfd = fopen(file,"w+"); 
    fwrite(testchar,sizeof(char),strlen(testchar),testfd);       
    fclose(testfd);
    recdval=do_scp();
    if (recdval != 1)
        syslog(LOG_ALERT,"SCP connection cannot established for CML\n");
    strcpy(sslfile,"/tmp/testdir/testfile"); 
    recdval=do_scp_ssl();
    if (recdval != 1)
        syslog(LOG_ALERT,"SCP connection cannot established for SSL\n");
    if(system("rm -f /tmp/testdir/testfile") < 0 ) {
        syslog(LOG_INFO,"cannot remove file: testfile \n");
        return -1;
    }

    if(recdval == 1){
        syslog(LOG_INFO,"connection established for CML/SSL scp\n");
    }
 
   return 0;
}


void sig_int(void)
{
    	    if(usbsockfd_cml >0)
 	        usbsockfd_cml=usb_close_sock(usbsockfd_cml);
	pthread_cancel(server_th);
    	    if(usbsockfd_ssl >0)
 	        usbsockfd_ssl=usb_close_sock(usbsockfd_ssl);
    exit(0);
}

void sig_term(void)
{
    sig_int();
}
void sig_segv(void)
{
    sig_int();
}
void sig_usr1(void) 
{
	get_conf();
	retry=0;
	syslog(LOG_INFO,"conf file reloaded with upload time %d,thresh1 %d retrycount %d retryinterval %d\n",uploadtime,threshold1,RetryCount,RetryInterval);
}
 
void get_conf(){
    FILE *confd;
    char input[255],tmp[50];
    char *sts;
    char *token, *temp_token;
    confd = fopen(conf_path, "r");

    if (confd ==NULL) {
            syslog(LOG_ALERT,"Error opening %s file\n", conf_path);
            return ;
	    //exit(0);
    }

    while( (sts = fgets(input, sizeof(input), confd)) != NULL ) {

        if (input[0] != '#' && input[0] != ';' && input[0] != ' '){//comments
            token = strtok(input, "=");
            
            if(strcasecmp(token, "ServerAddress") == 0 ){
                token=strtok(NULL,"\r\n ");
                memcpy(tmp, token, sizeof(tmp));
                if(strstr(token,"no-server") != NULL){
                    no_scp = 1;
                }
                
                else if(token != NULL){
                    temp_token = strstr(tmp, ".");
                    if(temp_token != NULL){
                         sprintf(servadd,"%s@%s",uname,token);
                     }
                     else {
                        sprintf(servadd,"\'[%s@%s]\'",uname,token);
                    }
                }
                else{
                    syslog(LOG_ALERT,"Server Address missing in conf file\n");
                }
            }

            if(strcasecmp(token, "SecurityKey") == 0 ){
                token=strtok(NULL,"\r\n ");

                if(token != NULL){
                    strcpy(key,token);
                }
                else{
                    syslog(LOG_ALERT,"Key missing in conf file\n");
                }
            }

            if(strcasecmp(token, "UserName") == 0 ){
                token=strtok(NULL,"\r\n ");

                if(token != NULL){
                strcpy(uname,token);
                }
                else{
                    syslog(LOG_ALERT,"username missing in conf file\n");
                }
            }

            if(strcasecmp(token, "FilePath") == 0 ){
                token=strtok(NULL,"\r\n ");

                if(token != NULL){
                    strcpy(filepath,token);
                }
                else{
                    syslog(LOG_ALERT,"File Path missing in conf file\n");
                }
               
            }

            if(strcasecmp(token, "UploadTime") == 0 ){
                token=strtok(NULL,"\r\n ");

                if(token != NULL){
                    uploadtime = atoi(token);
                }
                else{
                    syslog(LOG_ALERT,"Upload time not present setting default for 1 hour\n");
                    uploadtime = 3600;//default to 1 hour
                }
           }
	  if(strcasecmp(token, "Threshold1") == 0 ){
            	token=strtok(NULL,"\r\n ");

                if(token != NULL){
                    threshold1 = atoi(token);
                }
                else{
                    syslog(LOG_ALERT,"Threshold1 is not defined default value is 50\n");
                    threshold1=50;
                }
           } 
        
	  if(strcasecmp(token, "RetryCount") == 0 ){
            	token=strtok(NULL,"\r\n ");

                if(token != NULL){
                   RetryCount = atoi(token);
		}
                else{
                    syslog(LOG_ALERT,"Retry count  is not defined default value is 3\n");
                    RetryCount=3;
                }
           } 
	  if(strcasecmp(token, "RetryInterval") == 0 ){
            	token=strtok(NULL,"\r\n ");

                if(token != NULL){
                   RetryInterval = atoi(token);
                }
                else{
                    syslog(LOG_ALERT,"Retry interval  is not defined default value is 300\n");
                    RetryInterval=300;
                }
           } 
           memset(input,0,sizeof(input));
        }
    }
    fclose(confd);
}


void *offload_ssl(void *data){
    int retval_ssl=0;
    int multiples_ssl;
    int sts=-1,recdval;
    unsigned int crtime,prvtime,took_time_ssl;
    static int retry_ssl = 0;	
    char errbuff[100];
	struct timeval tv1;
    usbsock_cmd usbsock_ssl;
	gettimeofday(&tv1,NULL);
    prvtime = tv1.tv_sec;
    memset(&usbsock_ssl,0,sizeof(usbsock_ssl));
        gettimeofday(&tv1,NULL);
        prvtime = tv1.tv_sec;
	while(1){
        gettimeofday(&tv1,NULL);
        took_time_ssl = crtime = tv1.tv_sec;

        if((crtime-prvtime)>=uploadtime){
            prvtime = crtime;
            prvtime = prvtime - (prvtime % 60);
    	    if(!no_scp){
    	        system("mv -f /var/log/messages_* /tmp/usb/ssl/.");
           	retval_ssl= list_ssl();
        	while(retval_ssl == 0) {
        	    usbsockfd_ssl = usb_connect();
        	    sprintf(subdirectories,"RSE-logs/SSL/%s",apname);
        	    sprintf(dest_path_ssl,"%s:%s%s",servadd,filepath,subdirectories);
        	    memset(buff,0,sizeof(buff)); 
            	    recdval=do_scp_ssl();
	            if(recdval == 1){
			
		        retry_ssl = 0;
			usbsock_ssl.cmd = SCPCPY;
			offload_status=1;
                	usbsock_ssl.ret = offload_status; //1 for ssl success
               		
                	
			if((write(usbsockfd_ssl,&usbsock_ssl,sizeof(usbsock_ssl))) <0 ) {
                    		syslog(LOG_INFO,"usb write failed for SSL success code 1");
                 	}
                        if(read(usbsockfd_ssl,&usbsock_ssl,sizeof(usbsock_ssl)) <0) {
                            syslog(LOG_INFO,"CML:Read error from usbd");
                            return -1;
                        }
                        memset(&usbsock_ssl,0,sizeof(usbsock_ssl));

    	            	sprintf(logsyscmd_ssl,"rm -f %s",sslfile);
        	        	if(system(logsyscmd_ssl)<0) {
            	        		printf("Err: %s\n",logsyscmd_ssl);
                		}
                        retval_ssl= list_ssl();
	            } else {
			usbsock_ssl.cmd = SCPCPY;
			offload_status=9; //9 for failure
                	usbsock_ssl.ret = offload_status; //9 for failure
		
                	if((write(usbsockfd_ssl,&usbsock_ssl,sizeof(usbsock_ssl))) <0 ) {
                    	syslog(LOG_INFO,"usb write err for SSL error code 9");
                 	}
                        if(read(usbsockfd_ssl,&usbsock_ssl,sizeof(usbsock_ssl)) <0) {
                            syslog(LOG_INFO,"CML:Read error from usbd");
                            return -1;
                        }
                        memset(&usbsock_ssl,0,sizeof(usbsock_ssl));

			    if(retry_ssl != RetryCount){ // try till configurable retry count
		            retry_ssl++;
		
		        	if (retry_ssl == RetryCount){
			   	    syslog(LOG_WARNING,"[SSL_SCP]Could not transfer ssl at %s\n",dest_path_ssl);
				        retry_ssl = 0;	
                                    break;
		       		}
				else{
                                    //sched_yield();
				    sleep(RetryInterval);// wait till next retry
                                  }
				}
			}
    	    	    if(usbsockfd_ssl >0)
 	            usbsockfd_ssl=usb_close_sock(usbsockfd_ssl);
	        }
                gettimeofday(&tv1,NULL);
                took_time_ssl =tv1.tv_sec;
                took_time_ssl = took_time_ssl - prvtime;
                if(took_time_ssl  > uploadtime){
                multiples_ssl = took_time_ssl/uploadtime;
                    syslog(LOG_WARNING,"ssl offloads missed =%d since %d this offload took %d\n",multiples_ssl,prvtime,took_time_ssl);
                 prvtime = prvtime + (uploadtime * multiples_ssl);
    	}     
	}
        else {
        if((crtime-prvtime)>=uploadtime){
                prvtime = crtime;
        }
    }

	}
	//check_for_rotation(crtime);
        took_time_ssl = took_time_ssl % FIVE_MIN;
    //sched_yield();
	sleep(FIVE_MIN - took_time_ssl);
        took_time_ssl = 0;
	}
}
