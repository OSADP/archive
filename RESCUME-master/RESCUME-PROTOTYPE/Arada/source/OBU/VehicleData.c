/**
 * @file         VehicleData.c
 * @author       Joshua Branch, Veronica Hohe
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

#include "VehicleData.h"

#include <pthread.h>
#include <string.h>

#include "AuxiliaryBrakeStatus.h"
#include "BrakeBoostApplied.h"
#include "BrakeAppliedStatus.h"
#include "AntiLockBrakeStatus.h"
#include "TractionControlState.h"
#include "StabilityControlStatus.h"
#include "WiperStatus.h"

static VehicleData savedVehData = {0};
static pthread_mutex_t savedVehDataLock = PTHREAD_MUTEX_INITIALIZER;

void vehData_init(VehicleData *data)
{
	memset(data, 0, sizeof(VehicleData));
	data->speed = -1;
}

void vehData_set(VehicleData *data)
{
	pthread_mutex_lock(&savedVehDataLock);
	memcpy(&savedVehData, data, sizeof(VehicleData));
	pthread_mutex_unlock(&savedVehDataLock);
}

void vehData_get(VehicleData *out)
{
	pthread_mutex_lock(&savedVehDataLock);
	memcpy(out, &savedVehData, sizeof(VehicleData));
	pthread_mutex_unlock(&savedVehDataLock);
}

void vehData_lockForEdit(VehicleData *out)
{
	pthread_mutex_lock(&savedVehDataLock);
	memcpy(out, &savedVehData, sizeof(VehicleData));
}

void vehData_updateAndUnlock(VehicleData *data)
{
	memcpy(&savedVehData, data, sizeof(VehicleData));
	pthread_mutex_unlock(&savedVehDataLock);
}
