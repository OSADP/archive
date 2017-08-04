#ifndef OBU_QWarnSystem_h
#define OBU_QWarnSystem_h

#include "BasicSafetyMessage.h"
#include "TravelerInformation.h"
#include "VehicleStateMap.h"
#include "TimProcessor.h"
#include "BsmProcessor.h"
#include "Lock.h"
#include "gpsc_probe.h"

extern int  qwarnStart();
extern void qwarnStop();
extern void qwarnJoin();
extern void qwarnDestroy();

extern void qwarnEnableLogging();
extern void qwarnDisableLogging();

extern void qwarnUpdateGpsData(GPSData* pData);
extern int  qwarnOnRecvMessage(void* pMessage, unsigned short messageType);
extern int  qwarnIsQueued();

extern int qwarnSetAlertCallback(OnRecvAlert_t pCallback);
extern TravelerInformation_t* qwarnGetNextRelayMsg();

extern int qwarnGetCurrentVehicleState(VehicleState* pState_out);
extern double qwarnGetDistanceToClosestRv();
extern int qwarnGetNumberOfCvs();
extern TimProcessor* qwarnGetTimProcessor();
extern BsmProcessor* qwarnGetBsmProcessor();


#endif
