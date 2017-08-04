/*

 * Copyright (c) 2005-2007 Arada Syatems, Inc. All rights reserved.

 * Proprietary and Confidential Material.

 *

 */
#include <pthread.h>
#include <stdio.h>
#include <ctype.h>
#include <termio.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/ioctl.h>
#include <time.h>
#include <signal.h>
#include <stdlib.h>
#include <sys/syslog.h>
#include <tgmath.h>
#include "wave.h"
#include "wavelogger.h" //pvk
#include <asnwave.h>
#include <asn_application.h>
#include <asn_internal.h>
#include <BasicSafetyMessage.h>
#include <RoadSideAlert.h>
#include <ProbeVehicleData.h>
#include <crc.h>
#include "tool_def.h"
#include "AsmDef.h"
#include "can_gds.h"
#include <IntersectionCollision.h>
#include <MapData.h>
#include <SPAT.h>
#include <semaphore.h>
#include "genericAPI.h"
#include <math.h>
#include <TravelerInformation.h>
#include <RoadSignID.h>
#include <FurtherInfoID.h>
#include <ShapePointSet.h>
#include <NodeList.h>
#include <Offsets.h>
#include <ITIScodesAndText.h>
#include <ITIScodes.h>
#include <ValidRegion.h>
#include <errno.h>
#include <sys/mount.h>
#include <dirent.h>
#include <sys/stat.h>
#include <sys/syscall.h>
#include "fileparser.h"
#include "pathhist_defs.h"
#include "LocoParser.h"
#include "wnodehash.h"
#include "CertParse.h"
#include <EmergencyVehicleAlert.h>
#include <AlaCarte.h>

// Battelle  - wwg
#include <sys/file.h>
#include "SituationalApp.h"
#include "WaveRadio.h"
#include "VehicleData.h"
//#include "TimeStamp.h"
// End Battelle

/**DEFINITION SECTION**/

#define HIBYTE(t) (t >> 8) & 0xFF
#define LOBYTE(t) t & 0xFF
#define ENABLED 1
#define USER 0
#define PROVIDER 1
#define TX_PACKET 1
#define RX_PACKET 2
#define RATESET_NUM_ELMS 12
#define GPS_UPDATE_INT 200
#define MAXHEADING_BUF_LENGTH 6 //used in path prediction confidence calculation
#define INVALID_YAW_RATE -50000
#define TIMEOUT_SEC 10
//TX RX option flags: 7,6,5,4-Reserved Bit, 3-RXUDP, 2-RXALL, 1-RX,  0-TX
#define TXRX 0x05	//00000101
#define NOTX 0x04	//00000100
#define NORXALL 0x01	//00000001
#define NORX 0x03	//00000011
#define TXRXUDP 0x09	//00001001
#define NOTXRX 0x00	//00000000
#define TX_MASK 0x01	//00000001
#define RX_MASK 0x02	//00000010
#define RXALL_MASK 0x04	//00000100
#define RXUDP_MASK 0x08	//00001000
#define INTER_SIZE 200
#define SPEED_LATCH 0.56
#define SPEED_UNLATCH 0.83
//Display Type
#define INFO_DISPLAY 0
#define BLUETOOTH_DISPLAY 1
#define LED_DISPLAY 2
#define SERVICE_PORT 0 

//Battelle defined
#define TIM_TIMEOUT 1//000000

/**GLOBAL VARIABLE AND FUNCTION DECLARATION**/
char logbuf_tx[1024];
char logbuf[1024];
unsigned char ALL = 0;
int latitudeoffset;
int longitudeoffset;
int elevationoffset;
int timeoffset;
int hb_event_flag = 0;
int supress_hb_events = 0;
short int sec_16;
uint16_t ph_sec16;
double pi = 3.14159265358979323846;
double pidiv180 = 0.017453293;
double one80divpi = 57.295779513;
const double ELLIPSOID_MAJOR_AXIS = 6378137.0;
const double ELLIPSOID_MINOR_AXIS = 6356752.3142;
double R1 = 40680631590769.0; //square of constant 6378137
double R2 = 40408299984087.05552164; //square of constant 6356752.3142
double R2divR1 = 0.99330562;
double SqrOfTheEccentricity = 0.00669438; // SqrOfTheEccentricity = 1 - R2divR1;
typedef signed short int16_t;
int16_t lon_acc_accl = 0, yaw_rate = 0, sintg16;
uint8_t hb_flag = 0;
uint8_t heading_count = 0;
double heading_buff[20];
double pp_yaw[5], pp_yawrate;
long pp_roc, pp_confidence;
uint8_t lon_acc_flag = 1;
double lon_acc_ref_sec = 0, ref_time, ref_speed;
uint32_t yaw_head, ref_yaw;
double lon_acc_speed;
uint32_t min_msec, hb_time = 0;
long latitude_val;
long longitude_val;
uint32_t heading_val;
uint32_t latch_heading_val = 28800;
uint8_t transmission_speed[2];
uint8_t year_val;
uint8_t month_val;
uint8_t day_val;
uint8_t hour_val;
uint8_t minute_val;
uint8_t elevation_val[2];
uint8_t vehicle__type = 0;
int temp_id_control = 1; //default is random
extern uint32_t mod_depl_dev_id; //default model deployment device id
int TIME_OUT = TIMEOUT_SEC;
int bsm2data = 10;
int num = 0;
int dist_val;
uint16_t path_num = 4;
uint8_t FrameType = 1;//TravelerInfoType_unknown; //0
extern sem_t can_sem;
sem_t rse_sem, rse_sem2;
uint8_t threadCount = 0;
pthread_t thread_id = 0, bluethread = 0, gpsthread = 0, btCan_tId = 0,
		rxCan_tId = 0;
struct sockaddr_in6 server_addr;
uint8_t Tx_Now = 0, WAVEDEVICE = WAVEDEVICE_LOCAL;
char ifname[30] = "eth0", RemoteIp[128] = "127.0.0.1";
uint32_t CurTime = 0, PrevTime = 0;
int RemoteEnable = 0;
uint16_t sock_port = 0;
static int asmStatus_tx = 00, asmStatus_rx = 00;
static char can_interface[20] = "vcan0";
int CrtVrfyFlag = 1;
struct wnode wnodelist[MAX_LIST];
struct wnodehash whlist[MAX_HASH_SIZE];

uint8_t temp_id[4];
// Global msgCnt to reset upon cert exp
int msgCnt = 0;
// cert changed flag to send the new cert
int certchanged = 0;
can_GDSData_t candata;
int sock;
int randstatus = 0;
extern asn_TYPE_descriptor_t *asn_pdu_collection[];
typedef struct RoadSideAlert__description description_t;
int m_itisItems = 9;
ITIScodes_t itiscodes[9] = { 513, 514, 517, 519, 520, 521, 522, 524, 525 };

char signerType[][50] = { "self", "certificate_digest_with_ecdsap224",
		"certificate_digest_with_ecdsap256", "certificate", "certificate_chain",
		"certificate_digest_with_other_algorithm" };

u_int8_t random_srcmacaddr[IEEE80211_ADDR_LEN] = { [ 0 ... (IEEE80211_ADDR_LEN
		- 1) ] = 0 };
float TTC = 0.0;
double ph_lat1 = 0;
double ph_lon1 = 0;
double distance = 0;
double prevlat_ip[2] = { 0.0, 0.0 };
double prevlon_ip[2] = { 0.0, 0.0 };
double prevcourse_ip[2] = { 0.0, 0.0 };
int first_ip = 1;
int interpol_start = 0;
double prev_acttime = 0; //prev_actualtime to update prev-data points used for interpolation
int64_t tsfdiff = 0; //tsfdiff from prev actual gps packet sent
int64_t requery_diff = 0; //time since gpsc requery started
uint8_t requery_start = 0; //indicating start of gpsc requery
uint64_t requery_start_time = 0; //start time of gpsc requery
uint64_t tsfref = 0; // tsfref of actual gps point sent
uint8_t AlertStatus = 0; //Safety Application Alert Status
int PKT_DELAY_SECS = 0;
long int PKT_DELAY_NSECS = 0;
uint8_t app_ip_sch_permit = 0;
uint8_t is_HeadingLatch = 0;

WSMHDR wsmhdr;
enum {
	ADDR_MAC = 0, UINT8_T
};

//static PSTEntry entry;
//static USTEntry ust;
static WMEApplicationRequest wreq;
static WMEApplicationRequest entry, entry2;
static WMEApplicationRequest ust, ust2;
static WSMRequest wsmreq;
//static WSMRequest tmp_wsmreq;
//static WMECancelTxRequest cancelReq;
static WMEWRSSRequest wrssrq;
static WMETARequest tareq;
void receiveWME_NotifIndication(WMENotificationIndication *wmeindication);
void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication);
void receiveTsfTimerIndication(TSFTimer *timer);
void receiveWSMIndication(WSMIndication *ind);
int confirmBeforeJoin(WMEApplicationIndication *);
int check_mac(char *mac);
extern void get_RSE_options(void *);
extern int Get_LOCOMATE_Options(LocomateOptions *);
extern int fetch_vehicleData(LocomateVehicleSpecs *);
extern void *main_bluetooth(void *);
extern void *rx_can_client(void);
extern void sig_can(void);
extern int bt_write(char*, int);
extern int write_to_can(char *);
extern int get_canOption(char*, char*);
extern void sig_int_bluetooth(void);
int buildWSMRequestData();
int buildBSMRequestData(); //Battelle added
int buildTIMRequestData(); //Battelle added
int buildWSMRequestData_first();
int buildPSTEntry();
int buildWSMRequestPacket();
int buildWMEApplicationRequest();
int buildWMETARequest();
int txWSMPPkts(int, int);
void wrss_request();

void sig_int(void);
void sig_segv(void);
void sig_term(void);
#ifdef IF_NECESSARY
void asm_restart(void);
#endif
int extract_macaddr(u_int8_t *, char *);
void set_args(void *, void *, int);

static pthread_t localtx, localrx, txrx_udp, wrssi; //gpsthread;
static void *gps_update(void *data);
static void *udp_client(void *data);
static void *tx_client(void *data);
static void *rx_client(void *data);
static void *wrssi_client(void *arg);
//static void *gps_client( void *data );
void wsa_conf_parse(void);
int calculation_of_offsets();
int AsmDecodeContentType(WSMIndication *rxpkt);
int AsmSignData();
int AsmEncryptData();
int AsmVerifyData(WSMIndication *rxpkt);
int AsmDecryptData(WSMIndication *rxpkt);
int32_t certParse(uint8_t *, Certificate *);
uint8_t* FetchObjId(uint8_t *, uint8_t *, uint8_t *);
void ParseSpatPacket();

int certChangeFlag = 0, changeFlag = 2;
int random_mac_tmpid_at_cert_change(int);
char syscmd[200];
//char keysfname[255];
//static int time_val =1325376000;
int invokeIPServer(void);
extern float RV_ProcessData(GPSData *, BasicSafetyMessage_t *, float);
extern int CurveSpeed_check(char *, GPSData *);
extern int AsnLog(int, uint8_t pktnum, int msgType, int logFormat, char *buf, void *asnData, void *, double, uint16_t); //pvk
extern void mysleep(int sleep_secs, long int sleep_nsecs);
extern int get_date_yyyymmdd(int Tdate);
extern double fnKalmanFilter_Heading(double MeasuredHeading);
//extern int read_data(int gpssockfd,GPSData *gpsdata);
//extern void make_time_packet(GPSData *gpsdata);
//extern int false_read_data(int gpssockfd,GPSData *gpsdata);
extern int local_logging_client(int, void *, double, int, int, char *);
const char* _mac_sprintf(const u_int8_t *mac);
static int pid;
static uint64_t count = 0, blank = 0;
static uint64_t packets = 0;
static uint64_t drops = 0;
static uint64_t Errs = 0;
static struct timespec ntsleep, ntleft_sleep;
int sendreport = 1;
int retry = 0;
int rxclient = 1;
uint64_t tsfTime = 0;
uint8_t channelaccess;
uint8_t msgType[2] = {DSRCmsgID_basicSafetyMessage,DSRCmsgID_travelerInformation};
uint8_t secType = AsmOpen;
uint8_t repeatRate_Wsa = 50; //default WSA repeaterate = 50 for 5sec (10 Hz)
uint8_t repeatRate_Ta = 0;
UINT8 send_buff_tx[1024];
UINT8 recv_buff_tx[1024];
UINT8 send_buff_rx[1024];
UINT8 recv_buff_rx[1024];
UINT8 send_buff_Inter[1024];
int txsocket_id = -1, rxsocket_id = -1, tmpsock_id = -1;
static struct timeval tx_tvstart, tx_tvend;
static struct timeval rx_tvstart, rx_tvend;
static struct timeval gtv;
//static struct timeval txpktstart, txpktdone;
static GPSData wsmgps;
static double wsmgps_heading = 0;

static int gpssockfd = -1;
static double oldCourse_deg = 0;
/* Default Varibles*/
//enum { TXRX = 0, NOTX, NORXALL,NORX,TXRXUDP,NOTXRX};
enum {
	TXRXLOG = 0, TXLOG, RXLOG, NOLOG
};
int schan = 172; // Default Channel 172
int txChan = 172; // Default TxPkt Channel 255
uint8_t priority = 31; /* service priority */
uint8_t Active_Msg = 0, size, Udp_Rx = 0;
uint8_t app_wsmps = 0; //default wsmp-s disabled
u_int8_t maddr[17] = { 0 }; // Default Mac Address 00:00:00:00:00:00
uint8_t SecurityType = AsmOpen; // Default No Security
int ServiceType = PROVIDER;// USER; // PROVIDER; // Default Provider	change - wwg
int ChannelAcess = 1;
uint8_t TAChannel = 0; // Default Channel 178
uint8_t TAChannelInterval = 1;
uint8_t MessageType[2] = {DSRCmsgID_basicSafetyMessage,DSRCmsgID_travelerInformation};
long generationLatitude_tx = 900000001;
long generationLongitude_tx = 1800000001;
long generationLatitude_rx = 900000001;
long generationLongitude_rx = 1800000001;
int Udp_Socket = 0;
char pdu_buf[2048];
uint8_t Userreqtype = USER_REQ_SCH_ACCESS_AUTO; // May need changed Battelle - wwg -  USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL
uint8_t ImmAccess = 0;
uint16_t ExtAccess = 1;
uint8_t thread_options = TXRX;  // Default
//uint8_t thread_options = NOTX;
uint8_t log_options = NOLOG;
uint8_t Algo = 0;
uint8_t CertificateType = SIGNER_INTERFACE_TYPE_CERT_DIGEST; //SIGNER_TYPE_CERT_DIGEST_256;
uint32_t msgValidityDistance = 500; // 200;
uint8_t detectReplay = FALSE;
uint8_t generationLocation_tx = FALSE;
uint8_t generationLocation_rx = FALSE;
uint32_t certDelaytime = 500;
int print_parse = 9999; //0;
int use_interpolate = 1;
uint32_t pktdelaymsecs = 100; //no need to offset as now we sync with tsf
int usegps = 1;
static WSMMessage rxmsg;
static WSMIndication rxpkt;
//static WSMIndication tmp_rxpkt;
uint8_t qpriority = 2;
//uint32_t app_psid = 32, app_psid2 = 0; //default psid 32
uint32_t app_psid = 32, app_psid2 = 0; //uint32_t app_psid =5, app_psid2 = 0 //default psid 32 - for test wwg
int data_rateidx = 3;
int app_txpower = 14;
uint8_t restart_app = 0;
float rate_set[] = { 0.0f, 3.0f, 4.5f, 6.0f, 9.0f, 12.0f, 18.0f, 24.0f, 27.0f,
		36.0f, 48.0f, 54.0f };
int extract_rate(char *);
int rc = -1, display_type = -1, bret = 1;
int Btooth_forward = 0;

/*******************************************************************************************************
 * Battelle Defined
 * change - wwg
 */
int simpleLog = 1;
int gpsTestData = 0;
char spatMessage[150];
int gpsHeading = 0;
long gpsLogTimer = 0;
long phoneMessageTimer = 0; // Timer to send message to phone at periodic intervals
long lastSpatTimer = 0; // Timer to trigger clear message if no SPAT message has been received
//MapData_t *mapLog;
//Lock* pGpsLock = NULL;

// Bluetooth

void displayMapLanes();
double convertDegreesToRadians(double degrees);
//double distanceMeters(latLong_t *point1, latLong_t *point2);
//int findLane(latLong_t *position, int heading);
void displayMapLanes();
double convertDegreesToRadians(double degrees);
//double distanceMeters(latLong_t *point1, latLong_t *point2);
//void calculateOffsetLatLong(latLong_t *refPoint, latLong_t *point, uint8_t *buf);
void populateMapData();

/**
 * End Battelle Defined
 ********************************************************************************************************/

struct arguments {
	u_int8_t macaddr[17];
	u_int8_t channel;
};

struct EllipsoidalCoords {
	double lat;
	double lon;
	double elev; //above ellipsoidal
};

struct movement {
	uint32_t yellow_state;
	uint32_t cur_state;
	uint16_t mintimerem;
	uint16_t maxtimerem;
	uint16_t yellow_time;
	uint8_t ped_detect; //status
	uint8_t ped_count;
	uint8_t lane_set[11];

}__attribute__((__packed__));

struct SpatMsg {
	int32_t intsec_id;
	int8_t intsec_status;
	uint32_t timestamp; //in sec
	uint8_t ts_tenths; // 1/10 of secs
	uint8_t movcount; // no of movements
	struct movement mptr[10];
}__attribute__((__packed__));

struct ECEFCoords {
	double X;
	double Y;
	double Z;
};
struct ECEFCoords LLbuffer[INTER_SIZE], Start;

struct ta_argument {
	uint8_t channel;
	uint8_t channelinterval;
} taarg;
//pvk
static char logfile[255];
static uint8_t logformat = PCAP; //default pcap
static int logging = 0;
static int log_to_utc = 0;
//pvk
extern int gpsc_connect();
extern int gpsc_close_sock();

uint8_t vsize[3];
uint16_t v_wi = 0, v_le = 0;
int waveappmode;

double Prev_lat = 0, Prev_lon = 0;
double Ph_chord_length = 0.0;
double Cross_track_error = 0.0;
double Cross_track_error_2d = 0.0;
struct gps_datapoint_php4 Dp_start, Dp_prev, Dp_cur, Ph_pnt[23];
CircularBuffer cb;
int position = 0;
uint8_t pos_vec = 0;
//SPat
struct SpatMsg spatmsg;
uint8_t laneNum;
char clr[5] = "GYRXF"; //signal indication colours
char phaseClr[7][2] = { "SB", "LA", "RA", "SA", "SL", "SR", "UT" }; //different directions
//path history case4

/**************** data structure to store gps positions respect to pathHistoryPointSets-02,04,05,07 ************************/
union per_point_2 {
	struct val_offsets_2 {
		unsigned lat_Offset :18;
		unsigned long_Offset :18;
		unsigned elev_Offset :12;
		unsigned time_Offset :16;
	}__attribute__((packed)) lat_long_elev;
	unsigned char time_pos_hea_speed[15];
};

union per_point_4 {
	struct val_offsets_4 {
		unsigned lat_Offset :18;
		unsigned long_Offset :18;
		unsigned elev_Offset :12;
		unsigned time_Offset :16;
	}__attribute__((packed)) lat_long_elev;
	unsigned char tell[8]; //time,elev,lat,lon
};

union per_point_5 {
	struct val_offsets_5 {
		unsigned lat_Offset :18;
		unsigned long_Offset :18;
		unsigned elev_Offset :12;
	}__attribute__((packed)) lat_long_elev;
	unsigned char pos[10];
};

union per_point_7 {
	struct val_offsets_7 {
		unsigned lat_Offset :18;
		unsigned long_Offset :18;
//      unsigned  time_Offset : 16;
	}__attribute__((packed)) lat_long;
	unsigned char time_pos[11];
};

ActiveMsg *actMsg = NULL;
LocomateOptions *LocOpt = NULL;
uint8_t actMsgCount = 0;

/*FUNCTION DEFINITIONS*/

void usage() {
	printf("\nusage: getwbsstxrxencdec\n");
	printf(" \n******** Common Options ******\n");
	printf("\n\t -m:\tMac Address [xx:xx:xx:xx:xx:xx]\n");
	printf("\t -s:\tService Channel\n");
	printf("\t -H:\tCAN Inteface [vcan0/can0/can1]\n");
	printf("\t -b:\tTxPkt Channel\n");
	printf("\t -w:\tService Type [Provider/User]\n");
	printf("\t -t:\tMessage Type [BSM/PVD/RSA/ICA/SPAT/MAP/TIM]\n");
	printf("\t -e:\tSecurity Type [Plain/Sign/Encrypt]\n");
	printf(
			"\t -D:\tCertificate Attach Interval in millisec should be in multiple of packet delay\n");
	printf(
			"\t -l:\tOutput log filename, (specify path ending with / for pcap format)\n"); //pvk
	printf("\t -o:\tTx/Rx Options [TXRX/NOTX/NORXALL/NORX/TXRXUDP/NOTXRX]\n");
	printf("\t -X:\tLogging Options [TXRXLOG/TXLOG/RXLOG/NOLOG]\n");
	printf(
			"\t -g:\tsign certificate type [certificate/digest_224/digest_256/certificate_chain]\n");
	printf("\t -p:\tBSM Part II Packet interval (n BSM Part I messages) \n");
	printf(
			"\t -v:\tPath history number [2 represents BSM-PH-2, 5 represents BSM-PH-5] \n");
	printf("\t -k:\t Vehicle_Type (value as per DE_VehicleType)\n");
	printf(
			"\t -y:\tpsid value (any decimal value),to reigster with 2 psids give secondary psid after comma(-y 32,33)\n");
	printf("\t -d:\tpacket delay in millisec\n");
	printf("\t -q:\tUser Priority 0/1/2/3/4/5/6/7 \n");
	printf("\t -j:\ttxpower in dBm\n");
	printf("\t -M:\tModel Deployment Device ID\n");
	printf(
			"\t -T:\tTemporary ID control (1 = random, 0 = fixed upper two bytes)\n");
	printf("\t -S:\tSafety Supplement (wsmp-s) <0:disable / 1:enable>\n");
	printf("\t -L:\tVehicle Length in cm\n");
	printf("\t -W:\tVehicle Width in cm\n");
	printf(
			"\t -r:\tdata rate {0.0, 3.0, 4.5, 6.0, 9.0, 12.0, 18.0, 24.0, 27.0, 36.0, 48.0, 54.0}mbps\n");
	printf("\t -n:\tno argument, and selects no gps device available\n");
	printf(
			"\t -f:\tType xml or csv for logging in XML or CSV format. Type pcaphdr for only pcap header logging & pcap for full packet logging\n"); //pvk
	printf(
			"\t -F:\tframeType for TIM Packet 0-unknown(default) 1-advisory 2-roadSignage 3-commercialSignage\n");
	printf("\t -A:\tActive Message Status\n");
	printf("\t -B:\tPort Address for RSU receive from UDP Server\n");
	printf("\t -R:\tRepeat rate for WSA frame (Number of WSA per 5 seconds)\n");
	printf(
			"\t    \tRepeatrate is included in WSA-Header only if enabled from /proc/wsa_repeatrate_enable\n");
	printf("\t -G:\tRepeat rate for TA frame (Number of TA per 5 seconds)\n");
	printf(
			"\t    \tTA is available  only if TA channel [-c option] is given\n");
	printf("\t -I:\tIP service Enable 1= enable 0 = disable\n");
	printf("\t -O:\tTimeout for receiving udp data , in seconds\n");
	printf(
			"\t -Y:\tValue 0 = xml-print; Value[1 - 9998] = Display Received packet Stats interval in seconds; Value 9999 = Enable Safety Alert processing;\n");
	printf(
			"\t -V:\tEnable Full-Position Vector Transmit <0:disable / 1:enable>\n");
	printf("\t -Z:\tRemote Ip address. To run app remotely \n");
	printf("\t -K:\tInterface name for remote app\n");
	printf(" \n******** Provider Options ******\n");
	printf("\t -z:\tService Priority\n");
	printf("\t -a:\tService Channel Access [1:Alternating, 0:Continuous]\n");
	printf("\t -c:\tSpecify Channel Number to Transmit TA \n");
	printf("\t -i:\tTA Channel Interval [1:cch int, 2:sch int]\n");

	printf(" \n******** User Options ******\n");
	printf(
			"\t -u:\tUser Request Type [1:auto, 2:unconditional(not wait for WSA from provider), 3:none]\n");
	printf("\t -x:\tExtended Access <0:alternate /1:continuous>\n");

	printf("\nDefault values: \n");
	printf("\n\t\t -m:\tMac Address [00:00:00:00:00:00]\n");
	printf("\t\t -s:\tService Channel - 172\n");
	printf("\t\t -H:\tCAN Interface - vcan0\n");
	printf("\t\t -b:\tTxPkt Channel - 172\n");
	printf("\t\t -w:\tService Type - Provider\n");
	printf("\t\t -u:\tUser Request Type - [1:auto] \n");
	printf("\t\t -x:\tExtended Access - 0\n");
	printf("\t\t -c:\tTA disabled - 0\n");
	printf("\t\t -p:\tBSM Part II Interval - 10 packets\n");
	printf("\t\t -k:\tVehicle Type - 0 (not available)\n");
	printf("\t\t -v:\tPathHistory Number - 4 (PathHistory Set 4)\n");
	printf("\t\t -t:\tMessage Type - BSM\n");
	printf("\t\t -e:\tSecurity Type - Plain\n");
	printf("\t\t -l:\tOutput log filename, - NULL \n"); //pvk
	printf("\t\t -o:\tTx/Rx Options - TXRX\n");
	printf("\t\t -X:\tLogging Options - NOLOG\n");
	printf("\t\t -g:\tSign Certificate Type - certificate\n");
	printf("\t\t -y:\tpsid value - 32\n");
	printf("\t\t -d:\tpacket delay in millisec - 100\n");
	printf("\t\t -f:\tFormat Type PCAP\n"); //pvk
	printf("\t\t -F:\tFrame Type 0\n"); //unknown
	printf("\t\t -r:\tdata rate(mbps) - 3.0\n");
	printf("\t\t -A:\tActive Message Status - 0 (Disable)\n");
	printf(
			"\t\t -C:\tConfig File Name for active message - '/var/activemsg.conf'\n");
	printf("\t\t -B:\tPort Address for RSU receive from UDP Server - 0\n");
	printf("\t\t -R:\tRepeat rate for WSA frame - 50 \n");
	printf("\t\t -G:\tRepeat rate for TA frame  - 0\n");
	printf("\t\t -j:\ttxpower(dBm) - 14\n");
	printf("\t\t -q:\tUser Priority 2\n");
	printf("\t\t -S:\tSafety Supplement (wsmp-s) - 0 (disabled)\n");
	printf("\t\t -L:\tVehicle Length(cm) - 0\n");
	printf("\t\t -W:\tVehicle Width(cm) - 0\n");
	printf("\t\t -D:\tCertificate Attach Interval in millisec - 500\n");
	printf("\t\t -M:\tModel Deployment Device ID = 1\n");
	printf("\t\t -T:\tTemporary ID control = 1 (random 4 bytes)\n");
	printf("\t\t -I:\tIP service Enable = 0\n");
	printf("\t\t -O:\tTimeout for receiving udp data = 10 seconds\n");
	printf("\t\t -Y:\tDisplay Recv Packet Stats - 0 (xml-print)\n");
	printf("\t\t -V:\tEnable Full-Position Vector Transmit - 0 (disable)\n");
	printf("\t\t -Z:\tRemote ip address - 127.0.0.1\n");
	printf("\t\t -K:\tInterface name - eth0\n");
	exit(1);
}
void average_heading(uint8_t no_of_windows, double *average_buffer);
long confidence_lookup(double yawrate);
#if 1
void interpolate_gps(int msecs_diff) //to make interpolation ratio flexible
{
	float ratio = (float) msecs_diff / GPS_UPDATE_INT;
//printf(/*"msecs_diff=%d */"ratio=%f--"/*,msecs_diff*/,ratio);
	wsmgps.latitude = prevlat_ip[1] + ((prevlat_ip[1] - prevlat_ip[0]) * ratio);
	wsmgps.longitude = prevlon_ip[1]
			+ ((prevlon_ip[1] - prevlon_ip[0]) * ratio);

	if (wsmgps.course != GPS_INVALID_DATA) {
		if ((prevcourse_ip[1] - prevcourse_ip[0]) < -180) {
			wsmgps.course = prevcourse_ip[1] + (((360 - prevcourse_ip[0]) + (prevcourse_ip[1] - 0))* ratio);
		} else if ((prevcourse_ip[1] - prevcourse_ip[0]) > 150) {
			wsmgps.course = (prevcourse_ip[1] + (((0 - prevcourse_ip[0]) + (prevcourse_ip[1]) - 360)*ratio));
		} else {
			wsmgps.course = prevcourse_ip[1] + ((prevcourse_ip[1] - prevcourse_ip[0]) * ratio);
		}
		if (wsmgps.course < 0)
			wsmgps.course += 360;
		else if (wsmgps.course > 360)
			wsmgps.course -= 360;
	}
}
#endif
void Options(int argc, char *argv[]) {
	int index = 0, t;
	static struct option opts[] = { { "help", no_argument, 0, 'h' }, {
			"mac address", required_argument, 0, 'm' }, { "channel",
			required_argument, 0, 's' }, { "txpkt channel", required_argument,
			0, 'b' }, { "priority", required_argument, 0, 'z' }, {
			"service type", required_argument, 0, 'w' }, { "CAN Interface",
			required_argument, 0, 'H' }, { "channel access", required_argument,
			0, 'a' }, { "ta channel", required_argument, 0, 'c' }, {
			"ta channel Interval", required_argument, 0, 'i' }, {
			"message type", required_argument, 0, 't' }, { "security type",
			required_argument, 0, 'e' }, { "certificate attach delay",
			required_argument, 0, 'D' }, { "user request type",
			required_argument, 0, 'u' }, { "bsmII pkt interval",
			required_argument, 0, 'p' },
			{ "extended access", required_argument, 0, 'x' }, //pvk
			{ "logfile", required_argument, 0, 'l' }, //pvk
			{ "tx/rx/tx and rx", required_argument, 0, 'o' }, {
					"tx/rx/txrx logging", required_argument, 0, 'X' }, {
					"certificate/digest_224/digest_256/certificate_chain",
					required_argument, 0, 'g' }, { "psid", required_argument, 0,
					'y' }, { "delay", required_argument, 0, 'd' }, {
					"data rate", required_argument, 0, 'r' }, {
					"Active Message Status", required_argument, 0, 'A' }, {
					"Port", required_argument, 0, 'B' }, { "Repeatrate for WSA",
					required_argument, 0, 'R' }, { "Repeatrate for  TA",
					required_argument, 0, 'G' }, { "txpower", required_argument,
					0, 'j' }, { "path num", required_argument, 0, 'v' }, {
					"vehicle length", required_argument, 0, 'L' }, {
					"vehicle width", required_argument, 0, 'W' }, {
					"vehicle type", required_argument, 0, 'k' }, {
					"tempory id control", required_argument, 0, 'T' }, {
					"model deployment device id", required_argument, 0, 'M' }, {
					"user priority", required_argument, 0, 'q' }, { "format",
					required_argument, 0, 'f' }, { "frame type",
					required_argument, 0, 'F' }, { "wsmp-s", required_argument,
					0, 'S' }, { "nogps", no_argument, 0, 'n' }, { "ipschpermit",
					no_argument, 0, 'I' }, { "Time Out", required_argument, 0,
					'O' },
			{ "Full position vector", required_argument, 0, 'V' }, {
					"Display packet Stats", required_argument, 0, 'Y' }, {
					"To Interpolate", required_argument, 0, 'J' }, {
					"Remote ip", required_argument, 0, 'Z' }, { "Interface",
					required_argument, 0, 'K' }, { 0, 0, 0, 0 } };
//pvk
#ifdef  WIN32
	sprintf(logfile, "%s", ".\\asnmsg.log");
#else
	sprintf(logfile, "%s", "/tmp/asnmsg.log");
#endif
//pvk
	while (1) {
		t =
				getopt_long(
						argc,
						argv,
						"L:W:S:k:v:m:s:b:z:q:w:a:c:i:t:e:D:u:p:x:l:E:H:o:f:X:g:y:F:d:r:A:B:R:G:I:j:T:M:O:V:n:Y:Z:K:J:",
						opts, &index);
		if (t < 0) {
			break;
		}
		switch (t) {
		// Help
		case 'h':
			usage();
			break;
			// Mac Adress
		case 'm':
			memcpy(maddr, optarg, 17);
			break;
			// Channel
		case 's':
			schan = atoi(optarg);
			break;
		case 'H':
			strcpy(can_interface, optarg);
			break;

		case 'b':
			txChan = atoi(optarg);
			break;

		case 'z':
			priority = atoi(optarg);
			break;
			// Serive Type < Provider/User >
		case 'w':
			if (!strcasecmp(optarg, "PROVIDER"))
				ServiceType = PROVIDER;
			else if (!strcasecmp(optarg, "USER"))
				ServiceType = USER;
			else
				ServiceType = PROVIDER;
			break;
			// Channel Access
		case 'a':
			ChannelAcess = atoi(optarg);
			break;
		case 'q':
			qpriority = atoi(optarg);
			break;
			//TA Channel
		case 'c':
			TAChannel = atoi(optarg);
			break;
			// TA Channel Interval
		case 'i':
			TAChannelInterval = atoi(optarg);
			break;
		case 't':
			if (!strcasecmp(optarg, "BSM")) {
				MessageType[0] = DSRCmsgID_basicSafetyMessage;
				msgValidityDistance = 0;
				detectReplay = TRUE;
			} else if (!strcasecmp(optarg, "PVD"))
				MessageType[0] = DSRCmsgID_probeVehicleData;
			else if (!strcasecmp(optarg, "RSA"))
				MessageType[0] = DSRCmsgID_roadSideAlert;
			else if (!strcasecmp(optarg, "ICA"))
				MessageType[0] = DSRCmsgID_intersectionCollisionAlert;
			else if (!strcasecmp(optarg, "MAP"))
				MessageType[0] = DSRCmsgID_mapData;
			else if (!strcasecmp(optarg, "SPAT")) {
				MessageType[0] = DSRCmsgID_signalPhaseAndTimingMessage;
				generationLocation_tx = TRUE;
			} else if (!strcasecmp(optarg, "TIM"))
				MessageType[0] = DSRCmsgID_travelerInformation;
			else
				MessageType[0] = DSRCmsgID_basicSafetyMessage;
			break;
			// Security < No Security, Sign/Verify, Enc/Dec >
		case 'e':
			if (!strcasecmp(optarg, "PLAIN"))
				SecurityType = AsmOpen;
			else if (!strcasecmp(optarg, "SIGN"))
				SecurityType = AsmSign;
			else if (!strcasecmp(optarg, "ENCRYPT"))
				SecurityType = AsmEncrypt;
			else
				SecurityType = AsmOpen;
			break;
		case 'D':
			certDelaytime = atoi(optarg);
			break;
		case 'I':
			app_ip_sch_permit = atoi(optarg);
			break;
		case 'u':
			Userreqtype = atoi(optarg);
			break;
		case 'p':
			//	ImmAccess = atoi(optarg);
			bsm2data = atoi(optarg);
			break;
		case 'k':
			vehicle__type = atoi(optarg);
			break;

		case 'x':
			ExtAccess = atoi(optarg);
			break;

		case 'y':
			//psid
			//app_psid  = atoi(optarg);
			sscanf(optarg, "%u,%u", &app_psid, &app_psid2);
			if (app_psid == 0xBFE1 || app_psid == 0x23)
				app_ip_sch_permit = 1;
			if (app_psid2 == 0xBFE1 || app_psid2 == 0x23)
				app_ip_sch_permit = 1;
			break;

		case 'j':
			//txpower
			sscanf(optarg, "%u", &app_txpower);
			break;

		case 'r':
			//data_rateidx
			//data_rateidx  = atof(optarg);
			//sscanf(optarg,"%u", &data_rateidx);
			data_rateidx = extract_rate(optarg);
			if (data_rateidx < 0) {
				data_rateidx = 3;
			}
			break;

		case 'A':
			Active_Msg = atoi(optarg);
			msgType[0] = MessageType[0] = 0xff;
			msgType[1] = MessageType[1] = 0xff;
			break;

		case 'B':
			sock_port = (uint16_t) atoi(optarg);
			msgType[0] = MessageType[0] = 0xff;
			msgType[1] = MessageType[1] = 0xff;
			Udp_Rx = 1;
			pktdelaymsecs = 0;
			break;

		case 'R':
			repeatRate_Wsa = (uint8_t) atoi(optarg);
			break;

		case 'G':
			repeatRate_Ta = (uint8_t) atoi(optarg);
			break;

		case 'd':
			//packet_delay
			sscanf(optarg, "%u", &pktdelaymsecs);
			break;
//pvk
		case 'l':
			strcpy(logfile, optarg);
			log_to_utc = 1;
			break;

		case 'E':
			certChangeFlag = atoi(optarg);
			break;

		case 'o':
			if (!strcasecmp(optarg, "NOTX"))
				thread_options = NOTX;
			else if (!strcasecmp(optarg, "NORXALL"))
				thread_options = NORXALL;
			else if (!strcasecmp(optarg, "NORX"))
				thread_options = NORX;
			else if (!strcasecmp(optarg, "TXRXUDP"))
				thread_options = TXRXUDP;
			else if (!strcasecmp(optarg, "NOTXRX"))
				thread_options = NOTXRX;
			else
				thread_options = TXRX;
			break;
		case 'X':
			if (!strcasecmp(optarg, "TXRXLOG"))
				log_options = TXRXLOG;
			else if (!strcasecmp(optarg, "RXLOG"))
				log_options = RXLOG;
			else if (!strcasecmp(optarg, "TXLOG"))
				log_options = TXLOG;
			else if (!strcasecmp(optarg, "NOLOG"))
				log_options = NOLOG;
			if (log_options != NOLOG)
				logging = 1;
			break;
		case 'g':
			if (!strcasecmp(optarg, "certificate"))
				CertificateType = SIGNER_INTERFACE_TYPE_CERT;
			else if (!strcasecmp(optarg, "digest_224"))
				CertificateType = SIGNER_INTERFACE_TYPE_CERT_DIGEST;
			else if (!strcasecmp(optarg, "digest_256"))
				CertificateType = SIGNER_INTERFACE_TYPE_CERT_DIGEST;
			else if (!strcasecmp(optarg, "certificate_chain"))
				CertificateType = SIGNER_INTERFACE_TYPE_CERT_CHAIN;
			break;
		case 'f':
			if (!strcasecmp(optarg, "xml")) {
				logformat = XML;
			} else if (!strcasecmp(optarg, "csv")) {
				logformat = CSV;
			} else if (!strcasecmp(optarg, "pcaphdr")) {
				logformat = PCAPHDR;
			} else if (!strcasecmp(optarg, "pcap")) {
				logformat = PCAP;
			} else {
				logformat = PCAP;
				printf(
						"[LOG: Logformat %s not supported. Using default format]\n",
						optarg);
			}
			break;
//pvk//
		case 'F':
			FrameType = atoi(optarg);
			break;
		case 'v':
			path_num = atoi(optarg);
			break;
		case 'V':
			pos_vec = atoi(optarg);
			break;

		case 'L':
			v_le = atoi(optarg);
			vsize[2] = (uint8_t) v_le;
			vsize[1] = vsize[1] | ((v_le >> 8) & 0x3f);

			break;
		case 'W':
			v_wi = atoi(optarg);
			vsize[0] = (uint8_t) (v_wi >> 2);
			vsize[1] = vsize[1] | ((v_wi << 6) & 0xc0);
			break;
		case 'n':
			usegps = 0;
			break;
		case 'S':
			app_wsmps = atoi(optarg);
			break;
		case 'Y':
			print_parse = atoi(optarg);
			break;
		case 'J':
			use_interpolate = atoi(optarg);
			break;

		case 'M':
			//mod_depl_dev_id      range - 0x0001 to 0xffff (0 to 65535)
			sscanf(optarg, "0x%x", &mod_depl_dev_id);
			(void) syslog(LOG_INFO, "ModelDeploymentDeviceID = 0x%x \n",
					mod_depl_dev_id);
			break;

		case 'T':
			//temp_id control    Fixed upper two bytes = 0 , Random = 1
			temp_id_control = atoi(optarg);
			(void) syslog(LOG_INFO, "TemporaryIDControl = %d \n",
					temp_id_control);
			break;

		case 'O':
			TIME_OUT = atoi(optarg);
			syslog(LOG_INFO, "TIMEOUT range  = %d sec \n", TIME_OUT);
			break;
		case 'Z':
			strcpy(RemoteIp, optarg);
			WAVEDEVICE = WAVEDEVICE_REMOTE;
			RemoteEnable = 1;
			break;
		case 'K':
			strcpy(ifname, optarg);
			break;
		default:
			usage();
			break;
		}
	}
	num = bsm2data;
}

//ParseSpatPacket fun parse the required spat data from blob payload
//and it fills in common SpatMsg structure
void ParseSpatPacket() {
	uint8_t *ptr, size = 0, cnt = 0, objid = 0;
	uint16_t payload_size = 0;
	int i = 0;

	memcpy(&payload_size, (uint8_t *) (rxpkt.data.contents) + 2, 2);
	ptr = (uint8_t *) (rxpkt.data.contents) + 4;
	spatmsg.movcount = 0;

	while (payload_size > 1) {
		ptr = FetchObjId(ptr, &size, &objid);
		switch (objid) {
		case INTERSECTIONID:
			memcpy(&spatmsg.intsec_id, ptr, size);
			ptr += size;
			break;
		case INTERSECTIONSTATUS:
			memcpy(&spatmsg.intsec_status, ptr, size);
			ptr += size;
			break;
		case MSGTIMESTAMP:
			memcpy(&spatmsg.timestamp, ptr, (size - 1));
			ptr += (size - 1 );
			memcpy(&spatmsg.ts_tenths, ptr, 1);
			ptr += 1;
			break;
		case MOVEMENT:
			cnt = spatmsg.movcount++;
			break;
		case LANESET:
			spatmsg.mptr[cnt].lane_set[0] = size / 2;
			for (i = 1; i <= size / 2; i++) {
				ptr++;
				spatmsg.mptr[cnt].lane_set[i] = *ptr;
				ptr++;
			}
			break;
		case CURSTATE:
			memcpy(&spatmsg.mptr[cnt].cur_state, ptr, size);
			spatmsg.mptr[cnt].cur_state = spatmsg.mptr[cnt].cur_state
					>> ((4 - size) * 8);
			ptr += size;
			break;
		case MINTIMEREMAIN:
			memcpy(&spatmsg.mptr[cnt].mintimerem, ptr, size);
			ptr += size;
			break;
		case MAXTIMEREMAIN:
			memcpy(&spatmsg.mptr[cnt].maxtimerem, ptr, size);
			ptr += size;
			break;
		case YELLOWSTATE:
			memcpy(&spatmsg.mptr[cnt].yellow_state, ptr, size);
			spatmsg.mptr[cnt].yellow_state = spatmsg.mptr[cnt].yellow_state
					>> ((4 - size) * 8);
			ptr += size;
			break;
		case YELLOWTIME:
			memcpy(&spatmsg.mptr[cnt].yellow_time, ptr, size);
			ptr += size;
			break;
		case PEDESTRAIN:
			memcpy(&spatmsg.mptr[cnt].ped_detect, ptr, size);
			ptr += size;
			break;
		case PEDESORVEHICLECOUNT:
			memcpy(&spatmsg.mptr[cnt].ped_count, ptr, size);
			ptr += size;
			break;
		default:
			break;
		}
		if (objid != MOVEMENT)
			payload_size -= (2 + size );
		else
			payload_size -= 1;
	}
	return;
}

uint8_t* FetchObjId(uint8_t *ptr, uint8_t *size, uint8_t *objid) {
	memcpy(objid, ptr, 1);
	ptr++;
	if (*objid == MOVEMENT) {
		*size = 0;
		return ptr;
	}
	memcpy(size, ptr, 1);
	ptr++;
	return ptr;
}

void update_vehicleData(LocomateVehicleSpecs *veh_specs) {
	uint16_t veh_len = 0, veh_wid = 0;
	temp_id_control = (int) veh_specs->temp_id;
	mod_depl_dev_id = veh_specs->mod_depl;

	veh_len = veh_specs->veh_length;
	vsize[2] = (uint8_t) veh_len;
	vsize[1] = vsize[1] | ((veh_len >> 8) & 0x3f);

	veh_wid = veh_specs->veh_width;
	vsize[0] = (uint8_t) (veh_wid >> 2);
	vsize[1] = vsize[1] | ((veh_wid << 6) & 0xc0);

	vehicle__type = veh_specs->veh_type;
	//printf("\nUpdated : tmpid %d , modid %d , len %d ,wid %d , type %d\n",temp_id_control,mod_depl_dev_id,veh_len,veh_wid,vehicle__type);
}

/*:  This function converts decimal degrees to radians             :*/
double deg2rad(double deg) {
	return (deg * pidiv180);
}

/*:  This function converts radians to decimal degrees             :*/
double rad2deg(double rad) {
	return (rad * one80divpi);
}

double distance_cal(double lat1, double lon1, double lat2, double lon2,
		double elev) {
	double ang1, ang2, radpnt1, radpnt2, ethlat1, ethlon1, ethlat2, ethlon2, x,
			y, dist;
	ang1 = (atan(R2divR1*tan(lat1*pidiv180)));
	ang2 = (atan(R2divR1*tan(lat2*pidiv180)));
	radpnt1 =
			(sqrt(1/(((cos(ang1))*(cos(ang1)))/R1+(((sin(ang1))*(sin(ang1)))/R2))))
					+ elev;
	radpnt2 =
			(sqrt(1/(((cos(ang2))*(cos(ang2)))/R1+(((sin(ang2))*(sin(ang2)))/R2))))
					+ elev;
	ethlat1 = radpnt1 * cos(ang1);
	ethlat2 = radpnt2 * cos(ang2);
	ethlon1 = radpnt1 * sin(ang1);
	ethlon2 = radpnt2 * sin(ang2);
	x = 	sqrt((ethlat1-ethlat2)*(ethlat1-ethlat2)+(ethlon1-ethlon2)*(ethlon1-ethlon2));
	y = 2 * pi * ((((ethlat1 + ethlat2) / 2)) / 360) * (lon1 - lon2);
	dist = sqrt(x*x+y*y);
	return dist;
}

void fnConvertEllipsoidalToECEF(double latitude, double longitude,
		double altitude, struct ECEFCoords *ptrToECEFCoords) {
	double NormalRadiusOfCurvature;

	NormalRadiusOfCurvature =
			ELLIPSOID_MAJOR_AXIS
					/ (sqrt(1 - SqrOfTheEccentricity * sin(latitude * pidiv180) * sin(latitude * pidiv180)));
	ptrToECEFCoords->X = (NormalRadiusOfCurvature + altitude)
			* cos(latitude * pidiv180) * cos(longitude * pidiv180);
	ptrToECEFCoords->Y = (NormalRadiusOfCurvature + altitude)
			* cos(latitude * pidiv180) * sin(longitude * pidiv180);
	ptrToECEFCoords->Z = (NormalRadiusOfCurvature * (1 - SqrOfTheEccentricity)
			+ altitude) * sin(latitude * pidiv180);
	return;
}

double fnComputeDistanceFromPointToALine(struct ECEFCoords *A,
		struct ECEFCoords *B, struct ECEFCoords *C, double magnOfVectorAC) {
	double InnerProduct, RatioOfProjectionToVectorAC;
	double result;
	struct ECEFCoords InterceptPoint;

	//Project AB on to AC. Take the scalar product with the unit vector along vector AC
	InnerProduct = (B->X - A->X) * (C->X - A->X) + (B->Y - A->Y) * (C->Y - A->Y)
			+ (B->Z - A->Z) * (C->Z - A->Z);
	RatioOfProjectionToVectorAC = InnerProduct
			/ (magnOfVectorAC * magnOfVectorAC);

	//The point at which the normal from B will meet AC
	InterceptPoint.X = A->X + (C->X - A->X) * RatioOfProjectionToVectorAC;
	InterceptPoint.Y = A->Y + (C->Y - A->Y) * RatioOfProjectionToVectorAC;
	InterceptPoint.Z = A->Z + (C->Z - A->Z) * RatioOfProjectionToVectorAC;
#if 0    
	Confirm that we vector from B to InterceptPoint is indeed orthogonal to AC

	tmp = (InterceptPoint.X - B->X) * (C->X - A->X);
	tmp = tmp + (InterceptPoint.Y - B->Y) * (C->Y - A->Y);
	tmp = tmp + (InterceptPoint.Z - B->Z) * (C->Z - A->Z);
	assert(fabs((double) tmp) < 10E-6); // below is simplified form
#endif
	result =
			sqrt(((InterceptPoint.X - B->X)*(InterceptPoint.X - B->X)) + ((InterceptPoint.Y - B->Y)*(InterceptPoint.Y - B->Y)) + ((InterceptPoint.Z - B->Z)*(InterceptPoint.Z - B->Z))); //compute magnitude
	return result;
}

#if 0
static inline double distance_cal_eth(double lat,double elev,double *x_eth,double *y_eth) {
	double ang,radpnt;
	ang=(atan(R2divR1*tan(lat*pidiv180)));
	radpnt=(sqrt(1/(((cos(ang))*(cos(ang)))/R1+(((sin(ang))*(sin(ang)))/R2))))+elev;
	*x_eth = radpnt * cos(ang);
	*y_eth = radpnt * sin(ang);
}
#endif

void offset_cal_php4(int pos) {

	int tmp;
	int lat = (int) ((Ph_pnt[pos].lat) * 10000000);
	int longi = (int) ((Ph_pnt[pos].lon) * 10000000);
	int eleva = (int) ((Ph_pnt[pos].elev) * 10); //offset unit: 10cm
	int elev;
	lat = latitude_val - lat;
	longi = longitude_val - longi;

	elev = (int) round(wsmgps.altitude*10);

	elevationoffset = elev - eleva;

	if (lat >= 131071)
		latitudeoffset = 131071;

	else if (lat <= -131071)
		latitudeoffset = -131071;

	lat = lat & 0x8001FFFF;
	tmp = lat;
	lat &= 0x0001FFFF; //to remove sign since tmp will take care of that
	tmp = tmp >> 14;
	tmp = tmp & 0x00020000;
	lat = lat | tmp;
	latitudeoffset = htobe32(lat);

	if (longi >= 131071)
		longi = 131071;

	else if (longi <= -131071)
		longitudeoffset = -131071;

	longi = longi & 0x8001FFFF;
	tmp = longi;
	longi &= 0x0001FFFF;
	tmp = tmp >> 14;
	tmp = tmp & 0x00020000;
	longi = longi | tmp;
	longitudeoffset = htobe32(longi);

	if (elevationoffset >= 2047)
		elevationoffset = 2047;

	else if (elevationoffset <= -2047)
		elevationoffset = -2047;

	tmp = elevationoffset;
	elevationoffset &= 0x000007FF;
	tmp = tmp >> 20;
	tmp = tmp & 0x00000800;
	elevationoffset = elevationoffset | tmp;

	timeoffset = (wsmgps.actual_time * 100) - (Ph_pnt[pos].time * 100); //units of 10 msecs

	if (timeoffset >= 65534)
		timeoffset = 65534;

}

void apply_RSE_options(ActiveMsg *msg) {

	if (txChan != msg->txChan_rse || ChannelAcess != msg->ChannelAcess_rse
			|| ExtAccess != (!msg->ChannelAcess_rse)
			|| app_psid != msg->app_psid_rse) {
		restart_app = 1; //restart(unregister then resgister) app only when there is change in mode/channel/psid
		txChan = msg->txChan_rse; //-b
	}
	priority = msg->priority_rse; //-p
	secType = SecurityType = msg->SecurityType_rse; //-e
	ExtAccess = (uint16_t) (!msg->ChannelAcess_rse); //-x
	ChannelAcess = msg->ChannelAcess_rse; //-a

	if (msg->txChan_rse != 178) //channel switch happens for 178 and schan thus not possible for alternate mode
		schan = msg->txChan_rse;

	app_psid = msg->app_psid_rse; //-y
	//pktdelaymsecs = pktdelaymsecs_rse;//-d
}

int extract_rate(char *str) {
	int i = 0, numdots = 0;
	int len = strlen(str);
	float rate = 0.0f;

	for (i = 0; i < len; i++) {
		if ((!isdigit(str[i])) && (str[i] != '.'))
			return -1;

		if (str[i] == '.')
			numdots++;

		if (numdots > 1)
			return -1;
	}
	sscanf(str, "%f", &rate);

	if (rate <= 0.0f)
		return -1;

	for (i = 1; i < RATESET_NUM_ELMS; i++) {
		if (rate_set[i] == rate)
			return i;
	}

	return -1;
}

int certchange(void) {
	int send_size = 0, recv_size = 0;
	// send EscMsg_CertChg request
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_create_cert_change(send_buff_tx, &send_size);
	do {
		printf("Send EscMsg_CertChg request. [0x%02x]", send_buff_tx[0]);
		if (0 != AsmSend((char *) send_buff_tx, send_size, txsocket_id)) {
			return -1;
		}

		// receive EscMsg_CertChg response
		bzero(recv_buff_tx, sizeof(recv_buff_tx));
		recv_size = AsmRecv((char *) recv_buff_tx, sizeof(recv_buff_tx),
				txsocket_id);
		if (recv_size <= 0) {
			return -1;
		}
		if (recv_buff_tx[0] != CMD_OK_CERT_CHG_POST
				&& (recv_buff_tx[0] != CMD_LCM_STATUS_RDY
						&& recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
			printf("Receive error. [0x%02x], %lf", recv_buff_tx[0],
					wsmgps.actual_time);
			return -1;
		} else {
			printf("Receive EscMsg_CertChg response. [0x%02x]",
					recv_buff_tx[0]);
		}
	} while (recv_buff_tx[0] == CMD_LCM_STATUS_RDY
			|| recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED);
	return recv_buff_tx[0];
}

#ifdef USE_LCM
int misbehavior_report(void)
{
	int send_size = 0 ,recv_size = 0;

	// send EscMsg_Verify request
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_create_misbehavior_report(send_buff_tx, &send_size);
	do {
		printf("Send AsmMsg_Misbehavior_Rep request. [0x%02x]", send_buff_tx[0]);
		if (0 != AsmSend((char*)send_buff_tx, send_size,tmpsock_id)) {
			return -1;
		}

		// receive CMD_OK_CERT_INFO response
		bzero(recv_buff_tx, sizeof(recv_buff_tx));
		recv_size = AsmRecv((char *)recv_buff_tx, sizeof(recv_buff_tx),tmpsock_id);
		if (recv_size <= 0) {
			return -1;
		}
		if (recv_buff_tx[0] != CMD_OK_MISBEHAVIOR_REPORT && (recv_buff_tx[0]!=CMD_LCM_STATUS_RDY && recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
			printf("Receive error. [0x%02x]", recv_buff_tx[0]);
			return -1;
		}
		else {
			printf("Receive AsmMsg_Misbehavior_Rep response. [0x%02x]", recv_buff_tx[0]);
		}
	}while(recv_buff_tx[0]==CMD_LCM_STATUS_RDY || recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED);

	return 0;
}
#endif /* USE_LCM */

int register_app() {
	int regRet = 0;
	if (waveappmode == USER) {
		printf("Inside User process1\n");
		memset(&ust, 0, sizeof(WMEApplicationRequest));
		ust.psid = app_psid;
		if ((Userreqtype > USER_REQ_SCH_ACCESS_NONE)
				|| (Userreqtype < USER_REQ_SCH_ACCESS_AUTO)) {
			printf("User request type invalid: setting default to auto\n");
			ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO;
		} else {
			ust.userreqtype = Userreqtype;
		}
		if (ust.userreqtype == USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL) {
			ust.channel = schan;
		}
		ust.schaccess = ImmAccess;
		ust.ipservice = app_ip_sch_permit;
		ust.schextaccess = ExtAccess;
		ust.priority = priority;
		ust.serviceport = SERVICE_PORT;
		msgType[0] = MessageType[0];
		msgType[1] = MessageType[1];
		secType = SecurityType;
		if (RemoteEnable) {
			setRemoteDeviceIP(RemoteIp);
			getUSTIpv6Addr(&ust.ipv6addr, ifname);
		}
		if (RemoteEnable == 1
				&& ((thread_options & RXALL_MASK) || (thread_options & RX_MASK)))
			registerWSMIndication(receiveWSMIndication);
		printf("Invoking WAVE driver \n");

		registerLinkConfirm(confirmBeforeJoin);
		if (invokeWAVEDevice(WAVEDEVICE, 0) < 0) {
			printf("Open Failed. Quitting\n");
			exit(-1);
		}
		printf("Registering User %d  app pid = %d\n", ust.psid, pid);
		if ((regRet = registerUser(pid, &ust)) < 0) {
			printf("ERR::Register User Failed \n");
			printf("Removing user if already present  %d\n",
					!removeUser(pid, &ust));
			printf("USER Registered %d with PSID =%u \n", (regRet =
					registerUser(pid, &ust)), ust.psid );
		}

		if (app_psid2) {
			memcpy(&ust2, &ust, sizeof(WMEApplicationRequest));
			ust2.psid = app_psid2;
			printf("Registering Secondary User  %d  app pid = %d\n", ust2.psid,
					pid);
			if ((regRet = registerUser(pid, &ust2)) < 0) {
				printf("ERR::Register User Failed \n");
				printf("Removing user if already present  %d\n",
						!removeUser(pid, &ust2));
				printf("USER Registered %d with PSID =%u \n", (regRet =
						registerUser(pid, &ust2)), ust2.psid);
			}
		}
		printf("In User end\n");
	} else if (waveappmode == PROVIDER) {
		taarg.channel = TAChannel;
		taarg.channelinterval = TAChannelInterval;
		msgType[0] = MessageType[0];
		msgType[1] = MessageType[1];
		secType = SecurityType;
		if (ChannelAcess > 1) {
			printf("channel access set default to alternating access\n");
			channelaccess = CHACCESS_ALTERNATIVE;
		} else {
			channelaccess = ChannelAcess;
		}
		printf("Inside Provider process\n");
		printf("Filling Provider Service Table entry %d\n", buildPSTEntry());
		printf("Building a WME Application  Request %d\n",
				buildWMEApplicationRequest());
		if (TAChannel != 0)
			printf("Building TA request %d\n", buildWMETARequest());
		if (RemoteEnable) {
			setRemoteDeviceIP(RemoteIp);
			getUSTIpv6Addr(&entry.ipv6addr, ifname);
		}
		if (RemoteEnable == 1
				&& ((thread_options & RXALL_MASK) || (thread_options & RX_MASK)))
			registerWSMIndication(receiveWSMIndication);
		printf("Invoking WAVE driver \n");
		if (invokeWAVEDevice(WAVEDEVICE, 0) < 0) {
			printf("Open Failed. Quitting\n");
			exit(-1);
		} else {
			printf("Driver Invoked\n");
		}
		registerWMENotifIndication(receiveWME_NotifIndication);
		registerWRSSIndication(receiveWRSS_Indication);
		registertsfIndication(receiveTsfTimerIndication);

		printf("Registering provider\n ");
		if (!RemoteEnable)
			wsa_conf_parse();
		if ((regRet = registerProvider(pid, &entry)) < 0) {
			printf("ERR::Register Provider failed\n");
			removeProvider(pid, &entry);
			regRet = registerProvider(pid, &entry);
		} else {
			printf("provider registered with PSID = %u\n", entry.psid);
		}
		if (app_psid2) {
			printf("Registering Secondary provider\n ");
			memcpy(&entry2, &entry, sizeof(WMEApplicationRequest));
			entry2.psid = app_psid2;
			if ((regRet = registerProvider(pid, &entry2)) < 0) {
				printf("ERR::Register Provider failed\n");
				removeProvider(pid, &entry2);
				regRet = registerProvider(pid, &entry2);
			} else {
				printf("provider registered with PSID = %u\n", entry2.psid);
			}
		}
		if (TAChannel != 0) {
			printf("starting TA\n");
			if (transmitTA(&tareq) < 0) {
				printf("send TA failed\n ");
			} else {
				printf("send TA successful\n");
			}
		}

	} else {
		printf("ERR: Input value wrong for waveappmode\n");
	}
	if (regRet < 0)
		return -1;
	else
		return 0;
}

int list(void) {
	struct linux_dirent {
		long d_ino;
		off_t d_off;
		unsigned short d_reclen;
		char d_name[];
	};

	struct linux_dirent *d = NULL;
	int bpos = 0;
	char d_type;
	int fd;
	int nread = 0, i = 0;
	char lsbuf[1024];
	//sprintf(syscmd,"%s%s",mntpath,log_dest);//copy to some temperory directory
	fd = open("/var/AML/", O_RDONLY);
	if (fd == -1) {
		syslog(LOG_INFO, "Active Msg list: open err\n");
		return -1;
	}
	for (;;) {
		nread = syscall(SYS_getdents, fd, lsbuf, 1024);
		//	  printf("nread: %d\n",nread);
		if (nread == -1) {
			close(fd);
			syslog(LOG_INFO, "Active Msg list: getdents err\n");
			return -1;
		}

		if ((nread == 0)) {
			close(fd);
			if (bpos == 0)
				syslog(LOG_INFO, "Active Msg list: nofiles\n");
			break;
		}

		while (bpos < nread) {
			d = (struct linux_dirent *) (lsbuf + bpos);
			d_type = *(lsbuf + bpos + d->d_reclen - 1);
			if (d_type == DT_REG) {
				strcpy(actMsg[i].actfile, "/var/AML/"); /* for directory path */
				strcat(actMsg[i].actfile, (char *) d->d_name);
				i++;
			}
			bpos += d->d_reclen;
			//		  printf("%d\n",bpos);
		} //while
	} //for
	close(fd);
	return 0;
}

int check_usb_mount(void) {
	FILE *fd = popen("mount", "r");
	if (fd != NULL) {
		char tmpstr[100] = "0";
		while (fgets(tmpstr, 100, fd) != NULL) {
			if (strstr(tmpstr, "/tmp/usb") != NULL) {
				syslog(LOG_INFO, "%s\n", tmpstr);
				pclose(fd);
				return 1;
			}
		}
		pclose(fd);
	}
	return -1;
}

int main(int argc, char *argv[]) {
	//int result;
	//int waveappmode;
	char tmp_buf[100];
	FILE *randomization_ptr;
	char tmpstr[50];
	void *status_ptr;
	//long *thread_id;
	long processid;
	int ret;
	int btooth_arg = 1;
	//struct timeval tv;
	pthread_attr_t attr;
	struct sched_param param;
	//int chan;
	LocomateVehicleSpecs vehicle_specs;	

    int pid_file = open("/var/bin/infloOBU_Lock.pid", O_CREAT | O_RDWR, 0666);
    int rc = flock(pid_file, LOCK_EX | LOCK_NB);
    if(rc) {
        if(EWOULDBLOCK == errno)
        {
            printf("Another instance of infloOBU is already running! Exiting...\n");
            return -1;
        }
    }
    

	//qwarnStart();
	//qwarnEnableLogging();
	//pGpsLock = lockCreate();

	printf("Battelle OBU DSRC Implementation \n");
	rxmsg.wsmIndication = &rxpkt;
	struct arguments arg[3];
	sem_init(&rse_sem, 0, 0);
	sem_init(&rse_sem2, 0, 1);
	/* semaphore shared between udp and tx thread of this process with initial value 0*/
	pid = getpid();
	printf("Got PID\n");
	Options(argc, argv);
	printf("Thread id \n");

	if (MessageType[0] == DSRCmsgID_basicSafetyMessage || MessageType[1] == DSRCmsgID_travelerInformation) {
		printf("Message Type BSM \n");
		//printf("\ncertChangeFlag = 1\n");
		certChangeFlag = 1;
	}

	// Get configuration from ModelDeploymentRemovable.conf. Only succeeds with
	// file on the usb stick
	ret = fetch_vehicleData(&vehicle_specs);
	if (ret > 0) {
		update_vehicleData(&vehicle_specs);
	}

	// default printparse = 0
	if (print_parse == 9999) {
		LocOpt = (LocomateOptions *) calloc(1, sizeof(LocomateOptions));
		strcpy(LocOpt->filename, "/var/locomateoptions.conf");
		if (Get_LOCOMATE_Options(LocOpt) < 0) {
			LocOpt->TimeToContact = 5; //default TTC value
			LocOpt->LaneWidth = 5.0; //Default Lane Width Value
			strcpy(LocOpt->Display_Type, "info");
		} else {
			if (strcasecmp(LocOpt->Display_Type, "bluetooth") == 0) {
				if (app_psid == 49120 || app_psid2 == 49120) {
					if (strcasecmp(LocOpt->SpatDisplay, "yes") == 0) { //Spat Android Application
						btooth_arg = 3;
						rc = pthread_create(&bluethread, NULL, main_bluetooth,
								(void *) &btooth_arg);
						threadCount++;
					} else {
						printf("\nBluetooth thread not opened\n");
						rc = 0;
					}
				} else { //Locomate Safety Application
					rc = pthread_create(&bluethread, NULL, main_bluetooth,
							(void *) &btooth_arg);
					threadCount++;
				}
				if (rc) {
					printf(
							"\n ERROR; return code from pthread_create() is %d\n",
							rc);
					return -1;
				}
				sched_yield();
				display_type = BLUETOOTH_DISPLAY;
			} else if (strcasecmp(LocOpt->Display_Type, "LED") == 0) {
				display_type = LED_DISPLAY;
			} else //In conf file bluetooth and led flag is not present
			{
				display_type = INFO_DISPLAY;
			}

		}
	}

	/* dont support Tx logging with xml or csv format */
	// Default = NOLOG
	if (log_options == TXRXLOG || log_options == TXLOG) {
		if (logformat == XML || logformat == CSV) {
			printf(
					"\n xml & csv formats are not supported for Tx logging......\n");
			usage();
		}
	}
	/* 
	 TBD: Need to take of creating main_bluetooth for mutiple connections with multiple BT devices
	 This code block has to move to ASD functionlity block when main_bluetooth is getting started.
	 */
	if (!strcasecmp(can_interface, "BTCAN")) {
		btooth_arg = 4;
		pthread_create(&btCan_tId, NULL, main_bluetooth, (void *) &btooth_arg);
		threadCount++;
		sched_yield();
		pthread_create(&rxCan_tId, NULL, (void *) rx_can_client, NULL );
		threadCount++;
		sched_yield();
		sem_init(&can_sem, 0, 1);
	} else
		thread_id = get_can_data((void *) &sock, can_interface);
	strncpy((char *) arg->macaddr, (const char *) maddr, 17);
	// schan == 172 default
	arg->channel = (char) schan;
	// Default PROVIDER
	waveappmode = ServiceType;
	// Default 0
	if (Active_Msg == 1) {
		DIR * dirp;
		struct dirent * entry;
		int i = 0;

		dirp = opendir("/var/AML/"); /* There should be error handling after this */
		while ((entry = readdir(dirp)) != NULL) {
			if (entry->d_type == DT_REG
				) /* If the entry is a regular file */
				actMsgCount++;
		}
		closedir(dirp);
		actMsg = (ActiveMsg *) calloc(actMsgCount, sizeof(ActiveMsg));
		list();
		for (i = 0; i < actMsgCount; i++) {
			get_RSE_options(&actMsg[i]);
			actMsg[i].bcastintrvl_rse = (actMsg[i].pktdelaymsecs_rse)
					/ pktdelaymsecs;
			syslog(LOG_INFO, "AML: Active Msg got Loaded %s\n",
					actMsg[i].actfile);
		}
		apply_RSE_options(&actMsg[0]);
		sem_post(&rse_sem);
		if (actMsgCount > 0)
			Tx_Now = 1;
	}
	if (Udp_Rx == 1) {
		ChannelAcess = CHACCESS_CONTINUOUS; //for udp apps to be in continuous channelAccess for provider
		ExtAccess = 1; //ImmAccess and ExtAccess for User
		ImmAccess = 1;
		if ((invokeIPServer()) < 0) { //returns socket fd
			printf("Server Connection Establishment Failed..");
			return -1;
		}
	}
	cbInit(&cb, PH_MAX_POINTS, Ph_pnt); // initialize circular buffer to operate on Ph_pnt buffer

	srand((unsigned int) time(NULL));
	if (temp_id_control == 0) {
		temp_id[2] = (uint8_t) (((uint32_t) (mod_depl_dev_id) & 0xFF00) >> 8);
		temp_id[3] = (uint8_t) (((uint32_t) (mod_depl_dev_id) & 0x00FF));
	} else if (temp_id_control == 1) {
		temp_id[2] = rand();
		temp_id[3] = rand();
	}
	temp_id[0] = rand();
	temp_id[1] = rand();
	if (!RemoteEnable) {
		/* get the database value for mac address randomization & store it in a global variable */
		randomization_ptr = popen(
				"conf_get system:basicSettings:macAddressRandomization", "r");
		fgets(tmp_buf,
				strlen("system:basicSettings:macAddressRandomization") + 3,
				randomization_ptr);
		sscanf(tmp_buf, "%s %d", tmpstr, &randstatus);
		pclose(randomization_ptr);
		//printf("Random mac status %d\n", randstatus);
	}
	/* catch control-c and kill signal*/
	signal(SIGINT, (void *) sig_int);
	signal(SIGSEGV, (void *) sig_segv);
	signal(SIGTERM, (void *) sig_term);
	signal(SIGPIPE, SIG_IGN);
#ifdef IF_NECESSARY
	signal(SIGUSR1, (void *)asm_restart);
#endif
	if (thread_options & TX_MASK
		)
		//tmpsock_id = txsocket_id = AsmConnect(TX_SOCKET, RemoteIp);
	if (thread_options & RXALL_MASK
		)
		//tmpsock_id = rxsocket_id = AsmConnect(RX_SOCKET, RemoteIp);
	(void) syslog(LOG_INFO,
			"Starting Application getwbsstxrxencdec (%u) to%02x ta=%d ra=%d\n",
			app_psid, thread_options, txsocket_id, rxsocket_id);
	if (usegps)
		gpssockfd = gpsc_connect(RemoteIp);

	if (register_app() < 0)
		exit(-1);

#if 0
	if( waveappmode == USER )
	{
		printf("Inside User process1\n");
		memset(&ust, 0 , sizeof(WMEApplicationRequest));
		ust.psid = app_psid;
		if ((Userreqtype > USER_REQ_SCH_ACCESS_NONE) || (Userreqtype < USER_REQ_SCH_ACCESS_AUTO)) {
			printf("User request type invalid: setting default to auto\n");
			ust.userreqtype = USER_REQ_SCH_ACCESS_AUTO;
		} else {
			ust.userreqtype = Userreqtype;
		}
		if (ust.userreqtype == USER_REQ_SCH_ACCESS_AUTO_UNCONDITIONAL) {
			ust.channel = schan;
		}
		ust.schaccess = ImmAccess;
		ust.schextaccess = ExtAccess;
		ust.priority = priority;
		msgType = MessageType;
		secType = SecurityType;
		printf("Invoking WAVE driver \n");

		registerLinkConfirm(confirmBeforeJoin);
		if (invokeWAVEDevice(WAVEDEVICE_LOCAL, 0) < 0)
		{
			printf("Open Failed. Quitting\n");
			exit(-1);
		}
		printf("Registering User %d  app pid = %d\n", ust.psid,pid);
		if ((regRet = registerUser(pid, &ust)) < 0) {
			printf("ERR::Register User Failed \n");
			printf("Removing user if already present  %d\n", !removeUser(pid, &ust));
			printf("USER Registered %d with PSID =%u \n", (regRet=registerUser(pid, &ust)), ust.psid );
		}
		printf("In User end\n");
	}
	else if( waveappmode == PROVIDER )
	{
		taarg.channel = TAChannel;
		taarg.channelinterval = TAChannelInterval;
		msgType = MessageType;
		secType = SecurityType;
		if (ChannelAcess > 1) {
			printf("channel access set default to alternating access\n");
			channelaccess = CHACCESS_ALTERNATIVE;
		} else {
			channelaccess = ChannelAcess;
		}
		printf("Inside Provider process\n");
		printf("Filling Provider Service Table entry %d\n",buildPSTEntry());
		printf("Building a WME Application  Request %d\n",buildWMEApplicationRequest());
		printf("Builing TA request %d\n", buildWMETARequest());

		printf("Invoking WAVE driver \n");
		if (invokeWAVEDriver(0) < 0)
		{
			printf("Open Failed. Quitting\n");
			exit(-1);
		} else {
			printf("Driver Invoked\n");
		}
		registerWMENotifIndication(receiveWME_NotifIndication);
		registerWRSSIndication(receiveWRSS_Indication);

		registertsfIndication(receiveTsfTimerIndication);

		printf("Registering provider\n ");
		if ((regRet=registerProvider( pid, &entry)) < 0 ) {
			printf("ERR::Register Provider failed\n");
			removeProvider(pid, &entry);
			regRet=registerProvider(pid, &entry);
		} else {
			printf("provider registered with PSID = %u\n",entry.psid );
		}
		printf("starting TA\n");
		if (transmitTA(&tareq) < 0) {
			printf("send TA failed\n ");
		} else {
			printf("send TA successful\n");
		}
	}
	else {
		printf("ERR: Input value wrong for waveappmode\n");
	}
#endif
//pvk
	if (logging) {
		if (check_usb_mount() > 0) {
			set_logging_mode(0);
			set_logging_format(logformat); /*1=XML, 2=CSV 3=PCAPDHR(default) 4=PCAP*/
			set_logfile(log_to_utc ? logfile : NULL);
		} else {
			logging = 0;
			printf("Logging Failed USB NOT AVAILABLE..\n");
			syslog(LOG_INFO, "Logging Failed USB NOT AVAILABLE..\n");
		}
		if ((log_options == RXLOG) || usegps) {
			if ((open_log(app_psid)) < 0) {
				logging = 0;
				exit(-1);
			}
		}
		//gpssockfd = gpsc_connect();
	}
//pvk
	pthread_attr_init(&attr);
	pthread_attr_setschedpolicy(&attr, SCHED_OTHER);
	pthread_attr_setschedparam(&attr, &param);
	pthread_attr_setinheritsched(&attr, PTHREAD_INHERIT_SCHED);
	processid = getpid();
//     if(usegps){
//        ret = pthread_create(&gpsthread, NULL, gps_client, NULL );
//	sched_yield();
//	}

	if (thread_options & RXUDP_MASK) {
		printf(" UDP thread\n");
		pthread_create(&txrx_udp, NULL, udp_client, NULL);
		threadCount++;
		sched_yield();
	}
	if (RemoteEnable == 0
			&& ((thread_options & RXALL_MASK) || (thread_options & RX_MASK))) {
		printf(" RX thread\n");
		ret = pthread_create(&localrx, NULL, rx_client, NULL);
		threadCount++;
		if (ret) {
		}
		sched_yield();
	}
	if (thread_options == NOTX) {
		printf(" Gpsdata thread\n");
		ret = pthread_create(&gpsthread, NULL, gps_update, NULL);
		threadCount++;
		sched_yield();
	}
	if (thread_options & TX_MASK) {
		printf(" TX thread\n");
		pthread_create(&localtx, NULL, tx_client, NULL);
		threadCount++;
		sched_yield();
	}

	
    situationalApp_start();

	sched_yield();

	if (check_mac((char *) maddr) < 0) {
		pthread_create(&wrssi, NULL, wrssi_client, (void *) arg);
		threadCount++;
		sched_yield();
		pthread_join(wrssi, NULL);
	}

	if (RemoteEnable == 0
			&& ((thread_options & RXALL_MASK) || (thread_options & RX_MASK))) {
		if ((ret = pthread_join(localrx, &status_ptr)) < 0) {
			printf("join err\n");
		}
	}
	if (thread_options == NOTX) {
		if ((ret = pthread_join(gpsthread, NULL)) < 0) {
			printf("join err\n");
		}
	}
	if (thread_options & RXUDP_MASK) {
		if ((ret = pthread_join(txrx_udp, NULL)) < 0) {
			printf("join err\n");
		}
	}
	if (thread_options & TX_MASK
		)
		pthread_join(localtx, NULL);

	if (btCan_tId != 0)
		pthread_join(btCan_tId, NULL );
	if (rxCan_tId != 0)
		pthread_join(rxCan_tId, NULL );
	if (display_type == BLUETOOTH_DISPLAY)
		pthread_join(bluethread, NULL );
		
	if (threadCount == 0) {
		while (1)
			sleep(3600);
	}
	
	//qwarnStop();
	//qwarnJoin();
	//qwarnDestroy();
	//lockDestroy(pGpsLock);
	
	return 0;
}

int check_mac(char *mac) {
	int len = strlen(mac);
	int i = 0;
	while (i < len) {
		if (mac[i] != 0 && mac[i + 1] != 0)
			return -1;
		else
			i = i + 2;
	}
	return 0;
}

void *rx_client(void *data) {
	int ret = 0;
	pid = getpid();

	while (1) {
		ret = rxWSMMessage(pid, &rxmsg);
		if (ret > 0) {
			RxpktProcess();
		} else {
			blank++;
			usleep(1000);
		}
		sched_yield();
	}
}

int RxpktProcess() {
	int str_temp, len = 0;
	uint8_t pnum = 0;
	int error, curve_speed = 0;
	int Content_type, recv_size, from_txrx = 0;
	int lidx = 0, widx = 0, vsts = 0;

	if (count == 0)
	{
		gettimeofday(&rx_tvstart, NULL);
		PrevTime = rx_tvstart.tv_sec;
		PrevTime /= 300;
	}
	
	CurTime = (uint32_t) (wsmgps.actual_time);
	if ((int) ((CurTime / 300) - (PrevTime)) >= 1) //Free the hash structures for every 5 mins
	{
		FreeHash(whlist, wnodelist);
		PrevTime = (CurTime) / 300;
		CrtVrfyFlag = 1;
	}
	Content_type = AsmDecodeContentType(&rxpkt);

	/*if (rxpkt.wsmps == 0xff)
		printf(
			"Received WSMP Packet Channel = %d, Packet No =#%llu# Content_type# %s \n",
			rxpkt.chaninfo.channel,
			count,
			(Content_type ? (Content_type == AsmSign ? "Signed" : "Encrypted") : "Plain"));
	else
		printf("Received WSMP-S Packet Channel = %d, Packet No =#%llu# Content_type# %s \n", rxpkt.chaninfo.channel, count, (Content_type?(Content_type==AsmSign?"Signed":"Encrypted"):"Plain"));
	printf("<RSSI> %d <RSSI>\n",rxpkt.rssi);*/
	
	count++;
	
	if (Content_type == AsmSign)
	{
		bzero(send_buff_Inter, sizeof(send_buff_Inter));
		if (rxpkt.data.contents[2] >= 0 && rxpkt.data.contents[2] <= 5)
			printf("SignerIdentifierType: %s\n", signerType[(uint8_t) rxpkt.data.contents[2]]);
		else
			printf("SignerIdentifierType: unknown\n");
		msg_create_Sign_OTA(send_buff_Inter, (uint8_t*) rxpkt.data.contents,
				&recv_size, rxpkt.data.length);
		memcpy(&rxpkt.data.contents, &send_buff_Inter, recv_size);
		rxpkt.data.length = recv_size;
		//memcpy(&tmp_rxpkt, &rxpkt, sizeof(tmp_rxpkt));
		if (thread_options & RXALL_MASK) {
			lidx = LookupHash(whlist, wnodelist, rxpkt.macaddr);
			if (lidx == -1)
				CrtVrfyFlag = 1;
			if (rxpkt.psid == 0xBFE0)
				generationLocation_rx = TRUE;
			else
				generationLocation_rx = FALSE;
			error = AsmVerifyData(&rxpkt);
			if (error == 0x12)
				vsts = 1;
			else
				vsts = -1;
			if (lidx == -1 && vsts == 1) {
				widx = AllocateWnode(wnodelist, vsts, rxpkt.macaddr);
				InsertHash(whlist, wnodelist, widx, lidx);
			}
			if (vsts == -1)
				return -1;

		}
		//printf("Signature Algorithm %s\n",(Algo == ECDSA_256 ? "ECDSA-256": "ECDSA-224"));
	} else if (Content_type == AsmEncrypt) {
		
		bzero(send_buff_Inter, sizeof(send_buff_Inter));
		printf(
				"Encryption Algorithm %s\n",
				(rxpkt.data.contents[2] == SYMM_AES_128_CCM ? "AES_128_ccm" : " Not Known"));
		msg_create_Enc_OTA(send_buff_Inter, (uint8_t*) rxpkt.data.contents, &recv_size, rxpkt.data.length);
		
		memcpy(&rxpkt.data.contents, &send_buff_Inter, recv_size);
		rxpkt.data.length = recv_size;
		
		if (thread_options & RXALL_MASK)
			AsmDecryptData(&rxpkt);
	}
	
	if (rxpkt.data.contents[4] == WSMMSG_BSM) {
		memcpy(&str_temp, rxpkt.data.contents + 8, 4);
		if (BIGENDIAN) 
			str_temp = htobe32(str_temp);
		if (str_temp == *((uint32_t *) temp_id)) {
			if (temp_id_control == 0) {
				temp_id[0] = rand();
				temp_id[1] = rand();
			} else if (temp_id_control == 1) {
				temp_id[0] = rand();
				temp_id[1] = rand();
				temp_id[2] = rand();
				temp_id[3] = rand();
			}
		}
	}

	rxWSMIdentity(&rxmsg, Content_type);
	from_txrx = RX_PACKET;
	if (!rxmsg.decode_status)
	{
		asn_TYPE_descriptor_t **pdu = asn_pdu_collection;
		asn_TYPE_descriptor_t *pduType = pdu[rxmsg.type];

		WaveRxPacket *packet = waveRadio_createRxPacket(pduType, rxmsg.structure);
		if (packet)
		{
			situationalApp_pushWaveMessage(packet);
			rxmsg.structure = NULL;
		}
	} // !rxmsg.decode_status
}


//thread will update gpsdata for  every 200ms once
void *gps_update(void *data) {
	while (1) {
		printf("GPS Data gps_update\n");
		ReadGpsdata();
		printf("GPS Update Data %f %f\n", wsmgps.speed, wsmgps.course);
		if (wsmgps.course != GPS_INVALID_DATA && wsmgps.speed != GPS_INVALID_DATA) {
			printf("GPS Update Data %f %f\n", wsmgps.speed, wsmgps.course);
			//qwarnUpdateGpsData(&wsmgps);
			//btUpdateGpsData(&wsmgps);
		}
		sched_yield();
		mysleep(0, 200000000); //200ms
	}
}

int AsmSignData() {
	int send_size = 0, recv_size = 0; //i;
	uint32_t psId = 0, psidLen = 0;
	uint32_t certDelay;
	// send AsmMsg_Sign request

	if (wsmgps.latitude == GPS_INVALID_DATA)
		generationLatitude_tx = 900000001;
	else
		generationLatitude_tx = (long) ((wsmgps.latitude) * 10000000);
	if (wsmgps.longitude == GPS_INVALID_DATA)
		generationLongitude_tx = 1800000001;
	else
		generationLongitude_tx = (long) ((wsmgps.longitude) * 10000000);
	psId = swap32(app_psid);
	psId = putPsidbyLen((uint8_t*) &psId, app_psid, (int*) &psidLen);
	if ((pktdelaymsecs != 0)
			&& ((certDelay = certDelaytime / pktdelaymsecs)) != 0) {
		if (certchanged || packets == 0 || packets % certDelay == 0) {
			msg_create_sign_msg(send_buff_tx, (uint8_t*) wsmreq.data.contents,
					&send_size, wsmreq.data.length, SIGNER_INTERFACE_TYPE_CERT,
					&psId, psidLen, generationLocation_tx,
					generationLatitude_tx, generationLongitude_tx);
			certchanged = 0;
		} else {
			msg_create_sign_msg(send_buff_tx, (uint8_t*) wsmreq.data.contents,
					&send_size, wsmreq.data.length, CertificateType, &psId,
					psidLen, generationLocation_tx, generationLatitude_tx,
					generationLongitude_tx);
		}
	} else
		msg_create_sign_msg(send_buff_tx, (uint8_t*) wsmreq.data.contents,
				&send_size, wsmreq.data.length, SIGNER_INTERFACE_TYPE_CERT,
				&psId, psidLen, generationLocation_tx, generationLatitude_tx,
				generationLongitude_tx);
	do {
		INFO("Send AsmMsg_Sign request. [0x%02x]", send_buff_tx[0]);
		if (0 != AsmSend((char *) send_buff_tx, send_size, txsocket_id)) {
			ERROR(" Sending error.\n");
			return -1;
		}
		// receive AsmMsg_Sign response
		bzero(recv_buff_tx, sizeof(recv_buff_tx));
		recv_size = AsmRecv((char *) recv_buff_tx, sizeof(recv_buff_tx),
				txsocket_id);
		if (recv_size <= 0) {
			return -1;
		}
		if ((recv_buff_tx[0] != CMD_OK_SIGN_POST)
				&& (recv_buff_tx[0] != CMD_LCM_STATUS_RDY
						&& recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
			ERROR("Receive(AsmMsg_Sign request) error. [0x%02x]",
					recv_buff_tx[0]);
			if (asmStatus_tx == 00 || asmStatus_tx == 01) {
				asmStatus_tx = 10;
			}
			return recv_buff_tx[0];
		} else {
			INFO("Receive AsmMsg_Sign response. [0x%02x]", recv_buff_tx[0]);
			if (asmStatus_tx == 00 || asmStatus_tx == 10) {
				asmStatus_tx = 01;
			}
		}
	} while ((recv_buff_tx[0] == CMD_LCM_STATUS_RDY
			|| recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED));
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_extract_Sign_OTA(send_buff_tx, recv_buff_tx, &recv_size);
	memcpy(&wsmreq.data.contents, &send_buff_tx, recv_size);
	wsmreq.data.length = recv_size;
	return recv_buff_tx[0];
}

int AsmEncryptData() {
	int send_size = 0, recv_size = 0; //,i;
	// send AsmMsg_Enc request
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_create_enc_msg(send_buff_tx, (uint8_t *) wsmreq.data.contents,
			&send_size, wsmreq.data.length);
	do {
		INFO("Send AsmMsg_Enc request. [0x%02x]", send_buff_tx[0]);
		if (0 != AsmSend((char *) send_buff_tx, send_size, txsocket_id)) {
			return -1;
		}
		// receive AsmMsg_Enc response
		bzero(recv_buff_tx, sizeof(recv_buff_tx));
		recv_size = AsmRecv((char*) recv_buff_tx, sizeof(recv_buff_tx),
				txsocket_id);
		if (recv_size <= 0) {
			return -1;
		}
		if (recv_buff_tx[0] != CMD_OK_ENC_POST
				&& (recv_buff_tx[0] != CMD_LCM_STATUS_RDY
						&& recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
			ERROR("Receive error. [0x%02x]", recv_buff_tx[0]);
			return -1;
		} else {
			INFO("Receive AsmMsg_Enc response. [0x%02x]", recv_buff_tx[0]);
		}
	} while (recv_buff_tx[0] == CMD_LCM_STATUS_RDY
			|| recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED);
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_extract_Enc_OTA(send_buff_tx, recv_buff_tx, &recv_size);
	memcpy(&wsmreq.data.contents, &send_buff_tx, recv_size);
	wsmreq.data.length = recv_size;
	return recv_buff_tx[0];
}

int AsmVerifyData(WSMIndication *rxpkt) {
	int send_size = 0, recv_size = 0; //size1 = 0;
	int idx = 0, retIdx = 0, CertLen = 0;
	uint32_t psid = 0;
	Certificate parsedcert;
	// send AsmMsg_Verify request

	if (wsmgps.latitude == GPS_INVALID_DATA)
		generationLatitude_rx = 900000001;
	else
		generationLatitude_rx = (long) ((wsmgps.latitude) * 10000000);
	if (wsmgps.longitude == GPS_INVALID_DATA)
		generationLongitude_rx = 1800000001;
	else
		generationLongitude_rx = (long) ((wsmgps.longitude) * 10000000);
#ifndef AWSEC
	AsmMsg_Sign_Msg_Res_t *msg = (AsmMsg_Sign_Msg_Res_t *) rxpkt->data.contents;

	if (msg->signed_message_data[2] == 3 && CrtVrfyFlag == 1) { // For firstime certificate and contents send to asmbin to verify
		CrtVrfyFlag = 0;
#endif
		bzero(send_buff_rx, sizeof(send_buff_rx));
		msg_create_verify_msg((uint8_t*) rxpkt->data.contents, send_buff_rx,
				&send_size, msgValidityDistance, detectReplay,
				generationLocation_rx, generationLatitude_rx,
				generationLongitude_rx);
		do {
			INFO("Send AsmMsg_Verify request. [0x%02x]", send_buff_rx[0]);
			if (0 != AsmSend((char *) send_buff_rx, send_size, rxsocket_id)) {
				return -1;
			}
			// receive AsmMsg_Verify response
			bzero(recv_buff_rx, sizeof(recv_buff_rx));
			recv_size = AsmRecv((char *) recv_buff_rx, sizeof(recv_buff_rx),
					rxsocket_id);
			if (recv_size <= 0) {
				return -1;
			}
			if (recv_buff_rx[0] != CMD_OK_VERIFY_POST
					&& (recv_buff_tx[0] != CMD_LCM_STATUS_RDY
							&& recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
				uint8_t verifyerror = recv_buff_rx[0];
				switch (verifyerror) {
				case CMD_ERR_INCORRECT_SIGNING_CERT_TYPE: //0x36
				case CMD_ERR_MESSAGE_IS_REPLAY: //0x39
				case CMD_ERR_MESSAGE_OUT_OF_RANGE: //0x3A
				case CMD_ERR_INCORRECT_CA_CERT_TYPE: //0x3D
				case CMD_ERR_INCONSISTENT_CERT_SUBJECT_TYPE: //0x3E
				case CMD_ERR_INCONSISTENT_PERMISSIONS: //0x3F
				case CMD_ERR_INCONSISTENT_GEOGRAPHIC_SCOPE: //0x41
				case CMD_ERR_CERT_VERIFICATION_FAILED: //0x45
				case CMD_ERR_UNAUTHORIZED_GENERATION_LOCATION: //0x47
				case CMD_ERR_MESSAGE_VERIFICATION_FAILED: //0x48
				case CMD_ERR_UNSUPPORTED_SIGNER_TYPE: //0x50
				case CMD_ERR_UNAUTHORIZED_PSID_AND_PRIORITY: //0x52
				case CMD_ERR_INVALID_ALGORITHM: //0x55
				{
					uint8_t mb_rep_tx[1024], elevation_rx[2];
					int mb_send_size = 0; //size1 = 0;
					int val_16 = 0;
					const AsmMsg_Sign_Msg_Res_t* res = 0;
					res = (const AsmMsg_Sign_Msg_Res_t*) rxpkt->data.contents;
					if (wsmgps.altitude >= 0 && wsmgps.altitude <= 6143.9) {
						elevation_rx[0] =
								(uint8_t) (((uint32_t) (wsmgps.altitude * 10)
										& 0xFF00) >> 8);
						elevation_rx[1] =
								(uint8_t) (((uint32_t) (wsmgps.altitude * 10)
										& 0x00FF));
					} else if (wsmgps.altitude > -409.5
							&& wsmgps.altitude < -0.1) {
						val_16 = (uint32_t) (wsmgps.altitude * 10);
						val_16 = 65535 + val_16;
						elevation_rx[0] = (uint8_t) (((uint32_t) (val_16)
								& 0xFF00) >> 8);
						elevation_rx[1] = (uint8_t) (((uint32_t) (val_16)
								& 0x00FF));
					}

					if (wsmgps.altitude == GPS_INVALID_DATA) {
						elevation_rx[0] = ((61440 & 0xFF00) >> 8);
						elevation_rx[1] = ((61440 & 0x00FF));
					}
#if MBR_ENABLE
					msg_create_misbehavior_report(mb_rep_tx,&mb_send_size,res->signed_message_data, res->signed_message_length,generationLatitude_rx,generationLongitude_rx,elevation_rx);
					if(lcmSend((char *)mb_rep_tx,mb_send_size)<0) {
						//syslog(LOG_INFO,"MBR sending failed\n");
					}
#endif
				}
					break;
				}

				if (asmStatus_rx == 00 || asmStatus_rx == 01) {
					ERROR("Receive error. [0x%02x]", recv_buff_rx[0]);
					//		syslog(LOG_INFO,"Receive error. [0x%02x]", recv_buff_rx[0]);
					asmStatus_rx = 10;
				}
				return recv_buff_rx[0];
			} else {
				if (asmStatus_rx == 00 || asmStatus_rx == 10) {
					INFO("Receive AsmMsg_Verify response. [0x%02x]",
							recv_buff_rx[0]);
					//		syslog(LOG_INFO,"Receive AsmMsg_Verify response. [0x%02x]", recv_buff_rx[0]);
					asmStatus_rx = 01;
				}
			}
		} while (recv_buff_tx[0] == CMD_LCM_STATUS_RDY
				|| recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED);
		psid = getPsidbyLen(&recv_buff_rx[10], &retIdx);
		INFO("Receive AsmMsg_Verify PSID:%d", psid);
		idx = 10 + retIdx;
		send_size = 0;
		retIdx = 0;
		send_size = getValbyLen(&recv_buff_rx[idx], &retIdx);
		idx = idx + retIdx;
		memcpy(rxpkt->data.contents, &recv_buff_rx[idx], send_size);
		rxpkt->data.length = send_size;
		return recv_buff_rx[0];
#ifndef AWSEC
	} else if (msg->signed_message_data[2] == 3 && CrtVrfyFlag == 0) //For verified certificate Just extract the data
			{
		CertLen = certParse(
				(uint8_t*) (msg->signed_message_data + 3), &parsedcert);
		idx = 1 + 1 + 1 + CertLen + 1; // protocol version + content type + type + CertificateLength + MF/tf
		psid = getPsidbyLen(&msg->signed_message_data[idx], &retIdx);
		idx = idx + retIdx;
		send_size = 0;
		retIdx = 0;
		send_size = getValbyLen(&msg->signed_message_data[idx], &retIdx);
		idx = idx + retIdx;
		memmove(rxpkt->data.contents, &msg->signed_message_data[idx],
				send_size);
		rxpkt->data.length = send_size;
	} else if (msg->signed_message_data[2] == 2) //For Digest pkts only
			{
		idx = 1 + 1 + 1 + 8 + 1; // protocol version + content type + type + digest + MF/tf
		psid = getPsidbyLen(&msg->signed_message_data[idx], &retIdx);
		idx = idx + retIdx;
		send_size = 0;
		retIdx = 0;
		send_size = getValbyLen(&msg->signed_message_data[idx], &retIdx);
		idx = idx + retIdx;
		memmove(rxpkt->data.contents, &msg->signed_message_data[idx],
				send_size);
		rxpkt->data.length = send_size;
	}

	return 18; //0x12(success)
#endif
}

int AsmDecryptData(WSMIndication *rxpkt) {
	int send_size = 0, recv_size = 0;
	// send AsmMsg_Dec request
	bzero(send_buff_rx, sizeof(send_buff_rx));
	msg_create_dec_msg((uint8_t*) rxpkt->data.contents, send_buff_rx,
			&send_size);
	do {
		INFO("Send AsmMsg_Dec request. [0x%02x]", send_buff_rx[0]);
		if (0 != AsmSend((char *) send_buff_rx, send_size, rxsocket_id)) {
			return -1;
		}
		// receive AsmMsg_Dec response
		bzero(recv_buff_rx, sizeof(recv_buff_rx));
		recv_size = AsmRecv((char *) recv_buff_rx, sizeof(recv_buff_rx),
				rxsocket_id);
		if (recv_size <= 0) {
			return -1;
		}
		if (recv_buff_rx[0] != CMD_OK_DEC_POST
				&& (recv_buff_tx[0] != CMD_LCM_STATUS_RDY
						&& recv_buff_tx[0] != CMD_LCM_STATUS_CERT_CHANGED)) {
			ERROR("Receive error(AsmMsg_Dec request). [0x%02x]",
					recv_buff_rx[0]);
			return -1;
		} else {
			INFO("Receive AsmMsg_Dec response. [0x%02x]", recv_buff_rx[0]);
		}
	} while (recv_buff_tx[0] == CMD_LCM_STATUS_RDY
			|| recv_buff_tx[0] == CMD_LCM_STATUS_CERT_CHANGED);
	bzero(send_buff_tx, sizeof(send_buff_tx));
	msg_decode_dec_msg(send_buff_tx, recv_buff_rx, &recv_size);
	memcpy(rxpkt->data.contents, &send_buff_tx, recv_size);
	rxpkt->data.length = recv_size;
	return 0;
}

int AsmDecodeContentType(WSMIndication *rxpkt) {
	int version, Content_type = 0, offset = 0;
	version = rxpkt->data.contents[offset];
	if (version == 2) {
		offset++;
		Content_type = rxpkt->data.contents[offset];
		return Content_type;
	}
	return Content_type;
}

void *tx_client(void *data) {
	int result, CrtChngCheck = 0;
	//unsigned long tx_timediff_usec;
	unsigned long tmp_timediff_usec;
	long deltatsf, modtsf;
	int tmp_secs = 0;
	long int tmp_nsecs = 0;
	int from_txrx = 0;
	uint64_t tsf64;

	from_txrx = TX_PACKET;
	pid = getpid();

	buildWSMRequestPacket();

	while (1) {

		if ((num != 0) && (packets != 0) && (((packets + 1) % num) == 0))
			ALL = 1;
		if (pktdelaymsecs == 0) //for immediate forward wait for next receive after transmit once
			sem_wait(&rse_sem);
		//result =txWSMPPkts(pid, from_txrx);
		gettimeofday(&gtv, NULL);
		if (certChangeFlag && (wsmreq.security == AsmSign)) {
			CrtChngCheck = gtv.tv_sec % 300;
			if ((changeFlag == 2) && (CrtChngCheck >= 05)
					&& (CrtChngCheck <= 30))
				changeFlag = 1;
			else if ((changeFlag == 0) && (CrtChngCheck > 30))
				changeFlag = 2;
			if (changeFlag == 1)
				random_mac_tmpid_at_cert_change(CrtChngCheck);
		}
		if (packets == 0)
			tx_tvstart = gtv;
		if (msgType[0] != 0xff && msgType[1] != 0xff) {
			if (buildWSMRequestData_first() <= 0) {
				//printf("Err: w\n");  // Donot send the current pkt if WSM Request frame failed
			} else {
				//buildACMRequestData();
				//buildEVARequestData();
				buildBSMRequestData();
				//result = txWSMPPkts(pid, from_txrx);
				/*if(qwarnIsQueued()) {
					time(&Curr_Timer);
					if(difftime(Curr_Timer, TIM_Timer) > TIM_TIMEOUT) {
					    // Transmit dummy TIM

						buildTIMRequestData();	
						result = txWSMPPkts(pid, from_txrx);
						time(&TIM_Timer);
					}
				}*/
			    
			    // Rebroadcast TIMs in Queue
			    /*
			    {
			        TimProcessor* pTimProcessor = qwarnGetTimProcessor();
			    
			        void* pMessage = NULL;
                    unsigned short messageType = 0;
                    struct timespec tStart={0,0}, tEnd={0,0};
                    double timediff_ms = 0.0;
			        
			        clock_gettime(CLOCK_MONOTONIC, &tStart);
                    while(msgQueuePopBack(pTimProcessor->pOutgoingQueue, &pMessage, &messageType))
                    {
                        asn_enc_rval_t rvalenc = der_encode_to_buffer(&asn_DEF_TravelerInformation, pMessage, &wsmreq.data.contents, 2500);
                        
                        if(rvalenc.encoded != -1)
                        {
                            printf("Rebroadcasting TIM\n");
                            wsmreq.data.length = rvalenc.encoded;
                            txWSMPPkts(pid, from_txrx);
                        }
                        
                        msgQueueFreeMessage(pMessage, WSMMSG_TIM);
                    
                        clock_gettime(CLOCK_MONOTONIC, &tEnd);
                        
                        double timediff_s = tsToSeconds(tsSubtract(tEnd, tStart));
                        
                        if(timediff_s > 0.1)
                        {
                            break;
                        }
                    }
                }
                */	
		}
		} else if (Tx_Now) {
			if (pktdelaymsecs == 0) {
				memcpy(&wsmreq.data.contents, &actMsg[0].payload,
						actMsg[0].payload_size);
				wsmreq.data.length = actMsg[0].payload_size;
				Tx_Now = 0; //immediate forward only once
				result = txWSMPPkts(pid, from_txrx);
				sem_post(&rse_sem2); //Give access to udpClient to fill next packet
			} else {
				uint32_t i = 0;
				for (i = 0; i < actMsgCount; i++) {
					if (strcmp(actMsg[i].actfile, "NOFILE") != 0){
						if ((gtv.tv_sec >= actMsg[i].start_utctime_sec)
								&& (gtv.tv_sec <= actMsg[i].stop_utctime_sec)) {
							if (actMsg[i].bcastintrvl_rse == 1) {
								memcpy(&wsmreq.data.contents,
										&actMsg[i].payload,
										actMsg[i].payload_size);
								wsmreq.data.length = actMsg[i].payload_size;
								actMsg[i].bcastintrvl_rse =
										(actMsg[i].pktdelaymsecs_rse)
												/ pktdelaymsecs;
								apply_RSE_options(&actMsg[i]);
								buildWSMRequestPacket();
								result = txWSMPPkts(pid, from_txrx);
							} else
								actMsg[i].bcastintrvl_rse--;
						} else if (gtv.tv_sec > actMsg[i].stop_utctime_sec) {
							sprintf(syscmd, "rm -f %s", actMsg[i].actfile); //remove from usb
							system(syscmd);
							syslog(LOG_INFO, "AML: Active Msg got expired %s\n",
									actMsg[i].actfile);
							strcpy(actMsg[i].actfile, "NOFILE");
							//return 0; //delete that active msg list file since it's expired.
						}
					}
				}
			}
		} else
			return 0;

		if (pktdelaymsecs == 0) {
			continue;
		}
//uint8_t temp_id[4];
		tmp_timediff_usec = pktdelaymsecs * 1000;
		tsf64 = (uint64_t) generatetsfRequest();
		modtsf = (tsf64 % tmp_timediff_usec);
		deltatsf = tmp_timediff_usec - modtsf;
		//removed 30 ms addition as now we are sending gps-referenced time in packet and not tsf-referenced time

		if (deltatsf < 20000) {
			if (deltatsf < 0) {
				syslog(LOG_INFO, "timediff=%ld,tsif64=%llu dt %lu mc %d\n",
						tmp_timediff_usec, tsf64, deltatsf, msgCnt);
			} else {
				tmp_timediff_usec += deltatsf;
			}
		} else
			tmp_timediff_usec = deltatsf;

		// printf("timediff=%ld,tsf64=%llu\n",tmp_timediff_usec,tsf64);

		tmp_secs = tmp_timediff_usec / 1000000;
		tmp_nsecs = (tmp_timediff_usec % 1000000) * 1000;
		sched_yield();
		usleep(100000);
		//printf("tmp_secs: %d tmp_nsecs: %d\n", tmp_secs, tmp_nsecs);
		//mysleep(tmp_secs, tmp_nsecs);
		ALL = 0;

	}

}

void *wrssi_client(void *arg) {
//    int sts;
	struct arguments *argument;
	argument = (struct arguments *) arg;

	//   set_args( &wrssrq.macaddr, argument->macaddr, ADDR_MAC, 0);
	// set_args( &wrssrq.wrssreq_elem.request.channel, NULL, UINT8, argument->channel );
	set_args(&wrssrq.macaddr, argument->macaddr, ADDR_MAC);
	//set_args( &wrssrq.wrssreq_elem.request.channel, &argument->channel, UINT8);
	wrssrq.wrssreq_elem.request.channel = (char) argument->channel;
	registerWRSSIndication(receiveWRSS_Indication);

	signal(SIGINT, (void *) sig_int);
	signal(SIGTERM, (void *) sig_term);
	memset(&ntsleep, 0, sizeof(ntsleep));
	memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));

	PKT_DELAY_SECS = pktdelaymsecs / 1000;
	PKT_DELAY_NSECS = (pktdelaymsecs % 1000) * 1000 * 1000;
	while (1) {
		printf("WRSSI Thread\n");
		mysleep(PKT_DELAY_SECS, PKT_DELAY_NSECS);
		sendreport = 1;
		wrss_request();
	}

	/*
	 while(1)
	 {
	 ntsleep.tv_nsec = 0;
	 ntsleep.tv_sec = 1;
	 ntleft_sleep.tv_nsec = 0;
	 ntleft_sleep.tv_sec = 0;
	 do
	 {
	 sts = nanosleep(&ntsleep, &ntleft_sleep);
	 if((ntleft_sleep.tv_nsec == 0) && (ntleft_sleep.tv_sec == 0)) {
	 wrss_request();
	 break;
	 }
	 memcpy(&ntsleep,&ntleft_sleep, sizeof(ntsleep));
	 memset(&ntleft_sleep, 0, sizeof(ntleft_sleep));
	 }
	 while( 1 );
	 }
	 */

}
#if 0
void *gps_client( void *data )
{
	long int ns_sleep= 50 / 1000; //50ms sleep
	ns_sleep=(50 % 1000) *1000*1000;

	gpssockfd = create_connect_sock();

	if (gpssockfd <0) {
		printf("Socket connection failure errno %d (%d)\n", errno, gpssockfd);
		return -1;
	}
	false_read_data(gpssockfd,&wsmgps);
	sock_connected=1;
	while(1)
	{
		read_data(gpssockfd,&wsmgps);
		sched_yield();
		mysleep(0,ns_sleep);
	}
}
#endif
int buildPSTEntry() {

	entry.psid = app_psid;
	entry.priority = priority;
	entry.channel = schan;
	entry.serviceport = SERVICE_PORT;
	entry.ipservice = app_ip_sch_permit;
	entry.repeatrate = repeatRate_Wsa;
	entry.linkquality = 1;
	entry.channelaccess = channelaccess;

	return 1;
}
//converts a string of xx:xx:xx:xx:xx:xx hex digits to 6 byte mac address
int stringtomac(uint8_t*mac, char* macstr) {
	uint8_t number, i;
	char ch;
	const char cSep = '-';
	for (i = 0; i < 6; ++i) {
		ch = tolower(*macstr++);
		if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f')) {
			return 0;
		}
		number = isdigit (ch) ? (ch - '0') : (ch - 'a' + 10);
		ch = tolower(*macstr);
		if ((i < 5 && ch != cSep) || (i == 5 && ch != '\0' && !isspace (ch))) {
			if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f')) {
				return 0;
			}
			number <<= 4;
			number += isdigit (ch) ? (ch - '0') : (ch - 'a' + 10);
			++macstr;
			ch = *macstr;
			if (i < 5 && ch != cSep) {
				return 0;
			}
			mac[i] = number;

		}
		++macstr;
	}
	return 1;
}

//Function for encoding horizontal confidence and elevation confidence to 4 bit value
uint8_t encode_poselev_confidence(double actual_conf) {
	double confsteps[16] = { 100000, 500.0, 200.0, 100.0, 50.0, 20.0, 10.0, 5.0,
			2.0, 1.0, 0.5, 0.2, 0.1, 0.05, 0.02, 0.01 };
	uint8_t i;
	for (i = 0; i < 16; i++) {
		if (actual_conf >= confsteps[i])
			break;
	}
	if (i == 16)
		return 0;
	else if (actual_conf == confsteps[i])
		return i;
	else
		return i - 1;
}

void wsa_conf_parse(void) {
#define EMIX(x, y)  (((x) > (y)) ? (x) : (y)) 
	FILE *fdrd;
	char *token = NULL, *sts = NULL;
	uint8_t value;
	int8_t retn;
	char read_line[200];
	WMEWSAConfig appwsaconfig;
	char tmpstr[50] = { 0 };
	char ipv6_addr[50] = { 0 };
	char pflenstr[5] = { 0 };
	double eph;

	/* open the config file in read mode */
	char conf_file[] = "/var/wsa.conf";
	memset(&appwsaconfig, 0, sizeof(WMEWSAConfig));
	fdrd = fopen(conf_file, "r");
	if (fdrd == NULL) {
		printf("Error opening %s file\n", conf_file);
		return;
	}

//adding 3dloc values from gpsreceiver
	printf("GPS Data wsa_conf_parse\n");
	ReadGpsdata();

	if ((wsmgps.latitude != GPS_INVALID_DATA)
			&& (wsmgps.longitude != GPS_INVALID_DATA)) {
		appwsaconfig.loc_3d.latitude = wsmgps.latitude * 10000000;
		appwsaconfig.loc_3d.longitude = wsmgps.longitude * 10000000;
		appwsaconfig.loc_3d.elevation = wsmgps.altitude * 10;
		eph = EMIX(wsmgps.epx,wsmgps.epy);
		value = encode_poselev_confidence(eph);
		appwsaconfig.loc_3d.pos_elev_confidence |= ((value << 4) & 0xf0);
		value = encode_poselev_confidence(wsmgps.epv);
		appwsaconfig.loc_3d.pos_elev_confidence |= (value & 0x0f);
		appwsaconfig.loc_3d.pos_accuracy = 0xffffffff;
	} else {
		appwsaconfig.loc_3d.latitude = 900000001;
		appwsaconfig.loc_3d.longitude = 1800000001;
		appwsaconfig.loc_3d.elevation = 61440;
		appwsaconfig.loc_3d.pos_elev_confidence = 0xff;
		appwsaconfig.loc_3d.pos_accuracy = 0xffffffff;
	}
	memset(read_line, 0, sizeof(read_line));
	while ((sts = fgets(read_line, sizeof(read_line), fdrd)) != NULL) {
		if (read_line[0] != '#' && read_line[0] != ';' && read_line[0] != ' ') {
			token = strtok(read_line, "=");
#ifdef LOC3DINCONF
			if (strcasecmp(token, "3dloc.latitude") == 0) {
				token = strtok(NULL, "\n");
				if((token != NULL) &&(strcasecmp(token,"unavailable") != 0)) {
					appwsaconfig.loc_3d.latitude = atoi(token)*10000000;
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.longitude") == 0) {
				token = strtok(NULL, "\n");
				if((token != NULL) &&(strcasecmp(token,"unavailable") != 0)) {
					appwsaconfig.loc_3d.longitude = atoi(token)*10000000;
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.elevation") == 0) {
				token = strtok(NULL, "\n");
				if((token != NULL) &&(strcasecmp(token,"unavailable") != 0)) {
					appwsaconfig.loc_3d.elevation = atoi(token)*10;
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.pos_confidence") == 0) {
				token = strtok(NULL, "\n");
				if(token != NULL) {
					if(strcasecmp(token,"unavailable") == 0) value = 0;
					else if(strcasecmp(token,"500") == 0) value = 1;
					else if(strcasecmp(token,"200") == 0) value = 2;
					else if(strcasecmp(token,"100") == 0) value = 3;
					else if(strcasecmp(token,"50") == 0) value = 4;
					else if(strcasecmp(token,"20") == 0) value = 5;
					else if(strcasecmp(token,"10") == 0) value = 6;
					else if(strcasecmp(token,"5") == 0) value = 7;
					else if(strcasecmp(token,"2") == 0) value = 8;
					else if(strcasecmp(token,"1") == 0) value = 9;
					else if(strcasecmp(token,"0.5") == 0) value = 10;
					else if(strcasecmp(token,"0.2") == 0) value = 11;
					else if(strcasecmp(token,"0.1") == 0) value = 12;
					else if(strcasecmp(token,"0.05") == 0) value = 13;
					else if(strcasecmp(token,"0.02") == 0) value = 14;
					else if(strcasecmp(token,"0.01") == 0) value = 15;
					appwsaconfig.loc_3d.pos_elev_confidence |= ((value << 4) & 0xf0);
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.elev_confidence") == 0) {
				token = strtok(NULL, "\n");
				if(token != NULL) {
					if(strcasecmp(token,"unavailable") == 0) value = 0;
					else if(strcasecmp(token,"500") == 0) value = 1;
					else if(strcasecmp(token,"200") == 0) value = 2;
					else if(strcasecmp(token,"100") == 0) value = 3;
					else if(strcasecmp(token,"50") == 0) value = 4;
					else if(strcasecmp(token,"20") == 0) value = 5;
					else if(strcasecmp(token,"10") == 0) value = 6;
					else if(strcasecmp(token,"5") == 0) value = 7;
					else if(strcasecmp(token,"2") == 0) value = 8;
					else if(strcasecmp(token,"1") == 0) value = 9;
					else if(strcasecmp(token,"0.5") == 0) value = 10;
					else if(strcasecmp(token,"0.2") == 0) value = 11;
					else if(strcasecmp(token,"0.1") == 0) value = 12;
					else if(strcasecmp(token,"0.05") == 0) value = 13;
					else if(strcasecmp(token,"0.02") == 0) value = 14;
					else if(strcasecmp(token,"0.01") == 0) value = 15;
					appwsaconfig.loc_3d.pos_elev_confidence |= (value & 0x0f);
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.pos_elev_confidence") == 0) {
				if((token != NULL) &&(strcasecmp(token,"unavailable") != 0)) {
					token = strtok(NULL, "\n");
					appwsaconfig.loc_3d.pos_elev_confidence = atoi(token);
				}
				continue;
			}
			if (strcasecmp(token, "3dloc.pos_accuracy") == 0) {
				token = strtok(NULL, "\n");
				if(token != NULL) {
					if(strcasecmp(token,"unavailable") == 0)
					appwsaconfig.loc_3d.pos_accuracy = 0xffffffff;
					else
					appwsaconfig.loc_3d.pos_accuracy = atoi(token);
				}
				continue;
			}
#endif
			if (strcasecmp(token, "advertiser_identifier") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					sscanf(token, "%s", appwsaconfig.adv_identifier.ssid);
					appwsaconfig.adv_identifier.len = strlen(token);
				}
				continue;
			}
			if (strcasecmp(token, "servinfo.psc") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					sscanf(token, "%s", entry.psc);
					entry.psclen = strlen(token);
				}
				continue;
			}
			if (strcasecmp(token, "servinfo.ipv6addr") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					bzero(tmpstr, 50);
					sscanf(token, "%s", tmpstr);
					inet_pton(AF_INET6, tmpstr, &entry.ipv6addr);
				}
				continue;
			}
			if (strcasecmp(token, "servinfo.port") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL) && (strcasecmp(token, "unavailable") != 0))
					entry.serviceport = atoi(token);
				continue;
			}
			if (strcasecmp(token, "servinfo.prov_macaddr") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					if (stringtomac(entry.servmac, token) == 0)
						bzero(entry.servmac, 6);
				} else
					bzero(entry.servmac, 6);
				continue;
			}
			if (strcasecmp(token, "wra.ipprefix") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					bzero(tmpstr, 50);
					sscanf(token, "%s", tmpstr);
					inet_pton(AF_INET6, tmpstr, &appwsaconfig.wra.ipPrefix);
				}
				continue;
			}
			if (strcasecmp(token, "wra.prefixlen") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					appwsaconfig.wra.prefix_length = atoi(token);
					sprintf(pflenstr, "/%s", token);
				}
				continue;
			}
			if (strcasecmp(token, "wra.defaultgw") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					bzero(tmpstr, 50);
					sscanf(token, "%s", tmpstr);
					retn = inet_pton(AF_INET6, tmpstr,
							&appwsaconfig.wra.defaultgw);
					if (retn > 0)
						strcpy(ipv6_addr, tmpstr);
				}
				continue;
			}
			if (strcasecmp(token, "wra.primaryDNS") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					bzero(tmpstr, 50);
					sscanf(token, "%s", tmpstr);
					retn = inet_pton(AF_INET6, tmpstr,
							&appwsaconfig.wra.primaryDNS);
				}
				continue;
			}
			if (strcasecmp(token, "wra.secondaryDNS") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					bzero(tmpstr, 50);
					sscanf(token, "%s", tmpstr);
					inet_pton(AF_INET6, tmpstr,
							&appwsaconfig.wra_ext.secondaryDNS);
				}
				continue;
			}
			if (strcasecmp(token, "wra.gwmacaddr") == 0) {
				token = strtok(NULL, "\n");
				if ((token != NULL)
						&& (strcasecmp(token, "unavailable") != 0)) {
					if (stringtomac(appwsaconfig.wra_ext.gwmacaddr, token) == 0)
						bzero(appwsaconfig.wra_ext.gwmacaddr, 6);
					printf("in gwmacaddr");
				} else
					bzero(appwsaconfig.wra_ext.gwmacaddr, 6);
				continue;
			}
		}
	}
	if (appwsaconfig.wra.prefix_length > 0) {
		bzero(read_line, 200);
		strcat(ipv6_addr, pflenstr);
		sprintf(read_line, "ip a a %s dev brwifi", ipv6_addr);
		system(read_line);
	}
	fclose(fdrd);
	appwsaconfig.isupdated = 1;
	wsa_config(&appwsaconfig);
	return;

#undef EMIX
}
void buildICAPacket(WSMData *pktData) {
	// int j;
	static int pktnum = 0;
	PathHistoryPointType_01_t *first;
	asn_enc_rval_t rvalenc;
	IntersectionCollision_t *ica;
	ica = (IntersectionCollision_t *) calloc(1, sizeof(*ica));
	ica->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	ica->msgID.size = sizeof(uint8_t);
	ica->msgID.buf[0] = DSRCmsgID_intersectionCollisionAlert;

	ica->msgCnt = pktnum % 127;
	pktnum++;
	ica->id.buf = (uint8_t *) calloc(4, sizeof(uint8_t));
	ica->id.size = 4 * sizeof(uint8_t);
	memcpy(ica->id.buf, temp_id, 4);

	first = (PathHistoryPointType_01_t *) calloc(1,
			sizeof(PathHistoryPointType_01_t));

	ica->path.crumbData.present =
			PathHistory__crumbData_PR_pathHistoryPointSets_01;
	first->latOffset = 0;
	first->longOffset = 0;

	ASN_SEQUENCE_ADD(&ica->path.crumbData.choice.pathHistoryPointSets_01.list,
			first);

	ica->intersetionID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	ica->intersetionID.size = sizeof(uint8_t);
	ica->intersetionID.buf[0] = 0;

	ica->laneNumber.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	ica->laneNumber.size = sizeof(uint8_t);
	ica->laneNumber.buf[0] = 0;
	ica->eventFlag = 0;

	rvalenc = der_encode_to_buffer(&asn_DEF_IntersectionCollision, ica,
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
		printf("Structure successfully encoded %d\n", rvalenc.encoded);
		pktData->length = rvalenc.encoded;
		asn_DEF_IntersectionCollision.free_struct(
				&asn_DEF_IntersectionCollision, ica, 0);

	}
}

void buildMAPPacket(WSMData *pktData) {

	asn_enc_rval_t rvalenc;
	MapData_t *map;
	//int i;
	static int msgcount;

	map = (MapData_t *) calloc(1, sizeof(MapData_t));
	map->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	map->msgID.size = sizeof(uint8_t);
	map->msgID.buf[0] = DSRCmsgID_mapData;

	map->msgCnt = msgcount % 127;
	msgcount++;

	rvalenc = der_encode_to_buffer(&asn_DEF_MapData, map, &pktData->contents,
			1000);

	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
		printf("Structure successfully encoded %d\n", rvalenc.encoded);
		pktData->length = rvalenc.encoded;
		asn_DEF_MapData.free_struct(&asn_DEF_MapData, map, 0);
	}

}

void buildSPATPacket(WSMData *pktData) {
	//static int pktnum = 0;
	//int i;
	asn_enc_rval_t rvalenc;
	SPAT_t *spat;
	IntersectionState_t *intersectionstate;
	MovementState_t *movementstate;
	MovementState_t *movementstate1;

	spat = (SPAT_t *) calloc(1, sizeof(SPAT_t));
	spat->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	spat->msgID.size = sizeof(uint8_t);
	spat->msgID.buf[0] = DSRCmsgID_signalPhaseAndTimingMessage;

	intersectionstate = (IntersectionState_t *) calloc(1,
			sizeof(IntersectionState_t));
	intersectionstate->id.buf = (uint8_t *) calloc(4, sizeof(uint8_t));
	intersectionstate->id.size = 4;
	intersectionstate->id.buf[0] = 0x00;
	intersectionstate->id.buf[1] = 0x00;
	intersectionstate->id.buf[2] = 0x00;
	intersectionstate->id.buf[3] = 0x64;
	intersectionstate->status.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	intersectionstate->status.size = sizeof(uint8_t);
	intersectionstate->status.buf[0] = 0;
	intersectionstate->timeStamp = (TimeMark_t *) calloc(1, sizeof(uint32_t));
	*(intersectionstate->timeStamp) = 0x4EBAF6B5;

	movementstate = (MovementState_t *) calloc(1, sizeof(MovementState_t));
	movementstate->laneSet.buf = (uint8_t *) calloc(6, sizeof(uint8_t));
	movementstate->laneSet.size = 3;
	movementstate->laneSet.buf[0] = 0x01;
	movementstate->laneSet.buf[1] = 0x02;
	movementstate->laneSet.buf[2] = 0x03;
	movementstate->currState = (SignalLightState_t *) calloc(1,
			sizeof(SignalLightState_t));
	movementstate->currState[0] = 0x01;
	movementstate->timeToChange = 0x1E;
	movementstate->yellState = (SignalLightState_t *) calloc(1,
			sizeof(SignalLightState_t));
	movementstate->yellState[0] = 0x02;
	movementstate->yellTimeToChange = (TimeMark_t *) calloc(1,
			sizeof(uint32_t));
	*(movementstate->yellTimeToChange) = 0x00000024;
	ASN_SEQUENCE_ADD(&intersectionstate->states.list, movementstate);

	movementstate1 = (MovementState_t *) calloc(1, sizeof(MovementState_t));
	movementstate1->laneSet.buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	movementstate1->laneSet.size = 1;
	movementstate1->laneSet.buf[0] = 0x03;
	movementstate1->currState = (SignalLightState_t *) calloc(1,
			sizeof(SignalLightState_t));
	movementstate1->currState[0] = 0x10;
	movementstate1->timeToChange = 0x14;
	movementstate1->yellState = (SignalLightState_t *) calloc(1,
			sizeof(SignalLightState_t));
	movementstate1->yellState[0] = 0x20;
	movementstate1->yellTimeToChange = (TimeMark_t *) calloc(1,
			sizeof(uint32_t));
	*(movementstate1->yellTimeToChange) = 0x00000024;

	ASN_SEQUENCE_ADD(&intersectionstate->states.list, movementstate1);
	ASN_SEQUENCE_ADD(&spat->intersections.list, intersectionstate);

	rvalenc = der_encode_to_buffer(&asn_DEF_SPAT, spat, &pktData->contents,
			1000);

	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
		//  printf("Structure successfully encoded %d\n", rvalenc.encoded);
		pktData->length = rvalenc.encoded;
		asn_DEF_SPAT.free_struct(&asn_DEF_SPAT, spat, 0);
	}
}

void buildTIMPacket(WSMData *pktData) {
	int lat, lon;
	asn_enc_rval_t rvalenc;
	TravelerInformation_t *ti;
//	FurtherInfoID_t fid;
	struct TravelerInformation__dataFrames__Member *frame_member;
	ValidRegion_t *val_reg;
	Offsets_t *offset;
	struct ITIScodesAndText__Member *ITIScodeandtext_member;
	//Position3D_t *comanchor;
	//LaneWidth_t lane_width;
	//DirectionOfUse_t *directionality;
	ti = (TravelerInformation_t *) calloc(1, sizeof(TravelerInformation_t));
	/********************** part1 message frame header *****************/

	ti->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	ti->msgID.size = sizeof(uint8_t);
	ti->msgID.buf[0] = DSRCmsgID_travelerInformation;
	ti->packetID = calloc(1, sizeof(UniqueMSGID_t));
	ti->packetID->buf = (uint8_t *) calloc(1, 8*sizeof(uint8_t));
	ti->packetID->size = 8*sizeof(uint8_t);
	ti->packetID->buf[0] = rand();
	ti->packetID->buf[1] = rand();
	ti->packetID->buf[2] = rand();
	ti->packetID->buf[3] = rand();
	ti->packetID->buf[4] = 37;
	ti->packetID->buf[5] = rand();
	ti->packetID->buf[6] = rand();
	ti->packetID->buf[7] = rand();
	ti->crc.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	ti->crc.size = sizeof(uint8_t);
	ti->crc.buf[0] = 0;
	if (FrameType > 0 && FrameType < 4) {
		frame_member = (struct TravelerInformation__dataFrames__Member*) calloc(
				1, sizeof(struct TravelerInformation__dataFrames__Member));
		frame_member->frameType.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
		frame_member->frameType.size = sizeof(uint8_t);
		frame_member->frameType.buf[0] = FrameType; //infotype 2 "road signage"
		frame_member->msgId.present = msgId_PR_furtherInfoID;

		/*frame_member->msgId.choice.roadSignID.position.lat = latitude_val;
		frame_member->msgId.choice.roadSignID.position.Long = longitude_val;
		frame_member->msgId.choice.roadSignID.position.elevation =
				(Elevation_t *) calloc(1, sizeof(Elevation_t));
		frame_member->msgId.choice.roadSignID.position.elevation->buf =
				(uint8_t*) calloc(2, sizeof(uint8_t));
		frame_member->msgId.choice.roadSignID.position.elevation->size = 2
				* sizeof(uint8_t);
		frame_member->msgId.choice.roadSignID.position.elevation->buf[0] =
				elevation_val[0];
		frame_member->msgId.choice.roadSignID.position.elevation->buf[1] =
				elevation_val[1];
		frame_member->msgId.choice.roadSignID.viewAngle.buf =
				(uint8_t *) calloc(1, sizeof(uint8_t));
		frame_member->msgId.choice.roadSignID.viewAngle.size = sizeof(uint8_t);
		frame_member->msgId.choice.roadSignID.viewAngle.buf[0] = 0;
		frame_member->msgId.choice.roadSignID.mutcdCode = (MUTCDCode_t*) calloc(
				1, sizeof(uint8_t));
		frame_member->msgId.choice.roadSignID.mutcdCode->buf =
				(uint8_t*) calloc(1, sizeof(uint8_t));
		frame_member->msgId.choice.roadSignID.mutcdCode->size = sizeof(uint8_t);
		frame_member->msgId.choice.roadSignID.mutcdCode->buf[0] =
				MUTCDCode_none; */
		frame_member->startTime = 0;
		frame_member->duratonTime = 0;  
		frame_member->priority = 0;

		/************************ part2 message,applicable regions of use **********************/

		/*comanchor = (Position3D_t *) calloc(1,sizeof(Position3D_t));
		 comanchor->lat=0;
		 comanchor->Long=0;
		 comanchor->elevation = (Elevation_t *) calloc(1,sizeof(Elevation_t));
		 comanchor->elevation->buf = (uint8_t*) calloc(2,sizeof(uint8_t));
		 comanchor->elevation->size = 2*sizeof(uint8_t);
		 comanchor->elevation->buf[0] = 0;						//OPTIONAL ELEMENTS
		 comanchor->elevation->buf[1] = 1;
		 frame_member->commonAnchor = comanchor; //anchor position elevation could also be there
		 lane_width =  0; // Lanewidth
		 frame_member->commonLaneWidth = &lane_width;
		 directionality = (DirectionOfUse_t *) calloc(1,sizeof(DirectionOfUse_t));
		 directionality->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 directionality->size = sizeof(uint8_t);
		 directionality->buf[0] = DirectionOfUse_forward;
		 frame_member->commonDirectionality = directionality;
		 */

		/*val_reg = (ValidRegion_t *) calloc(1, sizeof(ValidRegion_t));
		val_reg->direction.buf = (uint16_t *) calloc(1, sizeof(uint16_t));
		val_reg->direction.size = sizeof(uint16_t);
if(wsmgps.course >= 0 && wsmgps.course <= 22.5) {	
			val_reg->direction.buf[0] = 0x40;
			val_reg->direction.buf[1] = 0x02; 	//[315.0,45.0]
		} else if(wsmgps.course > 22.5 && wsmgps.course <= 45.0) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0x03;		//[,45.0]
		} else if(wsmgps.course > 45.0 && wsmgps.course <= 67.5) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0x06;		//[22.5,67.5]
		} else if(wsmgps.course > 67.5 && wsmgps.course <= 90.0) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0x0C;		//[45.0,90.0]
		} else if(wsmgps.course > 90.0 && wsmgps.course <= 112.5) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0x18;		//[67.5,112.5]
		} else if(wsmgps.course > 112.5 && wsmgps.course <= 135.0) {
			val_reg->direction.buf[0] = 0x00;	
			val_reg->direction.buf[1] = 0x30;		//[90.0,135.0]
		} else if(wsmgps.course > 135.0 && wsmgps.course <= 157.5) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0x60;		//[112.5,157.5]
		} else if(wsmgps.course > 157.5 && wsmgps.course <= 180.0) {
			val_reg->direction.buf[0] = 0x00;
			val_reg->direction.buf[1] = 0xC0;		//[135.0,180.0]
		} else if(wsmgps.course > 180.0 && wsmgps.course <= 202.5) {
			val_reg->direction.buf[0] = 0x01;
			val_reg->direction.buf[1] = 0x80;		//[157.5,202.5]
		} else if(wsmgps.course > 202.5 && wsmgps.course <= 225.0) {
			val_reg->direction.buf[0] = 0x03;
			val_reg->direction.buf[1] = 0x00;		//[180.0,225.0]
		} else if(wsmgps.course > 225.0 && wsmgps.course <= 247.5) {
			val_reg->direction.buf[0] = 0x06;
			val_reg->direction.buf[1] = 0x00;		//[202.5,247.5]
		} else if(wsmgps.course > 247.5 && wsmgps.course <= 270.0) {
			val_reg->direction.buf[0] = 0x0C;
			val_reg->direction.buf[1] = 0x00;		//[225.0,270.0]
		} else if(wsmgps.course > 270.0 && wsmgps.course <= 292.5) {
			val_reg->direction.buf[0] = 0x18;
			val_reg->direction.buf[1] = 0x00;		//[247.5,292.5]
		} else if(wsmgps.course > 292.5 && wsmgps.course <= 315.0) {
			val_reg->direction.buf[0] = 0x30;
			val_reg->direction.buf[1] = 0x00;		//[270.0,315.0]
		} else if(wsmgps.course > 315.0 && wsmgps.course <= 337.5) {
			val_reg->direction.buf[0] = 0x60;
			val_reg->direction.buf[1] = 0x00;		//[292.5,337.5]
		} else if(wsmgps.course > 337.5 && wsmgps.course <= 360.0) {
			val_reg->direction.buf[0] = 0xC0;
			val_reg->direction.buf[1] = 0x00;		//[315.0,360.0]
		}*/

		//val_reg->area.present = ValidRegion__area_PR_shapePointSet;
		//val_reg->area.present = ValidRegion__area_PR_circle;
		//offset = (Offsets_t *) calloc(1, sizeof(Offsets_t));
		//offset->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
		//offset->size = sizeof(uint8_t);
		//offset->buf[0] = 0; //xoffset1
		//offset->buf[1] = 1;//yoffset1
		//offset->buf[2] = 2;//xoffset2
		//offset->buf[3] = 3;//yoffset2
		//offset->buf[4] = 4;//xoffset3
		//offset->buf[5] = 5;//yoffset3
		//ASN_SEQUENCE_ADD(&val_reg->area.choice.shapePointSet.nodeList.list,offset);
		/*val_reg->area.choice.circle.center.lat = wsmgps.latitude * BSM_BLOB1_GEO_POS_UNIT_CONV_DEG;
		val_reg->area.choice.circle.center.Long = wsmgps.longitude * BSM_BLOB1_GEO_POS_UNIT_CONV_DEG;
		val_reg->area.choice.circle.raduis.present = Circle__raduis_PR_miles;
		val_reg->area.choice.circle.raduis.choice.miles = 2;
		ASN_SEQUENCE_ADD(&frame_member->regions.list, val_reg);*/

		/*************************part3 Content *******************************************/

		frame_member->content.present = content_PR_advisory;
        
        const char* v2vAlert = "V, 2.0, 0, Warning: V2V-Queue ahead!";
        
		ITIScodeandtext_member = (struct ITIScodesAndText__Member *) calloc(1,
				sizeof(struct ITIScodesAndText__Member));
		ITIScodeandtext_member->item.present = item_PR_text_it;
		ITIScodeandtext_member->item.choice.text.size = strlen(v2vAlert)+1; // Add room for null terminator
		ITIScodeandtext_member->item.choice.text.buf = (uint8_t *) calloc(1,ITIScodeandtext_member->item.choice.text.size);
		memcpy(ITIScodeandtext_member->item.choice.text.buf, v2vAlert, ITIScodeandtext_member->item.choice.text.size);              //
		printf("V2Valert: %s\n",ITIScodeandtext_member->item.choice.text.buf);

		/*ITIScodeandtext_member = (struct ITIScodesAndText__Member *) calloc(7,sizeof(struct ITIScodesAndText__Member));
		 ITIScodeandtext_member[0].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[0].item.choice.itis = 13609; //right hand curve
		 ITIScodeandtext_member[1].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[1].item.choice.itis = 268; //speedlimit code
		 ITIScodeandtext_member[2].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[2].item.choice.itis = 56; //speedlimit value
		 ITIScodeandtext_member[3].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[3].item.choice.itis = 8721; //units in KPH
		 ITIScodeandtext_member[4].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[4].item.choice.itis = 268; //speedlimit code
		 ITIScodeandtext_member[5].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[5].item.choice.itis = 84; //speedlimit value
		 ITIScodeandtext_member[6].item.present = item_PR_itis_it;
		 ITIScodeandtext_member[6].item.choice.itis = 8721; //units in KPH

		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[0]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[1]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[2]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[3]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[4]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[5]);
		 ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,&ITIScodeandtext_member[6]);
		 */
		ASN_SEQUENCE_ADD(&frame_member->content.choice.advisory.list,
				ITIScodeandtext_member);
		ASN_SEQUENCE_ADD(&ti->dataFrames.list, frame_member);
	} //if frametype

	rvalenc = der_encode_to_buffer(&asn_DEF_TravelerInformation, ti,
			&pktData->contents, 2500);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
		//printf("Structure successfully encoded %d\n",rvalenc.encoded);
		pktData->length = rvalenc.encoded;
		printf("Structure successfully encoded %d\n",rvalenc.encoded);

		/*RSU_Flag = checkRSUConnection(Curr_Timer, RSU_Timer, RSU_Flag);
		if(RSU_Flag == 0) {
			sendTIMtoUI(pktData);
		}*/
		
		/* Need to remove qwarnOnRecvMessage call and add back in free_struct
		 * Added in for testing purposes */
		//qwarnOnRecvMessage(ti,WSMMSG_TIM);
		asn_fprint(stdout, &asn_DEF_TravelerInformation, ti);
		asn_DEF_TravelerInformation.free_struct(&asn_DEF_TravelerInformation, ti, 0);
	}
}

void buildBSMPacket(WSMData *pktData) {
	VehicleData vehData;
	vehData_get(&vehData);

	int j, k;
	static int p = 0, indx = 1, iIndx, number = 0;
	uint8_t stw = 0;
	static uint8_t *temp_pathhistory;
	double retDist = 0.0;
	uint16_t intg16, tmpintg16;
	uint32_t intg32;
	DDateTime_t *utcTime;
	Elevation_t *elevation;
	TransmissionAndSpeed_t *speed;
	int indexarr[PH_MAX_POINTS];
	int i;
	static int front = -1, rear = -1;
	//PositionalAccuracy_t *posAccuracy;
	// PositionConfidenceSet_t *posConfidence;
	//SpeedandHeadingandThrottleConfidence_t *speedConfidence;
	union per_point_4 value;
	union per_point_5 per_point_5_val;
	union per_point_2 per_point_2_val;
	union per_point_7 per_point_7_val;
	OCTET_STRING_t *ph_05, *ph_02, *ph_04, *ph_07;
	VehicleStatus_t *sts;
	VehicleSafetyExtension_t *vse;
	//LightbarInUse_t *lbar;
	//WiperStatusRear_t *wsr;
	BrakeSystemStatus_t *bss;
	//BrakeAppliedPressure_t *bap;
	RainSensor_t *rst;
	//SteeringWheelAngleConfidence_t *swac;
	//AccelerationSet4Way_t *accel4way;
	//VerticalAccelerationThreshold_t  *vertAccelThres;
	//YawRateConfidence_t *yawRateCon;
	//AccelerationConfidence_t *hozAccelCon;
	///TimeConfidence_t *timeConfidence;
	//ThrottleConfidence_t *throttleConfidence;
	// ThrottlePosition_t *throttlePos;
	//SpeedConfidence_t *speedC;
	//EssPrecipSituation_t *precipSituation;
	GPSstatus_t *gpsStatus;
	DescriptiveName_t *name;
	VINstring_t *vin;
	//IA5String_t *ownerCode;
	//TemporaryID_t *id;
	VehicleType_t *vehicleType;
	PathHistoryPointType_01_t *first;
	asn_enc_rval_t rvalenc;
	//VehicleState pState_out;
	//qwarnGetCurrentVehicleState(&pState_out);
	BasicSafetyMessage_t *bsm;
	bsm = (BasicSafetyMessage_t *) calloc(1, sizeof(*bsm));

#if (ENABLED)
//  sem_wait(&can_sem);
	bsm->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	bsm->msgID.size = sizeof(uint8_t);
	bsm->msgID.buf[0] = DSRCmsgID_basicSafetyMessage;

	if (ALL == 1) {
		sts = (VehicleStatus_t *) calloc(1, sizeof(VehicleStatus_t));
		bsm->status = sts;
		//vse = (VehicleSafetyExtension_t *) calloc(1, sizeof(VehicleSafetyExtension_t));
	}
#endif
	if (ALL == 1) {
		sem_wait(&can_sem);
		sts->lights = (ExteriorLights_t *) calloc(1, sizeof(ExteriorLights_t));
		sts->lights[0] = vehData.headlight_status; //candata.exteriorLights;
		/*int cnt, mv = 0;
		uint16_t bit = 0x1;
		printf("headlight status: ");
		for(cnt = 0; cnt < 16; cnt++) {
			printf("%u",(sts->lights[0] & (bit << mv) ? 1: 0));
			mv++;
		}
		printf("\n");*/
		sem_post(&can_sem); //the following data has to be updated ;
	}

	if (ALL==1){
	/*
	 lbar = (LightbarInUse_t *) calloc(1,sizeof(LightbarInUse_t));
	 lbar->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 lbar->size = sizeof(uint8_t);
	 lbar->buf[0] = LightbarInUse_unavailable;
	 sts->lightBar = lbar;
*/
	 sts->wipers =(struct VehicleStatus__wipers *) calloc(1,sizeof(struct VehicleStatus__wipers));

	 sts->wipers->statusFront.buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 sts->wipers->statusFront.size = sizeof(uint8_t);
	 sts->wipers->statusFront.buf[0] = vehData.wiper_status;
/*
	 sts->wipers->rateFront = 113;

	 wsr = (WiperStatusRear_t *)calloc(1,sizeof(WiperStatusRear_t));
	 wsr->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 wsr->size = sizeof(uint8_t);
	 wsr->buf[0] = WiperStatusRear_unavailable;
	 sts->wipers->statusRear = wsr;

	 sts->wipers->rateRear = (WiperRate_t *) calloc(1,sizeof(WiperRate_t));
	 sts->wipers->rateRear[0]=112;
	*/
//	qwarnUpdateGpsData(&wsmgps);
//	btUpdateGpsData(&wsmgps);
	
	 bss = ( BrakeSystemStatus_t *) calloc (1,sizeof( BrakeSystemStatus_t));
	 bss->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	unsigned int brake_status = 0x0000;
	bss->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	bss->size = sizeof(uint8_t);
	brake_status = brake_status | (vehData.brake_status << 0); //Brakes Status
	brake_status = brake_status | (vehData.brake_applied << 4);	//Brakes Applied
	//brake_status = brake_status | (qwarnIsQueued() << 5); 			//Queued State
	brake_status = brake_status | (vehData.traction_control_status << 6); 	//Traction Control State
	brake_status = brake_status | (vehData.anti_lock_braking_status << 8);	//Anti Lock Braking Status
	brake_status = brake_status | (vehData.stability_control_status << 10);	//Stability Control Status
	brake_status = brake_status | (vehData.brake_boost_status << 12);		//Brake Boost Status
	brake_status = brake_status | (vehData.auxiliary_brake_status << 14);	//Auxiliary Brake Status
/*
	int cnt, mv = 0;
	uint16_t bit = 0x1;
	printf("brake_status binary: ");
	for(cnt = 0; cnt < 16; cnt++) {
		printf("%u",(brake_status & (bit << mv) ? 1: 0));
		mv++;
	}
	printf("\n");
	*/
	bss->buf[0] = brake_status;
	sts->brakeStatus = bss;
/*
	 bap = (BrakeAppliedPressure_t *) calloc(1,sizeof(BrakeAppliedPressure_t));
	 bap->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 bap->size = sizeof(uint8_t);
	 bap->buf[0] = BrakeAppliedPressure_unavailable;
	 sts->brakePressure = bap;

	 sts->roadFriction = (CoefficientOfFriction_t *) calloc(1,sizeof(CoefficientOfFriction_t));
	 *(sts->roadFriction) = 9;

	 sts->sunData = (SunSensor_t *) calloc(1,sizeof(SunSensor_t));
	 sts->sunData[0] = 8;
*/
	//Storing humidity in RainData
 
	 sts->airTemp = (AmbientAirTemperature_t *) calloc(1,sizeof(AmbientAirTemperature_t));
	if(vehData.external_air_temperature > -1000) {
	 	sts->airTemp[0] = vehData.external_air_temperature;
	} else {
		sts->airTemp[0] = 0;
	}

	 sts->airPres = (AmbientAirPressure_t *) calloc(1,sizeof(AmbientAirPressure_t));
	if(vehData.barometric_pressure > 0) {
		 sts->airPres[0] = vehData.barometric_pressure; 
	} else {
		sts->airPres[0] = 0;
	}


	 sts->steering = (struct VehicleStatus__steering *) calloc(1,sizeof(struct VehicleStatus__steering));
/*
	 sts->steering->angle.buf =  (uint8_t *) calloc(1,sizeof(uint8_t));
	 sts->steering->angle.size = sizeof(uint8_t);
	 sts->steering->angle.buf[0] = 55;

	 swac = (SteeringWheelAngleConfidence_t *) calloc(1,sizeof(SteeringWheelAngleConfidence_t));
	 swac->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 swac->size = sizeof(uint8_t);
	 swac->buf[0] = SteeringWheelAngleConfidence_unavailable;
	 sts->steering->confidence = swac;
*/
	 sts->steering->rate = ( SteeringWheelAngleRateOfChange_t *) calloc(1,sizeof( SteeringWheelAngleRateOfChange_t));
	 sts->steering->rate[0] = vehData.rate_of_change_of_steering_wheel; //115;
/*
	 sts->steering->wheels = ( DrivingWheelAngle_t *) calloc(1,sizeof( DrivingWheelAngle_t));
	 sts->steering->rate[0] = 116;

	 sts->accelSets = (struct VehicleStatus__accelSets *) calloc(1,sizeof(struct VehicleStatus__accelSets));
	 accel4way = ( AccelerationSet4Way_t *) calloc(1,sizeof( AccelerationSet4Way_t));
	 accel4way->buf = (uint8_t *)calloc(1,sizeof(uint8_t));
	 accel4way->size = sizeof(uint8_t);
	 accel4way->buf[0] =55;
	 sts->accelSets->accel4way=accel4way;

	 vertAccelThres = (VerticalAccelerationThreshold_t *) calloc(1,sizeof(VerticalAccelerationThreshold_t));
	 vertAccelThres->buf = (uint8_t *)calloc (1,sizeof(uint8_t));
	 vertAccelThres->size = sizeof(uint8_t);
	 vertAccelThres->buf[0]  = VerticalAccelerationThreshold_allOff;
	 vertAccelThres->bits_unused = 0;
	 sts->accelSets->vertAccelThres = vertAccelThres;

	 yawRateCon = ( YawRateConfidence_t *) calloc(1,sizeof( YawRateConfidence_t));
	 yawRateCon->buf = (uint8_t *)calloc (1,sizeof(uint8_t));
	 yawRateCon->size = sizeof(uint8_t);
	 yawRateCon->buf[0] =  YawRateConfidence_unavailable;
	 sts->accelSets->yawRateCon = yawRateCon;

	 hozAccelCon = (AccelerationConfidence_t *) calloc(1,sizeof(AccelerationConfidence_t));
	 hozAccelCon->buf = (uint8_t *) calloc (1,sizeof(uint8_t));
	 hozAccelCon->size = sizeof(uint8_t);
	 hozAccelCon->buf[0] = AccelerationConfidence_unavailable;
	 sts->accelSets->hozAccelCon = hozAccelCon;

	 sts->accelSets->confidenceSet = (struct ConfidenceSet *) calloc(1,sizeof(struct ConfidenceSet));
	 sts->accelSets->confidenceSet->accelConfidence = (struct AccelSteerYawRateConfidence *)calloc(1,sizeof(struct AccelSteerYawRateConfidence));

	 sts->accelSets->confidenceSet->accelConfidence->yawRate.buf = (uint8_t *) calloc (1,sizeof(uint8_t));;
	 sts->accelSets->confidenceSet->accelConfidence->yawRate.size = sizeof(uint8_t);
	 sts->accelSets->confidenceSet->accelConfidence->yawRate.buf[0] = YawRateConfidence_unavailable;

	 sts->accelSets->confidenceSet->accelConfidence->acceleration.buf = (uint8_t *) calloc (1,sizeof(uint8_t));
	 sts->accelSets->confidenceSet->accelConfidence->acceleration.size = sizeof(uint8_t);
	 sts->accelSets->confidenceSet->accelConfidence->acceleration.buf[0] = AccelerationConfidence_unavailable;

	 sts->accelSets->confidenceSet->accelConfidence->steeringWheelAngle.buf = (uint8_t *) calloc (1,sizeof(uint8_t));
	 sts->accelSets->confidenceSet->accelConfidence->steeringWheelAngle.size = sizeof(uint8_t);
	 sts->accelSets->confidenceSet->accelConfidence->acceleration.buf[0] = SteeringWheelAngleConfidence_unavailable;

	 speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1, sizeof(SpeedandHeadingandThrottleConfidence_t));
	 speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	 speedConfidence->size = sizeof(uint8_t);
	 speedConfidence->buf[0] = 60;
	 sts->accelSets->confidenceSet->speedConfidence = speedConfidence;

	 timeConfidence = (TimeConfidence_t *) calloc(1,sizeof(TimeConfidence_t));
	 timeConfidence->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 timeConfidence->size = sizeof(uint8_t);
	 timeConfidence->buf[0] = TimeConfidence_unavailable;
	 sts->accelSets->confidenceSet->timeConfidence = timeConfidence;

	 posConfidence = (PositionConfidenceSet_t *) calloc(1,sizeof(PositionConfidenceSet_t));
	 posConfidence->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 posConfidence->size = sizeof(uint8_t);
	 posConfidence->buf[0] = TimeConfidence_unavailable;
	 sts->accelSets->confidenceSet->posConfidence = posConfidence;

	 swac = (SteeringWheelAngleConfidence_t *) calloc(1,sizeof(SteeringWheelAngleConfidence_t));
	 swac->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 swac->size = sizeof(uint8_t);
	 swac->buf[0] = SteeringWheelAngleConfidence_unavailable;
	 sts->accelSets->confidenceSet->steerConfidence = swac;

	 throttleConfidence = (ThrottleConfidence_t *) calloc(1,sizeof(ThrottleConfidence_t));
	 throttleConfidence->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
	 throttleConfidence->size = sizeof(uint8_t);
	 throttleConfidence->buf[0] = ThrottleConfidence_unavailable;
	 sts->accelSets->confidenceSet->throttleConfidence = throttleConfidence; */
	}

	/*
	 if(ALL==1)
	 sts->object = (struct VehicleStatus__object *) calloc(1,sizeof(struct VehicleStatus__object));
	 if(ALL==1){
	 sts->object->obDist = 100;
	 sts->object->obDirect = 200;
	 }
	 if(ALL==1) {
	 sts->object->dateTime.year = (DYear_t *) calloc(1, sizeof(DYear_t));
	 sts->object->dateTime.year[0]=year_val;
	 sts->object->dateTime.month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
	 sts->object->dateTime.month[0] = month_val;
	 sts->object->dateTime.day = (DDay_t *) calloc(1, sizeof(DDay_t));
	 sts->object->dateTime.day[0] = day_val;
	 sts->object->dateTime.hour = (DHour_t *) calloc(1, sizeof(DHour_t));
	 sts->object->dateTime.hour[0] =hour_val;
	 sts->object->dateTime.minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
	 sts->object->dateTime.minute[0] = minute_val;
	 sts->object->dateTime.second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
	 *(sts->object->dateTime.second)= sec_16;
	 }*/

	if (ALL == 1) {
		sts->throttlePos = ( ThrottlePosition_t *) calloc(1,sizeof(ThrottlePosition_t));
		 sts->throttlePos[0]= vehData.throttle;
/*
		 speedConfidence = (SpeedandHeadingandThrottleConfidence_t *)calloc(1,sizeof(SpeedandHeadingandThrottleConfidence_t));
		 speedConfidence->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 speedConfidence->size = sizeof(uint8_t);
		 speedConfidence->buf[0] = 110;
		 sts->speedHeadC = speedConfidence;*/

		/*  speedC = (SpeedConfidence_t *) calloc(1,sizeof(SpeedConfidence_t));
		 speedC->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 speedC->size = sizeof(uint8_t);
		 speedC->buf[0] = SpeedConfidence_unavailable;
		 sts->speedC = speedC;*/

		/*  sts->vehicleData = (struct VehicleStatus__vehicleData *) calloc(1,sizeof(struct VehicleStatus__vehicleData));
		 sts->vehicleData->height = 77;
		 sts->vehicleData->bumpers.frnt =76;
		 sts->vehicleData->bumpers.rear = 79;
		 sts->vehicleData->mass = 78;
		 sts->vehicleData->trailerWeight = 73;
		 sts->vehicleData->type.buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 sts->vehicleData->type.size = sizeof(uint8_t);
		 sts->vehicleData->type.buf[0] = VehicleType_none;*/

		sts->vehicleIdent = (VehicleIdent_t *) calloc(1,
				sizeof(VehicleIdent_t)); //roadway name in name field, convert milemarker into string and store in vin field
		
		/*name =( DescriptiveName_t *) calloc(1,sizeof(DescriptiveName_t));
		sts->vehicleIdent->name = name;

		char roadName[10];
		strncpy(roadName, pState_out.roadwayId, 10);
		strtok(roadName, "\n");
		name->size = strlen(roadName);
		name->buf = (uint8_t *) calloc(1,name->size);
		memcpy(name->buf, roadName, name->size);
	

		vin = (VINstring_t *) calloc(1,sizeof(VINstring_t));
		sts->vehicleIdent->vin = vin;
		char mileMarker[10];
		snprintf(mileMarker, 10, "%.4f",pState_out.mileMarker);
		vin->size = strlen(mileMarker);
		vin->buf = (uint8_t *) calloc(1,vin->size);
		memcpy(vin->buf, mileMarker, vin->size);*/
/*
		 ownerCode = (IA5String_t *) calloc(1,sizeof(IA5String_t));
		 ownerCode ->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 ownerCode ->size = sizeof(uint8_t);
		 ownerCode ->buf[0] ='c';
		 sts->vehicleIdent->ownerCode =  ownerCode ; */

		/*  id = (TemporaryID_t *) calloc(1,sizeof(TemporaryID_t));
		 id->buf = (uint8_t *) calloc(1,sizeof(uint8_t));
		 id->size = sizeof(uint8_t);
		 id->buf[0] ='d';
		 sts->vehicleIdent->id =id ;*/

		if (ALL == 1) {
			vehicleType = (VehicleType_t *) calloc(1, sizeof(VehicleType_t));
			vehicleType->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
			vehicleType->size = sizeof(uint8_t);
			vehicleType->buf[0] = vehicle__type;
			sts->vehicleIdent->vehicleType = vehicleType;
		}

		/*  sts->vehicleIdent->vehicleClass = (struct VehicleIdent__vehicleClass *)calloc(1,sizeof(struct VehicleIdent__vehicleClass));
		 sts->vehicleIdent->vehicleClass->present = VehicleIdent__vehicleClass_PR_vGroup;
		 sts->vehicleIdent->vehicleClass->choice.vGroup.buf = (uint8_t *) calloc(1,2*sizeof(uint8_t));
		 sts->vehicleIdent->vehicleClass->choice.vGroup.size = 2*sizeof(uint8_t);
		 sts->vehicleIdent->vehicleClass->choice.vGroup.buf[0] = VehicleGroupAffected_cars>>8;
		 sts->vehicleIdent->vehicleClass->choice.vGroup.buf[1] = (uint8_t)VehicleGroupAffected_cars;

		 sts->weatherReport = (struct VehicleStatus__weatherReport *)calloc(1,sizeof(struct VehicleStatus__weatherReport));
		 sts->weatherReport->isRaining.buf = (uint8_t *)calloc(1,sizeof(uint8_t));
		 sts->weatherReport->isRaining.size = sizeof(uint8_t);
		 sts->weatherReport->isRaining.buf[0]= EssPrecipYesNo_precip;

		 sts->weatherReport->rainRate = (EssPrecipRate_t *)calloc(1,sizeof(EssPrecipRate_t));
		 sts->weatherReport->rainRate[0] = 119;

		 precipSituation = ( EssPrecipSituation_t *)calloc(1,sizeof( EssPrecipSituation_t));
		 precipSituation->buf = (uint8_t *)calloc(1,sizeof(uint8_t));
		 precipSituation->size = sizeof(uint8_t);
		 precipSituation->buf[0]= EssPrecipSituation_other;
		 sts->weatherReport->precipSituation = precipSituation;

		 sts->weatherReport->solarRadiation = (EssSolarRadiation_t *)calloc(1,sizeof(EssSolarRadiation_t));
		 sts->weatherReport->solarRadiation[0] = 118;
		 sts->weatherReport->friction = (EssMobileFriction_t *)calloc(1,sizeof(EssMobileFriction_t));
		 sts->weatherReport->friction[0] = 117;
*/
		 gpsStatus = (GPSstatus_t *) calloc(1,sizeof(GPSstatus_t));
		 gpsStatus->buf = (uint8_t *)calloc(1,sizeof(uint8_t));
		 gpsStatus->size =sizeof(uint8_t);
		
		//Battelle added
		 gpsStatus->buf[0] = wsmgps.fix;
		 
		 gpsStatus->bits_unused =0;
		 sts->gpsStatus = gpsStatus;
	}
		
#if(0)
	if (ALL == 1) {
		vse->pathPrediction = (struct PathPrediction *) calloc(1,
				sizeof(struct PathPrediction));
		if (wsmgps.course != GPS_INVALID_DATA
				&& wsmgps.speed != GPS_INVALID_DATA) {
			vse->pathPrediction->radiusOfCurve = pp_roc;
			vse->pathPrediction->confidence = pp_confidence;
		} else {
			vse->pathPrediction->radiusOfCurve = 0;
			vse->pathPrediction->confidence = 0;
		}
	}

	if (hb_event_flag
			&& (wsmgps.latitude != GPS_INVALID_DATA
					&& wsmgps.longitude != GPS_INVALID_DATA)) {
		if (((num != 0) && (packets != 0) && (((packets + 1) % num) == 0))) {
			vse->pathHistory = (struct PathHistory *) calloc(1,
					sizeof(struct PathHistory));
//  first = (PathHistoryPointType_01_t *) calloc(1,sizeof(PathHistoryPointType_01_t));
			path_num = 4;
		}
	} else if (ALL == 1
			&& (wsmgps.latitude != GPS_INVALID_DATA
					&& wsmgps.longitude != GPS_INVALID_DATA)) {
		vse->pathHistory = (struct PathHistory *) calloc(1,
				sizeof(struct PathHistory));
//  first = (PathHistoryPointType_01_t *) calloc(1,sizeof(PathHistoryPointType_01_t));       
	}
	if (ALL == 1) {
//  vse->events = (EventFlags_t *) calloc(1,sizeof(EventFlags_t));  
		bsm->safetyExt = vse;
	}
//if(ALL==1)  
//  vse->events[0]=110;
	if (ALL == 1) {		
		  /*utcTime = (DDateTime_t *) calloc(1, sizeof(DDateTime_t));
		 vse->pathHistory->initialPosition->utcTime = utcTime;
		 utcTime->year = (DYear_t *) calloc(1, sizeof(DYear_t));
		 utcTime->month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
		 utcTime->day = (DDay_t *) calloc(1, sizeof(DDay_t));
		 utcTime->hour = (DHour_t *) calloc(1, sizeof(DHour_t));
		 utcTime->minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
		 utcTime->second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
		 utcTime->year[0] = year_val;
		 utcTime->month[0] = month_val;
		 utcTime->day[0] = day_val;
		 utcTime->hour[0] = hour_val;
		 utcTime->minute[0] = minute_val;
		 memcpy(utcTime->second,(char*)&sec_16,2);
		 vse->pathHistory->initialPosition->Long = longitude_val;
		 vse->pathHistory->initialPosition->lat = latitude_val;
		 elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
		 elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
		 elevation->size = 2 * sizeof(uint8_t);
		 elevation->buf[0] = elevation_val[0];
		 elevation->buf[1] = elevation_val[1];
		 vse->pathHistory->initialPosition->elevation = elevation;
		 vse->pathHistory->initialPosition->heading = (Heading_t *) calloc(1, sizeof(Heading_t));
		 *(vse->pathHistory->initialPosition->heading) = heading_val;
		 speed = (TransmissionAndSpeed_t *) calloc(1, sizeof(TransmissionAndSpeed_t));
		 speed->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
		 speed->size = 2 * sizeof(uint8_t);
		 speed->buf[0] = transmission_speed[0];
		 speed->buf[1] = transmission_speed[1];
		 vse->pathHistory->initialPosition->speed = speed; */
	}
	if (ALL == 1) {
		//posAccuracy = (PositionalAccuracy_t *) calloc(1, sizeof(PositionalAccuracy_t));
		//posAccuracy->buf = (uint8_t *) calloc(4, sizeof(uint8_t));
		//posAccuracy->size = 4 * sizeof(uint8_t);
		/*  posAccuracy->buf[0] = 0;
		 posAccuracy->buf[1] = 0;
		 posAccuracy->buf[2] = 0;
		 posAccuracy->buf[3] = 0;*/
		// vse->pathHistory->initialPosition->posAccuracy = posAccuracy;
		/*  posConfidence = (PositionConfidenceSet_t *) calloc(1, sizeof(PositionConfidenceSet_t));
		 posConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
		 posConfidence->size = sizeof(uint8_t);
		 posConfidence->buf[0] = 0;
		 vse->pathHistory->initialPosition->posConfidence = posConfidence;

		 speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1, sizeof(SpeedandHeadingandThrottleConfidence_t));
		 speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
		 speedConfidence->size = sizeof(uint8_t);
		 speedConfidence->buf[0] = 0;
		 vse->pathHistory->initialPosition->speedConfidence = speedConfidence;

		 gpsStatus = (GPSstatus_t *) calloc(1,sizeof(GPSstatus_t));
		 gpsStatus->buf = (uint8_t *)calloc(1,sizeof(uint8_t));
		 gpsStatus->size =sizeof(uint8_t);
		 gpsStatus->buf[0] = GPSstatus_unavailable;
		 gpsStatus->bits_unused =0;
		 vse->pathHistory->currGPSstatus = gpsStatus;*/

	}

	if (ALL == 1) {
		if (hb_event_flag == 1) {
			vse->events = (EventFlags_t *) calloc(1, sizeof(EventFlags_t));
			vse->events[0] = 0x80; //hardbraking
		}
	}

	switch (path_num) {
	case 1:
		vse->pathHistory->crumbData.present =
				PathHistory__crumbData_PR_pathHistoryPointSets_01;
		if (ALL == 1) {
			first->latOffset = 0;
			first->longOffset = 0;
			first->elevationOffset = (long *) calloc(1, sizeof(long));
			first->elevationOffset[0] = 99;
			first->timeOffset = (long *) calloc(1, sizeof(long));
			first->timeOffset[0] = 97;
			first->posAccuracy = (PositionalAccuracy_t *) calloc(1,
					sizeof(PositionalAccuracy_t));
			first->posAccuracy->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
			first->posAccuracy->size = sizeof(uint8_t);
			first->posAccuracy->buf[0] = 38;
			first->heading = (long *) calloc(1, sizeof(long));
				first->heading[0] = heading_val;
			first->speed = (TransmissionAndSpeed_t *) calloc(1,
					sizeof(TransmissionAndSpeed_t));
			first->speed->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
			first->speed->size = sizeof(uint8_t);
			first->speed->buf[0] = vehData.speed;

			ASN_SEQUENCE_ADD(
					&vse->pathHistory->crumbData.choice.pathHistoryPointSets_01.list,
					first);
		}
		break;
	case 2:
		if (ALL == 1) {
			vse->pathHistory->crumbData.present =
					PathHistory__crumbData_PR_pathHistoryPointSets_02;
			int k = 0;
			if ((wsmgps.latitude != GPS_INVALID_DATA)
					&& (wsmgps.longitude != GPS_INVALID_DATA)) {
				calculation_of_offsets();
				if ((ph_lat1 == 0) && (ph_lon1 == 0)) {
					ph_02 = (OCTET_STRING_t *) calloc(1,
							sizeof(OCTET_STRING_t));
					ph_02->buf = (uint8_t *) calloc(1, 345 * sizeof(uint8_t));
					ph_02->size = 345 * sizeof(uint8_t);
					temp_pathhistory = (uint8_t *) calloc(1,
							345 * sizeof(uint8_t));
					ph_lat1 = wsmgps.latitude;
					ph_lon1 = wsmgps.longitude;
					per_point_2_val.lat_long_elev.lat_Offset = latitudeoffset;
					per_point_2_val.lat_long_elev.long_Offset = longitudeoffset;
					per_point_2_val.lat_long_elev.elev_Offset = elevationoffset;
					per_point_2_val.lat_long_elev.time_Offset = timeoffset;
//			memcpy(ph_02->buf+p,&per_point_2_val,15);
					memcpy(temp_pathhistory + p, &per_point_2_val, 15);
					p = p + 15;
				} else {
					retDist = distance_cal(ph_lat1, ph_lon1, wsmgps.latitude,
							wsmgps.longitude, wsmgps.altitude);
					//retDist = distance_cal(ph_lat1, ph_lon1, gps[idx].lat, gps[idx].lon);
					if (retDist >= (300.0 / 23.0)) {
						per_point_2_val.lat_long_elev.lat_Offset =
								latitudeoffset;
						per_point_2_val.lat_long_elev.long_Offset =
								longitudeoffset;
						per_point_2_val.lat_long_elev.elev_Offset =
								elevationoffset;
						per_point_2_val.lat_long_elev.time_Offset = timeoffset;
//			  memcpy(ph_02->buf+p,&per_point_2_val,15);
						memcpy(temp_pathhistory + p, &per_point_2_val, 15);
						p = p + 15;
						distance = distance + retDist;
						retDist = 0;
						ph_lat1 = wsmgps.latitude;
						ph_lon1 = wsmgps.longitude;
					}
				}
				//idx++;
				if (distance >= 300.0) {
					for (k = 0; k <= 345; k++) {
						p = p - 15;
						memcpy(ph_02->buf + k, temp_pathhistory + p, 15);
						k = k + 15;
					}
					vse->pathHistory->crumbData.choice.pathHistoryPointSets_02 =
							*ph_02;
					free(temp_pathhistory);
					ph_lat1 = 0;
					ph_lon1 = 0;
					p = 0;
					distance = 0;
				}
			}

			// }
		}

		break;

	case 4:
		if (ALL == 1) {

			if (wsmgps.latitude != GPS_INVALID_DATA
					&& wsmgps.longitude != GPS_INVALID_DATA) {
				vse->pathHistory->crumbData.present =
						PathHistory__crumbData_PR_pathHistoryPointSets_04;
				vse->pathHistory->itemCnt = (Count_t *) calloc(1,
						sizeof(Count_t));
				vse->pathHistory->itemCnt[0] = 0;

				if (indx == 1) { //indx starts from 1 defined as indx=1
					Prev_lat = Dp_start.lat = wsmgps.latitude;
					Prev_lon = Dp_start.lon = wsmgps.longitude;
					Dp_start.elev = wsmgps.altitude;
					Dp_start.time = wsmgps.actual_time;
					Dp_start.distPrev = 0.0;
					Dp_start.course = wsmgps.course;
					cbWrite(&cb, (struct gps_datapoint_php4 *) &Dp_start); // to add pathhistory points to buffer
																		   // give a write request to pathhistory buffer
					position++;
					fnConvertEllipsoidalToECEF(wsmgps.latitude,
							wsmgps.longitude, wsmgps.altitude, &Start);
					//	printf("\nStart lat,lon,elev,time= %lf,%lf,%lf,%lf\n",wsmgps.latitude,wsmgps.longitude,wsmgps.altitude,wsmgps.actual_time);
				}

				if (indx > 1
						&& ((Prev_lat != wsmgps.latitude
								|| Prev_lon != wsmgps.longitude)
								&& wsmgps.speed >= SPEED_LATCH)) { //second time onwards

					rear = (rear + 1) % INTER_SIZE;
					fnConvertEllipsoidalToECEF(wsmgps.latitude,
							wsmgps.longitude, wsmgps.altitude, &LLbuffer[rear]);
					/*to compute where on array to save the x,y coordinates, this is an circular queue arrangement to maintain LLbuffer array*/
					/* LLbuffer is an array which contains x,y co-ordinates of a point and the altitude value at the same point for future computation*/
					if (rear == front) { //if rear crosses front , move front ahead
						front = (front + 1) % INTER_SIZE;
					}
					if (front == (-1)) { //for first data make front =0
						front = 0;
					}

					if (indx > 2) {
						/****Now Check Dp_prev points are path history points ?? *****/

//calculate chord length between start point and cur point co-ordinates
						Ph_chord_length =
								sqrt(((LLbuffer[rear].X - Start.X)*(LLbuffer[rear].X - Start.X)) + ((LLbuffer[rear].Y - Start.Y)*(LLbuffer[rear].Y - Start.Y)) + ((LLbuffer[rear].Z - Start.Z)*(LLbuffer[rear].Z - Start.Z)));

						if (Ph_chord_length >= PH_CHORD_LENGTH_THRESHOLD) { //310 meters
							Cross_track_error = PH_ALLOWABLE_ERROR + 1; //1+1 metre
						}
						/* for almost straight path if it crosses more than PH_CHORD_LENGTH_THRESHOLD then consider an pathhistory point and reset the computation */
						else {
							//compute cross track error to find out pathhistory points
							for (i = front; i != rear;
									i = (i + 1) % INTER_SIZE) {
								Cross_track_error =
										fnComputeDistanceFromPointToALine(
												&Start, &LLbuffer[i],
												&LLbuffer[rear],
												Ph_chord_length);
								if (Cross_track_error >= PH_ALLOWABLE_ERROR) {
									break;
								}
							}
						}

						if (Cross_track_error >= PH_ALLOWABLE_ERROR) { //condition successfull
							fnConvertEllipsoidalToECEF(Dp_prev.lat, Dp_prev.lon,
									Dp_prev.elev, &Start);
							front = 0;
							rear = 0;
							cbPeek(&cb, &position); //helps to get recent ph point in buffer

							fnConvertEllipsoidalToECEF(Ph_pnt[position].lat,
									Ph_pnt[position].lon, Ph_pnt[position].elev,
									&LLbuffer[rear]);
							/*calculate distance betwn recent 2 pathhistory points in ph buffer*/
							retDist =
									sqrt(((LLbuffer[rear].X - Start.X)*(LLbuffer[rear].X - Start.X)) + ((LLbuffer[rear].Y - Start.Y)*(LLbuffer[rear].Y - Start.Y)) + ((LLbuffer[rear].Z - Start.Z)*(LLbuffer[rear].Z - Start.Z))); //distance between recent 2 ph points

							Dp_prev.distPrev = retDist; // each point will hold distance from
							memcpy(&Dp_start, &Dp_prev, sizeof(Dp_prev));
							//write request to circular buffer
							cbWrite(&cb,
									(struct gps_datapoint_php4 *) &Dp_prev);
							fnConvertEllipsoidalToECEF(wsmgps.latitude,
									wsmgps.longitude, wsmgps.altitude,
									&LLbuffer[rear]);
						}
					}
// move on by making current point as previous points
					Prev_lat = Dp_prev.lat = wsmgps.latitude; //for both cases i.e. cte >< 1 mtr
					Prev_lon = Dp_prev.lon = wsmgps.longitude;
					Dp_prev.elev = wsmgps.altitude;
					Dp_prev.time = wsmgps.actual_time;
					Dp_prev.course = wsmgps.course;
				}
				//below part every time
				/* getindexes will compute addition of distances that were saved eariler in pathhistory circular buffer between recent two ph points and select no. of points to transmit (with either condition distance >=300  or 23 pathhistory points)  */
				getindexes(&cb, (double) PH_MAX_DISTANCE, indexarr, &number); // to compute how many points to send
				if (number != 0) {
					vse->pathHistory->itemCnt[0] = number; // no. of points given by getindexes. that we will encoded in current BSM packet
					ph_04 =
							&vse->pathHistory->crumbData.choice.pathHistoryPointSets_04;
					temp_pathhistory = (uint8_t *) calloc(1,
							number * PH_PER_POINT_SIZE * sizeof(uint8_t)); // directly fill in ph_04
					ph_04->buf = temp_pathhistory;
					ph_04->size = number * PH_PER_POINT_SIZE * sizeof(uint8_t);
					for (iIndx = 0; iIndx < number; iIndx++) {
						offset_cal_php4(indexarr[iIndx]);
						value.lat_long_elev.lat_Offset = latitudeoffset;
						value.lat_long_elev.long_Offset = longitudeoffset;
						value.lat_long_elev.elev_Offset = elevationoffset;
						value.lat_long_elev.time_Offset = timeoffset;
						memcpy(temp_pathhistory + (iIndx * PH_PER_POINT_SIZE),
								&value, PH_PER_POINT_SIZE); //copy to ph_04->buf as it is equal to temp_pathhistory
					} // to encode as offset as per BSM-SAE message set dictionary
					retDist = 0;
				}

				if (indx < 3)
					indx++;
			}
		}
		// Compute pp roc from recent PH point
		if (wsmgps.speed != GPS_INVALID_DATA
				&& wsmgps.course != GPS_INVALID_DATA) {
			cbPeek(&cb, &position);
			pp_roc = ((wsmgps.speed
					/ ((wsmgps.course - Ph_pnt[position].course) * (pidiv180)))
					* 10);
			pp_yawrate = fabs(wsmgps.course - Ph_pnt[position].course); //cur-prev
			pp_confidence = confidence_lookup(pp_yawrate) * 2; //lsb units of 0.5%
			if (pp_confidence == 1) //if lookup returns w/o finding valid confidence value
				pp_confidence = INVALID_YAW_RATE;
			if (pp_roc > 32767 || wsmgps.speed < 0.1389) { //speed < 0.5 kmph
				if (wsmgps.speed < 0.138)
					pp_confidence = 100; //for stationary entity
				pp_roc = 32767;
			} else if (pp_roc < -32767)
				pp_roc = -32767;
		}

		break;

	case 5:
		if (ALL == 1) {
			vse->pathHistory->crumbData.present =
					PathHistory__crumbData_PR_pathHistoryPointSets_05;

			if ((wsmgps.latitude = !GPS_INVALID_DATA) && (wsmgps.longitude =
					!GPS_INVALID_DATA)) {
				calculation_of_offsets();
				if ((ph_lat1 == 0) && (ph_lon1 == 0)) {
					temp_pathhistory = (uint8_t *) calloc(1,
							230 * sizeof(uint8_t));
					per_point_5_val.lat_long_elev.lat_Offset = latitudeoffset;
					per_point_5_val.lat_long_elev.long_Offset = longitudeoffset;
					per_point_5_val.lat_long_elev.elev_Offset = elevationoffset;
					ph_lat1 = wsmgps.latitude;
					ph_lon1 = wsmgps.longitude;
					memcpy(temp_pathhistory + p, &per_point_5_val, 10);
					p = p + 10;

				} else {
					per_point_5_val.lat_long_elev.lat_Offset = latitudeoffset;
					per_point_5_val.lat_long_elev.long_Offset = longitudeoffset;
					per_point_5_val.lat_long_elev.elev_Offset = elevationoffset;
					memcpy(temp_pathhistory + p, &per_point_5_val, 10);
					p = p + 10;
					ph_lat1 = wsmgps.latitude;
					ph_lon1 = wsmgps.longitude;
				}
//                }

				if (p == 230) {
					ph_05 = (OCTET_STRING_t *) calloc(1,
							sizeof(OCTET_STRING_t));
					ph_05->buf = (uint8_t *) calloc(1, 230 * sizeof(uint8_t));
					ph_05->size = 230 * sizeof(uint8_t);
					for (k = 0; k < 230; k += 10) {
						p = p - 10;
						memcpy(ph_05->buf + k, temp_pathhistory + p, 10);
					}
					vse->pathHistory->crumbData.choice.pathHistoryPointSets_05 =
							*ph_05;
					free(temp_pathhistory);
					ph_lat1 = 0;
					ph_lon1 = 0;

				}

			}
			/*  ph_05 = (OCTET_STRING_t *)calloc(1,sizeof(OCTET_STRING_t));
			 ph_05->buf = (uint8_t *)calloc(1,230*sizeof(uint8_t));
			 ph_05->size = 230*sizeof(uint8_t);
			 vse->pathHistory->crumbData.choice.pathHistoryPointSets_05 = *ph_05;
			 for(k=0;k<=22;k++) {
			 calculation_of_offsets();
			 per_point_5_val.lat_long_elev.lat_Offset = latitudeoffset;
			 per_point_5_val.lat_long_elev.long_Offset= longitudeoffset;
			 per_point_5_val.lat_long_elev.elev_Offset =elevationoffset;

			 memcpy(vse->pathHistory->crumbData.choice.pathHistoryPointSets_05.buf+p,&per_point_5_val,10);
			 p=p+10;
			 }*/
		}
		break;
	case 7:
		if (ALL == 1)
			vse->pathHistory->crumbData.present =
					PathHistory__crumbData_PR_pathHistoryPointSets_07;
		if (ALL == 1) {
			p = 0;
			ph_07 = (OCTET_STRING_t *) calloc(1, sizeof(OCTET_STRING_t));
			ph_07->buf = (uint8_t *) calloc(1, 253 * sizeof(uint8_t));
			ph_07->size = 253 * sizeof(uint8_t);
			vse->pathHistory->crumbData.choice.pathHistoryPointSets_07 = *ph_07;
			for (k = 0; k <= 22; k++) {
				calculation_of_offsets();
				per_point_7_val.lat_long.lat_Offset = latitudeoffset;
				per_point_7_val.lat_long.long_Offset = longitudeoffset;
//          per_point_7_val.lat_long.time_Offset= timeoffset;
				memcpy(
						vse->pathHistory->crumbData.choice.pathHistoryPointSets_07.buf
								+ p, &per_point_7_val, 11);
				p = p + 11;
			}
		}
		break;

	}
	/*
	 if(ALL ==1) {
	 vse->pathPrediction = (struct PathPrediction *)calloc(1,sizeof(PathPrediction_t));
	 vse->pathPrediction->radiusOfCurve = 10;
	 vse->pathPrediction->confidence = 11;
	 }*/
#endif
if(ALL==1){
	//if (pos_vec > 0) {
		

		time_t current_time;
		time(&current_time);

		struct tm *msg_time = gmtime(&current_time);

		sts->fullPos = (FullPositionVector_t *) calloc(1,
				sizeof(FullPositionVector_t)); //commented to include pathhistorypoint set-4
		utcTime = (DDateTime_t *) calloc(1, sizeof(DDateTime_t));
		bsm->status->fullPos->utcTime = utcTime;
		utcTime->year = (DYear_t *) calloc(1, sizeof(DYear_t));
		utcTime->month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
		utcTime->day = (DDay_t *) calloc(1, sizeof(DDay_t));
		utcTime->hour = (DHour_t *) calloc(1, sizeof(DHour_t));
		utcTime->minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
		utcTime->second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
		utcTime->year[0] = msg_time->tm_year;//year_val;
		utcTime->month[0] = msg_time->tm_mon;//month_val;
		utcTime->day[0] = msg_time->tm_mday;//day_val;
		utcTime->hour[0] = msg_time->tm_hour;//hour_val;
		utcTime->minute[0] = msg_time->tm_min;//minute_val;
//  *(utcTime->second) = sec_16;
		*(utcTime->second) = msg_time->tm_sec;// 0; //Need to resolve 3 byte occupancy after 32767 value
		bsm->status->fullPos->Long = longitude_val;
		bsm->status->fullPos->lat = latitude_val;
	/*	elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
		elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
		elevation->size = 2 * sizeof(uint8_t);
		elevation->buf[0] = elevation_val[0];
		elevation->buf[1] = elevation_val[1];

		bsm->status->fullPos->elevation = elevation;
	*/
		bsm->status->fullPos->heading = (Heading_t *) calloc(1,
				sizeof(Heading_t));
		*(bsm->status->fullPos->heading) = heading_val;
		speed = (TransmissionAndSpeed_t *) calloc(1,
				sizeof(TransmissionAndSpeed_t));
		speed->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
		speed->size = 2 * sizeof(uint8_t);
		speed->buf[0] = transmission_speed[0];
		speed->buf[1] = transmission_speed[1];
		//printf("transmission speeds 0,1: %X, %X\n",transmission_speed[0],transmission_speed[1]);
		bsm->status->fullPos->speed = speed;
		free(msg_time);
	}

//}
	/*
	 if(ALL==1){
	 posAccuracy = (PositionalAccuracy_t *) calloc(1, sizeof(PositionalAccuracy_t));
	 posAccuracy->buf = (uint8_t *) calloc(4, sizeof(uint8_t));
	 posAccuracy->size = 4 * sizeof(uint8_t);
	 posAccuracy->buf[0] = 0;
	 posAccuracy->buf[1] = 0;
	 posAccuracy->buf[2] = 0;
	 posAccuracy->buf[3] = 0;
	 bsm->status->fullPos->posAccuracy = posAccuracy;
	 posConfidence = (PositionConfidenceSet_t *) calloc(1, sizeof(PositionConfidenceSet_t));
	 posConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	 posConfidence->size = sizeof(uint8_t);
	 posConfidence->buf[0] = 0;
	 bsm->status->fullPos->posConfidence = posConfidence;
	 speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1, sizeof(SpeedandHeadingandThrottleConfidence_t));
	 speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	 speedConfidence->size = sizeof(uint8_t);
	 speedConfidence->buf[0] = 0;
	 bsm->status->fullPos->speedConfidence = speedConfidence;
	 }*/

//#if(ENABLED)
	bsm->blob1.buf = (uint8_t *) calloc(1, 38 * sizeof(uint8_t));
	bsm->blob1.size = 38 * sizeof(uint8_t);
	bsm->blob1.buf[0] = msgCnt % 128;
	msgCnt++;
	intg32 = htobe32(*((uint32_t *)(temp_id)));
	memcpy(bsm->blob1.buf + 1, &intg32, 4);
	intg16 = htobe16(sec_16);
	memcpy(bsm->blob1.buf + 5, &intg16, 2);
	j = latitude_val;
	intg32 = htobe32(j);
	memcpy(bsm->blob1.buf + 7, &intg32, 4);
	j = longitude_val;
	intg32 = htobe32(j);
	memcpy(bsm->blob1.buf + 11, &intg32, 4);

	memcpy(bsm->blob1.buf + 15, elevation_val, 2);
	j = 0xFFFFFFFF; // default value of positional_Accuracy
	memcpy(bsm->blob1.buf + 17, &j, 4);
	//printf("transmission speeds 0,1: %d, %d\n",transmission_speed[0],transmission_speed[1]);
	memcpy(bsm->blob1.buf + 21, transmission_speed, 2);


	intg16 = htobe16((uint16_t)(wsmgps_heading * 80));
	memcpy(bsm->blob1.buf + 23, &intg16, 2);


	stw = 127; //default value for Steering Wheel Angle
	memcpy(bsm->blob1.buf + 25, &stw, 1);
	if (wsmgps.speed == GPS_INVALID_DATA)
		lon_acc_accl = 2001; //default value for longitude accleration
	if(vehData.longitudinal_acceleration != -1) {
		sintg16 = vehData.longitudinal_acceleration;
	} else {
		sintg16 = lon_acc_accl;
	}
	sintg16 = htobe16(sintg16);
	memcpy(bsm->blob1.buf + 26, &sintg16, 2); //longi acc	
	if(vehData.lateral_acceleration != -1) {
		sintg16 = vehData.lateral_acceleration;
	} else {
		sintg16 = 2001;
	}
	sintg16 = htobe16(sintg16);
	//tmpintg16 = vehData.lateral_acceleration;//tmpintg16 = 2001; //default value for latitude accleration
	//intg16 = htobe16(tmpintg16);
	memcpy(bsm->blob1.buf + 28, &sintg16, 2);
	stw = -127; //default value for vertical accleration
	memcpy(bsm->blob1.buf + 30, &stw, 1);
	if(vehData.yaw_rate != -32768) {
		yaw_rate = vehData.yaw_rate;
	} else {
		if (wsmgps.course == GPS_INVALID_DATA || heading_val == 28800) {
			yaw_rate = 32767; //default value for yaw accleration
		}
	}
	sintg16 = yaw_rate;
	sintg16 = htobe16(sintg16);
	memcpy(bsm->blob1.buf + 31, &sintg16, 2); //yaw
	tmpintg16 = 0x0000;
	tmpintg16 = tmpintg16 | vehData.brake_status; //Brakes Status
	tmpintg16 = tmpintg16 | (vehData.brake_applied << 4);	//Brakes Applied
	//tmpintg16 = tmpintg16 | (qwarnIsQueued() << 5); 			//Queued State
	tmpintg16 = tmpintg16 | (vehData.traction_control_status << 6); 	//Traction Control State
	tmpintg16 = tmpintg16 | (vehData.anti_lock_braking_status << 8);	//Anti Lock Braking Status
	tmpintg16 = tmpintg16 | (vehData.stability_control_status << 10);	//Stability Control Status
	tmpintg16 = tmpintg16 | (vehData.brake_boost_status << 12);		//Brake Boost Status
	tmpintg16 = tmpintg16 | (vehData.auxiliary_brake_status << 14);	//Auxiliary Brake Status
/*	int cnt, mv = 0;
	unsigned int bit = 0x1;
	printf("brake_status binary: ");
	for(cnt = 0; cnt < 16; cnt++) {
		printf("%u",(tmpintg16 & (bit >> mv) ? 1: 0));
		mv++;
	}
	printf("\n"); */
	intg16 = htobe16(tmpintg16);
	memcpy(bsm->blob1.buf + 33, &intg16, 2);

	memcpy(bsm->blob1.buf + 35, vsize, 3);
	if(ALL==1) {
		//asn_fprint(stdout, &asn_DEF_BasicSafetyMessage, bsm);
	}
	rvalenc = der_encode_to_buffer(&asn_DEF_BasicSafetyMessage, bsm,
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
		//    printf("Structure successfully encoded %d\n", rvalenc.encoded);
		pktData->length = rvalenc.encoded;
	}
	/* 
	 * Battelle defined
	 * Send BSM messages to smartphone
	 */
	/*RSU_Flag = checkRSUConnection(Curr_Timer, RSU_Timer, RSU_Flag);
	if(ALL==1 && RSU_Flag == 0) {
		sendBSMtoUI(pktData);
	}*/

	asn_DEF_BasicSafetyMessage.free_struct(&asn_DEF_BasicSafetyMessage, bsm, 0);
}

int calculation_of_offsets() {
//        static int i=0;
	//unsigned char lat_buf[4];
	//unsigned char lat1_tmp[0],lat2_tmp[0];
	int lat = (int) ((wsmgps.latitude) * 10000000);
	int longi = (int) ((wsmgps.longitude) * 10000000);
	int eleva = (int) ((wsmgps.altitude) * 10);
	unsigned char *p, *q;
	int i = 0;
	uint8_t res;
	if (lat != 0) {
		p = (uint8_t*) &lat;
		q = (uint8_t *) &lat;
		if (BIGENDIAN) {
			swap32(lat);
			lat = lat & 0x800001FF;
		} else {
			lat = lat & 0x800001FF;
		}
		for (i = 0; i < 4; i++)
			p++;
		for (i = 0; i < 3; i++)
			q++;
		res = *p;
		res = res >> 6;
		res = res & 0x02;
		(*q) = (*q) | res;
		lat = lat & 0x000003FF;
		//  free(p);free(q);
	}
	if (longi != 0) {
		p = (uint8_t*) &longi;
		q = (uint8_t *) &longi;
		if (BIGENDIAN) {
			swap32(longi);
			longi = longi & 0x800001FF;
		} else {
			longi = longi & 0x800001FF;
		}
		for (i = 0; i < 4; i++)
			p++;
		for (i = 0; i < 3; i++)
			q++;
		res = *p;
		res = res >> 6;
		res = res & 0x02;
		(*q) = (*q) | res;
		longi = longi & 0x000003FF;
		//free(p);free(q);

	}

	if (eleva != 0) {
		p = (uint8_t*) &eleva;
		if (BIGENDIAN) {
			swap16(eleva);
			eleva = eleva & 0x87FF;
		} else {
			eleva = eleva & 0x87FF;
		}
		res = *p;
		res = res >> 3;
		res = res & 0x08;
		(*p) = (*p) | res;
		eleva = eleva & 0x87FF;
	}
	latitudeoffset = lat;
	if (latitudeoffset >= 131071)
		latitudeoffset = 131071;
	else if (latitudeoffset <= -131071)
		latitudeoffset = 131071;
	else if (lat == GPS_INVALID_DATA)
		latitudeoffset = -131072;
	longitudeoffset = longi;
	if (longitudeoffset >= 131071)
		longitudeoffset = 131071;
	else if (longitudeoffset <= -131071)
		longitudeoffset = 131071;
	else if (longi == GPS_INVALID_DATA)
		longitudeoffset = -131072;

	elevationoffset = eleva;
	if (elevationoffset >= 2047)
		elevationoffset = 2047;
	else if (elevationoffset <= -2047)
		elevationoffset = -2047;
	else if (eleva == GPS_INVALID_DATA)
		elevationoffset = -2048;

	timeoffset = (((unsigned int) wsmgps.time) % 100) * 1000;
	if (timeoffset >= 65534)
		timeoffset = 65534;
	else if (timeoffset == 0)
		timeoffset = 65535;
	return 0;
}

void buildPVDPacket(WSMData *pktData) {
	static int pktnum = 0;
	asn_enc_rval_t rvalenc;
	ProbeVehicleData_t *pvd;
	DDateTime_t *utcTime;
	Elevation_t *elevation;
//  uint16_t intg16;
//  uint32_t intg32;
	TransmissionAndSpeed_t *speed;
	PositionalAccuracy_t *posAccuracy;
	PositionConfidenceSet_t *posConfidence;
	SpeedandHeadingandThrottleConfidence_t *speedConfidence;
	Snapshot_t *snapshot;
	VehicleSafetyExtension_t *safetyExt;

	pvd = (ProbeVehicleData_t *) calloc(1, sizeof(ProbeVehicleData_t));
	pvd->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	pvd->msgID.size = sizeof(uint8_t);
	pvd->msgID.buf[0] = DSRCmsgID_probeVehicleData;
	pvd->startVector.Long = longitude_val;
	pvd->startVector.lat = latitude_val;
	utcTime = (DDateTime_t *) calloc(1, sizeof(DDateTime_t));
	pvd->startVector.utcTime = utcTime;
	utcTime->year = (DYear_t *) calloc(1, sizeof(DYear_t));
	utcTime->month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
	utcTime->day = (DDay_t *) calloc(1, sizeof(DDay_t));
	utcTime->hour = (DHour_t *) calloc(1, sizeof(DHour_t));
	utcTime->minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
	utcTime->second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
	utcTime->year[0] = year_val;
	utcTime->month[0] = month_val;
	utcTime->day[0] = day_val;
	utcTime->hour[0] = hour_val;
	utcTime->minute[0] = minute_val;
	*(utcTime->second) = sec_16;
	elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
	elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	elevation->size = 2 * sizeof(uint8_t);
	elevation->buf[0] = elevation_val[0];
	elevation->buf[1] = elevation_val[1];

	pvd->startVector.elevation = elevation;
	pvd->startVector.heading = (Heading_t *) calloc(1, sizeof(Heading_t));
	*(pvd->startVector.heading) = heading_val;
	speed = (TransmissionAndSpeed_t *) calloc(1,
			sizeof(TransmissionAndSpeed_t));
	speed->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	speed->size = 2 * sizeof(uint8_t);
	speed->buf[0] = transmission_speed[0];
	speed->buf[1] = transmission_speed[1];
	pvd->startVector.speed = speed;
	posAccuracy = (PositionalAccuracy_t *) calloc(1,
			sizeof(PositionalAccuracy_t));
	posAccuracy->buf = (uint8_t *) calloc(4, sizeof(uint8_t));
	posAccuracy->size = 4 * sizeof(uint8_t);
	posAccuracy->buf[0] = 0xFF;
	posAccuracy->buf[1] = 0xFF;
	posAccuracy->buf[2] = 0xFF;
	posAccuracy->buf[3] = 0xFF;
	pvd->startVector.posAccuracy = posAccuracy;
	posConfidence = (PositionConfidenceSet_t *) calloc(1,
			sizeof(PositionConfidenceSet_t));
	posConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	posConfidence->size = sizeof(uint8_t);
	posConfidence->buf[0] = 0;
	pvd->startVector.posConfidence = posConfidence;
	speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1,
			sizeof(SpeedandHeadingandThrottleConfidence_t));
	speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	speedConfidence->size = sizeof(uint8_t);
	speedConfidence->buf[0] = 0;
	pvd->startVector.speedConfidence = speedConfidence;
	pvd->vehicleType.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	pvd->vehicleType.size = sizeof(uint8_t);
	pvd->vehicleType.buf[0] = 4;
	pvd->cntSnapshots = (Count_t *) calloc(1, sizeof(Count_t));
	pvd->cntSnapshots[0] = 1;
	snapshot = (Snapshot_t *) calloc(1, sizeof(Snapshot_t));
	snapshot->thePosition.Long = 0;
	snapshot->thePosition.lat = 0;
	safetyExt = (VehicleSafetyExtension_t *) calloc(1,
			sizeof(VehicleSafetyExtension_t));
	safetyExt->events = (EventFlags_t *) calloc(1, sizeof(EventFlags_t));
	safetyExt->events[0] = 1;
	snapshot->safetyExt = safetyExt;
	ASN_SEQUENCE_ADD(&pvd->snapshots.list, snapshot);
	snapshot = (Snapshot_t *) calloc(1, sizeof(Snapshot_t));
	safetyExt = (VehicleSafetyExtension_t *) calloc(1,
			sizeof(VehicleSafetyExtension_t));
	safetyExt->events = (EventFlags_t *) calloc(1, sizeof(EventFlags_t));

	snapshot->thePosition.Long = 1000;
	snapshot->thePosition.lat = 1000;
	safetyExt->events[0] = 1;
	ASN_SEQUENCE_ADD(&pvd->snapshots.list, snapshot);
	pvd->segNum = (ProbeSegmentNumber_t *) calloc(1,
			sizeof(ProbeSegmentNumber_t));
	pvd->segNum[0] = pktnum % 127;
	pktnum++;

	rvalenc = der_encode_to_buffer(&asn_DEF_ProbeVehicleData, pvd,
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
//    printf("Structure successfully encoded %d\n", rvalenc.encoded);
		pktData->length = rvalenc.encoded;
		asn_DEF_ProbeVehicleData.free_struct(&asn_DEF_ProbeVehicleData, pvd, 0);
	}
}

void buildRSAPacket(WSMData *pktData) {
	int i;
	static int pktnum = 0;
	//uint8_t num = 0;
	DDateTime_t *utcTime;
	Elevation_t *elevation;
//  uint16_t intg16;
//  uint32_t intg32;
	TransmissionAndSpeed_t *speed;
	PositionalAccuracy_t *posAccuracy;
	PositionConfidenceSet_t *posConfidence;
	SpeedandHeadingandThrottleConfidence_t *speedConfidence;
	asn_enc_rval_t rvalenc;
	RoadSideAlert_t *rsa;
	ITIScodes_t *code;
	crc m_crc;
	uint8_t *buf;

	rsa = (RoadSideAlert_t *) calloc(1, sizeof(*rsa));
	rsa->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	rsa->msgID.size = sizeof(uint8_t);
	rsa->msgID.buf[0] = DSRCmsgID_roadSideAlert;
	rsa->typeEvent = itiscodes[0];
	rsa->description = (description_t *) calloc(1, sizeof(description_t)); //create
	for (i = 1; i < m_itisItems; i++) {
		code = (ITIScodes_t*) calloc(1, sizeof(ITIScodes_t));
		*code = itiscodes[i];
		ASN_SEQUENCE_ADD(&rsa->description->list, code);
	}

	sem_wait(&can_sem);/*example of data assigned to priority and not the exact data has been assigned */

	rsa->priority = (Priority_t *) calloc(1, sizeof(Priority_t));
	rsa->priority->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	rsa->priority->size = sizeof(uint8_t);
	rsa->priority->buf[0] = candata.exteriorLights;

	sem_post(&can_sem);/*the following paramerters has to be updated */

	rsa->heading = (HeadingSlice_t *) calloc(1, sizeof(HeadingSlice_t));
	rsa->heading->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	rsa->heading->size = 2 * sizeof(uint8_t);
	rsa->heading->buf[0] = 1;
	rsa->heading->buf[1] = 2;
	rsa->extent = (Extent_t *) calloc(1, sizeof(Extent_t));
	rsa->extent->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	rsa->extent->size = sizeof(uint8_t);
	rsa->extent->buf[0] = Extent_useFor5000meters;

	rsa->positon = (FullPositionVector_t *) calloc(1,
			sizeof(FullPositionVector_t));
	utcTime = (DDateTime_t *) calloc(1, sizeof(DDateTime_t));
	rsa->positon->utcTime = utcTime;
	utcTime->year = (DYear_t *) calloc(1, sizeof(DYear_t));
	utcTime->month = (DMonth_t *) calloc(1, sizeof(DMonth_t));
	utcTime->day = (DDay_t *) calloc(1, sizeof(DDay_t));
	utcTime->hour = (DHour_t *) calloc(1, sizeof(DHour_t));
	utcTime->minute = (DMinute_t *) calloc(1, sizeof(DMinute_t));
	utcTime->second = (DSecond_t *) calloc(1, sizeof(DSecond_t));
	utcTime->year[0] = year_val;
	utcTime->month[0] = month_val;
	utcTime->day[0] = day_val;
	utcTime->hour[0] = hour_val;
	utcTime->minute[0] = minute_val;
	*(utcTime->second) = sec_16;
	rsa->positon->Long = longitude_val;
	rsa->positon->lat = latitude_val;
	elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
	elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	elevation->size = 2 * sizeof(uint8_t);
	elevation->buf[0] = elevation_val[0];
	elevation->buf[1] = elevation_val[1];

	rsa->positon->elevation = elevation;
	rsa->positon->heading = (Heading_t *) calloc(1, sizeof(Heading_t));
	*(rsa->positon->heading) = heading_val;
	speed = (TransmissionAndSpeed_t *) calloc(1,
			sizeof(TransmissionAndSpeed_t));
	speed->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	speed->size = 2 * sizeof(uint8_t);
	speed->buf[0] = transmission_speed[0];
	speed->buf[1] = transmission_speed[1];
	rsa->positon->speed = speed;
	posAccuracy = (PositionalAccuracy_t *) calloc(1,
			sizeof(PositionalAccuracy_t));
	posAccuracy->buf = (uint8_t *) calloc(4, sizeof(uint8_t));
	posAccuracy->size = 4 * sizeof(uint8_t);
	posAccuracy->buf[0] = 0;
	posAccuracy->buf[1] = 0;
	posAccuracy->buf[2] = 0;
	posAccuracy->buf[3] = 0;
	rsa->positon->posAccuracy = posAccuracy;
	posConfidence = (PositionConfidenceSet_t *) calloc(1,
			sizeof(PositionConfidenceSet_t));
	posConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	posConfidence->size = sizeof(uint8_t);
	posConfidence->buf[0] = 0;
	rsa->positon->posConfidence = posConfidence;
	speedConfidence = (SpeedandHeadingandThrottleConfidence_t *) calloc(1,
			sizeof(SpeedandHeadingandThrottleConfidence_t));
	speedConfidence->buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	speedConfidence->size = sizeof(uint8_t);
	speedConfidence->buf[0] = 0;
	rsa->positon->speedConfidence = speedConfidence;

	rsa->crc.buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	rsa->crc.size = 2 * sizeof(uint8_t);
	rsa->msgCnt = pktnum % 127;
	// memcpy(&rsa->msgCnt, &num, 1);
	pktnum++;

	buf = &pktData->contents;
	rvalenc = der_encode_to_buffer(&asn_DEF_RoadSideAlert, rsa,
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
				strerror(errno));
	} else {
//    printf("Structure successfully encoded %d\n", rvalenc.encoded);
		crcInit();
		m_crc = crcFast(buf, rvalenc.encoded - 2);
		buf[rvalenc.encoded - 1] = LOBYTE(m_crc);
		buf[rvalenc.encoded - 2] = HIBYTE(m_crc);
		m_crc = crcFast(buf, rvalenc.encoded);
		if (m_crc == 0) {
			printf("CRC check successful\n");
		} else {
			fprintf(stderr, "CRC check unsuccessful\n");
		}
		pktData->length = rvalenc.encoded;
		asn_DEF_RoadSideAlert.free_struct(&asn_DEF_RoadSideAlert, rsa, 0);
	}
}

void buildEVAPacket(WSMData *pktData) {
	static int pktnum = 0;
	crc m_crc;
	uint8_t *buf;
	asn_enc_rval_t rvalenc;
	Elevation_t *elevation;
	EmergencyVehicleAlert_t *eva;

	eva = (EmergencyVehicleAlert_t *) calloc(1, sizeof(*eva));

	eva->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	eva->msgID.size = sizeof(uint8_t);
	eva->msgID.buf[0] = DSRCmsgID_emergencyVehicleAlert;

	eva->rsaMsg.msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	eva->rsaMsg.msgID.size = sizeof(uint8_t);
	eva->rsaMsg.msgID.buf[0] = DSRCmsgID_roadSideAlert;
	eva->rsaMsg.msgCnt = pktnum % 127;
	eva->rsaMsg.typeEvent = itiscodes[0]; // TODO: currently static event
	
	eva->rsaMsg.positon = (FullPositionVector_t *) calloc(1,
			sizeof(FullPositionVector_t));

	eva->rsaMsg.positon->lat = latitude_val;
	eva->rsaMsg.positon->Long =longitude_val;

	elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t));
	elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	elevation->size = 2 * sizeof(uint8_t);
	elevation->buf[0] = elevation_val[0];
	elevation->buf[1] = elevation_val[1];

	eva->rsaMsg.positon->elevation = elevation;

	eva->rsaMsg.crc.buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	eva->rsaMsg.crc.size = 2 * sizeof(uint8_t);
	eva->rsaMsg.crc.buf[0] = 0;
	eva->rsaMsg.crc.buf[1] = 0;

	eva->crc.buf = (uint8_t *) calloc(2, sizeof(uint8_t));
	eva->crc.size = 2 * sizeof(uint8_t);

	pktnum++;
	
	buf = &pktData->contents;
	rvalenc = der_encode_to_buffer(&asn_DEF_EmergencyVehicleAlert, eva, 
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
			strerror(errno));
	} else {
		crcInit();
		m_crc = crcFast(buf, rvalenc.encoded - 2);
		buf[rvalenc.encoded - 1] = LOBYTE(m_crc);
		buf[rvalenc.encoded - 2] = HIBYTE(m_crc);
		m_crc = crcFast(buf, rvalenc.encoded);
		if (m_crc == 0) {
		//	printf("CRC check successful\n");
		} else {
			fprintf(stderr, "CRC check unsuccessful\n");
		}
		pktData->length = rvalenc.encoded;
		asn_fprint(stdout, &asn_DEF_EmergencyVehicleAlert, eva);
		asn_DEF_EmergencyVehicleAlert.free_struct(&asn_DEF_EmergencyVehicleAlert, eva, 0);
	}
}

void buildACMPacket(WSMData *pktData) {
	static int pktnum = 0;
	crc m_crc;
	uint8_t *buf;
	asn_enc_rval_t rvalenc;
	AlaCarte_t *acm;

	acm = (AlaCarte_t *) calloc(1, sizeof(*acm));

	acm->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t));
	acm->msgID.size = sizeof(uint8_t);
	acm->msgID.buf[0] = DSRCmsgID_alaCarteMessage;

	pktnum++;
	
	buf = &pktData->contents;
	rvalenc = der_encode_to_buffer(&asn_DEF_AlaCarte, acm, 
			&pktData->contents, 1000);
	if (rvalenc.encoded == -1) {
		fprintf(stderr, "Cannot encode %s: %s\n", rvalenc.failed_type->name,
			strerror(errno));
	} else {
		crcInit();
		m_crc = crcFast(buf, rvalenc.encoded - 2);
		buf[rvalenc.encoded - 1] = LOBYTE(m_crc);
		buf[rvalenc.encoded - 2] = HIBYTE(m_crc);
		m_crc = crcFast(buf, rvalenc.encoded);
		if (m_crc == 0) {
			//printf("CRC check successful\n");
		} else {
			fprintf(stderr, "CRC check unsuccessful\n");
		}
		pktData->length = rvalenc.encoded;
		asn_fprint(stdout, &asn_DEF_AlaCarte, acm);
		asn_DEF_AlaCarte.free_struct(&asn_DEF_AlaCarte, acm, 0);
	}
}

int buildWSMRequestPacket() {
	wsmreq.chaninfo.channel = txChan;
	wsmreq.chaninfo.rate = data_rateidx;
	wsmreq.chaninfo.txpower = generateMaxtxpowReq();
	if (app_txpower < wsmreq.chaninfo.txpower) //Here we compare txpower from user,tx power from driver and we consider lesser one
		wsmreq.chaninfo.txpower = app_txpower;
	wsmreq.version = 2;
	wsmreq.psid = app_psid;
	wsmreq.wsmps = app_wsmps;
	getMACAddr(wsmreq.srcmacaddr, txChan);
	if ((qpriority < 8) && (qpriority > 0))
		wsmreq.txpriority = qpriority;
	else
		wsmreq.txpriority = 2;
	wsmreq.security = secType;

	return 0;
}

int roundoff(int msecs, int ipd) {
	int16_t modipd = msecs % ipd;
	if (modipd > ipd / 2)
		return (msecs + (ipd - modipd));
	else
		return (msecs - modipd);
}

int buildWSMRequestData_first() {

	int val_16 = 0;
	int msecs = 0;

	memset(&wsmreq.data, 0, sizeof(WSMData));
	tsfdiff = 0;
	gpsc_requery: ReadGpsdata();

	//gettimeofday(&tv,NULL);
	// in msecs
	wsmgps.local_tsf = (int) gtv.tv_usec / 1000;
	//  printf("gtvsec%lu ,gtvusec%lu ltsf%llu\n",gtv.tv_sec,gtv.tv_usec,wsmgps.local_tsf);
	wsmgps.local_tsf += ((uint64_t) gtv.tv_sec * 1000);
	//  printf("gtvsec%lu ,gtvusec%lu ltsf%llu\n",gtv.tv_sec,gtv.tv_usec,wsmgps.local_tsf);
	tsfTime = wsmgps.local_tsf;
	/*CHANGED TIME TO SHOW GPS-REFERENCED  AND NOT TSF REFERENCE	*/
#if 1
	if (wsmgps.latitude != GPS_INVALID_DATA
			&& wsmgps.longitude != GPS_INVALID_DATA) {
		if (interpol_start) //are we ready to interpolate ? have enough samples!
		{
			if (wsmgps.actual_time == prev_acttime) //do we need to interpolate? same value repeated
					{
				if (requery_start == 2) {
					//(void)syslog(LOG_INFO,"rt%lf",wsmgps.actual_time);
					//printf("rt%lf ",wsmgps.actual_time);
					requery_start = 0;
					interpol_start = 0;
					first_ip = 0;

					prevlat_ip[0] = prevlat_ip[1];
					prevlon_ip[0] = prevlon_ip[1];
					if (wsmgps.course != GPS_INVALID_DATA)
						prevcourse_ip[0] = prevcourse_ip[1];

					return -1;
				}
				tsfdiff = tsfTime - tsfref;

				if (tsfdiff >= (GPS_UPDATE_INT - 30)) { // to prevent interpolating 2 times if tsfdiff comes to 199
					//printf("R-");
					requery_start_time = tsfTime;
					requery_start++;
					mysleep(0, 10000000); //10ms
					gettimeofday(&gtv, NULL); //update gtv for requery
					goto gpsc_requery;
				}
//                    printf("--I");//,tsfdiff);//%d\n",tsfTime -msecs);
//	            printf("a%lf p%lf m%d t%llu s%d r%llu d%lld\n",wsmgps.actual_time, prev_acttime, msecs,tsfTime,sec_16, tsfref, tsfdiff);
				if (use_interpolate == 1) {
					interpolate_gps(tsfdiff);
					//printf("Interpolated Latitude [%lf] \n",wsmgps.latitude);
					//printf("Interpolated Longitude [%lf] \n",wsmgps.longitude);
				}
			} else //slide window on every gps-update based on time and not on different values
			{
				requery_start = 0;
				requery_start_time = 0;
				prevlat_ip[0] = prevlat_ip[1];
				prevlon_ip[0] = prevlon_ip[1];
				if (wsmgps.course != GPS_INVALID_DATA) {
					prevcourse_ip[0] = prevcourse_ip[1];
					prevcourse_ip[1] = wsmgps.course;
				}

				prevlat_ip[1] = wsmgps.latitude;
				prevlon_ip[1] = wsmgps.longitude;

				prev_acttime = wsmgps.actual_time;
				tsfref = tsfTime;
				tsfdiff = 0;
			}
		} else {
			if (first_ip) // collect first sample
			{
				requery_start = 0;
				requery_start_time = 0;
				prevlat_ip[0] = wsmgps.latitude;
				prevlon_ip[0] = wsmgps.longitude;
				if (wsmgps.course != GPS_INVALID_DATA)
					prevcourse_ip[0] = wsmgps.course;
				first_ip = 0;
				prev_acttime = wsmgps.actual_time;
				tsfref = tsfTime;
//printf("FS %lf\n", prev_acttime);
			} else if (wsmgps.actual_time != prev_acttime) //second sample and allow to interpolate
					{
				requery_start = 0;
				requery_start_time = 0;

				prevlat_ip[1] = wsmgps.latitude;
				prevlon_ip[1] = wsmgps.longitude;
				if (wsmgps.course != GPS_INVALID_DATA)
					prevcourse_ip[1] = wsmgps.course;
				interpol_start = 1;
				prev_acttime = wsmgps.actual_time;
				tsfref = tsfTime;
				tsfdiff = 0;
//printf("SS %lf\n", prev_acttime);
			}
#if 1
			else { // Sample after first sample collection
				tsfdiff = tsfTime - tsfref;
#if 0
				if (tsfdiff < 0) {
					if ((1000 + tsfdiff) <= GPS_UPDATE_INT)
					tsfdiff = 1000 + tsfdiff;
					else  // TSF got reset, pps ?
					tsfdiff = tsfTime;
				}
#endif
				prevlat_ip[1] = prevlat_ip[0];
				prevlon_ip[1] = prevlon_ip[0];
				if (wsmgps.course != GPS_INVALID_DATA)
					prevcourse_ip[1] = prevcourse_ip[0];

				if (use_interpolate == 1) {
					interpolate_gps(tsfdiff);
					//printf("Interpolated Latitude [%lf] \n",wsmgps.latitude);
					//printf("Interpolated Longitude [%lf] \n",wsmgps.longitude);
				}
//printf("IS %lf tt %lld tr %lld\n", prev_acttime, tsfTime, tsfref);
			}
#endif
		}
		//Kalman filter in heading
		wsmgps.course = fnKalmanFilter_Heading(wsmgps.course);
	} else { // if we miss values in between !! start again
//         printf("@M");
		first_ip = 1;
		interpol_start = 0;
	}

#endif

	if (tsfdiff < 0) {
		syslog(LOG_INFO, "tsfdiff %lld\n", tsfdiff);
		return -1;
	}
	if (tsfdiff >= 0) // Update only if the tsfdiff is positive, I guess this can happen if the system time gets updated in between
		//	wsmgps.actual_time += (double)(tsfdiff)/1000; //to remove seconds jump in pcap log
		msecs = ((int) ((wsmgps.actual_time - (int) wsmgps.actual_time) * 1000))
				% 1000;
	msecs += (tsfdiff % 1000); // assuming tsdiff is never more than 1000ms
	wsmgps.actual_time = (int) wsmgps.actual_time;
	msecs = roundoff(msecs, pktdelaymsecs);
	wsmgps.actual_time += (double) msecs / 1000;

	wsmgps.time = get_time(wsmgps.actual_time);
	wsmgps.date = get_date(wsmgps.actual_time);

	sec_16 = (uint16_t) (((unsigned int) (wsmgps.time)) % 100);
	sec_16 = (sec_16 * 1000) + (msecs % 1000);
//printf("msecs=%d tsf=%d sec16:%u\n",msecs,tsfTime,sec_16);
//printf(">a%lf m%d t%d s%u r%u d%lld\n",wsmgps.actual_time, msecs,tsfTime,sec_16, tsfref, tsfdiff);
//printf(">a%lf p%lf m%d t%d s%d r%u d%lld\n",wsmgps.actual_time, prev_acttime, msecs,tsfTime,sec_16, tsfref, tsfdiff);
//printf(">a%lf gt%lf p%lf m%d t%llu s%u r%llu d%llu\n",wsmgps.actual_time,wsmgps.time, prev_acttime, msecs,tsfTime,sec_16, tsfref, tsfdiff);

	year_val = (wsmgps.date % 100);
	month_val = ((wsmgps.date / 100)) % 100;
	day_val = (wsmgps.date / 10000);
	hour_val = ((unsigned int) wsmgps.time / 10000);
	minute_val = (((unsigned int) wsmgps.time / 100)) % 100;
	min_msec = (minute_val * 60) * 1000 + sec_16; //in msec
	if ((int) wsmgps.actual_time == GPS_INVALID_DATA) {
		day_val = 0;
		month_val = 15;
		year_val = 0;
		hour_val = 31;
		minute_val = 63;
		sec_16 = 65535;
	}
	if (wsmgps.latitude == GPS_INVALID_DATA)
		latitude_val = 900000001;
	else
		latitude_val = (long) ((wsmgps.latitude) * 10000000);

	if (wsmgps.longitude == GPS_INVALID_DATA)
		longitude_val = 1800000001;
	else
		longitude_val = (long) ((wsmgps.longitude) * 10000000);

	if (wsmgps.altitude >= 0 && wsmgps.altitude <= 6143.9) {
		elevation_val[0] = (uint8_t) (((uint32_t) (wsmgps.altitude * 10)
				& 0xFF00) >> 8);
		elevation_val[1] = (uint8_t) (((uint32_t) (wsmgps.altitude * 10)
				& 0x00FF));
	} else if (wsmgps.altitude > -409.5 && wsmgps.altitude < -0.1) {
		val_16 = (uint32_t) (wsmgps.altitude * 10);
		val_16 = 65535 + val_16;
		elevation_val[0] = (uint8_t) (((uint32_t) (val_16) & 0xFF00) >> 8);
		elevation_val[1] = (uint8_t) (((uint32_t) (val_16) & 0x00FF));
	}

	if (wsmgps.altitude == GPS_INVALID_DATA) {
		elevation_val[0] = ((61440 & 0xFF00) >> 8);
		elevation_val[1] = ((61440 & 0x00FF));
	}

	if (wsmgps.course == GPS_INVALID_DATA)
		heading_val = 28800;
	else {
		heading_val = (uint32_t) (((wsmgps.course) * 80));
		if (!is_HeadingLatch) {
			if (wsmgps.speed > SPEED_LATCH)
				latch_heading_val = heading_val;
			else {
				is_HeadingLatch = 1;
				heading_val = latch_heading_val;
			}
		} else {
			if (wsmgps.speed > SPEED_UNLATCH)
				is_HeadingLatch = 0;
			else
				heading_val = latch_heading_val;
		}
	}
	VehicleData vehData;
	vehData_get(&vehData);

	if(vehData.speed != 8191) {
		transmission_speed[0] = (int8_t) (((uint32_t) (vehData.speed * 50) & 0xFF00) >> 8);
		transmission_speed[1] = (uint8_t) (((uint32_t) (vehData.speed * 50) & 0x00FF));
		transmission_speed[0] = transmission_speed[0] | 0xE0;
	} else {
		if (wsmgps.speed != GPS_INVALID_DATA) {
			transmission_speed[0] = (uint8_t) (((uint32_t) (wsmgps.speed * 50)
					& 0xFF00) >> 8);
			transmission_speed[1] = (uint8_t) (((uint32_t) (wsmgps.speed * 50)
					& 0x00FF));
			transmission_speed[0] = transmission_speed[0] | 0xE0;
		} else {
			transmission_speed[0] = ((8191 & 0xFF00) >> 8);
			transmission_speed[1] = ((8191 & 0x00FF));
			transmission_speed[0] = transmission_speed[0] | 0xE0;
		}
	}
	return 1;
}

int buildWSMRequestData() {
//	char ch='1';
//        int val_16 = 0;
	/*static int first_heading=1;
	 static double heading_avg[5];//average values*/
	static double lon_acc_hb = 0, yaw_hb;
	//int msecs =0,msec_hb;
	int msec_hb;
	//struct timeval tv;

	if (wsmgps.speed != GPS_INVALID_DATA && wsmgps.course != GPS_INVALID_DATA) {
//path prediction
#if 0
		if(heading_count >= MAXHEADING_BUF_LENGTH )
		{ // wait till 6
			if(!first_heading) {
				heading_buff[4] = wsmgps.course;
				average_heading(1,heading_avg); //calculate for 5 recent including current  
				memmove(heading_buff+0,heading_buff+1,32);
			}
			else {
				heading_buff[6] = wsmgps.course;
				average_heading(3,heading_avg); //for first time take 4 averages by sliding window on 7 recent heading values including current
				memmove(heading_buff+0,heading_buff+4,32);// move last 4 elements to be first 4 4 nxt calculation
				memset(heading_buff+4,0,32);
				first_heading = 0;
			}

			pp_yaw[0] = (heading_avg[1] - heading_avg[0])*10; //yaw per 100 msecs, old
			pp_yaw[1] = (heading_avg[2] - heading_avg[1])*10;//yaw per 100 msecs, recent
			pp_yawrate = fabs(pp_yaw[1]-pp_yaw[0]);//cur-prev
			pp_roc = ((wsmgps.speed/deg2rad(pp_yaw[1]))*10);
			pp_confidence = confidence_lookup(pp_yawrate)*2;//lsb units of 0.5%    
			if(pp_confidence == 1 )//if lookup returns w/o finding valid confidence value
			pp_confidence = INVALID_YAW_RATE;
			if(pp_roc > 32767 || wsmgps.speed < 0.1389 ) { //speed < 0.5 kmph
				if(wsmgps.speed < 0.138)
				pp_confidence = 100;//for stationary entity
				pp_roc = 32767;
			}
			else if(pp_roc < -32767)
			pp_roc = -32767;
			memmove(heading_avg+0,heading_avg+1,16); //move last two avg to first three 
		}
		else {
			//save heading to buffer
			heading_buff[heading_count] = wsmgps.course;
			heading_count ++;
			pp_roc = ((wsmgps.speed/deg2rad(wsmgps.course-heading_buff[heading_count-2]))*10);
			if(heading_count > 1) {
				pp_yawrate = fabs(wsmgps.course - heading_buff[heading_count-2] ); //cur-prev
				pp_confidence = confidence_lookup(pp_yawrate)*2;//lsb units of 0.5%    
			}
		}
#endif

		//longitudinal acc, yaw rate , hard braking event
		if (lon_acc_flag == 1) { //ref1
			lon_acc_ref_sec = wsmgps.actual_time;
			lon_acc_speed = wsmgps.speed;
			yaw_head = heading_val;
			lon_acc_flag = 0;
		}
		msec_hb = roundoff(((wsmgps.actual_time - lon_acc_ref_sec) * 1000), 100)
				;
		if ((msec_hb >= GPS_UPDATE_INT) && lon_acc_flag == 0) {
			if ((roundoff(((wsmgps.actual_time - ref_time) * 1000), 100)) == 300){
				//check with ref2, tbd using update macro
				ref_time = wsmgps.actual_time; //ref2 200msec
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			} else if (msec_hb >= 2 * GPS_UPDATE_INT) { //400 msec
				lon_acc_hb = ((wsmgps.speed - lon_acc_speed) * 1000)
						/ (2 * GPS_UPDATE_INT); // meter/400msec^2,,= delta-v*1000/400
				lon_acc_accl = (int16_t) (lon_acc_hb * 100); //lsb units are 0.01m/s^2
				yaw_hb = ((heading_val - yaw_head) * 1000)
						/ (2 * GPS_UPDATE_INT); //lsb units are 0.01 deg/400msec(signed),=(0.01*1000)/400
				yaw_rate = (int16_t) yaw_hb;
				lon_acc_ref_sec = ref_time; //ref1=ref2
				lon_acc_speed = ref_speed;
				yaw_head = ref_yaw;
				ref_time = wsmgps.actual_time; //ref2 current
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			}
		}
		if (!strcasecmp(can_interface, "BTCAN")) {
			if (candata.eventFlag) {
				hb_event_flag = 1;
				ALL = 1;
				path_num = 0;
				candata.eventFlag = 0;
				printf("---HB_EVT_SET---\n");
			} else {
				hb_event_flag = 0;
				path_num = 4;
			}
		} else if (lon_acc_hb < -4) {
			hb_event_flag = 1;
			ALL = 1;
			path_num = 0;
		} else {
			hb_event_flag = 0;
			path_num = 4;
		}
	} //invalid check if

	if (lon_acc_accl < -2000)
		lon_acc_accl = -2000; //min value
	if (lon_acc_accl > 2000)
		lon_acc_accl = 2000; //max value
	///hard braking

	int msgID_case = msgType[0];
	/*if(difftime(Curr_Timer, TIM_Timer) > TIM_TIMEOUT) {
		msgID_case = msgType[1];
	}*/
	switch (msgID_case) {
	case DSRCmsgID_basicSafetyMessage:
		buildBSMPacket(&wsmreq.data);
		break;
	case DSRCmsgID_probeVehicleData:
		buildPVDPacket(&wsmreq.data);
		break;
	case DSRCmsgID_roadSideAlert:
		buildRSAPacket(&wsmreq.data);
		break;
	case DSRCmsgID_intersectionCollisionAlert:
		buildICAPacket(&wsmreq.data);
		break;
	case DSRCmsgID_mapData:
		buildMAPPacket(&wsmreq.data);
		break;
	case DSRCmsgID_signalPhaseAndTimingMessage:
		buildSPATPacket(&wsmreq.data);
		break;
	case DSRCmsgID_travelerInformation:
		buildTIMPacket(&wsmreq.data);
		break;
	default:
		fprintf(stderr, "Invalid Message Type - %d", msgType[0]);
		exit(-1);
	}
	return 1;
}
int buildBSMRequestData() {
	static double lon_acc_hb = 0, yaw_hb;
	int msec_hb;

	if (wsmgps.speed != GPS_INVALID_DATA && wsmgps.course != GPS_INVALID_DATA) {
		if (lon_acc_flag == 1) { //ref1
			lon_acc_ref_sec = wsmgps.actual_time;
			lon_acc_speed = wsmgps.speed;
			yaw_head = heading_val;
			lon_acc_flag = 0;
		}
		msec_hb = roundoff(((wsmgps.actual_time - lon_acc_ref_sec) * 1000), 100)
				;
		if ((msec_hb >= GPS_UPDATE_INT) && lon_acc_flag == 0) {
			if ((roundoff(((wsmgps.actual_time - ref_time) * 1000), 100)) == 300){
				//check with ref2, tbd using update macro
				ref_time = wsmgps.actual_time; //ref2 200msec
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			} else if (msec_hb >= 2 * GPS_UPDATE_INT) { //400 msec
				lon_acc_hb = ((wsmgps.speed - lon_acc_speed) * 1000)
						/ (2 * GPS_UPDATE_INT); // meter/400msec^2,,= delta-v*1000/400
				lon_acc_accl = (int16_t) (lon_acc_hb * 100); //lsb units are 0.01m/s^2
				yaw_hb = ((heading_val - yaw_head) * 1000)
						/ (2 * GPS_UPDATE_INT); //lsb units are 0.01 deg/400msec(signed),=(0.01*1000)/400
				yaw_rate = (int16_t) yaw_hb;
				lon_acc_ref_sec = ref_time; //ref1=ref2
				lon_acc_speed = ref_speed;
				yaw_head = ref_yaw;
				ref_time = wsmgps.actual_time; //ref2 current
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			}
		}
		if (!strcasecmp(can_interface, "BTCAN")) {
			if (candata.eventFlag) {
				hb_event_flag = 1;
				ALL = 1;
				path_num = 0;
				candata.eventFlag = 0;
				printf("---HB_EVT_SET---\n");
			} else {
				hb_event_flag = 0;
				path_num = 4;
			}
		} else if (lon_acc_hb < -4) {
			hb_event_flag = 1;
			ALL = 1;
			path_num = 0;
		} else {
			hb_event_flag = 0;
			path_num = 4;
		}
	} //invalid check if

	if (lon_acc_accl < -2000)
		lon_acc_accl = -2000; //min value
	if (lon_acc_accl > 2000)
		lon_acc_accl = 2000; //max value
	///hard braking

	buildBSMPacket(&wsmreq.data);
	return 1;
}

int buildEVARequestData() {
	static double lon_acc_hb = 0, yaw_hb;
	int msec_hb;

	if (wsmgps.speed != GPS_INVALID_DATA && wsmgps.course != GPS_INVALID_DATA) {
		if (lon_acc_flag == 1) { //ref1
			lon_acc_ref_sec = wsmgps.actual_time;
			lon_acc_speed = wsmgps.speed;
			yaw_head = heading_val;
			lon_acc_flag = 0;
		}
		msec_hb = roundoff(((wsmgps.actual_time - lon_acc_ref_sec) * 1000), 100)
				;
		if ((msec_hb >= GPS_UPDATE_INT) && lon_acc_flag == 0) {
			if ((roundoff(((wsmgps.actual_time - ref_time) * 1000), 100)) == 300){
				//check with ref2, tbd using update macro
				ref_time = wsmgps.actual_time; //ref2 200msec
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			} else if (msec_hb >= 2 * GPS_UPDATE_INT) { //400 msec
				lon_acc_hb = ((wsmgps.speed - lon_acc_speed) * 1000)
						/ (2 * GPS_UPDATE_INT); // meter/400msec^2,,= delta-v*1000/400
				lon_acc_accl = (int16_t) (lon_acc_hb * 100); //lsb units are 0.01m/s^2
				yaw_hb = ((heading_val - yaw_head) * 1000)
						/ (2 * GPS_UPDATE_INT); //lsb units are 0.01 deg/400msec(signed),=(0.01*1000)/400
				yaw_rate = (int16_t) yaw_hb;
				lon_acc_ref_sec = ref_time; //ref1=ref2
				lon_acc_speed = ref_speed;
				yaw_head = ref_yaw;
				ref_time = wsmgps.actual_time; //ref2 current
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			}
		}
		if (!strcasecmp(can_interface, "BTCAN")) {
			if (candata.eventFlag) {
				hb_event_flag = 1;
				ALL = 1;
				path_num = 0;
				candata.eventFlag = 0;
				printf("---HB_EVT_SET---\n");
			} else {
				hb_event_flag = 0;
				path_num = 4;
			}
		} else if (lon_acc_hb < -4) {
			hb_event_flag = 1;
			ALL = 1;
			path_num = 0;
		} else {
			hb_event_flag = 0;
			path_num = 4;
		}
	} //invalid check if

	if (lon_acc_accl < -2000)
		lon_acc_accl = -2000; //min value
	if (lon_acc_accl > 2000)
		lon_acc_accl = 2000; //max value
	///hard braking

	buildEVAPacket(&wsmreq.data);
	return 1;
}

int buildACMRequestData() {
	static double lon_acc_hb = 0, yaw_hb;
	int msec_hb;

	if (wsmgps.speed != GPS_INVALID_DATA && wsmgps.course != GPS_INVALID_DATA) {
		if (lon_acc_flag == 1) { //ref1
			lon_acc_ref_sec = wsmgps.actual_time;
			lon_acc_speed = wsmgps.speed;
			yaw_head = heading_val;
			lon_acc_flag = 0;
		}
		msec_hb = roundoff(((wsmgps.actual_time - lon_acc_ref_sec) * 1000), 100)
				;
		if ((msec_hb >= GPS_UPDATE_INT) && lon_acc_flag == 0) {
			if ((roundoff(((wsmgps.actual_time - ref_time) * 1000), 100)) == 300){
				//check with ref2, tbd using update macro
				ref_time = wsmgps.actual_time; //ref2 200msec
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			} else if (msec_hb >= 2 * GPS_UPDATE_INT) { //400 msec
				lon_acc_hb = ((wsmgps.speed - lon_acc_speed) * 1000)
						/ (2 * GPS_UPDATE_INT); // meter/400msec^2,,= delta-v*1000/400
				lon_acc_accl = (int16_t) (lon_acc_hb * 100); //lsb units are 0.01m/s^2
				yaw_hb = ((heading_val - yaw_head) * 1000)
						/ (2 * GPS_UPDATE_INT); //lsb units are 0.01 deg/400msec(signed),=(0.01*1000)/400
				yaw_rate = (int16_t) yaw_hb;
				lon_acc_ref_sec = ref_time; //ref1=ref2
				lon_acc_speed = ref_speed;
				yaw_head = ref_yaw;
				ref_time = wsmgps.actual_time; //ref2 current
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			}
		}
		if (!strcasecmp(can_interface, "BTCAN")) {
			if (candata.eventFlag) {
				hb_event_flag = 1;
				ALL = 1;
				path_num = 0;
				candata.eventFlag = 0;
				printf("---HB_EVT_SET---\n");
			} else {
				hb_event_flag = 0;
				path_num = 4;
			}
		} else if (lon_acc_hb < -4) {
			hb_event_flag = 1;
			ALL = 1;
			path_num = 0;
		} else {
			hb_event_flag = 0;
			path_num = 4;
		}
	} //invalid check if

	if (lon_acc_accl < -2000)
		lon_acc_accl = -2000; //min value
	if (lon_acc_accl > 2000)
		lon_acc_accl = 2000; //max value
	///hard braking

	buildACMPacket(&wsmreq.data);
	return 1;
}

int buildTIMRequestData() {
	static double lon_acc_hb = 0, yaw_hb;
	int msec_hb;

	if (wsmgps.speed != GPS_INVALID_DATA && wsmgps.course != GPS_INVALID_DATA) {
		//longitudinal acc, yaw rate , hard braking event
		if (lon_acc_flag == 1) { //ref1
			lon_acc_ref_sec = wsmgps.actual_time;
			lon_acc_speed = wsmgps.speed;
			yaw_head = heading_val;
			lon_acc_flag = 0;
		}
		msec_hb = roundoff(((wsmgps.actual_time - lon_acc_ref_sec) * 1000), 100);
		if ((msec_hb >= GPS_UPDATE_INT) && lon_acc_flag == 0) {
			if ((roundoff(((wsmgps.actual_time - ref_time) * 1000), 100)) == 300){
				//check with ref2, tbd using update macro
				ref_time = wsmgps.actual_time; //ref2 200msec
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			} else if (msec_hb >= 2 * GPS_UPDATE_INT) { //400 msec
				lon_acc_hb = ((wsmgps.speed - lon_acc_speed) * 1000)
						/ (2 * GPS_UPDATE_INT); // meter/400msec^2,,= delta-v*1000/400
				lon_acc_accl = (int16_t) (lon_acc_hb * 100); //lsb units are 0.01m/s^2
				yaw_hb = ((heading_val - yaw_head) * 1000)
						/ (2 * GPS_UPDATE_INT); //lsb units are 0.01 deg/400msec(signed),=(0.01*1000)/400
				yaw_rate = (int16_t) yaw_hb;
				lon_acc_ref_sec = ref_time; //ref1=ref2
				lon_acc_speed = ref_speed;
				yaw_head = ref_yaw;
				ref_time = wsmgps.actual_time; //ref2 current
				ref_speed = wsmgps.speed;
				ref_yaw = heading_val;
			}
		}
		if (!strcasecmp(can_interface, "BTCAN")) {
			if (candata.eventFlag) {
				hb_event_flag = 1;
				ALL = 1;
				path_num = 0;
				candata.eventFlag = 0;
				printf("---HB_EVT_SET---\n");
			} else {
				hb_event_flag = 0;
				path_num = 4;
			}
		} else if (lon_acc_hb < -4) {
			hb_event_flag = 1;
			ALL = 1;
			path_num = 0;
		} else {
			hb_event_flag = 0;
			path_num = 4;
		}
	} //invalid check if

	if (lon_acc_accl < -2000)
		lon_acc_accl = -2000; //min value
	if (lon_acc_accl > 2000)
		lon_acc_accl = 2000; //max value
	///hard braking

	buildTIMPacket(&wsmreq.data);
	return 1;
}

int buildWMEApplicationRequest() {
	wreq.psid = app_psid;
	wreq.repeats = 1;
	wreq.persistence = 1;
	wreq.channel = schan;
	return 1;
}

int buildWMETARequest() {
	tareq.action = TA_ADD;
	tareq.repeatrate = repeatRate_Ta;
	tareq.channel = taarg.channel;
	tareq.channelinterval = taarg.channelinterval;
	tareq.servicepriority = 1;
	return 0;
}

int random_mac_tmpid_at_cert_change(int slotCheck) {
	if (certchange() == CMD_OK_CERT_CHG_POST) {
		if (randstatus == 1)
			(void) generate_random_mac(wsmreq.srcmacaddr);
		if (temp_id_control == 0) {
			temp_id[0] = rand();
			temp_id[1] = rand();
		} else if (temp_id_control == 1) {
			temp_id[0] = rand();
			temp_id[1] = rand();
			temp_id[2] = rand();
			temp_id[3] = rand();
		}
		msgCnt = rand() % 128;
		changeFlag = 0;
		certchanged = 1;
		/*(void)syslog(LOG_INFO,"cert change   M addr %02x%02x%02x%02x%02x%02x  T id %02x%02x%02x%02x\n",
		 wsmreq.srcmacaddr[0],wsmreq.srcmacaddr[1],
		 wsmreq.srcmacaddr[2],wsmreq.srcmacaddr[3],
		 wsmreq.srcmacaddr[4],wsmreq.srcmacaddr[5],temp_id[0],temp_id[1],temp_id[2],temp_id[3]);*/
	} else if (slotCheck > 30)
		changeFlag = 0;
	return 0;

}

int txWSMPPkts(int pid, int from_txrx) {
	//int pwrvalues, ratecount, txprio,
	int ret = 0, count = 0, error; //, pktcount

	//    printf("%.10lf,%.10lf\n",wsmgps.latitude,wsmgps.longitude); //--hits
	if (wsmreq.security) {
		if (wsmreq.security == AsmSign) {
			error = AsmSignData();
		} else if (wsmreq.security == AsmEncrypt)
			error = AsmEncryptData();
		else
			printf(" Not Supported As Provider\n");
	}
	if (wsmreq.security == 0 || error == 0x11 || error == 0x13)
		ret = txWSMPacket(pid, &wsmreq);
	if (ret > 0 && logging) {
		if (log_options == TXLOG || log_options == TXRXLOG) {
			(void) AsnLog(from_txrx, 0, msgType[0], logformat, logbuf_tx, NULL,
					&wsmreq, wsmgps.actual_time, sec_16);
			//local_logging_client(&wsmreq,&wsmgps);
		}
	}
	if (ret < 0) {
		printf("ERR::txWSMPacket status=%d\n", ret);
		drops++;
	} else if (ret == 0) {
		Errs++;
	} else {
		packets++;
		count++;
	}
	//printf("Transmitted #%llu#   dropped #%llu#\n", packets, drops);
	//printf("T #%llu#  D #%llu# E #%llu#\n", packets, drops, Errs);
	return 0;
}

void set_args(void *data, void *argname, int datatype) {
	u_int8_t string[1000];
	// int i;
	//int temp = 0;
	// u_int8_t temp8 = 0;
	struct arguments *argument1;
	argument1 = (struct arguments *) argname;
	switch (datatype) {
	case ADDR_MAC:
		memcpy(string, argument1->macaddr, 17);
		string[17] = '\0';
		if (extract_macaddr((u_int8_t *) data, (char *) string) < 0) {
			printf("invalid address\n");
		}
		break;

	case UINT8_T:

		//temp = atoi(argument1->channel);
		memcpy(data, (char *) argname, sizeof(u_int8_t));
		break;
	}
}

int extract_macaddr(u_int8_t *mac, char *str) {
	int maclen = IEEE80211_ADDR_LEN;
	int len = strlen(str);
	int i = 0, j = 0, octet = 0, digits = 0, ld = 0, rd = 0;
	char num[2];
	u_int8_t tempmac[maclen];
	memset(tempmac, 0, maclen);
	memset(mac, 0, maclen);
	if ((len < (2 * maclen - 1)) || (len > (3 * maclen - 1)))
		return -1;
	while (i < len) {
		j = i;
		while (str[i] != ':' && (i < len)) {
			i++;
		}
		if (i > len)
			exit(0);
		digits = i - j;
		if ((digits > 2) || (digits < 1) || (octet >= maclen)) {
			return -1;
		}
		num[1] = tolower(str[i - 1]);
		num[0] = (digits == 2) ? tolower(str[i - 2]) : '0';
		if (isxdigit(num[0]) && isxdigit(num[1])) {
			ld = (isalpha(num[0])) ? 10 + num[0] - 'a' : num[0] - '0';
			rd = (isalpha(num[1])) ? 10 + num[1] - 'a' : num[1] - '0';
			tempmac[octet++] = ld * 16 + rd;
		} else {
			return -1;
		}
		i++;
	}
	if (octet > maclen)
		return -1;
	memcpy(mac, tempmac, maclen);
	return 0;
}

int confirmBeforeJoin(WMEApplicationIndication *appind) {
	//    printf("\nJoin\n");
	return 1; /*Return 0 for NOT Joining the WBSS*/
}

void wrss_request() {
	int result;
	if (sendreport || retry) {
		result = getWRSSReport(pid, &wrssrq);
		if (result < 0) {
			//printf(" result = %d\n", result );
			//printf( "WRSS Request Failed");
		}
		sendreport = 0;
	}
}

void receiveWME_NotifIndication(WMENotificationIndication *wmeindication) {
}

void receiveWRSS_Indication(WMEWRSSRequestIndication *wrssindication) {
	printf("WRSS receive Channel = %d   Report = %d\n",
			(u_int8_t) wrssindication->wrssreport.channel,
			(u_int8_t) wrssindication->wrssreport.wrss);
	sendreport = 1;
}

void receiveTsfTimerIndication(TSFTimer *timer) {
	printf("TSF Timer: Result=%d, Timer=%llu", (u_int8_t) timer->result,
			(u_int64_t) timer->timer);
}

void receiveWSMIndication(WSMIndication *ind) {
	int k = 0;

	if ((ind->data.contents[1] == 0)) {
		for (k = 0; k < ind->data.length; k++) {
			ind->data.contents[k] = ind->data.contents[k + 2];
		}

		//memmove(ind->data.contents,ind->data.contents+2,ind->data.length-2);
		ind->data.length = ind->data.length - 2;
	}
	memcpy(&rxpkt, ind, sizeof(WSMIndication));
	RxpktProcess();
}

void RxLatencycal() {
	unsigned long rx_timedif_usec;
	unsigned int rx_latency;
	gettimeofday(&rx_tvend, NULL);
	rx_timedif_usec = (((rx_tvend.tv_sec * 1000000) + rx_tvend.tv_usec)
			- ((rx_tvstart.tv_sec * 1000000) + rx_tvstart.tv_usec));
	if (count) {
		rx_latency = rx_timedif_usec / count;
		printf(" Rx Latency per packet(usec)  %d\n", rx_latency);
	} else
		printf(" NO Packets Received\n");
}

void TxLatencycal() {
	unsigned long tx_timedif_usec;
	unsigned int tx_latency;
	gettimeofday(&tx_tvend, NULL);
	tx_timedif_usec = (((tx_tvend.tv_sec * 1000000) + tx_tvend.tv_usec)
			- ((tx_tvstart.tv_sec * 1000000) + tx_tvstart.tv_usec));
	if (packets) {
		tx_latency = tx_timedif_usec / packets;
		printf(" Tx Latency per packet(usec)  %d\n", tx_latency);
	} else
		printf(" NO Packets Transmitted\n");
}

void sig_int(void) {

	printf("\n\n");
	
	int ret;
	int sem_val;
	// Shut down BT Server
	situationalApp_stop();

	if (RemoteEnable == 0
			&& ((thread_options & RXALL_MASK) || (thread_options & RX_MASK))) {
		//printf("Cancel rx\n");
		pthread_cancel(localrx);
	}
	if (RemoteEnable == 1) //closing  client threads in libwave
		closeWAVEDevice();
	if (thread_options == NOTX
		)
		pthread_cancel(gpsthread);

	if (thread_options & TX_MASK) {
		//printf("Cancel tx\n");
		pthread_cancel(localtx);
	}
	if (thread_options & RXUDP_MASK) {
		//    printf("Cancel udptxrx\n");
		pthread_cancel(txrx_udp);
		close(Udp_Socket);
	}
	if (thread_id != 0) {
		//printf("Cancel can\n");
		close(sock);
		pthread_cancel(thread_id);
	}
	if (check_mac((char *) maddr) < 0) {
		//printf("Cancel wrssi\n");
		pthread_cancel(wrssi);
	}
	gpsc_close_sock();
	gpssockfd = -1;
	ret = sem_getvalue(&can_sem, &sem_val);/*getting semaphore value and unlocking if locked */
	if (sem_val <= 0 && ret == 0)
		sem_post(&can_sem);
	sem_destroy(&can_sem);
	ret = sem_getvalue(&rse_sem, &sem_val);/*getting semaphore value and unlocking if locked */
	if (sem_val <= 0 && ret == 0)
		sem_post(&rse_sem);
	sem_destroy(&rse_sem);
	ret = sem_getvalue(&rse_sem2, &sem_val);/*getting semaphore value and unlocking if locked */
	if (sem_val <= 0 && ret == 0)
		sem_post(&rse_sem2);
	sem_destroy(&rse_sem2);

	if (actMsg) {
		free(actMsg);
		actMsg = NULL;
	}
	if (btCan_tId != 0) {
		pthread_cancel(btCan_tId);
		//printf("Cancel bluetooth thread\n");
		sig_int_bluetooth();
	}
	if (rxCan_tId != 0) {
		pthread_cancel(rxCan_tId);
		//printf("Cancel bluetooth thread\n");
		sig_can();
	}
	if (display_type == BLUETOOTH_DISPLAY) {
		display_type = -1;
		pthread_cancel(bluethread);
		//printf("Cancel bluetooth thread\n");
		sig_int_bluetooth();
	}

	if (print_parse == 9999) {
		if ((display_type == LED_DISPLAY) && (AlertStatus == 1)) { //Turn alert OFF if ON
			system("/usr/bin/panel_led 12");
			system("/usr/bin/panel_led 1");
		}
		if (LocOpt) {
			free(LocOpt);
			LocOpt = NULL;
		}
	}
	//printf("remove user\n");
	if (ust.psid)
		ret = removeUser(pid, &ust);
	if (ust2.psid)
		ret = removeUser(pid, &ust2);
	//ret = stopWBSS(pid, &wreq);
	//printf("remove provider\n");
	if (entry.psid)
		removeProvider(pid, &entry);
	if (entry2.psid)
		removeProvider(pid, &entry2);
	//printf("asm disconnect\n");
	AsmDisconnect(txsocket_id, rxsocket_id);
	txsocket_id = -1;
	rxsocket_id = -1;
	if (logging) {
		printf("close log\n");
		close_log(1);
	}
	signal(SIGINT, SIG_DFL);
	printf(" ***** STATISTICS *****\n");
	printf(" Packet received = %llu\n", count);
	RxLatencycal();
	printf(" Packets Sent =  %llu\n", packets);
	TxLatencycal();
	printf(" Packets Dropped = %llu\n", drops);
	printf(" ***** ********** *****\n");
	(void) syslog(LOG_INFO, "Closing Application getwbsstxrxencdec(%u) \n",
			app_psid);
	if (threadCount == 0)
		exit(0);

}
#ifdef IF_NECESSARY
void asm_restart(void) {
	UINT8 send_buff[10240];
	UINT8 recv_buff[10240];
	int send_size = 0;
	int recv_size = 0;
	(void)syslog(LOG_INFO, "Restarting Asmbin \n" );

//    	send EscMsg_Restart request
//    	create signed message to be extracted
	bzero(send_buff, sizeof(send_buff));
	msg_create_restart_msg(send_buff, &send_size);
	syslog(LOG_INFO,"Send EscMsg_Restart request. [0x%02x]\n", send_buff[0]);
	if (0 != AsmSend(send_buff, send_size,tmpsock_id)) {
		printf(" Sending error.\n");
		return -1;
	}
//	receive EscMsg_Restart response
	bzero(recv_buff, sizeof(recv_buff));
	recv_size = AsmRecv(recv_buff, sizeof(recv_buff),tmpsock_id);
	if (recv_size <= 0) {
		return -1;
	}
	if (recv_buff[0] != CMD_OK_Restart_POST) {
		printf("Receive error. [0x%02x]\n", recv_buff[0]);
		return -1;
	}
	else {
		printf("Receive EscMsg_Restart response. [0x%02x]\n", recv_buff[0]);
	}

}
#endif
void sig_term(void) {
	sig_int();
}
void sig_segv(void) {
	(void) syslog(LOG_INFO, "getwbsstxrxencdec(%u) SEGFAULT!!!! \n", app_psid);
	syslog(LOG_INFO, "Asm Recovery running due to Segfault\n");
	system("/usr/local/bin/asmrestart.sh &");

	printf("\n\nSEG FAULT\n\n");

	exit(-1);

	sig_int();
}

void average_heading(uint8_t num, double *avgbuff) { //calculates average for num of window slide from start
	double sum = 0, x;
	uint8_t i, j;
	for (j = 0; j < num; j++) {
		for (i = 0; i < 5; i++) {
			if ((*(heading_buff + i + j)) > 350)
				x = *(heading_buff + i + j) - 360;
			else
				x = *(heading_buff + i + j);
			sum += x;
		}
		sum = sum / 5;
		if (sum < 0)
			sum += 360;
		if (num == 1) { // from second calculation onwards
			avgbuff[2] = sum;
		} else {
			avgbuff[j] = sum;
			sum = 0;
		}
	} //outer for
}

long confidence_lookup(double yawrate) {
	if (yawrate == 0.00)
		return 100;
	else if (yawrate > 0.00 && yawrate <= 0.50)
		return 90;
	else if (yawrate > 0.5 && yawrate <= 1.00)
		return 80;
	else if (yawrate > 1.0 && yawrate <= 1.50)
		return 70;
	else if (yawrate > 1.5 && yawrate <= 2.00)
		return 60;
	else if (yawrate > 2.00 && yawrate <= 2.50)
		return 50;
	else if (yawrate > 2.50 && yawrate <= 5.00)
		return 40;
	else if (yawrate > 5.00 && yawrate <= 10.00)
		return 30;
	else if (yawrate > 10.00 && yawrate <= 15.00)
		return 20;
	else if (yawrate > 15.00)
		return 0;
	return 1;
}

int invokeIPServer(void) {
	int ret;
	struct timeval tv;
	if ((Udp_Socket = socket(AF_INET6, SOCK_DGRAM, IPPROTO_UDP)) <= 0) {
		printf("socket creation failed..\n");
		return -1;
	}
	bzero(&server_addr, sizeof(server_addr));
	server_addr.sin6_addr = in6addr_any;
	server_addr.sin6_family = AF_INET6;
	server_addr.sin6_port = htons(sock_port);
	if ((bind(Udp_Socket, (struct sockaddr *) &server_addr, sizeof(server_addr)))
			< 0)
		printf("bind failed\n");

	if (setsockopt(Udp_Socket, SOL_SOCKET, SO_REUSEADDR, (char *) &ret,
			sizeof(ret)) < 0) {
		perror("setsockopt(SO_REUSEADDR) failed");
		return -1;
	}

	tv.tv_sec = TIME_OUT;
	tv.tv_usec = 0;
	if (setsockopt(Udp_Socket, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv)) < 0) {
		perror("setsockopt(SO_RCVTIMEO) failed");
		return -1;
	}
	return 0;
}

void *udp_client(void *data) {
	socklen_t ipaddrlen;
	FILE *fd;
	int sysval = 0;
	char cmd[80];
	char filename[25];
	pthread_setcancelstate(PTHREAD_CANCEL_ENABLE, NULL );
	pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS, NULL );
	sprintf(filename, "/tmp/udp_%d.txt", sock_port);
	ipaddrlen = sizeof(struct sockaddr_in);
	actMsg = (ActiveMsg *) calloc(1, sizeof(ActiveMsg));
	while (1) {
		if (recvfrom(Udp_Socket, pdu_buf, sizeof(pdu_buf), 0,
				(struct sockaddr *) &server_addr, &ipaddrlen) < 0) {
			if (sysval == 0) {
				syslog(
						LOG_INFO,
						"UDP not received on port[ %d ] since TIMEOUT = %d seconds",
						sock_port, TIME_OUT);
				sysval = 1;
			}
			continue;
		}
		//Tx_Now=0;
		fd = fopen(filename, "w");
		fputs(pdu_buf, fd);
		fclose(fd);
		sprintf(cmd, "if [ -e /tmp/ipfwd_%d.sh ] ; then /tmp/ipfwd_%d.sh ; fi",
				sock_port, sock_port);
		system(cmd);
		sem_wait(&rse_sem2); // wait here until txclient transmit packet
		strcpy(actMsg->actfile, filename);
		get_RSE_options(actMsg);
		apply_RSE_options(actMsg);
		if (actMsg->MessageType_rse == 13) //SPAT
			generationLocation_tx = TRUE;
		else
			generationLocation_tx = FALSE;
		if (restart_app >= 1) {
			if (waveappmode == PROVIDER
				)
				removeProvider(pid, &entry);
			else
				removeUser(pid, &ust);
			if (register_app() < 0)
				txChan = 172;
		}
		restart_app = 0;
		buildWSMRequestPacket();
		if (actMsg->payload_size > 0)
			Tx_Now = 1;

		printf("GPS Data udp_client\n");
		ReadGpsdata();
		sem_post(&rse_sem);
		sysval = 0;
	} //while
//    return 0;
} //end

//This function to extract the Certificate offset from wsmindication contents
int32_t certParse(uint8_t *cert, Certificate * parsedcert) {
	int32_t certparseoffset = 0;
	int32_t ll = 0;
	int32_t len = 0;
	//int32_t scope_namelen=0;
	int32_t permitted_subject_type = 0;
	int32_t psid_array_premissionslen = 0;
	uint8_t psid_array_type = 0;
	uint8_t geographic_region_type = 0;
	//int16_t polylen=0;
	//int16_t crllen=0;
	uint8_t version_and_type = cert[0];
	uint8_t subject_type = cert[1];
	uint8_t cf = cert[2];
	parsedcert->version_and_type = version_and_type;
	parsedcert->subject_type = subject_type;
	parsedcert->cf = cf;
	certparseoffset += 3;
	if (subject_type != root_ca) {
		parsedcert->signer_id = certparseoffset; //signer_id
		certparseoffset += DIGESTLEN;
		parsedcert->signature_alg = cert[certparseoffset]; //sig_alg
		certparseoffset += 1;
	}
//certspecificdata parsing
	if ((subject_type != crl_signer) && (subject_type != message_anonymous)) {
		ll = 0;
		len = 0;
		len = decode_length((cert + certparseoffset), &ll);
		certparseoffset += ll;
		certparseoffset += len;
		//printf("%d - %d\n",ll,certparseoffset);
	}
	//printf("--%d\n",subject_type);
	switch (subject_type) {

	case root_ca:
		ll = 0;
		permitted_subject_type = (int16_t) decode_length(
				(cert + certparseoffset), &ll);
		certparseoffset += ll;
		//printf("****%d - %d- %x\n",ll,certparseoffset,permitted_subject_type);
		if (permitted_subject_type & 0x024f) //parse properly later
				{
			psid_array_type = *(uint8_t *) (cert + certparseoffset);
			certparseoffset += 1; //psid_array_type
			if (psid_array_type == specified) {
				ll = 0;
				psid_array_premissionslen = decode_length(
						(cert + certparseoffset), &ll);
				certparseoffset += ll;
				certparseoffset += psid_array_premissionslen;
			}
		}
		if (permitted_subject_type & 0x00b0) //parse properly later
				{
			psid_array_type = *(uint8_t *) (cert + certparseoffset);
			certparseoffset += 1; //psid_array_type
			if (psid_array_type == specified) {
				ll = 0;
				psid_array_premissionslen = decode_length(
						(cert + certparseoffset), &ll);
				certparseoffset += ll;
				certparseoffset += psid_array_premissionslen;
			}

		}
		//geographic_region
		geographic_region_type = *(uint8_t*) (cert + certparseoffset);
		certparseoffset += 1; //region_type

		switch (geographic_region_type) {
		case from_issuer_region:
		case none:
			break;
		case circle:
			certparseoffset += (sizeof(TwoDLocation) * 2 + sizeof(uint16_t)); //circularregionsize
			break;
		case rectangle:
		case polygon:
			ll = 0;
			len = 0;
			len = (int16_t) decode_length((cert + certparseoffset), &ll);
			certparseoffset += ll;
			certparseoffset += len;
			break;
		}
		break;
	case message_ca:
	case message_ra:
	case message_csr:
	case wsa_ca:
	case wsa_csr:
	case message_identified_not_localized:
	case message_identified_localized:
	case message_anonymous:
	case wsa:
		//subject_type
		if (subject_type == message_ca || subject_type == message_ra
				|| subject_type == message_csr) {
			ll = 0;
			permitted_subject_type = (int16_t) decode_length(
					(cert + certparseoffset), &ll);
			certparseoffset += ll;
			//printf("****%d - %d\n",ll,certparseoffset);
		}
		//additional_data for message_anonymous
		if (subject_type == message_anonymous) {
			ll = 0;
			len = 0;
			len = (int16_t) decode_length((cert + certparseoffset), &ll);
			certparseoffset += ll;
			parsedcert->ad_len = len;
			parsedcert->ad = certparseoffset;
			certparseoffset += len;
			//printf("****ll=%d - len=%d - ad_start=%d - ff=%d\n",ll,len,parsedcert->ad,certparseoffset);
		}
		//psid-array
		psid_array_type = cert[certparseoffset];
		certparseoffset += 1; //psid_array_type
		if (psid_array_type == specified) {
			ll = 0;
			psid_array_premissionslen = decode_length(
					(cert + certparseoffset), &ll);
			certparseoffset += ll;
			certparseoffset += psid_array_premissionslen;
			//printf("%d - %d\n",ll,certparseoffset);
		}
		if (subject_type != message_identified_not_localized) {
			//geographic_region
			geographic_region_type = cert[certparseoffset];
			certparseoffset += 1; //region_type

			switch (geographic_region_type) {
			case from_issuer_region:
			case none:
				break;
			case circle:
				certparseoffset += (sizeof(TwoDLocation) * 2
						+ sizeof(uint16_t)); //circularregionsize
				break;
			case rectangle:
			case polygon:
				ll = 0;
				len = 0;
				len = (uint16_t) decode_length((cert + certparseoffset), &ll);
				certparseoffset += ll;
				certparseoffset += len;
				break;
			}
		}
		break;
	default: //default includes crl_signer
		ll = 0;
		len = 0;
		len = (int16_t) decode_length((cert + certparseoffset), &ll);
		certparseoffset += ll;
		certparseoffset += len;

		break;
	}

	//printf("%d - %d\n",ll,certparseoffset);
	parsedcert->expiration = certparseoffset; //expiration
	certparseoffset += 4; //sizeof Time32
	if (cf & 0x01) {
		parsedcert->start_validity_or_lft = certparseoffset; //startvalidity_or_lft
		if ((cf & 0x02) == 2)
			certparseoffset += 2; //sizeof Time32
		else
			certparseoffset += 4; //sizeof Time32
	}
	//printf("%d - %d\n",ll,certparseoffset);
	parsedcert->crl_series = certparseoffset; //startvalidity_or_lft
	certparseoffset += 4; //sizeof Time32
	//printf("%d - %d\n",ll,certparseoffset);
	if (version_and_type == 2) { //assuming only case 2 or case 3
		certparseoffset += 1; //PKAlgorithm
		parsedcert->verification_key = certparseoffset;
		certparseoffset += 33; //assuming SHA256
	}
	//printf("%d - %d\n",parsedcert->verification_key,certparseoffset);
	if (cf & 0x04) //cf encryption
			{
		certparseoffset += 1; //PKAlgorithm
		certparseoffset += 1; //SymmAlgorithm
		parsedcert->encryption_key = certparseoffset;
		certparseoffset += 33; //assuming SHA256

	}
	//printf("%d - %d\n",parsedcert->verification_key,certparseoffset);
	//TobeSignedCert done

	parsedcert->sig_RV = certparseoffset;
	switch (version_and_type) //assuming SHA256 and ignoring private_use
	{
	case 2:
		certparseoffset += 65;
		break;
	case 3:
		certparseoffset += 33;
		break;
	}
	//printf("%d - %d\n",parsedcert->verification_key,certparseoffset);
	return certparseoffset;
}

//swap gps data 
void SwapGpsdata() {
	uint64_t *lat, *lng, *alt, *tsf, *spd;
	uint64_t *at, *ti, *pt, *ex, *ey, *ev, *crs;
	uint32_t *dte;

	at = ((uint64_t*) (&wsmgps.actual_time));
	*at = swap64(*at);
	ti = ((uint64_t*) (&wsmgps.time));
	*ti = swap64(*ti);
	pt = ((uint64_t*) (&wsmgps.local_tod));
	*pt = swap64(*pt);
	tsf = ((uint64_t*) (&wsmgps.local_tsf));
	*tsf = swap64(*tsf);
	spd = ((uint64_t*) (&wsmgps.speed));
	*spd = swap64(*spd);
	crs = ((uint64_t*) (&wsmgps.course));
	*crs = swap64(*crs);
	lat = ((uint64_t*) (&wsmgps.latitude));
	*lat = swap64(*lat);
	lng = ((uint64_t*) (&wsmgps.longitude));
	*lng = swap64(*lng);
	alt = ((uint64_t*) (&wsmgps.altitude));
	*alt = swap64(*alt);
	dte = (uint32_t*) (&wsmgps.date);
	*dte = swap32_(*dte);
	ex = ((uint64_t*) (&wsmgps.epx));
	*ex = swap64(*ex);
	ey = ((uint64_t*) (&wsmgps.epy));
	*ey = swap64(*ey);
	ev = (uint64_t*) (&wsmgps.epv);
	*ev = swap64(*ev);

}

void ReadGpsdata() {
    //lockLock(pGpsLock);

    /*if(cfGetConfigFile()->useSpoofGpsData)
    {
        wsmgps = cfGetConfigFile()->spoofGpsData;
        btUpdateGpsData(&wsmgps);
        qwarnUpdateGpsData(&wsmgps);
		wsmgps_heading = wsmgps.course;
    }
    else
    {*/
	    char ch = '1';
	    if (gpssockfd > 0) {
	
	        double oldCourse_deg2 = wsmgps.course;
	
		    write(gpssockfd, &ch, 1);
		    read(gpssockfd, &wsmgps, sizeof(wsmgps));

		    if (!BIGENDIAN)
			    SwapGpsdata();

			if ((wsmgps.course != GPS_INVALID_DATA) && (wsmgps.speed != GPS_INVALID_DATA) && (wsmgps.latitude != GPS_INVALID_DATA) && (wsmgps.longitude != GPS_INVALID_DATA)) {
				if(wsmgps.speed < 1.5)//cfGetConfigFile()->headingLockSpeed)
				{
					wsmgps.course = oldCourse_deg;

				}
				else
				{
					oldCourse_deg = wsmgps.course;
				}
				//btUpdateGpsData(&wsmgps);
				//qwarnUpdateGpsData(&wsmgps);
				wsmgps_heading = wsmgps.course;
			}
			else
			{
				//printf("Invalid GPS Data Speed:%f Heading:%f Lat:%f Long:%f!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!\n", wsmgps.speed, wsmgps.course, wsmgps.latitude, wsmgps.longitude);
			}

	    }
	//}
	
	//lockUnlock(pGpsLock);
}
