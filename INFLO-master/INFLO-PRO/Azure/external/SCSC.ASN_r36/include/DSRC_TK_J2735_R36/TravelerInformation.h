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
 * Generated by asn1c-0.9.22 (http://lionet.info/asn1c) [SCSCrev09]
 * From ASN.1 module "DSRC"
 * 	found in "DSRC_R36_Source.ASN"
 * 	`asn1c -S/skeletons`
 */

#ifndef	_TravelerInformation_H_
#define	_TravelerInformation_H_


#include <asn_application.h>

/* Including external dependencies */
#include "DSRCmsgID.h"
#include "UniqueMSGID.h"
#include "URL-Base.h"
#include "Count.h"
#include "MsgCRC.h"
#include <asn_SEQUENCE_OF.h>
#include "TravelerInfoType.h"
#include "DYear.h"
#include "MinuteOfTheYear.h"
#include "MinutesDuration.h"
#include "SignPrority.h"
#include "LaneWidth.h"
#include "DirectionOfUse.h"
#include "URL-Short.h"
#include "FurtherInfoID.h"
#include "RoadSignID.h"
#include <constr_CHOICE.h>
#include <constr_SEQUENCE_OF.h>
#include "ITIScodesAndText.h"
#include "WorkZone.h"
#include "GenericSignage.h"
#include "SpeedLimit.h"
#include "ExitService.h"
#include <constr_SEQUENCE.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Dependencies */
typedef enum msgId_PR {
	msgId_PR_NOTHING = 0,	/* No components present */
	msgId_PR_furtherInfoID = 1,  // DCK mod, added fixed values for debugging
	msgId_PR_roadSignID = 2
} msgId_PR;
typedef enum content_PR {
	content_PR_NOTHING = 0,	/* No components present */
	content_PR_advisory = 1,  // DCK mod, added fixed values for debugging
	content_PR_workZone = 2,
	content_PR_genericSign = 3,
	content_PR_speedLimit = 4,
	content_PR_exitService = 5
} content_PR;

/* Forward declarations */
struct Position3D;
struct ValidRegion;

//
// DCK added strucs
//
typedef	struct tiMsgId {
				msgId_PR present;
				union msgId_u {
					FurtherInfoID_t	 furtherInfoID;
					RoadSignID_t	 roadSignID;
				} choice;
				
				/* Context for parsing across buffer boundaries */
				asn_struct_ctx_t _asn_ctx;
			} tiMsgId;

typedef struct tiRegions {
				A_SEQUENCE_OF(struct ValidRegion) list;
				
				/* Context for parsing across buffer boundaries */
				asn_struct_ctx_t _asn_ctx;
			} tiRegions;

typedef struct tiContent {
				content_PR present;
				union content_u {
					ITIScodesAndText_t	 advisory;
					WorkZone_t	 workZone;
					GenericSignage_t	 genericSign;
					SpeedLimit_t	 speedLimit;
					ExitService_t	 exitService;
				} choice;
				
				/* Context for parsing across buffer boundaries */
				asn_struct_ctx_t _asn_ctx;
			} tiContent;

typedef struct tiMember {
			TravelerInfoType_t	 frameType;
			//struct msgId {
			//	msgId_PR present;
			//	union msgId_u {
			//		FurtherInfoID_t	 furtherInfoID;
			//		RoadSignID_t	 roadSignID;
			//	} choice;
			//	
			//	/* Context for parsing across buffer boundaries */
			//	asn_struct_ctx_t _asn_ctx;
			//} msgId;
			struct tiMsgId msgId; 
			DYear_t	*startYear	/* OPTIONAL */;
			MinuteOfTheYear_t	 startTime;
			MinutesDuration_t	 duratonTime;
			SignPrority_t	 priority;
		    struct Position3D	*commonAnchor	/* OPTIONAL */;
			LaneWidth_t	*commonLaneWidth	/* OPTIONAL */;
			DirectionOfUse_t	*commonDirectionality	/* OPTIONAL */;
			//struct regions {
			//	A_SEQUENCE_OF(struct ValidRegion) list;
			//	
			//	/* Context for parsing across buffer boundaries */
			//	asn_struct_ctx_t _asn_ctx;
			//} regions;
			struct tiRegions  regions;
			//struct content {
			//	content_PR present;
			//	union content_u {
			//		ITIScodesAndText_t	advisory;
			//		WorkZone_t			workZone;
			//		GenericSignage_t	genericSign;
			//		SpeedLimit_t		speedLimit;
			//		ExitService_t		exitService;
			//	} choice;
			//	
			//	/* Context for parsing across buffer boundaries */
			//	asn_struct_ctx_t _asn_ctx;
			//} content;
			struct tiContent content;
			URL_Short_t	*url	/* OPTIONAL */;
			
			/* Context for parsing across buffer boundaries */
			asn_struct_ctx_t _asn_ctx;
		} tiMember;

typedef struct dataFrames_ti {
		A_SEQUENCE_OF(struct tiMember  ) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} dataFrames_ti;
//
// DCK end edits (and below)
//



/* TravelerInformation */
typedef struct TravelerInformation {
	DSRCmsgID_t	 msgID;
	UniqueMSGID_t	*packetID	/* OPTIONAL */;
	URL_Base_t	*urlB	/* OPTIONAL */;
	Count_t	*dataFrameCount	/* OPTIONAL */;
	struct dataFrames {
		A_SEQUENCE_OF(struct tiMember ) list;
		// DCK mod, above line replaces below lines
		//A_SEQUENCE_OF(struct Member {
		//	TravelerInfoType_t	 frameType;
		//	struct msgId {
		//		msgId_PR present;
		//		union msgId_u {
		//			FurtherInfoID_t	 furtherInfoID;
		//			RoadSignID_t	 roadSignID;
		//		} choice;
		//		
		//		/* Context for parsing across buffer boundaries */
		//		asn_struct_ctx_t _asn_ctx;
		//	} msgId;
		//	DYear_t	*startYear	/* OPTIONAL */;
		//	MinuteOfTheYear_t	 startTime;
		//	MinutesDuration_t	 duratonTime;
		//	SignPrority_t	 priority;
		//	struct Position3D	*commonAnchor	/* OPTIONAL */;
		//	LaneWidth_t	*commonLaneWidth	/* OPTIONAL */;
		//	DirectionOfUse_t	*commonDirectionality	/* OPTIONAL */;
		//	struct regions {
		//		A_SEQUENCE_OF(struct ValidRegion) list;
		//		
		//		/* Context for parsing across buffer boundaries */
		//		asn_struct_ctx_t _asn_ctx;
		//	} regions;
		//	struct content {
		//		content_PR present;
		//		union content_u {
		//			ITIScodesAndText_t	 advisory;
		//			WorkZone_t	 workZone;
		//			GenericSignage_t	 genericSign;
		//			SpeedLimit_t	 speedLimit;
		//			ExitService_t	 exitService;
		//		} choice;
		//		
		//		/* Context for parsing across buffer boundaries */
		//		asn_struct_ctx_t _asn_ctx;
		//	} content;
		//	URL_Short_t	*url	/* OPTIONAL */;
		//	
		//	/* Context for parsing across buffer boundaries */
		//	asn_struct_ctx_t _asn_ctx;
		//} ) list;
		
		/* Context for parsing across buffer boundaries */
		asn_struct_ctx_t _asn_ctx;
	} dataFrames;
	MsgCRC_t	 crc;
	/*
	 * This type is extensible,
	 * possible extensions are below.
	 */
	
	/* Context for parsing across buffer boundaries */
	asn_struct_ctx_t _asn_ctx;
} TravelerInformation_t;

/* Implementation */
extern asn_TYPE_descriptor_t asn_DEF_TravelerInformation;

/* Define supported Utility Calls */
void TravelerInformation_Init (TravelerInformation_t* theObj);
//void TravelerInformation_ToBlob (TravelerInformation_t* theObj, char* theBlob);
//void TravelerInformation_FromBlob (TravelerInformation_t* theObj, char* theBlob);


#ifdef __cplusplus
}
#endif

/* Referred external types */
#include "Position3D.h"
#include "ValidRegion.h"

#endif	/* _TravelerInformation_H_ */
