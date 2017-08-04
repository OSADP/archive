/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "PositionConfidence.h"

int
PositionConfidence_constraint(asn_TYPE_descriptor_t *td, const void *sptr,
			asn_app_constraint_failed_f *ctfailcb, void *app_key) {
	/* Replace with underlying type checker */
	td->check_constraints = asn_DEF_ENUMERATED.check_constraints;
	return td->check_constraints(td, sptr, ctfailcb, app_key);
}

/*
 * This type is implemented using ENUMERATED,
 * so here we adjust the DEF accordingly.
 */
static void
PositionConfidence_1_inherit_TYPE_descriptor(asn_TYPE_descriptor_t *td) {
	td->free_struct    = asn_DEF_ENUMERATED.free_struct;
	td->print_struct   = asn_DEF_ENUMERATED.print_struct;
	td->ber_decoder    = asn_DEF_ENUMERATED.ber_decoder;
	td->der_encoder    = asn_DEF_ENUMERATED.der_encoder;
	td->xer_decoder    = asn_DEF_ENUMERATED.xer_decoder;
	td->xer_encoder    = asn_DEF_ENUMERATED.xer_encoder;
	td->uper_decoder   = asn_DEF_ENUMERATED.uper_decoder;
	td->uper_encoder   = asn_DEF_ENUMERATED.uper_encoder;
	if(!td->per_constraints)
		td->per_constraints = asn_DEF_ENUMERATED.per_constraints;
	td->elements       = asn_DEF_ENUMERATED.elements;
	td->elements_count = asn_DEF_ENUMERATED.elements_count;
     /* td->specifics      = asn_DEF_ENUMERATED.specifics;	// Defined explicitly */
}

void
PositionConfidence_free(asn_TYPE_descriptor_t *td,
		void *struct_ptr, int contents_only) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	td->free_struct(td, struct_ptr, contents_only);
}

int
PositionConfidence_print(asn_TYPE_descriptor_t *td, const void *struct_ptr,
		int ilevel, asn_app_consume_bytes_f *cb, void *app_key) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	return td->print_struct(td, struct_ptr, ilevel, cb, app_key);
}

asn_dec_rval_t
PositionConfidence_decode_ber(asn_codec_ctx_t *opt_codec_ctx, asn_TYPE_descriptor_t *td,
		void **structure, const void *bufptr, size_t size, int tag_mode) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	return td->ber_decoder(opt_codec_ctx, td, structure, bufptr, size, tag_mode);
}

asn_enc_rval_t
PositionConfidence_encode_der(asn_TYPE_descriptor_t *td,
		void *structure, int tag_mode, ber_tlv_tag_t tag,
		asn_app_consume_bytes_f *cb, void *app_key) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	return td->der_encoder(td, structure, tag_mode, tag, cb, app_key);
}

asn_dec_rval_t
PositionConfidence_decode_xer(asn_codec_ctx_t *opt_codec_ctx, asn_TYPE_descriptor_t *td,
		void **structure, const char *opt_mname, const void *bufptr, size_t size) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	return td->xer_decoder(opt_codec_ctx, td, structure, opt_mname, bufptr, size);
}

asn_enc_rval_t
PositionConfidence_encode_xer(asn_TYPE_descriptor_t *td, void *structure,
		int ilevel, enum xer_encoder_flags_e flags,
		asn_app_consume_bytes_f *cb, void *app_key) {
	PositionConfidence_1_inherit_TYPE_descriptor(td);
	return td->xer_encoder(td, structure, ilevel, flags, cb, app_key);
}

static asn_INTEGER_enum_map_t asn_MAP_PositionConfidence_value2enum_1[] = {
	{ 0,	11,	"unavailable" },
	{ 1,	5,	"a500m" },
	{ 2,	5,	"a200m" },
	{ 3,	5,	"a100m" },
	{ 4,	4,	"a50m" },
	{ 5,	4,	"a20m" },
	{ 6,	4,	"a10m" },
	{ 7,	3,	"a5m" },
	{ 8,	3,	"a2m" },
	{ 9,	3,	"a1m" },
	{ 10,	5,	"a50cm" },
	{ 11,	5,	"a20cm" },
	{ 12,	5,	"a10cm" },
	{ 13,	4,	"a5cm" },
	{ 14,	4,	"a2cm" },
	{ 15,	4,	"a1cm" }
};
static unsigned int asn_MAP_PositionConfidence_enum2value_1[] = {
	3,	/* a100m(3) */
	12,	/* a10cm(12) */
	6,	/* a10m(6) */
	15,	/* a1cm(15) */
	9,	/* a1m(9) */
	2,	/* a200m(2) */
	11,	/* a20cm(11) */
	5,	/* a20m(5) */
	14,	/* a2cm(14) */
	8,	/* a2m(8) */
	1,	/* a500m(1) */
	10,	/* a50cm(10) */
	4,	/* a50m(4) */
	13,	/* a5cm(13) */
	7,	/* a5m(7) */
	0	/* unavailable(0) */
};
static asn_INTEGER_specifics_t asn_SPC_PositionConfidence_specs_1 = {
	asn_MAP_PositionConfidence_value2enum_1,	/* "tag" => N; sorted by tag */
	asn_MAP_PositionConfidence_enum2value_1,	/* N => "tag"; sorted by N */
	16,	/* Number of elements in the maps */
	0,	/* Enumeration is not extensible */
	1	/* Strict enumeration */
};
static ber_tlv_tag_t asn_DEF_PositionConfidence_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (10 << 2))
};
asn_TYPE_descriptor_t asn_DEF_PositionConfidence = {
	"PositionConfidence",
	"PositionConfidence",
	PositionConfidence_free,
	PositionConfidence_print,
	PositionConfidence_constraint,
	PositionConfidence_decode_ber,
	PositionConfidence_encode_der,
	PositionConfidence_decode_xer,
	PositionConfidence_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_PositionConfidence_tags_1,
	sizeof(asn_DEF_PositionConfidence_tags_1)
		/sizeof(asn_DEF_PositionConfidence_tags_1[0]), /* 1 */
	asn_DEF_PositionConfidence_tags_1,	/* Same as above */
	sizeof(asn_DEF_PositionConfidence_tags_1)
		/sizeof(asn_DEF_PositionConfidence_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	0, 0,	/* Defined elsewhere */
	&asn_SPC_PositionConfidence_specs_1	/* Additional specs */
};

