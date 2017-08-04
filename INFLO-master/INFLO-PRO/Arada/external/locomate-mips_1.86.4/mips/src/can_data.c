#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <errno.h>
#include <signal.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include "can_gds.h"
#include <semaphore.h>
#include <pthread.h>
#include <semaphore.h>
#include <sys/uio.h>
#include <net/if.h>
#ifdef SDK_NEW
#include <socketcan/can.h>
#include <socketcan/can/raw.h>
#endif

can_GDSData_t candata;
sem_t can_sem;
pthread_t thread_id;
char interface[20];
void* open_sock(void *);

int get_can_data(void *sock,char *can_interface) {
    strcpy(interface,can_interface);
    sem_init(&can_sem,0,1);
#ifdef SDK_NEW
    pthread_create(&thread_id, NULL, open_sock, sock);
    return(thread_id);
#else
	return 0;
#endif
 }

#ifdef SDK_NEW
void* open_sock(void *sock1) { 
      int nbytes;
      int sock;
      struct sockaddr_can addr;
      struct can_frame frame; 
      struct ifreq ifr;
    
    sock = socket(PF_CAN,SOCK_RAW,CAN_RAW);
    if (sock < 0) {
      perror("socket");
      return 1;
    }
    sock1 = (void *)&sock;
    strcpy(ifr.ifr_name,interface);
    if(ioctl(sock, SIOCGIFINDEX ,&ifr) < 0){
      perror("SIOCGIFINDEX");
      return 1;
    }

    addr.can_family = AF_CAN;
    addr.can_ifindex = ifr.ifr_ifindex;
   
    if(bind(sock,(struct sockaddr *)&addr ,sizeof(addr))<0){
      perror("Bind");
      return 1;
    }
   
  while(1){
    nbytes = read(sock, &frame, sizeof(struct can_frame));
    if(nbytes < sizeof(struct can_frame)){
      printf("frame not completely received\n");
      continue;
    }
    sem_wait(&can_sem);
    if(frame.can_id == 1)
      candata.exteriorLights = frame.data[0];
    if(frame.can_id == 2)
      candata.ratefront = frame.data[0];
    if(frame.can_id == 3)
      candata.raterear = frame.data[0];
     sem_post(&can_sem);
  }
}
#endif
