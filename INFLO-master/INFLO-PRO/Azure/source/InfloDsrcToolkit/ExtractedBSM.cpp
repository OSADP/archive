/*!
    @file         InfloDsrcToolkit/ExtractedBSM.cpp
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

#include "InfloDsrcToolkit/ExtractedBSM.h"

#pragma managed(push, off)
#include "DSRC_TK_J2735_R36/DSRC_ASN_Utils.h"
#include "DSRC_TK_J2735_R36/BLOB1_util.h"
#pragma managed(pop)

using System::String;

bool ExtractedBSM::loadFromASN(array<System::Byte>^ msg)
{
	bool results = false;

	// Converts managed array to byte*
	pin_ptr<System::Byte> msgManagedPointer = &msg[0];
    unsigned char* msgPointer = msgManagedPointer;

	//Deserialized the BSM
	ssize_t size;
	void  *msgPtr = NULL; // set a generic pointer to empty/null so code will allocate it
	size = DSRC_UNserializer((void **)&msgPtr, msgPointer, (int)msg->Length, true); // decode into a struc
	BasicSafetyMessage_t *bsm = NULL;
	
	//Good evaluation, so load values into fields.
	if (size > 0) // asn_dec_rval_code_e::RC_OK is inside above call
	{
		//Cast results and extract the blob
		bsm = (BasicSafetyMessage_t *)msgPtr;
		BSM_BLOB1 blob;
		blob.UnPack(bsm->blob1.buf);

		//Make tempID
		System::Text::StringBuilder tempID;
		tempID.AppendFormat("{0:X} ", blob.tempID);
		this->nomadicId = tempID.ToString();

		//Values from blob
		this->heading = DSRCHeading2Degs(blob.heading);
		this->speed = blob.speed * 0.02;
		this->latitude = blob.lat / 10000000.0;
		this->longitude = blob.lon / 10000000.0;

		this->latAccel = blob.latAccel / 100.0;
		this->longAccel = blob.lonAccel / 100.0;

		this->queuedState = (((blob.brakeSystemStatus[1] >> 5) & 0x1) == 0x1);

		//Values from status
		if (bsm->status != 0)
		{
			if (bsm->status->roadFriction != 0)
				this->coefOfFriction = (double)*bsm->status->roadFriction;

			if (bsm->status->airTemp != 0)
				this->airTemp = (double)*bsm->status->airTemp;

			if (bsm->status->vehicleIdent != 0)
			{
				if (bsm->status->vehicleIdent->name != 0)
				{
					DescriptiveName_t *roadway = bsm->status->vehicleIdent->name;
					array<wchar_t>^ rawString = gcnew array<wchar_t>(roadway->size);

					for(int i = 0; i < roadway->size; i++)
					{
						rawString[i] = roadway->buf[i];
					}

					this->roadwayId = gcnew System::String(rawString);
				}

				if (bsm->status->vehicleIdent->vin != 0)
				{
					VINstring_t *mmarker = bsm->status->vehicleIdent->vin;

					array<wchar_t>^ rawString = gcnew array<wchar_t>(mmarker->size);

					for(int i = 0; i < mmarker->size; i++)
					{
						rawString[i] = mmarker->buf[i];
					}

					System::Double::TryParse(gcnew System::String(rawString), this->mileMarker);
				}
			}
			
		}
		results = true;
	}

	//FREE
	ASN_STRUCT_FREE(asn_DEF_BasicSafetyMessage, bsm); 

	return results;
}


String^ ExtractedBSM::getNomadicId()
{
	return this->nomadicId;
}

double ExtractedBSM::getSpeed()
{
	return this->speed;
}

double ExtractedBSM::getHeading()
{
	return this->heading;
}

double ExtractedBSM::getLatitude()
{
	return this->latitude;
}

double ExtractedBSM::getLongitude()
{
	return this->longitude;
}

double ExtractedBSM::getAirTemp()
{
	return this->airTemp;
}


double ExtractedBSM::getCoefOfFriction()
{
	return this->coefOfFriction;
}

double ExtractedBSM::getLatAccel()
{
	return this->latAccel;
}

double ExtractedBSM::getLongAccel()
{
	return this->longAccel;
}

bool ExtractedBSM::getQueuedState()
{
	return this->queuedState;
}

double ExtractedBSM::getMileMarker()
{
	return this->mileMarker;
}

System::String^ ExtractedBSM::getRoadwayId()
{
	return this->roadwayId;
}


String^ ExtractedBSM::ToString()
{
	System::Text::StringBuilder results;
	results.AppendFormat("ID: {0}\n", this->nomadicId);
	results.AppendFormat("Latitude: {0}\n", this->latitude);
	results.AppendFormat("Longitude: {0}\n", this->longitude);
	results.AppendFormat("Heading: {0}\n", this->heading);
	results.AppendFormat("Speed: {0}\n", this->speed);
	results.AppendFormat("Queued: {0}\n", this->queuedState);

	return results.ToString();
}
