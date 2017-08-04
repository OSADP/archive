#include <stdio.h>   /* Standard input/output definitions */
#include <string.h>  /* String function definitions */
#include <unistd.h>  /* UNIX standard function definitions */
#include <fcntl.h>   /* File control definitions */
#include <errno.h>   /* Error number definitions */
#include <termios.h> /* POSIX terminal control definitions */
#include <sys/syslog.h>
#include <stdint.h>  /* for opaque data types */

int settings();
struct termios options;
int fd_gps;  // File descriptor
int initialize_commands();
int n,i;
char DevName[20],Ip[20],PortNo[10];
uint32_t  BaudRate;
uint16_t UpdateRate,GsvRr,WAAS,DevMode;


int configuregps(int waas_enable , int baudrate)

{

       char buf[30];int res;
       fd_gps = open_port();

	n = write(fd_gps, "$PMTK314,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0*28\r\n" ,49); //Disable GPZDA && GPMCHN
	
	if(waas_enable == 0)
        {                     
              n = write(fd_gps, "$PMTK313,0*2F\r\n", 15); //WAAS DISABLE
              n = write(fd_gps, "$PMTK301,0*2C\r\n", 15);
              n = initialize_commands(); //Initilization Commands
              (void)syslog(LOG_INFO, "WAAS_DISABLED \n");
            
        }
	else
	{
	      n = write(fd_gps, "$PMTK313,1*2E\r\n", 15); //WAAS ENABLE
              n = write(fd_gps, "$PMTK301,2*2E\r\n", 15);
	      n = initialize_commands(); //Initilization Commands	
	      (void)syslog(LOG_INFO, "WAAS_ENABLED \n");
	
	}	
	if(baudrate != 115200)
	{
	   (void)syslog(LOG_INFO, "Default Baudrate of gps device is %d \n" ,baudrate);
           n = write(fd_gps, "$PMTK251,115200*1F\r\n", 20);//BaudRate
           n = write(fd_gps, "$PMTK220,200*2C\r\n", 17);   //Fix Update Rate
           cfsetispeed(&options, B115200);
           cfsetospeed(&options, B115200);
           settings();
           n = initialize_commands(); //Initilization Commands
           (void)syslog(LOG_INFO, "Baudrate set to 115200 \n");
        }

    return (0);
}
//********************opening port****************

int open_port(void)
{
      int fd_gps; /* File descriptor for the port */

      fd_gps = open("/dev/ttyS0", O_RDWR | O_NOCTTY | O_NDELAY);
      if (fd_gps == -1)
        {
         /* Could not open the port. */
          perror("open_port: Unable to open /dev/ttyS0 - ");
        }
     else
         {
        fcntl(fd_gps, F_SETFL, FNDELAY);
//        printf("opening of device /dev/ttyS0 succeeded !!\n");
        (void)syslog(LOG_INFO, "Device Opened Successfully \n");
     }

//      printf ( "In Open port fd = %i\n", fd_gps);
        (void)syslog(LOG_INFO, "Port Opened fd_gps = %i\n",fd_gps);
      return (fd_gps);

}

//***********************Port Setting**********************
int settings()
{
      options.c_cflag |= ( CLOCAL | CREAD );

      // Set the Charactor size

      options.c_cflag &= ~CSIZE; /* Mask the character size bits */
      options.c_cflag |= CS8;    /* Select 8 data bits */

      // Set parity - No Parity (8N1)
      options.c_cflag &= ~PARENB;
      options.c_cflag &= ~CSTOPB;
      options.c_cflag &= ~CSIZE;
      options.c_cflag |= CS8;

      //options.c_lflag |= CRTSCTS;
      //Disable Hardware flowcontrol
      //options.c_cflag &= ~CNEW_RTSCTS;  -- not supported

      //Enable Raw Input
      options.c_lflag &= ~(ICANON | ECHO | ECHOE | ISIG);
      //Disable Software Flow control
      options.c_iflag &= ~(IXON | IXOFF | IXANY);
      //Chose raw (not processed) output
      options.c_oflag &= ~OPOST;

            /* set input mode (non-canonical, no echo,...) */
      options.c_lflag = 0;

      options.c_cc[VTIME]    = 0;   /* inter-character timer unused */
      options.c_cc[VMIN]     = 1;   /* blocking read until 1 chars received */
      if ( tcsetattr( fd_gps, TCSAFLUSH, &options ) == -1 )
//         printf ("Error with tcsetattr = %s\n", strerror ( errno ) );
        (void)syslog(LOG_INFO, " Error with tcsetattr \n");
      else
        (void)syslog(LOG_INFO, "tcsetattr succeed \n");
//      printf ( "%s\n", "tcsetattr succeed" );
      fcntl(fd_gps, F_SETFL, FNDELAY);
      return (0);

}
	 
int initialize_commands()
{
     n = write(fd_gps, "$PMTK414*33\r\n", 13);
     n = write(fd_gps, "$PMTK400*36\r\n", 13);
     n = write(fd_gps, "$PMTK602*36\r\n", 13);
     n = write(fd_gps, "$PMTK401*37\r\n", 13);
     n = write(fd_gps, "$PMTK419*3E\r\n", 13);
     n = write(fd_gps, "$PMTK490*3F\r\n", 13);
     n = write(fd_gps, "$PMTK430*35\r\n", 13);
     return (0);
}

int GetGpsOptions()
{
        FILE *fdrd;
        char mline[200];
        char *sts,fname[15]="/var/gps.conf";
        char *token = NULL;

	
	fdrd = fopen(fname, "r");
        if (fdrd <=0) {
            syslog(LOG_INFO,"Error opening %s file\n", fname);
            return -1;
        }
        memset(mline, 0, sizeof(mline));
        while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {
            if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' '){
                token = strtok(mline, "=");

                if(strcasecmp(token, "DeviceName") == 0 ){
                    token = strtok(NULL,"\r\n ");
                    sscanf(token,"%s",DevName);
                    //syslog(LOG_INFO,"####Device name%s#\n",DevName);
                }
                else if(strcasecmp(token, "DeviceMode") == 0 ){
                    token = strtok(NULL,"\r\n ");
                     DevMode = (uint16_t)atoi(token);
                    //syslog(LOG_INFO,"####DeviceMode %d#\n",DevMode);
                }
                else if(strcasecmp(token, "BaudRate") == 0 ){
                    token = strtok (NULL,"=");
                     BaudRate = (uint32_t)atoi(token);
                     //syslog(LOG_INFO,"####BaudRate:%d#\n",BaudRate);

                }
                else if(strcasecmp(token, "UpdateRate") == 0 ){
                    token = strtok (NULL,"=");
                     UpdateRate = (uint16_t)atoi(token);
                    //syslog(LOG_INFO,"####UpdateRate%d#\n",UpdateRate);
                }
                else if(strcasecmp(token, "WAAS") == 0 ){
                    token = strtok (NULL,"=");
                     WAAS = (uint16_t)atoi(token);
                    //syslog(LOG_INFO,"####WASS%d#\n",WAAS);
                }
                else if(strcasecmp(token, "GSVRepeatRate") == 0 ){
                    token = strtok (NULL,"=");
                     GsvRr = (uint16_t)atoi(token);
                     //syslog(LOG_INFO,"####GSVRepeatRate:%d#\n",GsvRr);
                }
                else if(strcasecmp(token, "IPAddress") == 0 ){
                    token = strtok(NULL,"\r\n");
                    sprintf(Ip,"%s",token);
                    //syslog(LOG_INFO,"####Ip address%s#\n",Ip);
                }
                else if(strcasecmp(token, "PortNo") == 0 ){
                    token = strtok(NULL,"\r\n");
                    sprintf(PortNo,"%s",token);
                    //syslog(LOG_INFO,"####PortNo%s#\n",PortNo);
                }
            }
            memset(mline, 0, sizeof(mline));
        }
	fclose(fdrd);
        return 1;
	
}

