#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <syslog.h>
#include<sys/socket.h>
#include<sys/types.h>
#include<unistd.h>
#include<arpa/inet.h>
#include<sys/time.h> // for gettimeofday

#include "usbd.h"

static struct sockaddr_in usb_devaddr;
static int is_usb_devaddr_set = 0;

#define DEFAULT_DEVADDR "127.0.0.1"

char* get_usb_devaddr()
{
        if(is_usb_devaddr_set)
                return inet_ntoa(usb_devaddr.sin_addr);
        else
                return (char *)DEFAULT_DEVADDR;
}

char* set_usb_devaddr(uint8_t *devaddr)
{
                int ret;
                is_usb_devaddr_set = 0;
#ifdef WIN32
                usb_devaddr.sin_addr.s_addr = inet_addr ((devaddr)? devaddr : (char*)DEFAULT_DEVADDR);
#else
                ret = inet_aton((devaddr)? devaddr : (char*)DEFAULT_DEVADDR, &usb_devaddr.sin_addr);
#endif
                if(!ret)
                        return NULL;
                is_usb_devaddr_set = 1;
                return (devaddr)? devaddr : (char*)DEFAULT_DEVADDR ;
}

int usb_connect()
{
        int ret, one =1;
        struct sockaddr_in usbdaddr;
        int flags;
        int usbsockfd = -1;

        if ( (usbsockfd = socket(AF_INET, SOCK_STREAM,6)) < 0) {
        (void)syslog(LOG_ERR,"usb %d\n", __LINE__);
                return -1;
    }

        if (usbsockfd > 0) {
                bzero(&usbdaddr, sizeof(usbdaddr));

                if(!is_usb_devaddr_set)
                        set_usb_devaddr(DEFAULT_DEVADDR);

                usbdaddr.sin_addr = usb_devaddr.sin_addr;
                usbdaddr.sin_family = AF_INET;
                usbdaddr.sin_port = htons(6666);

                if(setsockopt(usbsockfd,SOL_SOCKET, SO_REUSEADDR,(char *)&one,sizeof(one)) == -1)
                {
        (void)syslog(LOG_ERR,"usb %d\n", __LINE__);
            usb_close_sock(usbsockfd);
                        return -2;
        }
                ret = connect(usbsockfd, (struct sockaddr *) &usbdaddr, sizeof(usbdaddr));
                if (ret < 0) {
        (void)syslog(LOG_ERR,"usb %d\n", __LINE__);
            usb_close_sock(usbsockfd);
            (void)syslog(LOG_ERR,"failing on connect to usb\n");
                        return -2;
                }
        }
    return usbsockfd;
}

int usb_close_sock(int usbsockfd)
{
        close(usbsockfd);
        return -1;
}
