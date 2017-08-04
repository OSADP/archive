/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "BasicSafetyMessage.h"

static asn_TYPE_member_t asn_MBR_BasicSafetyMessage_1[] = {
	{ ATF_NOFLAGS, 0, offsetof(struct BasicSafetyMessage, msgID),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DSRCmsgID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgID"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct BasicSafetyMessage, blob1),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_BSMblob,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"blob1"
		},
	{ ATF_POINTER, 2, offsetof(struct BasicSafetyMessage, safetyExt),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_VehicleSafetyExtension,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"safetyExt"
		},
	{ ATF_POINTER, 1, offsetof(struct BasicSafetyMessage, status),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_VehicleStatus,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"status"
		},
};
static ber_tlv_tag_t asn_DEF_BasicSafetyMessage_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_BasicSafetyMessage_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* msgID at 52 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* blob1 at 55 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* safetyExt at 84 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 } /* status at 85 */
};
static asn_SEQUENCE_specifics_t asn_SPC_BasicSafetyMessage_specs_1 = {
	sizeof(struct BasicSafetyMessage),
	offsetof(struct BasicSafetyMessage, _asn_ctx),
	asn_MAP_BasicSafetyMessage_tag2el_1,
	4,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	3,	/* Start extensions */
	5	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_BasicSafetyMessage = {
	"BasicSafetyMessage",
	"BasicSafetyMessage",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_BasicSafetyMessage_tags_1,
	sizeof(asn_DEF_BasicSafetyMessage_tags_1)
		/sizeof(asn_DEF_BasicSafetyMessage_tags_1[0]), /* 1 */
	asn_DEF_BasicSafetyMessage_tags_1,	/* Same as above */
	sizeof(asn_DEF_BasicSafetyMessage_tags_1)
		/sizeof(asn_DEF_BasicSafetyMessage_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_BasicSafetyMessage_1,
	4,	/* Elements count */
	&asn_SPC_BasicSafetyMessage_specs_1	/* Additional specs */
};

