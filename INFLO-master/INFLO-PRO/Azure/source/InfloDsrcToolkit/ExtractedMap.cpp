/*!
    @file         InfloDsrcToolkit/ExtractedMap.cpp
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

#include "InfloDsrcToolkit/ExtractedMap.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#include "DSRC_TK_J2735_R36/MapData.h"
#pragma managed(pop)

struct ApproachObject *buildApproach(ExtractedNodeList^ approach)
{
	struct ApproachObject *results = (struct ApproachObject *)calloc(1, sizeof(struct ApproachObject));

	results->refPoint = (struct Position3D *)calloc(1, sizeof(struct Position3D));
	int latBase = approach->getAverageLatitude();
	int longBase = approach->getAverageLongitude();
	results->refPoint->lat = latBase;
	results->refPoint->Long = longBase;

	results->approach = (struct Approach *)calloc(1, sizeof (struct Approach));
	results->approach->drivingLanes = (struct Approach::drivingLanes *)calloc(1, sizeof(struct Approach::drivingLanes));
	asn_set_init(&results->approach->drivingLanes->list);

	if (approach->getName() != nullptr)
	{
		int nameSize = approach->getName()->Length;
		results->approach->name = (DescriptiveName_t *)calloc(1, sizeof(DescriptiveName_t));
		results->approach->name->buf = (uint8_t *)calloc(nameSize + 1, sizeof(uint8_t));
		results->approach->name->size = nameSize + 1;
		array<wchar_t>^ managedName = approach->getName()->ToCharArray();
		for(int i = 0; i < nameSize; i++)
		{
			results->approach->name->buf[i] = (uint8_t)managedName[i];
		}
	}
		
	struct VehicleReferenceLane *drivingLane = (struct VehicleReferenceLane *)calloc(1, sizeof(struct VehicleReferenceLane ));
	asn_set_init(&drivingLane->nodeList.list);

	for(int i = 0; i < approach->getNodeCount(); i++)
	{
		Offsets_t *offset = (Offsets_t *)calloc(1, sizeof(Offsets_t));
		int16_t xOffset = approach->getNode(i)->getLongitudeOffset(longBase);
		int16_t yOffset = approach->getNode(i)->getLatitudeOffset(latBase);
		int16_t mm = approach->getNode(i)->getMileMarker();

		offset->buf = (uint8_t *)calloc(3, sizeof(int16_t));
		offset->size = 3*sizeof(int16_t);

		offset->buf[0] = (uint8_t)(xOffset >> 8);
		offset->buf[1] = (uint8_t)(xOffset >> 0);
		offset->buf[2] = (uint8_t)(yOffset >> 8);
		offset->buf[3] = (uint8_t)(yOffset >> 0);
		offset->buf[4] = (uint8_t)(mm >> 8);
		offset->buf[5] = (uint8_t)(mm >> 0);

		asn_set_add(&drivingLane->nodeList.list, offset);
	}

	asn_set_add(&results->approach->drivingLanes->list, drivingLane);

	return results;
}

struct Intersection *buildIntersection(ExtractedMapIntersection^ intersection)
{
	struct Intersection *results = (struct Intersection *)calloc(1, sizeof(struct Intersection));
	asn_set_init(&results->approaches);
		
	results->refPoint = (struct Position3D *)calloc(1, sizeof(struct Position3D));
	results->refPoint->lat = intersection->getAverageLatitude();
	results->refPoint->Long = intersection->getAverageLongitude();

	for(int i = 0; i < intersection->getApproachCount(); i++)
	{
		struct ApproachObject *approach = buildApproach(intersection->getApproach(i));
		asn_set_add(&results->approaches, approach);
	}
	
	return results;
}



ExtractedMap::ExtractedMap(System::String^ name)
{
	intersections = nullptr;
	this->name = name;
}

void ExtractedMap::setIntersections(array<ExtractedMapIntersection^>^ intersections)
{
	this->intersections = intersections;
}

array<System::Byte>^ ExtractedMap::generateASN()
{
	MapData_t *map = (MapData_t *)calloc(1, sizeof(MapData_t));
	MapData_Init(map);

	int nameSize = this->name->Length;
	map->name = (DescriptiveName_t *)calloc(1, sizeof(DescriptiveName_t));
	map->name->buf = (uint8_t *)calloc(nameSize + 1, sizeof(uint8_t));
	map->name->size = nameSize + 1;
	array<wchar_t>^ managedName = this->name->ToCharArray();
	for(int j = 0; j < nameSize; j++)
	{
		map->name->buf[j] = (uint8_t)managedName[j];
	}

	map->intersections = (struct MapData_t::intersections *)calloc(1, sizeof(struct MapData_t::intersections));
	asn_set_init(&map->intersections->list);
		
	for(int j = 0; j < this->intersections->Length; j++)
	{
		struct Intersection *intersection = buildIntersection(this->intersections[j]);
		asn_set_add(&map->intersections->list, intersection);
	}

	//Serialize and transfer to managed Byte[]
	BYTE serializedBuffer[MAX_MSG_SIZE];
	INT32 msgSize = (INT32)DSRC_serializer(&asn_DEF_MapData,
								map, serializedBuffer);

	array<System::Byte>^ mapResults = gcnew array<System::Byte>(msgSize);
	for(INT32 j = 0; j < msgSize; ++j)
	{
		mapResults[j] = serializedBuffer[j];
	}

	ASN_STRUCT_FREE(asn_DEF_MapData, map);

	return mapResults;
}