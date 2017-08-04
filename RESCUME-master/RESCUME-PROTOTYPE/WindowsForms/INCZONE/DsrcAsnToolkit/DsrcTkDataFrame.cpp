/*!
@file         DsrcAsnToolkit/DsrcTkDataFrame.cpp
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

#include "DsrcTkDataFrame.h"

DsrcTkDataFrame::DsrcTkDataFrame(DsrcTkDataFrameOptions^ options)
{
	this->contentType = options->getContentType();
	this->startTime = options->getStartTime();
	this->duration = options->getDuration();
	this->priority = options->getPriority();
	this->itisCodes = options->getItisCodes();
	this->shapePointRegions = options->getShapePointRegions();

}

DsrcTkTIMDataFrameType DsrcTkDataFrame::getContentType()
{
	return this->contentType;
}
System::DateTime DsrcTkDataFrame::getStartTime()
{
	return this->startTime;
}
int DsrcTkDataFrame::getDuration()
{
	return this->duration;
}
int DsrcTkDataFrame::getPriority()
{
	return this->priority;
}
System::Collections::Generic::List<System::String^>^ DsrcTkDataFrame::getItisCodes()
{
	return this->itisCodes;
}
array<DsrcTkShapePointSet^>^ DsrcTkDataFrame::getShapePointRegions()
{
	return this->shapePointRegions;
}

double DsrcTkDataFrame::getAverageLatitude()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < shapePointRegions->Length; i++)
	{
		results += shapePointRegions[i]->getAverageLatitude();
		count++;
	}

	return results / count;
}

double DsrcTkDataFrame::getAverageLongitude()
{
	double results = 0;
	int count = 0;

	for (int i = 0; i < shapePointRegions->Length; i++)
	{
		results += shapePointRegions[i]->getAverageLongitude();
		count++;
	}

	return results / count;
}
