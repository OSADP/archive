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

#define EXIT_CMD 1

char cmd_execute[50];
char cmd[50];
int base_mac;
char ip_address[50];
char temp_ip_address[50];
char buf[1024];

struct sockaddr_in server_address, client_address;
fd_set readfds,testfds,mst_readfds,mst_testfds;

int server_socket;
int checkCall;

int execcmd(char *buf)
{
  if(!strncmp(buf,"quit",4))
  	return EXIT_CMD;

//  printf("string= %s \n",buf);
//  printf("-%s-%s-\n",inet_ntoa(client_address.sin_addr),ip_address);
  if((strncmp((inet_ntoa(client_address.sin_addr)),ip_address, strlen(ip_address)) ==0) ||(strncmp((inet_ntoa(client_address.sin_addr)),temp_ip_address, strlen(temp_ip_address)) ==0) )
   ;
//       printf("match= %s \n",buf);
  else
  	// command to execute
  	system(buf);

  return 0;
}

int command_server(char *prtno)
{

   unsigned int clientLength;
   int  message, sts;
   FILE *fd;	
#if 0
   if (argc != 2) {
	printf("%s <port number>\n", argv[0]);
 	exit(0);
   }
#endif   
   /*Create socket */
   server_socket=socket(AF_INET, SOCK_DGRAM, 0);
   if(server_socket == -1)
        perror("Error: socket failed");

   bzero((char*) &server_address, sizeof(server_address));

   /*Fill in server's sockaddr_in*/
   server_address.sin_family=AF_INET;
   server_address.sin_addr.s_addr=htonl(INADDR_ANY);
   //server_address.sin_port=htons(atoi(argv[1]));
   server_address.sin_port=htons(atoi(prtno));

   
   checkCall = bind(server_socket, (struct sockaddr *) &server_address, sizeof(struct sockaddr));
   if(checkCall == -1)
        perror("Error: Bind failed");
   fd = popen("ifconfig brtrunk | grep inet | awk '{print $2}'","r");
   if(fd != NULL)
   {
       fscanf(fd, "addr:%s",ip_address);
       pclose(fd);
   }
   fd = popen("ifconfig brtrunk:1 | grep inet | awk '{print $2}'","r");
   if(fd != NULL)
   {
       fscanf(fd, "addr:%s",temp_ip_address);
       pclose(fd);
   }
//printf("My addr %s\n", ip_address);
   while(1)
   {
       	//printf("SERVER: waiting for data from client\n");
	
	clientLength = sizeof(client_address);
	message = recvfrom(server_socket, buf, sizeof(buf), 0,
		  (struct sockaddr*) &client_address, &clientLength);
	if(message == -1)
        perror("Error: recvfrom call failed");

//	printf("SERVER: read %d bytes from IP %s(%s)\n", message,inet_ntoa(client_address.sin_addr), buf);

	buf[message]='\0';
//  printf(">>>>> -%s-%s-\n",inet_ntoa(client_address.sin_addr),ip_address);
	sts = execcmd(buf);
 	if (sts == EXIT_CMD) {
	   break;
	}

	//strcpy(buf,"ok");
   }

}

void create_ipfwdscript(int sock_port)
{     
    char cmd[150];
    memset(cmd,0,sizeof(cmd));
    sprintf(cmd,"echo \"#!/bin/sh\" > /tmp/ipfwd_%d.sh;",sock_port);
    system(cmd);
    memset(cmd,0,sizeof(cmd));

    sprintf(cmd,"echo \"nc -w 1 -u 192.168.2.41 %d < /tmp/udp_%d.txt & \" >> /tmp/ipfwd_%d.sh ",sock_port,sock_port,sock_port);
    system(cmd);
    memset(cmd,0,sizeof(cmd));

    sprintf(cmd,"chmod 777 /tmp/ipfwd_%d.sh",sock_port);
    system(cmd);
    memset(cmd,0,sizeof(cmd));
}

int main()

{
   char deployment[20],product_id[50],basemac[50],mst_msg[50];
   FILE *fd = NULL;
   int ret;
   fd = popen("cat /var/config | grep deployment | awk '{print $2}'","r");
   if(fd != NULL)
   {
            fscanf(fd, "%s",deployment);
            pclose(fd);
   }
   fd = NULL;
   fd = popen("cat /tmp/productid","r"); 
   if(fd != NULL)
   {
            fscanf(fd, "%s",product_id);
            pclose(fd);
   }
   if((strcasecmp(deployment ,"USDOT")==0)  && (strcasecmp(product_id,"LOCOMATE-200-RSU")==0))
   {
       /* Immediate Forward */      	

       system("ifconfig brtrunk:1 192.168.2.40 up"); 	    
       system("echo \"#!/bin/sh\" > /tmp/setip.sh");
       system("echo  /usr/local/bin/rsumsgctrl 192.168.2.41 5555 \\\"ifconfig brtrunk:1 down\\; ifconfig brtrunk 192.168.2.41 up\\\" >> /tmp/setip.sh");
       system("chmod 777 /tmp/setip.sh");
       system("/tmp/setip.sh");	
       
       create_ipfwdscript(4587);
       create_ipfwdscript(12345);   
       ret = command_server("4444");  

   }
   else
   {
    	system("ifconfig brtrunk:1 192.168.2.41 up");        
    	system("/usr/local/bin/rsumsgctrl 192.168.2.40 4444 \"/tmp/setip.sh\"");        
    	ret = command_server("5555"); 	     
   }   
   checkCall = close(server_socket);
   if(checkCall == -1)
        perror("Error: server close call failed");
   return 0;
}


        	  
