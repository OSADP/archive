/*!
@file         DsrcAsnToolkit/DsrcTkAbsoluteNode.cpp
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

#include "DsrcTkAbsoluteNode.h"

DsrcTkAbsoluteNode::DsrcTkAbsoluteNode(double latitude, double longitude)
{
	this->latitude = latitude;
	this->longitude = longitude;
	this->hasElevation = false;
	this->hasLaneWidth = false;
}
DsrcTkAbsoluteNode::DsrcTkAbsoluteNode(double latitude, double longitude, double elevation)
{
	this->latitude = latitude;
	this->longitude = longitude;
	this->elevation = elevation;
	this->hasElevation = true;
	this->hasLaneWidth = false;
}
DsrcTkAbsoluteNode::DsrcTkAbsoluteNode(double latitude, double longitude, double elevation, double laneWidth)
{
	this->latitude = latitude;
	this->longitude = longitude;
	this->elevation = elevation;
	this->laneWidth = laneWidth;
	this->hasElevation = true;
	this->hasLaneWidth = true;
}

double DsrcTkAbsoluteNode::getLatitude()
{
	return this->latitude;
}
double DsrcTkAbsoluteNode::getLongitude()
{
	return this->longitude;
}
double DsrcTkAbsoluteNode::getElevation()
{
	return this->elevation;
}
double DsrcTkAbsoluteNode::getLaneWidth()
{
	return this->laneWidth;
}
bool DsrcTkAbsoluteNode::hasElevationData()
{
	return this->hasElevation;
}
bool DsrcTkAbsoluteNode::hasLaneWidthData()
{
	return this->hasLaneWidth;
}

double DsrcTkAbsoluteNode::getNorthingOffset(double baseLatitude)
{
	double verticalDegDiff = this->latitude - baseLatitude;

	return 2 * DSRCTK_POLAR_RADIUS * M_PI * verticalDegDiff / 360.0;
}

double DsrcTkAbsoluteNode::getEastingOffset(double baseLongitude)
{
	double horizonalDegDiff = this->longitude - baseLongitude;
	double downscale = cos(this->latitude * M_PI / 180);
	double results = downscale * 2 * DSRCTK_EQUATORIAL_RADIUS * M_PI * horizonalDegDiff / 360.0;
	return results;
}

double DsrcTkAbsoluteNode::getElevationOffset(double baseElevation)
{
	if (!this->hasElevation)
		return 0;
	return this->elevation - baseElevation;
}
