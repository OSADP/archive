/*!
    @file         InfloDsrcToolkit/ExtractedMapIntersection.cpp
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

#include "InfloDsrcToolkit/ExtractedMapIntersection.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#pragma managed(pop)

ExtractedMapIntersection::ExtractedMapIntersection(System::String^ id, array<ExtractedNodeList^>^ approaches)
{
	this->id = id;
	this->approaches = approaches;
}

System::String^ ExtractedMapIntersection::getId()
{
	return this->id;
}

int ExtractedMapIntersection::getAverageLatitude()
{
	if (getApproachCount() == 0)
		return 0;

	long long results = 0;
	for(int i = 0; i < getApproachCount(); i++)
	{
		results += getApproach(i)->getAverageLatitude();
	}

	return results / getApproachCount();
}

int ExtractedMapIntersection::getAverageLongitude()
{
	if (getApproachCount() == 0)
		return 0;

	long long results = 0;
	for(int i = 0; i < getApproachCount(); i++)
	{
		results += getApproach(i)->getAverageLongitude();
	}

	return results / getApproachCount();
}

int ExtractedMapIntersection::getApproachCount()
{
	return approaches->Length;
}

ExtractedNodeList^ ExtractedMapIntersection::getApproach(int number)
{
	return approaches[number];
}