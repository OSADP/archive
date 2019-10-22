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

#ifndef	_WorkZone_H_
#define	_WorkZone_H_


#include <asn_application.h>

/* Including external dependencies */
#include <asn_SEQUENCE_OF.h>
#include "ITIScodes.h"
#include "ITIStext.h"
#include <IA5String.h>
#include <constr_CHOICE.h>
#include <constr_SEQUENCE.h>
#include <constr_SEQUENCE_OF.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Dependencies */
typedef enum item_PRwz {
	item_PR_NOTHING_wz,	/* No components present */
	item_PR_itis_wz,
	item_PR_text_wz
} item_PRwz;

//
// DCK added struc
//
typedef	struct item {
			item_PRwz present;
			union choice {
				ITIScodes_t	 itis;
				ITIStext_t	 text;
			} choice;
			
			/* Context for parsing across buffer boundaries */
			asn_struct_ctx_t _asn_ctx;
		} item;

typedef struct Member_wz {
	    struct item;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} Member_wz;
//
// DCK edit end (and below)
//


/* WorkZone */
typedef struct WorkZone {
	A_SEQUENCE_OF(struct Member ) list;
	// DCK, added above to replace below
	//A_SEQUENCE_OF(struct Member {
	//	struct item {
	//		item_PRwz present;
	//		union item_u {
	//			ITIScodes_t	 itis;
	//			IA5String_t	 text;
	//		} choice;
	//		
	//		/* Context for parsing across buffer boundaries */
	//		asn_struct_ctx_t _asn_ctx;
	//	} item;
	//	
	//	/* Context for parsing across buffer boundaries */
	//	asn_struct_ctx_t _asn_ctx;
	//} ) list;
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} WorkZone_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_WorkZone;

/* Define supported Utility Calls */
void WorkZone_Init (WorkZone_t* theObj);
//void WorkZone_ToBlob (WorkZone_t* theObj, char* theBlob);
//void WorkZone_FromBlob (WorkZone_t* theObj, char* theBlob);


#ifdef __cplusplus
}
#endif

#endif	/* _WorkZone_H_ */