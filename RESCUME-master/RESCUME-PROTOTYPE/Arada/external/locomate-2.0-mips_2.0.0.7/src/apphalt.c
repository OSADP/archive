#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <syslog.h>
#include <string.h>
#include <signal.h>
#include <stdint.h>
#include <sys/wait.h>
#ifdef SDK_NEW
#define WPS_OFF 8
#endif

void terminate_apps(char *appname,int forcekill)
{
    int ret=-1;
    uint8_t first_time = 0;
    char cmd[64]="";
 
    if(strcmp(appname,"")) {
	if(forcekill == 0)
	    sprintf(cmd, "killall -q  %s",appname);	
	else
	    sprintf(cmd, "killall -q -9 %s",appname);
        ret=system(cmd);
    	}
    if((WIFEXITED(ret)) && (WEXITSTATUS(ret) == 0))
      {
	sprintf(cmd, "pgrep -l -x %s > /dev/null",appname);
        while(1)
        {
            ret = system(cmd);
    	    if((WIFEXITED(ret)) && (WEXITSTATUS(ret) != 0))
            {
	        printf(" %s Succesfully Terminated \n",appname);
                break;
            }
            else
            {
                if(first_time == 0)
                {
		    if(strcmp(appname,"usbd") == 0){
			printf("Please wait while terminating %s [Termination can take time if USB-PURGE is going on]\n",appname);
		    }
		    else
			printf("Please wait while terminating %s \n",appname);
                    first_time = 1;
                }
            }
           usleep(100000);
       }
    }
}

int main(int argc,char *argv[])
{
    int i,status,UmntSts=1,LcmKillSts=1;
    char name[64]={0},tmp_buf[100]={0},appname[20]={0},tmpstr[80]={0};
    char *str_end,cmd[30]="pidof quiethandler";
    FILE *status_ptr,*appname_ptr,*fp;
    uint16_t pid1=0,pid2=0;    

    fp = popen(cmd,"r");                 //This test  avoids the execution
    if(fp != NULL)                       //of 2 quiethandlers when user press
        fscanf(fp,"%hu %hu",&pid1,&pid2);//sequentially IGNITION OFF, ON 
    else
        printf("popen failed\n");
    
    if(pid1 && pid2)
    {
        close(fp);
        exit(0);
    }
    if(fp)
        close(fp);
	if(argc >= 3)   
 	   LcmKillSts=atoi(argv[2]);

	if(argc >= 4)   
	   UmntSts=atoi(argv[3]);

    for(i=1; i <= 8; i++) {
        sprintf(name,"conf_get system:applicationSettings:app%dStatus",i);
        status_ptr = popen(name,"r");
        fgets(tmp_buf,90,status_ptr);
        sscanf(tmp_buf, "%s %d", tmpstr, &status);
        pclose(status_ptr);
        if(status)
        {
            sprintf(name,"conf_get system:applicationSettings:app%dName",i);
            appname_ptr = popen(name,"r");
            fgets(tmp_buf,90,appname_ptr);
            sscanf(tmp_buf, "%s %s", tmpstr, appname);
            pclose(appname_ptr);
            str_end = strrchr(appname, '/');
            terminate_apps(str_end + 1,0);
        }
    }
    system("killall -q capture_app");
    if(argv[1] != NULL && ((strcmp(argv[1], "eventlog") == 0)||(strcmp(argv[1], "powercycle") == 0)))
	terminate_apps("Asm.bin",1);
    else	
	terminate_apps("Asm.bin",0);
    if(LcmKillSts == 1)
    	terminate_apps("lcmd",0);
    terminate_apps("eth_app",0);
    terminate_apps("auto_off_load",0);
    terminate_apps("hbrtc",0);
    terminate_apps("capture_app",0);
    terminate_apps("usbd",0);
    system("rm -f /tmp/udp_*");
    terminate_apps("application",0);

    if(argv[1] != NULL && ((strcmp(argv[1], "eventlog") == 0)||(strcmp(argv[1], "powercycle") == 0)))	
    {
        syslog(LOG_INFO,"APHALT interrupt IGN_OFF\n");
        syslog(LOG_INFO,"ST,\t Quiet,\n");
        syslog(LOG_INFO,"RM,\t Unmounted,\n");
        system("/usr/local/bin/system_event_log");
        system("nohup umount /tmp/usb > /dev/null");
        system("nohup umount /dev/sda1 > /dev/null");
        system("rm -f /tmp/SystemEvents.log");
        system("rm -f /tmp/first_time.txt");
        /* to resume the transmission if the ignition is ON again before power cut after ignition-OFF */
	if(strcmp(argv[1], "powercycle") != 0){
        sleep(16);
        system("/usr/local/bin/application < /var/config > /dev/null");
	}
    } else {
        syslog(LOG_INFO,"ST,\t Halt,\n");
        if(UmntSts == 1)
	{	
	  syslog(LOG_INFO,"RM,\t Unmounted,\n");
	  system("nohup umount /tmp/usb > /dev/null");
	  system("nohup umount /dev/sda1 2> /dev/null");
  	}  
    }
#ifdef SDK_NEW
    system("/usr/bin/panel_led WPS_OFF > /dev/null 2>&1 ");
#endif
    return 0;
}
