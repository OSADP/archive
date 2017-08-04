/*!
    @file         InfloDsrcToolkit/ExtractedTIM.h
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

#include "ExtractedTimFrame.h"

public ref class ExtractedTIM
{
public:
	ExtractedTIM();
	~ExtractedTIM() { }

	void setPacketId(int agencyId, System::DateTime time);
	void setFrames(array<ExtractedTimFrame^>^ frames);
	
	array<System::Byte>^ generateASN();

private:
	int packetID;
	array<ExtractedTimFrame^>^ frames;


};
