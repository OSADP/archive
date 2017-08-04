#ifndef OBU_ConfigFile_h
#define OBU_ConfigFile_h

#include "gpsc_probe.h"

typedef enum {
	VehicleIdLock_noLock = 0,
	VehicleIdLock_temporary,
	VehicleIdLock_permanent
} VehicleIdLock;

typedef struct ConfigFile
{
    // GPS Spoofing
    int useSpoofGpsData;   
    GPSData spoofGpsData;
    
    VehicleIdLock appVehicleIdLock;
    uint32_t appVehicleId;

    // Q-Warn
    double queuedSpeedThreshold_m_s;
    double queuedDistanceThreshold_m;
    double queuedResetTime_s;
    
    // Other
    int rsuCellNetTransRSSI;
    
    // Map Data
    char mapDir[256];
    double maximumRoadDistance;
    double dirSimilarityTolerance;

    // Setting
    double headingLockSpeed;
    int enableAndroidConsole;

    char rsuMacAddr[18][18];
    int rsuNum;

} ConfigFile;

extern int          cfInitialize(const char* filename);
extern ConfigFile*  cfGetConfigFile();
extern void cfEdit(ConfigFile *out);
extern void cfUnlock(ConfigFile *data);

#endif
