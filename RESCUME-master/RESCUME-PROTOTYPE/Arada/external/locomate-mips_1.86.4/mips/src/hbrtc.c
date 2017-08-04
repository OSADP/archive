#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <stdio.h>
#include <unistd.h>
#include <errno.h>
#include <string.h>
#include <stdlib.h>
#include <sys/syslog.h>
#include <signal.h>

extern void get_HBT_options(char *);
int invokeIPServer(void);
extern void getpayload(void);	               
extern void usbpurge(void);	               
extern void get_logconf(void);
void sig_conf(void);

static struct sockaddr_in6 server_addr6;
static struct sockaddr_in server_addr;
extern uint32_t start_utctime_sec,stop_utctime_sec,payload_size;
extern uint32_t pktdelaysec,threshdelaysec;
char config_file[40] ;//heartbeat.conf file
extern int port,tcount;
extern int usb_usage;
extern char ip[25];static int i;
static int ipaddrlen = 0;
extern char msgpld[100];
extern int halt,t2,threshold2,threshold3;


int invokeIPServer(void)
{
                int ret,ipsock;

                ret = inet_pton(AF_INET, ip, &server_addr.sin_addr);

                if( ret == 1 )
                {
                        
                        if ((ipsock=socket(AF_INET, SOCK_DGRAM, 0))==-1)
                        {

                                perror("socket");
                                exit(1);

                        }

                	server_addr.sin_family = AF_INET;
                	server_addr.sin_port = htons(port);
              	        bzero(&(server_addr.sin_zero),8);
                	i=1;
                
		return ipsock;
                }

		else
                {
                        ret = inet_pton(AF_INET6, ip, &server_addr6.sin6_addr);
                if( ret == 1 )
                {
                        
 			ipsock = socket(AF_INET6, SOCK_DGRAM, 0);
                        
			if (ipsock < 0)
                        return -1;

                        if (connect(ipsock, (struct sockaddr *)&server_addr6, sizeof(server_addr6)) < 0) {
                        perror("rlogin: connect");
                        exit(5);
                        }

                        if(setsockopt(ipsock, SOL_SOCKET, SO_REUSEADDR,(char *)&ret, sizeof(ret ) ) < 0 )
                        {
                                perror("setsockopt(SO_REUSEADDR) failed");
                        }

                        server_addr6.sin6_family = AF_INET6;
                        server_addr6.sin6_port = htons(port);
                        ipaddrlen = sizeof(struct sockaddr_in6);
                	i=2;
                return ipsock;
                }
                }        

  
  }


int main()
{

        static struct timeval tv;
        int sock,result,retval,tx_sts=1,tx_tset=1,tc;
	signal(SIGUSR1,(void *)sig_conf);
	strcpy(config_file,"/var/heartbeat.conf");
        get_HBT_options(config_file);
	get_logconf();
	

   sock=invokeIPServer();
   

   while (1)
   {

	tc=tcount;
        gettimeofday(&tv, NULL);
              

        if((tv.tv_sec >= start_utctime_sec) && (tv.tv_sec <= stop_utctime_sec)){
		
		if(tx_tset==0)
                        syslog(LOG_INFO,"Heartbeat msg tx resumes for time in the range \n");
                        tx_tset=1;

		getpayload();	               

		if(halt==0){
			
			if(tx_sts==0)
			syslog(LOG_INFO,"Heartbeat msg tx resume :: usage %d%% < (th3 %d%%) \n", usb_usage,threshold3);
			else if(tx_sts==2)
			syslog(LOG_INFO,"Heartbeat msg tx resume with usb mount sucess\n");
			else if(tx_sts==3)
			syslog(LOG_INFO,"Heartbeat msg tx resume for apprun \n");
			tx_sts=1;
                	
			if(t2 == 1){
				while(tc){ //  transmission at usage thresh2 processing
					if (i==1) 
						retval = sendto(sock, msgpld, payload_size, 0,(struct sockaddr *)&server_addr, sizeof(struct sockaddr));
					else 
					if (i==2)
						retval = sendto(sock, msgpld, payload_size, 0,(struct sockaddr *)&server_addr6, ipaddrlen);

					if (retval < 0) {
                               		        //exit(0);
                                                halt=4;
					}
					tc--;
				}
				
				sleep(threshdelaysec);
				continue;
			}
			else { // default transmission
				if (i==1) 
					result = sendto(sock, msgpld, payload_size, 0,(struct sockaddr *)&server_addr, sizeof(struct sockaddr));
				else if (i==2) 
					result = sendto(sock, msgpld, payload_size , 0, (struct sockaddr *)&server_addr6, ipaddrlen);

               			if(result<0)
                		{
                        		syslog(LOG_ERR,"Heartbeat msg tx suspend for 2send err %d (%d) i=%d", result, errno,i);
                        		//exit(0);
                                        halt=4;
				}
               		}

        	}
   		else if((tx_sts==1) || (halt==1))
   		{
			if(halt == 2){
				syslog(LOG_INFO,"Heartbeat msg tx halt for usb mount error \n");
				tx_sts=2;
			}
			
			else if(halt == 3){
				syslog(LOG_INFO,"Heartbeat msg tx halt for apphalt \n");
                                tx_sts=3;
                        }
                        else if (halt == 4){
                        	syslog(LOG_ERR,"Heartbeat msg tx suspend for Network Unreachable");
                                tx_sts = 4;
                        }
			else {
		//	    syslog(LOG_INFO,"usb purge func called \n");
   		            syslog(LOG_INFO,"Heartbeat msg tx halt :: usage %d%% >= (th3 %d%%) \n", usb_usage ,threshold3);
	                    usbpurge();
			    tx_sts=0;
			}
   		}
  	 
	}
     
	else if(tx_tset==1)
        {
           syslog(LOG_INFO,"Heartbeat msg tx halts for time out of range \n");
           tx_tset=0;
           continue;
    	}
	sleep(pktdelaysec);
   }
}

void sig_conf(void)
{
        
	get_logconf();
        syslog(LOG_INFO,"logconf file reloaded with threshold2=%d threshold3=%d\n",threshold2,threshold3);
	get_HBT_options(config_file);
	syslog(LOG_INFO,"heartbeat.conf reloaded with tx broadcast interval:%d,tx interval:%d,tcount:%d \n",pktdelaysec,threshdelaysec,tcount);

}




