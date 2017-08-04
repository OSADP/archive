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
 * Copyright (c) 2004, 2006 Lev Walkin <vlm@lionet.info>. All rights reserved.
 * Redistribution and modifications are permitted subject to BSD license.
 */

/*
 * Application-level ASN.1 callbacks.
 */
#ifndef	_ASN_APPLICATION_H_
#define	_ASN_APPLICATION_H_

// DCK: The all important declare to keep the ASN C 
// code out of the mananged rules of the CRL in Windows 
#pragma unmanaged 
// DCK mod end

#include "asn_system.h"		/* for platform-dependent types */
#include "asn_codecs.h"		/* for ASN.1 codecs specifics */

#ifdef __cplusplus
extern "C" {
#endif

/*
 * Generic type of an application-defined callback to return various
 * types of data to the application.
 * EXPECTED RETURN VALUES:
 *  -1: Failed to consume bytes. Abort the mission.
 * Non-negative return values indicate success, and ignored.
 */
typedef int (asn_app_consume_bytes_f)(const void *buffer, size_t size,
	void *application_specific_key);

/*
 * A callback of this type is called whenever constraint validation fails
 * on some ASN.1 type. See "constraints.h" for more details on constraint
 * validation.
 * This callback specifies a descriptor of the ASN.1 type which failed
 * the constraint check, as well as human readable message on what
 * particular constraint has failed.
 */
typedef void (asn_app_constraint_failed_f)(void *application_specific_key,
	struct asn_TYPE_descriptor_s *type_descriptor_which_failed,
	const void *structure_which_failed_ptr,
	const char *error_message_format, ...) GCC_PRINTFLIKE(4, 5);

#ifdef __cplusplus
}
#endif

#include "constr_TYPE.h"	/* for asn_TYPE_descriptor_t */

#endif	/* _ASN_APPLICATION_H_ */
