#include <sys/types.h>
#include <sys/socket.h>
#include <stdio.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <stdlib.h>
#include <sys/msg.h>
#include <pthread.h>
#include <fcntl.h>
#include <poll.h>
#include <string.h>
#include <sys/ioctl.h>
#include <net/if.h>
#include "network.h"
#include "log.h"

typedef struct
{
	int status;
	int client_sock;
	pthread_t thread_id;
	pthread_t send_thread_id;

} net_connection_t;

void vsmConnectionHandler(net_connection_t *net_connection);
void vsmNetworktransmit(); //net_connection_t *net_connection);


#define NET_DELIM ";"
#define MAX_CONNECTIONS	5

static net_connection_t net_connection[MAX_CONNECTIONS];
static 	int server_sockfd = (int) NULL;
static pthread_t video_thread_id;
pthread_t telemetry_thread_id;

char nodeName[20];
int PORT_NUMBER = 4000;

void setPortNumber(int port) {
	PORT_NUMBER = port;
}

void destroy_netserver(void)
{
	int i;
	//close all called threadsg_message ("Message Received:%s", msg);
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		if(net_connection[i].thread_id != (int) NULL)	//If it is not null then it exist so try to cancel it
		{
			pthread_cancel(net_connection[i].thread_id);
			pthread_join(net_connection[i].thread_id,NULL);
		}
	}			
	//close socket
	close(server_sockfd);
} 

void netServer(void)
{


	int temp_fd;
	int server_len,client_len;
	struct sockaddr_in server_address;
	struct sockaddr_in client_address;
	int i;
	int flags;
	int retvalue;
	int on = 1;
	
	pthread_cleanup_push((void*) destroy_netserver, NULL);
	pthread_setcancelstate(PTHREAD_CANCEL_ENABLE,NULL);
	pthread_setcanceltype(PTHREAD_CANCEL_DEFERRED,NULL);

	initializeTelemetry();

	//Create unnamed socket
	server_sockfd = socket(AF_INET, SOCK_STREAM,0);
	//Allows the socket to reuse the binding address so that on quick restarts we do not get an error
	retvalue = setsockopt(server_sockfd,SOL_SOCKET,SO_REUSEADDR,&on,sizeof(on));
	//printf("server_sockerfd = %d\n",server_sockfd);
	
	//name the socket
	server_address.sin_family = AF_INET;
//	server_address.sin_addr.s_addr = htonl(INADDR_ANY);//inet_addr("131.167.81.36");
	server_address.sin_addr.s_addr = inet_addr("192.168.0.40");
//	server_address.sin_addr.s_addr = inet_addr("192.168.1.40");
	server_address.sin_port = htons(PORT_NUMBER);
	server_len = sizeof(server_address);
	if(bind(server_sockfd, (struct sockaddr *)&server_address, server_len))
		perror("BIND:");

	if(listen(server_sockfd, MAX_CONNECTIONS))
		perror("LISTEN:");
	
	client_len = sizeof(client_address);

  
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		net_connection[i].client_sock = (int) NULL;
		net_connection[i].status = DISCONNECTED;
		net_connection[i].thread_id = (int) NULL;
		net_connection[i].send_thread_id = (int) NULL;
	}	
	//Set socket as non blocking
	flags = fcntl(server_sockfd,F_GETFL,0);
	retvalue = fcntl(server_sockfd,F_SETFL,O_NONBLOCK|flags);


	// Get IP address and append
    struct ifreq *ifr, *ifend;
    struct ifreq ifreq;
    struct ifconf ifc;
    struct ifreq ifs[64];
    char ipAddr[20];

    ifc.ifc_len = sizeof(ifs);
    ifc.ifc_req = ifs;

    if (ioctl(server_sockfd, SIOCGIFCONF, &ifc) < 0)
 	{
        printf("ioctl(SIOCGIFCONF): %m\n");
    }
    else {
		ifend = ifs + (ifc.ifc_len / sizeof(struct ifreq));
		for (ifr = ifc.ifc_req; ifr < ifend; ifr++)
		{
			if (ifr->ifr_addr.sa_family == AF_INET)
			{
				strncpy(ifreq.ifr_name, ifr->ifr_name,sizeof(ifreq.ifr_name));
				if (ioctl (server_sockfd, SIOCGIFHWADDR, &ifreq) < 0)
				{
				  printf("SIOCGIFHWADDR(%s): %m\n", ifreq.ifr_name);
				}
				else {
					char tempIP[20];
//					strcpy(tempIP,inet_ntoa( ( (struct sockaddr_in *)  &ifr->ifr_addr)->sin_addr));
					strcpy(tempIP,inet_ntoa( server_address.sin_addr));
					if(strcmp(tempIP,"127.0.0.1")) {
						strcpy(ipAddr,tempIP);
						strcpy(SINK_IP,tempIP);
						int a,b,c,d;
						sscanf(ipAddr, "%d.%d.%d.%d", &a,&b,&c,&d);
						sprintf(nodeName,"SINK_%d_%d",d, PORT_NUMBER);
					}
				}
			 }
		}
    }
    sprintf(SINK_PORT,"%d",PORT_NUMBER);
	log_entry(LOG_INFO, LOG_LOG, __FILE__, __LINE__, "Node waiting for connections on %s port %s", ipAddr, SINK_PORT);

	// Start Telemetry Thread
	pthread_create(&telemetry_thread_id,NULL,vsmNetworktransmit, 0);


    //-----------------------------------------------------------------------------------------------------------------------------

	while(1)
	{
		
			for(i=0;(i<MAX_CONNECTIONS) & (net_connection[i].client_sock != (int) NULL);i++);	//look for first available socket fd
			
			if(i < MAX_CONNECTIONS)
			{
				temp_fd = accept(server_sockfd,(struct sockaddr *)&client_address,(socklen_t *)&client_len);
				if(temp_fd >= 0)
				{
					log_entry(LOG_INFO, LOG_LOG, __FILE__, __LINE__,
						"new connection %d from %s",i,inet_ntoa(*(struct in_addr *)(&client_address.sin_addr)));
	
					//printf("new connection %d\n",i);
					net_connection[i].client_sock = temp_fd;
					net_connection[i].status = CONNECTED;
					pthread_create(&net_connection[i].thread_id, NULL, vsmConnectionHandler,  &net_connection[i]);
				}
				
			}
			else
				printf("No open sockets\n");	 
			sleep(1);

			//Check if any of the connections have closed
			for(i=0;i<MAX_CONNECTIONS;i++)
			{
				if((net_connection[i].status == DISCONNECTED) & (net_connection[i].client_sock != (int) NULL))
				{
					printf("joining connection %d\n",i);
					if(!(pthread_join(net_connection[i].thread_id,NULL)))
						net_connection[i].thread_id = (int) NULL;					//Thread has been joined set the id to null
//					pthread_cancel(net_connection[i].send_thread_id);
//					if(!(pthread_join(net_connection[i].send_thread_id,NULL)))
						net_connection[i].send_thread_id = (int) NULL;					//Thread has been joined set the id to null
					net_connection[i].client_sock = (int) NULL;
					printf("thread joined (%d) all is well\n",i); 
				}
			}

		
		pthread_testcancel();
	}	

 pthread_cleanup_pop(1);
}

#define MAX_MSGBUF_SIZE 256
void vsmConnectionHandler(net_connection_t *net_connection)
{
	char ServerName[10];
	char label[70];
	char MessageType[32];
	char ch[100];
	char messagebuffer[MAX_MSGBUF_SIZE];
	char *chptr;
	char *sentptr,*localptr;
	int retvalue;
	long int temp;
	message_t MessageToSerial;
	size_t MessageSize;

	// Set up access to message queue
	MessageSize = sizeof(MessageToSerial) - sizeof(temp);
	pthread_setcancelstate(PTHREAD_CANCEL_ENABLE,NULL);
	pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS,NULL);

	// Initialize Telemetry

	int params = 0;
	while(1)
	{
		params = 0;
		retvalue = read(net_connection->client_sock,&ch,100);
		if(retvalue <= 0)	//connection has been closed 
		{
			log_entry(LOG_INFO, LOG_LOG, __FILE__, __LINE__,
			"connection closing...");
			net_connection->status = DISCONNECTED;
			pthread_exit(0);
		
		}		
		chptr = ch;
	   	while(*chptr != '\n') {
	   		if (*chptr == ';') params++;
	   		*chptr++;//	printf("%c",*chptr++);
	   	}
	   	printf("\n");
		*chptr = 0;				// remove the \n from the string, it complicates the parsing of the final element of the string
		log_entry(LOG_INFO, LOG_LOG, __FILE__, __LINE__,
			"%s",&ch);

		sendMessage(&ch, params);
		/*
		//get server name
		sentptr = strtok_r(ch, NET_DELIM, &localptr);
		if(sentptr)
		{
			strcpy(ServerName, sentptr);
			//printf("Server Name = %s\n",sentptr);

		}

		/*
		if(!strncmp(sentptr,"SINK", 4))		// Confirm that the message was from an DECODER client
		{
			//strncpy(tempbuffer,&sentptr[3],1);
			//vsm_address = atoi(tempbuffer);
		}

		// get message number
		sentptr = strtok_r(NULL,NET_DELIM,&localptr);
		if(sentptr)printf("%s\n",sentptr);

		//Get Message type
		sentptr = strtok_r(NULL,NET_DELIM,&localptr);
		if(sentptr)
		{
			strcpy(MessageType,sentptr);
		    printf("Message type = %s\n",sentptr);
		}

		if(!strcmp(MessageType, CONNECTION))	// Received CONNECTION message
		{

			log_entry(LOG_INFO, LOG_LOG, __FILE__, __LINE__,
			"Connection Command...");
			// Get Action

		}
		else if(!strcmp(MessageType,TELEMETRY))	//Received TELEMETRY  message
		{

		}
		else if(!strcmp(MessageType,VERSION_REQUEST))	//Received VERSION_REQUEST  message
		{
		   sprintf(messagebuffer,"%s;0;VERSION_REPLY;SERVER;%s\n", nodeName, VERSION);
		   sendMessageToAllClients(messagebuffer);
			
		}
		else if(!strcmp(MessageType,VERSION_REPLY))	//Received VERSION_REPLY  message
		{

		}
		*/
		sleep(0);
	}

}

void connectToStream() {
	// Remove

}

void vsmNetworktransmit() //net_connection_t *net_connection)
{
	char messagebuffer[2500];
	int i;

	log_entry(LOG_INFO, LOG_SERVER, __FILE__, __LINE__,"Starting Telemetry Thread");
	pthread_setcancelstate(PTHREAD_CANCEL_ENABLE,NULL);
	pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS,NULL);

	while(1)
	{
		strcpy(messagebuffer,"Stay Alive\n");
		sendMessageToAllClients(messagebuffer);
		sleep(1);
	}

}

void sendMessageToAllClients(char *message)
{
	int retvalue;
	int i;
	//scroll through each connected client
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		if ((net_connection[i].status) && (net_connection[i].client_sock != (int) NULL)) {
			retvalue = write(net_connection[i].client_sock,message,strlen(message));
//			log_entry(LOG_INFO, LOG_SERVER, __FILE__, __LINE__,"Client %d - %s", i, message);
		}
	}

}

void initializeTelemetry() {
	log_entry(LOG_INFO, LOG_SERVER, __FILE__, __LINE__, "Initializing Telemetry");
}

