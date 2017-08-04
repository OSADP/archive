/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#include <asn_internal.h>

#include "SpeedConfidence.h"

int
SpeedConfidence_constraint(asn_TYPE_descriptor_t *td, const void *sptr,
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
SpeedConfidence_1_inherit_TYPE_descriptor(asn_TYPE_descriptor_t *td) {
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
SpeedConfidence_free(asn_TYPE_descriptor_t *td,
		void *struct_ptr, int contents_only) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	td->free_struct(td, struct_ptr, contents_only);
}

int
SpeedConfidence_print(asn_TYPE_descriptor_t *td, const void *struct_ptr,
		int ilevel, asn_app_consume_bytes_f *cb, void *app_key) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	return td->print_struct(td, struct_ptr, ilevel, cb, app_key);
}

asn_dec_rval_t
SpeedConfidence_decode_ber(asn_codec_ctx_t *opt_codec_ctx, asn_TYPE_descriptor_t *td,
		void **structure, const void *bufptr, size_t size, int tag_mode) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	return td->ber_decoder(opt_codec_ctx, td, structure, bufptr, size, tag_mode);
}

asn_enc_rval_t
SpeedConfidence_encode_der(asn_TYPE_descriptor_t *td,
		void *structure, int tag_mode, ber_tlv_tag_t tag,
		asn_app_consume_bytes_f *cb, void *app_key) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	return td->der_encoder(td, structure, tag_mode, tag, cb, app_key);
}

asn_dec_rval_t
SpeedConfidence_decode_xer(asn_codec_ctx_t *opt_codec_ctx, asn_TYPE_descriptor_t *td,
		void **structure, const char *opt_mname, const void *bufptr, size_t size) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	return td->xer_decoder(opt_codec_ctx, td, structure, opt_mname, bufptr, size);
}

asn_enc_rval_t
SpeedConfidence_encode_xer(asn_TYPE_descriptor_t *td, void *structure,
		int ilevel, enum xer_encoder_flags_e flags,
		asn_app_consume_bytes_f *cb, void *app_key) {
	SpeedConfidence_1_inherit_TYPE_descriptor(td);
	return td->xer_encoder(td, structure, ilevel, flags, cb, app_key);
}

static asn_INTEGER_enum_map_t asn_MAP_SpeedConfidence_value2enum_1[] = {
	{ 0,	11,	"unavailable" },
	{ 1,	9,	"prec100ms" },
	{ 2,	8,	"prec10ms" },
	{ 3,	7,	"prec5ms" },
	{ 4,	7,	"prec1ms" },
	{ 5,	9,	"prec0-1ms" },
	{ 6,	10,	"prec0-05ms" },
	{ 7,	10,	"prec0-01ms" }
};
static unsigned int asn_MAP_SpeedConfidence_enum2value_1[] = {
	7,	/* prec0-01ms(7) */
	6,	/* prec0-05ms(6) */
	5,	/* prec0-1ms(5) */
	1,	/* prec100ms(1) */
	2,	/* prec10ms(2) */
	4,	/* prec1ms(4) */
	3,	/* prec5ms(3) */
	0	/* unavailable(0) */
};
static asn_INTEGER_specifics_t asn_SPC_SpeedConfidence_specs_1 = {
	asn_MAP_SpeedConfidence_value2enum_1,	/* "tag" => N; sorted by tag */
	asn_MAP_SpeedConfidence_enum2value_1,	/* N => "tag"; sorted by N */
	8,	/* Number of elements in the maps */
	0,	/* Enumeration is not extensible */
	1	/* Strict enumeration */
};
static ber_tlv_tag_t asn_DEF_SpeedConfidence_tags_1[] = {
	(ASN_TAG_CLASS_UNIVERSAL | (10 << 2))
};
asn_TYPE_descriptor_t asn_DEF_SpeedConfidence = {
	"SpeedConfidence",
	"SpeedConfidence",
	SpeedConfidence_free,
	SpeedConfidence_print,
	SpeedConfidence_constraint,
	SpeedConfidence_decode_ber,
	SpeedConfidence_encode_der,
	SpeedConfidence_decode_xer,
	SpeedConfidence_encode_xer,
	0, 0,	/* No PER support, use "-gen-PER" to enable */
	0,	/* Use generic outmost tag fetcher */
	asn_DEF_SpeedConfidence_tags_1,
	sizeof(asn_DEF_SpeedConfidence_tags_1)
		/sizeof(asn_DEF_SpeedConfidence_tags_1[0]), /* 1 */
	asn_DEF_SpeedConfidence_tags_1,	/* Same as above */
	sizeof(asn_DEF_SpeedConfidence_tags_1)
		/sizeof(asn_DEF_SpeedConfidence_tags_1[0]), /* 1 */
	0,	/* No PER visible constraints */
	0, 0,	/* Defined elsewhere */
	&asn_SPC_SpeedConfidence_specs_1	/* Additional specs */
};

