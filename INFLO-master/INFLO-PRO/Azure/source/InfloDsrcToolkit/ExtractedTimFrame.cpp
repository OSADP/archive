/*!
    @file         InfloDsrcToolkit/ExtractedTimFrame.cpp
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

#include "InfloDsrcToolkit/ExtractedTimFrame.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#pragma managed(pop)


ExtractedTimFrame::ExtractedTimFrame(System::String^ itisAlertText, System::DateTime startTime, int duration)
{
	this->alertPaths = gcnew array<ExtractedNodeList^>(0);
	this->itisAlertText = itisAlertText;
	this->startTime = startTime;
	this->duration = duration;
}

void ExtractedTimFrame::setAlertPaths(array<ExtractedNodeList^>^ alertPaths)
{
	this->alertPaths = alertPaths;
}

int ExtractedTimFrame::getAverageLatitude()
{
	if (getAlertPathCount() == 0)
		return 0;

	long long results = 0;
	for(int i = 0; i < getAlertPathCount(); i++)
	{
		results += getAlertPath(i)->getAverageLatitude();
	}

	return results / getAlertPathCount();
}

int ExtractedTimFrame::getAverageLongitude()
{
	if (getAlertPathCount() == 0)
		return 0;

	long long results = 0;
	for(int i = 0; i < getAlertPathCount(); i++)
	{
		results += getAlertPath(i)->getAverageLongitude();
	}

	return results / getAlertPathCount();
}

int ExtractedTimFrame::getAlertPathCount()
{
	return alertPaths->Length;
}

ExtractedNodeList^ ExtractedTimFrame::getAlertPath(int number)
{
	return alertPaths[number];
}
	
System::String^ ExtractedTimFrame::getItisAlertText()
{
	return itisAlertText;
}

System::DateTime ExtractedTimFrame::getStartTime()
{
	return startTime;
}

int ExtractedTimFrame::getDuration()
{
	return duration;
}