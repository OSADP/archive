#include "ConfigFile.h"

#include <stdio.h>
#include <stdbool.h>

#define LINE_BUFFER_SIZE 1024
#define KEY_BUFFER_SIZE 512
#define VALUE_BUFFER_SIZE 512

static ConfigFile* gpConfigFile = NULL;

static const char* ENABLE_GPS_SPOOFING          = "EnableGpsSpoofing";
static const char* GPS_DATA_FIELDS              = "GpsData";
static const char* APP_VEHICLE_ID		= "AppVehicleId";
static const char* RSU_CELLNET_TRANSITION_RSSI  = "RsuCellNetTransitionRSSI";
static const char* RSU_NUM			= "RsuNum";
static const char* RSU_MAC_ADDR			= "RsuMacAddr";
static const char* MAP_DIR                      = "MapDir";
static const char* MAX_ROAD_DIST                = "MaximumRoadDist_m";
static const char* DIR_SIMILARITY_TOLER         = "DirSimilarityTolerance";
static const char* QUEUED_SPEED_THRESHOLD_M_S   = "QueuedSpeedThreshold_m_s";
static const char* QUEUED_DIST_THRESHOLD_M      = "QueuedDistanceThreshold_m";
static const char* QUEUED_RESET_TIME_S          = "QueuedResetTime_s";
static const char* HEADING_LOCK_SPEED          	= "HeadingLockSpeed";
static const char* ENABLE_ANDROID_CONSOLE      	= "EnableAndroidConsole";

int numOfRsus;

char* trimWhitespace(char* str)
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

void cfParseLine(char* pLine)
{
    pLine = trimWhitespace(pLine);

    int separatorPos = 0;
    int lineLength = strlen(pLine);
    
    if(lineLength < 3)
    {
        return;
    }
    
    if(pLine[0] == '#')
    {
        return;
    }
    
    while(separatorPos != lineLength && pLine[separatorPos] != '=')
    {
        separatorPos++;
    }    
    pLine[separatorPos] = '\0';
    
    char* key = trimWhitespace(pLine);
    char* value = trimWhitespace(&pLine[separatorPos+1]);
    bool success = true;
    
    printf("Cfg: k=%s, v=%s\n", key, value);
    
    if(strcmp(key, ENABLE_GPS_SPOOFING) == 0)
    {
        if(strcmp(value, "true") == 0)
        {
            gpConfigFile->useSpoofGpsData = true;
        }
        else
        {
            gpConfigFile->useSpoofGpsData = false;
        }
    }
    else if(strcmp(key, GPS_DATA_FIELDS) == 0)
    {
        int count = sscanf(
	        value, 
	        "%lf%*c %lf%*c %lf%*c %lf%*c %lf%*c", 
	        &gpConfigFile->spoofGpsData.latitude,
	        &gpConfigFile->spoofGpsData.longitude,
	        &gpConfigFile->spoofGpsData.course,
	        &gpConfigFile->spoofGpsData.speed,
	        &gpConfigFile->spoofGpsData.altitude
        );
        
        gpConfigFile->spoofGpsData.fix = 1;
        
        success = (count == 5);
    }
    else if(strcmp(key, APP_VEHICLE_ID) == 0)
    {
    	success = (sscanf(value, "%x", &gpConfigFile->appVehicleId) == 1);
    	if (success && gpConfigFile->appVehicleId != 0)
    	{
    		printf("Setting Lock on Vehicle ID %X\n", gpConfigFile->appVehicleId);
    		gpConfigFile->appVehicleIdLock = VehicleIdLock_permanent;
    	}
    }
    else if(strcmp(key, RSU_CELLNET_TRANSITION_RSSI) == 0)
    {
        int count = sscanf(value, "%i", &gpConfigFile->rsuCellNetTransRSSI);
        
        success = (count == 1);
    }
    else if(strcmp(key, RSU_NUM) == 0)
    {
	int count = sscanf(value, "%i", &gpConfigFile->rsuNum);
	success = (count == 1);
    }
    else if(strcmp(key, RSU_MAC_ADDR) == 0)
    {
	int count = sscanf(value, "%s", &gpConfigFile->rsuMacAddr[numOfRsus]);
	success = (count == 1);
	numOfRsus++;
    }
    else if(strcmp(key, MAP_DIR) == 0)
    {
        strcpy(gpConfigFile->mapDir, value);
    }
    else if(strcmp(key, MAX_ROAD_DIST) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->maximumRoadDistance);
        
        success = (count == 1);
    }
    else if(strcmp(key, DIR_SIMILARITY_TOLER) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->dirSimilarityTolerance);
        
        success = (count == 1);
    }
    else if(strcmp(key, QUEUED_SPEED_THRESHOLD_M_S) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->queuedSpeedThreshold_m_s);
        
        success = (count == 1);
    }
    else if(strcmp(key, QUEUED_DIST_THRESHOLD_M) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->queuedDistanceThreshold_m);
        
        success = (count == 1);
    }
    else if(strcmp(key, QUEUED_RESET_TIME_S) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->queuedResetTime_s);
        
        success = (count == 1);
    }
    else if(strcmp(key, HEADING_LOCK_SPEED) == 0)
    {
        int count = sscanf(value, "%lf", &gpConfigFile->headingLockSpeed);

        success = (count == 1);
    }
    else if(strcmp(key, ENABLE_ANDROID_CONSOLE) == 0)
    {
        if(strcmp(value, "true") == 0)
        {
            gpConfigFile->enableAndroidConsole = true;
        }
        else
        {
            gpConfigFile->enableAndroidConsole = false;
        }
    }
    else
    {
        printf("ConfigFile: Unknown field %s\n", key);
    }
    
    if(!success)
    {        
        printf("Error parsing %s, %s\n", key, value);
    }
    
    return;
}

void cfSetDefaults()
{
    gpConfigFile->useSpoofGpsData = false;
    gpConfigFile->rsuCellNetTransRSSI = 40;
    gpConfigFile->maximumRoadDistance = 40.0;
    gpConfigFile->dirSimilarityTolerance = 0.1;
    gpConfigFile->appVehicleId = rand() << 16 ^ rand();
    return;    
}

int cfInitialize(const char* pFilename)
{
    gpConfigFile = (ConfigFile*)malloc(sizeof(ConfigFile));
    
    cfSetDefaults();
    
    FILE* pFile = fopen(pFilename, "r");
    
    if(!pFile)
    {
        return false;
    }
    numOfRsus = 0;
    char buffer[LINE_BUFFER_SIZE];
    while(fgets(buffer, LINE_BUFFER_SIZE, pFile))
    {
        cfParseLine(buffer);
    }
    
    fclose(pFile);
    
    return true;
}

ConfigFile* cfGetConfigFile()
{
    return gpConfigFile;
}

void cfEdit(ConfigFile *out)
{
	memcpy(out, gpConfigFile, sizeof(ConfigFile));
}

void cfUpdate(ConfigFile *data)
{
	memcpy(gpConfigFile, data, sizeof(ConfigFile));
}
