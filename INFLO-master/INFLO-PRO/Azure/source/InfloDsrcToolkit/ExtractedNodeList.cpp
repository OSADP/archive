/*!
    @file         InfloDsrcToolkit/ExtractedNodeList.cpp
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

#include "InfloDsrcToolkit/ExtractedNodeList.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#pragma managed(pop)


ExtractedNodeList::ExtractedNodeList(array<Extracted3DOffset^>^ nodes)
{
	this->nodes = nodes;
	this->name = nullptr;
}

int ExtractedNodeList::getAverageLatitude()
{
	if (getNodeCount() == 0)
		return 0;
	long long results = 0;
	for(int i = 0; i < getNodeCount(); i++)
	{
		results += getNode(i)->getLatitude();
	}

	return results / getNodeCount();
}

int ExtractedNodeList::getAverageLongitude()
{
	if (getNodeCount() == 0)
		return 0;

	long long results = 0;
	for(int i = 0; i < getNodeCount(); i++)
	{
		results += getNode(i)->getLongitude();
	}

	return results / getNodeCount();
}

int ExtractedNodeList::getNodeCount() 
{ 
	return nodes->Length;
}

Extracted3DOffset^ ExtractedNodeList::getNode(int number)
{
	return nodes[number];
}

void ExtractedNodeList::setName(System::String^ name)
{
	this->name = name;
}

System::String^ ExtractedNodeList::getName()
{
	return this->name;
}