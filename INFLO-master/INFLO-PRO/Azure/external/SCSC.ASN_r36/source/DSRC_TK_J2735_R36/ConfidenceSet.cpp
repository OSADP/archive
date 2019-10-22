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

#include "stdafx.h"
#include <asn_internal.h> 
#include "ConfidenceSet.h"

static asn_TYPE_member_t asn_MBR_ConfidenceSet_1[] = {
	{ ATF_POINTER, 6, offsetof(struct ConfidenceSet, accelConfidence),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_AccelSteerYawRateConfidence,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"accelConfidence"
		},
	{ ATF_POINTER, 5, offsetof(struct ConfidenceSet, speedConfidence),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_SpeedandHeadingandThrottleConfidence,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"speedConfidence"
		},
	{ ATF_POINTER, 4, offsetof(struct ConfidenceSet, timeConfidence),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TimeConfidence,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"timeConfidence"
		},
	{ ATF_POINTER, 3, offsetof(struct ConfidenceSet, posConfidence),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_PositionConfidenceSet,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"posConfidence"
		},
	{ ATF_POINTER, 2, offsetof(struct ConfidenceSet, steerConfidence),
		(ASN_TAG_CLASS_CONTEXT | (4 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_SteeringWheelAngleConfidence,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"steerConfidence"
		},
	{ ATF_POINTER, 1, offsetof(struct ConfidenceSet, throttleConfidence),
		(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_ThrottleConfidence,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"throttleConfidence"
		},
};
static ber_tlv_tag_t asn_DEF_ConfidenceSet_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_ConfidenceSet_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* accelConfidence at 914 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* speedConfidence at 915 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* timeConfidence at 916 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 }, /* posConfidence at 917 */
    { (ASN_TAG_CLASS_CONTEXT | (4 << 2)), 4, 0, 0 }, /* steerConfidence at 918 */
    { (ASN_TAG_CLASS_CONTEXT | (5 << 2)), 5, 0, 0 } /* throttleConfidence at 919 */
};
static asn_SEQUENCE_specifics_t asn_SPC_ConfidenceSet_specs_1 = {
	sizeof(struct ConfidenceSet),
	offsetof(struct ConfidenceSet, _asn_ctx),
	asn_MAP_ConfidenceSet_tag2el_1,
	6,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	5,	/* Start extensions */
	7	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_ConfidenceSet = {
	"ConfidenceSet",
	"ConfidenceSet",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_ConfidenceSet_tags_1,
	sizeof(asn_DEF_ConfidenceSet_tags_1)
		/sizeof(asn_DEF_ConfidenceSet_tags_1[0]), /* 1 */
	asn_DEF_ConfidenceSet_tags_1,	/* Same as above */
	sizeof(asn_DEF_ConfidenceSet_tags_1)
		/sizeof(asn_DEF_ConfidenceSet_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_ConfidenceSet_1,
	6,	/* Elements count */
	&asn_SPC_ConfidenceSet_specs_1	/* Additional specs */
};
