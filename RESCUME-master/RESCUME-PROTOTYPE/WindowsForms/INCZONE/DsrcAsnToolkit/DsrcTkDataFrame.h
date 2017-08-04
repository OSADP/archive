/*!
@file         DsrcAsnToolkit/DsrcTkDataFrame.h
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

#include "DsrcTkDataFrameOptions.h"

public ref class DsrcTkDataFrame
{
public:
	DsrcTkDataFrame(DsrcTkDataFrameOptions^ options);
	~DsrcTkDataFrame() { }

	DsrcTkTIMDataFrameType getContentType();
	System::DateTime getStartTime();
	int getDuration();
	int getPriority();
	System::Collections::Generic::List<System::String^>^ getItisCodes();
	array<DsrcTkShapePointSet^>^ getShapePointRegions();

	double getAverageLatitude();
	double getAverageLongitude();

private:

	DsrcTkTIMDataFrameType contentType;
	System::DateTime startTime;
	int duration;
	int priority;
	System::Collections::Generic::List<System::String^>^ itisCodes;
	array<DsrcTkShapePointSet^>^ shapePointRegions;
};
