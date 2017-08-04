/*!
@file         DsrcAsnToolkit/DsrcTkDataFrameOptions.cpp
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

#include "DsrcTkDataFrameOptions.h"

DsrcTkDataFrameOptions::DsrcTkDataFrameOptions()
{
	this->contentType = DsrcTkTIMDataFrameType::DsrcTkTIMDataFrameType_advisory;
	this->startTime = System::DateTime::Now;
	this->duration = 3200;
	this->priority = 7;
	this->itisCodes = gcnew System::Collections::Generic::List<System::String^>();
}

DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::setContentType(DsrcTkTIMDataFrameType contentType)
{
	this->contentType = contentType;
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::setStartTime(System::DateTime startTime)
{
	this->startTime = startTime;
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::setDuration(int duration)
{
	this->duration = duration;
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::setPriority(int priority)
{
	this->priority = priority;
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::addItisCode(int itisCode)
{
	this->itisCodes->Add(System::String::Format("$({0})", itisCode));
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::addItisText(System::String^ itisText)
{
	this->itisCodes->Add(itisText);
	return this;
}
DsrcTkDataFrameOptions^ DsrcTkDataFrameOptions::setShapePointRegions(array<DsrcTkShapePointSet^>^ shapePointRegions)
{
	this->shapePointRegions = shapePointRegions;
	return this;
}

DsrcTkTIMDataFrameType DsrcTkDataFrameOptions::getContentType()
{
	return this->contentType;
}
System::DateTime DsrcTkDataFrameOptions::getStartTime()
{
	return this->startTime;
}
int DsrcTkDataFrameOptions::getDuration()
{
	return this->duration;
}
int DsrcTkDataFrameOptions::getPriority()
{
	return this->priority;
}
System::Collections::Generic::List<System::String^>^ DsrcTkDataFrameOptions::getItisCodes()
{
	return this->itisCodes;
}
array<DsrcTkShapePointSet^>^ DsrcTkDataFrameOptions::getShapePointRegions()
{
	return this->shapePointRegions;
}