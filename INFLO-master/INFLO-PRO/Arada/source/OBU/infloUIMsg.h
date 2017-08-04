#ifndef	_infloUIMsg_H_
#define	_infloUIMsg_H_

#include "wave.h"
#include "QWarnController.h"
#include "AuxiliaryBrakeStatus.h"
#include "BrakeBoostApplied.h"
#include "BrakeAppliedStatus.h"
#include "AntiLockBrakeStatus.h"
#include "TractionControlState.h"
#include "StabilityControlStatus.h"

#define DIATIMEOUT 3
#define TRQTIMEOUT 10
#define MAX_V2I_BUNDLE_SIZE 5
#define POST_REQ_LEN 250
#define TIM_LEN 5000

#define VERSION "0.2.2"

typedef struct {
	long double temp;
	double pres;
	double hum;
} wtr;

typedef struct {
	int time;
	int location;
	int speed;
	int heading;
	long barometric_pressure;
	double lateral_acceleration;
	double longitudinal_acceleration;
	long yaw_rate;
	int rate_of_change_of_steering_wheel;
	long steering_wheel_angle;
	int brake_status;
	int brake_applied;
	int auxiliary_brake_status;
	int brake_boost_status;
	int impact_sensor_status;
	int anti_lock_braking_status;
	long external_air_temperature;
	int wiper_status;
	long headlight_status;
	int traction_control_status;
	int stability_control_status;
	int differential_wheel_speed;
	double rpm;
	long throttle;
	double maf;
} veh;

wtr btwtr;
veh btveh;

TravelerInformation_t btTIM;

int RVbsm;
int rssi;
extern int RSUrssi;
int RSU_Flag;
int GPS_Fix_Flag;
time_t lastDIAtime;

void sendBSMtoUI(WSMData *pktData);
void sendTRQ();
void sendDIA();


void sendAlert(AlertMessage *pMessage);
void sendClrSpdHarmUIAlert();
void sendClrQWarnUIAlert();


//void parseMessage(char *msg, int msgLength);
void btTIMMsg(WSMData *pktData, char *btMessage);
void addToV2IBundle(char *btMessage);
void sendTIMToUI(WSMData *pktData, char *btMessage);
void createPOSTRequest(WSMData *pktData, char *btMessage);
void sendPOSTRequest(WSMData *pktData);
void parseWeatherData(char *msg, wtr *wtrData);
void parseTIMData(TravelerInformation_t *tim, char *msg);
void initializeWTR(wtr *wtrData);
void initializeVEH(veh *vehData);

void uiprintf(const char* format, ... );
#endif
