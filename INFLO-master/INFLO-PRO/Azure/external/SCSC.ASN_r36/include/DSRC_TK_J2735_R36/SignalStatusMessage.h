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

#ifndef	_SignalStatusMessage_H_
#define	_SignalStatusMessage_H_


#include <asn_application.h>

/* Including external dependencies */
#include "DSRCmsgID.h"
#include "MsgCount.h"
#include "IntersectionID.h"
#include "IntersectionStatusObject.h"
#include "TransitStatus.h"
#include "SignalState.h"
#include <asn_SEQUENCE_OF.h>
#include <constr_SEQUENCE_OF.h>
#include <constr_SEQUENCE.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Forward declarations */
struct VehicleIdent;

//
// DCK mods
//
typedef struct priority {
		A_SEQUENCE_OF(SignalState_t) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} priority;

//typedef	struct VehicleIdent	*priorityCause	/* OPTIONAL */;

typedef struct prempt {
		A_SEQUENCE_OF(SignalState_t) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} prempt;

//typedef	struct VehicleIdent2	*preemptCause	/* OPTIONAL */;

//
// DCK mods end
//

/* SignalStatusMessage */
typedef struct SignalStatusMessage {
	DSRCmsgID_t	 msgID;
	MsgCount_t	 msgCnt;
	IntersectionID_t	 id;
	IntersectionStatusObject_t	 status;
	struct priority {
		A_SEQUENCE_OF(SignalState_t) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *priority;
	struct VehicleIdent	*priorityCause	/* OPTIONAL */;
	struct prempt {
		A_SEQUENCE_OF(SignalState_t) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} *prempt;
	struct VehicleIdent	*preemptCause	/* OPTIONAL */;
	TransitStatus_t	*transitStatus	/* OPTIONAL */;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} SignalStatusMessage_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_SignalStatusMessage;

/* Define supported Utility Calls */
void SignalStatusMessage_Init (SignalStatusMessage_t* theObj);
//void SignalStatusMessage_ToBlob (SignalStatusMessage_t* theObj, char* theBlob);
//void SignalStatusMessage_FromBlob (SignalStatusMessage_t* theObj, char* theBlob);


#ifdef __cplusplus
}
#endif

/* Referred external types */
#include "VehicleIdent.h"

#endif	/* _SignalStatusMessage_H_ */
