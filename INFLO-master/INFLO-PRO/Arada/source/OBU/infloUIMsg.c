#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "BTServer.h"
#include "infloUIMsg.h"
#include "TimProcessor.h"
#include "ConfigFile.h"

time_t msgtime;
struct tm *zulutime;
int v2iCount = 0;
char v2iBundle[MAX_V2I_BUNDLE_SIZE*TIM_LEN];


GPSData curr_gps;

#define UI_MSG_LENGTH 2500
#define UI_TMP_LENGTH 300
#define ALERT_TEXT_LENGTH 50

static pthread_mutex_t ui_msg_lock = PTHREAD_MUTEX_INITIALIZER;
static char ui_msg[UI_MSG_LENGTH];
static char ui_tmp[UI_TMP_LENGTH];

typedef struct {
	double distboq;
	double distfoq;
	int time_remaining;
	char text[ALERT_TEXT_LENGTH];
} QWarnAlert;

typedef struct {
	int speed;
	char text[ALERT_TEXT_LENGTH];
} SpdHarmAlert;

void snprintSpdHarmAlert(char *msg_buff, int buff_len, SpdHarmAlert *alert);
void snprintQWarnAlert(char *msg_buff, int buff_len, QWarnAlert *alert);

/**
 * Create string in JSON format of BSM Part II and send to clients
 */
void sendBSMtoUI(WSMData *pktData) {
	pthread_mutex_lock(&ui_msg_lock);

	int counter, length;

	int len = 0;
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x02");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "BSM{\"typeid\":\"BSM\",\"payload\":\"");
	
	//Find length of message
	length = pktData->length;
	for(counter=0;counter<length;counter++) {
		uint8_t tmp = pktData->contents[counter];
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "%.2X", tmp);
	}
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\",\"time\":\"");

	time(&msgtime);
	struct tm *utctime = gmtime(&msgtime);
	len += strftime(ui_msg + len, UI_MSG_LENGTH - len, "%FT%Xz", utctime);

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\"}");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x03");

//	printf("Sending Message: %s\n",ui_msg);
	sendMessageToAllClients(ui_msg);

	pthread_mutex_unlock(&ui_msg_lock);
}

//TIM Request
void sendTRQ() {
	pthread_mutex_lock(&ui_msg_lock);

    VehicleState currentState;
	qwarnGetCurrentVehicleState(&currentState);
	
	char* saveptr = NULL;

	if (currentState.roadwayId[0] != '\0')
	{
		int len = 0;
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x02");
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "TRQ{\"typeid\":\"TRQ\",\"roadwayid\":");
	
		strncpy(ui_tmp, currentState.roadwayId, UI_TMP_LENGTH);
		strtok_r(ui_tmp,"\n", &saveptr);
		
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\"%s\"", ui_tmp);

		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"mm\":%f}", currentState.mileMarker);
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x03");

		printf("Sending Message: %s\n",ui_msg);
		sendMessageToAllClients(ui_msg);
	}	

	pthread_mutex_unlock(&ui_msg_lock);
}

void sendDIA() {

//	 uiprintf("sendDIA Speed:%6.2f Heading:%6.2f", curr_gps.speed, curr_gps.course);

	pthread_mutex_lock(&ui_msg_lock);	
	
    VehicleState currState = {0};
	qwarnGetCurrentVehicleState(&currState);

	int len = 0;
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x02");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "DIA{\"typeid\":\"DIA\"");

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"gps\":");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, GPS_Fix_Flag ? "true" : "false");
	
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rsu\":");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, RSU_Flag ? "true" : "false");
	
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rsurssi\":%d", RSUrssi);
	
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"qState\":");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, qwarnIsQueued() ? "true" : "false");

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"mmarker\":%f", currState.mileMarker);

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rvbsm\":%d", RVbsm);

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rssi\":%d", rssi);

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"heading\":%d", (int) curr_gps.course);
	
	if(btveh.speed != 8191) {
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"speed\":%f", (double) btveh.speed);
	} else {
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"speed\":%f", (double) curr_gps.speed);
	}

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rvdist\":%f", qwarnGetDistanceToClosestRv());
	
	if(currState.roadwayId[0] == 0)
	{
	    len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"road\":\"%s\"", "Unknown");
	}
	else
	{
	    // Temporary super hacky strtok replacement	
	    char* roadwayName = currState.roadwayId;
	    while(roadwayName[0] != '\n')
	    {
	        roadwayName++;
	    }
	    roadwayName++;
	    
	    len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"road\":\"%s\"", roadwayName);
	}

    /*
	strncat(ui_msg,",\"road\":\"", UI_MSG_LENGTH);
	
	if(currState.roadwayId[0] != '\0') {
		strncpy(ui_tmp, currState.roadwayId, UI_TMP_LENGTH);
		char *tok = strtok(ui_tmp, "\n");
		tok = strtok(NULL, "");

		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"road\":\"%s\"", tok);
	} else {
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"road\":\"Unknown\"");
	}
	*/

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"rvcount\":%d", qwarnGetNumberOfCvs());

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"version\":\"%s\"", VERSION);
	
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, ",\"timrelay\":%lu", timprocGetRelayCount());

	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "}\x03");

	printf("Sending Message: %s\n",ui_msg);
	sendMessageToAllClients(ui_msg);

	pthread_mutex_unlock(&ui_msg_lock);
}


static time_t alertTime_SpdHarm;
static time_t alertTime_QWarn;
static time_t alertTime_TmeQWarn;

static SpdHarmAlert alert_lastSpdHarm;
static QWarnAlert alert_lastQWarn;

//Generate alert messages based on information stored in TIM
void sendAlert(AlertMessage *pMessage) {    
	pthread_mutex_lock(&ui_msg_lock);

	char *data, *tmp, *saveptr;
	
	tmp = strdup(pMessage->message);
	data = strtok_r(tmp,",", &saveptr);

	while(data != NULL) {
		// Speed HARM
		if(strcmp(data,"S") == 0 || strcmp(data,"s") == 0) {
			SpdHarmAlert alert;

			//SPEED
			data = strtok_r(NULL,",", &saveptr);
			alert.speed = atoi(data);

			//TXT
			data = strtok_r(NULL,",", &saveptr);
			strncpy(alert.text, data, ALERT_TEXT_LENGTH);

			time_t currentTime;
			time(&currentTime);
			if (strcmp(alert.text, alert_lastSpdHarm.text) || alert.speed != alert_lastSpdHarm.speed) //If would cause TTS
			{
				if (difftime(currentTime, alertTime_SpdHarm) < 3.0)
				{
					//Do Nothing
				}
				else
				{
					//Setup new alert and reset the time
					memcpy(&alert_lastSpdHarm, &alert, sizeof(SpdHarmAlert));
					time(&alertTime_SpdHarm);
				}
			}
			else
			{
				memcpy(&alert_lastSpdHarm, &alert, sizeof(SpdHarmAlert));
			}

			snprintSpdHarmAlert(ui_msg, UI_MSG_LENGTH, &alert_lastSpdHarm);
			printf("Sending Message: %s\n",ui_msg);
			sendMessageToAllClients(ui_msg);

		}
		// In-Q
		else if(strcmp(data,"Q") == 0 || strcmp(data,"q") == 0) {
			time(&alertTime_TmeQWarn);

			QWarnAlert alert;
			alert.distboq = -1;

			//LENGTH
			data = strtok_r(NULL,",", &saveptr);

			//TIME
			data = strtok_r(NULL,",", &saveptr);
			alert.time_remaining = (atoi(data)) * (1.0 - pMessage->completion_pct);

			//TXT
			data = strtok_r(NULL,",", &saveptr);
			strncpy(alert.text, data, ALERT_TEXT_LENGTH);
			
			alert.distfoq = (pMessage->length_m / 1609.344) * (1.0 - pMessage->completion_pct);

			time_t currentTime;
			time(&currentTime);
			if (alert_lastQWarn.distboq >= 0 || strcmp(alert.text, alert_lastQWarn.text)) //If would cause TTS
			{
				if (difftime(currentTime, alertTime_QWarn) < 2.5)
				{
					//Do Nothing
				}
				else
				{
					//Setup new alert and reset the time
					time(&alertTime_QWarn);
					memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
				}
			}
			else
			{
				memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
			}

			snprintQWarnAlert(ui_msg, UI_MSG_LENGTH, &alert_lastQWarn);
			printf("Sending Message: %s\n",ui_msg);
			sendMessageToAllClients(ui_msg);

		}
		// Q-Ahead
		else if (strcmp(data,"A") == 0 || strcmp(data,"a") == 0) {
			time(&alertTime_TmeQWarn);

			QWarnAlert alert;
			alert.distfoq = -1;

			//LENGTH
			data = strtok_r(NULL,",", &saveptr);

			//TIME
			data = strtok_r(NULL,",", &saveptr);

			//TXT
			data = strtok_r(NULL,",", &saveptr);
			strncpy(alert.text, data, ALERT_TEXT_LENGTH);

			alert.distboq = (pMessage->length_m / 1609.344) * (1.0 - pMessage->completion_pct);

			time_t currentTime;
			time(&currentTime);
			if (alert_lastQWarn.distfoq >= 0 || strcmp(alert.text, alert_lastQWarn.text)) //If would cause TTS
			{
				if (difftime(currentTime, alertTime_QWarn) < 3.5)
				{
					//Do Nothing
				}
				else
				{
					//Setup new alert and reset the time
					time(&alertTime_QWarn);
					memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
				}
			}
			else
			{
				memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
			}

			snprintQWarnAlert(ui_msg, UI_MSG_LENGTH, &alert_lastQWarn);
			printf("Sending Message: %s\n",ui_msg);
			sendMessageToAllClients(ui_msg);

   		}
   		//V2V Queue Ahead
   		else if (strcmp(data,"V") == 0 || strcmp(data,"v") == 0) {
			time_t currentTime;
			time(&currentTime);
   			if (difftime(currentTime, alertTime_TmeQWarn) < 10)
   			{
   				free(tmp);
   				pthread_mutex_unlock(&ui_msg_lock);
   				return;
   			}

			QWarnAlert alert;
			alert.distfoq = -1;

			//LENGTH
			data = strtok_r(NULL,",", &saveptr);

			//TIME
			data = strtok_r(NULL,",", &saveptr);

			//TXT
			data = strtok_r(NULL,",", &saveptr);
			strncpy(alert.text, data, ALERT_TEXT_LENGTH);

			alert.distboq = (pMessage->length_m / 1609.344) * (1.0 - pMessage->completion_pct);

			if (alert_lastQWarn.distfoq >= 0 || strcmp(alert.text, alert_lastQWarn.text)) //If would cause TTS
			{
				if (difftime(currentTime, alertTime_QWarn) < 3.5)
				{
					//Do Nothing
				}
				else
				{
					//Setup new alert and reset the time
					time(&alertTime_QWarn);
					memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
				}
			}
			else
			{
				memcpy(&alert_lastQWarn, &alert, sizeof(QWarnAlert));
			}

			snprintQWarnAlert(ui_msg, UI_MSG_LENGTH, &alert_lastQWarn);
			printf("Sending Message: %s\n",ui_msg);
			sendMessageToAllClients(ui_msg);

   		}
		data = strtok_r(NULL,",", &saveptr);
	}
	
	free(tmp);

	pthread_mutex_unlock(&ui_msg_lock);
}

void sendClrSpdHarmUIAlert() {    
	pthread_mutex_lock(&ui_msg_lock);

	SpdHarmAlert alert;
	alert.speed = -1;

	snprintSpdHarmAlert(ui_msg, UI_MSG_LENGTH, &alert);
	printf("Sending Message: %s\n",ui_msg);
	sendMessageToAllClients(ui_msg);

	pthread_mutex_unlock(&ui_msg_lock);
}

void sendClrQWarnUIAlert() {
	pthread_mutex_lock(&ui_msg_lock);

	QWarnAlert alert;
	alert.distboq = -1;
	alert.distfoq = -1;

	snprintQWarnAlert(ui_msg, UI_MSG_LENGTH, &alert);
	printf("Sending Message: %s\n",ui_msg);
	sendMessageToAllClients(ui_msg);

	pthread_mutex_unlock(&ui_msg_lock);
}

/*void parseMessage(char *msg, int msgLength)
{
	printf("MESSAGE: %s\n",msg);

	if(strstr(msg,"TIM") == msg) {
		TravelerInformation_t *tim = (TravelerInformation_t *)calloc(1, sizeof(TravelerInformation_t));
		parseTIMData(tim, msg);
		qwarnOnRecvMessage(tim, WSMMSG_TIM);

		
	} else if(strstr(msg,"WTR") == msg) {
		parseWeatherData(msg,wtrData);
		memcpy(&btwtr, wtrData, sizeof(btwtr));
		btveh.external_air_temperature = -1000;
		btveh.barometric_pressure = -1;

	} else if(strstr(msg,"VEH") == msg) {
		parseVEHData(msg,vehData);
		memcpy(&btveh, vehData,sizeof(btveh));
	}
}*/

void sendTIMtoUI(WSMData *pktData) {

	pthread_mutex_lock(&ui_msg_lock);

	int counter, length;

	int len = 0;
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x02");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "TIM{\"typeid\":\"TIM\",\"payload\":\"");
	
	//Find length of message
	length = pktData->length;
	for(counter=0;counter<length;counter++) {
		uint8_t tmp = pktData->contents[counter];
		len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "%.2X", tmp);
	}
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\"}");
	len += snprintf(ui_msg + len, UI_MSG_LENGTH - len, "\x03");

	printf("Sending Message: %s\n",ui_msg);
	sendMessageToAllClients(ui_msg);
	pthread_mutex_unlock(&ui_msg_lock);
}

void btUpdateGpsData(GPSData *gpsData) {
//    uiprintf("btUpdateGpsData Speed:%6.2f Heading:%6.2f", gpsData->speed, gpsData->course);
//    printf("btUpdateGpsData Speed:%6.2f Heading:%6.2f\n", gpsData->speed, gpsData->course);

	curr_gps.latitude = gpsData->latitude;	
	curr_gps.longitude = gpsData->longitude;
	curr_gps.course = gpsData->course;
	curr_gps.speed = gpsData->speed;
	curr_gps.altitude = gpsData->altitude;
}

void OnRecvAlertMsg(AlertMessage *pMessage) {    
	uiprintf("Got Alert: ");
	uiprintf("%s", pMessage->message);
	uiprintf("len = %.2f m; end = %.2f m\n", pMessage->length_m, pMessage->completion_pct * pMessage->length_m);
	sendAlert(pMessage);
	pthread_mutex_lock(&ui_msg_lock);
	pthread_mutex_unlock(&ui_msg_lock);
}


void initializeWtr(wtr *wtrData) {
	wtrData->temp = -1001;
	wtrData->pres = 0;
	wtrData->hum = -1;
}

void parseWeatherData(char *msg, wtr* wtrData) {   
    char *saveptr = NULL; 
	char json[250];
	strcpy(json,msg);
	json[strlen(msg)] = '\0';
	char *data = strtok_r(json,"{:", &saveptr);
	while(data != NULL) {
		if(strcmp(data,"\"pres\"")== 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			wtrData->pres = atof(data);
		} else if(strcmp(data,"\"temp\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			wtrData->temp = atof(data);
		} else if(strcmp(data,"\"hum\"")== 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			wtrData->hum = atof(data);
		}	
		data = strtok_r(NULL,":,}", &saveptr);
	}
}

void initializeVEH(veh *vehData) {
	vehData->time = -1;
	vehData->location = -1;
	vehData->speed = 8191;
	vehData->heading = curr_gps.course * 80;
	vehData->barometric_pressure = 0;
	vehData->lateral_acceleration = 2001/100;
	vehData->longitudinal_acceleration = 2001/100;
	vehData->yaw_rate = -32768;
	vehData->rate_of_change_of_steering_wheel = -1;
	vehData->steering_wheel_angle = 127;
	vehData->brake_status = 0x0000;
	vehData->brake_boost_status = BrakeBoostApplied_unavailable;
	vehData->brake_applied = BrakeAppliedStatus_allOff;
	vehData->auxiliary_brake_status = AuxiliaryBrakeStatus_unavailable;
	vehData->impact_sensor_status = -1;
	vehData->anti_lock_braking_status = AntiLockBrakeStatus_unavailable;
	vehData->external_air_temperature = -1001;
	vehData->wiper_status = WiperStatusFront_unavailable;
	vehData->headlight_status = -1;
	vehData->traction_control_status = TractionControlState_unavailable;
	vehData->stability_control_status = StabilityControlStatus_unavailable;
	vehData->differential_wheel_speed = -1;
	vehData->rpm = -1;
	vehData->throttle = -1;
	vehData->maf = -1;
}

void parseVEHData(char *msg, veh* vehData) {
    char* saveptr = NULL;
	char json[250];
	initializeVEH(vehData);
	strcpy(json,msg);
	json[strlen(msg)] = '\0';
	char *data = strtok_r(json,"{:", &saveptr);
	while(data != NULL) {
		if(strcmp(data,"\"rpm\"")== 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->rpm = atof(data);
		} else if(strcmp(data,"\"spd\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->speed = atoi(data) * 0.277777777778;
		} else if(strcmp(data,"\"pres\"")== 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->barometric_pressure = (((atol(data)*10) /580) + .5);
		} else if(strcmp(data,"\"maf\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->maf = atof(data);
		} else if(strcmp(data,"\"airtemp\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->external_air_temperature = atol(data);
		} else if(strcmp(data,"\"throttle\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->throttle = atol(data);
		} else if(strcmp(data,"\"time\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->time = atoi(data);
		}  else if(strcmp(data,"\"location\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->location = atoi(data);
		} else if(strcmp(data,"\"heading\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->heading = atoi(data);
		}else if(strcmp(data,"\"lataccel\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->lateral_acceleration = atof(data);
		} else if(strcmp(data,"\"longaccel\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->longitudinal_acceleration = atof(data);
		}  else if(strcmp(data,"\"yaw_rate\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->yaw_rate = atol(data);
		} else if(strcmp(data,"\"rate_of_change_of_steering_wheel\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->rate_of_change_of_steering_wheel = atol(data); 
		} else if (strcmp(data, "\"steerangle\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr);
			vehData->steering_wheel_angle = atol(data);
		} else if(strcmp(data,"\"brakeposition\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->brake_status = atoi(data);
		} else if(strcmp(data,"\"impact_sensor_status\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->impact_sensor_status = atoi(data);
		} else if(strcmp(data,"\"anti_lock_braking_status\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->anti_lock_braking_status = atoi(data);
		} else if(strcmp(data,"\"wipers\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->wiper_status = atoi(data);
		} else if(strcmp(data,"\"headlight_status\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->headlight_status = atoi(data);
		} else if(strcmp(data,"\"traction_control_status\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->traction_control_status = atoi(data);
		} else if(strcmp(data,"\"differential_wheel_speed\"") == 0) {
			data = strtok_r(NULL,":,}", &saveptr); 
			vehData->differential_wheel_speed = atoi(data);
		}
		data = strtok_r(NULL,":,}", &saveptr);
	}
}
void parseTIMData(TravelerInformation_t * tim, char *msg) {
	asn_codec_ctx_t *opt_codec_ctx = 0;
	unsigned char bytearray[TIM_LEN];
	char json[TIM_LEN];
	int i,bytecount = 0;
	strcpy(json,msg);
	
	char* saveptr = NULL;
	
	char *data = strtok_r(json,"{:\"", &saveptr);
	while(data != NULL) {
		if(strcmp(data,"\"payload\"") == 0) {
			data = strtok_r(NULL,"\",}", &saveptr);
			for(i = 0; i < strlen(data)/2; i++) {
				sscanf(data+(2*i),"%2hhx",&bytearray[bytecount]);
				bytecount++;
			}
			ber_decode(opt_codec_ctx, &asn_DEF_TravelerInformation, (void **)&tim, bytearray, bytecount);
			
			//asn_fprint(stdout, &asn_DEF_TravelerInformation, tim);
			
		}
		data = strtok_r(NULL,":,}", &saveptr);
	}
}

void uiprintf(const char* format, ... ) {
	// If configuration has console turned off return
	if(!cfGetConfigFile()->enableAndroidConsole) return;

	pthread_mutex_lock(&ui_msg_lock);

	static char msg[1000];
	
	va_list args;
	va_start( args, format );
	vsnprintf(msg, 1000, format, args); 
	va_end( args );

	ui_msg[0] = 0x02;
	snprintf(ui_msg + 1, UI_MSG_LENGTH - 1, "CON{\"typeid\":\"CON\",\"msg\":\"%s\"}\x03", msg);
	sendMessageToAllClients(ui_msg);

	pthread_mutex_unlock(&ui_msg_lock);
}

void snprintSpdHarmAlert(char *msg_buff, int buff_len, SpdHarmAlert *alert) {
	char tmpBuff[25];

	strncpy(msg_buff, "\x02", buff_len);
	strncat(msg_buff, "SHA{\"typeid\":\"SHA\",\"text\":\"", buff_len);
	strncat(msg_buff, alert->text, buff_len);
	strncat(msg_buff, "\",\"speed\":", buff_len);
	snprintf(tmpBuff, 25, "%d", alert->speed);
	strncat(msg_buff, tmpBuff, buff_len);	
	strncat(msg_buff, "}\x03", buff_len);
}

void snprintQWarnAlert(char *msg_buff, int buff_len, QWarnAlert *alert) {
	char tmp_buff[25];

	strncpy(msg_buff, "\x02", buff_len);
	strncat(msg_buff, "QWA{\"typeid\":\"QWA\",\"text\":\"", buff_len);
	strncat(msg_buff, alert->text, buff_len);
	strncat(msg_buff, "\"", buff_len);

	if (alert->distboq >= 0)
	{
		strncat(msg_buff, ",\"distboq\":", buff_len);
		snprintf(tmp_buff, 25, "%f", alert->distboq);
		strncat(msg_buff, tmp_buff, buff_len);
	}

	if (alert->distfoq >= 0)
	{
		strncat(msg_buff, ",\"distfoq\":", buff_len);
		snprintf(tmp_buff, 25, "%f", alert->distfoq);
		strncat(msg_buff, tmp_buff, buff_len);
		strncat(msg_buff, ",\"time\":", buff_len);
		snprintf(tmp_buff, 25, "%d", alert->time_remaining);
		strncat(msg_buff, tmp_buff, buff_len);
	}

	strncat(msg_buff, "}\x03", buff_len);
}
