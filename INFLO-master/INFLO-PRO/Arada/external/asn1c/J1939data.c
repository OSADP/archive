/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "J1939data.h"

static int
memb_tires_constraint_1(asn_TYPE_descriptor_t *td, const void *sptr,
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
	
	if((size <= 16)) {
		/* Perform validation of the inner elements */
		return td->check_constraints(td, sptr, ctfailcb, app_key);
	} else {
		_ASN_CTFAIL(app_key, td, sptr,
			"%s: constraint failed (%s:%d)",
			td->name, __FILE__, __LINE__);
		return -1;
	}
}

static int
memb_axle_constraint_1(asn_TYPE_descriptor_t *td, const void *sptr,
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
	
	if((size <= 16)) {
		/* Perform validation of the inner elements */
		return td->check_constraints(td, sptr, ctfailcb, app_key);
	} else {
		_ASN_CTFAIL(app_key, td, sptr,
			"%s: constraint failed (%s:%d)",
			td->name, __FILE__, __LINE__);
		return -1;
	}
}

static asn_TYPE_member_t asn_MBR_Member_3[] = {
	{ ATF_POINTER, 7, offsetof(struct J1939data__tires__Member, location),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TireLocation,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"location"
		},
	{ ATF_POINTER, 6, offsetof(struct J1939data__tires__Member, pressure),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TirePressure,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"pressure"
		},
	{ ATF_POINTER, 5, offsetof(struct J1939data__tires__Member, temp),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TireTemp,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"temp"
		},
	{ ATF_POINTER, 4, offsetof(struct J1939data__tires__Member, wheelSensorStatus),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_WheelSensorStatus,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"wheelSensorStatus"
		},
	{ ATF_POINTER, 3, offsetof(struct J1939data__tires__Member, wheelEndElectFault),
		(ASN_TAG_CLASS_CONTEXT | (4 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_WheelEndElectFault,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"wheelEndElectFault"
		},
	{ ATF_POINTER, 2, offsetof(struct J1939data__tires__Member, leakageRate),
		(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TireLeakageRate,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"leakageRate"
		},
	{ ATF_POINTER, 1, offsetof(struct J1939data__tires__Member, detection),
		(ASN_TAG_CLASS_CONTEXT | (6 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TirePressureThresholdDetection,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"detection"
		},
};
static ber_tlv_tag_t asn_DEF_Member_tags_3[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_Member_tag2el_3[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* location at 1135 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* pressure at 1136 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* temp at 1137 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 }, /* wheelSensorStatus at 1138 */
    { (ASN_TAG_CLASS_CONTEXT | (4 << 2)), 4, 0, 0 }, /* wheelEndElectFault at 1139 */
    { (ASN_TAG_CLASS_CONTEXT | (5 << 2)), 5, 0, 0 }, /* leakageRate at 1140 */
    { (ASN_TAG_CLASS_CONTEXT | (6 << 2)), 6, 0, 0 } /* detection at 1141 */
};
static asn_SEQUENCE_specifics_t asn_SPC_Member_specs_3 = {
	sizeof(struct J1939data__tires__Member),
	offsetof(struct J1939data__tires__Member, _asn_ctx),
	asn_MAP_Member_tag2el_3,
	7,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	6,	/* Start extensions */
	8	/* Stop extensions */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_Member_3 = {
	"SEQUENCE",
	"SEQUENCE",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_Member_tags_3,
	sizeof(asn_DEF_Member_tags_3)
		/sizeof(asn_DEF_Member_tags_3[0]), /* 1 */
	asn_DEF_Member_tags_3,	/* Same as above */
	sizeof(asn_DEF_Member_tags_3)
		/sizeof(asn_DEF_Member_tags_3[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_Member_3,
	7,	/* Elements count */
	&asn_SPC_Member_specs_3	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_tires_2[] = {
	{ ATF_POINTER, 0, 0,
		(ASN_TAG_CLASS_UNIVERSAL | (16 << 2)),
		0,
		&asn_DEF_Member_3,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		""
		},
};
static ber_tlv_tag_t asn_DEF_tires_tags_2[] = {
	(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_SET_OF_specifics_t asn_SPC_tires_specs_2 = {
	sizeof(struct J1939data__tires),
	offsetof(struct J1939data__tires, _asn_ctx),
	0,	/* XER encoding is XMLDelimitedItemList */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_tires_2 = {
	"tires",
	"tires",
	SEQUENCE_OF_free,
	SEQUENCE_OF_print,
	SEQUENCE_OF_constraint,
	SEQUENCE_OF_decode_ber,
	SEQUENCE_OF_encode_der,
	SEQUENCE_OF_decode_xer,
	SEQUENCE_OF_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_tires_tags_2,
	sizeof(asn_DEF_tires_tags_2)
		/sizeof(asn_DEF_tires_tags_2[0]) - 1, /* 1 */
	asn_DEF_tires_tags_2,	/* Same as above */
	sizeof(asn_DEF_tires_tags_2)
		/sizeof(asn_DEF_tires_tags_2[0]), /* 2 */
	0,	/* No PER visible constraints */
	asn_MBR_tires_2,
	1,	/* Single element */
	&asn_SPC_tires_specs_2	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_Member_13[] = {
	{ ATF_POINTER, 2, offsetof(struct J1939data__axle__Member, location),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_AxleLocation,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"location"
		},
	{ ATF_POINTER, 1, offsetof(struct J1939data__axle__Member, weight),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_AxleWeight,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"weight"
		},
};
static ber_tlv_tag_t asn_DEF_Member_tags_13[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_Member_tag2el_13[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* location at 1146 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 } /* weight at 1147 */
};
static asn_SEQUENCE_specifics_t asn_SPC_Member_specs_13 = {
	sizeof(struct J1939data__axle__Member),
	offsetof(struct J1939data__axle__Member, _asn_ctx),
	asn_MAP_Member_tag2el_13,
	2,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	1,	/* Start extensions */
	3	/* Stop extensions */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_Member_13 = {
	"SEQUENCE",
	"SEQUENCE",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_Member_tags_13,
	sizeof(asn_DEF_Member_tags_13)
		/sizeof(asn_DEF_Member_tags_13[0]), /* 1 */
	asn_DEF_Member_tags_13,	/* Same as above */
	sizeof(asn_DEF_Member_tags_13)
		/sizeof(asn_DEF_Member_tags_13[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_Member_13,
	2,	/* Elements count */
	&asn_SPC_Member_specs_13	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_axle_12[] = {
	{ ATF_POINTER, 0, 0,
		(ASN_TAG_CLASS_UNIVERSAL | (16 << 2)),
		0,
		&asn_DEF_Member_13,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		""
		},
};
static ber_tlv_tag_t asn_DEF_axle_tags_12[] = {
	(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_SET_OF_specifics_t asn_SPC_axle_specs_12 = {
	sizeof(struct J1939data__axle),
	offsetof(struct J1939data__axle, _asn_ctx),
	0,	/* XER encoding is XMLDelimitedItemList */
};
static /* Use -fall-defs-global to expose */
asn_TYPE_descriptor_t asn_DEF_axle_12 = {
	"axle",
	"axle",
	SEQUENCE_OF_free,
	SEQUENCE_OF_print,
	SEQUENCE_OF_constraint,
	SEQUENCE_OF_decode_ber,
	SEQUENCE_OF_encode_der,
	SEQUENCE_OF_decode_xer,
	SEQUENCE_OF_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_axle_tags_12,
	sizeof(asn_DEF_axle_tags_12)
		/sizeof(asn_DEF_axle_tags_12[0]) - 1, /* 1 */
	asn_DEF_axle_tags_12,	/* Same as above */
	sizeof(asn_DEF_axle_tags_12)
		/sizeof(asn_DEF_axle_tags_12[0]), /* 2 */
	0,	/* No PER visible constraints */
	asn_MBR_axle_12,
	1,	/* Single element */
	&asn_SPC_axle_specs_12	/* Additional specs */
};

static asn_TYPE_member_t asn_MBR_J1939data_1[] = {
	{ ATF_POINTER, 10, offsetof(struct J1939data, tires),
		(ASN_TAG_CLASS_CONTEXT | (0 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_tires_2,
		memb_tires_constraint_1,
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"tires"
		},
	{ ATF_POINTER, 9, offsetof(struct J1939data, axle),
		(ASN_TAG_CLASS_CONTEXT | (1 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_axle_12,
		memb_axle_constraint_1,
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"axle"
		},
	{ ATF_POINTER, 8, offsetof(struct J1939data, trailerWeight),
		(ASN_TAG_CLASS_CONTEXT | (2 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_TrailerWeight,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"trailerWeight"
		},
	{ ATF_POINTER, 7, offsetof(struct J1939data, cargoWeight),
		(ASN_TAG_CLASS_CONTEXT | (3 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_CargoWeight,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"cargoWeight"
		},
	{ ATF_POINTER, 6, offsetof(struct J1939data, steeringAxleTemperature),
		(ASN_TAG_CLASS_CONTEXT | (4 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_SteeringAxleTemperature,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"steeringAxleTemperature"
		},
	{ ATF_POINTER, 5, offsetof(struct J1939data, driveAxleLocation),
		(ASN_TAG_CLASS_CONTEXT | (5 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DriveAxleLocation,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"driveAxleLocation"
		},
	{ ATF_POINTER, 4, offsetof(struct J1939data, driveAxleLiftAirPressure),
		(ASN_TAG_CLASS_CONTEXT | (6 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DriveAxleLiftAirPressure,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"driveAxleLiftAirPressure"
		},
	{ ATF_POINTER, 3, offsetof(struct J1939data, driveAxleTemperature),
		(ASN_TAG_CLASS_CONTEXT | (7 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DriveAxleTemperature,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"driveAxleTemperature"
		},
	{ ATF_POINTER, 2, offsetof(struct J1939data, driveAxleLubePressure),
		(ASN_TAG_CLASS_CONTEXT | (8 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_DriveAxleLubePressure,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"driveAxleLubePressure"
		},
	{ ATF_POINTER, 1, offsetof(struct J1939data, steeringAxleLubePressure),
		(ASN_TAG_CLASS_CONTEXT | (9 << 2)),
		-1,	/* IMPLICIT tag at current level */
		&asn_DEF_SteeringAxleLubePressure,
		0,	/* Defer constraints checking to the member type */
		0,	/* PER is not compiled, use -gen-PER */
		0,
		"steeringAxleLubePressure"
		},
};
static ber_tlv_tag_t asn_DEF_J1939data_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (16 << 2))
};
static asn_TYPE_tag2member_t asn_MAP_J1939data_tag2el_1[] = {
    { (ASN_TAG_CLASS_CONTEXT | (0 << 2)), 0, 0, 0 }, /* tires at 1143 */
    { (ASN_TAG_CLASS_CONTEXT | (1 << 2)), 1, 0, 0 }, /* axle at 1149 */
    { (ASN_TAG_CLASS_CONTEXT | (2 << 2)), 2, 0, 0 }, /* trailerWeight at 1150 */
    { (ASN_TAG_CLASS_CONTEXT | (3 << 2)), 3, 0, 0 }, /* cargoWeight at 1151 */
    { (ASN_TAG_CLASS_CONTEXT | (4 << 2)), 4, 0, 0 }, /* steeringAxleTemperature at 1152 */
    { (ASN_TAG_CLASS_CONTEXT | (5 << 2)), 5, 0, 0 }, /* driveAxleLocation at 1153 */
    { (ASN_TAG_CLASS_CONTEXT | (6 << 2)), 6, 0, 0 }, /* driveAxleLiftAirPressure at 1154 */
    { (ASN_TAG_CLASS_CONTEXT | (7 << 2)), 7, 0, 0 }, /* driveAxleTemperature at 1155 */
    { (ASN_TAG_CLASS_CONTEXT | (8 << 2)), 8, 0, 0 }, /* driveAxleLubePressure at 1156 */
    { (ASN_TAG_CLASS_CONTEXT | (9 << 2)), 9, 0, 0 } /* steeringAxleLubePressure at 1157 */
};
static asn_SEQUENCE_specifics_t asn_SPC_J1939data_specs_1 = {
	sizeof(struct J1939data),
	offsetof(struct J1939data, _asn_ctx),
	asn_MAP_J1939data_tag2el_1,
	10,	/* Count of tags in the map */
	0, 0, 0,	/* Optional elements (not needed) */
	9,	/* Start extensions */
	11	/* Stop extensions */
};
asn_TYPE_descriptor_t asn_DEF_J1939data = {
	"J1939data",
	"J1939data",
	SEQUENCE_free,
	SEQUENCE_print,
	SEQUENCE_constraint,
	SEQUENCE_decode_ber,
	SEQUENCE_encode_der,
	SEQUENCE_decode_xer,
	SEQUENCE_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_J1939data_tags_1,
	sizeof(asn_DEF_J1939data_tags_1)
		/sizeof(asn_DEF_J1939data_tags_1[0]), /* 1 */
	asn_DEF_J1939data_tags_1,	/* Same as above */
	sizeof(asn_DEF_J1939data_tags_1)
		/sizeof(asn_DEF_J1939data_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	asn_MBR_J1939data_1,
	10,	/* Elements count */
	&asn_SPC_J1939data_specs_1	/* Additional specs */
};

