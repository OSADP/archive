/**
 * @file         RescumeConfig.c
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

#include "RescumeConfig.h"
 
#include "Debug.h"

#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>
#include <ctype.h>
#include <pthread.h>

#define LINE_BUFFER_SIZE 1024
#define KEY_BUFFER_SIZE 512
#define VALUE_BUFFER_SIZE 512

void rescumeConfig_setDefaults();
char* rescumeConfig_trimWhitespace(char* str);
void rescumeConfig_parseLine(char* line);

static int rescumeConfigInitialized = 0;
static RescumeConfig rescumeConfig;
static pthread_mutex_t lock = PTHREAD_MUTEX_INITIALIZER;

void rescumeConfig_set(RescumeConfig *options)
{
	pthread_mutex_lock(&lock);

	rescumeConfigInitialized = 1;
	memcpy(&rescumeConfig, options, sizeof(RescumeConfig));

	pthread_mutex_unlock(&lock);
}

void rescumeConfig_get(RescumeConfig *out)
{
	pthread_mutex_lock(&lock);

	if (!rescumeConfigInitialized)
		rescumeConfig_setDefaults();

	memcpy(out, &rescumeConfig, sizeof(RescumeConfig));

	pthread_mutex_unlock(&lock);
}

void rescumeConfig_lockForEdit(RescumeConfig *out)
{
	pthread_mutex_lock(&lock);
	memcpy(out, &rescumeConfig, sizeof(RescumeConfig));
}

void rescumeConfig_updateAndUnlock(RescumeConfig *data)
{
	memcpy(&rescumeConfig, data, sizeof(RescumeConfig));
	pthread_mutex_unlock(&lock);
}

void rescumeConfig_loadFromFile(const char *filename)
{
	pthread_mutex_lock(&lock);

	if (!rescumeConfigInitialized)
		rescumeConfig_setDefaults();

	FILE* pFile = fopen(filename, "r");
	
	if(!pFile)
	{
		DBG_FATAL(printf("RescumeConfig: Unable to open configuration file '%s'\n", filename));
	}
	else
	{
		char buffer[LINE_BUFFER_SIZE];
		while(fgets(buffer, LINE_BUFFER_SIZE, pFile))
		{
			rescumeConfig_parseLine(buffer);
		}
		
		fclose(pFile);
	}

	pthread_mutex_unlock(&lock);
}

void rescumeConfig_setDefaults()
{
	RescumeConfig config = {0};
	rescumeConfig = config;

	rescumeConfig.gpsEnableSpoof = 0;
	rescumeConfig.gpsSpoofFile[0] = '\0';

	rescumeConfig.appMode = APPMODE_UNKNOWN;
	rescumeConfig.appVehicleIdLock = VehicleIdLock_noLock;
	rescumeConfig.appVehicleId = rand() << 16 ^ rand();

	rescumeConfig.uiEnableConsoleMessages = 0;
	rescumeConfig.uiDiaMaxRate = 2;
	rescumeConfig.uiNmeaMaxRate = 0.5;

	rescumeConfig.dsrcBsmMaxRate = 10;
	rescumeConfig.dsrcEvaMaxRate = 5;
	rescumeConfig.dsrcTimMaxRate = 2;
	rescumeConfig.dsrcAcmMaxRate = 2;
	
	rescumeConfig.oncomingStaleEvaTimeout = 15.0;
	//rescumeConfig.oncomingLegibilityDistance = 55;
	rescumeConfig.oncomingEmergencyDecelRate = 4.572;
	rescumeConfig.oncomingAggressiveDecelRate = 3.048;
	//rescumeConfig.oncomingIntentionalityMode = 0;
	rescumeConfig.oncomingMaxCalculationRate = 20;
	rescumeConfig.oncomingCollisionDriverPrtTime = 2.5;
	rescumeConfig.oncomingCollisionResponderPrtTime = 2.5;
	rescumeConfig.oncomingCollisionResetTime = 2.0;
	rescumeConfig.oncomingCollisionLaneWidth = 1.0;
	rescumeConfig.oncomingCollisionMinSpeed = 2.5;

	rescumeConfig.responderPreloadedTimFile[0] = '\0';
	rescumeConfig.responderStaleAcmTimeout = 3.0;
	rescumeConfig.responderMaxThreatMessageRate = 0.0;

	rescumeConfig.waveOptions.security = 0;
	rescumeConfig.waveOptions.psid = 32;
	rescumeConfig.waveOptions.txChannel = 172;
	rescumeConfig.waveOptions.txPower = 14;
	rescumeConfig.waveOptions.txPriority = 2;
	rescumeConfig.waveOptions.txRate = 3;
	rescumeConfig.waveOptions.wsmps = 0;

	rescumeConfig.dsrcMatchOptions.headingAcceptance = .85;
	rescumeConfig.dsrcMatchOptions.defaultLaneWidth = DSRC_DEFAULT_LANE_WIDTH;

	rescumeConfigInitialized = 1;
}

char* rescumeConfig_trimWhitespace(char* str)
{
	char *end;

	// Trim leading space
	while(isspace(*str)) str++;

	if(*str == 0)  // All spaces?
	return str;

	// Trim trailing space
	end = str + strlen(str) - 1;
	while(end > str && isspace(*end)) end--;

	// Write new null terminator
	*(end+1) = 0;

	return str;
}

void rescumeConfig_parseLine(char* line)
{
	line = rescumeConfig_trimWhitespace(line);

	int separatorPos = 0;
	int lineLength = strlen(line);
	
	if(lineLength < 3) return;
	
	if(line[0] == '#') return;
	
	while(separatorPos != lineLength && line[separatorPos] != '=')
	{
		separatorPos++;
	}    
	line[separatorPos] = '\0';
	
	char* key = rescumeConfig_trimWhitespace(line);
	char* value = rescumeConfig_trimWhitespace(&line[separatorPos+1]);
	bool success = 1;
	
	DBG(DBGM_CF, printf("RescumeConfig: key=%s, value=%s\n", key, value));
	
	if(strcmp(key, "GpsEnableSpoof") == 0)
	{
		rescumeConfig.gpsEnableSpoof = (strcmp(value, "true") == 0);
	}
	else if(strcmp(key, "GpsSpoofFile") == 0)
	{
		strncpy(rescumeConfig.gpsSpoofFile, value, sizeof(rescumeConfig.gpsSpoofFile));
	}
	else if(strcmp(key, "AppMode") == 0)
	{
		if(strcmp(value, "responder") == 0)
			rescumeConfig.appMode = APPMODE_RESPONDER;
		else
			rescumeConfig.appMode = APPMODE_ONCOMING;
	}
	else if(strcmp(key, "AppVehicleId") == 0)
	{
		success = (sscanf(value, "%x", &rescumeConfig.appVehicleId) == 1);
		if (success && rescumeConfig.appVehicleId != 0)
			rescumeConfig.appVehicleIdLock = VehicleIdLock_permanent;
	}

	else if(strcmp(key, "UiEnableConsoleMessages") == 0)
	{
		rescumeConfig.uiEnableConsoleMessages = (strcmp(value, "true") == 0);
	}
	else if(strcmp(key, "UiDiaMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.uiDiaMaxRate) == 1);
	}
	else if(strcmp(key, "UiNmeaMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.uiNmeaMaxRate) == 1);
	}

	else if(strcmp(key, "DsrcBsmMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcBsmMaxRate) == 1);
	}
	else if(strcmp(key, "DsrcEvaMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcEvaMaxRate) == 1);
	}
	else if(strcmp(key, "DsrcTimMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcTimMaxRate) == 1);
	}
	else if(strcmp(key, "DsrcAcmMaxRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcAcmMaxRate) == 1);
	}

	else if(strcmp(key, "OncomingStaleEvaTimeout") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.oncomingStaleEvaTimeout) == 1);
	}
	/*else if(strcmp(key, "OncomingLegibilityDistance") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.oncomingLegibilityDistance) == 1);
	}*/
	else if(strcmp(key, "OncomingEmergencyDecelRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.oncomingEmergencyDecelRate) == 1);
	}
	else if(strcmp(key, "OncomingAggressiveDecelRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.oncomingAggressiveDecelRate) == 1);
	}

	else if(strcmp(key, "ResponderPreloadedTimFile") == 0)
	{
		strncpy(rescumeConfig.responderPreloadedTimFile, value, sizeof(rescumeConfig.responderPreloadedTimFile));
	}
	else if(strcmp(key, "ResponderStaleAcmTimeout") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.responderStaleAcmTimeout) == 1);
	}
	else if(strcmp(key, "ResponderMaxThreatMessageRate") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.responderMaxThreatMessageRate) == 1);
	}

	else if(strcmp(key, "HeadingAcceptance") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcMatchOptions.headingAcceptance) == 1);
	}
	else if(strcmp(key, "DefaultLaneWidth") == 0)
	{
		success = (sscanf(value, "%lf", &rescumeConfig.dsrcMatchOptions.defaultLaneWidth) == 1);
	}

	else
	{
		DBG_WARN(DBGM_CF, printf("RescumeConfig: Unknown field %s\n", key));
	}
	
	if(!success)
	{
		DBG_WARN(DBGM_CF, printf("RescumeConfig: Error parsing %s, %s\n", key, value));
	}
	
	return;
}
