/*!
    @file         InfloDsrcToolkit/ExtractedNodeList.h
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

#include "Extracted3DOffset.h"

public ref class ExtractedNodeList
{
public:
	ExtractedNodeList(array<Extracted3DOffset^>^ nodes);
	~ExtractedNodeList() { }
	
	int getAverageLatitude();
	int getAverageLongitude();
	int getNodeCount();
	Extracted3DOffset^ getNode(int number);
	void setName(System::String^ name);
	System::String^ getName();

private:
	array<Extracted3DOffset^>^ nodes;
	System::String^ name;
	
};
