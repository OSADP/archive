/*!
@file         DsrcAsnToolkit/DsrcTkAbsoluteNode.h
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

#define DSRCTK_POLAR_RADIUS 6356752.314247833
#define DSRCTK_EQUATORIAL_RADIUS 6378137

#define _USE_MATH_DEFINES
#include <cmath>

public ref class DsrcTkAbsoluteNode
{
public:
	DsrcTkAbsoluteNode(double latitude, double longitude);
	DsrcTkAbsoluteNode(double latitude, double longitude, double elevation);
	DsrcTkAbsoluteNode(double latitude, double longitude, double elevation, double laneWidth);
	~DsrcTkAbsoluteNode() { }

	double getLatitude();
	double getLongitude();
	double getElevation();
	double getLaneWidth();
	bool hasElevationData();
	bool hasLaneWidthData();

	double getNorthingOffset(double baseLatitude);

	double getEastingOffset(double baseLongitude);

	double getElevationOffset(double baseElevation);

private:
	double latitude;
	double longitude;
	double elevation;
	double laneWidth;

	bool hasElevation;
	bool hasLaneWidth;
};
