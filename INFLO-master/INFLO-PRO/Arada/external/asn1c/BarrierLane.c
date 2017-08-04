/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "BarrierLane.h"

static asn_TYPE_member_t asn_MBR_BarrierLane_1[] = {
	{ ATF_NOFLAGS, 0, offsetof(struct BarrierLane, laneNumber),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_LaneNumber,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"laneNumber"
		},
	{ ATF_POINTER, 1, offsetof(struct BarrierLane, laneWidth),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_LaneWidth,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"laneWidth"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct BarrierLane, barrierAttributes),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_BarrierAttributes,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"barrierAttributes"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct BarrierLane, nodeList),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_NodeList,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"nodeList"
		},
};
static ber_tlv_tag_t asn_DEF_BarrierLane_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_BarrierLane_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* laneNumber at 834 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* laneWidth at 835 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* barrierAttributes at 836 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 } /* nodeList at 837 */
};
static asn_SEQUENCE_specifics_t asn_SPC_BarrierLane_specs_1 = {
	sizeof(struct BarrierLane),
	offsetof(struct BarrierLane, _asn_ctx),
	asn_MAP_BarrierLane_tag2el_1,
	4,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	3,	/* Start extensions */
	5	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_BarrierLane = {
	"BarrierLane",
	"BarrierLane",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_BarrierLane_tags_1,
	sizeof(asn_DEF_BarrierLane_tags_1)
		/sizeof(asn_DEF_BarrierLane_tags_1[0]), /* 1 */
	asn_DEF_BarrierLane_tags_1,	/* Same as above */
	sizeof(asn_DEF_BarrierLane_tags_1)
		/sizeof(asn_DEF_BarrierLane_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_BarrierLane_1,
	4,	/* Elements count */
	&asn_SPC_BarrierLane_specs_1	/* Additional specs */
};

