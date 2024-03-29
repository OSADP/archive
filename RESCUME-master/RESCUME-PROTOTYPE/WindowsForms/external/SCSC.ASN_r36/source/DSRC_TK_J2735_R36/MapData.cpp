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
#include "MapData.h"

static int
memb_intersections_constraint_1(asn_TYPE_descriptor_t *td, const void *sptr,
			asn_app_constraint_failed_f *ctfailcb, void *app_key) {
	size_t size;
	
	if(!sptr) {
		_ASN_CTFAIL(app_key, td, sptr,
			"%s: value not given (%s:%d)",
			td->name, __FILE__, __LINE__);
		return -1;
	}
	
	/* Determine the number of elements */
	size = _A_CSEQUENCE_FROM_VOID(sptr)->count;
	
	if((size >= 1 && size <= 32)) {
		/* Perform validation of the inner elements */
		return td->check_constraints(td, sptr, ctfailcb, app_key);
	} else {
		_ASN_CTFAIL(app_key, td, sptr,
			"%s: constraint failed (%s:%d)",
			td->name, __FILE__, __LINE__);
		return -1;
	}
}

static asn_TYPE_member_t asn_MBR_intersections_7[] = {
	{ ATF_POINTER, 0, 0,
		(ASN_TAG_CLASS_UNIVERSAL | (16 << 2)),
		0,
		&asn_DEF_Intersection,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		""
		},
};
static ber_tlv_tag_t asn_DEF_intersections_tags_7[] = {
	(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_SET_OF_specifics_t asn_SPC_intersections_specs_7 = {
	sizeof(struct intersections),
	offsetof(struct intersections, _asn_ctx),
	0,	/* XER encoding is XMLDelimitedItemList */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_intersections_7 = {
	"intersections",
	"intersections",
	SEQUENCE_OF_free,
	SEQUENCE_OF_print,
	SEQUENCE_OF_constraint,
	SEQUENCE_OF_decode_ber,
	SEQUENCE_OF_encode_der,
	SEQUENCE_OF_decode_xer,
	SEQUENCE_OF_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_intersections_tags_7,
	sizeof(asn_DEF_intersections_tags_7)
		/sizeof(asn_DEF_intersections_tags_7[0]) - 1, /* 1 */
	asn_DEF_intersections_tags_7,	/* Same as above */
	sizeof(asn_DEF_intersections_tags_7)
		/sizeof(asn_DEF_intersections_tags_7[0]), /* 2 */
	0,	/* No PER visible constraints */
	asn_MBR_intersections_7,
	1,	/* Single element */
	&asn_SPC_intersections_specs_7	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_MapData_1[] = {
	{ ATF_NOFLAGS, 0, offsetof(struct MapData, msgID),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DSRCmsgID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgID"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct MapData, msgCnt),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_MsgCount,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgCnt"
		},
	{ ATF_POINTER, 5, offsetof(struct MapData, name),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DescriptiveName,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"name"
		},
	{ ATF_POINTER, 4, offsetof(struct MapData, layerType),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_LayerType,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"layerType"
		},
	{ ATF_POINTER, 3, offsetof(struct MapData, layerID),
		(ASN_TAG_CLASS_CONTEXT | (4 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_LayerID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"layerID"
		},
	{ ATF_POINTER, 2, offsetof(struct MapData, intersections),
		(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
		0,
		&asn_DEF_intersections_7,
		memb_intersections_constraint_1,
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"intersections"
		},
	{ ATF_POINTER, 1, offsetof(struct MapData, dataParameters),
		(ASN_TAG_CLASS_CONTEXT | (6 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DataParameters,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"dataParameters"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct MapData, crc),
		(ASN_TAG_CLASS_CONTEXT | (7 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_MsgCRC,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"crc"
		},
};
static ber_tlv_tag_t asn_DEF_MapData_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_MapData_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* msgID at 195 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* msgCnt at 196 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* name at 197 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 }, /* layerType at 198 */
    { (ASN_TAG_CLASS_CONTEXT | (4 << 2)), 4, 0, 0 }, /* layerID at 199 */
    { (ASN_TAG_CLASS_CONTEXT | (5 << 2)), 5, 0, 0 }, /* intersections at 201 */
    { (ASN_TAG_CLASS_CONTEXT | (6 << 2)), 6, 0, 0 }, /* dataParameters at 212 */
    { (ASN_TAG_CLASS_CONTEXT | (7 << 2)), 7, 0, 0 } /* crc at 213 */
};
static asn_SEQUENCE_specifics_t asn_SPC_MapData_specs_1 = {
	sizeof(struct MapData),
	offsetof(struct MapData, _asn_ctx),
	asn_MAP_MapData_tag2el_1,
	8,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	7,	/* Start extensions */
	9	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_MapData = {
	"MapData",
	"MapData",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_MapData_tags_1,
	sizeof(asn_DEF_MapData_tags_1)
		/sizeof(asn_DEF_MapData_tags_1[0]), /* 1 */
	asn_DEF_MapData_tags_1,	/* Same as above */
	sizeof(asn_DEF_MapData_tags_1)
		/sizeof(asn_DEF_MapData_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_MapData_1,
	8,	/* Elements count */
	&asn_SPC_MapData_specs_1	/* Additional specs */
};


/* Utility Calls */
/* Utility Calls */


// Set all basic structure pointers to 'safe' null values
void MapData_Init ( MapData_t* theObj)
{
	if (theObj != NULL)
	{
		theObj->msgID = DSRCmsgID_mapData; // the only valid value
		theObj->msgCnt = 0; 
		theObj->name = NULL;
		theObj->layerType = NULL;
		theObj->layerID = NULL;
		theObj->intersections = NULL;
		theObj->dataParameters = NULL;
		MsgCRC_Init(&theObj->crc);
		theObj->_asn_ctx.ptr = NULL; // not used
	}
}

// Not used in this object
// Move the structure into the packed bytes of the blob
void MapData_ToBlob ( MapData_t* theObj, char* theBlob)
{
	if ((theObj != NULL) && (theBlob != NULL))
	{
	}
}

// Not used in this object
// Fill the structure from the packed bytes of the blob
void MapData_FromBlob ( MapData_t* theObj, char* theBlob)
{
	if ((theObj != NULL) && (theBlob != NULL))
	{
	}
}

