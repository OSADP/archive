/*!
    @file         DsrcAsnToolkit/DsrcTkTIM.h
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

#include "DsrcTkDataFrame.h"
#include "DsrcTkTIMOptions.h"

public ref class DsrcTkTIM
{
public:
	DsrcTkTIM(DsrcTkTIMOptions^ options);
	~DsrcTkTIM() { };
	
	array<System::Byte>^ generateAsn();

private:
	array<DsrcTkDataFrame^>^ frames;
};
