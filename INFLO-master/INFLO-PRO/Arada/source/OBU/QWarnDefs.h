#ifndef OBU_QWarnDefs_h
#define OBU_QWarnDefs_h

#include <string.h>

typedef unsigned char       uint8;
typedef unsigned short      uint16;
typedef unsigned long       uint32;
typedef unsigned long long  uint64;
typedef long                int32;
typedef short               int16;
typedef char                int8;

static const double PI = 3.14159265359;
static const double TWO_PI = 6.28318530718;
static const double PI_2 = 1.57079632679;
static const double PI_4 = 0.78539816339;
static const double THREE_PI_4 = 2.35619449019;
static const double THREE_PI_2 = 4.71238898038;

static const double DEG2RAD = 0.0174532925;
static const double RAD2DEG = 57.2957795;
static const double MILES2METERS = 1609.34;

static const uint16 BSM_BLOB1_SPEED_MASK               = 0x0FFF;
static const double BSM_BLOB1_SPEED_UNIT_CONV_M_S      = 0.02;
static const double BSM_BLOB1_HEADING_CONV_DEG         = 0.0125;
static const double BSM_BLOB1_GEO_POS_UNIT_CONV_DEG    = 10000000.0;

static const uint16 BSM_BLOB1_BRAKE_SYS_STATUS_APPLICATION_MASK    = 0x000F;
static const uint16 BSM_BLOB1_BRAKE_SYS_STATUS_UNAVAILABLE_MASK    = 0x0010;
static const uint16 BSM_BLOB1_BRAKE_SYS_STATUS_SPARE_BIT_MASK      = 0x0020;

typedef struct BsmBlob1
{
    uint8       msgCnt;
    uint32      id;
    uint16      secMark;
    int32       lat;
    int32       lon;
    uint16      elev;
    uint8       accuracy[4];
    uint16      speed;
    uint16      heading;
    uint8       steeringAngle;
    uint8       accelSet[7];
    uint16      brakeSystemStatus;
    uint8       vehicleSize[3];    

}__attribute__ ((packed)) BsmBlob1;

static inline void strBufferCopy(char* pDestBuffer, const char* pSourceBuffer, size_t destBufferSize)
{
    strncpy(pDestBuffer, pSourceBuffer, destBufferSize);
    pDestBuffer[destBufferSize - 1] = 0;
    
    return;
}

#endif
