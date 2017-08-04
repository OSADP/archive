/*
 * Application to generate a system event log file from /var/log/messages 
 * Events like mount USB, transition between the states of run, quite, halt etc., and
 * umount USB will be captured.
*/

#include <stdio.h>
#include <ctype.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/time.h>
#include <sys/syslog.h>


#define swap32_(x) (((x & 0xff)<<24)|((x>>24) & 0xff)|((x & 0x0000ff00)<<8)|((x & 0x00ff0000) >>8))

#define swap16_(x)       ((((x)&0x00FF)<<8) | (((x)>>8)&0x00FF))

#define TRUE 1
#define FALSE 0

char read_file[50];
char msg_file[50]="/var/log/messages";
char tmp_event_file[]="/var/log/sysevents_nodevid";
char temp_file[20] = "0";
char write_file[]="/tmp/SystemEvents.log";
char conf_file[]="/tmp/usb/ModelDeploymentConfigurationItems/ModelDeploymentRemovable.conf";
char usb_conf_file[]="ModelDeploymentConfigurationItems/ModelDeploymentRemovable.conf";
char log_file[50], usb_path[]="/tmp/usb/";
char system_log_dir[]="ModelDeploymentSystemEventLogs/";
char write_line[200];
static int year = 0;
static int found = 0;
static int devid_present = TRUE;
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

void parse_and_rearrange(char *string) 
{
    char *token = NULL, *str = NULL, tmp_str1[100] = "", tmp_buf[100] = "",tmp[50] = "";
    int month_num=0, date = 0, hour = 0 , min = 0, sec = 0, i = 0;

    str = string;
    token = strtok(str," ");		//month
    for(i=1; i <= 12; i++)
    {
    	if(!strcmp(mon[i-1],token))
	    month_num = i;
    }

    token = strtok(NULL," ");		//date
    date=atoi(token);

    token = strtok(NULL,":");		//hour
    hour=atoi(token);

    token = strtok(NULL,":");		//min
    min=atoi(token);

    token = strtok(NULL," ");		//sec
    sec=atoi(token);
	
    sprintf(tmp_buf, " %04d-%02d-%02dT%02d:%02d:%02dZ", year, month_num, date, hour, min, sec);
    token = strtok(NULL, ":");
    token = strtok(NULL, ",");
    sprintf(tmp_str1,"%s,",token);
    token = strtok(NULL, ",");
    strcat(token,",");
    sprintf(tmp,"%-20s",token);
    strcat(tmp_str1, tmp);
    strcat(tmp_str1, tmp_buf);
    strcat(tmp_str1, "\n");
    memcpy(write_line, tmp_str1, strlen(tmp_str1)); 
    return;	
}

/* file name format "Deviceid_Configurationid_UTCTimestamp" */
void create_file_name(void)
{
    char *token = NULL, *str = NULL;
    int month = 0, day = 0, hour = 0, min = 0, sec = 0, i = 0;
    struct timeval tv;
    FILE *rdfd;
    char read_line[200], *status = NULL;
    uint32_t model_deployment_deviceid = 0; /* four bytes*/
    uint16_t Device_id = 0, Conf_id = 0;  /* lower two bytes of model deployment device id are for "device id" and upper two bytes are for "configuration id" */


    rdfd = fopen(conf_file, "r");
    if (rdfd == NULL) {
    	printf("Error opening %s file\n", conf_file);
	strcpy(log_file, tmp_event_file);
	devid_present = FALSE;
    	syslog(LOG_INFO,"Error opening in ModelDeploymentRemovable.conf file, Generating  System EventLogs in /var/log\n");
        //exit(0);
    }
  if(devid_present == TRUE) {
    memset(read_line, 0, sizeof(read_line));
    while( (status = fgets(read_line, sizeof(read_line), rdfd)) != NULL ) {
        if (read_line[0] != '#' && read_line[0] != ';' && read_line[0] != ' '){
	    token = strtok(read_line, "=");
	    if (strcasecmp(token, "ModelDeploymentDeviceID") == 0) {
                token = strtok(NULL, " ");
		//model_deployment_deviceid = atoi(token);	
		sscanf(token,"0x%x",&model_deployment_deviceid);
            }
	    else 
                token = strtok(NULL, " ");
	} /* if */
    	memset(read_line, 0, sizeof(read_line));
    } /* while */
    
    /* to separate device id and configuration id */
    memcpy(&Conf_id, &model_deployment_deviceid, sizeof(uint16_t));
    model_deployment_deviceid = swap32_(model_deployment_deviceid);
    memcpy(&Device_id, &model_deployment_deviceid, sizeof(uint16_t));
    Device_id = swap16_(Device_id);

    fclose(rdfd);
  }

    /* to get the UTC time stamp*/
    gettimeofday(&tv, NULL);
    str = ctime(&tv.tv_sec);

    token = strtok(str," ");                //week_day        

    token = strtok(NULL," ");               //month
    for(i=1; i <= 12; i++)
    {
        if(!strcmp(mon[i-1],token))
            month = i;
    }
    
    token = strtok(NULL," ");               //date
    day = atoi(token);

    token = strtok(NULL,":");               //hour
    hour = atoi(token);

    token = strtok(NULL,":");               //min
    min = atoi(token);

    token = strtok(NULL," ");               //sec
    sec = atoi(token);

    token = strtok(NULL," ");               //year
    year = atoi(token);

  if(devid_present == TRUE) 
    sprintf(log_file,"%x_%x_%04d-%02d-%02d_%02d%02d%02d",Device_id, Conf_id, year, month, day, hour, min, sec);
     /* may we need this logic in future */
    //sprintf(temp_file,"%s_%04d%02d%02d_%02d%02d", read_file, year, month, day, hour, min);
    
    return;	
} /* create_file_name */

int main(void)
{
    FILE *fdrd = NULL, *wrfd = NULL, *tmpfd = NULL;
    char read_line[200];
    char *token = NULL, *rdsts;
    int sts, start_event_log = 0;
    char buf[150];
    char tmp_str[100], mnth[5] = "0", date[50] = "0";
    
    create_file_name();
    /* open the file in which we have written the mount_time on bootup in read mode */
    fdrd = fopen("/tmp/first_time.txt", "r");
    if (fdrd == NULL) {
    	//printf("Error opening /tmp/first_time.txt file\n");
	/* exit(0); should we exit????? */
    } else {
    	fgets(read_line, sizeof(read_line), fdrd);
    	fclose(fdrd);
	token = strtok(read_line, " ");
	if(token != NULL)
	    strcpy(mnth, token);
	token = strtok(NULL, " ");
	if(token != NULL)
	    strcpy(date, token);
	strcat(date, " ");
	token = strtok(NULL, "\n");
	if(token != NULL)
	    strcat(date, token);
    }	

    fdrd = popen("ls -c /var/log/messages_*", "r");
    if(fdrd != NULL)
    {
	fscanf(fdrd, "%s", read_file);
	pclose(fdrd);
	if(!strcmp(read_file, ""))
	    strcpy(read_file, msg_file);
    } 
again:    
    /* open the config file in read mode */
    fdrd = fopen(read_file, "r");
    if (fdrd == NULL) {
    	printf("Error opening %s file\n", read_file);
        exit(0);
    }
    
    /* open a temp config file in write mode */
    wrfd = fopen(write_file, "a+");
    if (wrfd == NULL) {
	fclose(fdrd);
    	printf("Error opening %s file\n", write_file);
        exit(0);
    }

    memset(tmp_str, 0, sizeof(tmp_str));
  if(!found) {
    sprintf(tmp_str,"grep \"%s\" %s", date, read_file);
    tmpfd = popen(tmp_str,"r");
    if(tmpfd != NULL) {
    	memset(tmp_str, 0, sizeof(tmp_str));
	fgets(tmp_str,sizeof(tmp_str),tmpfd);	
	found = 1;
	pclose(tmpfd);
    }
  }
    if(!strcmp(tmp_str, ""))
    {
	if(strcmp(read_file, msg_file)){
            fclose(fdrd);
    	    fclose(wrfd);
	    strcpy(read_file, msg_file);
	    goto again;
	} 
    }
    memset(tmp_str, 0, sizeof(tmp_str));
    memset(read_line, 0, sizeof(read_line));
    memset(write_line, 0, sizeof(write_line));
    while( (rdsts = fgets(read_line, sizeof(read_line), fdrd)) != NULL ) {
	memcpy(write_line, read_line, sizeof(read_line));
	if(!start_event_log && (strstr(read_line, date) != NULL) && (strstr(read_line, mnth) != NULL))	
	{
	    start_event_log = 1;
	}
	token = NULL;
	if(start_event_log){
	    token = strtok(read_line, ",");
	    if(token == NULL)
		break;
	    if (strstr(token, "RM") != NULL) {
	        //sprintf(write_line,"%s", token);
	    	//printf(" %s *** %s \n",write_line,token);
    	    	memset(tmp_str, 0, sizeof(tmp_str));
	    	if (strstr(token, "syslog") != NULL) {
		    sprintf(tmp_str,"%s,", token);
		    token = strtok(NULL, ",");
		    if(token != NULL) {
		        strcat(tmp_str, token);
    			memset(write_line, 0, sizeof(write_line));
		    	parse_and_rearrange(tmp_str);
	    	    	sts = fputs(write_line,wrfd);
	      	    }
	        }
	    }
	    if (strstr(token, "CL") != NULL) {
	    	//sprintf(write_line,"%s=", token);
	    	//printf(" %s *** %s \n",write_line,token);
    	    	memset(tmp_str, 0, sizeof(tmp_str));
	    	if (strstr(token, "syslog") != NULL) {
		    sprintf(tmp_str,"%s,", token);
		    token = strtok(NULL, ",");
		    if(token != NULL) {
		    	strcat(tmp_str, token);
    			memset(write_line, 0, sizeof(write_line));
		    	parse_and_rearrange(tmp_str);
	    	    	sts = fputs(write_line,wrfd);
	      	    }
	        }
	    }
	    if (strstr(token, "ST") != NULL) {
	    	//sprintf(write_line,"%s=", token);
	    	//printf(" %s *** %s \n",write_line,token);
    	    	memset(tmp_str, 0, sizeof(tmp_str));
	    	if (strstr(token, "syslog") != NULL) {
		    sprintf(tmp_str,"%s,", token);
		    token = strtok(NULL, ",");
		    if(token != NULL) {
		    	strcat(tmp_str, token);
    			memset(write_line, 0, sizeof(write_line));
		    	parse_and_rearrange(tmp_str);
	    	    	sts = fputs(write_line,wrfd);
	      	    }
	        }
	    }
	    memset(read_line, 0, sizeof(read_line));
    	    memset(write_line, 0, sizeof(write_line));
    	}
    }
    fclose(fdrd);
    fclose(wrfd);

    if(!strcmp(read_file, msg_file))
    {
	;
    } else {
	strcpy(read_file, msg_file);
	goto again;
    }
    if(strcmp(log_file, tmp_event_file)) {
        fdrd = fopen(tmp_event_file, "r");    
    	if(fdrd != NULL)
    	{
    	    fgets(read_line, sizeof(read_line), fdrd);
	    if(strcmp(read_line, "")) {
    	        sprintf(buf,"cat %s >> /tmp/%s",tmp_event_file, log_file);    
	        system(buf);
		sprintf(buf, "rm -f %s", tmp_event_file);
	        system(buf);
	    }
    	}
    	sprintf(buf,"cat %s >> /tmp/%s", write_file, log_file);    
	system(buf);
    }
    else {
    	sprintf(buf,"cat %s >> %s", write_file, tmp_event_file);    
    	//sprintf(buf,"mv -f %s %s",write_file, log_file);    
    	if (system(buf) < 0) {
	    syslog(LOG_INFO, "Err: %s failed\n",  buf);
	    return -1;
        }
    }
/*    sprintf(buf,"umount /dev/sda1");
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }
*/    
    /*sprintf(buf,"mount -t vfat /dev/sda1 %s",usb_path);
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }*/

    sprintf(buf,"mkdir -p  %s%s",usb_path,system_log_dir);
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }

    //copy system event log file to USB
  if(devid_present == TRUE) {
    sprintf(buf,"cp -f /tmp/%s %s%s%s", log_file,usb_path,system_log_dir,log_file);    
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }
    //copy conf_file with mount time to USB 
    sprintf(buf,"cp -f /tmp/temp.conf %s%s",usb_path,usb_conf_file);   
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }
  }
    /*system("sync"); 
    sprintf(buf,"umount /dev/sda1");
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }*/
/*
    sprintf(buf,"mv -f %s %s",read_file,temp_file);
    if (system(buf) < 0) {
	syslog(LOG_INFO, "Err: %s failed\n",  buf);
	return -1;
    }
*/
    return 0;
}
