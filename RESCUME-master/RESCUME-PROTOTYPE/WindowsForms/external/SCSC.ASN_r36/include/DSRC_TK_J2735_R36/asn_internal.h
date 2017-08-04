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
 * Copyright (c) 2003, 2004, 2005, 2007 Lev Walkin <vlm@lionet.info>.
 * All rights reserved.
 * Redistribution and modifications are permitted subject to BSD license.
 */
/*
 * Declarations internally useful for the ASN.1 support code.
 */


#ifndef	_ASN_INTERNAL_H_
#define	_ASN_INTERNAL_H_

#include "asn_application.h"	/* Application-visible API */


#ifndef	__NO_ASSERT_H__		/* Include assert.h only for internal use. */
#include <assert.h>		/* for assert() macro */
#endif

#ifdef	__cplusplus
extern "C" {
#endif

/* Environment version might be used to avoid running with the old libraries */
#define	ASN1C_ENVIRONMENT_VERSION	922		/* Compile-time version */
#define	ASN1C_SCSC_VERSION	9				/* SCSC Compile-time version */
int get_asn1c_environment_version(void);	/* Run-time version */
int get_asn1c_SCSC_version(void);			/* Run-time version */

#define	CALLOC(nmemb, size)	calloc(nmemb, size)
#define	MALLOC(size)		malloc(size)
#define	REALLOC(oldptr, size)	realloc(oldptr, size)
#define	FREEMEM(ptr)		free(ptr)

/*
 * A macro for debugging the ASN.1 internals.
 * You may enable or override it.
 */

// DCK mods start  
// Used to turn detailed parser messages on/off in stream
// Comment out below line when not wanted, enable to add debug traces
// OFF    #define EMIT_ASN_DEBUG  1 /* DCK added to enable debugging calls */
// DCK mods end

#ifndef	ASN_DEBUG	/* If debugging code is not defined elsewhere... */
#if	EMIT_ASN_DEBUG == 1	/* And it was asked to emit this code... */
#ifdef	__GNUC__
#ifdef	ASN_THREAD_SAFE
#define	asn_debug_indent	0
#else	/* !ASN_THREAD_SAFE */
int asn_debug_indent;
#endif	/* ASN_THREAD_SAFE */
#define	ASN_DEBUG(fmt, args...)	do {			\
		int adi = asn_debug_indent;		\
		while(adi--) fprintf(stderr, " ");	\
		fprintf(stderr, fmt, ##args);		\
		fprintf(stderr, " (%s:%d)\n",		\
			__FILE__, __LINE__);		\
	} while(0)
#else	/* !__GNUC__ */
void ASN_DEBUG_f(const char *fmt, ...);
#define	ASN_DEBUG	ASN_DEBUG_f
#endif	/* __GNUC__ */
#else	/* EMIT_ASN_DEBUG != 1 */
static inline void ASN_DEBUG(const char *fmt, ...) { (void)fmt; }
#endif	/* EMIT_ASN_DEBUG */
#endif	/* ASN_DEBUG */

/*
 * Invoke the application-supplied callback and fail, if something is wrong.
 */
#define	__ASN_E_cbc(buf, size)	(cb((buf), (size), app_key) < 0)
#define	_ASN_E_CALLBACK(foo)	do {					\
		if(foo)	goto cb_failed;					\
	} while(0)
#define	_ASN_CALLBACK(buf, size)					\
	_ASN_E_CALLBACK(__ASN_E_cbc(buf, size))
#define	_ASN_CALLBACK2(buf1, size1, buf2, size2)			\
	_ASN_E_CALLBACK(__ASN_E_cbc(buf1, size1) || __ASN_E_cbc(buf2, size2))
#define	_ASN_CALLBACK3(buf1, size1, buf2, size2, buf3, size3)		\
	_ASN_E_CALLBACK(__ASN_E_cbc(buf1, size1)			\
		|| __ASN_E_cbc(buf2, size2)				\
		|| __ASN_E_cbc(buf3, size3))

#define	_i_ASN_TEXT_INDENT(nl, level) do {				\
	int __level = (level);						\
	int __nl = ((nl) != 0);						\
	int __i;							\
	if(__nl) _ASN_CALLBACK("\n", 1);				\
	if(__level < 0) __level = 0;					\
	for(__i = 0; __i < __level; __i++)				\
		_ASN_CALLBACK("    ", 4);				\
	er.encoded += __nl + 4 * __level;				\
} while(0)

#define	_i_INDENT(nl)	do {						\
	int __i;							\
	if((nl) && cb("\n", 1, app_key) < 0) return -1;			\
	for(__i = 0; __i < ilevel; __i++)				\
		if(cb("    ", 4, app_key) < 0) return -1;		\
} while(0)

/*
 * Check stack against overflow, if limit is set.
 */
#define	_ASN_DEFAULT_STACK_MAX	(30000)
static inline int
_ASN_STACK_OVERFLOW_CHECK(asn_codec_ctx_t *ctx) {
	if(ctx && ctx->max_stack_size) {

		/* ctx MUST be allocated on the stack */
		ptrdiff_t usedstack = ((char *)ctx - (char *)&ctx);
		if(usedstack > 0) usedstack = -usedstack; /* grows up! */

		/* double negative required to avoid int wrap-around */
		if(usedstack < -(ptrdiff_t)ctx->max_stack_size) {
			ASN_DEBUG("Stack limit %ld reached",
				(long)ctx->max_stack_size);
			return -1;
		}
	}
	return 0;
}

#ifdef	__cplusplus
}
#endif

#endif	/* _ASN_INTERNAL_H_ */
