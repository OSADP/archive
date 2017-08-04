/**
 * @file         VehicleData.h
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

#ifndef _VEHICLEDATA_H_
#define _VEHICLEDATA_H_

#include <time.h>

typedef struct {
	struct timespec time;
	double speed;
	
	int location;
	int heading;
	long barometric_pressure;
	long lateral_acceleration;
	long longitudinal_acceleration;
	long yaw_rate;
	int rate_of_change_of_steering_wheel;
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
} VehicleData;


void vehData_init(VehicleData *data);
void vehData_set(VehicleData *data);
void vehData_get(VehicleData *data);
void vehData_lockForEdit(VehicleData *out);
void vehData_updateAndUnlock(VehicleData *data);

#endif
