/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#ifndef	_ConnectsTo_H_
#define	_ConnectsTo_H_


#include <asn_application.h>

/* Including external dependencies */
#include <OCTET_STRING.h>

#ifdef __cplusplus
extern "C" {
#endif

/* ConnectsTo */
typedef OCTET_STRING_t	 ConnectsTo_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_ConnectsTo;
asn_struct_free_f ConnectsTo_free;
asn_struct_print_f ConnectsTo_print;
asn_constr_check_f ConnectsTo_constraint;
ber_type_decoder_f ConnectsTo_decode_ber;
der_type_encoder_f ConnectsTo_encode_der;
xer_type_decoder_f ConnectsTo_decode_xer;
xer_type_encoder_f ConnectsTo_encode_xer;

#ifdef __cplusplus
}
#endif

#endif	/* _ConnectsTo_H_ */
