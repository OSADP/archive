/**
 * @file         GpsUBloxDevice.c
 * @author       Joshua Branch
 * 
 * @copyright Copyright (c) 2014 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
 */


#include "GpsUBloxDevice.h"

#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <stdio.h>
#include <sys/socket.h>

#include "BtServer.h"

#include "Debug.h"

#define UUID "6f408520-f4ee-11e3-a3ac-0800200c9a66"
#define RFCOMM_CHAN 7

typedef struct {

	BtServer *btServer;

} UBloxDevice;

void *gpsUBloxDevice_run(void *arg);
void gpsUBloxDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length);

void gpsUBloxDevice_buildChecksum(uint8_t *data, int length);

void gpsUBloxDevice_init(GpsDevice *gps, const char *deviceName)
{
	if (!gps)
		return;

	gps->model = GpsDeviceModel_UBlox;
	
	gps->lock = lock_create();
	strncpy(gps->deviceName, deviceName, GPS_DEVICE_DEVICE_NAME_MAX_LENGTH);

	nmea_zero_INFO(&gps->nmeaInfo);
	nmea_parser_init(&gps->nmeaParser);

	gps->nmeaSentenceCallback = NULL;
	gps->threadId = 0;

	gps->func_run = gpsUBloxDevice_run;
	gps->func_injectRtcmData = gpsUBloxDevice_injectRtcmData;
	gps->func_setNmeaSentenceRate = gpsUBloxDevice_setNmeaSentenceRate;
}

void *gpsUBloxDevice_run(void *arg)
{
	GpsDevice *gps = (GpsDevice *) arg;
	assert(gps != NULL);

	BtServer *btServer = btServer_create();
	assert(btServer != NULL);
	btServer_start(btServer, UUID, RFCOMM_CHAN);
	btServer_setRxMessageCallback(btServer, (void (*)(void *, char *, int))gpsUBloxDevice_injectRtcmData, gps);

	const int MAX_BUFFER_LENGTH = 2000;
	char buffer[MAX_BUFFER_LENGTH];
	int bufferLength = 0;

	while(gps->enabled)
	{
		gps->gpsfd = open(gps->deviceName, O_RDWR);

		if (gps->gpsfd < 0)
		{
			DBG_ERR(DBGM_GPS, printf("GpsUBloxDevice '%s': - Could not open connection to GpsUBloxDevice '%s':\n", gps->deviceName, gps->deviceName));
			perror("GpsDevice");
			sleep(1);
			continue;
		}

		DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Opened connection to GpsUBloxDevice '%s':\n", gps->deviceName, gps->deviceName));
		
		//gpsUBloxDevice_setPowerManagement(gps);
		//sleep(6);
		//gpsUBloxDevice_loadDefaults(gps);
		//gpsUBloxDevice_setCfgRateInfo(gps, 1000);
		//gpsDevice_setNmeaSentenceRate(gps, NMEA_GPGSA, 1);
		//gpsDevice_setNmeaSentenceRate(gps, NMEA_GPGGA, 1);
		//gpsDevice_setNmeaSentenceRate(gps, NMEA_GPVTG, 1);
		//gpsDevice_setNmeaSentenceRate(gps, NMEA_GPRMC, 1);
		gpsUBloxDevice_setDynamicMode(gps, UBloxDynamicMode_Automotive);
		//gpsUBloxDevice_setStaticHoldThreshold(gps, 20);
		//gpsUBloxDevice_pollForNav5Info(gps);
		//gpsUBloxDevice_pollForCfgRateInfo(gps);
		//gpsUBloxDevice_pollForPowerManagementInfo(gps);
		//gpsUBloxDevice_pollForPowerMode(gps);

		while(gps->enabled)
		{
			int readLength = read(gps->gpsfd, buffer + bufferLength, MAX_BUFFER_LENGTH - bufferLength);
			if (readLength <= 0)
				continue;

			bufferLength += readLength;

			if (bufferLength == MAX_BUFFER_LENGTH)
			{
				bufferLength = 0;
				continue;
			}

			int msgLength = 0;

			if (buffer[0] == '$') //NMEA String
			{
				char *end;
				if ((end = strstr(buffer, "\r\n")))
				{
					msgLength = (end - buffer) + 2;

					int parseStatus = gpsDevice_parseNmeaSentence(gps, buffer, msgLength);

					if (strstr(buffer, "$GPGGA") == buffer)
						btServer_sendMessageToAllClients(btServer, buffer, msgLength);

					*end = '\0';
					if (parseStatus)
					{
						DBG(DBGM_GPS, printf("GpsUBloxDevice '%s': Received \"%s\"\n", gps->deviceName, buffer));
					}
					else
						DBG_ERR(DBGM_GPS, printf("GpsUBloxDevice '%s': Error parsing \"%s\"\n", gps->deviceName, buffer));
					
					if (gps->nmeaSentenceCallback && parseStatus)
						gps->nmeaSentenceCallback(buffer, strlen(buffer));

				}
			}
			else if ((uint8_t)buffer[0] == 0xB5 && (uint8_t)buffer[1] == 0x62)
			{
				int class = buffer[2];
				int id = buffer[3];
				int length = (buffer[4]) | ((int)buffer[5] << 8);

				char buf[1000];
				int bufLen = 0;
				int i;
				for(i = 0; i < length + 8; i++)
					bufLen += snprintf(buf + bufLen, sizeof(buf) - bufLen, "[%d]:0x%02x", i, (uint8_t)buffer[i]);

				bufLen += snprintf(buf + bufLen, sizeof(buf) - bufLen, "\n");

				DBG(DBGM_GPS, printf("GpsUBloxDevice '%s': Received: %s\"\n", gps->deviceName, buf));

				if (class == 0x06 && id == 0x24)
				{
					DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Received NAV5 Poll Response: %s\"\n", gps->deviceName, buf));
				}
				else if (class == 0x05 && id == 0x01 && (uint8_t)buffer[6] == 0x06 && (uint8_t)buffer[7] == 0x24)
				{
					//gpsUBloxDevice_pollForNav5Info(gps);
				}
				msgLength = length + 8;

			}
			else //Don't know what this message is, so just throw it all out
			{
				bufferLength = 0;
			}

			if (msgLength)
			{
				memcpy(buffer, buffer + msgLength, bufferLength - msgLength);
				bufferLength -= msgLength;
			}
		}

//		DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Closing connection to GpsUBloxDevice '%s':\n", gps->deviceName, gps->deviceName));

		close(gps->gpsfd);
	}

	btServer_stop(btServer);
	btServer_destroy(btServer);

	return NULL;
}

void gpsUBloxDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

//	DBG(DBGM_GPS, printf("GpsUBloxDevice '%s': Injecting RTCM Data (%d bytes)\n", gps->deviceName, length));

	write(gps->gpsfd, data, length);
}

void gpsUBloxDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	if (gps->model != GpsDeviceModel_UBlox)
		return;

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Nmea Sentence rate.\n", gps->deviceName));
	int uBloxSentenceType = -1;

	switch(sentenceType)
	{
		case NMEA_GPGGA:
			uBloxSentenceType = 0;
			break;
		case NMEA_GPGLL:
			uBloxSentenceType = 1;
			break;
		case NMEA_GPGSA:
			uBloxSentenceType = 2;
			break;
		case NMEA_GPGSV:
			uBloxSentenceType = 3;
			break;
		case NMEA_GPRMC:
			uBloxSentenceType = 4;
			break;
		case NMEA_GPVTG:
			uBloxSentenceType = 5;
			break;
	}

	if (uBloxSentenceType == -1)
	{
		DBG_WARN(DBGM_GPS, printf("GpsUBloxDevice '%s': Unknown NMEA Sentence type.\n", gps->deviceName));
		return;
	}

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x01, 0x03, 0x00, 0xF0, uBloxSentenceType, rate, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Writing Nmea Sentence rate.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Finished writing Nmea Sentence rate.\n", gps->deviceName));
}


void gpsUBloxDevice_pollForNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	if (gps->model != GpsDeviceModel_UBlox)
		return;

	int uBloxSentenceType = -1;

	switch(sentenceType)
	{
		case NMEA_GPGGA:
			uBloxSentenceType = 0;
			break;
		case NMEA_GPGLL:
			uBloxSentenceType = 1;
			break;
		case NMEA_GPGSA:
			uBloxSentenceType = 2;
			break;
		case NMEA_GPGSV:
			uBloxSentenceType = 3;
			break;
		case NMEA_GPRMC:
			uBloxSentenceType = 4;
			break;
		case NMEA_GPVTG:
			uBloxSentenceType = 5;
			break;
	}

	if (uBloxSentenceType == -1)
	{
		DBG_WARN(DBGM_GPS, printf("GpsUBloxDevice '%s': Unknown NMEA Sentence type.\n", gps->deviceName));
		return;
	}

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x01, 0x02, 0x00, 0xF0, uBloxSentenceType, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Polling for NMEA Sentence type Information.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));

}

void gpsUBloxDevice_pollForNav5Info(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x24, 0x00, 0x00, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Polling for Nav5 Information.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_pollForCfgRateInfo(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x08, 0x00, 0x00, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Polling for Measurement/Navigation Rate Information.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_setCfgRateInfo(GpsDevice *gps, uint16_t rate)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;
	
	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x08, 0x06, 0x00, 
		rate & 0xFF, (rate >> 8) & 0xFF, 0x01, 0x00, 0x01, 0x00,
		0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Measurement/Navigation Interval to %u ms\n", gps->deviceName, rate));
	write(gps->gpsfd, data, sizeof(data));
}

/*void gpsUBloxDevice_pollForPowerManagementInfo(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x3B, 0x00, 0x00, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Polling for Power Management Information.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_setPowerManagement(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x32, 0x18, 0x00, 
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe8, 0x03,
		0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x02, 0x00, 0x00, 0x00,
		0x00, 0x00 };
	uint8_t data2[] = { 0xB5, 0x62, 0x06, 0x3b, 0x2c, 0x00, 
		0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe8, 0x00,
		0x00, 0x00, 0x10, 0x27, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Power Management Position\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}*/

void gpsUBloxDevice_pollForPowerMode(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;
	
	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x11, 0x00, 0x00, 0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Polling for Power Mode.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_setDynamicMode(GpsDevice *gps, UBloxDynamicMode mode)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;
	
	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x24, 0x24, 0x00,
		0x01, 0x00, mode, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Dynamic mode to %d\n", gps->deviceName, mode));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_setStaticHoldThreshold(GpsDevice *gps, int cmPerSec)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;
	
	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x24, 0x24, 0x00,
		0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, cmPerSec, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

//	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Static Hold Threshold to %d cm/s\n", gps->deviceName, cmPerSec));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_loadDefaults(GpsDevice *gps)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;
	
	if (gps->model != GpsDeviceModel_UBlox)
		return;
	
	uint8_t data[] = { 0xB5, 0x62, 0x06, 0x09, 0x0C, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00,
		0x00, 0x00 };

	gpsUBloxDevice_buildChecksum(data, sizeof(data));

	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Loading Defaults\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
}

void gpsUBloxDevice_buildChecksum(uint8_t *data, int length)
{
	uint8_t checkA = 0;
	uint8_t checkB = 0;

	int i;
	for(i = 2; i < length - 2; i++)
	{
		checkA = checkA + data[i];
		checkB = checkB + checkA;
	}

	data[length - 2] = checkA;
	data[length - 1] = checkB;
}
