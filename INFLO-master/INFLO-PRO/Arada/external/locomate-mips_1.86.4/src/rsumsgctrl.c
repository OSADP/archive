#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <stdlib.h>
#include <errno.h>
#include <syslog.h>

/* Client.c */

int main( int argc, char *argv[])
{
   char buf[512];
   int clientToServer_socket;
   unsigned int fromLength;
   struct sockaddr_in Remote_Address, Server_Address;
   struct hostent *hostPointer;
   int message, checkCall;
   int counter1 = 0;
   int counter2 = 0;
   int broadcastOn = 1;
   int broadcastOff = 0;
   char broadcastMessage[512];
   in_addr_t  bcast_addr;

   if (argc != 4) {
        printf("%s <ip address> <port number> <message>\n", argv[0]);
        exit(0);
   }


   strcpy(broadcastMessage,argv[3]);;
   bcast_addr = INADDR_BROADCAST;

//   printf("Message %s\n", broadcastMessage); 

   /*Create client socket*/ 
   clientToServer_socket=socket(AF_INET, SOCK_DGRAM, 0);
   if (clientToServer_socket==-1)
	perror("Error: client socket not created");

   /*Fill in client's sockaddr_in */
   bzero(&Remote_Address, sizeof(Remote_Address));
   Remote_Address.sin_family=AF_INET;
 //  printf("%s \n", argv[1]);
   //hostPointer=gethostbyname(argv[1]);
//   memcpy((unsigned char * ) &Remote_Address.sin_addr, (unsigned char *) hostPointer -> h_addr, hostPointer -> h_length);
   inet_aton(argv[1], &bcast_addr);
   memcpy((unsigned char * ) &Remote_Address.sin_addr, (unsigned char *)&bcast_addr, sizeof(bcast_addr));
   Remote_Address.sin_port=htons(atoi(argv[2]));
   //printf("%s \n", argv[2]);

//#if 0
   checkCall = setsockopt(clientToServer_socket, SOL_SOCKET, SO_BROADCAST, &broadcastOn, 4);
   if(checkCall == -1)
	perror("Error: setsockopt call failed");
  
//#endif
// uncomment to send bcast 
   Remote_Address.sin_addr.s_addr|=htonl(0xff);
//   Remote_Address.sin_addr.s_addr = inet_addr(argv[1]);;

#if 0
   checkCall = setsockopt(clientToServer_socket, SOL_SOCKET, SO_BROADCAST, &broadcastOff, 4);
   if(checkCall == -1)
	perror("Error: Second setsockopt call failed");
#endif

   int broadcastMessageLen = strlen(broadcastMessage) + 1; 
   
   //printf("Message test %d\n", broadcastMessageLen);
 
    
     
   message = sendto(clientToServer_socket, broadcastMessage, broadcastMessageLen, 0, (struct sockaddr *) &Remote_Address, sizeof(Remote_Address));
   if (message ==-1)
	perror("Error: sendto call failed");
   
#if 0
   while(1)
   {
      fromLength = sizeof(Server_Address);
      message = recvfrom(clientToServer_socket, buf, 512, 0, (struct sockaddr *) &Server_Address, &fromLength);
      if (message ==-1)
	 perror("Error: recvfrom call failed");
  
      printf("SERVER: read %d bytes from IP %s(%s)\n", message, inet_ntoa(Server_Address.sin_addr), buf);
   }
#endif
   close(clientToServer_socket);

}

