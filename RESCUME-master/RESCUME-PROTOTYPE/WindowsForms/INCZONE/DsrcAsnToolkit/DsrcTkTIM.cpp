/*!
    @file         DsrcAsnToolkit/DsrcTkTIM.cpp
    @author       Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/

#include "DsrcTkTIM.h"
#include "DsrcTkShapePointSet.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#include "DSRC_TK_J2735_R36/TravelerInformation.h"
#include <intrin.h>
#pragma managed(pop)

using System::String;
using System::DateTime;

#define LATLONG_MULT 10000000.0
#define LANEWIDTH_MULT 100.0
#define OFFSET_MULT 10.0

ValidRegion *buildShapePointRegion(DsrcTkShapePointSet^ shapePointSet)
{
	ValidRegion *region = (ValidRegion *)calloc(1, sizeof(ValidRegion));

	region->area.present = area_PR_shapePointSet;

	double latitudeAnchor = shapePointSet->getAverageLatitude();
	double longitudeAnchor = shapePointSet->getAverageLongitude();
	double elevationAnchor = shapePointSet->getAverageElevation();

	region->area.choice.shapePointSet.anchor = (Position3D *)calloc(1, sizeof(Position3D));
	region->area.choice.shapePointSet.anchor->lat = (Latitude_t)(latitudeAnchor * LATLONG_MULT);
	region->area.choice.shapePointSet.anchor->Long = (Longitude_t)(longitudeAnchor * LATLONG_MULT);
	if (shapePointSet->hasElevationData())
	{
		region->area.choice.shapePointSet.anchor->elevation = (Elevation_t *)calloc(1, sizeof(Elevation_t));
		region->area.choice.shapePointSet.anchor->elevation->buf = (uint8_t *)calloc(1, 2);
		region->area.choice.shapePointSet.anchor->elevation->size = 2;
		int16_t elevation = elevationAnchor * 10;
		region->area.choice.shapePointSet.anchor->elevation->buf[0] = elevation >> 8;
		region->area.choice.shapePointSet.anchor->elevation->buf[0] = elevation & 0xFF;
	}

	region->area.choice.shapePointSet.laneWidth = (LaneWidth_t *)calloc(1, sizeof(LaneWidth_t));
	*region->area.choice.shapePointSet.laneWidth = (LaneWidth_t)(shapePointSet->getAverageLaneWidth() * LANEWIDTH_MULT);

	region->area.choice.shapePointSet.directionality = (DirectionOfUse_t *)calloc(1, sizeof(DirectionOfUse_t));
	*region->area.choice.shapePointSet.directionality = (DirectionOfUse_t)shapePointSet->getDirectionOfUse();

	asn_set_init(&region->area.choice.shapePointSet.nodeList.list);

	for (int i = 0; i < shapePointSet->getNodes()->Length; i++)
	{
		Offsets_t *offset = (Offsets_t *)calloc(1, sizeof(Offsets_t));
		int16_t xOffset = (int16_t)(shapePointSet->getNodes()[i]->getEastingOffset(longitudeAnchor) * OFFSET_MULT);
		int16_t yOffset = (int16_t)(shapePointSet->getNodes()[i]->getNorthingOffset(latitudeAnchor) * OFFSET_MULT);
		int16_t eOffset = (int16_t)(shapePointSet->getNodes()[i]->getElevationOffset(elevationAnchor) * OFFSET_MULT);
		if (shapePointSet->hasElevationData())
		{
			offset->buf = (uint8_t *)calloc(3, sizeof(int16_t));
			offset->size = 3 * sizeof(int16_t);
		}
		else
		{
			offset->buf = (uint8_t *)calloc(2, sizeof(int16_t));
			offset->size = 2 * sizeof(int16_t);
		}

		offset->buf[0] = (uint8_t)(xOffset >> 8);
		offset->buf[1] = (uint8_t)(xOffset >> 0);
		offset->buf[2] = (uint8_t)(yOffset >> 8);
		offset->buf[3] = (uint8_t)(yOffset >> 0);
		if (shapePointSet->hasElevationData())
		{
			offset->buf[4] = (uint8_t)(eOffset >> 8);
			offset->buf[5] = (uint8_t)(eOffset >> 0);
		}

		asn_set_add(&region->area.choice.shapePointSet.nodeList.list, offset);
	}
	
	return region;
}

tiMember *buildFrame(DsrcTkDataFrame^ frame)
{
	tiMember *results = (tiMember*)calloc(1, sizeof(tiMember));
	results->frameType = frame->getContentType();
	results->msgId.present = msgId_PR::msgId_PR_furtherInfoID;
	
	results->startTime = (long)(frame->getStartTime() - System::DateTime(System::DateTime::Now.Year, 1, 1)).TotalMinutes;
	results->duratonTime = frame->getDuration();

	results->priority = frame->getPriority();

	/*
	 * This anchor point is not required... The anchor point is in the ShapePointSet (ValidRegion), not the data frame. Not going to use it right now.
	 */
	/*results->commonAnchor = (Position3D *)calloc(1, sizeof(Position3D));
	results->commonAnchor->lat = (Latitude_t)(frame->getAverageLatitude() * LATLONG_MULT);
	results->commonAnchor->Long = (Longitude_t)(frame->getAverageLongitude() * LATLONG_MULT);*/


	asn_set_init(&results->regions.list);
	for(int i = 0; i < frame->getShapePointRegions()->Length; i++)
	{
		ValidRegion *region = buildShapePointRegion(frame->getShapePointRegions()[i]);
		asn_set_add(&results->regions.list, region);
	}

	results->content.present = content_PR::content_PR_advisory;
	asn_set_init(&results->content.choice.advisory.list);

	for (int i = 0; i < frame->getItisCodes()->Count; i++)
	{
		System::String^ itisCodeOrText = frame->getItisCodes()[i];

		if (itisCodeOrText->StartsWith("$("))
		{
			int code = System::Int32::Parse(itisCodeOrText->Substring(2, itisCodeOrText->IndexOf(')') - 2));
			itisMember *itisCodeMember = (itisMember*)calloc(1, sizeof(itisMember));
			itisCodeMember->item.present = item_PR::item_PR_itis;
			itisCodeMember->item.choice.itis = code;
			asn_set_add(&results->content.choice.advisory.list, itisCodeMember);
		}
		else
		{
			int textLength = itisCodeOrText->Length + 1;

			itisMember *itisCodeMember = (itisMember*)calloc(1, sizeof(itisMember));
			itisCodeMember->item.present = item_PR::item_PR_text;
			itisCodeMember->item.choice.text.buf = (uint8_t*)calloc(textLength, sizeof(uint8_t));
			itisCodeMember->item.choice.text.size = textLength;

			for (int j = 0; j < itisCodeOrText->Length; j++)
			{
				itisCodeMember->item.choice.text.buf[j] = itisCodeOrText[j];
			}

			itisCodeMember->item.choice.text.buf[textLength - 1] = '\0';

			asn_set_add(&results->content.choice.advisory.list, itisCodeMember);
		}

	}
	
	return results;
}

DsrcTkTIM::DsrcTkTIM(DsrcTkTIMOptions^ options)
{
	this->frames = options->getFrames();
}

array<System::Byte>^ DsrcTkTIM::generateAsn()
{
	TravelerInformation_t *tim = (TravelerInformation_t *)calloc(1, sizeof(TravelerInformation_t));
	TravelerInformation_Init(tim);

	DateTime currentTime = DateTime::UtcNow;
	
	double packetId = (currentTime - System::DateTime(currentTime.Year, currentTime.Month, currentTime.Day)).TotalMilliseconds;
	int packetIdSize = sizeof(double);

	tim->packetID = (UniqueMSGID_t *)calloc(1, sizeof(UniqueMSGID_t));
	tim->packetID->buf = (uint8_t *)calloc(1, packetIdSize);
	tim->packetID->size = packetIdSize;

	System::Random^ random = gcnew System::Random();
	//for (int i = 0; i < packetIdSize; i++)
	//{
	//	tim->packetID->buf[i] = random->Next();
	//}
	*((double *)tim->packetID->buf) = packetId;

	asn_set_init(&tim->dataFrames.list);
	for(int i = 0; i < this->frames->Length; i++)
	{
		tiMember *frame = buildFrame(this->frames[i]);
		asn_set_add(&tim->dataFrames.list, frame);
	}

	tim->dataFrameCount = (Count_t *)calloc(1, sizeof(Count_t));
	*tim->dataFrameCount = tim->dataFrames.list.count;
	
	//Serialize and transfer to managed Byte[]
	BYTE serializedBuffer[MAX_MSG_SIZE];
	INT32 msgSize = (INT32)DSRC_serializer(&asn_DEF_TravelerInformation,
							  tim, serializedBuffer);

	array<System::Byte>^ results;// = gcnew array<System::Byte>(msgSize);
	if (msgSize > 0)
	{
		results = gcnew array<System::Byte>(msgSize);
	}
	else
	{
		results = gcnew array<System::Byte>(0);
	}
	for(INT32 i=0; i < results->Length; ++i)
	{
		results[i] = serializedBuffer[i];
	}

	ASN_STRUCT_FREE(asn_DEF_TravelerInformation, tim);

	return results;
}

