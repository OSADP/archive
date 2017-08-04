/*!
    @file         InfloDsrcToolkit/ExtractedTimFrame.h
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

public ref class ExtractedTimFrame
{
public:
	ExtractedTimFrame(System::String^ itisAlertText, System::DateTime startTime, int duration);
	~ExtractedTimFrame() { }

	void setAlertPaths(array<ExtractedNodeList^>^ alertPaths);
	int getAverageLatitude();
	int getAverageLongitude();
	int getAlertPathCount();
	ExtractedNodeList^ getAlertPath(int number);
	System::String^ getItisAlertText();
	System::DateTime getStartTime();
	int getDuration();

private:
	array<ExtractedNodeList^>^ alertPaths;
	System::String^ itisAlertText;
	System::DateTime startTime;
	int duration;

};
