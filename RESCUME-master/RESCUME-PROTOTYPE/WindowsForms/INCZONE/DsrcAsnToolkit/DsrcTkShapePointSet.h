/*!
@file         DsrcAsnToolkit/DsrcTkShapePointSet.h
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
#pragma once

#include "DsrcTkAbsoluteNode.h"

#define DSRCTK_DEFAULT_LANEWIDTH 3.5

public enum DsrcTkDirectionOfUse {
	DsrcTkDirectionOfUse_Forward = 0,
	DsrcTkDirectionOfUse_Reverse = 1,
	DsrcTkDirectionOfUse_Both = 2
};

public ref class DsrcTkShapePointSet
{
public:
	DsrcTkShapePointSet(array<DsrcTkAbsoluteNode^>^ nodes, DsrcTkDirectionOfUse directionOfUse);
	~DsrcTkShapePointSet() { }

	array<DsrcTkAbsoluteNode^>^ getNodes();
	int getDirectionOfUse();

	double getAverageLatitude();
	double getAverageLongitude();
	double getAverageElevation();
	double getAverageLaneWidth();
	bool hasElevationData();

private:
	array<DsrcTkAbsoluteNode^>^ nodes;
	int directionOfUse;
};
