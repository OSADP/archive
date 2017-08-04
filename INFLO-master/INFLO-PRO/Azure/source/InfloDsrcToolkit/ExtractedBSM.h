/*!
    @file         InfloDsrcToolkit/ExtractedBSM.h
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

public ref class ExtractedBSM
{
public:
	ExtractedBSM() { }
	~ExtractedBSM() { }

	bool loadFromASN(array<System::Byte>^ msg); 

	System::String^ getNomadicId();
	double getSpeed();
	double getHeading();
	double getLatitude();
	double getLongitude();
	double getAirTemp();
	double getCoefOfFriction();
	double getLatAccel();
	double getLongAccel();
	bool getQueuedState();
	double getMileMarker();
	System::String^ getRoadwayId();

	virtual System::String^ ToString() override;

private:
	System::String^ nomadicId;
	long dSecond;
	double speed;
	double heading;
	double latitude;
	double longitude;
	double airTemp;
	double coefOfFriction;
	double latAccel;
	double longAccel;
	bool queuedState;
	double mileMarker;
	System::String^ roadwayId;
};
