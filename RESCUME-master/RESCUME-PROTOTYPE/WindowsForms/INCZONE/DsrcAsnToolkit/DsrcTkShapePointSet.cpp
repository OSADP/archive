/*!
@file         DsrcAsnToolkit/DsrcTkShapePointSet.cpp
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

#include "DsrcTkShapePointSet.h"

DsrcTkShapePointSet::DsrcTkShapePointSet(array<DsrcTkAbsoluteNode^>^ nodes, DsrcTkDirectionOfUse directionOfUse)
{
	this->nodes = nodes;
	this->directionOfUse = directionOfUse;
}

array<DsrcTkAbsoluteNode^>^ DsrcTkShapePointSet::getNodes()
{
	return this->nodes;
}

int DsrcTkShapePointSet::getDirectionOfUse()
{
	return this->directionOfUse;
}

double DsrcTkShapePointSet::getAverageLatitude()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < nodes->Length; i++)
	{
		results += nodes[i]->getLatitude();
		count++;
	}

	return results / count;
}

double DsrcTkShapePointSet::getAverageLongitude()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < nodes->Length; i++)
	{
		results += nodes[i]->getLongitude();
		count++;
	}

	return results / count;
}

double DsrcTkShapePointSet::getAverageElevation()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < nodes->Length; i++)
	{
		if (nodes[i]->hasElevationData())
		{
			results += nodes[i]->getLatitude();
			count++;
		}
	}

	if (count == 0)
		return 0;
	return results / count;
}

double DsrcTkShapePointSet::getAverageLaneWidth()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < nodes->Length; i++)
	{
		if (nodes[i]->hasLaneWidthData())
		{
			results += nodes[i]->getLaneWidth();
			count++;
		}
	}

	if (count == 0)
		return DSRCTK_DEFAULT_LANEWIDTH;
	return results / count;
}

bool DsrcTkShapePointSet::hasElevationData()
{
	int count = 0;
	for (int i = 0; i < nodes->Length; i++)
	{
		if (nodes[i]->hasElevationData())
		{
			count++;
		}
	}

	return count != 0;
}