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

#ifndef	_ProbeDataManagement_H_
#define	_ProbeDataManagement_H_


#include <asn_application.h>

/* Including external dependencies */
#include "DSRCmsgID.h"
#include "Sample.h"
#include "HeadingSlice.h"
#include "TxTime.h"
#include "Count.h"
#include "TermTime.h"
#include "TermDistance.h"
#include <constr_CHOICE.h>
#include "SnapshotTime.h"
#include "SnapshotDistance.h"
#include <asn_SEQUENCE_OF.h>
#include <constr_SEQUENCE_OF.h>
#include <constr_SEQUENCE.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Dependencies */
typedef enum term_PR {
	term_PR_NOTHING = 0,	/* No components present */
	term_PR_termtime = 1,	// DCK mods, added fixed values for debugging
	term_PR_termDistance = 2
} term_PR;
typedef enum snapshot_PR {
	snapshot_PR_NOTHING = 0,		/* No components present */
	snapshot_PR_snapshotTime = 1,	// DCK mods, added fixed values for debugging
	snapshot_PR_snapshotDistance = 2
} snapshot_PR;

/* Forward declarations */
struct VehicleStatusRequest;

//
// DCK mod
//
typedef struct Term_pd {
		term_PR present;
		union ProbeDataManagement__term_u {
			TermTime_t	 termtime;
			TermDistance_t	 termDistance;
		} choice;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} Term_pd;

typedef	struct Snapshot_pd {
		snapshot_PR present;
		union ProbeDataManagement__snapshot_u {
			SnapshotTime_t	 snapshotTime;
			SnapshotDistance_t	 snapshotDistance;
		} choice;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} Snapshot_pd;

typedef	struct DataElements_pd {
		A_SEQUENCE_OF(struct VehicleStatusRequest) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} DataElements_pd;
//
// DCK mod end
//


/* ProbeDataManagement */
typedef struct ProbeDataManagement {
	DSRCmsgID_t	 msgID;
	Sample_t	 sample;
	HeadingSlice_t	 directions;
	struct term {
		term_PR present;
		union ProbeDataManagement__term_u {
			TermTime_t	 termtime;
			TermDistance_t	 termDistance;
		} choice;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} term;
	struct snapshot {
		snapshot_PR present;
		union ProbeDataManagement__snapshot_u {
			SnapshotTime_t	 snapshotTime;
			SnapshotDistance_t	 snapshotDistance;
		} choice;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} snapshot;
	TxTime_t	 txInterval;
	Count_t	 cntTthreshold;
	struct dataElements {
		A_SEQUENCE_OF(struct VehicleStatusRequest) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} dataElements;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} ProbeDataManagement_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_ProbeDataManagement;

/* Define supported Utility Calls */
void ProbeDataManagement_Init (ProbeDataManagement_t* theObj);
//void ProbeDataManagement_ToBlob (ProbeDataManagement_t* theObj, char* theBlob);
//void ProbeDataManagement_FromBlob (ProbeDataManagement_t* theObj, char* theBlob);


#ifdef __cplusplus
}
#endif

/* Referred external types */
#include "VehicleStatusRequest.h"

#endif	/* _ProbeDataManagement_H_ */