/*!
@file         DsrcAsnToolkit/DsrcTkDataFrameOptions.h
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

#include "DsrcTkShapePointSet.h"

public enum DsrcTkTIMDataFrameType {
	DsrcTkTIMDataFrameType_unknown = 0,
	DsrcTkTIMDataFrameType_advisory = 1,
	DsrcTkTIMDataFrameType_roadSignage = 2,
	DsrcTkTIMDataFrameType_commercialSignage = 3
};

public ref class DsrcTkDataFrameOptions
{
public:
	DsrcTkDataFrameOptions();
	~DsrcTkDataFrameOptions() { }

	DsrcTkDataFrameOptions^ setContentType(DsrcTkTIMDataFrameType contentType);
	DsrcTkDataFrameOptions^ setStartTime(System::DateTime startTime);
	DsrcTkDataFrameOptions^ setDuration(int duration);
	DsrcTkDataFrameOptions^ setPriority(int priority);
	DsrcTkDataFrameOptions^ addItisCode(int itisCode);
	DsrcTkDataFrameOptions^ addItisText(System::String^ itisText);
	DsrcTkDataFrameOptions^ setShapePointRegions(array<DsrcTkShapePointSet^>^ shapePointRegions);

	DsrcTkTIMDataFrameType getContentType();
	System::DateTime getStartTime();
	int getDuration();
	int getPriority();
	System::Collections::Generic::List<System::String^>^ getItisCodes();
	array<DsrcTkShapePointSet^>^ getShapePointRegions();

private:
	DsrcTkTIMDataFrameType contentType;
	System::DateTime startTime;
	int duration;
	int priority;
	System::Collections::Generic::List<System::String^>^ itisCodes;
	array<DsrcTkShapePointSet^>^ shapePointRegions;
};
