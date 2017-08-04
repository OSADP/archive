/*!
    @file         InfloDsrcToolkit/Extracted3DOffset.cpp
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

#include "InfloDsrcToolkit/Extracted3DOffset.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#pragma managed(pop)

#define _USE_MATH_DEFINES
#include <cmath>

#define DSRCTK_POLAR_RADIUS 6356752.314247833
#define DSRCTK_EQUATORIAL_RADIUS 6378137.0
#define LATLONG_MULT 10000000.0
#define MM_MULT 100.0
#define OFFSET_MULT 10.0


Extracted3DOffset::Extracted3DOffset(double latitude, double longitude, double mmarker)
{
	this->latitude = latitude * LATLONG_MULT;
	this->longitude = longitude * LATLONG_MULT;
	this->mmarker = mmarker * MM_MULT;
}

int Extracted3DOffset::getLatitudeOffset(int baseLatitude)
{
	double verticalDegDiff = (this->latitude - baseLatitude) / LATLONG_MULT;

	return 2 * DSRCTK_POLAR_RADIUS * M_PI * (verticalDegDiff / 360.0) * OFFSET_MULT;


	//111034.60528834906
	//return (int)((latitude - baseLatitude)*0.111034);
}
int Extracted3DOffset::getLongitudeOffset(int baseLongitude)
{
	double horizonalDegDiff = (this->longitude - baseLongitude) / LATLONG_MULT;
	double downscale = cos((this->latitude / LATLONG_MULT) * M_PI / 180.0);
	return downscale * 2 * DSRCTK_EQUATORIAL_RADIUS * M_PI * (horizonalDegDiff / 360.0) * OFFSET_MULT;

	//85393.82609037454
	//return (int)((longitude-baseLongitude)*0.085393);
}
int Extracted3DOffset::getMileMarker()
{
	return mmarker;
}

int Extracted3DOffset::getLatitude()
{
	return latitude;
}

int Extracted3DOffset::getLongitude()
{
	return longitude;
}

/*
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
}*/