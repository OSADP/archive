/*!
    @file         InfloDsrcToolkit/ExtractedMapIntersection.h
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

#include "ExtractedNodeList.h"

public ref class ExtractedMapIntersection
{
public:
	ExtractedMapIntersection(System::String^ id, array<ExtractedNodeList^>^ approaches);
	~ExtractedMapIntersection() { }

	System::String^ getId();
	int getAverageLatitude();
	int getAverageLongitude();
	int getApproachCount();
	ExtractedNodeList^ getApproach(int number);


private:
	System::String^ id;
	array<ExtractedNodeList^>^ approaches;
	
};
