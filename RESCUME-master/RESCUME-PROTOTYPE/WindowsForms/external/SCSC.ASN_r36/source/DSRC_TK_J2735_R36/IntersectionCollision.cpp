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
#include "IntersectionCollision.h"

static asn_TYPE_member_t asn_MBR_IntersectionCollision_1[] = {
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, msgID),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DSRCmsgID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgID"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, msgCnt),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_MsgCount,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgCnt"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, id),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TemporaryID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"id"
		},
	{ ATF_POINTER, 1, offsetof(struct IntersectionCollision, secMark),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DSecond,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"secMark"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, path),
		(ASN_TAG_CLASS_CONTEXT | (4 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_PathHistory,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"path"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, intersetionID),
		(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_IntersectionID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"intersetionID"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, laneNumber),
		(ASN_TAG_CLASS_CONTEXT | (6 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_LaneNumber,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"laneNumber"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct IntersectionCollision, eventFlag),
		(ASN_TAG_CLASS_CONTEXT | (7 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_EventFlags,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"eventFlag"
		},
};
static ber_tlv_tag_t asn_DEF_IntersectionCollision_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_IntersectionCollision_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* msgID at 173 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* msgCnt at 174 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* id at 175 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 }, /* secMark at 176 */
    { (ASN_TAG_CLASS_CONTEXT | (4 << 2)), 4, 0, 0 }, /* path at 177 */
    { (ASN_TAG_CLASS_CONTEXT | (5 << 2)), 5, 0, 0 }, /* intersetionID at 179 */
    { (ASN_TAG_CLASS_CONTEXT | (6 << 2)), 6, 0, 0 }, /* laneNumber at 182 */
    { (ASN_TAG_CLASS_CONTEXT | (7 << 2)), 7, 0, 0 } /* eventFlag at 185 */
};
static asn_SEQUENCE_specifics_t asn_SPC_IntersectionCollision_specs_1 = {
	sizeof(struct IntersectionCollision),
	offsetof(struct IntersectionCollision, _asn_ctx),
	asn_MAP_IntersectionCollision_tag2el_1,
	8,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	7,	/* Start extensions */
	9	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_IntersectionCollision = {
	"IntersectionCollision",
	"IntersectionCollision",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_IntersectionCollision_tags_1,
	sizeof(asn_DEF_IntersectionCollision_tags_1)
		/sizeof(asn_DEF_IntersectionCollision_tags_1[0]), /* 1 */
	asn_DEF_IntersectionCollision_tags_1,	/* Same as above */
	sizeof(asn_DEF_IntersectionCollision_tags_1)
		/sizeof(asn_DEF_IntersectionCollision_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_IntersectionCollision_1,
	8,	/* Elements count */
	&asn_SPC_IntersectionCollision_specs_1	/* Additional specs */
};


/* Utility Calls */
/* Utility Calls */
 

// Set all basic structure pointers to 'safe' null values
void IntersectionCollision_Init ( IntersectionCollision_t* theObj)
{
	if (theObj != NULL)
	{
		theObj->msgID = DSRCmsgID_intersectionCollisionAlert;
		theObj->msgCnt = 0;
		TemporaryID_Init(&theObj->id); 
		theObj->secMark = NULL;
		// TODO: PathHistory_Init(theObj->path);
		// TODO: IntersectionID_Init(&theObj->intersetionID, 4); // size not known, use 4
		// TODO: LaneNumber_Init(theObj->laneNumber); // not known, so zero
		theObj->eventFlag = 0; // none
		theObj->_asn_ctx.ptr = NULL; // not used
	}
}

// Not used in this object
// Move the structure into the packed bytes of the blob
void IntersectionCollision_ToBlob ( IntersectionCollision_t* theObj, char* theBlob)
{
	if ((theObj != NULL) && (theBlob != NULL))
	{
	}
}

// Not used in this object
// Fill the structure from the packed bytes of the blob
void IntersectionCollision_FromBlob ( IntersectionCollision_t* theObj, char* theBlob)
{
	if ((theObj != NULL) && (theBlob != NULL))
	{
	}
}

