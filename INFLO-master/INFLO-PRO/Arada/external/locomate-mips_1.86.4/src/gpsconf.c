#include <stdio.h>   /* Standard input/output definitions */
#include <stdlib.h>   /* Standard input/output definitions */
#include <stdint.h>   /* Standard input/output definitions */
#include <string.h>  /* String function definitions */
#include <syslog.h>  /* String function definitions */
#include <unistd.h>  /* UNIX standard function definitions */
#include <fcntl.h>   /* File control definitions */
#include <errno.h>   /* Error number definitions */
#include <termios.h> /* POSIX terminal control definitions */
#include <sys/ioctl.h>

#define TMPBUFSZ 300
#define PUT_ORIGIN      0
#define putbyte(buf,off,b) do {buf[(off)-(PUT_ORIGIN)] = (unsigned char)(b); } while (0)
#define putleword(buf, off, w) do {putbyte(buf, (off)+1, (uint)(w) >> 8); putbyte(buf, (off), (w)); } while (0)
#define putlelong(buf, off, l) do {putleword(buf, (off)+2, (uint)(l) >> 16); putleword(buf, (off), (l)); } while (0)

struct termios options,options_old;
int fd;  // File descriptor
fd_set set;
struct timeval timeout;
extern char DevName[20];
extern uint32_t  BaudRate;
extern uint16_t UpdateRate,GsvRr,WAAS,DevMode;


/***************************baudrate to code convert*********/
speed_t get_baud(int baudrate) {
    switch (baudrate) {
    case 0:
        return B0;
    case 50:
        return B50;
    case 75:
        return B75;
    case 110:
        return B110;
    case 134:
        return B134;
    case 150:
        return B150;
    case 200:
        return B200;
    case 300:
        return B300;
    case 600:
        return B600;
    case 1200:
        return B1200;
    case 1800:
        return B1800;
    case 2400:
        return B2400;
    case 4800:
        return B4800;
    case 9600:
        return B9600;
    case 19200:
        return B19200;
    case 38400:
        return B38400;
    case 57600:
        return B57600;
    case 115200:
        return B115200;
    default:
        return B9600;
    }
}


//*************************Opening Port***********************
int OpenPort(char * device)
{
    int fd;   /* File descriptor for the port */

    fd = open(device, O_RDWR | O_NOCTTY | O_NONBLOCK );  //check if NDELAY is needed
    if (fd == -1)
    {
        /* Could not open the port. */
        perror("open_port: Unable to open /dev/ttyUSB0 - ");
    }

    //get current settings and save in other variable
    tcgetattr(fd, &options_old);
    (void)memcpy(&options,&options_old, sizeof(options));
    /*
     * Only block until we get at least one character, whatever the
     * third arg of read(2) says.
     */
    /*make raw*/
    memset(options.c_cc, 0, sizeof(options.c_cc));
    options.c_cc[VMIN] = 1;
    options.c_cflag &= ~(PARENB | PARODD | CRTSCTS);
    options.c_iflag &= ~(PARMRK | INPCK);       //no parity
    options.c_cflag &= ~(CSIZE | CSTOPB | PARENB | PARODD);
    options.c_cflag |= ( CREAD | CLOCAL | CS8 ); /* Select 8 data bits */
    options.c_iflag = options.c_oflag = options.c_lflag = (tcflag_t) 0;

    printf ( "In Open port fd = %i\n", fd);
    return (fd);

}


//***********************Port Setting**********************

int Settings(unsigned int speed)
{
    speed_t spdcode =0;
    spdcode=get_baud(speed);
    cfsetispeed(&options, spdcode);
    cfsetospeed(&options, spdcode);
    if ( tcsetattr( fd, TCSANOW, &options ) == -1 )
        (void)syslog(LOG_INFO, "GPSCONF: Error with tcsetattr \n");
    else
        (void)syslog(LOG_INFO, "GPSCONF:change spd %d \n",speed);
    printf("changing spd to %d\n",speed);
    (void)tcflush(fd, TCIOFLUSH);
    (void)usleep(200000);
    (void)tcflush(fd, TCIOFLUSH);
    return (0);

}

/********************generic ublox command write func **********/

int ubx_write(unsigned int msg_class, unsigned int msg_id,
    unsigned char *msg, unsigned short data_len)
{
    unsigned char CK_A, CK_B;
    ssize_t i, count,j;
    int ok,msgbuflen=0;
    unsigned char cmd_send[100] = { 0 }; //init send buffer

    /*@ -type @*/
    cmd_send[0] = 0xb5; //UBX protocol headers 2 bytes B5 62
    cmd_send[1] = 0x62;

    CK_A = CK_B = 0;
    cmd_send[2] = msg_class;
    cmd_send[3] = msg_id;
    cmd_send[4] = data_len & 0xff;
    cmd_send[5] = (data_len >> 8) & 0xff;

    (void)memcpy(&cmd_send[6], msg, data_len);

    /* calculate CRC */
    for (i = 2; i < 6; i++) {
        CK_A += cmd_send[i];
        CK_B += CK_A;
    }
    /*@ -nullderef @*/
    for (i = 0; i < data_len; i++) {
        CK_A += msg[i];
        CK_B += CK_A;
    }

    cmd_send[6 + data_len] = CK_A;
    cmd_send[7 + data_len] = CK_B;
    msgbuflen = data_len + 8;
    /*@ +type @*/
    printf ("ubxwrite = ");
    for (j=0; j < msgbuflen; j++)
        printf("%02x ",cmd_send[j]);
    printf ("\n");
    tcflush(fd,TCIOFLUSH);
    count = write(fd,cmd_send,msgbuflen);
    (void)tcdrain(fd);
    ok = msgbuflen;
    /*@ +nullderef @*/
    return (ok);
}
/* buffers for baud ,update rate ,waas and cfg-save accrding to ublox receiver description*/

int speed_default_buf(unsigned char * buff)
{
    buff[0] = 0x01; //port-ID 1byte
    buff[1] = 0x00;  //reserved
    buff[2] = 0x00;  //tx-ready 2 bytes set to 0
    buff[3] = 0x00;
    buff[4] = 0xc0;  //mode 8N1 4bytes
    buff[5] = 0x08;
    buff[6] = 0x00;
    buff[7] = 0x00;
    buff[8] = 0x80;  //baud rate here it is 0x2580=9600 but overwritten later before sending (4bytes)
    buff[9] = 0x25;
    buff[10] = 0x00;
    buff[11] = 0x00;
    buff[12] = 0x07;  //inprotomask = 0+1+2 UBX+NMEA+RTCM (2bytes)
    buff[13] = 0x00;
    buff[14] = 0x06;  //outprotomask = 1+2 NMEA+RTCM (2bytes)
    buff[15] = 0x00;
    buff[16] = 0x00;  // 2 * 2byte reserved values
    buff[17] = 0x00;
    buff[18] = 0x00;
    buff[19] = 0x00;
    return 20;

}
int mode_default_buf(unsigned char * buff,int mode)
{
    
    if(mode == 4)
	buff[2]=0x04;  // 4 - For Automotive Mode
    else 
	buff[2]=0x00;  // 0 - For Portable Mode
    buff[0] = 0x01; //apply DYN mask,only masked parameters will be applied
    buff[1] = 0x00;  
    buff[3] = 0x00;
    buff[4] = 0x00;  
    buff[5] = 0x00;
    buff[6] = 0x00;
    buff[7] = 0x00;
    buff[8] = 0x00;  
    buff[9] = 0x00;
    buff[10] = 0x00;
    buff[11] = 0x00;
    buff[12] = 0x00;  
    buff[13] = 0x00;
    buff[14] = 0x00;  
    buff[15] = 0x00;
    buff[16] = 0x00;  
    buff[17] = 0x00;
    buff[18] = 0x00;
    buff[19] = 0x00;
    buff[20] = 0x00;
    buff[21] = 0x00;
    buff[22] = 0x00;  
    buff[23] = 0x00;
    buff[24] = 0x00;  
    buff[25] = 0x00;
    buff[26] = 0x00;  
    buff[27] = 0x00;
    buff[28] = 0x00;
    buff[29] = 0x00;
    buff[30] = 0x00;
    buff[31] = 0x00;
    buff[32] = 0x00;  
    buff[33] = 0x00;
    buff[34] = 0x00;  
    buff[35] = 0x00;
    return 36;

}

int disable_gsv_buf(unsigned char * buff, uint8_t gsvrr1)
{
    buff[0] = 0xF0; //msg class & msg-id for gsv 2bytes
    buff[1] = 0x03;
    buff[2] = gsvrr1;//disable on all 6 I/O targets 6bytes
    buff[3] = gsvrr1;
    buff[4] = gsvrr1; 
    buff[5] = gsvrr1;
    buff[6] = gsvrr1;
    buff[7] = gsvrr1;
    return 8;
}

int cfgsave_default_buf(unsigned char * buff)
{
    buff[0] = 0x00; //clearmask (4bytes)
    buff[1] = 0x00;
    buff[2] = 0x00;
    buff[3] = 0x00;
    buff[4] = 0xff; //savemask (4bytes) save all configuration sections
    buff[5] = 0xff;
    buff[6] = 0x00;
    buff[7] = 0x00;
    buff[8] = 0x00; //loadmask (4bytes)
    buff[9] = 0x00;
    buff[10] = 0x00;
    buff[11] = 0x00;
    buff[12] = 0x07; //devicemask (1byte)   all the 3 devices 0b111
    return 13;

}

int updaterate_default_buf(unsigned char *buf_rate)
{

    buf_rate[0] = 0xE8; //measure-rate  gps measurement rate currently 0x03e8=1000 but changed to 200 before sending(2bytes)
    buf_rate[1] = 0x03;
    buf_rate[2] = 0x01; //navrate fixed to 1 (2bytes)
    buf_rate[3] = 0x00;
    buf_rate[4] = 0x01; //timeref 1=GPSTime
    buf_rate[5] = 0x00;

    return 6;
}

int waas_default_buf(unsigned char *waas)
{

    waas[0] = 0x00; // SBAS-mode =0 changed depending on arguement (1byte)
    waas[1] = 0x03; //SBAS-range=0+1 = diffcorrection +range (1byte)
    waas[2] = 0x03; //maxSBAS kept same as default (1byte)
    waas[3] = 0x00; //scanmode2 list of PRN kept same as in default config  (1byte)
    waas[4] = 0xD5; //scanmode1 (4bytes)
    waas[5] = 0xCA;
    waas[6] = 0x06;
    waas[7] = 0x00;
    return 8;

}

int main(int argc,char *argv[])
{
    int res,i,n,j= 0;
    unsigned int speed,len =0;
    int ubx_res,waas,mode_set;
    unsigned char tmpbuf[TMPBUFSZ] = { 0 };    //init baudrate buffer
    unsigned char send_bytes[200] = { 0 };    //init baudrate buffer
    int update_rate,exitcount = 0;
    uint8_t devtype = 0,rv,gsvrr;
    int rdret,rdretry,current_baud=0;
    int baud_array[3]={115200,9600,38400};
    //if (argc != 6) {     //need 5 args -> dev, baud, update_rate waas_sts gsv_rr 
    //    printf( "Please provide device, baudrate updaterate and waasstatus  & gsv-rr(effective for U-blox only) by giving 4 arguements\n");
    //    printf("eg. ./gpsconf /dev/ttyS0 115200 200 1 5\n");
    //    return -1;
    //}
    if(GetGpsOptions()< 0){
    	   syslog(LOG_INFO,"Error in Fetching Gps parameters\n");
    		return -1;
	}
    fd = OpenPort(DevName);
    speed = BaudRate;
    update_rate = UpdateRate;
    waas = WAAS;
    gsvrr = GsvRr;
    syslog(LOG_INFO,"GPSCONF:speed %d updrate %d waas %d devmode %d\n",speed,update_rate,waas,DevMode);
    //find current baud and devtype


    while(!current_baud && (j < 3))
    {
        res = Settings(baud_array[j]);
        rdret=0;
        rdretry=4;
        for(i=3; i>0; i--)
        {
            write(fd, "$PMTK000*32\r\n", 13);
            tcdrain(fd);
            write(fd, "$PUBX,00*33\r\n", 13);
            tcdrain(fd);
        }
        while ((rdretry != 0))
        {
            if(exitcount >12)
            {
                syslog(LOG_INFO,"GPSCONF:end NO GPS DEVICE\n");
                exit(0);
            }
            usleep(100000);
            memset(tmpbuf,0,TMPBUFSZ);
            rdret = read(fd,tmpbuf,TMPBUFSZ);
            if(rdret > 10)
                rdretry--;
            else if(rdret < 0)
            {
                exitcount++;
                continue;
            }
            for(i=0; i < rdret; i++)
            {
                if(tmpbuf[i] == '$')
                {
                    if((tmpbuf[i+1] == 'P') || (tmpbuf[i+1]=='G'))
                    {
                        current_baud = baud_array[j];
                        if(tmpbuf[i+2] == 'U')
                            devtype = 1;
                        else if (tmpbuf[i+2] == 'M')
                            devtype = 2;
                    }
                }
            }
        }
        j++;
    }
    if(current_baud == 38400)
        devtype = 2;
    if(current_baud == 0 || devtype == 0)
    {
        (void)syslog(LOG_INFO, "GPSCONF:: cannot find current baud or devtype%d %d\n",current_baud,devtype);
        printf("gpsconf ERROR:: cannot find current baud or devtype%d %d\n",current_baud,devtype);
    }
    syslog(LOG_INFO,"GPSCONF:currentbaud=%d devtype=%d\n",current_baud,devtype);

	if(devtype != 2){
	mode_set = mode_default_buf(send_bytes,DevMode);
        (void)ubx_write(0x06u, 0x24, send_bytes, mode_set); /* set automotive mode in port's settings */
        (void)usleep(50000);
	}

    if(current_baud)
    {

        if(speed != current_baud && speed != 0)
        {
            if(devtype == 2)
            {
                tcflush(fd,TCIOFLUSH);
                n = write(fd, "$PMTK251,115200*1F\r\n", 20); //BaudRate
                // (void)tcdrain(fd);

            }
            else
            {
                len=speed_default_buf(send_bytes);
                putlelong(send_bytes, 8, speed);
                (void)ubx_write(0x06u, 0x00, send_bytes, len); /* get this port's settings */
            }
            (void)usleep(50000);
            Settings(speed);

        }

        if(update_rate)
        {
            if(devtype == 2)
            {
                tcflush(fd,TCIOFLUSH);
                n = write(fd, "$PMTK220,200*2C\r\n", 17); //Fix Update Rate
                // (void)tcdrain(fd);
            }
            else{
                ubx_res = updaterate_default_buf(send_bytes);
                putlelong(send_bytes, 0, update_rate);
                (void)ubx_write(0x06u, 0x08, send_bytes, ubx_res);
            }
            (void)usleep(50000);
        }

        if(waas)
        {
            if(devtype == 2)
            {
                tcflush(fd,TCIOFLUSH);
                n = write(fd, "$PMTK313,1*2E\r\n", 15); //Enable WAAS
                n = write(fd, "$PMTK301,2*2E\r\n", 15);
                //(void)tcdrain(fd);
            }
            else{
                ubx_res = waas_default_buf(send_bytes);
                send_bytes[0] = 1;
                (void)ubx_write(0x06u, 0x16, send_bytes, ubx_res);
            }
        }
        else
        {
            if(devtype == 2)
            {
                tcflush(fd,TCIOFLUSH);
                n = write(fd, "$PMTK313,0*2F\r\n", 15); //WAAS DISABLE
                n = write(fd, "$PMTK301,0*2C\r\n", 15);
                // (void)tcdrain(fd);
            }
            else{
                ubx_res = waas_default_buf(send_bytes);
                (void)ubx_write(0x06u, 0x16, send_bytes, ubx_res);
            }

        }
        if(devtype == 2 )
        {
            tcflush(fd,TCIOFLUSH);
            write(fd,"$PMTK314,1,1,1,1,5,5,0,0,0,0,0,0,0,0,0,0,0,0,0*28\r\n" ,49);//gsv & gsa  after5*200ms 
            n = write(fd, "$PMTK414*33\r\n", 13);
            n = write(fd, "$PMTK400*36\r\n", 13);
            n = write(fd, "$PMTK602*36\r\n", 13);
            n = write(fd, "$PMTK401*37\r\n", 13);
            n = write(fd, "$PMTK419*3E\r\n", 13);
            n = write(fd, "$PMTK490*3F\r\n", 13);
            n = write(fd, "$PMTK430*35\r\n", 13);
            // (void)tcdrain(fd);
        }
        else
        {
            tcflush(fd,TCIOFLUSH);
            ubx_res = disable_gsv_buf(send_bytes,gsvrr);
            (void)ubx_write(0x06u, 0x01, send_bytes, ubx_res); //CFG-MSG command
            send_bytes[1] = 0x02; //making GSA repeatrate same as GSV repeatrate
            (void)ubx_write(0x06u, 0x01, send_bytes, ubx_res); //CFG-MSG command
            ubx_res = cfgsave_default_buf(send_bytes);
            (void)ubx_write(0x06u, 0x09, send_bytes, ubx_res);
            // (void)tcdrain(fd);
        }
    }
    close( fd );
    syslog(LOG_INFO,"GPSCONF:done configuring GPS dev\n");
    return(0);

}
