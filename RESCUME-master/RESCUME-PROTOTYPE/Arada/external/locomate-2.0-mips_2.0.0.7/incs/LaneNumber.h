/*
 * Generated by asn1c-0.9.21 (http://lionet.info/asn1c)
 * From ASN.1 module "DSRC"
 * 	found in "../downloads/DSRC_R36_Source.ASN"
 * 	`asn1c -fcompound-names`
 */

#ifndef	_LaneNumber_H_
#define	_LaneNumber_H_


#include <asn_application.h>

/* Including external dependencies */
#include <OCTET_STRING.h>

#ifdef __cplusplus
extern "C" {
#endif

/* LaneNumber */
typedef OCTET_STRING_t	 LaneNumber_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_LaneNumber;
asn_struct_free_f LaneNumber_free;
asn_struct_print_f LaneNumber_print;
asn_constr_check_f LaneNumber_constraint;
ber_type_decoder_f LaneNumber_decode_ber;
der_type_encoder_f LaneNumber_encode_der;
xer_type_decoder_f LaneNumber_decode_xer;
xer_type_encoder_f LaneNumber_encode_xer;

#ifdef __cplusplus
}
#endif

#endif	/* _LaneNumber_H_ */
