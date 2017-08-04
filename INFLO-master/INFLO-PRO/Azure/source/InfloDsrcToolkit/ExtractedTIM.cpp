/*!
    @file         InfloDsrcToolkit/ExtractedTIM.cpp
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

#include "InfloDsrcToolkit/ExtractedTIM.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#include "DSRC_TK_J2735_R36/TravelerInformation.h"
#pragma managed(pop)

using System::String;

ValidRegion *buildRegion(ExtractedNodeList^ nodes)
{
	ValidRegion *region = (ValidRegion *)calloc(1, sizeof(ValidRegion));

	region->area.present = area_PR_shapePointSet;
	region->area.choice.shapePointSet.anchor = (Position3D *)calloc(1, sizeof(Position3D));
	int latBase = nodes->getAverageLatitude();
	int longBase = nodes->getAverageLongitude();
	region->area.choice.shapePointSet.anchor->lat = latBase;
	region->area.choice.shapePointSet.anchor->Long = longBase;

	asn_set_init(&region->area.choice.shapePointSet.nodeList.list);

	for(int i = 0; i < nodes->getNodeCount(); i++)
	{
		Offsets_t *offset = (Offsets_t *)calloc(1, sizeof(Offsets_t));
		int16_t xOffset = nodes->getNode(i)->getLongitudeOffset(longBase);
		int16_t yOffset = nodes->getNode(i)->getLatitudeOffset(latBase);
		int16_t mm = nodes->getNode(i)->getMileMarker();

		offset->buf = (uint8_t *)calloc(3, sizeof(int16_t));
		offset->size = 3*sizeof(int16_t);

		offset->buf[0] = (uint8_t)(xOffset >> 8);
		offset->buf[1] = (uint8_t)(xOffset >> 0);
		offset->buf[2] = (uint8_t)(yOffset >> 8);
		offset->buf[3] = (uint8_t)(yOffset >> 0);
		offset->buf[4] = (uint8_t)(mm >> 8);
		offset->buf[5] = (uint8_t)(mm >> 0);

		asn_set_add(&region->area.choice.shapePointSet.nodeList.list, offset);
	}

	return region;
}

tiMember *buildFrame(ExtractedTimFrame^ frame)
{
	tiMember *results = (tiMember*)calloc(1, sizeof(tiMember));
	results->frameType = TravelerInfoType::TravelerInfoType_advisory;
	results->msgId.present = msgId_PR::msgId_PR_furtherInfoID;
	
	results->startTime = (long)(frame->getStartTime() - System::DateTime(System::DateTime::Now.Year, 1, 1)).TotalMinutes;
	results->duratonTime = frame->getDuration();

	results->commonAnchor = (Position3D *)calloc(1, sizeof(Position3D));
	results->commonAnchor->lat = frame->getAverageLatitude();
	results->commonAnchor->Long = frame->getAverageLongitude();

	asn_set_init(&results->regions.list);

	for(int i = 0; i < frame->getAlertPathCount(); i++)
	{
		ValidRegion *region = buildRegion(frame->getAlertPath(i));
		asn_set_add(&results->regions.list, region);
	}

	results->content.present = content_PR::content_PR_advisory;
	asn_set_init(&results->content.choice.advisory.list);

	int itisTextLength = frame->getItisAlertText()->Length;
	itisMember *itisTextMember = (itisMember*)calloc(1, sizeof(itisMember));
	itisTextMember->item.present = item_PR::item_PR_text;
	itisTextMember->item.choice.text.buf = (uint8_t *)calloc(itisTextLength + 1, sizeof(uint8_t));
	itisTextMember->item.choice.text.size = itisTextLength + 1;

	array<wchar_t>^ itisText = frame->getItisAlertText()->ToCharArray();
	for(int i = 0; i < itisTextLength; i++)
	{
		itisTextMember->item.choice.text.buf[i] = (uint8_t)itisText[i];
	}

	asn_set_add(&results->content.choice.advisory.list, itisTextMember);

	return results;
}

ExtractedTIM::ExtractedTIM() 
{
	frames = gcnew array<ExtractedTimFrame^>(0);
}

void ExtractedTIM::setPacketId(int agencyId, System::DateTime time)
{
	int secondsInMonth = (int)(time - System::DateTime(time.Year, time.Month, 01)).TotalSeconds;
	this->packetID = ((agencyId & 0xff) << 24) | (secondsInMonth & 0xffffff);
}

void ExtractedTIM::setFrames(array<ExtractedTimFrame^>^ frames)
{
	this->frames = frames;
}

array<System::Byte>^ ExtractedTIM::generateASN()
{
	TravelerInformation_t *tim = (TravelerInformation_t *)calloc(1, sizeof(TravelerInformation_t));
	TravelerInformation_Init(tim);

	tim->packetID = (UniqueMSGID_t *)calloc(1, sizeof(UniqueMSGID_t));
	
	// Set Packet ID
	// Always put these on heap, so the destroy and teardown will work
	int packetIdSize = 8;
	tim->packetID->buf = (uint8_t *)calloc(1, packetIdSize);
	if (tim->packetID->buf != 0)
	{
		tim->packetID->size = packetIdSize;
		tim->packetID->buf[0] = rand() % 255;
		tim->packetID->buf[1] = rand() % 255;
		tim->packetID->buf[2] = rand() % 255;
		tim->packetID->buf[3] = rand() % 255;
		tim->packetID->buf[4] = (this->packetID >> 24) & 0xff;
		tim->packetID->buf[5] = (this->packetID >> 16) & 0xff;
		tim->packetID->buf[6] = (this->packetID >> 8) & 0xff;
		tim->packetID->buf[7] = (this->packetID) & 0xff;
	}
	
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

	array<System::Byte>^ results = gcnew array<System::Byte>(msgSize);
	for(INT32 i=0; i < msgSize; ++i)
	{
		results[i] = serializedBuffer[i];
	}

	ASN_STRUCT_FREE(asn_DEF_TravelerInformation, tim);

	return results;
}

