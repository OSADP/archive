
//****************************** Compile this application for parsing


#include <stdio.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <stdlib.h>
#include <sys/types.h>
#include <inttypes.h>   /* C99 specifies this file */
#include <dirent.h>   
#include <string.h>   
#include <unistd.h>   
#include <endian.h>   


#define SECURITY 193
#define DIGEST 194
#define MSG_CNT_DIGEST 213
#define MSG_CNT_CERT 284
#define MSG_CNT_PLAIN 201

#define RSSI_OFFSET 68
#define DATA_RATE 116
#define TX_POW 188
#define MAC_ADDRESS_LEN 6	
#define MAC_ADDR 154	
#define NO_OF_MAC_ADDRESS 10	
#define PROTOCOL 144	



struct pktstats {
	
	int totalpkts; // total pkts based on expected msgCnt increment and num pkts logged
        int numpktsrecd; // few pkts may be lost, found from msgCnt
        int pkts_lost; // few pkts may be lost

	// For Msg interval
	int ipdavg; // avg inter pkt delay
	int ipdmax; // max inter pkt delay
	int ipdmin; // min inter pkt delay
       
        // For data rate
        int datarateavg; // datarate in 500kbps  (i guess it will be single data rate, if we find a change in data rate need to alert) 
        int dataratemax;     
        int dataratemin; 

        // For pkt size
        int msdusizeavg;  // avg msdu size  (expected to be constant)
        int msdusizemax;  // MAX msdu size  (expected to be constant)
        int msdusizemin;  // MIN msdu size  (expected to be constant) 
 
	// rx RSSI
	int rssiavg;  // avg rssi
	int rssimax;  // max rssi
	int rssimin;  // min rssi
	
	//Msg Interval(msecs)
	int msgintavg;  // avg msg_interval
	int msgintmax;  // max msg_interval
	int msgintmin;  // min msg_interval
	
	// Transmit power used
	int txpowavg;  // avg tx pow  // expected to be constant
	int txpowmax;  // max tx pow
	int txpowmin;  // min tx pow
	
	// Local GPS information
	//GPSData localgpsdata;
	// to compute distance
	// Recd gps data
	double lat;
	double lon;
	double speed;

	int first_time;
	int first_msg_cnt;
		
};

struct mac_addr {
        unsigned char macaddress[NO_OF_MAC_ADDRESS][MAC_ADDRESS_LEN];
        struct pktstats pkt_sts[NO_OF_MAC_ADDRESS];
};

struct mac_addr address; 

//struct pktstats pkt_sts;



struct pcap_file_header {
        unsigned int magic;
        u_short version_major;
        u_short version_minor;
        unsigned int thiszone;     /* gmt to local correction */
        unsigned int sigfigs;    /* accuracy of timestamps */
        unsigned int snaplen;    /* max length saved portion of each pkt */
        unsigned int linktype;   /* data link type (LINKTYPE_*) */
};

struct pcap_pkthdr {
        struct timeval ts;      /* time stamp */
        unsigned int caplen;     /* length of portion present */
        unsigned int len;        /* length this packet (off wire) */
};


unsigned char pcap_hdr[] = {
0xa1,0xb2,0xc3,0xd4,0x00,0x02,0x00,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xff,0xff,0x00,0x00,0x00,0x77
};

#define swap16_(x)       ((((x)&0x00FF)<<8) | (((x)>>8)&0x00FF))
#define swap32_(x)       ((((x)&0xFF)<<24)       \
                         |(((x)>>24)&0xFF)       \
                         |(((x)&0x0000FF00)<<8)  \
                         |(((x)&0x00FF0000)>>8)  )
#define swap64_(x)        ((((x)&0x00000000000000FF)<<56) \
                         |(((x)&0xFF00000000000000)>>56) \
                         |(((x)&0x000000000000FF00)<<40) \
                         |(((x)&0x00FF000000000000)>>40) \
                         |(((x)&0x0000000000FF0000)<<24) \
                         |(((x)&0x0000FF0000000000)>>24) \
                         |(((x)&0x00000000FF000000)<<8)  \
                         |(((x)&0x000000FF00000000)>>8))
#define swap64(x1) \
({ \
        unsigned long long x = (x1);\
        (((x & 0xFFULL) << 56| (x & 0xFF00000000000000ULL) >> 56| \
        (x & 0xFF00ULL) << 40| (x & 0xFF000000000000ULL) >> 40| \
        (x & 0xFF0000ULL) <<24| (x & 0xFF0000000000ULL) >> 24| \
        (x & 0xFF000000ULL) <<8 | (x & 0xFF00000000ULL) >>8 ));\
})



void swapGenericData(int size, void *data)
{
        switch(size) {
                case 2:
                        *(uint16_t*)data = swap16_(*(uint16_t*)data);
                break;

                case 4:
                        *(uint32_t*)data = swap32_(*(uint32_t*)data);
                break;

                case 8:
                        *(uint64_t*)data = swap64(*(uint64_t*)data);
                break;

        }
}


int main(int argc, char *argv[])
{
    int rdfd; //, wrfd;
    int sts;
    char rdval;
    int rdbytes=0, wrbytes, pktnum=0, pktlen,msginterval,txpower;
    struct pcap_file_header pfilehdr;
    struct pcap_pkthdr pkthdr;
    int byteswap=0;
    char rdpkt[1500];

    DIR *dir;
    struct dirent *ent;
    char filename[100];
    int first_time_addr = 1;
    uint8_t protocol_type;
    int wsa_pkt=0;
    int wsmp_pkt=0;int i;
    unsigned char first_mac[6]; 
    int addr_ret;
    int mac_full;
    static int k;	  	
    int j;
    unsigned char zerobuf[6] = { 0 };

    // 	printf("mac_addr %02x%02x%02x%02x%02x%02x  \n",mac_addr[0],mac_addr[1],mac_addr[2],mac_addr[3],mac_addr[4],mac_addr[5]);
    if (argc !=2)
    {
     	printf("%s <input file> \n", __FILE__);
	exit(0);
    }    

    dir = opendir (argv[1]);

    if (dir != NULL) {  
        while ((ent = readdir(dir)) != NULL) {

        if((strcmp(ent->d_name,".")==0) || (strcmp(ent->d_name,"..")==0))
        ;
        else
        {	
            strcpy(filename,argv[1]);
            strcat(filename,ent->d_name);  
            //printf("filename = %s, name = %s arg = %s \n", filename,ent->d_name,argv[1]);
            rdfd = open(filename, O_RDONLY);
            if (rdfd == -1)
            {
	        printf("Error opening file %s\n", argv[1]);
	        exit(0);
            }

	    for(i=0;i<10;i++)
	        address.pkt_sts[i].first_time = 1;
#if 0
    wrfd = open(argv[2], O_WRONLY|O_CREAT);
    if (wrfd == -1)
    {
	printf("Error opening file %s\n", argv[2]);
	exit(0);
    }
#endif
    // Read file header
           sts = read(rdfd, &pfilehdr, sizeof(pfilehdr));
           rdbytes = sizeof(pfilehdr);

         //  printf("Majic number %x\n", pfilehdr.magic); 
           if (pfilehdr.magic == 0xd4c3b2a1)
               byteswap = 1;

           do {
            // Read pkt header
               sts = read(rdfd, &pkthdr, sizeof(pkthdr));
               //printf("read bytes = %d \n",sts);
               if (sts < 0 ) break;
               rdbytes += sizeof(pkthdr);

               pktlen = pkthdr.len;
	       msginterval = pkthdr.ts.tv_usec;		
	       if(byteswap == 1)
	       {
                   swapGenericData(4, &pktlen);
                   swapGenericData(4, &msginterval);
	       }
               //printf("Pkt %d len %d\n \n", ++pktnum, pktlen);
               wrbytes ++, rdbytes++;
               sts = read(rdfd, rdpkt, pktlen);
	       txpower = *(unsigned char *)&rdpkt[TX_POW];
	       protocol_type = *(unsigned char *)&rdpkt[PROTOCOL];
	       if(protocol_type == 136)	
	           wsmp_pkt=1;
	       else if(protocol_type == 208)	
		   wsa_pkt=1;
		
if(wsmp_pkt == 1)
	       if(first_time_addr == 1)
	       {	
	           memset(address.macaddress,0,sizeof((address.macaddress[0][0])*NO_OF_MAC_ADDRESS*MAC_ADDRESS_LEN));
		   memcpy(first_mac,(unsigned char *)&rdpkt[MAC_ADDR],MAC_ADDRESS_LEN); 
	           memcpy(address.macaddress[0],(unsigned char *)&rdpkt[MAC_ADDR],MAC_ADDRESS_LEN); 
	           first_time_addr =0;
		   i=0;
	       }
	       
//************************ Filling mac address 
if(wsmp_pkt == 1)
              //printf("read bytes  22 \n");
	       if( (memcmp(&address.macaddress[NO_OF_MAC_ADDRESS-1][0],zerobuf,MAC_ADDRESS_LEN) == 0 ) )
	       {
	           for(k=0;k<NO_OF_MAC_ADDRESS;k++)
	           {    
		       if(memcmp(address.macaddress[k],(unsigned char *)&rdpkt[MAC_ADDR],MAC_ADDRESS_LEN) == 0)
		           break;
		       else if(memcmp(address.macaddress[k],zerobuf,MAC_ADDRESS_LEN) == 0)
		       {	  
			   memcpy(address.macaddress[k],(unsigned char *)&rdpkt[MAC_ADDR],MAC_ADDRESS_LEN);	
			   break;
		       }
                   }
	       
	       }	
	       else
	           printf("  Will not parse packet \n");		        
//************************ Comparison of mac address
if(wsmp_pkt == 1)
	       if(first_time_addr == 0)
	       {	
	           for(j=0;j<NO_OF_MAC_ADDRESS;j++)
	           {	 
	               addr_ret = memcmp(address.macaddress[j],(unsigned char *)&rdpkt[MAC_ADDR],MAC_ADDRESS_LEN); 
	               if(addr_ret ==0 )
	               {
	                   i = j;
                           break;	                   
	               }
                   }
	       }

//******************
if(wsmp_pkt == 1)
if((addr_ret ==0) || (i==0))     
{
	
               if(address.pkt_sts[i].first_time == 1)
	       {
		   address.pkt_sts[i].txpowmin = txpower;
		   address.pkt_sts[i].txpowmax = txpower;
		   address.pkt_sts[i].msgintmin = msginterval;
		   address.pkt_sts[i].msgintmax = msginterval;
		   address.pkt_sts[i].msdusizemin = pktlen;
		   address.pkt_sts[i].msdusizemax = pktlen;
		   address.pkt_sts[i].rssimin = rdpkt[RSSI_OFFSET];
                   address.pkt_sts[i].dataratemin = rdpkt[DATA_RATE]/2; 
	           address.pkt_sts[i].rssimax = rdpkt[RSSI_OFFSET];
                   address.pkt_sts[i].dataratemax = rdpkt[DATA_RATE]/2; 
	          // first_time = 0;       
	       }
	       
	       //Tx Power	
	       if((rdpkt[TX_POW] < address.pkt_sts[i].txpowmin))
	       {
	           address.pkt_sts[i].txpowmin = rdpkt[TX_POW];
	       }		
	       else if(rdpkt[TX_POW] > address.pkt_sts[i].txpowmax )
	       {
	           address.pkt_sts[i].txpowmax = rdpkt[TX_POW];
	       }		
	       //RSSI	
	       if((rdpkt[RSSI_OFFSET] < address.pkt_sts[i].rssimin))
	       {
	           address.pkt_sts[i].rssimin = rdpkt[RSSI_OFFSET];
	       }		
	       else if(rdpkt[RSSI_OFFSET] > address.pkt_sts[i].rssimax )
	       {
	           address.pkt_sts[i].rssimax = rdpkt[RSSI_OFFSET];
	       }		
	       //DATA_RATE
	       if(rdpkt[DATA_RATE]/2 < address.pkt_sts[i].dataratemin )
	       {
	           address.pkt_sts[i].dataratemin = rdpkt[DATA_RATE]/2;
	       }
	       else if(rdpkt[DATA_RATE]/2 > address.pkt_sts[i].dataratemax )
	       {
		   address.pkt_sts[i].dataratemax = rdpkt[DATA_RATE]/2;
	       }		
	       //Msg Len
	       if( pktlen < address.pkt_sts[i].msdusizemin )
	       {
	           address.pkt_sts[i].msdusizemin = pktlen ;
	       }
	       else if(pktlen > address.pkt_sts[i].msdusizemax )
	       {
		   address.pkt_sts[i].msdusizemax = pktlen;
	       }		
	       //Msg Interval
	       if( msginterval < address.pkt_sts[i].msgintmin )
	       {
	           address.pkt_sts[i].msgintmin = msginterval;
	       }
	       else if(msginterval > address.pkt_sts[i].msgintmax )
	       {
		   address.pkt_sts[i].msgintmax = msginterval;
	       }		
	       		
	       address.pkt_sts[i].txpowavg  = address.pkt_sts[i].txpowavg + txpower; 
	       address.pkt_sts[i].msgintavg  = address.pkt_sts[i].msgintavg + msginterval; 
	       address.pkt_sts[i].msdusizeavg  = address.pkt_sts[i].msdusizeavg + pktlen; 
	       address.pkt_sts[i].datarateavg  = address.pkt_sts[i].datarateavg + rdpkt[DATA_RATE]/2; 
	       address.pkt_sts[i].rssiavg  = address.pkt_sts[i].rssiavg + rdpkt[RSSI_OFFSET];
               address.pkt_sts[i].totalpkts++;  
	       
                           //printf("Msg size %d \n",pktlen);
	       if(wsmp_pkt == 1)
	       {
	           if(*(unsigned char *)&rdpkt[SECURITY] != 0x00) // To check whether is secured or unsecured
		   {
		       if(*(unsigned char *)&rdpkt[DIGEST] == 0x02)	// For digest
		       {
                          // printf("digest %d \n",rdpkt[213]);
                           if(address.pkt_sts[i].first_time == 1)
		               address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_DIGEST];    //msg_cnt byte 213
                           if(address.pkt_sts[i].first_time == 0)
			   {
			       if((*(unsigned char *)&rdpkt[MSG_CNT_DIGEST]) - (address.pkt_sts[i].first_msg_cnt)!=1) 
			       {
			           
			           if((*(unsigned char *)&rdpkt[MSG_CNT_DIGEST]) - (address.pkt_sts[i].first_msg_cnt) < 0) 
				      // pkts_lost = pkts_lost + 256 - (*(unsigned char *)&rdpkt[213] - (first_msg_cnt));
				       address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (127 + (*(unsigned char *)&rdpkt[MSG_CNT_DIGEST] - (address.pkt_sts[i].first_msg_cnt))+ (*(unsigned char *)&rdpkt[MSG_CNT_DIGEST]))-1;
				   else	
				       address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (*(unsigned char *)&rdpkt[MSG_CNT_DIGEST] - (address.pkt_sts[i].first_msg_cnt)) -1;
				   address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_DIGEST];	
			       }
                               else
			       {
			           if((*(unsigned char *)&rdpkt[MSG_CNT_DIGEST]) == 127)
				   {    
				       address.pkt_sts[i].pkts_lost++;	
				       address.pkt_sts[i].first_msg_cnt =0;
				   }
				   else		
                                       address.pkt_sts[i].first_msg_cnt = address.pkt_sts[i].first_msg_cnt +1;
			       }		
			       //printf("Lost packets -->cert  %d  %d \n",address.pkt_sts[i].pkts_lost,i);			
			   }

		       }
                       else if(*(unsigned char *)&rdpkt[DIGEST] == 0x03)   //For cert
		       {
                           //printf("cert %d \n",rdpkt[284]);
                           if(address.pkt_sts[i].first_time == 1)
		               address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_CERT];	 //msg_cnt byte 284
                           if(address.pkt_sts[i].first_time == 0)
			   {	
			       if(((*(unsigned char *)&rdpkt[MSG_CNT_CERT]) - (address.pkt_sts[i].first_msg_cnt))!=1) 
			       {
			           if((*(unsigned char *)&rdpkt[MSG_CNT_CERT]) - (address.pkt_sts[i].first_msg_cnt) < 0) 
				       //pkts_lost = pkts_lost + 256 + (*(unsigned char *)&rdpkt[284] - (first_msg_cnt));
				       address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (127 + (*(unsigned char *)&rdpkt[MSG_CNT_CERT] - (address.pkt_sts[i].first_msg_cnt))+ (*(unsigned char *)&rdpkt[MSG_CNT_CERT])) -1;
				       	
				   else	
			               address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (*(unsigned char *)&rdpkt[MSG_CNT_CERT] - (address.pkt_sts[i].first_msg_cnt)) - 1;
				   address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_CERT];	
			       }
                               else
			       {
			           if((*(unsigned char *)&rdpkt[MSG_CNT_CERT]) == 127)
				   {	
				       address.pkt_sts[i].pkts_lost++;	
				       address.pkt_sts[i].first_msg_cnt =0;
				   }
				   else	
                                       address.pkt_sts[i].first_msg_cnt = address.pkt_sts[i].first_msg_cnt +1;
			       }		
			       //printf("Lost packets -->digest  %d  %d \n",address.pkt_sts[i].pkts_lost,i);			
		            } 
			}	
		   }
	           else if(*(unsigned char *)&rdpkt[SECURITY] == 0x00)
		   {
                       //printf("plain %d \n",rdpkt[201]);
                       if(address.pkt_sts[i].first_time == 1)
		           address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_PLAIN];    //msg_cnt byte 213
                           if(address.pkt_sts[i].first_time == 0)
			   {
			       if((*(unsigned char *)&rdpkt[MSG_CNT_PLAIN]) - (address.pkt_sts[i].first_msg_cnt)!=1) 
			       {
			           if((*(unsigned char *)&rdpkt[MSG_CNT_PLAIN]) - (address.pkt_sts[i].first_msg_cnt) < 0) 
				       //pkts_lost = pkts_lost + 256 - (*(unsigned char *)&rdpkt[201] - (first_msg_cnt));
				       address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (127 + (*(unsigned char *)&rdpkt[MSG_CNT_PLAIN] - (address.pkt_sts[i].first_msg_cnt))+ (*(unsigned char *)&rdpkt[MSG_CNT_PLAIN])) -1;
				   else	
			               address.pkt_sts[i].pkts_lost = address.pkt_sts[i].pkts_lost + (*(unsigned char *)&rdpkt[MSG_CNT_PLAIN] - (address.pkt_sts[i].first_msg_cnt)) -1 ;
				   address.pkt_sts[i].first_msg_cnt = *(unsigned char *)&rdpkt[MSG_CNT_PLAIN];	
			       }
                               else
			       {
			           if((*(unsigned char *)&rdpkt[MSG_CNT_PLAIN]) == 127)
				   {	
				       address.pkt_sts[i].pkts_lost++;	
				       address.pkt_sts[i].first_msg_cnt =0;
				   }
				   else	
                                       address.pkt_sts[i].first_msg_cnt = address.pkt_sts[i].first_msg_cnt +1;
			       }
			       //printf("Lost packets -->plain   %d %d \n",address.pkt_sts[i].pkts_lost,i);			
		            }
		   } 
		  wsmp_pkt = 0;	
	          address.pkt_sts[i].first_time = 0;       
	
	       }
               //printf("rssi avg %x \n",rdpkt[RSSI_OFFSET]);
           
} //addr_ret

	  } while (sts > 0);

          // printf("File status %x: rdbytes %d wrbytes %d\n", sts, rdbytes, wrbytes);
           close(rdfd);
           strcpy(filename,argv[1]);
 
         } // else 
      } // while dir
      closedir (dir);

for(i=0;i<10;i++)
{
   if( (memcmp(&address.macaddress[i][0],zerobuf,MAC_ADDRESS_LEN) != 0 ) )
   {
      printf("\n");	
      printf("*****Mac address***   %02x:%02x:%02x:%02x:%02x:%02x  \n",address.macaddress[i][0],address.macaddress[i][1],address.macaddress[i][2],address.macaddress[i][3],address.macaddress[i][4],address.macaddress[i][5]);
      printf("\n");	

      printf("Total WSMP Pkts \n");
      printf("     Expected  : %d\n",address.pkt_sts[i].totalpkts-1 + address.pkt_sts[i].pkts_lost);
      printf("     Received  : %d\n",address.pkt_sts[i].totalpkts-1);
      printf("     Lost      :(%f) percent \n ",(float)address.pkt_sts[i].pkts_lost*100/(address.pkt_sts[i].totalpkts-1 + address.pkt_sts[i].pkts_lost));
      printf("\n");	
#if 1
      printf("Txpower(db)\t\t");
      printf("RSSI (db)\t\t");
      printf("Data rate (500kbps)\t\t");
      printf("Msg Size (bytes)\t\t");
      printf("Msg Interval (msecs)\n");
      printf("     avg  is : %d\t", (address.pkt_sts[i].txpowavg/(address.pkt_sts[i].totalpkts-1)));
      printf("     avg  is : %d\t", (address.pkt_sts[i].rssiavg/(address.pkt_sts[i].totalpkts-1)));
      printf("     avg  is : %d\t\t", (address.pkt_sts[i].datarateavg/(address.pkt_sts[i].totalpkts-1)));
      printf("     avg  is : %d\t\t", (address.pkt_sts[i].msdusizeavg)/(address.pkt_sts[i].totalpkts-1));
      printf("     avg  is : %d\n", ((address.pkt_sts[i].msgintavg)/(address.pkt_sts[i].totalpkts-1))/1000);
      printf("     min  is : %d\t", address.pkt_sts[i].txpowmin);
      printf("     min  is : %d\t", address.pkt_sts[i].rssimin);
      printf("     min  is : %d\t\t", address.pkt_sts[i].dataratemin);
      printf("     min  is : %d\t\t", address.pkt_sts[i].msdusizemin);
      printf("     min  is : %d\n", (address.pkt_sts[i].msgintmin)/1000);
      printf("     max  is : %d\t", address.pkt_sts[i].txpowmax);
      printf("     max  is : %d\t", address.pkt_sts[i].dataratemax);
      printf("     max  is : %d\t\t", address.pkt_sts[i].rssimax);
      printf("     max  is : %d\t\t", address.pkt_sts[i].msdusizemax);
      printf("     max  is : %d\n", (address.pkt_sts[i].msgintmax)/1000);
      printf("\t");	
#endif
      printf("\n");	
   }
}
    } //if dir
#if 0 
    close(wrfd);
#endif
}

