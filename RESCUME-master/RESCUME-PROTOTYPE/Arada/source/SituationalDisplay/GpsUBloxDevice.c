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

#include "Debug.h"

void *gpsUBloxDevice_run(void *arg);
void gpsUBloxDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length);
void gpsUBloxDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate);

void gpsUBloxDevice_init(GpsDevice *gps, const char *deviceName)
{
	if (!gps)
		return;
	
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

	const int MAX_BUFFER_LENGTH = 300;
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

		gpsDevice_setNmeaSentenceRate(gps, NMEA_GPGSA, 5);

		int readLength;
		while((readLength = read(gps->gpsfd, buffer + bufferLength, MAX_BUFFER_LENGTH - bufferLength)) > 0 && gps->enabled)
		{
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

					*end = '\0';
					if (parseStatus)
						DBG(DBGM_GPS, printf("GpsUBloxDevice '%s': Received \"%s\"\n", gps->deviceName, buffer));
					else
						DBG_ERR(DBGM_GPS, printf("GpsUBloxDevice '%s': Error parsing \"%s\"\n", gps->deviceName, buffer));
					
					if (gps->nmeaSentenceCallback && parseStatus)
						gps->nmeaSentenceCallback(buffer, strlen(buffer));
				}
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

		DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Closing connection to GpsUBloxDevice '%s':\n", gps->deviceName, gps->deviceName));

		close(gps->gpsfd);
	}

	return NULL;
}

void gpsUBloxDevice_injectRtcmData(GpsDevice *gps, uint8_t *data, int length)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	write(gps->gpsfd, data, length);
}

void gpsUBloxDevice_setNmeaSentenceRate(GpsDevice *gps, uint8_t sentenceType, uint8_t rate)
{
	if (!gps)
		return;

	if (!gps->enabled)
		return;

	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Setting Nmea Sentence rate.\n", gps->deviceName));
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

	uint8_t checkA = 0;
	uint8_t checkB = 0;

	int i;
	for(i = 2; i < sizeof(data) - 2; i++)
	{
		checkA = checkA + data[i];
		checkB = checkB + checkA;
	}

	data[sizeof(data) - 2] = checkA;
	data[sizeof(data) - 1] = checkB;

	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Writing Nmea Sentence rate.\n", gps->deviceName));
	write(gps->gpsfd, data, sizeof(data));
	DBG_INFO(DBGM_GPS, printf("GpsUBloxDevice '%s': Finished writing Nmea Sentence rate.\n", gps->deviceName));
}
