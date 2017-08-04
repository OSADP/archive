/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "SPAT.h"

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

static asn_TYPE_member_t asn_MBR_intersections_4[] = {
	{ ATF_POINTER, 0, 0,
		(ASN_TAG_CLASS_UNIVERSAL | (16 << 2)),
		0,
		&asn_DEF_IntersectionState,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		""
		},
};
static ber_tlv_tag_t asn_DEF_intersections_tags_4[] = {
	(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_SET_OF_specifics_t asn_SPC_intersections_specs_4 = {
	sizeof(struct SPAT__intersections),
	offsetof(struct SPAT__intersections, _asn_ctx),
	0,	/* XER encoding is XMLDelimitedItemList */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_intersections_4 = {
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
	asn_DEF_intersections_tags_4,
	sizeof(asn_DEF_intersections_tags_4)
		/sizeof(asn_DEF_intersections_tags_4[0]) - 1, /* 1 */
	asn_DEF_intersections_tags_4,	/* Same as above */
	sizeof(asn_DEF_intersections_tags_4)
		/sizeof(asn_DEF_intersections_tags_4[0]), /* 2 */
	0,	/* No PER visible constraints */
	asn_MBR_intersections_4,
	1,	/* Single element */
	&asn_SPC_intersections_specs_4	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_SPAT_1[] = {
	{ ATF_NOFLAGS, 0, offsetof(struct SPAT, msgID),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DSRCmsgID,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"msgID"
		},
	{ ATF_POINTER, 1, offsetof(struct SPAT, name),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DescriptiveName,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"name"
		},
	{ ATF_NOFLAGS, 0, offsetof(struct SPAT, intersections),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_intersections_4,
		memb_intersections_constraint_1,
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"intersections"
		},
};
static ber_tlv_tag_t asn_DEF_SPAT_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_SPAT_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* msgID at 433 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* name at 434 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 } /* intersections at 438 */
};
static asn_SEQUENCE_specifics_t asn_SPC_SPAT_specs_1 = {
	sizeof(struct SPAT),
	offsetof(struct SPAT, _asn_ctx),
	asn_MAP_SPAT_tag2el_1,
	3,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	2,	/* Start extensions */
	4	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_SPAT = {
	"SPAT",
	"SPAT",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_SPAT_tags_1,
	sizeof(asn_DEF_SPAT_tags_1)
		/sizeof(asn_DEF_SPAT_tags_1[0]), /* 1 */
	asn_DEF_SPAT_tags_1,	/* Same as above */
	sizeof(asn_DEF_SPAT_tags_1)
		/sizeof(asn_DEF_SPAT_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_SPAT_1,
	3,	/* Elements count */
	&asn_SPC_SPAT_specs_1	/* Additional specs */
};

