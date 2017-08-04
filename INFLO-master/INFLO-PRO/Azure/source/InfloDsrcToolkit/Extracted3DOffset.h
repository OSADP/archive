/*!
    @file         InfloDsrcToolkit/Extracted3DOffset.h
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

public ref class Extracted3DOffset
{
public:
	Extracted3DOffset(double latitude, double longitude, double mmarker);
	~Extracted3DOffset() { }

	int getLatitudeOffset(int baseLatitude);
	int getLongitudeOffset(int baseLongitude);
	int getMileMarker();
	int getLatitude();
	int getLongitude();

private:
	int latitude, longitude, mmarker;
	
};
