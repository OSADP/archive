/**
 * @file         GpsAdapter.h
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

#ifndef _GPSADAPTER_H_
#define _GPSADAPTER_H_

#include "GpsDevice.h"

GpsDevice *gps_getDefaultDevice(void);
void gps_enableSpoofDevice(char *file);
void gps_disableSpoofDevice();

void gps_destroy();

#endif
