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

#ifndef	_ShapePointSet_H_
#define	_ShapePointSet_H_


#include <asn_application.h>

/* Including external dependencies */
#include "LaneWidth.h"
#include "DirectionOfUse.h"
#include "NodeList.h"
#include <constr_SEQUENCE.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Forward declarations */
struct Position3D;

/* ShapePointSet */
typedef struct ShapePointSet {
	struct Position3D	*anchor	/* OPTIONAL */;
	LaneWidth_t	*laneWidth	/* OPTIONAL */;
	DirectionOfUse_t	*directionality	/* OPTIONAL */;
	NodeList_t	 nodeList;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} ShapePointSet_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_ShapePointSet;

/* Define supported Utility Calls */
void ShapePointSet_Init (ShapePointSet_t* theObj);
//void ShapePointSet_ToBlob (ShapePointSet_t* theObj, char* theBlob);
//void ShapePointSet_FromBlob (ShapePointSet_t* theObj, char* theBlob);


#ifdef __cplusplus
}
#endif

/* Referred external types */
#include "Position3D.h"

#endif	/* _ShapePointSet_H_ */
