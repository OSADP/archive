
#include <stdio.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <fcntl.h>
#include <stdlib.h>
#include <sys/types.h>
#include <inttypes.h>   /* C99 specifies this file */
#include <dirent.h>   
#include <string.h>   
#include <unistd.h>   
#include <endian.h>  
#include <signal.h>  
#include "wave.h" 
#include <BasicSafetyMessage.h>
#include <math.h>

#define FRONT 1
#define BACK 2
#define MAC_ADDRESS_LEN 6
#define NO_OF_MAC_ADDRESS 10
#define STRAIGHT 0
#define LEFT_CURVE 1
#define RIGHT_CURVE 2
#define REQ_ACCELERATION -3.9

enum BOOLEAN {true, false};
double A[3][3];
double P[3][3];
double Q, R[1][1];
double dt,MEAS_STDDEV; 
double TURNENTER_THRESHOLD_VEL = 10.0; //Changed from 4.0, 
double TURNEXIT_THRESHOLD_VEL = 3.5; //in meters/sec
double CORRECTION_FACTOR = 1.4; // was 6.0 for the fixed correction;
double MANEOUVRE_FACTOR = 10;
double MANEOUVRE_THRESHOLD = 1.5;
double ErrorVector[1][1];
double StateVector[3][1];
double K[3][1];
enum BOOLEAN IsFirst;
enum BOOLEAN IN_TURN = false;
int first_entry = 1;

void check_msg_count(void *, void * , int);
void print_display(void *);
uint8_t new_rate_set_t[] = {0,6,9,12,18,24,36,48,54,72,96,108};
unsigned char zerobuf[6] = { 0 };
int prev;
int after;
struct timeval msg_int;
double  msginterval;
int lat;
int lng;
double latrx;
double lngrx;
int rx_time;
double distance;
float LANE_WIDTH=0.0;
extern double pi ;
extern double pidiv180 ;
extern double one80divpi ;
extern double R1 ; //square of constant 6378137
extern double R2 ;//square of constant 6356752.3142
extern double R2divR1;
extern double distance_cal(double,double,double,double,double);
void degtodms(double,double ,GPSData);
float ComputeBearing(double,double,double,double);
double ComputeDistance(GPSData *,BasicSafetyMessage_t *,int,int);
uint8_t FindDirection(double,float);
double Lane_Detection(int,int,GPSData *,BasicSafetyMessage_t *);
int CurveSpeed_check(char *,GPSData*);

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
//	int msgintavg;  // avg msg_interval
//	int msgintmax;  // max msg_interval
//	int msgintmin;  // min msg_interval
	double msgintavg;  // avg msg_interval
	double msgintmax;  // max msg_interval
	double msgintmin;  // min msg_interval
	
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
	double msginterval_t;
        int diff_msgint;
		
};


struct mac_addr {
        unsigned char macaddress[NO_OF_MAC_ADDRESS][MAC_ADDRESS_LEN];
        struct pktstats pkt_sts[NO_OF_MAC_ADDRESS];
};

struct mac_addr address; 

struct EllipsoidalCoords {
        double lat;
        double lon;
        double elev; //above ellipsoidal
        };

struct ECEFCoords {
        double X;
        double Y;
        double Z;
        };
struct ECEFCoords ECEFcoord;

extern void fnConvertEllipsoidalToECEF(double latitude,double longitude,double altitude,struct ECEFCoords *ptrToECEFCoords);
extern double fnComputeDistanceFromPointToALine(struct ECEFCoords *A, struct ECEFCoords *B,struct ECEFCoords *C, double magnOfVectorAC);


void parse_packet(void *rxpkt ,void *wsm_gps,int print_sec ,void *type)
{
    
    WSMIndication *wsmind = (WSMIndication *)rxpkt;
    static int first_time_addr = 1;
    unsigned char first_mac[6];
    uint8_t i,j,k,addr_ret;
    GPSData *wsmgps = (GPSData *)wsm_gps;
    BasicSafetyMessage_t *bsmLog;
    bsmLog = (BasicSafetyMessage_t *)type;	
   
   // gettimeofday(&msg_int,NULL);
    msginterval = (wsmgps->actual_time)*1000 ;

    if(first_time_addr == 1)
    {	
        for(i=0;i<10;i++)
        address.pkt_sts[i].first_time = 1;
	wsmgps->time = get_time(wsmgps->actual_time);
        prev = (((unsigned int)wsmgps->time) % 100) + ((((unsigned int)wsmgps->time/100)%100)*60);
	memset(address.macaddress,0,sizeof((address.macaddress[0][0])*NO_OF_MAC_ADDRESS*MAC_ADDRESS_LEN));
	memcpy(first_mac,wsmind->macaddr,MAC_ADDRESS_LEN); 
	memcpy(address.macaddress[0],wsmind->macaddr,MAC_ADDRESS_LEN); 
	first_time_addr =0;
	i=0;
        
    }

   if( (memcmp(&address.macaddress[NO_OF_MAC_ADDRESS-1][0],zerobuf,MAC_ADDRESS_LEN) == 0 ) )
   {
       for(k=0;k<NO_OF_MAC_ADDRESS;k++)
       {
           if(memcmp(address.macaddress[k],wsmind->macaddr,MAC_ADDRESS_LEN) == 0)
	       break;
	   else if(memcmp(address.macaddress[k],zerobuf,MAC_ADDRESS_LEN) == 0)
	   {	  
	       memcpy(address.macaddress[k],wsmind->macaddr,MAC_ADDRESS_LEN);	
	       break;
	   }
       }
	       
   }	

   if(first_time_addr == 0)
   {
       for(j=0;j<NO_OF_MAC_ADDRESS;j++)
       {
           addr_ret = memcmp(address.macaddress[j],wsmind->macaddr,MAC_ADDRESS_LEN);
           if(addr_ret ==0 )
           {
               i = j;
               break;
           }
       }
   }


   if((addr_ret ==0) || (i==0))     
   {
	      // printf("msgintervalmax = %lf \n",msginterval); 
	      // printf("first_time = %d \n",address.pkt_sts[i].first_time); 
	      // printf("msgintervalmax = %lf \n",address.pkt_sts[i].msginterval_t); 
	
               if(address.pkt_sts[i].first_time == 1)
	       {
		   address.pkt_sts[i].msginterval_t = msginterval;
		   address.pkt_sts[i].txpowmin = wsmind->chaninfo.txpower;
		   //address.pkt_sts[i].txpowmax = wsmind->chaninfo.txpower;
		   if(*(unsigned char *)&wsmind->data.contents[1] > 0x80)
		   {
		       address.pkt_sts[i].msdusizemin = wsmind->data.contents[2];
		   }
		   else
		   {
		       address.pkt_sts[i].msdusizemin = wsmind->data.contents[1];
		   }
		   address.pkt_sts[i].rssimin = wsmind->rssi;
                   address.pkt_sts[i].dataratemin = new_rate_set_t[wsmind->chaninfo.rate]/2; 
	           //address.pkt_sts[i].rssimax = wsmind->rssi;
                   //address.pkt_sts[i].dataratemax = new_rate_set_t[wsmind->chaninfo.rate]; 
	          // first_time = 0;       
	       }
	      
	       //Tx Power	
	       if(( wsmind->chaninfo.txpower < address.pkt_sts[i].txpowmin))
	       {
	           address.pkt_sts[i].txpowmin = wsmind->chaninfo.txpower;
	       }		
	       else if(wsmind->chaninfo.txpower > address.pkt_sts[i].txpowmax )
	       {

	           address.pkt_sts[i].txpowmax = wsmind->chaninfo.txpower;
	       }
	       //RSSI	
	       if((wsmind->rssi < address.pkt_sts[i].rssimin))
	       {
	           address.pkt_sts[i].rssimin = wsmind->rssi;
	       }		
	       else if(wsmind->rssi > address.pkt_sts[i].rssimax )
	       {
	           address.pkt_sts[i].rssimax = wsmind->rssi;
	       }		
	       //DATA_RATE
	       if((new_rate_set_t[wsmind->chaninfo.rate]/2)< address.pkt_sts[i].dataratemin )
	       {
	           address.pkt_sts[i].dataratemin = new_rate_set_t[wsmind->chaninfo.rate]/2;
	       }
	       else if(new_rate_set_t[wsmind->chaninfo.rate]/2 > address.pkt_sts[i].dataratemax )
	       {
		   address.pkt_sts[i].dataratemax = new_rate_set_t[wsmind->chaninfo.rate]/2;
	       }		
	       //Msg Length	      
	      if(*(unsigned char *)&wsmind->data.contents[1] > 0x80)
	      {
	          if(address.pkt_sts[i].msdusizemin > wsmind->data.contents[2])
		      address.pkt_sts[i].msdusizemin = wsmind->data.contents[2];
	          else if(address.pkt_sts[i].msdusizemax < wsmind->data.contents[2])
	              address.pkt_sts[i].msdusizemax = wsmind->data.contents[2];
	              
		      address.pkt_sts[i].msdusizeavg = address.pkt_sts[i].msdusizeavg+wsmind->data.contents[2];
	      }
	      else
	      {
	          if(address.pkt_sts[i].msdusizemin > wsmind->data.contents[1])
		      address.pkt_sts[i].msdusizemin = wsmind->data.contents[1];
	          else if(address.pkt_sts[i].msdusizemax < wsmind->data.contents[1])
	          {    
		      address.pkt_sts[i].msdusizemax = wsmind->data.contents[1];
		   
                  }   
		      address.pkt_sts[i].msdusizeavg = address.pkt_sts[i].msdusizeavg + wsmind->data.contents[1];
	      }
	      //Msg Interval
#if 1
	 
              if(address.pkt_sts[i].diff_msgint == 0)
	      {    
	          address.pkt_sts[i].msgintmin = msginterval - address.pkt_sts[i].msginterval_t;
	          address.pkt_sts[i].diff_msgint = 1;
              }  
	      address.pkt_sts[i].msgintavg = address.pkt_sts[i].msgintavg + (msginterval-address.pkt_sts[i].msginterval_t);
#endif
	       //printf("msgintervalmax = %lf \n",address.pkt_sts[i].msgintmax); 
	       //printf("msgintervalmin = %lf \n",address.pkt_sts[i].msgintmin); 

	      if(msginterval - address.pkt_sts[i].msginterval_t > address.pkt_sts[i].msgintmax)	
	          address.pkt_sts[i].msgintmax = msginterval - address.pkt_sts[i].msginterval_t;
	      else if(msginterval - address.pkt_sts[i].msginterval_t < address.pkt_sts[i].msgintmin)	
	          address.pkt_sts[i].msgintmin = msginterval - address.pkt_sts[i].msginterval_t;
		      
	      address.pkt_sts[i].msginterval_t = msginterval;	


	       address.pkt_sts[i].txpowavg  = address.pkt_sts[i].txpowavg + wsmind->chaninfo.txpower; 
	       address.pkt_sts[i].datarateavg  = address.pkt_sts[i].datarateavg + (new_rate_set_t[wsmind->chaninfo.rate]/2); 
	       address.pkt_sts[i].rssiavg  = address.pkt_sts[i].rssiavg + wsmind->rssi;
               address.pkt_sts[i].totalpkts++; 
 		           
	      check_msg_count(&address, wsmind , i);
               
	      address.pkt_sts[i].first_time = 0;       
	       
               //printf("No of packets %x \n",address.pkt_sts[i].totalpkts++);
    }

   memcpy(&lat,bsmLog->blob1.buf+7,4);
   memcpy(&lng,bsmLog->blob1.buf+11,4);
   memcpy(&rx_time,bsmLog->blob1.buf+5,2);
   latrx = (double)lat/10000000; 
   lngrx = (double)lng/10000000;
#if 0
   printf("Long:%lf\n",lngrx);
   printf("Latitude_wsm:%lf\n",wsmgps->latitude);
   printf("Long_wsm:%lf\n",wsmgps->longitude);
   printf("Alt_wsm:%lf\n",wsmgps->altitude);
#endif
  // printf("rx_time %d \n",rx_time);   
  // printf("wsmgps_time %lf \n",wsmgps->actual_time);   

   distance = distance_cal(latrx,lngrx,wsmgps->latitude,wsmgps->longitude,wsmgps->altitude); 
   degtodms(latrx,lngrx,*wsmgps);
   printf("[Time:%lf] [Speed:%lf][Distance(meters):%lf][RSSI:%u]\n",wsmgps->time,wsmgps->speed,distance,wsmind->rssi);
   printf("\n");
   wsmgps->time = get_time(wsmgps->actual_time);
   after = ((unsigned int )wsmgps->time % 100) + ((((unsigned int)wsmgps->time/100)%100)*60);	
 // printf("prev = %d after = %d \n",prev,after);
    if((after - prev) == print_sec)
        print_display(&address);

}

void check_msg_count(void *address_t, void *rd_pkt , int i)
{

    struct mac_addr *address = (struct mac_addr *)address_t;
    WSMIndication *rdpkt= (WSMIndication *)rd_pkt;
    int msg_cnt = 7;
	       //printf(" msg_cnt content = %d \n",rdpkt->data.contents[7]); 

    if(address->pkt_sts[i].first_time == 1)
    {	
            if((unsigned char)rdpkt->data.contents[1] > 0x80)
                address->pkt_sts[i].first_msg_cnt = rdpkt->data.contents[msg_cnt+1];    //msg_cnt byte 213
            else
                address->pkt_sts[i].first_msg_cnt = rdpkt->data.contents[msg_cnt];    //msg_cnt byte 213
    } 
    if(address->pkt_sts[i].first_time == 0)
    {
        if((unsigned char )rdpkt->data.contents[1] > 0x80)
	{
                if(((unsigned char )rdpkt->data.contents[msg_cnt+1]) - (address->pkt_sts[i].first_msg_cnt)!=1)
                {
                    if(((unsigned char )rdpkt->data.contents[msg_cnt+1]) - (address->pkt_sts[i].first_msg_cnt) < 0)

                        address->pkt_sts[i].pkts_lost = address->pkt_sts[i].pkts_lost + (127 + ((unsigned char)rdpkt->data.contents[msg_cnt+1] - (address->pkt_sts[i].first_msg_cnt))+ ((unsigned char )rdpkt->data.contents[msg_cnt+1])) -1;
                     else
                        address->pkt_sts[i].pkts_lost = address->pkt_sts[i].pkts_lost + ((unsigned char )rdpkt->data.contents[msg_cnt+1] - (address->pkt_sts[i].first_msg_cnt)) -1 ;
                    address->pkt_sts[i].first_msg_cnt = (unsigned char )rdpkt->data.contents[msg_cnt+1];
                }
                else
                {
                    if(((unsigned char )rdpkt->data.contents[msg_cnt+1]) == 127)
                    {
                        address->pkt_sts[i].pkts_lost++;
                        address->pkt_sts[i].first_msg_cnt =0;
                    }
                    else
                        address->pkt_sts[i].first_msg_cnt = address->pkt_sts[i].first_msg_cnt +1;
                }
        } 
        else 
	{
	         //printf(" msg_cnt = %d \n",(unsigned char )rdpkt->data.contents[msg_cnt]); 
                if(((unsigned char)rdpkt->data.contents[msg_cnt]) - (address->pkt_sts[i].first_msg_cnt)!=1)
                {
                    if(((unsigned char )rdpkt->data.contents[msg_cnt]) - (address->pkt_sts[i].first_msg_cnt) < 0)

                        address->pkt_sts[i].pkts_lost = address->pkt_sts[i].pkts_lost + (127 + ((unsigned char )rdpkt->data.contents[msg_cnt] - (address->pkt_sts[i].first_msg_cnt))+ ((unsigned char )rdpkt->data.contents[msg_cnt])) -1;
                     else
                        address->pkt_sts[i].pkts_lost = address->pkt_sts[i].pkts_lost + ((unsigned char )rdpkt->data.contents[msg_cnt] - (address->pkt_sts[i].first_msg_cnt)) -1 ;
                    address->pkt_sts[i].first_msg_cnt = (unsigned char)rdpkt->data.contents[msg_cnt];
                }
                else
                {
                    if(((unsigned char )rdpkt->data.contents[msg_cnt]) == 127)
                    {
                        address->pkt_sts[i].pkts_lost++;
                        address->pkt_sts[i].first_msg_cnt =0;
                    }
                    else
                        address->pkt_sts[i].first_msg_cnt = address->pkt_sts[i].first_msg_cnt +1;
                }
        } 

    }
	     //  printf(" pkt_lost = %d \n",address->pkt_sts[i].pkts_lost); 
	     //  printf(" pkt_total = %d \n",address->pkt_sts[i].totalpkts); 

}


void print_display(void *address_t)
{

   int i;   
   struct mac_addr *address = (struct mac_addr *)address_t;

   for(i=0;i<10;i++)
   {
   if(address->pkt_sts[i].pkts_lost ==-1 )
       address->pkt_sts[i].pkts_lost =0;
 
   if( (memcmp(&address->macaddress[i][0],zerobuf,MAC_ADDRESS_LEN) != 0 ) )
   {
      printf("\n");
   //printf("pkts_display %d i= %d\n",address->pkt_sts[i].pkts_lost);
      printf("*****Mac address***   %02x:%02x:%02x:%02x:%02x:%02x  \n",address->macaddress[i][0],address->macaddress[i][1],address->macaddress[i][2],address->macaddress[i][3],address->macaddress[i][4],address->macaddress[i][5]);
      printf("\n");

      printf("Total WSMP Pkts \n");
      printf("     Expected  : %d\n",address->pkt_sts[i].totalpkts + address->pkt_sts[i].pkts_lost);
      printf("     Received  : %d\n",address->pkt_sts[i].totalpkts);
      printf("     Lost      :(%f) percent \n ",(float)address->pkt_sts[i].pkts_lost*100/(address->pkt_sts[i].totalpkts + address->pkt_sts[i].pkts_lost));
      
      printf("\n");
      printf("Txpower(db)\t\t");
      printf("RSSI (db)\t\t");
      printf("Data rate (Mb/s)\t\t");
      printf("\n");
      printf("     avg  is : %d\t", (address->pkt_sts[i].txpowavg/(address->pkt_sts[i].totalpkts)));
      printf("     avg  is : %d\t", (address->pkt_sts[i].rssiavg/(address->pkt_sts[i].totalpkts)));
      printf("     avg  is : %d\t\t", (address->pkt_sts[i].datarateavg/(address->pkt_sts[i].totalpkts)));
      printf("\n");
      printf("     min  is : %d\t", address->pkt_sts[i].txpowmin);
      printf("     min  is : %d\t", address->pkt_sts[i].rssimin);
      printf("     min  is : %d\t\t", address->pkt_sts[i].dataratemin);
      printf("\n");
      printf("     max  is : %d\t", address->pkt_sts[i].txpowmax);
      printf("     max  is : %d\t", address->pkt_sts[i].rssimax);
      printf("     max  is : %d\t\t", address->pkt_sts[i].dataratemax);
      printf("\t");
      printf("\n");
      printf("\n");
#if 1     
      printf("Msg Size (bytes)\t");
      printf("Msg Interval (msecs)\n");
      printf("     avg  is : %d\t", (address->pkt_sts[i].msdusizeavg)/(address->pkt_sts[i].totalpkts));
      printf("     avg  is : %d\n", (int)((address->pkt_sts[i].msgintavg)/(address->pkt_sts[i].totalpkts)));
      printf("     min  is : %d\t", (address->pkt_sts[i].msdusizemin));
      printf("     min  is : %d\n", (int)(address->pkt_sts[i].msgintmin));
      printf("     max  is : %d \t",(address->pkt_sts[i].msdusizemax));
      printf("     max  is : %d\n", (int)(address->pkt_sts[i].msgintmax));
#endif
      printf("\n");
   }
   }
   prev = after;
}

void degtodms(double lat_t,double lng_t,GPSData wsmgps)
{

    GPSData *wsmgps_t = (GPSData *)&wsmgps;
    int d_lat=0,m_lat=0,s_lat=0;
    int d_lng=0,m_lng=0,s_lng=0;
   // printf("Latiude_rx [%lf]  Latitude_self [%lf]\n",lat_t,wsmgps_t->latitude);
   // printf("Longitde_rx [%lf] Longitude_self [%lf]\n",lng_t,wsmgps_t->longitude);
    d_lat = (int)lat_t; 	
    lat_t = lat_t - d_lat;
    lat_t = lat_t *60;
    m_lat = (int)lat_t; 	
    lat_t = lat_t-m_lat;
    lat_t = lat_t *60;
    s_lat = (int)lat_t;
    d_lng = (int)lng_t; 	
    lng_t = lng_t - d_lng;
    lng_t = lng_t *60;
    m_lng = (int)lng_t; 	
    lng_t = lng_t-m_lng;
    lng_t = lng_t *60;
    s_lng = (int)lng_t;
    
    printf("Received: [Lat:%c%d.%d.%d  Long:%c%d.%d.%d]\n",((d_lat < 0)? 'S' : 'N'),((d_lat < 0)? -d_lat : d_lat),((m_lat < 0)? -m_lat : m_lat),((s_lat < 0)? -s_lat : s_lat),((d_lng < 0)? 'W' : 'E'),((d_lng < 0)? -d_lng : d_lng),((m_lng < 0)? -m_lng : m_lng),((s_lng < 0)? -s_lng : s_lng));


    d_lat=0;m_lat=0;s_lat=0;
    d_lng=0;m_lng=0;s_lng=0;
    d_lat = (int)wsmgps_t->latitude; 	
    wsmgps_t->latitude = wsmgps_t->latitude - d_lat;
    wsmgps_t->latitude = wsmgps_t->latitude *60;
    m_lat = (int)wsmgps_t->latitude; 	
    wsmgps_t->latitude = wsmgps_t->latitude-m_lat;
    wsmgps_t->latitude = wsmgps_t->latitude *60;
    s_lat = (int)wsmgps_t->latitude;
    d_lng = (int)wsmgps_t->longitude; 	
    wsmgps_t->longitude = wsmgps_t->longitude - d_lng;
    wsmgps_t->longitude = wsmgps_t->longitude *60;
    m_lng = (int)wsmgps_t->longitude; 	
    wsmgps_t->longitude = wsmgps_t->longitude-m_lng;
    wsmgps_t->longitude = wsmgps_t->longitude *60;
    s_lng = (int)wsmgps_t->longitude;
    printf("Self:     [Lat:%c%d.%d.%d  Long:%c%d.%d.%d]\n",((d_lat < 0)? 'S' : 'N'),((d_lat < 0)? -d_lat : d_lat),((m_lat < 0)? -m_lat : m_lat),((s_lat < 0)? -s_lat : s_lat),((d_lng < 0)? 'W' : 'E'),((d_lng < 0)? -d_lng : d_lng),((m_lng < 0)? -m_lng : m_lng),((s_lng < 0)? -s_lng : s_lng));
    	    

}

#if 0
double distance_vk(double lat1,double lon1,double lat2,double lon2)
{
    double theta,dist;
    theta = lon1-lon2;
    dist = sin(deg2rad(lat1))*sin(deg2rad(lat2))+cos(deg2rad(lat1))*cos(deg2rad(theta));
    dist = acos(dist);
    dist = rad2deg(dist);
    dist = dist * 60 *1.1515;
    dist = dist * 1.609344;
    dist = dist * 1000;
    return dist;
}
#endif
union per_point_4 {
        struct val_offsets_4 {
                unsigned lat_Offset  :18;
                unsigned long_Offset :18;
                unsigned elev_Offset :12;
                unsigned time_Offset :16;
        }__attribute__((packed))lat_long_elev;
        unsigned char tell[8];//time,elev,lat,lon
}RxValue;
struct offset{
    int latoffset;
    int longoffset;
    int elevationoffset;
    int timeoffset;
} offset_var;

float RV_ProcessData(GPSData *GpsStructure,BasicSafetyMessage_t *BsmStructure,float lane_width)
{
    double Latitude,Longitude;
    double RemoteSpeed;
    float ttc = 0.0,Angle,RelativeSpeed=0.0;
    double RemoteDistance=0.0;
    uint8_t Direction,RxSpeed[2];
    double laneDistance;
    float acceleration,lane_ratio;
    int16_t acceleration_raw;

    LANE_WIDTH = lane_width;
    memcpy(&lat,BsmStructure->blob1.buf+7,4);
    memcpy(&lng,BsmStructure->blob1.buf+11,4);
    memcpy(&RxSpeed,BsmStructure->blob1.buf+21,2);
    Latitude = (double) lat /10000000;//remote current position
    Longitude= (double) lng /10000000;
    //printf("Host:%lf,%lf Remote:%lf,%lf\n",GpsStructure->latitude,GpsStructure->longitude,Latitude,Longitude);

//find out that Remote vehicle is in front or back side use computation only if its front side 
    Angle = ComputeBearing(GpsStructure->latitude,GpsStructure->longitude,Latitude,Longitude);

// ref 0 deg as north assume 4 quadrants i)0 to 90 ii)90 to 180 iii)180 to 270 iv)270 to 360
    Direction = FindDirection(GpsStructure->course,Angle); //returns whether remote point is at forward/backward direction

    if(Direction == FRONT)
    {
        laneDistance = Lane_Detection(lat,lng,GpsStructure,BsmStructure);
	if(laneDistance == -1111.0){
	    return -1111.0;
	}
	lane_ratio = 0.5 * LANE_WIDTH;
	//check condition to accept lane 
	if(laneDistance < (LANE_WIDTH - lane_ratio))
	{
		//printf("\nSAME LANE\n");
            RemoteDistance = ComputeDistance(GpsStructure,BsmStructure,lat,lng);
	    if(RemoteDistance == -1111.0)
		return RemoteDistance;
    	    RemoteSpeed = ((((RxSpeed[0] & 0x1F) << 8) | RxSpeed[1] )*0.02);
    	    RelativeSpeed = (float)(fabs(GpsStructure->speed-RemoteSpeed));
    	    if(RelativeSpeed == 0.0)
		return -2222.0;
	    ttc = (float)(RemoteDistance/RelativeSpeed);	
		return ttc;
	}//if
	else if(laneDistance > (LANE_WIDTH - lane_ratio)  && laneDistance < LANE_WIDTH + LANE_WIDTH - lane_ratio )
	{
		//printf("\nADJACENT LANE\n");
	    memcpy(&acceleration_raw,BsmStructure->blob1.buf+26,2);
            acceleration = (float)(acceleration_raw/10.0);
	    if(acceleration < REQ_ACCELERATION || ( (BsmStructure->safetyExt!=NULL) \
					       && (BsmStructure->safetyExt->events != NULL) \
					       && (BsmStructure->safetyExt->events[0] == 128)))
	    {
	        return -7777.0;
	    }
	    return -6666.0;
	}
	else{
		//printf("\nSECOND LANE\n");
	    return -6666.0;
	}
    }
    else if(Direction == BACK)
        return  -9999.0;
    else{
    //    printf("Direction not found for Remote Vehicle\n");
        return -8888.0;
    }
}


void CalculateOffset(void){
    int tempoffset = 0;
    offset_var.latoffset = (int)RxValue.tell[0];
    offset_var.latoffset <<= 10;
    tempoffset = (int) RxValue.tell[1];
    tempoffset <<= 2;
    tempoffset &= 0xfffffffc;
    tempoffset = tempoffset | (int)((RxValue.tell[2] & 0xc0) >> 6);
    offset_var.latoffset = offset_var.latoffset | tempoffset;
    tempoffset = 0;

    offset_var.longoffset = (int)RxValue.tell[2] & 0x3f;
    offset_var.longoffset <<= 12;
    tempoffset = (int)RxValue.tell[3];
    tempoffset <<= 4;
    tempoffset &= 0xfffffffc;
    tempoffset = tempoffset | (int)((RxValue.tell[4] & 0xf0) >> 4);
    offset_var.longoffset = offset_var.longoffset | tempoffset;
    
}


int find_latitude(int CurrentLat){
    int tmp,latt;
    tmp = offset_var.latoffset;
    tmp = tmp & 0x00020000;//preserve sign bit
    offset_var.latoffset &= 0x0001FFFF;
    if(tmp !=0){
        offset_var.latoffset = -((0x0001ffff - offset_var.latoffset)+1);
    }
    latt = CurrentLat - offset_var.latoffset;
    return latt;
}

int find_longitude(int CurrentLon){
    int tmp,longi;
    tmp = offset_var.longoffset;
    tmp = tmp & 0x00020000;//preserve sign bit
    offset_var.longoffset &= 0x0001FFFF;
    if(tmp !=0){
        offset_var.longoffset =-( (0x0001ffff - offset_var.longoffset)+1);
    }
    longi = CurrentLon - offset_var.longoffset;
    return longi;
}

float ComputeBearing(double Lat1,double Lon1,double Lat2,double Lon2){
    float x_vector,y_vector,angle=0.0;
    Lat1 = pidiv180 * Lat1;
    Lon1 = pidiv180 * Lon1;
    Lat2 = pidiv180 * Lat2;
    Lon2 = pidiv180 * Lon2;
    x_vector = (float) ((cos(Lat1)*sin(Lat2))-(sin(Lat1)*cos(Lat2)*cos(Lon2-Lon1)));
    y_vector = (float) sin(Lon2-Lon1)*cos(Lat2);

    if(y_vector > 0.0){
        if(x_vector == 0.0)
            angle = 90.0;
        else
            angle = (float)one80divpi*atan2(y_vector,x_vector);
    }

    if(y_vector < 0.0){
        if(x_vector == 0.0)
            angle = 270.0;
        else
            angle = (float)one80divpi*atan2(y_vector,x_vector);
    }

    if(y_vector == 0.0){
        if(x_vector > 0.0)
            angle = 0.0;
        if(x_vector < 0.0)
            angle = 180.0;
        if(x_vector == 0.0) //two points are same
            angle = 0.0;
    }
    if(angle < 0.0)
        angle = 360 + angle;	
    return angle;
}

double ComputeDistance(GPSData *GpsInformation,BasicSafetyMessage_t *bsm,int Latitude,int Longitude){
    uint8_t count =0,direction;
    double lat2,lon2;
    double lat1,lon1;
    float BearingAngle;
    double Distance = 0.0;
    lat1 = (double)Latitude/10000000;//lat of rv
    lon1 = (double)Longitude/10000000;//lon of rv
    if(bsm->safetyExt != NULL && bsm->safetyExt->pathHistory != NULL && bsm->safetyExt->pathHistory->itemCnt[0] > 0){			
        for(count=0;count<bsm->safetyExt->pathHistory->itemCnt[0];count++){
            memcpy(RxValue.tell,bsm->safetyExt->pathHistory->crumbData.choice.pathHistoryPointSets_04.buf+(count*8),8);//simplify count*8
            (void)CalculateOffset();
            lat2 =(double) find_latitude(Latitude)/10000000;//lat of ph(rv)
            lon2 =(double) find_longitude(Longitude)/10000000;//lon of ph(rv)
            BearingAngle = ComputeBearing(GpsInformation->latitude,GpsInformation->longitude,lat2,lon2);//between Host and path history points
	    direction = FindDirection(GpsInformation->course,BearingAngle);
	    if(direction == FRONT){
	        Distance += distance_cal(lat1,lon1,lat2,lon2,GpsInformation->altitude);
		lat1 = lat2;
		lon1 = lon2;
	    }
	    else{
	        Distance += distance_cal(GpsInformation->latitude,GpsInformation->longitude,lat1,lon1,GpsInformation->altitude);
		return Distance;
            }
	    
        } 
	Distance += distance_cal(GpsInformation->latitude,GpsInformation->longitude,lat1,lon1,GpsInformation->altitude);
	return Distance;
    }
    return -1111.0;
}

uint8_t FindDirection(double Heading,float Bearing){//could be more simplified

    if(Heading >= 0.0 && Heading <= 90.0){//1st quadrant
        if(Bearing >= 135.0 && Bearing <= 315.0){ // && becoz its continous range
	    return BACK;
        }
        else{
	    return FRONT;
        }
    }
    else if(Heading > 90.0 && Heading <= 180.0){//2nd quadrant
        if(Bearing >= 225.0 || Bearing <= 45.0){ // 225 to 360 or 0 to 45
	    return BACK;
        }
        else{
	    return FRONT;

        }
    }
    else if(Heading > 180.0 && Heading <= 270.0){//3rd quadrant
        if(Bearing >= 315.0 || Bearing <= 135.0){ // 315 to 360 or 0 to 135
	    return BACK;
        }
        else{
	    return FRONT;
        }
    }    
    else if(Heading > 270.0 && Heading <= 360.0){//4th quadrant
        if(Bearing <= 225.0 && Bearing >= 45.0){ // 45 to 225
	    return BACK;
        }
        else{
	    return FRONT;
        }

    }
    return 0;
}

double Lane_Detection(int lat,int lng,GPSData *wsm_gps,BasicSafetyMessage_t *bsm)
{
    struct ECEFCoords PointA,PointB,PointC;
    double MagofAC,Perp_distance;	
    int RetrievedLatitude,RetrievedLongitude;
    double ph_Latitude,ph_Longitude;
    double lat_rx,lng_rx;
    float BearingAngle;
    uint8_t count = 0,direction;
    struct EllipsoidalCoords RV_pathpoints[2]; 

    lat_rx = (double)lat/10000000;
    lng_rx = (double)lng/10000000;

    if(bsm->safetyExt != NULL && bsm->safetyExt->pathHistory != NULL && bsm->safetyExt->pathHistory->itemCnt[0] > 0){
	RV_pathpoints[0].lat = lat_rx;
	RV_pathpoints[0].lon = lng_rx;

	for(count=0;count<bsm->safetyExt->pathHistory->itemCnt[0];count++){
            memcpy(RxValue.tell,bsm->safetyExt->pathHistory->crumbData.choice.pathHistoryPointSets_04.buf + (count*8),8);//simplify count*8
            CalculateOffset();
            RetrievedLatitude=find_latitude(lat);
            RetrievedLongitude=find_longitude(lng);
                                                               
            ph_Latitude = (double)RetrievedLatitude/10000000;
            ph_Longitude = (double)RetrievedLongitude/10000000;

	    BearingAngle = ComputeBearing(wsm_gps->latitude,wsm_gps->longitude,ph_Latitude,ph_Longitude);//between Host and path history points
	    direction = FindDirection(wsm_gps->course,BearingAngle);

 	    if(count == 0){
		RV_pathpoints[1].lat = ph_Latitude;
		RV_pathpoints[1].lon = ph_Longitude;
	    }
	    else{
		RV_pathpoints[0].lat = RV_pathpoints[1].lat;
		RV_pathpoints[0].lon = RV_pathpoints[1].lon;
		RV_pathpoints[1].lat = ph_Latitude;
		RV_pathpoints[1].lon = ph_Longitude;
	    }
	    if(direction != FRONT){
		break;
	    }
	}//for
	if(count >= bsm->safetyExt->pathHistory->itemCnt[0]){
	    return -1111.0;
	}
    }//if
    else 
	return -1111.0;                       

    fnConvertEllipsoidalToECEF(RV_pathpoints[0].lat,RV_pathpoints[0].lon,0,&PointA);
    fnConvertEllipsoidalToECEF(wsm_gps->latitude,wsm_gps->longitude,0,&PointB);
    fnConvertEllipsoidalToECEF(RV_pathpoints[1].lat,RV_pathpoints[1].lon,0,&PointC);
  
    MagofAC = sqrt(((PointC.X - PointA.X)*(PointC.X - PointA.X)) + ((PointC.Y - PointA.Y)*(PointC.Y - PointA.Y)) + ((PointC.Z - PointA.Z)*(PointC.Z - PointA.Z))); 
    Perp_distance = fnComputeDistanceFromPointToALine(&PointA, &PointB, &PointC,MagofAC);
    return Perp_distance;
}

int CurveSpeed_check(char * contents,GPSData *wsmgps)
{
    int msgid;
    int speedid,speedid1,advisory_speed;
    int unitid,unitid1;

    msgid = (int)contents[5];
    if(msgid == 16){  

        memcpy(&speedid,contents + 149 ,2);
        speedid1 = ntohl(speedid);
        speedid1 =((0x0000FFFF)&(speedid1 >> 16)); //first 2 bytes are needed

        advisory_speed = (int)contents[158];

        memcpy(&unitid,contents + 165 ,2);//taken first two bytes for dev_id else tempid is 4 bytes
        unitid1 = ntohl(unitid);
        unitid1 =((0x0000FFFF)&(unitid1 >> 16)); //first 2 bytes are needed

        if(unitid1 == 8721){
                advisory_speed = (int)round(advisory_speed * 5 / 18);
        }
        if((advisory_speed < wsmgps->speed) && (wsmgps->speed != GPS_INVALID_DATA)){
               // printf("\nWARNING: Current Speed %lf mps > Advisory Speed %d mps\n",wsmgps->speed,advisory_speed);
                return 1;
        }
        else if (wsmgps->speed == GPS_INVALID_DATA){
                return 2;
        }
        return 3;
    }
    return 0;
}

void fnInit(double Measurement_StdDev, double SampleInterval, double acceleration_noise_variatiance)
{
    /*
    Measurement_StdDev is in degrees
    SampleInterval is in seconds
    acceleration_noise_variatiance is in (meters per second^2)^2
    */
    dt = SampleInterval;
    MEAS_STDDEV = Measurement_StdDev;
    Q = acceleration_noise_variatiance;
    R[0][0] = MEAS_STDDEV * MEAS_STDDEV;
    IsFirst = true;

    A[0][0] = 1.0; A[0][1] = dt;   A[0][2] = 0.5*dt*dt;
    A[1][0] = 0;    A[1][1] = 1.0; A[1][2] = dt;
    A[2][0] = 0;    A[2][1] = 0;    A[2][2] = 1.0;

    P[0][0] = 5.0;  P[0][1] = 0.0; P[0][2] = 0.0;
    P[1][0] = 0.0;  P[1][1] = 5.0; P[1][2] = 0.0;
    P[2][0] = 0.0;  P[2][1] = 0.0; P[2][2] = 5.0;

    StateVector[0][0] = 0;
    StateVector[1][0] = 0;
    StateVector[2][0] = 0;
}

double fnKalmanFilter_Heading(double MeasuredHeading)
{
    double Heading = 0;
    enum BOOLEAN MeasurementIn1stQuad, MeasurementIn2ndQuad;
    enum BOOLEAN EstimateIn1stQuad, EstimateIn2ndQuad;
    enum BOOLEAN CrossingNorth;

    double P_predicted[3][3];
    double StateVector_predicted[3][1];
    double Heading_EstimationErr, Heading_dot, Correction;
	
    if(first_entry == 1){
	fnInit(0.5,0.1,0.1); // Meas_STDDev = 0.5 , SampleInterval = 0.1  , acceleration_noice_variatiance
	first_entry = 0;
    }

    if (true == IsFirst)
    {
        IsFirst = false;
        StateVector[0][0] = MeasuredHeading;
    }
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%Check if we are crossing North
    if (0 <= MeasuredHeading && 90 >= MeasuredHeading)
        MeasurementIn1stQuad = true;
    else
        MeasurementIn1stQuad = false;

    if (360 >= MeasuredHeading && 270 <= MeasuredHeading)
        MeasurementIn2ndQuad = true;
    else
        MeasurementIn2ndQuad = false;

    if (0 <= StateVector[0][0] && 90 >= StateVector[0][0])
        EstimateIn1stQuad = true;
    else
        EstimateIn1stQuad = false;

    if (360 >= StateVector[0][0] && 270 <= StateVector[0][0])
        EstimateIn2ndQuad = true;
    else
        EstimateIn2ndQuad = false;

    if((true == EstimateIn2ndQuad && true == MeasurementIn1stQuad)||(true == EstimateIn1stQuad && true == MeasurementIn2ndQuad))
        CrossingNorth = true;
    else
        CrossingNorth = false;

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    if (false == CrossingNorth) 
    {
        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    
        //%    StateVector_predicted = A*StateVector; 
	StateVector_predicted[0][0] = StateVector[0][0] + A[0][1] * StateVector[1][0] + A[0][2] * StateVector[2][0];  
	StateVector_predicted[1][0] = StateVector[1][0] + A[1][2] * StateVector[2][0];  
	StateVector_predicted[2][0] = StateVector[2][0];  

        //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
	//%     P_predicted = A*P*A' + Q;
	//%         A*P*A' = [1 dt 0.5*dt^2;   [P(1,1) P(1,2) P(1,3);      [1        0   0;
	//%                   0   1    dt;   X  P(2,1) P(2,2) P(2,3);   X   dt       1   0;
	//%                   0   0    1];      P(3,1) P(3,2) P(3,3)];     0.5*dt^2  dt  1];
	//%
	//%Covariance matrices have to be symettric. So we can work with just one set of of-diagonal terms 
	//%and the diagonal terms
	//%We could use the fact that the diagonal elements of A and A' are 1 and the former is upper dagonal; 
	//%the latter is lower diagonal
	//%
	//% Only Q(3,3) is non-zero. Hence there is only one add
        //    %P*A'
        P[0][0] = P[0][0] + dt * P[0][1] + 0.5 * dt * dt * P[0][2];
        P[0][1] = P[0][1] + dt * P[0][2];
        P[0][2] = P[0][2];

        P[1][0] = P[1][0] + dt * P[1][1] + 0.5 * dt * dt * P[1][2];
        P[1][1] = P[1][1] + dt * P[1][2];
        P[1][2] = P[1][2];

        P[2][0] = P[2][0] + dt * P[2][1] + 0.5 * dt * dt * P[2][2];
        P[2][1] = P[2][1] + dt * P[2][2];
        P[2][2] = P[2][2];

        //%Now A*(P*A')
        P_predicted[0][0] = P[0][0] + dt * P[1][0] + 0.5 * dt * dt * P[2][0];
        P_predicted[0][1] = P[0][1] + dt * P[1][1] + 0.5 * dt * dt * P[2][1];
        P_predicted[0][2] = P[0][2] + dt * P[1][2] + 0.5 * dt * dt * P[2][2];

	//%     P_predicted[1][0] = P[1][0] + dt * P[2][0];
	P_predicted[1][0] = P_predicted[0][1];
        P_predicted[1][1] = P[1][1] + dt * P[2][1];
        P_predicted[1][2] = P[1][2] + dt * P[2][2];

	//%     P_predicted[2][0] = P[2][0];
	//%     P_predicted[2][1] = P[2][1];
	P_predicted[2][0] = P_predicted[0][2];
        P_predicted[2][1] = P_predicted[1][2];
        P_predicted[2][2] = P[2][2] + Q;

	//%Not much advantage in using the symetric property of covariance matrices.
	//%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
	//%    ErrorVector = (MeasurementVector - H*StateVector_predicted);
	ErrorVector[0][0] = MeasuredHeading - StateVector_predicted[0][0];

        Heading_EstimationErr = -ErrorVector[0][0]; //%Negated as I am following the convention the Error = (Observed - True)	
	if((MANEOUVRE_THRESHOLD * MEAS_STDDEV) < fabs(Heading_EstimationErr))
        {
            //P_predicted = 10.0*eye(3)*P_predicted;
            P_predicted[0][0] = MANEOUVRE_FACTOR * P_predicted[0][0]; P_predicted[0][1] = 10.0 * P_predicted[0][1] ;  P_predicted[0][2] = 10.0 * P_predicted[0][2];
            P_predicted[1][0] = MANEOUVRE_FACTOR * P_predicted[1][0]; P_predicted[1][1] = 10.0 * P_predicted[1][1] ;  P_predicted[1][2] = 10.0 * P_predicted[1][2];
            P_predicted[2][0] = MANEOUVRE_FACTOR * P_predicted[2][0]; P_predicted[2][1] = 10.0 * P_predicted[2][1] ;  P_predicted[2][2] = 10.0 * P_predicted[2][2];
         //printf("Maneouvre Detected\n");
         }

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
    //%     K = P_predicted*H'*inv(H*P_predicted*H' + R)
    //% %         K = [pp(1,1)/(pp(1,1)+ R(1,1))
    //%                pp(2,1)/(pp(1,1)+ R(1,1))    
    //%                pp(3,1)/(pp(1,1)+ R(1,1))];
	K[0][0] = P_predicted[0][0]/(P_predicted[0][0] + R[0][0]);
        K[1][0] = P_predicted[1][0]/(P_predicted[0][0] + R[0][0]);
        K[2][0] = P_predicted[2][0]/(P_predicted[0][0] + R[0][0]);

    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    
    //%     StateVector = StateVector_predicted + K*ErrorVector;
    //%     [s(1,1);      [sp(1,1) +  pp(1,1)*(m(1,1) - sp(1,1))/(p(1,1)+ R(1,1));
    //%      s(2,1); =     sp(2,1) +  pp(2,1)*(m(1,1) - sp(1,1))/(p(1,1)+ R(1,1));
    //%      s(3,1)]       sp(3,1) +  pp(3,1)*(m(1,1) - sp(1,1))/(p(1,1)+ R(1,1))];
	StateVector[0][0] = StateVector_predicted[0][0] + K[0][0]*ErrorVector[0][0];
        StateVector[1][0] = StateVector_predicted[1][0] + K[1][0]*ErrorVector[0][0];
        StateVector[2][0] = StateVector_predicted[2][0] + K[2][0]*ErrorVector[0][0];

    //printf("\nState_Vector = %lf ,K[0][0] = %lf, P_predicted[0][0]: %lf , R[0][0]: %lf\n",StateVector[0][0],K[0][0],P_predicted[0][0],R[0][0]);
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    
    //%     P = P_predicted - K*H*P_predicted;   
    //%         K*H*P_predicted =  [ K(1,1);    [pp(1,1) pp(1,2) pp(1,3)];
    //%                              K(2,1); X 
    //%                              K(3,1)];

	P[0][0] = P_predicted[0][0] - K[0][0]*P_predicted[0][0];
        P[0][1] = P_predicted[0][1] - K[0][0]*P_predicted[0][1];
        P[0][2] = P_predicted[0][2] - K[0][0]*P_predicted[0][2];

        P[1][0] = P_predicted[1][0] - K[1][0]*P_predicted[0][0];
        P[1][1] = P_predicted[1][1] - K[1][0]*P_predicted[0][1];
        P[1][2] = P_predicted[1][2] - K[1][0]*P_predicted[0][2];

        P[2][0] = P_predicted[2][0] - K[2][0]*P_predicted[0][0];
        P[2][1] = P_predicted[2][1] - K[2][0]*P_predicted[0][1];
        P[2][2] = P_predicted[2][2] - K[2][0]*P_predicted[0][2];

    //    Cov = P;
    //%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    
    //%     printf('Kalman Filter Done\n');
    }
    else
    {
    //    %Coast
    //%    StateVector_predicted = A*StateVector;
	StateVector_predicted[0][0] = StateVector[0][0] + A[0][1] * StateVector[1][0] + A[0][2] * StateVector[2][0];  
        StateVector_predicted[1][0] = StateVector[1][0] + A[1][2] * StateVector[2][0];  
        StateVector_predicted[2][0] = StateVector[2][0];

        //P_predicted = A*P*A' + Q;
	P[0][0] = P[0][0] + dt * P[0][1] + 0.5 * dt * dt * P[0][2];
        P[0][1] = P[0][1] + dt * P[0][2];
        P[0][2] = P[0][2];

        P[1][0] = P[1][0] + dt * P[1][1] + 0.5 * dt * dt * P[1][2];
        P[1][1] = P[1][1] + dt * P[1][2];
        P[1][2] = P[1][2];

        P[2][0] = P[2][0] + dt * P[2][1] + 0.5 * dt * dt * P[2][2];
        P[2][1] = P[2][1] + dt * P[2][2];
        P[2][2] = P[2][2];

        //%Now A*(P*A')
        P_predicted[0][0] = P[0][0] + dt * P[1][0] + 0.5 * dt * dt * P[2][0];
        P_predicted[0][1] = P[0][1] + dt * P[1][1] + 0.5 * dt * dt * P[2][1];
        P_predicted[0][2] = P[0][2] + dt * P[1][2] + 0.5 * dt * dt * P[2][2];

    //%     P_predicted[1][0] = P[1][0] + dt * P[2][0];
	P_predicted[1][0] = P_predicted[0][1];
        P_predicted[1][1] = P[1][1] + dt * P[2][1];
        P_predicted[1][2] = P[1][2] + dt * P[2][2];

    //%     P_predicted[2][0] = P[2][0];
    //%     P_predicted[2][1] = P[2][1];
	P_predicted[2][0] = P_predicted[0][2];
        P_predicted[2][1] = P_predicted[1][2];
        P_predicted[2][2] = P[2][2] + Q;

        Heading_EstimationErr = 0;

        StateVector[0][0] = StateVector_predicted[0][0];
        StateVector[1][0] = StateVector_predicted[1][0];
        StateVector[2][0] = StateVector_predicted[2][0];

        P[0][0] = P_predicted[0][0];  P[0][1] = P_predicted[0][1];  P[0][2] = P_predicted[0][2];   
        P[1][0] = P_predicted[1][0];  P[1][1] = P_predicted[1][1];  P[1][2] = P_predicted[1][2];   
        P[2][0] = P_predicted[2][0];  P[2][1] = P_predicted[2][1];  P[2][2] = P_predicted[2][2];   
    }

    if (0 > StateVector[0][0])
    {
        StateVector[0][0] = StateVector[0][0] + 360;
    }

    if (360 <= StateVector[0][0])
    {
        StateVector[0][0] = StateVector[0][0] - 360;
    }

    Heading = StateVector[0][0];
    Heading_dot = StateVector[1][0];

    //Heading_dbldot = StateVector(3);
    //% printf('Heading Stored\n');

    if ((TURNENTER_THRESHOLD_VEL <= fabs(Heading_dot)) || true == IN_TURN)
    {
        IN_TURN = true;
	Correction = CORRECTION_FACTOR * fabs(Heading_dot) * dt;

        if (0 <= Heading_dot)
        {
            //Heading = Heading + CORRECTION_FACTOR * TURNENTER_THRESHOLD_VEL * dt;
	    Heading = Heading + Correction;
        }
        else
        {
            //Heading = Heading - CORRECTION_FACTOR * TURNENTER_THRESHOLD_VEL * dt;
	    Heading = Heading - Correction;
        }
     }

    if ((TURNEXIT_THRESHOLD_VEL >= fabs(Heading_dot)) && (true == IN_TURN))
    {
        IN_TURN = false;
    }

    if (0 > Heading)
    {
        Heading  += 360;
    }

    if (360 <= Heading)
    {
        Heading -= 360;
   }
    return Heading;
}
