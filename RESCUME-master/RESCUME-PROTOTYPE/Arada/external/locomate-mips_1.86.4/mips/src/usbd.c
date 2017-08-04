#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <syslog.h>
#include<sys/socket.h>
#include<sys/types.h>
#include<unistd.h>
#include<arpa/inet.h>
#include<sys/time.h> // for gettimeofday
#include <netinet/in.h>
#include <sys/ioctl.h>
#include <sys/select.h>
#include <unistd.h>
#include <ctype.h>
#include <math.h>
#include <errno.h>
#include <assert.h>
#include <signal.h>
#include <stdbool.h>
#include <pthread.h>
#include <getopt.h>
#include <sys/mount.h>
#include <dirent.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/syscall.h>
#include <sys/wait.h>

#include "usbd.h"

#define LOG_FILENAME_LEN 255

#define COMMAND_LEN 20
#define DATA_SIZE 1000
#define DATASIZEFW 150


struct sockaddr_in server_address;
struct sockaddr_in client_address;
fd_set readfds, testfds;
fd_set rfds;

extern int usb_connect();
extern int usb_close_sock(int usbsockfd);
static int usbsockfd = -1;
int usbsize;
sigset_t sigint_t , sigterm_t;

//sem_t rse_sem;
static pthread_t server_th, cert_th;
static void *usbd_server(void *data);
void sig_int(void);
void sig_segv(void);
void sig_term(void);
int USB_Usage(int cal_now);
int mount_usb();
int unmount_usb();

static unsigned int usbchkcnt=1;
static unsigned int usbchkthreshold=1;
static unsigned int logfilesz,radio_logfilesize,ethernet_logfilesize;

usbsock_cmd usbsock;

static char mntpath[255];
static char certprefix[255];
static char log_dest[255];
static char scp_dest[255];
char logfile[255];
int scp_status=0;
int usb_usage=0;


FILE *fd=NULL;
char file[255],logsyscmd[250];

struct linux_dirent {
	long           d_ino;
	off_t          d_off;
	unsigned short d_reclen;
	char           d_name[];
};
#define BUF_SIZE 1024
char lsbuf[BUF_SIZE];

void usage() {
    printf("\nusage: usb-deamon\n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -l:\tUSB mount path (specify path ending with /)\n");
    printf("\t -x:\tDestination folder in USB where log files can be copied (specify path ending with /)\n");
    printf("\t -s:\tDestination folder for SCP (specify path ending with /)\n");
	exit(0);
}

void Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"USB Mount", required_argument, 0, 'l'},
        {"log file USB folder", required_argument, 0, 'x'},
        {"folder for SCP", required_argument, 0, 's'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hl:x:s:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
            case 'h':
                usage();
            break;
            case 'l':
                strcpy(mntpath, optarg);
                break;

            case 'x':
                strcpy(log_dest, optarg);
            break;

            case 's':
                strcpy(scp_dest, optarg);
                strcpy(file,"NOFILE");
            break;

            default:
               usage();
            break;
        }
    }
}

int mount_usb(){
#if 0
    int ret;
	ret = system("mount -t vfat /dev/sda1 /tmp/usb");
	if((WIFEXITED(ret)) && (WEXITSTATUS(ret)==0)){
    //    syslog(LOG_INFO, "usb mount success (%d) \n", usbsock.cmd);
    	return 0;
	}
	else{ 
        syslog(LOG_INFO, "usb mount err (%d) \n", usbsock.cmd);
    	return -1;
	}
#endif
	return 0;
}

int unmount_usb(){
#if 0
    int ret;
	ret = system("umount /dev/sda1");
	if((WIFEXITED(ret)) && (WEXITSTATUS(ret)==0)){
    //    syslog(LOG_INFO, "usb mount success (%d) \n", usbsock.cmd);
    	return 0;
	}
	else{ 
        syslog(LOG_INFO, "usb un mount err (%d) \n", usbsock.cmd);
    	return -1;
	}
#endif
	return 0;
}

int list(void)
{

    static int nread=0;
    FILE *pf;
    int ret=0;
    char data[512]={0};
    bzero(logsyscmd,250);
    sprintf(logsyscmd," ls -rt %s%s",mntpath,log_dest);//copy to some temperory directory
    pf = popen(logsyscmd,"r");
        if(!pf){
            syslog(LOG_INFO,"usbd-list:Could not open pipe for output.\n");
            return -2;
        }
    nread = fread(data,sizeof(char),512, pf);
    if(nread){
        sscanf(data, "%s",file);
        ret = 0;
    }
    else{
        strcpy(file,"NOFILE");
        ret = -1;
    }
        fflush(pf);
    if (pclose(pf) < 0)
        syslog(LOG_INFO,"usbd-list:Error: Failed to close command stream %d\n");

    return ret;
#if 0    
        struct linux_dirent *d=NULL;
        static int bpos=0;
        char d_type;
        static int fd;
        static int nread=0;
        if(bpos >= nread){
           nread=bpos=0;
    	   sprintf(logsyscmd,"%s%s",mntpath,log_dest);//copy to some temperory directory
           fd = open(logsyscmd, O_RDONLY);
           if (fd == -1){
    	      syslog(LOG_INFO,"list: open err\n");
              strcpy(file,"NOFILE");
              return -1;
	   }

           nread = syscall(SYS_getdents, fd, lsbuf, BUF_SIZE);
           close(fd);
           if (nread == -1){
    	      syslog(LOG_INFO,"list: getdents err\n");
              strcpy(file,"NOFILE");
              return -1;
           }

           if (nread == 0){
    	      syslog(LOG_INFO,"list: nofiles\n");
              strcpy(file,"NOFILE");
              return -1;
           }
        }

        while(bpos < nread) {
           d = (struct linux_dirent *) (lsbuf + bpos);
           d_type = *(lsbuf + bpos + d->d_reclen - 1);
           if (d_type == DT_REG)  {
              bpos += d->d_reclen;
              strcpy(file,(char *) d->d_name);
              return 0;
           }
           bpos += d->d_reclen;
        }
        strcpy(file,"NOFILE");
        return -1;
#endif
}



int scp_filels(void){
	int ret;
	
	/*if (strcmp(file,"NOFILE")!=0){
    	sprintf(logsyscmd,"rm -f %s%s%s",mntpath,log_dest,file);//remove from usb
	    if(system(logsyscmd)<0) {
    	   syslog(LOG_INFO,"Err: %s \n",logsyscmd);
	       return -1;
    	}
	}*/
	list();
    if(strcmp(file,"NOFILE")!=0){
    bzero(logsyscmd,250);
    sprintf(logsyscmd,"%s%s%s",mntpath,log_dest,file);//copy to some temperory directory
    strcpy(file,logsyscmd);
	}
    strcpy(usbsock.fname,file);
    return 0;
}

int check_pending( int sig, char *signame ) {

    sigset_t sigset;

    if( sigpending( &sigset ) != 0 )
        perror( "sigpending() error\n" );

    else if( sigismember( &sigset, sig ) ){
            // syslog( LOG_INFO,"a %s signal is pending\n", signame );
             return 1;
        }
         else{
             //syslog( LOG_INFO,"no %s signals are pending\n", signame );
           return 0;
         }
}


int main(int argc, char*argv[])
{

    pthread_attr_t attr;
    struct sched_param param;
    int ret;
    FILE *pf_fw;
    char data[DATASIZEFW]={0};
	Options(argc ,argv);
//    sem_init(&rse_sem,0,1);
#if 0
    signal(SIGINT,(void *)sig_int);
    signal(SIGTERM,(void *)sig_term);
#endif
    signal(SIGSEGV,(void *)sig_segv);
    signal(SIGTERM,NULL);
    signal(SIGINT,NULL);
    sigemptyset(&sigint_t);
    sigemptyset(&sigterm_t);
    sigaddset(&sigint_t, SIGINT);
    sigaddset(&sigint_t, SIGTERM);
    pthread_attr_init (&attr);
    pthread_attr_setschedpolicy (&attr, SCHED_OTHER);
    pthread_attr_setschedparam (&attr, &param);
    pthread_attr_setinheritsched (&attr, PTHREAD_INHERIT_SCHED);


   pthread_sigmask(SIG_BLOCK, &sigint_t, NULL);
   pthread_sigmask(SIG_BLOCK, &sigterm_t, NULL);
   pf_fw = popen("uname -a","r");
    if(pf_fw) {
        
        fread(data,sizeof(char),DATASIZEFW, pf_fw);
        //sscanf(data, "%s",fwver);
        syslog(LOG_INFO, "Firmware Version  %s",data);
    }       
    else    
        syslog(LOG_INFO, "Could not open pipe [usbd FW]\n");
    if (pclose(pf_fw) != 0)
        syslog(LOG_INFO," Error: Failed to close command stream [usbd FW]\n");



   pf_fw = popen("/bin/grep -i radio-logfilesize /var/config | awk '{print $2}'","r");
    if(pf_fw) {
    char tmp[50] = {0};
    fscanf(pf_fw, "%s",tmp);
    radio_logfilesize = atoi(tmp);
      //  syslog(LOG_INFO, "usbd radio_logfilesz=  %d",radio_logfilesize);
    }
    else
        syslog(LOG_INFO, "Could not open pipe [usbd RL]\n");
    if (pclose(pf_fw) != 0)
        syslog(LOG_INFO," Error: Failed to close command stream [usbd RL]\n");



   pf_fw = popen("/bin/grep -i ethernet-logfilesize /var/config | awk '{print $2}'","r");
    if(pf_fw) {
    char tmp[50] = {0};
    fscanf(pf_fw, "%s",tmp);
    ethernet_logfilesize = atoi(tmp);
        //syslog(LOG_INFO, "usbd ethernet_logfilesz=  %d",ethernet_logfilesize);
    }
    else
        syslog(LOG_INFO, "Could not open pipe [usbd EL]\n");
    if (pclose(pf_fw) != 0)
        syslog(LOG_INFO," Error: Failed to close command stream [usbd EL]\n");
    if (ethernet_logfilesize != radio_logfilesize )
        logfilesz = (ethernet_logfilesize > radio_logfilesize ) ? ethernet_logfilesize : radio_logfilesize;
    else
        logfilesz = radio_logfilesize;

    (void)syslog(LOG_INFO, "Starting Usbd application \n");
    ret = pthread_create(&server_th, NULL, usbd_server, NULL );
    sched_yield();

         usbchkcnt = 1;
         usbchkthreshold = 1;
         usb_usage=USB_Usage(1);
         if(usb_usage >= 0)
         {
         usbchkthreshold =  (0.02 * usbsize)/logfilesz; //2% threshold
	 usbchkthreshold = usbchkthreshold + 1;  //To avoid frequent USB_Usage calls
         syslog(LOG_INFO,"USB Usbchkthreshold %d logfilesz=%d",usbchkthreshold,logfilesz);
         if(usbchkthreshold == 0)
             usbchkthreshold = 1;
         usbchkcnt = 1;
        }
        else {
            usbchkthreshold = 1;
            usbchkcnt = 1;
        }
    pthread_join(server_th, NULL);
    sig_int(); 
    return 0;

}
int server_sockfd;
void *usbd_server(void *data)
{
	int client_sockfd;
    int server_len; 
    socklen_t client_len;
    int result;static int file_name =1 ;
    int logcpy_ret,ethlogcpy_ret;	
 //   char ch;
    int fd;
    int nread;
    uint8_t spacetofree;
    int actual_spacetofree;
    int no_of_files,i=0;
    int ret_sigterm,ret_sigint;
    struct timeval timeout;
    timeout.tv_sec = 1;
    timeout.tv_usec = 0;
	 
/*  Create and name a socket for the server.  */

	printf("in usbd_server\n");
    server_sockfd = socket(AF_INET, SOCK_STREAM, 0);

    server_address.sin_family = AF_INET;
    server_address.sin_addr.s_addr = htonl(INADDR_ANY);
    server_address.sin_port = htons(6666);
    server_len = sizeof(server_address);

    if(bind(server_sockfd, (struct sockaddr *)&server_address, server_len)<0){
	    syslog(LOG_INFO,"bind failed:usbd\n");
	    exit(-1);
    }

/*  Create a connection queue and initialize readfds to handle input from server_sockfd.  */

    listen(server_sockfd, 5);

    FD_ZERO(&readfds);
    FD_SET(server_sockfd, &readfds);

/*  Now wait for clients and requests.
 *      Since we have passed a null pointer as the timeout parameter, no timeout will occur.
 *          The program will exit and report an error if select returns a value of less than 1.  */
    //if(mount_usb()<0){
    //    syslog(LOG_INFO,"mounting failed. (%d) \n", usbsock.cmd);
    //    strcpy(usbsock.fname,"MNTERROR");                                         
    //}

    while(1) {
     ret_sigterm = check_pending( SIGTERM, "SIGTERM" );
     ret_sigint = check_pending( SIGINT, "SIGINT" );
     if((ret_sigterm == 1) || (ret_sigint == 1))
     break;
        testfds = readfds;

        result = select(FD_SETSIZE, &testfds, (fd_set *)0,
            (fd_set *)0, &timeout);
	    timeout.tv_sec = 1;
            timeout.tv_usec = 0;

        if(result == 0)
	    continue;
	else if(result < 0) {
            syslog(LOG_INFO,"USBD:select error");
            exit(1);
        }

/*  Once we know we've got activity,
 *      we find which descriptor it's on by checking each in turn using FD_ISSET.  */

        for(fd = 0; fd < FD_SETSIZE; fd++) {
            if(FD_ISSET(fd,&testfds)) {

/*  If the activity is on server_sockfd, it must be a request for a new connection
 *      and we add the associated client_sockfd to the descriptor set.  */
               if(fd == server_sockfd) {
                    client_len = sizeof(client_address);
                    client_sockfd = accept(server_sockfd,
                        (struct sockaddr *)&client_address, &client_len);
                    FD_SET(client_sockfd, &readfds);
                   // printf("adding client on fd %d\n", client_sockfd);
                }

/*  If it isn't the server, it must be client activity.
 *      If close is received, the client has gone away and we remove it from the descriptor set.
 *          Otherwise, we 'serve' the client as in the previous examples.  */

                else {
                    ioctl(fd, FIONREAD, &nread);

                    if(nread == 0) {
                        close(fd);
                        FD_CLR(fd, &readfds);
                     //   printf("removing client on fd %d\n", fd);
                    }

                    else {
			memset(&usbsock,0,sizeof(usbsock));
                      	read(fd, &usbsock, sizeof(usbsock));
               			switch(usbsock.cmd)
                                {
                                    case LOGCPY:
#if 0
    					if(mount_usb()<0){
					    syslog(LOG_INFO,"mounting failed. (%d) \n", usbsock.cmd);
					    strcpy(usbsock.fname,"MNTERROR");                                         
              				    write(fd,&usbsock,sizeof(usbsock));
      	     				}
					else 
#endif
                                        {
					    if(file_name == 1){
					    system("mkdir -p /tmp/usb/ModelDeploymentPktCaptures/");
					    file_name =0 ;
					    }
					    strcpy(logfile,usbsock.fname);
					    if(!strcmp(logfile,"/tmp/wlan_capture/"))
					    {
                                                bzero(logsyscmd,250);
					        sprintf(logsyscmd,"%s %s%s","mv -f /tmp/wlan_capture/*",mntpath,log_dest);
					        logcpy_ret = system(logsyscmd);	
					    }
					    else
					    {		
                                                bzero(logsyscmd,250);
					        sprintf(logsyscmd, "mv -f %s %s%s.",logfile,mntpath,log_dest);
					        logcpy_ret = system(logsyscmd);	
					    }	
					    if((WIFEXITED(logcpy_ret)) && (WEXITSTATUS(logcpy_ret)!=0)) {
	    				        syslog(LOG_INFO, "LOGCPY Err:[%d] %s \n",WEXITSTATUS(logcpy_ret),logsyscmd);
					    	strcpy(usbsock.fname,"MNTERROR");                                         
#if 0
        				    	if(unmount_usb()<0){
	    						syslog(LOG_INFO,"unmounting failed. (%d)\n",usbsock.cmd);
					    		strcpy(usbsock.fname,"MNTERROR");                                         
        				    	}
					    }
        				    else {
						if(unmount_usb()<0){
	    						syslog(LOG_INFO,"unmounting failed. (%d)\n",usbsock.cmd);
                                                	strcpy(usbsock.fname,"MNTERROR");
        				    	}
				/*	    	else {
					    		usb_usage = USB_Usage();
					    		if(usb_usage == -1) 
					    		{
                                                	    strcpy(usbsock.fname,"MNTERROR");
					    		}		
					    	}*/
#endif
					    }
                                            write(fd,&usbsock,sizeof(usbsock));
                                        }
                                    break;
                                    case ETH_LOGCPY:						
#if 0
    					if(mount_usb()<0){
					    syslog(LOG_INFO,"mounting failed. (%d) \n", usbsock.cmd);
					    strcpy(usbsock.fname,"MNTERROR");                                         
              				    write(fd,&usbsock,sizeof(usbsock));
      	     				}
					else
#endif 
                                        {
					    if(file_name == 1){
					    system("mkdir -p /tmp/usb/ModelDeploymentPktCaptures/");
					    file_name =0 ;
					    }
					    strcpy(logfile,usbsock.fname);
					    if(!strcmp(logfile,"/tmp/eth_capture/"))
					    {
                                                bzero(logsyscmd,250);
					        sprintf(logsyscmd,"%s %s%s","mv -f /tmp/eth_capture/*",mntpath,log_dest);
					        ethlogcpy_ret = system(logsyscmd);	
					    }
					    else
					    {		
                                                bzero(logsyscmd,250);
					        sprintf(logsyscmd, "mv -f %s %s%s.",logfile,mntpath,log_dest);
					        ethlogcpy_ret = system(logsyscmd);	
					    }	
					    if((WIFEXITED(ethlogcpy_ret)) && (WEXITSTATUS(ethlogcpy_ret)!=0)) {
	    				        syslog(LOG_INFO, "LOGCPY Err:[%d] %s \n",WEXITSTATUS(ethlogcpy_ret),logsyscmd);
					    	strcpy(usbsock.fname,"MNTERROR");
#if 0
        				    	if(unmount_usb()<0){
	    						syslog(LOG_INFO,"unmounting failed. (%d)\n",usbsock.cmd);
					    		strcpy(usbsock.fname,"MNTERROR");                                         
        				    	}
					    }
        				    else {
						if(unmount_usb()<0){
	    						syslog(LOG_INFO,"unmounting failed. (%d)\n",usbsock.cmd);
                                                	strcpy(usbsock.fname,"MNTERROR");
        				    	}
					    /*	else {
					    		usb_usage = USB_Usage();
					    		if(usb_usage == -1) 
					    		{
                                                	    strcpy(usbsock.fname,"MNTERROR");
					    		}		
					    	}*/
#endif
					    }
                                            write(fd,&usbsock,sizeof(usbsock));
					    
                                        }	

				    break;
										
                                    case SCPCPY:						
    					scp_status=usbsock.ret;
#if 0
    					if(mount_usb()<0){
					    syslog(LOG_INFO,"mounting failed. (%d) \n", usbsock.cmd);
					    strcpy(usbsock.fname,"MNTERROR");                                         
                        		    write(fd,&usbsock,sizeof(usbsock));
					    break;
      	     			        }
#endif
                                        scp_filels();
#if 0
        				if(unmount_usb()<0){
	    					syslog(LOG_INFO,"unmounting failed. (%d)\n",usbsock.cmd);
					    strcpy(usbsock.fname,"MNTERROR");                                         
        				}
#endif
                                        write(fd,&usbsock,sizeof(usbsock));
                                    //    usb_usage = USB_Usage();
                                       break;

				    case USBCALUSAGE:
				    // calculate and return usbusage for apps modifying USB
				    // write() for status
					usb_usage = USB_Usage(usbsock.ret);
                                        if(usb_usage == -1) 
					    strcpy(usbsock.fname,"MNTERROR");
					else
						memset(usbsock.fname,'\0',sizeof(usbsock.fname));						
					usbsock.ret = usb_usage;
					usbsock.cmd = scp_status;
					write(fd,&usbsock,sizeof(usbsock));
                                        break;
				    case USBUSAGE:
				    // return usb usage in %.for apps just wanting to know the status.
				    // write() for status
                                       if(usb_usage == -1) 
					    strcpy(usbsock.fname,"MNTERROR");
					else
						memset(usbsock.fname,'\0',sizeof(usbsock.fname));						
					usbsock.ret = usb_usage;
					usbsock.cmd = scp_status;
					write(fd,&usbsock,sizeof(usbsock));
					break;

                    		    case USBPURGE:
#if 0
                        	        if(mount_usb()<0){
                            		    syslog(LOG_INFO,"USBPURGE :mounting failed. (%d) \n", usbsock.cmd);
                            		    break;
                        		}
#endif
                       			spacetofree = (usbsock.ret > 10) ? usbsock.ret : 10;
                       			actual_spacetofree = usbsize * spacetofree / 100; //percentage to MB depending upon
                       			no_of_files = actual_spacetofree / logfilesz;
	    			   	syslog(LOG_INFO,"USBPURGE started -- trying to free %d%% files%d \n",spacetofree,no_of_files);
                        
                        		for(i=0 ; i < no_of_files ; i++)
                        		{
                           		    list(); //get oldest files
                                            bzero(logsyscmd,250);
                           		    sprintf(logsyscmd,"rm -f %s%s%s",mntpath,log_dest,file);//remove from usb
                           		    if(system(logsyscmd)<0) {
                            		        syslog(LOG_INFO,"USBPURGE Err: %s \n",logsyscmd);
                               			return -1;
                           		    }
                        		} 
                        		bzero(file,255);                        
                        		//system("sync; echo 3 > /proc/sys/vm/drop_caches"); 
                        		system("sync"); 
#if 0
                        		if(unmount_usb()<0){
                            		    syslog(LOG_INFO,"USBPURGE:unmounting failed. (%d)\n",usbsock.cmd);
                        		}
#endif
                        		usbchkcnt = 0 ; // to update usb_usage
	    				syslog(LOG_INFO,"USBPURGE exit -- calculating usage...\n");
                        		usb_usage = USB_Usage(1);
                    			break; 

                                    default:
                                        syslog(LOG_INFO,"Unknown Command");
                                    break;

                                }
                   
                    }
                }
            }
        }
        sched_yield();
    }
	return 0;
}

void sig_int(void)
{
    //sem_destroy(&rse_sem);
    pthread_cancel(server_th);
    if(server_sockfd >0 )
       close(server_sockfd); 
      //  unmount_usb();
   (void)syslog(LOG_INFO, "Closing Usbd Application \n");
   pthread_sigmask(SIG_UNBLOCK, &sigint_t, NULL);
   pthread_sigmask(SIG_UNBLOCK, &sigterm_t, NULL);
	
}

void sig_term(void)
{
    sig_int();
}
void sig_segv(void)
{
    sig_int();
}

int USB_Usage(int cal_now)
{
    FILE *pf = NULL;
    char command[COMMAND_LEN]={0};
    char data[DATA_SIZE]={0},*copy = NULL;
    char tmpstr1[10],tmpstr2[10],tmpstr3[10],tmpstr4[10],tmpstr5[10],tmpstr6[10],tmpstr7[10],tmpstr8[10],tmpstr9[10],tmpstr10[10],tmpstr11[10],tmpstr12[10] ={0};
    int usageval = usb_usage;
    if (usbchkcnt % usbchkthreshold == 0 || cal_now == 1) {
#if 0
        if(mount_usb()<0){
	    syslog(LOG_INFO,"usbusage:mounting failed. (%d) \n", usbsock.cmd);
	    return -1;
        }
#endif
         //system("sync; echo 3 > /proc/sys/vm/drop_caches"); 
         system("sync"); 
         sprintf(command, "df -k /dev/sda1");
         pf = popen(command,"r");
                if(!pf){
                       syslog(LOG_INFO, "Could not open pipe for output.\n");
                       return -2;
                       }
         fread(data,sizeof(char),DATA_SIZE , pf);
         sscanf(data, "%s %s %s %s %s %s %s %s %s %s %s %s", tmpstr1, tmpstr2, tmpstr3 ,tmpstr4,
         tmpstr5,tmpstr6,tmpstr7,tmpstr8,tmpstr9,tmpstr10,tmpstr11,tmpstr12);
         copy=strtok(tmpstr12," ");
         if(isdigit(tmpstr12[0]))
            usageval =atoi(copy);
         else{
            if (pclose(pf) < 0)
                syslog(LOG_INFO," Error: Failed to close command stream \n");
            return -1;   
         }
	 //syslog(LOG_INFO,"USB Usage in 1k-blocks (%s/%s)",tmpstr10,tmpstr9);
         usbsize = atoi(tmpstr9) / 1024;  
      /*   if(usbchkthreshold == 1 ) {      //first time 
            usbchkthreshold =  (0.02 * usbsize)/logfilesz; //2% threshold
	 syslog(LOG_INFO,"USB Usbchkthreshold %d logfilesz=%d",usbchkthreshold,logfilesz);
         if(usbchkthreshold == 0)
             usbchkthreshold = 1;
         usbchkcnt = 1;
         }*/
         if (pclose(pf) != 0)
            syslog(LOG_INFO," Error: Failed to close command stream \n");
#if 0
        if(unmount_usb()<0){
	    syslog(LOG_INFO,"usbusage:unmounting failed.\n");
	    return -1;
        }
#endif
        syslog(LOG_INFO,"usbusage:%d%% -%d-\n", usageval,usbchkcnt);
    }
    usbchkcnt++;
    return usageval ;
}

