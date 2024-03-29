/*************************************************************************
 * 
 * SCSC CONFIDENTIAL
 * __________________
 * 
 * Copyright (c) [2009] - [2012] 
 * SubCarrier System Corp. (SCSC) 
 * All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is, and remains,
 * the property of SubCarrier System Corp. (SCSC) and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to SubCarrier System Corp. (SCSC)
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from SubCarrier System Corp. (SCSC).
 *
 *
 * This file is subject to the terms and conditions of use defined 
 * in the files 'LICENSE.rft' or 'LICENSE.pdf' which is part of this 
 * source code package.
 *
 * LIC: #Battelle_001_01_dbfff42a90727d02153511a33480572b#
 */

/*
 * Generated by asn1c-0.9.22 (http://lionet.info/asn1c) [SCSCrev09]
 * From ASN.1 module "DSRC"
 * 	found in "DSRC_R36_Source.ASN"
 * 	`asn1c -S/skeletons`
 */

#ifndef	_Approach_H_
#define	_Approach_H_


#include <asn_application.h>

/* Including external dependencies */
#include "DescriptiveName.h"
#include "ApproachNumber.h"
#include <asn_SEQUENCE_OF.h>
#include <constr_SEQUENCE_OF.h>
#include <constr_SEQUENCE.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Forward declarations */
struct VehicleReferenceLane;
struct VehicleComputedLane;
struct SpecialLane;
struct BarrierLane;
struct CrosswalkLane;

//
// DCK modes
//
typedef	struct drivingLanes {
		A_SEQUENCE_OF(struct VehicleReferenceLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} drivingLanes;

typedef	struct computedLanes {
		A_SEQUENCE_OF(struct VehicleComputedLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} computedLanes;

typedef	struct trainsAndBuses {
		A_SEQUENCE_OF(struct SpecialLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} trainsAndBuses;

typedef	struct barriers {
		A_SEQUENCE_OF(struct BarrierLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} barriers;

typedef	struct crosswalks {
		A_SEQUENCE_OF(struct CrosswalkLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} crosswalks;
//
// DCK modes end
//

/* Approach */
typedef struct Approach {
	DescriptiveName_t	*name	/* OPTIONAL */;
	ApproachNumber_t	*id	/* OPTIONAL */;
	struct drivingLanes {
		A_SEQUENCE_OF(struct VehicleReferenceLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *drivingLanes;
	struct computedLanes {
		A_SEQUENCE_OF(struct VehicleComputedLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *computedLanes;
	struct trainsAndBuses {
		A_SEQUENCE_OF(struct SpecialLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *trainsAndBuses;
	struct barriers {
		A_SEQUENCE_OF(struct BarrierLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *barriers;
	struct crosswalks {
		A_SEQUENCE_OF(struct CrosswalkLane) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *crosswalks;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} Approach_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_Approach;

#ifdef __cplusplus
}
#endif

/* Define supported Utility Calls */
void Approach_Init (Approach_t* theObj);
//void Approach_ToBlob (Approach_t* theObj, char* theBlob);
//void Approach_FromBlob (Approach_t* theObj, char* theBlob);


/* Referred external types */
#include "VehicleReferenceLane.h"
#include "VehicleComputedLane.h"
#include "SpecialLane.h"
#include "BarrierLane.h"
#include "CrosswalkLane.h"

#endif	/* _Approach_H_ */
