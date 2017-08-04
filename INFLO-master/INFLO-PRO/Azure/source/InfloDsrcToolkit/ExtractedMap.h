/*!
    @file         InfloDsrcToolkit/ExtractedMap.h
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

#include "ExtractedMapIntersection.h"

public ref class ExtractedMap
{
public:
	ExtractedMap(System::String^ name);
	~ExtractedMap() { }

	void setIntersections(array<ExtractedMapIntersection^>^ intersections);
	array<System::Byte>^ generateASN();
	

private:
	array<ExtractedMapIntersection^>^ intersections;
	System::String^ name;
};
